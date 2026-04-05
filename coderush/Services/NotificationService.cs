using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using coderush.Data;
using coderush.Models;
using coderush.Pages;
using Microsoft.EntityFrameworkCore;

namespace coderush.Services
{
    public class NotificationService(ApplicationDbContext context) : INotificationService
    {
        private readonly ApplicationDbContext _context = context;
        private static readonly Dictionary<string, string> ControllerToRoleMap = BuildControllerToRoleMap();

        // One-time flag: once the table has been verified, skip the check.
        private static volatile bool _tableVerified;

        private static Dictionary<string, string> BuildControllerToRoleMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Type t = typeof(MainMenu);
            foreach (Type item in t.GetNestedTypes())
            {
                string controllerName = null;
                string roleName = null;

                foreach (FieldInfo field in item.GetFields())
                {
                    if (field.Name == "ControllerName")
                        controllerName = (string)field.GetValue(null);
                    else if (field.Name == "RoleName")
                        roleName = (string)field.GetValue(null);
                }

                if (controllerName != null && roleName != null)
                    map[controllerName] = roleName;
            }

            // API controllers whose class name differs from the MainMenu ControllerName
            map["PurchaseOrderLine"] = "Purchase Order";
            map["SalesOrderLine"] = "Sales Order";

            if (!map.ContainsKey("User"))
                map["User"] = "User";

            return map;
        }

        /// <summary>
        /// Ensures the Notification table exists before any read/write.
        /// Uses a static flag so the SQL check runs at most once per app lifetime.
        /// </summary>
        private async Task EnsureTableAsync()
        {
            if (_tableVerified) return;

            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID('Notification', 'U') IS NULL
                    BEGIN
                        CREATE TABLE Notification (
                            NotificationId INT IDENTITY(1,1) PRIMARY KEY,
                            Message NVARCHAR(MAX) NOT NULL,
                            CreatedDateTime DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
                            TargetUserId NVARCHAR(450) NULL,
                            TargetRole NVARCHAR(256) NULL,
                            EntityName NVARCHAR(256) NULL,
                            EntityAction NVARCHAR(50) NULL
                        );
                    END");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID('NotificationReadStatus', 'U') IS NULL
                    BEGIN
                        CREATE TABLE NotificationReadStatus (
                            NotificationReadStatusId INT IDENTITY(1,1) PRIMARY KEY,
                            NotificationId INT NOT NULL,
                            UserId NVARCHAR(450) NOT NULL,
                            IsRead BIT NOT NULL DEFAULT 0,
                            IsDeleted BIT NOT NULL DEFAULT 0
                        );
                    END");

                _tableVerified = true;
            }
            catch
            {
            }
        }

        public string ResolveRoleFromControllerName(string controllerName)
        {
            return ControllerToRoleMap.TryGetValue(controllerName, out string role) ? role : null;
        }

        public async Task AddNotificationAsync(string message, string targetUserId = null, string targetRole = null, string entityName = null, string entityAction = null)
        {
            await EnsureTableAsync();

            var notification = new Notification
            {
                Message = message,
                CreatedDateTime = DateTimeOffset.UtcNow,
                TargetUserId = targetUserId,
                TargetRole = targetRole,
                EntityName = entityName,
                EntityAction = entityAction
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Notification> Items, int TotalCount, int UnreadCount)> GetNotificationsAsync(string userId, IList<string> userRoles, int page, int pageSize, string search = null)
        {
            await EnsureTableAsync();

            // All notifications visible to this user that they haven't soft-deleted
            var baseQuery = _context.Notification
                .Where(n =>
                    (n.TargetUserId != null && n.TargetUserId == userId) ||
                    (n.TargetRole != null && userRoles.Contains(n.TargetRole))
                )
                .Where(n => !_context.NotificationReadStatus
                    .Any(s => s.NotificationId == n.NotificationId && s.UserId == userId && s.IsDeleted));

            // Apply keyword search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                baseQuery = baseQuery.Where(n => n.Message.Contains(search));
            }

            int totalCount = await baseQuery.CountAsync();

            int unreadCount = await baseQuery
                .Where(n => !_context.NotificationReadStatus
                    .Any(s => s.NotificationId == n.NotificationId && s.UserId == userId && s.IsRead))
                .CountAsync();

            // Project items with their read status
            var items = await baseQuery
                .OrderByDescending(n => n.CreatedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount, unreadCount);
        }

        public async Task<int> GetUnreadCountAsync(string userId, IList<string> userRoles)
        {
            await EnsureTableAsync();

            return await _context.Notification
                .Where(n =>
                    (n.TargetUserId != null && n.TargetUserId == userId) ||
                    (n.TargetRole != null && userRoles.Contains(n.TargetRole))
                )
                .Where(n => !_context.NotificationReadStatus
                    .Any(s => s.NotificationId == n.NotificationId && s.UserId == userId && s.IsDeleted))
                .Where(n => !_context.NotificationReadStatus
                    .Any(s => s.NotificationId == n.NotificationId && s.UserId == userId && s.IsRead))
                .CountAsync();
        }

        public async Task<bool> ToggleReadAsync(int notificationId, string userId)
        {
            await EnsureTableAsync();

            var status = await _context.NotificationReadStatus
                .FirstOrDefaultAsync(s => s.NotificationId == notificationId && s.UserId == userId);

            if (status == null)
            {
                status = new NotificationReadStatus
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    IsRead = true,
                    IsDeleted = false
                };
                _context.NotificationReadStatus.Add(status);
            }
            else
            {
                status.IsRead = !status.IsRead;
            }

            await _context.SaveChangesAsync();
            return status.IsRead;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            await EnsureTableAsync();

            var status = await _context.NotificationReadStatus
                .FirstOrDefaultAsync(s => s.NotificationId == notificationId && s.UserId == userId);

            if (status == null)
            {
                status = new NotificationReadStatus
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    IsRead = true,
                    IsDeleted = true
                };
                _context.NotificationReadStatus.Add(status);
            }
            else
            {
                status.IsDeleted = true;
                status.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
