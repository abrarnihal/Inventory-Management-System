using System.ComponentModel.DataAnnotations;

namespace coderush.Models
{
    /// <summary>
    /// Tracks per-user read/dismissed state for each notification.
    /// Needed because a single notification can target an entire role (many users).
    /// </summary>
    public class NotificationReadStatus
    {
        public int NotificationReadStatusId { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; }

        public bool IsRead { get; set; }

        public bool IsDeleted { get; set; }
    }
}
