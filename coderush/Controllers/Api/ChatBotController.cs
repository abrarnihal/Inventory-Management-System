using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/ChatBot")]
    public class ChatBotController(IChatBotService chatBotService, IFileParserService fileParserService, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IChatResponseOrchestrator chatResponseOrchestrator) : Controller
    {
        private readonly IChatBotService _chatBotService = chatBotService;
        private readonly IFileParserService _fileParserService = fileParserService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ApplicationDbContext _context = context;
        private readonly IChatResponseOrchestrator _chatResponseOrchestrator = chatResponseOrchestrator;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [HttpPost("[action]")]
        public async Task<IActionResult> Send([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Ok(new ChatResponse { Success = false, Message = "Please enter a message." });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                var response = await _chatBotService.ChatAsync(request.Message, request.History, userRoles, userId: user?.Id);
                return Ok(new ChatResponse { Success = true, Message = response });
            }
            catch (Exception)
            {
                return Ok(new ChatResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your request. Please try again."
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> BeginResponse([FromBody] StartChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message) || request.ConversationId <= 0)
            {
                return Ok(new StartChatResponse { Success = false, Message = "Please enter a message." });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Unable to identify the current user." });
                }

                ChatConversation conversation = await _context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == request.ConversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Conversation not found." });
                }

                if (_chatResponseOrchestrator.IsPending(request.ConversationId))
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Please wait for the current response to finish." });
                }

                if (conversation.Title == "New Chat")
                {
                    conversation.Title = request.Message.Length > 30
                        ? request.Message[..30] + "..."
                        : request.Message;
                }

                _context.ChatMessage.Add(new ChatMessage
                {
                    ChatConversationId = request.ConversationId,
                    Role = "user",
                    Content = request.Message,
                    SentAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                bool queued = await _chatResponseOrchestrator.QueueResponseAsync(
                    request.ConversationId,
                    user.Id,
                    request.Message,
                    request.History,
                    userRoles);

                if (!queued)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Please wait for the current response to finish." });
                }

                return Ok(new StartChatResponse
                {
                    Success = true,
                    IsPending = true,
                    Title = conversation.Title
                });
            }
            catch (Exception)
            {
                return Ok(new StartChatResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your request. Please try again."
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> BeginResponseWithFiles(List<IFormFile> files, [FromForm] int conversationId, [FromForm] string message, [FromForm] string historyJson)
        {
            if (string.IsNullOrWhiteSpace(message) || conversationId <= 0)
            {
                return Ok(new StartChatResponse { Success = false, Message = "Please enter a message." });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Unable to identify the current user." });
                }

                ChatConversation conversation = await _context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Conversation not found." });
                }

                if (_chatResponseOrchestrator.IsPending(conversationId))
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Please wait for the current response to finish." });
                }

                List<ChatMessageDto> history = null;
                if (!string.IsNullOrWhiteSpace(historyJson))
                {
                    history = JsonSerializer.Deserialize<List<ChatMessageDto>>(historyJson, _jsonSerializerOptions);
                }

                var parsedFiles = new List<ChatFileContent>();
                var fileNames = new List<string>();

                if (files != null)
                {
                    if (files.Count > 5)
                    {
                        return Ok(new StartChatResponse
                        {
                            Success = false,
                            Message = "You can upload a maximum of 5 files per message."
                        });
                    }

                    foreach (IFormFile f in files.Where(f => f != null && f.Length > 0))
                    {
                        if (!_fileParserService.IsSupported(f.FileName))
                        {
                            return Ok(new StartChatResponse
                            {
                                Success = false,
                                Message = $"Unsupported file type: \"{f.FileName}\". Please upload only .txt, .md, .docx, .xlsx, or .xls files."
                            });
                        }

                        fileNames.Add(f.FileName);
                        parsedFiles.Add(new ChatFileContent
                        {
                            FileName = f.FileName,
                            Content = await _fileParserService.ExtractTextAsync(f)
                        });
                    }
                }

                if (conversation.Title == "New Chat")
                {
                    conversation.Title = message.Length > 30
                        ? message[..30] + "..."
                        : message;
                }

                _context.ChatMessage.Add(new ChatMessage
                {
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = message,
                    FileNames = fileNames.Count > 0 ? JsonSerializer.Serialize(fileNames) : null,
                    SentAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                bool queued = await _chatResponseOrchestrator.QueueResponseAsync(
                    conversationId,
                    user.Id,
                    message,
                    history,
                    userRoles,
                    parsedFiles);

                if (!queued)
                {
                    return Ok(new StartChatResponse { Success = false, Message = "Please wait for the current response to finish." });
                }

                return Ok(new StartChatResponse
                {
                    Success = true,
                    IsPending = true,
                    Title = conversation.Title
                });
            }
            catch (Exception)
            {
                return Ok(new StartChatResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your request. Please try again."
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendWithFile(IFormFile file, [FromForm] string message, [FromForm] string historyJson)
        {
            // Single-file backward compatibility — delegate to multi-file handler
#pragma warning disable IDE0028 // Simplify collection initialization
            var files = file != null ? new List<IFormFile> { file } : new List<IFormFile>();
#pragma warning restore IDE0028 // Simplify collection initialization
            return await SendWithFiles(files, message, historyJson);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendWithFiles(List<IFormFile> files, [FromForm] string message, [FromForm] string historyJson)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Ok(new ChatResponse { Success = false, Message = "Please enter a message." });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                IList<string> userRoles = await _userManager.GetRolesAsync(user);

                List<ChatMessageDto> history = null;
                if (!string.IsNullOrWhiteSpace(historyJson))
                {
                    history = JsonSerializer.Deserialize<List<ChatMessageDto>>(historyJson, _jsonSerializerOptions);
                }

                var parsedFiles = new List<ChatFileContent>();

                if (files != null)
                {
                    if (files.Count > 5)
                    {
                        return Ok(new ChatResponse
                        {
                            Success = false,
                            Message = "You can upload a maximum of 5 files per message."
                        });
                    }

                    foreach (IFormFile f in files.Where(f => f != null && f.Length > 0))
                    {
                        if (!_fileParserService.IsSupported(f.FileName))
                        {
                            return Ok(new ChatResponse
                            {
                                Success = false,
                                Message = $"Unsupported file type: \"{f.FileName}\". Please upload only .txt, .md, .docx, .xlsx, or .xls files."
                            });
                        }

                        parsedFiles.Add(new ChatFileContent
                        {
                            FileName = f.FileName,
                            Content = await _fileParserService.ExtractTextAsync(f)
                        });
                    }
                }

                var response = await _chatBotService.ChatAsync(message, history, userRoles, parsedFiles, userId: user?.Id);
                return Ok(new ChatResponse { Success = true, Message = response });
            }
            catch (Exception)
            {
                return Ok(new ChatResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your request. Please try again."
                });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Conversations()
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                var conversations = await _context.ChatConversation
                    .Where(c => c.ApplicationUserId == user.Id)
                    .OrderByDescending(c => c.IsPinned)
                    .ThenByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        c.ChatConversationId,
                        c.Title,
                        c.IsPinned,
                        c.CreatedAt
                    })
                    .ToListAsync();

                var conversationResults = conversations.Select(c => new
                {
                    c.ChatConversationId,
                    c.Title,
                    c.IsPinned,
                    c.CreatedAt,
                    IsPending = _chatResponseOrchestrator.IsPending(c.ChatConversationId)
                }).ToList();

                return Ok(new { Conversations = conversationResults });
            }
            catch (Exception)
            {
                return Ok(new { Conversations = Array.Empty<object>() });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> PendingStatus([FromQuery] int conversationId)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                bool exists = await _context.ChatConversation
                    .AnyAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == user.Id);

                if (!exists)
                {
                    return Ok(new { Success = false, IsPending = false });
                }

                return Ok(new { Success = true, IsPending = _chatResponseOrchestrator.IsPending(conversationId) });
            }
            catch (Exception)
            {
                return Ok(new { Success = false, IsPending = false });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> StopResponse([FromQuery] int conversationId)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new { Success = false });
                }

                bool stopped = await _chatResponseOrchestrator.StopResponseAsync(conversationId, user.Id);
                return Ok(new { Success = stopped });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteConversation([FromQuery] int conversationId)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new { Success = false });
                }

                ChatConversation conversation = await _context.ChatConversation
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new { Success = false });
                }

                _context.ChatMessage.RemoveRange(conversation.Messages);
                _context.ChatConversation.Remove(conversation);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateConversation()
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                var conversation = new ChatConversation
                {
                    ApplicationUserId = user.Id,
                    Title = "New Chat",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatConversation.Add(conversation);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true, conversation.ChatConversationId, conversation.Title });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TogglePin([FromQuery] int conversationId)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                ChatConversation conversation = await _context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new { Success = false });
                }

                conversation.IsPinned = !conversation.IsPinned;
                await _context.SaveChangesAsync();
                return Ok(new { Success = true, conversation.IsPinned });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RenameConversation([FromBody] RenameConversationRequest request)
        {
            if (request == null || request.ConversationId <= 0 || string.IsNullOrWhiteSpace(request.Title))
            {
                return Ok(new { Success = false });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Ok(new { Success = false });
                }

                ChatConversation conversation = await _context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == request.ConversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new { Success = false });
                }

                conversation.Title = request.Title.Trim().Length > 50
                    ? request.Title.Trim()[..50]
                    : request.Title.Trim();

                await _context.SaveChangesAsync();
                return Ok(new { Success = true, Title = conversation.Title });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SaveMessage([FromBody] SaveChatMessageRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.Role)
                || string.IsNullOrWhiteSpace(request.Content)
                || request.ConversationId <= 0)
            {
                return Ok(new { Success = false });
            }

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                ChatConversation conversation = await _context.ChatConversation
                    .FirstOrDefaultAsync(c => c.ChatConversationId == request.ConversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(new { Success = false });
                }

                if (conversation.Title == "New Chat" && request.Role == "user")
                {
                    conversation.Title = request.Content.Length > 30
                        ? request.Content[..30] + "..."
                        : request.Content;
                }

                _context.ChatMessage.Add(new ChatMessage
                {
                    ChatConversationId = request.ConversationId,
                    Role = request.Role,
                    Content = request.Content,
                    FileNames = request.FileNames != null && request.FileNames.Length > 0
                        ? JsonSerializer.Serialize(request.FileNames)
                        : null,
                    SentAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return Ok(new { Success = true, Title = conversation.Title });
            }
            catch (Exception)
            {
                return Ok(new { Success = false });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> History([FromQuery] int conversationId)
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);
                ChatConversation conversation = await _context.ChatConversation
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.ApplicationUserId == user.Id);

                if (conversation == null)
                {
                    return Ok(Array.Empty<object>());
                }

                var messages = conversation.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.Role,
                        Text = m.Content,
                        FileNames = !string.IsNullOrEmpty(m.FileNames)
                            ? JsonSerializer.Deserialize<string[]>(m.FileNames)
                            : Array.Empty<string>()
                    })
                    .ToList();

                return Ok(new { Messages = messages });
            }
            catch (Exception)
            {
                return Ok(Array.Empty<object>());
            }
        }
    }
}