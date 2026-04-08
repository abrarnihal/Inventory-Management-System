using coderush.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coderush.Services
{
    public interface IChatResponseOrchestrator
    {
        bool IsPending(int conversationId);
        Task<bool> QueueResponseAsync(int conversationId, string applicationUserId, string message, List<ChatMessageDto> history, IList<string> userRoles, List<ChatFileContent> files = null);
        Task<bool> StopResponseAsync(int conversationId, string applicationUserId);
    }
}