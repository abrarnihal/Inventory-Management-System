using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Notification")]
    public class NotificationController(
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        IServiceScopeFactory serviceScopeFactory) : Controller
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        [HttpGet]
        public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 5, string search = null)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var (items, totalCount, unreadCount) = await _notificationService.GetNotificationsAsync(user.Id, roles, page, pageSize, search);

            // Build read-status lookup for the returned page
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationIds = items.Select(n => n.NotificationId).ToList();
            var readStatuses = await context.NotificationReadStatus
                .Where(s => s.UserId == user.Id && notificationIds.Contains(s.NotificationId))
                .ToDictionaryAsync(s => s.NotificationId, s => s.IsRead);

            var projected = items.Select(n => new
            {
                n.NotificationId,
                n.Message,
                n.CreatedDateTime,
                n.EntityName,
                n.EntityAction,
                IsRead = readStatuses.TryGetValue(n.NotificationId, out bool read) && read
            });

            return Ok(new
            {
                Items = projected,
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPost("{id}/toggle-read")]
        public async Task<IActionResult> ToggleRead(int id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            bool isNowRead = await svc.ToggleReadAsync(id, user.Id);

            IList<string> roles = await _userManager.GetRolesAsync(user);
            int unreadCount = await _notificationService.GetUnreadCountAsync(user.Id, roles);

            return Ok(new { IsRead = isNowRead, UnreadCount = unreadCount });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            await svc.DeleteNotificationAsync(id, user.Id);

            IList<string> roles = await _userManager.GetRolesAsync(user);
            int unreadCount = await _notificationService.GetUnreadCountAsync(user.Id, roles);

            return Ok(new { UnreadCount = unreadCount });
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestNotification()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            string testMessage = $"Test notification created at {DateTimeOffset.UtcNow:O}";
            string writeError = null;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await svc.AddNotificationAsync(
                    message: testMessage,
                    targetUserId: user.Id,
                    entityName: "Test",
                    entityAction: "Test");
            }
            catch (Exception ex)
            {
                writeError = ex.ToString();
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var (items, totalCount, unreadCount) = await _notificationService.GetNotificationsAsync(user.Id, roles, 1, 5);

            return Ok(new
            {
                WriteError = writeError,
                UserId = user.Id,
                Roles = roles,
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                Items = items
            });
        }
    }
}
