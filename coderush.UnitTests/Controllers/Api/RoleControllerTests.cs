using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using coderush.Controllers.Api;
using coderush.Models;
using coderush.Models.AccountViewModels;
using coderush.Models.SyncfusionViewModels;
using coderush.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the RoleController class.
    /// </summary>
    [TestClass]
    public class RoleControllerTests
    {
        /// <summary>
        /// Tests that GetRole returns an empty list with zero count when no roles exist in the role manager.
        /// </summary>
        [TestMethod]
        public async Task GetRole_NoRolesExist_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var mockRoles = new Mock<IRoles>();
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            mockRoleManager.Setup(rm => rm.Roles).Returns(new List<IdentityRole>().AsQueryable());

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRole();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);

            var items = resultValue.Items as List<IdentityRole>;
            var count = (int)resultValue.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetRole returns a list with one role and count of one when a single role exists.
        /// </summary>
        [TestMethod]
        public async Task GetRole_SingleRoleExists_ReturnsListWithOneRoleAndCountOne()
        {
            // Arrange
            var mockRoles = new Mock<IRoles>();
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

            var singleRole = new IdentityRole { Id = "1", Name = "Admin" };
            var rolesList = new List<IdentityRole> { singleRole };

            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            mockRoleManager.Setup(rm => rm.Roles).Returns(rolesList.AsQueryable());

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRole();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);

            var items = resultValue.Items as List<IdentityRole>;
            var count = (int)resultValue.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("Admin", items[0].Name);
        }

        /// <summary>
        /// Tests that GetRole returns a list with all roles and correct count when multiple roles exist.
        /// </summary>
        [TestMethod]
        public async Task GetRole_MultipleRolesExist_ReturnsListWithAllRolesAndCorrectCount()
        {
            // Arrange
            var mockRoles = new Mock<IRoles>();
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

            var rolesList = new List<IdentityRole>
            {
                new IdentityRole { Id = "1", Name = "Admin" },
                new IdentityRole { Id = "2", Name = "User" },
                new IdentityRole { Id = "3", Name = "Manager" },
                new IdentityRole { Id = "4", Name = "Guest" }
            };

            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            mockRoleManager.Setup(rm => rm.Roles).Returns(rolesList.AsQueryable());

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRole();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);

            var items = resultValue.Items as List<IdentityRole>;
            var count = (int)resultValue.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(4, count);
            Assert.AreEqual("Admin", items[0].Name);
            Assert.AreEqual("User", items[1].Name);
            Assert.AreEqual("Manager", items[2].Name);
            Assert.AreEqual("Guest", items[3].Name);
        }

        /// <summary>
        /// Tests that GetRole always calls GenerateRolesFromPagesAsync before retrieving roles.
        /// </summary>
        [TestMethod]
        public async Task GetRole_WhenCalled_CallsGenerateRolesFromPagesAsync()
        {
            // Arrange
            var mockRoles = new Mock<IRoles>();
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            mockRoleManager.Setup(rm => rm.Roles).Returns(new List<IdentityRole>().AsQueryable());

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            await controller.GetRole();

            // Assert
            mockRoles.Verify(r => r.GenerateRolesFromPagesAsync(), Times.Once);
        }

        /// <summary>
        /// Tests that GetRole returns the correct structure when roles with special characters exist.
        /// </summary>
        [TestMethod]
        public async Task GetRole_RolesWithSpecialCharacters_ReturnsCorrectStructure()
        {
            // Arrange
            var mockRoles = new Mock<IRoles>();
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

            var rolesList = new List<IdentityRole>
            {
                new IdentityRole { Id = "1", Name = "Admin-User" },
                new IdentityRole { Id = "2", Name = "User_Manager" },
                new IdentityRole { Id = "3", Name = "Role.With.Dots" }
            };

            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            mockRoleManager.Setup(rm => rm.Roles).Returns(rolesList.AsQueryable());

            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRole();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic resultValue = okResult.Value;
            var items = resultValue.Items as List<IdentityRole>;
            var count = (int)resultValue.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Admin-User", items[0].Name);
            Assert.AreEqual("User_Manager", items[1].Name);
            Assert.AreEqual("Role.With.Dots", items[2].Name);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId returns correct role information when user exists and has some roles.
        /// Input: Valid user ID with multiple roles where user belongs to some of them.
        /// Expected: Returns OkObjectResult with correct Items list and Count.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_ValidUserWithSomeRoles_ReturnsCorrectRoleList()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" },
                new IdentityRole { Id = "role2", Name = "User" },
                new IdentityRole { Id = "role3", Name = "Manager" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, "User")).ReturnsAsync(false);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, "Manager")).ReturnsAsync(true);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);

            Assert.AreEqual(1, items[0].CounterId);
            Assert.AreEqual(userId, items[0].ApplicationUserId);
            Assert.AreEqual("Admin", items[0].RoleName);
            Assert.IsTrue(items[0].IsHaveAccess);

            Assert.AreEqual(2, items[1].CounterId);
            Assert.AreEqual(userId, items[1].ApplicationUserId);
            Assert.AreEqual("User", items[1].RoleName);
            Assert.IsFalse(items[1].IsHaveAccess);

            Assert.AreEqual(3, items[2].CounterId);
            Assert.AreEqual(userId, items[2].ApplicationUserId);
            Assert.AreEqual("Manager", items[2].RoleName);
            Assert.IsTrue(items[2].IsHaveAccess);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId returns empty list when no roles exist in the system.
        /// Input: Valid user ID but no roles in the system.
        /// Expected: Returns OkObjectResult with empty Items list and Count = 0.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_NoRolesInSystem_ReturnsEmptyList()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>();

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId returns correct information when user has all roles.
        /// Input: Valid user ID with user belonging to all roles.
        /// Expected: Returns OkObjectResult with all roles marked as IsHaveAccess = true.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_UserHasAllRoles_ReturnsAllRolesWithAccess()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" },
                new IdentityRole { Id = "role2", Name = "User" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(true);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items.All(i => i.IsHaveAccess));
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId returns correct information when user has no roles.
        /// Input: Valid user ID with user belonging to no roles.
        /// Expected: Returns OkObjectResult with all roles marked as IsHaveAccess = false.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_UserHasNoRoles_ReturnsAllRolesWithoutAccess()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" },
                new IdentityRole { Id = "role2", Name = "User" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(false);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items.All(i => !i.IsHaveAccess));
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId works with single role in system.
        /// Input: Valid user ID with only one role in the system.
        /// Expected: Returns OkObjectResult with single role information.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_SingleRole_ReturnsCorrectSingleRoleInfo()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].CounterId);
            Assert.AreEqual("Admin", items[0].RoleName);
            Assert.IsTrue(items[0].IsHaveAccess);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId handles very long user ID strings.
        /// Input: Very long user ID string.
        /// Expected: Processes normally if user is found, otherwise handles as user not found.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_VeryLongUserId_HandlesCorrectly()
        {
            // Arrange
            var userId = new string('a', 1000);
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(false);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(userId, items[0].ApplicationUserId);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId handles special characters in user ID.
        /// Input: User ID with special characters.
        /// Expected: Processes normally if user is found.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_SpecialCharactersInUserId_HandlesCorrectly()
        {
            // Arrange
            var userId = "user-123!@#$%^&*()";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Admin" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(true);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;

            Assert.IsNotNull(items);
            Assert.AreEqual(userId, items[0].ApplicationUserId);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId calls GenerateRolesFromPagesAsync.
        /// Input: Valid user ID.
        /// Expected: GenerateRolesFromPagesAsync is called exactly once.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_ValidCall_CallsGenerateRolesFromPagesAsync()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>();

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            await controller.GetRoleByApplicationUserId(userId);

            // Assert
            mockRoles.Verify(r => r.GenerateRolesFromPagesAsync(), Times.Once);
        }

        /// <summary>
        /// Tests that GetRoleByApplicationUserId correctly increments CounterId.
        /// Input: Valid user ID with multiple roles.
        /// Expected: CounterId starts at 1 and increments sequentially.
        /// </summary>
        [TestMethod]
        public async Task GetRoleByApplicationUserId_MultipleRoles_CorrectlyIncrementsCounterId()
        {
            // Arrange
            var userId = "test-user-123";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "role1", Name = "Role1" },
                new IdentityRole { Id = "role2", Name = "Role2" },
                new IdentityRole { Id = "role3", Name = "Role3" },
                new IdentityRole { Id = "role4", Name = "Role4" },
                new IdentityRole { Id = "role5", Name = "Role5" }
            };

            var mockRoles = new Mock<IRoles>();
            mockRoles.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(false);

            var mockRoleManager = CreateMockRoleManager();
            mockRoleManager.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            // Act
            var result = await controller.GetRoleByApplicationUserId(userId);

            // Assert
            var okResult = result as OkObjectResult;
            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<UserRoleViewModel>;

            Assert.IsNotNull(items);
            Assert.AreEqual(5, items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                Assert.AreEqual(i + 1, items[i].CounterId, $"CounterId at index {i} should be {i + 1}");
            }
        }

        /// <summary>
        /// Helper method to create a mock UserManager.
        /// </summary>
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            return mockUserManager;
        }

        /// <summary>
        /// Helper method to create a mock RoleManager.
        /// </summary>
        private static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                store.Object,
                null,
                null,
                null,
                null);
            return mockRoleManager;
        }

        /// <summary>
        /// Tests that UpdateUserRole returns OkObjectResult with null value when payload.value is null.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_NullPayloadValue_ReturnsOkWithNull()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);
            var payload = new CrudViewModel<UserRoleViewModel> { value = null };

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that UpdateUserRole adds user to role when user exists and IsHaveAccess is true.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_ValidUserWithAccessTrue_AddsUserToRole()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "user123";
            var roleName = "Admin";
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = true
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(userRole, okResult.Value);
            mockUserManager.Verify(um => um.FindByIdAsync(userId), Times.Once);
            mockUserManager.Verify(um => um.AddToRoleAsync(user, roleName), Times.Once);
            mockUserManager.Verify(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateUserRole removes user from role when user exists and IsHaveAccess is false.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_ValidUserWithAccessFalse_RemovesUserFromRole()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "user456";
            var roleName = "Editor";
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = false
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.RemoveFromRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(userRole, okResult.Value);
            mockUserManager.Verify(um => um.FindByIdAsync(userId), Times.Once);
            mockUserManager.Verify(um => um.RemoveFromRoleAsync(user, roleName), Times.Once);
            mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that UpdateUserRole returns Ok without modifying roles when user is not found.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_UserNotFound_ReturnsOkWithoutModifyingRoles()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "nonexistent";
            var roleName = "Admin";
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = true
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(userRole, okResult.Value);
            mockUserManager.Verify(um => um.FindByIdAsync(userId), Times.Once);
            mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            mockUserManager.Verify(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests UpdateUserRole with various edge cases for ApplicationUserId.
        /// </summary>
        /// <param name="userId">The user ID to test.</param>
        /// <param name="isHaveAccess">Whether the user should have access.</param>
        [TestMethod]
        [DataRow(null, true, DisplayName = "Null ApplicationUserId with Access True")]
        [DataRow("", true, DisplayName = "Empty ApplicationUserId with Access True")]
        [DataRow("   ", true, DisplayName = "Whitespace ApplicationUserId with Access True")]
        [DataRow(null, false, DisplayName = "Null ApplicationUserId with Access False")]
        [DataRow("", false, DisplayName = "Empty ApplicationUserId with Access False")]
        public async Task UpdateUserRole_InvalidApplicationUserId_ReturnsOkWithoutModifyingRoles(string userId, bool isHaveAccess)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var roleName = "Admin";
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = isHaveAccess
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(userRole, okResult.Value);
            mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            mockUserManager.Verify(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests UpdateUserRole with various edge cases for RoleName.
        /// </summary>
        /// <param name="roleName">The role name to test.</param>
        /// <param name="isHaveAccess">Whether the user should have access.</param>
        [TestMethod]
        [DataRow(null, true, DisplayName = "Null RoleName with Access True")]
        [DataRow("", true, DisplayName = "Empty RoleName with Access True")]
        [DataRow("   ", true, DisplayName = "Whitespace RoleName with Access True")]
        [DataRow(null, false, DisplayName = "Null RoleName with Access False")]
        [DataRow("", false, DisplayName = "Empty RoleName with Access False")]
        public async Task UpdateUserRole_InvalidRoleName_CallsUserManagerWithInvalidRoleName(string roleName, bool isHaveAccess)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "user123";
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = isHaveAccess
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.AddToRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(um => um.RemoveFromRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(userRole, okResult.Value);
            mockUserManager.Verify(um => um.FindByIdAsync(userId), Times.Once);
            if (isHaveAccess)
            {
                mockUserManager.Verify(um => um.AddToRoleAsync(user, roleName), Times.Once);
            }
            else
            {
                mockUserManager.Verify(um => um.RemoveFromRoleAsync(user, roleName), Times.Once);
            }
        }

        /// <summary>
        /// Tests UpdateUserRole with special characters in role name.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_SpecialCharactersInRoleName_ProcessesCorrectly()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "user123";
            var roleName = "Admin@#$%^&*()";
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = true
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockUserManager.Verify(um => um.AddToRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        /// Tests UpdateUserRole with very long strings for ApplicationUserId and RoleName.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_VeryLongStrings_ProcessesCorrectly()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = new string('a', 10000);
            var roleName = new string('b', 10000);
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = true
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockUserManager.Verify(um => um.FindByIdAsync(userId), Times.Once);
            mockUserManager.Verify(um => um.AddToRoleAsync(user, roleName), Times.Once);
        }

        /// <summary>
        /// Tests UpdateUserRole returns the original userRole in the response.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_ValidRequest_ReturnsOriginalUserRole()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var userId = "user123";
            var roleName = "Manager";
            var user = new ApplicationUser { Id = userId };
            var userRole = new UserRoleViewModel
            {
                CounterId = 42,
                ApplicationUserId = userId,
                RoleName = roleName,
                IsHaveAccess = true
            };
            var payload = new CrudViewModel<UserRoleViewModel> { value = userRole };

            mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.UpdateUserRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnedUserRole = okResult.Value as UserRoleViewModel;
            Assert.IsNotNull(returnedUserRole);
            Assert.AreEqual(userRole.CounterId, returnedUserRole.CounterId);
            Assert.AreEqual(userRole.ApplicationUserId, returnedUserRole.ApplicationUserId);
            Assert.AreEqual(userRole.RoleName, returnedUserRole.RoleName);
            Assert.AreEqual(userRole.IsHaveAccess, returnedUserRole.IsHaveAccess);
            Assert.AreSame(userRole, returnedUserRole);
        }

        /// <summary>
        /// Tests UpdateUserRole with multiple sequential calls to verify state independence.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserRole_MultipleSequentialCalls_EachCallIsIndependent()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockRoleManager = CreateMockRoleManager();
            var mockRoles = new Mock<IRoles>();
            var controller = new RoleController(mockUserManager.Object, mockRoleManager.Object, mockRoles.Object);

            var user1 = new ApplicationUser { Id = "user1" };
            var user2 = new ApplicationUser { Id = "user2" };

            var userRole1 = new UserRoleViewModel
            {
                ApplicationUserId = "user1",
                RoleName = "Admin",
                IsHaveAccess = true
            };
            var payload1 = new CrudViewModel<UserRoleViewModel> { value = userRole1 };

            var userRole2 = new UserRoleViewModel
            {
                ApplicationUserId = "user2",
                RoleName = "Editor",
                IsHaveAccess = false
            };
            var payload2 = new CrudViewModel<UserRoleViewModel> { value = userRole2 };

            mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user1);
            mockUserManager.Setup(um => um.FindByIdAsync("user2")).ReturnsAsync(user2);
            mockUserManager.Setup(um => um.AddToRoleAsync(user1, "Admin")).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(um => um.RemoveFromRoleAsync(user2, "Editor")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result1 = await controller.UpdateUserRole(payload1);
            var result2 = await controller.UpdateUserRole(payload2);

            // Assert
            Assert.IsInstanceOfType(result1, typeof(OkObjectResult));
            Assert.IsInstanceOfType(result2, typeof(OkObjectResult));
            mockUserManager.Verify(um => um.AddToRoleAsync(user1, "Admin"), Times.Once);
            mockUserManager.Verify(um => um.RemoveFromRoleAsync(user2, "Editor"), Times.Once);
        }
    }
}