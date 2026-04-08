using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace coderush.Models
{
    public class ChatConversation
    {
        public int ChatConversationId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        public string Title { get; set; } = "New Chat";

        public bool IsPinned { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = [];
    }

    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int ChatConversationId { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public string Content { get; set; }

        public string FileNames { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ChatConversation ChatConversation { get; set; }
    }
}
