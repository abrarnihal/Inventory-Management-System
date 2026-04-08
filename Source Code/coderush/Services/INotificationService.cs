using System.Collections.Generic;
using System.Threading.Tasks;
using coderush.Models;

namespace coderush.Services
{
    public interface INotificationService
    {
        Task AddNotificationAsync(string message, string targetUserId = null, string targetRole = null, string entityName = null, string entityAction = null);

        Task<(List<Notification> Items, int TotalCount, int UnreadCount)> GetNotificationsAsync(string userId, IList<string> userRoles, int page, int pageSize, string search = null);

        Task<bool> ToggleReadAsync(int notificationId, string userId);

        Task<bool> DeleteNotificationAsync(int notificationId, string userId);

        Task<int> GetUnreadCountAsync(string userId, IList<string> userRoles);

        string ResolveRoleFromControllerName(string controllerName);
    }
}
