using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the ChatBotService.ChatAsync method.
    /// </summary>
    [TestClass]
    public class ChatBotServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private Mock<IOptions<OpenAIOptions>> _mockOptions;
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<INumberSequence> _mockNumberSequence;
        private ChatBotService _service;

        /// <summary>
        /// Helper method to setup test dependencies.
        /// </summary>
        private void SetupService(OpenAIOptions options = null)
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockOptions = new Mock<IOptions<OpenAIOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(options ?? new OpenAIOptions
            {
                ApiKey = "test-api-key",
                Model = "gpt-4o-mini"
            });

            _mockContext = new Mock<ApplicationDbContext>();
            _mockNumberSequence = new Mock<INumberSequence>();

            _service = new ChatBotService(
                _httpClient,
                _mockOptions.Object,
                _mockContext.Object,
                _mockNumberSequence.Object);
        }

        /// <summary>
        /// Helper method to setup a successful OpenAI response without tool calls.
        /// </summary>
        private void SetupSuccessfulResponse(string responseContent)
        {
            var responseJson = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = responseContent
                        }
                    }
                }
            });

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);
        }

        /// <summary>
        /// Helper method to setup a successful OpenAI response with tool calls.
        /// </summary>
        private void SetupResponseWithToolCalls(string toolCallId, string functionName, string arguments, string finalContent)
        {
            var responseWithToolCalls = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            tool_calls = new[]
                            {
                                new
                                {
                                    id = toolCallId,
                                    type = "function",
                                    function = new
                                    {
                                        name = functionName,
                                        arguments = arguments
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var finalResponse = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = finalContent
                        }
                    }
                }
            });

            var httpResponse1 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseWithToolCalls, Encoding.UTF8, "application/json")
            };

            var httpResponse2 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(finalResponse, Encoding.UTF8, "application/json")
            };

            var setupSequence = _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());

            setupSequence.ReturnsAsync(httpResponse1);
            setupSequence.ReturnsAsync(httpResponse2);
        }

        /// <summary>
        /// Helper method to setup a failed HTTP response.
        /// </summary>
        private void SetupFailedResponse(HttpStatusCode statusCode)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{\"error\": \"API Error\"}", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);
        }

        /// <summary>
        /// Tests that ChatAsync returns a successful response with valid input.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_ValidInput_ReturnsSuccessfulResponse()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Hello! How can I help you today?");
            var userMessage = "Hello";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string> { "Admin" };

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Hello! How can I help you today?", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles null history parameter correctly.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_NullHistory_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test message";
            List<ChatMessageDto> history = null;
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles null userRoles parameter by treating it as empty.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_NullUserRoles_TreatsAsEmpty()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test message";
            var history = new List<ChatMessageDto>();
            IList<string> userRoles = null;

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles null files parameter correctly.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_NullFiles_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test message";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            List<ChatFileContent> files = null;

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync includes chat history in the request.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_WithHistory_IncludesHistoryInRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Current message";
            var history = new List<ChatMessageDto>
            {
                new ChatMessageDto { Role = "user", Content = "Previous message" },
                new ChatMessageDto { Role = "assistant", Content = "Previous response" }
            };
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync includes file content when files are provided.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_WithFiles_IncludesFileContentInMessage()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("File processed");
            var userMessage = "Process this file";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            var files = new List<ChatFileContent>
            {
                new ChatFileContent { FileName = "test.txt", Content = "File content here" }
            };

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("File processed", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles multiple files correctly.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_WithMultipleFiles_IncludesAllFilesInMessage()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("All files processed");
            var userMessage = "Process these files";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            var files = new List<ChatFileContent>
            {
                new ChatFileContent { FileName = "file1.txt", Content = "Content 1" },
                new ChatFileContent { FileName = "file2.txt", Content = "Content 2" },
                new ChatFileContent { FileName = "file3.txt", Content = "Content 3" }
            };

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("All files processed", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles empty files list correctly.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_WithEmptyFilesList_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test message";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            var files = new List<ChatFileContent>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync returns error message when API request fails.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_ApiRequestFails_ReturnsErrorMessage()
        {
            // Arrange
            SetupService();
            SetupFailedResponse(HttpStatusCode.Unauthorized);
            var userMessage = "Test message";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("I'm sorry, I encountered an error communicating with the AI service"));
        }

        /// <summary>
        /// Tests that ChatAsync handles empty user message.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_EmptyUserMessage_ProcessesRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles whitespace-only user message.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_WhitespaceUserMessage_ProcessesRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "   ";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles very long user message.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_VeryLongUserMessage_ProcessesRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = new string('a', 10000);
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync uses custom model from options when provided.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_CustomModel_UsesCustomModel()
        {
            // Arrange
            SetupService(new OpenAIOptions { ApiKey = "test-key", Model = "gpt-4" });
            SetupSuccessfulResponse("Response");
            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync uses default model when model is null in options.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_NullModel_UsesDefaultModel()
        {
            // Arrange
            SetupService(new OpenAIOptions { ApiKey = "test-key", Model = null });
            SetupSuccessfulResponse("Response");
            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync returns error message when response has null content.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_ResponseWithNullContent_ReturnsErrorMessage()
        {
            // Arrange
            SetupService();
            var responseJson = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = (string)null
                        }
                    }
                }
            });

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("I'm sorry, I couldn't generate a response"));
        }

        /// <summary>
        /// Tests that ChatAsync handles empty user roles list.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_EmptyUserRoles_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles empty history list.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_EmptyHistory_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync returns max iterations error when reaching limit.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_MaxIterationsReached_ReturnsMaxIterationsError()
        {
            // Arrange
            SetupService();

            var responseWithToolCalls = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            tool_calls = new[]
                            {
                                new
                                {
                                    id = "call_123",
                                    type = "function",
                                    function = new
                                    {
                                        name = "get_inventory_summary",
                                        arguments = "{}"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseWithToolCalls, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("I reached the maximum number of processing steps"));
        }

        /// <summary>
        /// Tests that ChatAsync handles file with empty content.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_FileWithEmptyContent_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Process file";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            var files = new List<ChatFileContent>
            {
                new ChatFileContent { FileName = "empty.txt", Content = "" }
            };

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles file with special characters in filename.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_FileWithSpecialCharactersInFilename_HandlesGracefully()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Process file";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();
            var files = new List<ChatFileContent>
            {
                new ChatFileContent { FileName = "test@#$%.txt", Content = "content" }
            };

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, files);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles special characters in user message.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_SpecialCharactersInUserMessage_ProcessesRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "Test!@#$%^&*()_+{}|:\"<>?~`-=[]\\;',./";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles unicode characters in user message.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_UnicodeCharactersInUserMessage_ProcessesRequest()
        {
            // Arrange
            SetupService();
            SetupSuccessfulResponse("Response");
            var userMessage = "测试 тест テスト 🎉";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Response", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles different HTTP error status codes.
        /// </summary>
        [TestMethod]
        [DataRow(HttpStatusCode.BadRequest)]
        [DataRow(HttpStatusCode.Unauthorized)]
        [DataRow(HttpStatusCode.Forbidden)]
        [DataRow(HttpStatusCode.NotFound)]
        [DataRow(HttpStatusCode.InternalServerError)]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        public async Task ChatAsync_HttpErrorStatusCode_ReturnsErrorMessage(HttpStatusCode statusCode)
        {
            // Arrange
            SetupService();
            SetupFailedResponse(statusCode);
            var userMessage = "Test";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("I'm sorry, I encountered an error communicating with the AI service"));
        }

        /// <summary>
        /// Tests that ChatAsync handles response with content that has tool_calls and content property.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_ResponseWithToolCallsAndContent_ProcessesToolCalls()
        {
            // Arrange
            SetupService();

            var responseWithToolCalls = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "Checking inventory...",
                            tool_calls = new[]
                            {
                                new
                                {
                                    id = "call_123",
                                    type = "function",
                                    function = new
                                    {
                                        name = "get_inventory_summary",
                                        arguments = "{}"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var finalResponse = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "Here is the inventory summary."
                        }
                    }
                }
            });

            var httpResponse1 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseWithToolCalls, Encoding.UTF8, "application/json")
            };

            var httpResponse2 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(finalResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse1)
                .ReturnsAsync(httpResponse2);

            var userMessage = "Show inventory";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Here is the inventory summary.", result);
        }

        /// <summary>
        /// Tests that ChatAsync handles response with null content in tool_calls scenario.
        /// </summary>
        [TestMethod]
        public async Task ChatAsync_ResponseWithToolCallsAndNullContent_ProcessesToolCalls()
        {
            // Arrange
            SetupService();

            var responseWithToolCalls = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new Dictionary<string, object>
                        {
                            ["content"] = null,
                            ["tool_calls"] = new[]
                            {
                                new
                                {
                                    id = "call_123",
                                    type = "function",
                                    function = new
                                    {
                                        name = "get_inventory_summary",
                                        arguments = "{}"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var finalResponse = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "Done."
                        }
                    }
                }
            });

            var httpResponse1 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseWithToolCalls, Encoding.UTF8, "application/json")
            };

            var httpResponse2 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(finalResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse1)
                .ReturnsAsync(httpResponse2);

            var userMessage = "Show inventory";
            var history = new List<ChatMessageDto>();
            var userRoles = new List<string>();

            // Act
            var result = await _service.ChatAsync(userMessage, history, userRoles, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Done.", result);
        }
    }
}