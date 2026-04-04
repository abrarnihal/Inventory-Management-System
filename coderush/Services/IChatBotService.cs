using coderush.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Services
{
    public interface IChatBotService
    {
        Task<string> ChatAsync(string userMessage, List<ChatMessageDto> history, IList<string> userRoles, List<ChatFileContent> files = null, CancellationToken cancellationToken = default);
    }
}