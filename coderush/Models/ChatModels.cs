using System.Collections.Generic;

namespace coderush.Models
{
    public class ChatMessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public List<ChatMessageDto> History { get; set; } = [];
    }

    public class StartChatRequest
    {
        public int ConversationId { get; set; }
        public string Message { get; set; }
        public List<ChatMessageDto> History { get; set; } = [];
    }

    public class ChatResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class StartChatResponse : ChatResponse
    {
        public bool IsPending { get; set; }
        public string Title { get; set; }
    }

    public class ChatFileContent
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class SaveChatMessageRequest
    {
        public int ConversationId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public string[] FileNames { get; set; }
    }

    public class RenameConversationRequest
    {
        public int ConversationId { get; set; }
        public string Title { get; set; }
    }
}
