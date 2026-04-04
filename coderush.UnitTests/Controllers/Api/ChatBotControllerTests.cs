using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the ChatBotController class.
    /// </summary>
    [TestClass]
    public class ChatBotControllerTests
    {
        /// <summary>
        /// Tests that SendWithFiles returns error response when message is null.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_NullMessage_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            // Act
            var result = await controller.SendWithFiles(new List<IFormFile>(), null!, string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when message is empty string.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_EmptyMessage_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            // Act
            var result = await controller.SendWithFiles(new List<IFormFile>(), string.Empty, string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when message contains only whitespace.
        /// </summary>
        [TestMethod]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        [DataRow("\r\n")]
        [DataRow("  \t\n  ")]
        public async Task SendWithFiles_WhitespaceMessage_ReturnsErrorResponse(string message)
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            // Act
            var result = await controller.SendWithFiles(new List<IFormFile>(), message, string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when files count exceeds 5.
        /// </summary>
        [TestMethod]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task SendWithFiles_MoreThanFiveFiles_ReturnsErrorResponse(int fileCount)
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var files = new List<IFormFile>();
            for (int i = 0; i < fileCount; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(100);
                mockFile.Setup(f => f.FileName).Returns($"file{i}.txt");
                files.Add(mockFile.Object);
            }

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("You can upload a maximum of 5 files per message.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when unsupported file type is provided.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_UnsupportedFileType_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.exe");
            var files = new List<IFormFile>
            {
                mockFile.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("test.exe")).Returns(false);
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Message.Contains("Unsupported file type"));
            Assert.IsTrue(response.Message.Contains("test.exe"));
        }

        /// <summary>
        /// Tests that SendWithFiles successfully processes valid message without files.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ValidMessageNoFiles_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Bot response", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles successfully processes valid message with supported files.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ValidMessageWithSupportedFiles_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            var files = new List<IFormFile>
            {
                mockFile.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("test.txt")).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFile.Object)).ReturnsAsync("File content");
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Bot response", response.Message);
            mockFileParserService.Verify(s => s.ExtractTextAsync(mockFile.Object), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles successfully processes exactly 5 files (boundary case).
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ExactlyFiveFiles_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var files = new List<IFormFile>();
            for (int i = 0; i < 5; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(100);
                mockFile.Setup(f => f.FileName).Returns($"file{i}.txt");
                files.Add(mockFile.Object);
            }

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported(It.IsAny<string>())).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(It.IsAny<IFormFile>())).ReturnsAsync("File content");
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockFileParserService.Verify(s => s.ExtractTextAsync(It.IsAny<IFormFile>()), Times.Exactly(5));
        }

        /// <summary>
        /// Tests that SendWithFiles filters out null files from the list.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_FilesListContainsNulls_FiltersOutNullFiles()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            var files = new List<IFormFile>
            {
                null!,
                mockFile.Object,
                null!
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("test.txt")).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFile.Object)).ReturnsAsync("File content");
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockFileParserService.Verify(s => s.ExtractTextAsync(It.IsAny<IFormFile>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles filters out files with zero length.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_FilesWithZeroLength_FiltersOutEmptyFiles()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFileEmpty = new Mock<IFormFile>();
            mockFileEmpty.Setup(f => f.Length).Returns(0);
            mockFileEmpty.Setup(f => f.FileName).Returns("empty.txt");
            var mockFileValid = new Mock<IFormFile>();
            mockFileValid.Setup(f => f.Length).Returns(100);
            mockFileValid.Setup(f => f.FileName).Returns("valid.txt");
            var files = new List<IFormFile>
            {
                mockFileEmpty.Object,
                mockFileValid.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("valid.txt")).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFileValid.Object)).ReturnsAsync("File content");
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockFileParserService.Verify(s => s.ExtractTextAsync(mockFileValid.Object), Times.Once);
            mockFileParserService.Verify(s => s.IsSupported("empty.txt"), Times.Never);
        }

        /// <summary>
        /// Tests that SendWithFiles processes valid history JSON correctly.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ValidHistoryJson_DeserializesAndPassesToChatService()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var historyList = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    Role = "user",
                    Content = "Hello"
                },
                new ChatMessageDto
                {
                    Role = "assistant",
                    Content = "Hi there"
                }
            };
            var historyJson = JsonSerializer.Serialize(historyList);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, "test message", historyJson);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync("test message", It.Is<List<ChatMessageDto>?>(h => h != null && h.Count == 2), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles handles null historyJson by passing null history to chat service.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_NullHistoryJson_PassesNullHistoryToChatService()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, "test message", null!);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync("test message", null, It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles handles empty historyJson by passing null history to chat service.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_EmptyHistoryJson_PassesNullHistoryToChatService()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync("test message", null, It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when invalid JSON is provided for history.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_InvalidHistoryJson_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            // Act
            var result = await controller.SendWithFiles(null!, "test message", "invalid json {{{");
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when GetUserAsync throws exception.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_GetUserAsyncThrows_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new Exception("User retrieval failed"));
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when GetRolesAsync throws exception.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_GetRolesAsyncThrows_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ThrowsAsync(new Exception("Role retrieval failed"));
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when ExtractTextAsync throws exception.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ExtractTextAsyncThrows_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            var files = new List<IFormFile>
            {
                mockFile.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("test.txt")).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFile.Object)).ThrowsAsync(new Exception("File extraction failed"));
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when ChatAsync throws exception.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_ChatAsyncThrows_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ThrowsAsync(new Exception("Chat service failed"));
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles returns error response when IsSupported throws exception.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_IsSupportedThrows_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            var files = new List<IFormFile>
            {
                mockFile.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("test.txt")).Throws(new Exception("IsSupported failed"));
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFiles processes empty files list successfully.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_EmptyFilesList_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(new List<IFormFile>(), "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockFileParserService.Verify(s => s.ExtractTextAsync(It.IsAny<IFormFile>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendWithFiles handles very long message strings.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_VeryLongMessage_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var longMessage = new string ('a', 10000);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, longMessage, string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync(longMessage, It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles handles messages with special characters.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_MessageWithSpecialCharacters_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var specialMessage = "Hello! @#$%^&*()_+-=[]{}|;':\",./<>?`~";
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, specialMessage, string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFiles correctly passes user roles to chat service.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_MultipleUserRoles_PassesAllRolesToChatService()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            var roles = new List<string>
            {
                "Admin",
                "User",
                "Manager"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(null!, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync("test message", null, It.Is<IList<string>>(r => r.Count == 3 && r.Contains("Admin") && r.Contains("User") && r.Contains("Manager")), It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that SendWithFiles correctly passes parsed files to chat service with correct structure.
        /// </summary>
        [TestMethod]
        public async Task SendWithFiles_MultipleFiles_PassesAllParsedFilesToChatService()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            SetupControllerUser(controller);
            var mockFile1 = new Mock<IFormFile>();
            mockFile1.Setup(f => f.Length).Returns(100);
            mockFile1.Setup(f => f.FileName).Returns("file1.txt");
            var mockFile2 = new Mock<IFormFile>();
            mockFile2.Setup(f => f.Length).Returns(200);
            mockFile2.Setup(f => f.FileName).Returns("file2.md");
            var files = new List<IFormFile>
            {
                mockFile1.Object,
                mockFile2.Object
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            mockFileParserService.Setup(s => s.IsSupported("file1.txt")).Returns(true);
            mockFileParserService.Setup(s => s.IsSupported("file2.md")).Returns(true);
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFile1.Object)).ReturnsAsync("Content 1");
            mockFileParserService.Setup(s => s.ExtractTextAsync(mockFile2.Object)).ReturnsAsync("Content 2");
            mockChatBotService.Setup(s => s.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>?>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            // Act
            var result = await controller.SendWithFiles(files, "test message", string.Empty);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            mockChatBotService.Verify(s => s.ChatAsync("test message", null, It.IsAny<IList<string>>(), It.Is<List<ChatFileContent>>(f => f.Count == 2 && f[0].FileName == "file1.txt" && f[0].Content == "Content 1" && f[1].FileName == "file2.md" && f[1].Content == "Content 2")), Times.Once);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static void SetupControllerUser(ChatBotController controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1"), new Claim(ClaimTypes.Name, "testuser") }))
                }
            };
        }

        /// <summary>
        /// Tests that Send returns an error response when the request is null.
        /// </summary>
        [TestMethod]
        public async Task Send_NullRequest_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            // Act
            var result = await controller.Send(null);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that Send returns an error response when the message is null, empty, or whitespace.
        /// </summary>
        /// <param name = "message">The message value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null message")]
        [DataRow("", DisplayName = "Empty message")]
        [DataRow(" ", DisplayName = "Single whitespace")]
        [DataRow("   ", DisplayName = "Multiple whitespaces")]
        [DataRow("\t", DisplayName = "Tab character")]
        [DataRow("\n", DisplayName = "Newline character")]
        [DataRow("\r\n", DisplayName = "Carriage return and newline")]
        [DataRow("  \t\n  ", DisplayName = "Mixed whitespace")]
        public async Task Send_InvalidMessage_ReturnsErrorResponse(string message)
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            var request = new ChatRequest
            {
                Message = message
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that Send returns an error response when GetUserAsync throws an exception.
        /// </summary>
        [TestMethod]
        public async Task Send_GetUserAsyncThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("User not found"));
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = "Hello"
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that Send returns an error response when GetRolesAsync throws an exception.
        /// </summary>
        [TestMethod]
        public async Task Send_GetRolesAsyncThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var user = new ApplicationUser();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.GetRolesAsync(user)).ThrowsAsync(new InvalidOperationException("Role retrieval failed"));
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = "Hello"
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that Send returns an error response when ChatAsync throws an exception.
        /// </summary>
        [TestMethod]
        public async Task Send_ChatAsyncThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var user = new ApplicationUser();
            var userRoles = new List<string>
            {
                "User"
            };
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(userRoles);
            mockChatBotService.Setup(cbs => cbs.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ThrowsAsync(new Exception("ChatBot service error"));
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = "Hello"
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that Send returns a success response when all dependencies work correctly.
        /// </summary>
        [TestMethod]
        public async Task Send_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var user = new ApplicationUser();
            var userRoles = new List<string>
            {
                "Admin",
                "User"
            };
            var expectedBotResponse = "This is the bot's response";
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(userRoles);
            mockChatBotService.Setup(cbs => cbs.ChatAsync("Hello, bot!", It.IsAny<List<ChatMessageDto>>(), userRoles, It.IsAny<List<ChatFileContent>>())).ReturnsAsync(expectedBotResponse);
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = "Hello, bot!"
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(expectedBotResponse, response.Message);
        }

        /// <summary>
        /// Tests that Send passes the history from the request to ChatAsync.
        /// </summary>
        [TestMethod]
        public async Task Send_ValidRequestWithHistory_PassesHistoryToChatAsync()
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var user = new ApplicationUser();
            var userRoles = new List<string>
            {
                "User"
            };
            var history = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    Role = "user",
                    Content = "Previous message"
                },
                new ChatMessageDto
                {
                    Role = "assistant",
                    Content = "Previous response"
                }
            };
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(userRoles);
            mockChatBotService.Setup(cbs => cbs.ChatAsync(It.IsAny<string>(), history, It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Response with history");
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = "New message",
                History = history
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            mockChatBotService.Verify(cbs => cbs.ChatAsync("New message", history, userRoles, It.IsAny<List<ChatFileContent>>()), Times.Once);
        }

        /// <summary>
        /// Tests that Send handles various message edge cases correctly.
        /// </summary>
        /// <param name = "message">The message to test.</param>
        [TestMethod]
        [DataRow("a", DisplayName = "Single character")]
        [DataRow("Hello!", DisplayName = "Simple message")]
        [DataRow("Message with special chars: !@#$%^&*()", DisplayName = "Special characters")]
        [DataRow("Message\nwith\nnewlines", DisplayName = "Message with newlines")]
        [DataRow("Very long message " + "abcdefghijklmnopqrstuvwxyz0123456789", DisplayName = "Long message")]
        public async Task Send_VariousValidMessages_ReturnsSuccessResponse(string message)
        {
            // Arrange
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var mockUserManager = CreateMockUserManager();
            var mockContext = new Mock<ApplicationDbContext>();
            var user = new ApplicationUser();
            var userRoles = new List<string>
            {
                "User"
            };
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(userRoles);
            mockChatBotService.Setup(cbs => cbs.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Bot response");
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, mockOrchestrator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            var request = new ChatRequest
            {
                Message = message
            };
            // Act
            var result = await controller.Send(request);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Helper method to create a mock UserManager for testing.
        /// </summary>
        /// <returns>A mock UserManager instance.</returns>
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<IChatBotService>? _mockChatBotService;
        private Mock<IFileParserService>? _mockFileParserService;
        private Mock<UserManager<ApplicationUser>>? _mockUserManager;
        private Mock<ApplicationDbContext>? _mockContext;
        private ChatBotController? _controller;
        [TestInitialize]
        public void Initialize()
        {
            _mockChatBotService = new Mock<IChatBotService>();
            _mockFileParserService = new Mock<IFileParserService>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _mockContext = new Mock<ApplicationDbContext>(dbContextOptions);
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            _controller = new ChatBotController(_mockChatBotService.Object, _mockFileParserService.Object, _mockUserManager.Object, _mockContext.Object, mockOrchestrator.Object);
            // Setup default controller context with user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id"), new Claim(ClaimTypes.Name, "test@example.com") }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        /// <summary>
        /// Tests that SendWithFile with null file creates an empty list and returns success response.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_FileIsNull_ReturnsSuccessResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            // Act
            var result = await _controller!.SendWithFile(null, "test message", "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile with a valid file creates a list with one file and returns success response.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_FileIsNotNull_ReturnsSuccessResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            var mockFile = new Mock<IFormFile>();
            var content = "test content";
            var fileName = "test.txt";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.ContentType).Returns("text/plain");
            _mockFileParserService!.Setup(x => x.IsSupported(fileName)).Returns(true);
            _mockFileParserService.Setup(x => x.ExtractTextAsync(It.IsAny<IFormFile>())).ReturnsAsync("extracted text");
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            // Act
            var result = await _controller!.SendWithFile(mockFile.Object, "test message", "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile with null message returns error response.
        /// </summary>
        [TestMethod]
        [DataRow(null, DisplayName = "Null message")]
        [DataRow("", DisplayName = "Empty message")]
        [DataRow(" ", DisplayName = "Whitespace message")]
        [DataRow("   ", DisplayName = "Multiple whitespace message")]
        [DataRow("\t", DisplayName = "Tab message")]
        [DataRow("\n", DisplayName = "Newline message")]
        public async Task SendWithFile_InvalidMessage_ReturnsErrorResponse(string? message)
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            // Act
            var result = await _controller!.SendWithFile(null, message, "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please enter a message.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFile with null historyJson processes correctly.
        /// </summary>
        [TestMethod]
        [DataRow(null, DisplayName = "Null historyJson")]
        [DataRow("", DisplayName = "Empty historyJson")]
        [DataRow(" ", DisplayName = "Whitespace historyJson")]
        public async Task SendWithFile_NullOrEmptyHistoryJson_ReturnsSuccessResponse(string? historyJson)
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            // Act
            var result = await _controller!.SendWithFile(null, "test message", historyJson);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile with valid historyJson deserializes and processes correctly.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_ValidHistoryJson_ReturnsSuccessResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            var historyJson = "[{\"role\":\"user\",\"content\":\"Hello\"},{\"role\":\"assistant\",\"content\":\"Hi\"}]";
            // Act
            var result = await _controller!.SendWithFile(null, "test message", historyJson);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile with unsupported file type returns error response.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_UnsupportedFileType_ReturnsErrorResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            var mockFile = new Mock<IFormFile>();
            var fileName = "test.exe";
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(100);
            _mockFileParserService!.Setup(x => x.IsSupported(fileName)).Returns(false);
            // Act
            var result = await _controller!.SendWithFile(mockFile.Object, "test message", "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Message.Contains("Unsupported file type"));
        }

        /// <summary>
        /// Tests that SendWithFile with empty file (Length = 0) skips the file and processes message.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_EmptyFile_SkipsFileAndReturnsSuccessResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            mockFile.Setup(f => f.Length).Returns(0);
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            // Act
            var result = await _controller!.SendWithFile(mockFile.Object, "test message", "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            _mockFileParserService!.Verify(x => x.IsSupported(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendWithFile with special characters in message processes correctly.
        /// </summary>
        [TestMethod]
        [DataRow("message with <html> tags", DisplayName = "HTML tags")]
        [DataRow("message with \"quotes\"", DisplayName = "Quotes")]
        [DataRow("message with 'single quotes'", DisplayName = "Single quotes")]
        [DataRow("message with \\ backslash", DisplayName = "Backslash")]
        [DataRow("message with \n newline", DisplayName = "Newline in message")]
        [DataRow("message with special chars: !@#$%^&*()", DisplayName = "Special characters")]
        public async Task SendWithFile_MessageWithSpecialCharacters_ReturnsSuccessResponse(string message)
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            // Act
            var result = await _controller!.SendWithFile(null, message, "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile with very long message processes correctly.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_VeryLongMessage_ReturnsSuccessResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ReturnsAsync("Test response");
            var longMessage = new string ('a', 10000);
            // Act
            var result = await _controller!.SendWithFile(null, longMessage, "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        /// <summary>
        /// Tests that SendWithFile handles exceptions and returns error response.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_ServiceThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            _mockChatBotService!.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<ChatMessageDto>>(), It.IsAny<IList<string>>(), It.IsAny<List<ChatFileContent>>())).ThrowsAsync(new Exception("Test exception"));
            // Act
            var result = await _controller!.SendWithFile(null, "test message", "[]");
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("An error occurred while processing your request. Please try again.", response.Message);
        }

        /// <summary>
        /// Tests that SendWithFile with invalid JSON historyJson handles gracefully.
        /// </summary>
        [TestMethod]
        public async Task SendWithFile_InvalidJsonHistory_ReturnsErrorResponse()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "test@example.com"
            };
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);
            _mockUserManager.Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(new List<string> { "User" });
            var invalidJson = "{invalid json";
            // Act
            var result = await _controller!.SendWithFile(null, "test message", invalidJson);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as ChatResponse;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
        }

        /// <summary>
        /// Tests that CreateConversation returns success when user is authenticated and conversation is created successfully.
        /// Input: Valid authenticated user.
        /// Expected: Returns OkObjectResult with Success = true, ChatConversationId, and Title.
        /// </summary>
        [TestMethod]
        public async Task CreateConversation_ValidUser_ReturnsSuccessWithConversationDetails()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId
            };
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockChatConversationSet = new Mock<DbSet<ChatConversation>>();
            mockContext.Setup(c => c.ChatConversation).Returns(mockChatConversationSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            // Act
            var result = await controller.CreateConversation();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var successProperty = value.GetType().GetProperty("Success");
            Assert.IsNotNull(successProperty);
            var success = (bool)successProperty.GetValue(value);
            Assert.IsTrue(success);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that CreateConversation returns failure when GetUserAsync returns null.
        /// Input: GetUserAsync returns null user.
        /// Expected: Returns OkObjectResult with Success = false due to NullReferenceException.
        /// </summary>
        [TestMethod]
        public async Task CreateConversation_UserIsNull_ReturnsFailure()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser? )null);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            // Act
            var result = await controller.CreateConversation();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var successProperty = value.GetType().GetProperty("Success");
            Assert.IsNotNull(successProperty);
            var success = (bool)successProperty.GetValue(value);
            Assert.IsFalse(success);
        }

        /// <summary>
        /// Tests that CreateConversation returns failure when SaveChangesAsync throws an exception.
        /// Input: Valid user but SaveChangesAsync throws DbUpdateException.
        /// Expected: Returns OkObjectResult with Success = false.
        /// </summary>
        [TestMethod]
        public async Task CreateConversation_SaveChangesFails_ReturnsFailure()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId
            };
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockChatConversationSet = new Mock<DbSet<ChatConversation>>();
            mockContext.Setup(c => c.ChatConversation).Returns(mockChatConversationSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateException("Database error"));
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            // Act
            var result = await controller.CreateConversation();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var successProperty = value.GetType().GetProperty("Success");
            Assert.IsNotNull(successProperty);
            var success = (bool)successProperty.GetValue(value);
            Assert.IsFalse(success);
        }

        /// <summary>
        /// Tests that CreateConversation returns failure when GetUserAsync throws an exception.
        /// Input: GetUserAsync throws InvalidOperationException.
        /// Expected: Returns OkObjectResult with Success = false.
        /// </summary>
        [TestMethod]
        public async Task CreateConversation_GetUserAsyncThrowsException_ReturnsFailure()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("User retrieval failed"));
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            // Act
            var result = await controller.CreateConversation();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var successProperty = value.GetType().GetProperty("Success");
            Assert.IsNotNull(successProperty);
            var success = (bool)successProperty.GetValue(value);
            Assert.IsFalse(success);
        }

        /// <summary>
        /// Tests that CreateConversation creates conversation with correct default values.
        /// Input: Valid authenticated user.
        /// Expected: ChatConversation is created with Title "New Chat" and current UTC timestamp.
        /// </summary>
        [TestMethod]
        public async Task CreateConversation_ValidUser_CreatesConversationWithCorrectDefaults()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId
            };
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            ChatConversation? capturedConversation = null;
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockChatConversationSet = new Mock<DbSet<ChatConversation>>();
            mockChatConversationSet.Setup(s => s.Add(It.IsAny<ChatConversation>())).Callback<ChatConversation>(c => capturedConversation = c);
            mockContext.Setup(c => c.ChatConversation).Returns(mockChatConversationSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var mockChatBotService = new Mock<IChatBotService>();
            var mockFileParserService = new Mock<IFileParserService>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            var beforeExecution = DateTime.UtcNow;
            // Act
            var result = await controller.CreateConversation();
            // Assert
            var afterExecution = DateTime.UtcNow;
            Assert.IsNotNull(capturedConversation);
            Assert.AreEqual(userId, capturedConversation.ApplicationUserId);
            Assert.AreEqual("New Chat", capturedConversation.Title);
            Assert.IsTrue(capturedConversation.CreatedAt >= beforeExecution);
            Assert.IsTrue(capturedConversation.CreatedAt <= afterExecution);
            mockChatConversationSet.Verify(s => s.Add(It.IsAny<ChatConversation>()), Times.Once);
        }

        /// <summary>
        /// Tests that TogglePin returns success false when FirstOrDefaultAsync throws an exception.
        /// </summary>
        [TestMethod]
        public async Task TogglePin_FirstOrDefaultAsyncThrowsException_ReturnsSuccessFalse()
        {
            // Arrange
            const int conversationId = 1;
            const string userId = "user123";
            var user = new ApplicationUser
            {
                Id = userId
            };
            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockSet = new Mock<DbSet<ChatConversation>>();
            mockSet.As<IQueryable<ChatConversation>>().Setup(m => m.Provider).Throws(new InvalidOperationException("Database error"));
            mockContext.Setup(x => x.ChatConversation).Returns(mockSet.Object);
            var controller = CreateController(mockUserManager.Object, mockContext.Object);
            // Act
            var result = await controller.TogglePin(conversationId);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsFalse(value.GetType().GetProperty("Success")?.GetValue(value));
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static ChatBotController CreateController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockOrchestrator = new Mock<IChatResponseOrchestrator>();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, userManager, context, mockOrchestrator.Object);
            var httpContext = new DefaultHttpContext();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user123"), new Claim(ClaimTypes.Name, "testuser") }));
            httpContext.User = claimsPrincipal;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

        private static ControllerContext CreateControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.NameIdentifier, "user123") }, "mock"));
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when request is null.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_NullRequest_ReturnsFalse()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = MockDbContext();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            // Act
            var result = await controller.SaveMessage(null);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when Role is null, empty, or whitespace.
        /// </summary>
        /// <param name = "role">The role value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Role is null")]
        [DataRow("", DisplayName = "Role is empty")]
        [DataRow("   ", DisplayName = "Role is whitespace")]
        public async Task SaveMessage_InvalidRole_ReturnsFalse(string role)
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = MockDbContext();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = role,
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when Content is null, empty, or whitespace.
        /// </summary>
        /// <param name = "content">The content value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Content is null")]
        [DataRow("", DisplayName = "Content is empty")]
        [DataRow("   ", DisplayName = "Content is whitespace")]
        public async Task SaveMessage_InvalidContent_ReturnsFalse(string content)
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = MockDbContext();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = content,
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when ConversationId is zero or negative.
        /// </summary>
        /// <param name = "conversationId">The conversation ID to test.</param>
        [TestMethod]
        [DataRow(0, DisplayName = "ConversationId is zero")]
        [DataRow(-1, DisplayName = "ConversationId is negative")]
        [DataRow(int.MinValue, DisplayName = "ConversationId is int.MinValue")]
        public async Task SaveMessage_InvalidConversationId_ReturnsFalse(int conversationId)
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockUserManager = MockUserManager();
            var mockContext = MockDbContext();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = conversationId
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when conversation is not found.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ConversationNotFound_ReturnsFalse()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>().AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when conversation belongs to different user.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ConversationBelongsToDifferentUser_ReturnsFalse()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "differentUser",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage successfully saves message and returns success true with conversation title.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ValidRequest_ReturnsSuccessTrue()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Existing Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var success = GetPropertyValue<bool>(okResult.Value, "Success");
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.IsTrue(success);
            Assert.AreEqual("Existing Chat", title);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("user", messages[0].Role);
            Assert.AreEqual("Test content", messages[0].Content);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveMessage updates title when conversation title is "New Chat" and role is "user" with short content.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_NewChatWithUserRoleShortContent_UpdatesTitle()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversation = new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user123",
                Title = "New Chat"
            };
            var conversations = new List<ChatConversation>
            {
                conversation
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Short message",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.AreEqual("Short message", title);
            Assert.AreEqual("Short message", conversation.Title);
        }

        /// <summary>
        /// Tests that SaveMessage updates title when conversation title is "New Chat" and role is "user" with content exactly 30 characters.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_NewChatWithUserRoleExactly30Chars_UpdatesTitleWithoutEllipsis()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversation = new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user123",
                Title = "New Chat"
            };
            var conversations = new List<ChatConversation>
            {
                conversation
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var content = "123456789012345678901234567890"; // Exactly 30 chars
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = content,
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.AreEqual(content, title);
            Assert.AreEqual(content, conversation.Title);
        }

        /// <summary>
        /// Tests that SaveMessage updates title with ellipsis when conversation title is "New Chat" and role is "user" with content longer than 30 characters.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_NewChatWithUserRoleLongContent_UpdatesTitleWithEllipsis()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversation = new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user123",
                Title = "New Chat"
            };
            var conversations = new List<ChatConversation>
            {
                conversation
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var content = "This is a very long message that exceeds thirty characters";
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = content,
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.AreEqual("This is a very long message th...", title);
            Assert.AreEqual("This is a very long message th...", conversation.Title);
        }

        /// <summary>
        /// Tests that SaveMessage does not update title when conversation title is not "New Chat".
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ExistingTitleWithUserRole_DoesNotUpdateTitle()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversation = new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user123",
                Title = "Existing Title"
            };
            var conversations = new List<ChatConversation>
            {
                conversation
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "New content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.AreEqual("Existing Title", title);
            Assert.AreEqual("Existing Title", conversation.Title);
        }

        /// <summary>
        /// Tests that SaveMessage does not update title when role is not "user".
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_NewChatWithAssistantRole_DoesNotUpdateTitle()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversation = new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user123",
                Title = "New Chat"
            };
            var conversations = new List<ChatConversation>
            {
                conversation
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "assistant",
                Content = "Assistant response",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var title = GetPropertyValue<string>(okResult.Value, "Title");
            Assert.AreEqual("New Chat", title);
            Assert.AreEqual("New Chat", conversation.Title);
        }

        /// <summary>
        /// Tests that SaveMessage correctly serializes FileNames when provided.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_WithFileNames_SerializesFileNames()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Message with files",
                ConversationId = 1,
                FileNames = new[]
                {
                    "file1.txt",
                    "file2.pdf"
                }
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, messages.Count);
            Assert.IsNotNull(messages[0].FileNames);
            var deserializedFileNames = JsonSerializer.Deserialize<string[]>(messages[0].FileNames);
            Assert.IsNotNull(deserializedFileNames);
            Assert.AreEqual(2, deserializedFileNames.Length);
            Assert.AreEqual("file1.txt", deserializedFileNames[0]);
            Assert.AreEqual("file2.pdf", deserializedFileNames[1]);
        }

        /// <summary>
        /// Tests that SaveMessage sets FileNames to null when FileNames is null.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_WithNullFileNames_SetsFileNamesToNull()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Message without files",
                ConversationId = 1,
                FileNames = null
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, messages.Count);
            Assert.IsNull(messages[0].FileNames);
        }

        /// <summary>
        /// Tests that SaveMessage sets FileNames to null when FileNames is an empty array.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_WithEmptyFileNames_SetsFileNamesToNull()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Message with empty files array",
                ConversationId = 1,
                FileNames = new string[0]
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, messages.Count);
            Assert.IsNull(messages[0].FileNames);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when an exception occurs during save.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ExceptionDuringSave_ReturnsFalse()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage returns success false when GetUserAsync throws an exception.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ExceptionInGetUser_ReturnsFalse()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new Exception("User manager error"));
            var mockContext = MockDbContext();
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsFalse(value);
        }

        /// <summary>
        /// Tests that SaveMessage correctly handles special characters in content.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_ContentWithSpecialCharacters_SavesCorrectly()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var content = "Test with special chars: <>&\"'\\n\\t\\r";
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = content,
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(content, messages[0].Content);
        }

        /// <summary>
        /// Tests that SaveMessage correctly handles very long content strings.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_VeryLongContent_SavesCorrectly()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = 1,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var content = new string ('A', 10000);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = content,
                ConversationId = 1
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(content, messages[0].Content);
        }

        /// <summary>
        /// Tests that SaveMessage correctly handles maximum integer value for ConversationId.
        /// </summary>
        [TestMethod]
        public async Task SaveMessage_MaxConversationId_WorksCorrectly()
        {
            // Arrange
            var mockChatBotService = new Mock<Services.IChatBotService>();
            var mockFileParserService = new Mock<Services.IFileParserService>();
            var user = new ApplicationUser
            {
                Id = "user123"
            };
            var mockUserManager = MockUserManager();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = int.MaxValue,
                    ApplicationUserId = "user123",
                    Title = "Test Chat"
                }
            }.AsQueryable();
            var mockConversationSet = MockDbSet(conversations);
            var messages = new List<ChatMessage>();
            var mockMessageSet = new Mock<DbSet<ChatMessage>>();
            mockMessageSet.Setup(m => m.Add(It.IsAny<ChatMessage>())).Callback<ChatMessage>(messages.Add);
            var mockContext = MockDbContext();
            mockContext.Setup(c => c.ChatConversation).Returns(mockConversationSet.Object);
            mockContext.Setup(c => c.ChatMessage).Returns(mockMessageSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            var request = new SaveChatMessageRequest
            {
                Role = "user",
                Content = "Test content",
                ConversationId = int.MaxValue
            };
            // Act
            var result = await controller.SaveMessage(request);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var success = GetPropertyValue<bool>(okResult.Value, "Success");
            Assert.IsTrue(success);
        }

        private static Mock<ApplicationDbContext> MockDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return new Mock<ApplicationDbContext>(options);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(IQueryable<T> data)
            where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            return mockSet;
        }

        private static T GetPropertyValue<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found on type '{obj.GetType().Name}'");
            }

            return (T)property.GetValue(obj);
        }

        /// <summary>
        /// Tests that History returns Ok with messages when conversation exists with messages.
        /// </summary>
        [TestMethod]
        public async Task History_ValidConversationWithMessages_ReturnsOkWithMessages()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = 1,
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = "Hello",
                    FileNames = null,
                    SentAt = DateTime.UtcNow.AddMinutes(-2)
                },
                new ChatMessage
                {
                    ChatMessageId = 2,
                    ChatConversationId = conversationId,
                    Role = "assistant",
                    Content = "Hi there!",
                    FileNames = "[\"file1.txt\",\"file2.pdf\"]",
                    SentAt = DateTime.UtcNow.AddMinutes(-1)
                }
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = messages
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic responseValue = okResult.Value;
            Assert.IsNotNull(responseValue);
        }

        /// <summary>
        /// Tests that History returns Ok with empty array when conversation is not found.
        /// </summary>
        [TestMethod]
        public async Task History_ConversationNotFound_ReturnsOkWithEmptyArray()
        {
            // Arrange
            int conversationId = 999;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation>());
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History returns Ok with empty array when conversation has no messages.
        /// </summary>
        [TestMethod]
        public async Task History_ConversationWithNoMessages_ReturnsOkWithEmptyArray()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = new List<ChatMessage>()
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History returns Ok with empty array when conversation belongs to different user.
        /// </summary>
        [TestMethod]
        public async Task History_ConversationBelongsToDifferentUser_ReturnsOkWithEmptyArray()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            string otherUserId = "user456";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = otherUserId,
                Messages = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        ChatMessageId = 1,
                        ChatConversationId = conversationId,
                        Role = "user",
                        Content = "Secret message",
                        FileNames = null,
                        SentAt = DateTime.UtcNow
                    }
                }
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History handles messages with empty FileNames string correctly.
        /// </summary>
        [TestMethod]
        public async Task History_MessageWithEmptyFileNames_ReturnsEmptyFileNamesArray()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = 1,
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = "Test message",
                    FileNames = "",
                    SentAt = DateTime.UtcNow
                }
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = messages
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History returns Ok with empty array when exception occurs during user retrieval.
        /// </summary>
        [TestMethod]
        public async Task History_ExceptionDuringUserRetrieval_ReturnsOkWithEmptyArray()
        {
            // Arrange
            int conversationId = 1;
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("User retrieval failed"));
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History returns Ok with empty array when exception occurs during database query.
        /// </summary>
        [TestMethod]
        public async Task History_ExceptionDuringDatabaseQuery_ReturnsOkWithEmptyArray()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = new Mock<DbSet<ChatConversation>>();
            conversationDbSetMock.As<IQueryable<ChatConversation>>().Setup(m => m.Provider).Throws(new InvalidOperationException("Database connection failed"));
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests History with conversationId boundary values.
        /// Input: conversationId can be 0, negative, int.MinValue, or int.MaxValue.
        /// Expected: Returns Ok with empty array for non-existent conversations.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public async Task History_ConversationIdBoundaryValues_ReturnsOkWithEmptyArray(int conversationId)
        {
            // Arrange
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation>());
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History correctly orders messages by SentAt timestamp.
        /// </summary>
        [TestMethod]
        public async Task History_MultipleMessages_OrderedBySentAt()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            DateTime now = DateTime.UtcNow;
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = 3,
                    ChatConversationId = conversationId,
                    Role = "assistant",
                    Content = "Third message",
                    FileNames = null,
                    SentAt = now.AddMinutes(2)
                },
                new ChatMessage
                {
                    ChatMessageId = 1,
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = "First message",
                    FileNames = null,
                    SentAt = now
                },
                new ChatMessage
                {
                    ChatMessageId = 2,
                    ChatConversationId = conversationId,
                    Role = "assistant",
                    Content = "Second message",
                    FileNames = null,
                    SentAt = now.AddMinutes(1)
                }
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = messages
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History handles messages with valid JSON FileNames correctly.
        /// </summary>
        [TestMethod]
        public async Task History_MessageWithValidJsonFileNames_DeserializesCorrectly()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            string[] fileNames = new[]
            {
                "document.pdf",
                "image.png",
                "data.xlsx"
            };
            string fileNamesJson = JsonSerializer.Serialize(fileNames);
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = 1,
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = "Here are the files",
                    FileNames = fileNamesJson,
                    SentAt = DateTime.UtcNow
                }
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = messages
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that History handles messages with null FileNames correctly.
        /// </summary>
        [TestMethod]
        public async Task History_MessageWithNullFileNames_ReturnsEmptyFileNamesArray()
        {
            // Arrange
            int conversationId = 1;
            string userId = "user123";
            ApplicationUser user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = 1,
                    ChatConversationId = conversationId,
                    Role = "user",
                    Content = "Message without files",
                    FileNames = null,
                    SentAt = DateTime.UtcNow
                }
            };
            ChatConversation conversation = new ChatConversation
            {
                ChatConversationId = conversationId,
                ApplicationUserId = userId,
                Messages = messages
            };
            Mock<UserManager<ApplicationUser>> userManagerMock = MockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            Mock<ApplicationDbContext> contextMock = MockDbContext();
            Mock<DbSet<ChatConversation>> conversationDbSetMock = MockDbSet(new List<ChatConversation> { conversation });
            contextMock.Setup(x => x.ChatConversation).Returns(conversationDbSetMock.Object);
            Mock<IChatBotService> chatBotServiceMock = new Mock<IChatBotService>();
            Mock<IFileParserService> fileParserServiceMock = new Mock<IFileParserService>();
            ChatBotController controller = new ChatBotController(chatBotServiceMock.Object, fileParserServiceMock.Object, userManagerMock.Object, contextMock.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
            // Act
            IActionResult result = await controller.History(conversationId);
            // Assert
            Assert.IsNotNull(result);
            OkObjectResult okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data)
            where T : class
        {
            IQueryable<T> queryable = data.AsQueryable();
            Mock<DbSet<T>> dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return dbSetMock;
        }

        /// <summary>
        /// Tests that Conversations returns Ok with empty array when GetUserAsync throws exception.
        /// Input: GetUserAsync throws exception.
        /// Expected: Returns OkObjectResult with empty array.
        /// </summary>
        [TestMethod]
        public async Task Conversations_GetUserAsyncThrowsException_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var mockChatBotService = new Mock<coderush.Services.IChatBotService>();
            var mockFileParserService = new Mock<coderush.Services.IFileParserService>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("Database connection failed"));
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = CreateControllerContext();
            // Act
            var result = await controller.Conversations();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);
            var conversationsProp = resultValue.GetType().GetProperty("Conversations");
            Assert.IsNotNull(conversationsProp);
            var conversationsArray = conversationsProp.GetValue(resultValue) as Array;
            Assert.IsNotNull(conversationsArray);
            Assert.AreEqual(0, conversationsArray.Length);
        }

        /// <summary>
        /// Tests that Conversations returns Ok with empty array when database query throws exception.
        /// Input: Database query throws exception.
        /// Expected: Returns OkObjectResult with empty array.
        /// </summary>
        [TestMethod]
        public async Task Conversations_DatabaseQueryThrowsException_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser"
            };
            var mockChatBotService = new Mock<coderush.Services.IChatBotService>();
            var mockFileParserService = new Mock<coderush.Services.IFileParserService>();
            var mockUserManager = MockUserManager(user);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ChatConversation>>();
            mockDbSet.As<IQueryable<ChatConversation>>().Setup(m => m.Provider).Throws(new InvalidOperationException("Database error"));
            mockContext.Setup(c => c.ChatConversation).Returns(mockDbSet.Object);
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = CreateControllerContext();
            // Act
            var result = await controller.Conversations();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);
            var conversationsProp = resultValue.GetType().GetProperty("Conversations");
            Assert.IsNotNull(conversationsProp);
            var conversationsArray = conversationsProp.GetValue(resultValue) as Array;
            Assert.IsNotNull(conversationsArray);
            Assert.AreEqual(0, conversationsArray.Length);
        }

        /// <summary>
        /// Tests that Conversations returns Ok with empty array when GetUserAsync returns null.
        /// Input: GetUserAsync returns null.
        /// Expected: Returns OkObjectResult with empty array (due to NullReferenceException caught).
        /// </summary>
        [TestMethod]
        public async Task Conversations_GetUserAsyncReturnsNull_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var mockChatBotService = new Mock<coderush.Services.IChatBotService>();
            var mockFileParserService = new Mock<coderush.Services.IFileParserService>();
            var mockUserManager = MockUserManager(null);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new ChatBotController(mockChatBotService.Object, mockFileParserService.Object, mockUserManager.Object, mockContext.Object, new Mock<IChatResponseOrchestrator>().Object);
            controller.ControllerContext = CreateControllerContext();
            // Act
            var result = await controller.Conversations();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);
            var conversationsProp = resultValue.GetType().GetProperty("Conversations");
            Assert.IsNotNull(conversationsProp);
            var conversationsArray = conversationsProp.GetValue(resultValue) as Array;
            Assert.IsNotNull(conversationsArray);
            Assert.AreEqual(0, conversationsArray.Length);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser? user)
        {
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            return mockUserManager;
        }

    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression, this);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression, this);
        public object Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(Expression) })
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult });
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IAsyncQueryProvider _provider;

        public TestAsyncEnumerable(Expression expression, IAsyncQueryProvider provider)
            : base(expression)
        {
            _provider = provider;
        }

        IQueryProvider IQueryable.Provider => _provider;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}