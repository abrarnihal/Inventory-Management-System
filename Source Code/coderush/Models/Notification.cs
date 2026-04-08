using System;
using System.ComponentModel.DataAnnotations;

namespace coderush.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// When set, this notification is visible only to the specified user.
        /// </summary>
        public string TargetUserId { get; set; }

        /// <summary>
        /// When set, this notification is visible to any user who holds this role.
        /// </summary>
        public string TargetRole { get; set; }

        public string EntityName { get; set; }

        public string EntityAction { get; set; }
    }
}
