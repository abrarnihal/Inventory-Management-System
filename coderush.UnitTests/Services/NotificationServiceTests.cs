using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the NotificationService class.
    /// </summary>
    [TestClass]
    public class NotificationServiceTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        #region ResolveRoleFromControllerName

        /// <summary>
        /// Tests that ResolveRoleFromControllerName returns the correct role for a known MainMenu controller.
        /// </summary>
        [TestMethod]
        [DataRow("Customer", "Customer")]
        [DataRow("Vendor", "Vendor")]
        [DataRow("Product", "Product")]
        [DataRow("PurchaseOrder", "Purchase Order")]
        [DataRow("SalesOrder", "Sales Order")]
        [DataRow("Invoice", "Invoice")]
        [DataRow("Bill", "Bill")]
        [DataRow("Shipment", "Shipment")]
        [DataRow("Warehouse", "Warehouse")]
        [DataRow("Branch", "Branch")]
        [DataRow("Dashboard", "Dashboard Main")]
        public void ResolveRoleFromControllerName_KnownController_ReturnsCorrectRole(string controllerName, string expectedRole)
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            var result = service.ResolveRoleFromControllerName(controllerName);

            // Assert
            Assert.AreEqual(expectedRole, result);
        }

        /// <summary>
        /// Tests that ResolveRoleFromControllerName is case-insensitive.
        /// </summary>
        [TestMethod]
        [DataRow("customer", "Customer")]
        [DataRow("CUSTOMER", "Customer")]
        [DataRow("Customer", "Customer")]
        [DataRow("purchaseorder", "Purchase Order")]
        public void ResolveRoleFromControllerName_CaseInsensitive_ReturnsCorrectRole(string controllerName, string expectedRole)
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            var result = service.ResolveRoleFromControllerName(controllerName);

            // Assert
            Assert.AreEqual(expectedRole, result);
        }

        /// <summary>
        /// Tests that ResolveRoleFromControllerName returns null for an unknown controller.
        /// </summary>
        [TestMethod]
        [DataRow("UnknownController")]
        [DataRow("Foo")]
        [DataRow("")]
        public void ResolveRoleFromControllerName_UnknownController_ReturnsNull(string controllerName)
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            var result = service.ResolveRoleFromControllerName(controllerName);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that ResolveRoleFromControllerName resolves API-specific controllers added manually.
        /// </summary>
        [TestMethod]
        [DataRow("PurchaseOrderLine", "Purchase Order")]
        [DataRow("SalesOrderLine", "Sales Order")]
        [DataRow("User", "User")]
        public void ResolveRoleFromControllerName_ApiControllers_ReturnsCorrectRole(string controllerName, string expectedRole)
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            var result = service.ResolveRoleFromControllerName(controllerName);

            // Assert
            Assert.AreEqual(expectedRole, result);
        }

        #endregion

        #region AddNotificationAsync

        /// <summary>
        /// Tests that AddNotificationAsync persists a notification to the database.
        /// </summary>
        [TestMethod]
        public async Task AddNotificationAsync_ValidInput_PersistsNotification()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            await service.AddNotificationAsync("Test message", "user1", "Customer", "Product", "Added");

            // Assert
            Assert.AreEqual(1, await context.Notification.CountAsync());
            var notification = await context.Notification.FirstAsync();
            Assert.AreEqual("Test message", notification.Message);
            Assert.AreEqual("user1", notification.TargetUserId);
            Assert.AreEqual("Customer", notification.TargetRole);
            Assert.AreEqual("Product", notification.EntityName);
            Assert.AreEqual("Added", notification.EntityAction);
        }

        /// <summary>
        /// Tests that AddNotificationAsync works with only a message (all optional parameters null).
        /// </summary>
        [TestMethod]
        public async Task AddNotificationAsync_OnlyMessage_PersistsWithNulls()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            await service.AddNotificationAsync("Simple message");

            // Assert
            var notification = await context.Notification.FirstAsync();
            Assert.AreEqual("Simple message", notification.Message);
            Assert.IsNull(notification.TargetUserId);
            Assert.IsNull(notification.TargetRole);
            Assert.IsNull(notification.EntityName);
            Assert.IsNull(notification.EntityAction);
        }

        /// <summary>
        /// Tests that AddNotificationAsync sets CreatedDateTime to approximately now.
        /// </summary>
        [TestMethod]
        public async Task AddNotificationAsync_SetsCreatedDateTime_ApproximatelyNow()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);
            var before = DateTimeOffset.UtcNow;

            // Act
            await service.AddNotificationAsync("Timed message");

            // Assert
            var notification = await context.Notification.FirstAsync();
            Assert.IsTrue(notification.CreatedDateTime >= before.AddSeconds(-1));
            Assert.IsTrue(notification.CreatedDateTime <= DateTimeOffset.UtcNow.AddSeconds(1));
        }

        #endregion

        #region GetNotificationsAsync

        /// <summary>
        /// Tests that GetNotificationsAsync returns notifications targeted at a specific user.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_UserTargeted_ReturnsUserNotifications()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "For user1", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "For user2", TargetUserId = "user2", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 10);

            // Assert
            Assert.AreEqual(1, totalCount);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("For user1", items[0].Message);
        }

        /// <summary>
        /// Tests that GetNotificationsAsync returns notifications targeted at a user's role.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_RoleTargeted_ReturnsRoleNotifications()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "For Customer role", TargetRole = "Customer", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "For Vendor role", TargetRole = "Vendor", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string> { "Customer" }, 1, 10);

            // Assert
            Assert.AreEqual(1, totalCount);
            Assert.AreEqual("For Customer role", items[0].Message);
        }

        /// <summary>
        /// Tests that GetNotificationsAsync excludes soft-deleted notifications.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_DeletedNotification_ExcludesFromResults()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Visible", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Deleted", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 2, UserId = "user1", IsDeleted = true });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 10);

            // Assert
            Assert.AreEqual(1, totalCount);
            Assert.AreEqual("Visible", items[0].Message);
        }

        /// <summary>
        /// Tests that GetNotificationsAsync respects pagination.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            for (int i = 1; i <= 5; i++)
            {
                context.Notification.Add(new Notification
                {
                    NotificationId = i,
                    Message = $"Notification {i}",
                    TargetUserId = "user1",
                    CreatedDateTime = DateTimeOffset.UtcNow.AddMinutes(i)
                });
            }
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 2);

            // Assert
            Assert.AreEqual(5, totalCount);
            Assert.AreEqual(2, items.Count);
        }

        /// <summary>
        /// Tests that GetNotificationsAsync filters by search keyword.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_WithSearch_FiltersResults()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Product record added", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Customer record deleted", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 10, "Product");

            // Assert
            Assert.AreEqual(1, totalCount);
            Assert.AreEqual("Product record added", items[0].Message);
        }

        /// <summary>
        /// Tests that GetNotificationsAsync returns correct unread count.
        /// </summary>
        [TestMethod]
        public async Task GetNotificationsAsync_MixedReadState_ReturnsCorrectUnreadCount()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Unread", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Read", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 2, UserId = "user1", IsRead = true });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var (items, totalCount, unreadCount) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 10);

            // Assert
            Assert.AreEqual(2, totalCount);
            Assert.AreEqual(1, unreadCount);
        }

        #endregion

        #region ToggleReadAsync

        /// <summary>
        /// Tests that ToggleReadAsync creates a new read status when none exists and marks as read.
        /// </summary>
        [TestMethod]
        public async Task ToggleReadAsync_NoExistingStatus_CreatesStatusAndMarksRead()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Test", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.ToggleReadAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);
            var status = await context.NotificationReadStatus.FirstAsync();
            Assert.IsTrue(status.IsRead);
            Assert.AreEqual("user1", status.UserId);
            Assert.AreEqual(1, status.NotificationId);
        }

        /// <summary>
        /// Tests that ToggleReadAsync toggles read state from true to false.
        /// </summary>
        [TestMethod]
        public async Task ToggleReadAsync_AlreadyRead_TogglestoUnread()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Test", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 1, UserId = "user1", IsRead = true });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.ToggleReadAsync(1, "user1");

            // Assert
            Assert.IsFalse(result);
            var status = await context.NotificationReadStatus.FirstAsync();
            Assert.IsFalse(status.IsRead);
        }

        /// <summary>
        /// Tests that ToggleReadAsync toggles read state from false to true.
        /// </summary>
        [TestMethod]
        public async Task ToggleReadAsync_AlreadyUnread_TogglestoRead()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Test", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 1, UserId = "user1", IsRead = false });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.ToggleReadAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region DeleteNotificationAsync

        /// <summary>
        /// Tests that DeleteNotificationAsync creates a new status and marks as deleted when none exists.
        /// </summary>
        [TestMethod]
        public async Task DeleteNotificationAsync_NoExistingStatus_CreatesDeletedStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Test", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.DeleteNotificationAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);
            var status = await context.NotificationReadStatus.FirstAsync();
            Assert.IsTrue(status.IsDeleted);
            Assert.IsTrue(status.IsRead);
        }

        /// <summary>
        /// Tests that DeleteNotificationAsync updates existing status to deleted.
        /// </summary>
        [TestMethod]
        public async Task DeleteNotificationAsync_ExistingStatus_UpdatesToDeleted()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Test", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 1, UserId = "user1", IsRead = false, IsDeleted = false });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.DeleteNotificationAsync(1, "user1");

            // Assert
            Assert.IsTrue(result);
            var status = await context.NotificationReadStatus.FirstAsync();
            Assert.IsTrue(status.IsDeleted);
            Assert.IsTrue(status.IsRead);
        }

        /// <summary>
        /// Tests that a deleted notification no longer appears in GetNotificationsAsync.
        /// </summary>
        [TestMethod]
        public async Task DeleteNotificationAsync_AfterDeletion_NotificationHiddenFromUser()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "To delete", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Keep", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);
            await service.DeleteNotificationAsync(1, "user1");

            // Act
            var (items, totalCount, _) = await service.GetNotificationsAsync("user1", new List<string>(), 1, 10);

            // Assert
            Assert.AreEqual(1, totalCount);
            Assert.AreEqual("Keep", items[0].Message);
        }

        #endregion

        #region GetUnreadCountAsync

        /// <summary>
        /// Tests that GetUnreadCountAsync returns 0 when there are no notifications.
        /// </summary>
        [TestMethod]
        public async Task GetUnreadCountAsync_NoNotifications_ReturnsZero()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user1", new List<string>());

            // Assert
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Tests that GetUnreadCountAsync counts only unread notifications for the user.
        /// </summary>
        [TestMethod]
        public async Task GetUnreadCountAsync_MixedReadAndUnread_ReturnsUnreadOnly()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Unread 1", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Read 1", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 3, Message = "Unread 2", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 2, UserId = "user1", IsRead = true });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user1", new List<string>());

            // Assert
            Assert.AreEqual(2, result);
        }

        /// <summary>
        /// Tests that GetUnreadCountAsync excludes deleted notifications.
        /// </summary>
        [TestMethod]
        public async Task GetUnreadCountAsync_DeletedNotification_ExcludedFromCount()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "Deleted unread", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Active unread", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.NotificationReadStatus.Add(new NotificationReadStatus { NotificationId = 1, UserId = "user1", IsDeleted = true, IsRead = false });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user1", new List<string>());

            // Assert
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Tests that GetUnreadCountAsync includes role-targeted notifications.
        /// </summary>
        [TestMethod]
        public async Task GetUnreadCountAsync_RoleTargetedNotification_IncludedInCount()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Notification.Add(new Notification { NotificationId = 1, Message = "User notif", TargetUserId = "user1", CreatedDateTime = DateTimeOffset.UtcNow });
            context.Notification.Add(new Notification { NotificationId = 2, Message = "Role notif", TargetRole = "Customer", CreatedDateTime = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetUnreadCountAsync("user1", new List<string> { "Customer" });

            // Assert
            Assert.AreEqual(2, result);
        }

        #endregion
    }
}
