using coderush.Data;
using coderush.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Services
{
    public class ChatResponseOrchestrator(IServiceScopeFactory serviceScopeFactory, ILogger<ChatResponseOrchestrator> logger) : IChatResponseOrchestrator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<ChatResponseOrchestrator> _logger = logger;
        private readonly ConcurrentDictionary<int, PendingConversationResponse> _pendingResponses = new();

        public bool IsPending(int conversationId) => _pendingResponses.ContainsKey(conversationId);

        public Task<bool> QueueResponseAsync(int conversationId, string applicationUserId, string message, List<ChatMessageDto> history, IList<string> userRoles, List<ChatFileContent> files = null)
        {
            var pendingResponse = new PendingConversationResponse(applicationUserId);
            if (!_pendingResponses.TryAdd(conversationId, pendingResponse))
            {
                return Task.FromResult(false);
            }

            _ = ProcessResponseAsync(conversationId, pendingResponse, message, history ?? [], userRoles ?? Array.Empty<string>(), files ?? []);
            return Task.FromResult(true);
        }

        public async Task<bool> StopResponseAsync(int conversationId, string applicationUserId)
        {
            if (!_pendingResponses.TryGetValue(conversationId, out PendingConversationResponse pendingResponse)
                || pendingResponse.ApplicationUserId != applicationUserId)
            {
                return false;
            }

            if (!_pendingResponses.TryRemove(new KeyValuePair<int, PendingConversationResponse>(conversationId, pendingResponse)))
            {
                return false;
            }

            pendingResponse.CancellationTokenSource.Cancel();

            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                ChatConversation conversation = await context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == applicationUserId);

                if (conversation != null)
                {
                    context.ChatMessage.Add(new ChatMessage
                    {
                        ChatConversationId = conversationId,
                        Role = "assistant",
                        Content = "Response was stopped.",
                        SentAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop chatbot response for conversation {ConversationId}.", conversationId);
                return false;
            }
        }

        private async Task ProcessResponseAsync(int conversationId, PendingConversationResponse pendingResponse, string message, List<ChatMessageDto> history, IList<string> userRoles, List<ChatFileContent> files)
        {
            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                IChatBotService chatBotService = scope.ServiceProvider.GetRequiredService<IChatBotService>();
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                string response = await chatBotService.ChatAsync(message, history, userRoles, files, pendingResponse.CancellationTokenSource.Token, pendingResponse.ApplicationUserId);

                if (pendingResponse.CancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                ChatConversation conversation = await context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == pendingResponse.ApplicationUserId);

                if (conversation == null)
                {
                    return;
                }

                context.ChatMessage.Add(new ChatMessage
                {
                    ChatConversationId = conversationId,
                    Role = "assistant",
                    Content = response,
                    SentAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process chatbot response for conversation {ConversationId}.", conversationId);

                try
                {
                    using IServiceScope scope = _serviceScopeFactory.CreateScope();
                    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    ChatConversation conversation = await context.ChatConversation
                        .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == pendingResponse.ApplicationUserId);

                    if (conversation != null)
                    {
                        context.ChatMessage.Add(new ChatMessage
                        {
                            ChatConversationId = conversationId,
                            Role = "assistant",
                            Content = "An error occurred while processing your request. Please try again.",
                            SentAt = DateTime.UtcNow
                        });

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to persist chatbot error message for conversation {ConversationId}.", conversationId);
                }
            }
            finally
            {
                _pendingResponses.TryRemove(new KeyValuePair<int, PendingConversationResponse>(conversationId, pendingResponse));
                pendingResponse.CancellationTokenSource.Dispose();
            }
        }

        private sealed class PendingConversationResponse(string applicationUserId)
        {
            public string ApplicationUserId { get; } = applicationUserId;

            public CancellationTokenSource CancellationTokenSource { get; } = new();
        }
    }
}
