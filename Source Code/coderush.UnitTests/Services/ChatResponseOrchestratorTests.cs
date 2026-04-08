using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the ChatResponseOrchestrator class.
    /// </summary>
    [TestClass]
    public class ChatResponseOrchestratorTests
    {
        private Mock<IServiceScopeFactory> _mockScopeFactory;
        private Mock<ILogger<ChatResponseOrchestrator>> _mockLogger;
        private ChatResponseOrchestrator _orchestrator;

        [TestInitialize]
        public void Setup()
        {
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockLogger = new Mock<ILogger<ChatResponseOrchestrator>>();
            _orchestrator = new ChatResponseOrchestrator(_mockScopeFactory.Object, _mockLogger.Object);
        }

        #region IsPending

        /// <summary>
        /// Tests that IsPending returns false when no conversations are queued.
        /// </summary>
        [TestMethod]
        public void IsPending_NoQueuedConversations_ReturnsFalse()
        {
            // Act
            var result = _orchestrator.IsPending(1);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsPending returns false for a non-existent conversation ID.
        /// </summary>
        [TestMethod]
        public void IsPending_NonExistentConversationId_ReturnsFalse()
        {
            // Act
            var result = _orchestrator.IsPending(999);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsPending returns true after a conversation has been queued.
        /// </summary>
        [TestMethod]
        public async Task IsPending_AfterQueueing_ReturnsTrue()
        {
            // Arrange
            SetupMockScopeForChatAsync("AI response");

            await _orchestrator.QueueResponseAsync(
                42, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Act
            var result = _orchestrator.IsPending(42);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that IsPending returns false for a different conversation ID than the one queued.
        /// </summary>
        [TestMethod]
        public async Task IsPending_DifferentConversationId_ReturnsFalse()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            await _orchestrator.QueueResponseAsync(
                42, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Act
            var result = _orchestrator.IsPending(99);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region QueueResponseAsync

        /// <summary>
        /// Tests that QueueResponseAsync returns true on first queue for a conversation.
        /// </summary>
        [TestMethod]
        public async Task QueueResponseAsync_FirstQueue_ReturnsTrue()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            // Act
            var result = await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that QueueResponseAsync returns false when the same conversation is already pending.
        /// </summary>
        [TestMethod]
        public async Task QueueResponseAsync_DuplicateConversation_ReturnsFalse()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Act
            var result = await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello again",
                new List<ChatMessageDto>(),
                new List<string>());

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that QueueResponseAsync allows different conversations to be queued.
        /// </summary>
        [TestMethod]
        public async Task QueueResponseAsync_DifferentConversations_AllReturnTrue()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            // Act
            var result1 = await _orchestrator.QueueResponseAsync(1, "user1", "Msg1", new List<ChatMessageDto>(), new List<string>());
            var result2 = await _orchestrator.QueueResponseAsync(2, "user1", "Msg2", new List<ChatMessageDto>(), new List<string>());
            var result3 = await _orchestrator.QueueResponseAsync(3, "user2", "Msg3", new List<ChatMessageDto>(), new List<string>());

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.IsTrue(result3);
        }

        /// <summary>
        /// Tests that QueueResponseAsync handles null history and files gracefully.
        /// </summary>
        [TestMethod]
        public async Task QueueResponseAsync_NullHistoryAndFiles_ReturnsTrue()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            // Act
            var result = await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello", null, null, null);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region StopResponseAsync

        /// <summary>
        /// Tests that StopResponseAsync returns false when no conversation is pending.
        /// </summary>
        [TestMethod]
        public async Task StopResponseAsync_NoPendingConversation_ReturnsFalse()
        {
            // Act
            var result = await _orchestrator.StopResponseAsync(1, "user1");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that StopResponseAsync returns false when the user ID doesn't match.
        /// </summary>
        [TestMethod]
        public async Task StopResponseAsync_WrongUser_ReturnsFalse()
        {
            // Arrange
            SetupMockScopeForChatAsync("response");

            await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Act
            var result = await _orchestrator.StopResponseAsync(1, "wrong-user");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that StopResponseAsync returns true and removes the pending conversation.
        /// </summary>
        [TestMethod]
        public async Task StopResponseAsync_MatchingUserAndConversation_ReturnsTrueAndRemovesPending()
        {
            // Arrange
            SetupMockScopeForStop();

            await _orchestrator.QueueResponseAsync(
                1, "user1", "Hello",
                new List<ChatMessageDto>(),
                new List<string>());

            // Give a small delay for the background task to start
            await Task.Delay(50);

            // Act
            var result = await _orchestrator.StopResponseAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);

            // Give time for cleanup
            await Task.Delay(100);
            Assert.IsFalse(_orchestrator.IsPending(1));
        }

        #endregion

        #region Helpers

        private void SetupMockScopeForChatAsync(string chatResponse)
        {
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockChatBotService = new Mock<IChatBotService>();

            // Setup ChatBotService to delay indefinitely so the pending state persists during testing
            mockChatBotService.Setup(s => s.ChatAsync(
                    It.IsAny<string>(),
                    It.IsAny<List<ChatMessageDto>>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<List<ChatFileContent>>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>()))
                .Returns(async (string msg, List<ChatMessageDto> h, IList<string> r, List<ChatFileContent> f, CancellationToken ct, string uid) =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return chatResponse;
                });

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(dbOptions);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IChatBotService)))
                .Returns(mockChatBotService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
                .Returns(dbContext);

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        }

        private void SetupMockScopeForStop()
        {
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockChatBotService = new Mock<IChatBotService>();

            mockChatBotService.Setup(s => s.ChatAsync(
                    It.IsAny<string>(),
                    It.IsAny<List<ChatMessageDto>>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<List<ChatFileContent>>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<string>()))
                .Returns(async (string msg, List<ChatMessageDto> h, IList<string> r, List<ChatFileContent> f, CancellationToken ct, string uid) =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return "response";
                });

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(dbOptions);

            // Add a conversation for Stop to find
            dbContext.ChatConversation.Add(new ChatConversation
            {
                ChatConversationId = 1,
                ApplicationUserId = "user1",
                Title = "Test"
            });
            dbContext.SaveChanges();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IChatBotService)))
                .Returns(mockChatBotService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
                .Returns(dbContext);

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        }

        #endregion
    }
}
