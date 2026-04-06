using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using coderush.Filters;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Filters.UnitTests
{
    /// <summary>
    /// Unit tests for the NotificationActionFilter class.
    /// </summary>
    [TestClass]
    public class NotificationActionFilterTests
    {
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IServiceScopeFactory> _mockScopeFactory;
        private NotificationActionFilter _filter;

        [TestInitialize]
        public void Setup()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _filter = new NotificationActionFilter();

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationService)))
                .Returns(_mockNotificationService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<NotificationActionFilter>)))
                .Returns(Mock.Of<ILogger<NotificationActionFilter>>());

            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync does not create a notification when the result is not OkObjectResult.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_NonOkResult_DoesNotCreateNotification()
        {
            // Arrange
            var context = CreateActionExecutingContext("Product", "Insert");
            var executedContext = CreateActionExecutedContext(context, new BadRequestResult());

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync does not create a notification for skipped controllers.
        /// </summary>
        [TestMethod]
        [DataRow("Notification")]
        [DataRow("Captcha")]
        [DataRow("UploadProfilePicture")]
        [DataRow("ChatBot")]
        [DataRow("ChangePassword")]
        [DataRow("Health")]
        public async Task OnActionExecutionAsync_SkippedController_DoesNotCreateNotification(string controllerName)
        {
            // Arrange
            var context = CreateActionExecutingContext(controllerName, "Insert");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync does not create a notification for non-mutating actions.
        /// </summary>
        [TestMethod]
        [DataRow("Index")]
        [DataRow("GetById")]
        [DataRow("List")]
        [DataRow("Details")]
        public async Task OnActionExecutionAsync_NonMutatingAction_DoesNotCreateNotification(string actionName)
        {
            // Arrange
            var context = CreateActionExecutingContext("Product", actionName);
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync creates a notification for Insert action.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_InsertAction_CreatesNotificationWithAddedAction()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("Product")).Returns("Product");
            _mockNotificationService.Setup(s => s.AddNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var context = CreateActionExecutingContext("Product", "Insert", "user-123");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(
                    It.Is<string>(m => m.Contains("added")),
                    "user-123",
                    "Product",
                    "Product",
                    "Added"),
                Times.Once);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync creates a notification for Update action.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_UpdateAction_CreatesNotificationWithEditedAction()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("Customer")).Returns("Customer");
            _mockNotificationService.Setup(s => s.AddNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var context = CreateActionExecutingContext("Customer", "Update", "user-456");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(
                    It.Is<string>(m => m.Contains("edited")),
                    "user-456",
                    "Customer",
                    "Customer",
                    "Edited"),
                Times.Once);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync creates a notification for Remove action.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_RemoveAction_CreatesNotificationWithDeletedAction()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("Vendor")).Returns("Vendor");
            _mockNotificationService.Setup(s => s.AddNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var context = CreateActionExecutingContext("Vendor", "Remove", "user-789");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(
                    It.Is<string>(m => m.Contains("deleted")),
                    "user-789",
                    "Vendor",
                    "Vendor",
                    "Deleted"),
                Times.Once);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync does not create a notification when role cannot be resolved.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_UnresolvableRole_DoesNotCreateNotification()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("UnknownCtrl")).Returns((string)null);

            var context = CreateActionExecutingContext("UnknownCtrl", "Insert");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync sends a welcome notification when a User is created.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_UserInsert_SendsWelcomeNotification()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("User")).Returns("User");
            _mockNotificationService.Setup(s => s.AddNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var newUser = new UserProfile { ApplicationUserId = "new-user-id" };
            var context = CreateActionExecutingContext("User", "Insert", "admin-user");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult(newUser));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            // Standard notification
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(
                    It.Is<string>(m => m.Contains("added")),
                    "admin-user",
                    "User",
                    "User",
                    "Added"),
                Times.Once);

            // Welcome notification
            _mockNotificationService.Verify(
                s => s.AddNotificationAsync(
                    It.Is<string>(m => m.Contains("Congratulations")),
                    "new-user-id",
                    null, null, null),
                Times.Once);
        }

        /// <summary>
        /// Tests that OnActionExecutionAsync handles exceptions in notification creation gracefully.
        /// </summary>
        [TestMethod]
        public async Task OnActionExecutionAsync_NotificationServiceThrows_DoesNotBubbleException()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.ResolveRoleFromControllerName("Product")).Returns("Product");
            _mockNotificationService.Setup(s => s.AddNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var context = CreateActionExecutingContext("Product", "Insert");
            var executedContext = CreateActionExecutedContext(context, new OkObjectResult("data"));

            ActionExecutionDelegate next = () => Task.FromResult(executedContext);

            // Act & Assert — should not throw
            await _filter.OnActionExecutionAsync(context, next);
        }

        #region Helpers

        private ActionExecutingContext CreateActionExecutingContext(
            string controllerName, string actionName, string userId = null)
        {
            var httpContext = new DefaultHttpContext();

            // Register mock services so that real IServiceScopeFactory resolves them in child scopes
            var services = new ServiceCollection();
            services.AddSingleton<INotificationService>(_mockNotificationService.Object);
            services.AddSingleton<ILogger<NotificationActionFilter>>(Mock.Of<ILogger<NotificationActionFilter>>());
            httpContext.RequestServices = services.BuildServiceProvider();

            if (userId != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            }

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = controllerName,
                ActionName = actionName
            };

            var routeData = new RouteData();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller: null);
        }

        private ActionExecutedContext CreateActionExecutedContext(
            ActionExecutingContext executingContext, IActionResult result)
        {
            var executedContext = new ActionExecutedContext(
                executingContext,
                new List<IFilterMetadata>(),
                controller: null)
            {
                Result = result
            };

            return executedContext;
        }

        #endregion
    }
}
