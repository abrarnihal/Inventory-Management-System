using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the Roles service class.
    /// </summary>
    [TestClass]
    public class RolesTests
    {
        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync creates all roles when none exist.
        /// Verifies that all nested types in MainMenu with RoleName fields result in role creation.
        /// </summary>
        [TestMethod]
        public async Task GenerateRolesFromPagesAsync_NoRolesExist_CreatesAllRoles()
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            // Setup RoleExistsAsync to return false (no roles exist)
            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Setup CreateAsync to return success
            mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.GenerateRolesFromPagesAsync();

            // Assert
            // Verify that CreateAsync was called for each role (30 nested types in MainMenu)
            mockRoleManager.Verify(
                rm => rm.CreateAsync(It.IsAny<IdentityRole>()),
                Times.Exactly(30),
                "CreateAsync should be called once for each nested type in MainMenu");
        }

        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync does not create roles when all already exist.
        /// Verifies that RoleExistsAsync is called but CreateAsync is not called.
        /// </summary>
        [TestMethod]
        public async Task GenerateRolesFromPagesAsync_AllRolesExist_DoesNotCreateRoles()
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            // Setup RoleExistsAsync to return true (all roles exist)
            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.GenerateRolesFromPagesAsync();

            // Assert
            // Verify that RoleExistsAsync was called for each role
            mockRoleManager.Verify(
                rm => rm.RoleExistsAsync(It.IsAny<string>()),
                Times.Exactly(30),
                "RoleExistsAsync should be called once for each nested type in MainMenu");

            // Verify that CreateAsync was never called
            mockRoleManager.Verify(
                rm => rm.CreateAsync(It.IsAny<IdentityRole>()),
                Times.Never,
                "CreateAsync should not be called when roles already exist");
        }

        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync creates specific roles with correct names.
        /// Verifies that the method processes specific roles like Customer and Vendor.
        /// </summary>
        [TestMethod]
        [DataRow("Customer")]
        [DataRow("Vendor")]
        [DataRow("Product")]
        [DataRow("Purchase Order")]
        [DataRow("Dashboard Main")]
        public async Task GenerateRolesFromPagesAsync_SpecificRole_CreatesWithCorrectName(string expectedRoleName)
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            // Setup RoleExistsAsync to return false
            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.GenerateRolesFromPagesAsync();

            // Assert
            mockRoleManager.Verify(
                rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == expectedRoleName)),
                Times.Once,
                $"CreateAsync should be called once with role name '{expectedRoleName}'");
        }

        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync creates only non-existing roles when some exist.
        /// Verifies selective role creation based on existence check.
        /// </summary>
        [TestMethod]
        public async Task GenerateRolesFromPagesAsync_SomeRolesExist_CreatesOnlyMissingRoles()
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            // Setup RoleExistsAsync to return true for "Customer" and false for others
            mockRoleManager.Setup(rm => rm.RoleExistsAsync("Customer"))
                .ReturnsAsync(true);
            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.Is<string>(s => s != "Customer")))
                .ReturnsAsync(false);

            mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.GenerateRolesFromPagesAsync();

            // Assert
            // Verify Customer role was not created
            mockRoleManager.Verify(
                rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Customer")),
                Times.Never,
                "CreateAsync should not be called for existing Customer role");

            // Verify other roles were created (29 remaining)
            mockRoleManager.Verify(
                rm => rm.CreateAsync(It.IsAny<IdentityRole>()),
                Times.Exactly(29),
                "CreateAsync should be called for 29 non-existing roles");
        }

        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync checks existence of all expected role names.
        /// Verifies that all role names from MainMenu nested types are checked.
        /// </summary>
        [TestMethod]
        public async Task GenerateRolesFromPagesAsync_ExecutesSuccessfully_ChecksAllExpectedRoleNames()
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            var checkedRoles = new System.Collections.Generic.List<string>();

            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .Callback<string>(roleName => checkedRoles.Add(roleName))
                .ReturnsAsync(true);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.GenerateRolesFromPagesAsync();

            // Assert
            Assert.AreEqual(30, checkedRoles.Count, "Should check 30 role names");
            Assert.IsTrue(checkedRoles.Contains("Customer"), "Should check Customer role");
            Assert.IsTrue(checkedRoles.Contains("Vendor"), "Should check Vendor role");
            Assert.IsTrue(checkedRoles.Contains("Product"), "Should check Product role");
            Assert.IsTrue(checkedRoles.Contains("Dashboard Main"), "Should check Dashboard Main role");
        }

        /// <summary>
        /// Tests that GenerateRolesFromPagesAsync completes successfully when no exceptions occur.
        /// Verifies the method executes without throwing under normal conditions.
        /// </summary>
        [TestMethod]
        public async Task GenerateRolesFromPagesAsync_NormalExecution_CompletesWithoutException()
        {
            // Arrange
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object,
                null,
                null,
                null,
                null);

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

            mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act & Assert
            try
            {
                await roles.GenerateRolesFromPagesAsync();
                Assert.IsTrue(true, "Method completed without exception");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Method should not throw exception but threw: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that AddToRoles does not add roles when the user is not found.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_UserNotFound_DoesNotAddRoles()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);
            var userId = "nonexistent-user-id";

            // Act
            await roles.AddToRoles(userId);

            // Assert
            mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
            mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        /// <summary>
        /// Tests that AddToRoles successfully adds user to all available roles when user is found and roles exist.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_UserFoundWithMultipleRoles_AddsUserToAllRoles()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            var user = new ApplicationUser { Id = "user-123", UserName = "testuser" };
            var rolesList = new List<IdentityRole>
            {
                new IdentityRole { Id = "role-1", Name = "Admin" },
                new IdentityRole { Id = "role-2", Name = "User" },
                new IdentityRole { Id = "role-3", Name = "Manager" }
            };

            mockUserManager.Setup(x => x.FindByIdAsync("user-123"))
                .ReturnsAsync(user);
            mockRoleManager.Setup(x => x.Roles)
                .Returns(rolesList.AsQueryable());
            mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles("user-123");

            // Assert
            mockUserManager.Verify(x => x.FindByIdAsync("user-123"), Times.Once);
            mockUserManager.Verify(x => x.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r => r.Count() == 3 && r.Contains("Admin") && r.Contains("User") && r.Contains("Manager"))),
                Times.Once);
        }

        /// <summary>
        /// Tests that AddToRoles adds user to roles when only one role exists.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_UserFoundWithSingleRole_AddsUserToRole()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            var user = new ApplicationUser { Id = "user-456", UserName = "testuser2" };
            var rolesList = new List<IdentityRole>
            {
                new IdentityRole { Id = "role-1", Name = "Admin" }
            };

            mockUserManager.Setup(x => x.FindByIdAsync("user-456"))
                .ReturnsAsync(user);
            mockRoleManager.Setup(x => x.Roles)
                .Returns(rolesList.AsQueryable());
            mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles("user-456");

            // Assert
            mockUserManager.Verify(x => x.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r => r.Count() == 1 && r.Contains("Admin"))),
                Times.Once);
        }

        /// <summary>
        /// Tests that AddToRoles handles empty role collection gracefully.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_UserFoundWithNoRoles_AddsUserToEmptyRoleList()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            var user = new ApplicationUser { Id = "user-789", UserName = "testuser3" };
            var rolesList = new List<IdentityRole>();

            mockUserManager.Setup(x => x.FindByIdAsync("user-789"))
                .ReturnsAsync(user);
            mockRoleManager.Setup(x => x.Roles)
                .Returns(rolesList.AsQueryable());
            mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles("user-789");

            // Assert
            mockUserManager.Verify(x => x.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r => r.Count() == 0)),
                Times.Once);
        }

        /// <summary>
        /// Tests that AddToRoles handles null applicationUserId by passing it to FindByIdAsync.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_NullUserId_CallsFindByIdAndDoesNotAddRoles()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles(null);

            // Assert
            mockUserManager.Verify(x => x.FindByIdAsync(null), Times.Once);
            mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        /// <summary>
        /// Tests that AddToRoles handles empty string userId.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_EmptyStringUserId_CallsFindByIdAndDoesNotAddRoles()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            mockUserManager.Setup(x => x.FindByIdAsync(string.Empty))
                .ReturnsAsync((ApplicationUser?)null);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles(string.Empty);

            // Assert
            mockUserManager.Verify(x => x.FindByIdAsync(string.Empty), Times.Once);
            mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        /// <summary>
        /// Tests that AddToRoles handles whitespace-only userId.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_WhitespaceUserId_CallsFindByIdAndDoesNotAddRoles()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            mockUserManager.Setup(x => x.FindByIdAsync("   "))
                .ReturnsAsync((ApplicationUser?)null);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles("   ");

            // Assert
            mockUserManager.Verify(x => x.FindByIdAsync("   "), Times.Once);
            mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        /// <summary>
        /// Tests that AddToRoles correctly extracts role names from IdentityRole objects.
        /// </summary>
        [TestMethod]
        public async Task AddToRoles_RolesWithNames_ExtractsCorrectRoleNames()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                mockRoleStore.Object, null, null, null, null);

            var user = new ApplicationUser { Id = "user-999", UserName = "testuser4" };
            var rolesList = new List<IdentityRole>
            {
                new IdentityRole { Id = "role-1", Name = "SuperAdmin" },
                new IdentityRole { Id = "role-2", Name = "PowerUser" }
            };

            mockUserManager.Setup(x => x.FindByIdAsync("user-999"))
                .ReturnsAsync(user);
            mockRoleManager.Setup(x => x.Roles)
                .Returns(rolesList.AsQueryable());

            List<string>? capturedRoleNames = null;
            mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .Callback<ApplicationUser, IEnumerable<string>>((u, r) => capturedRoleNames = r.ToList())
                .ReturnsAsync(IdentityResult.Success);

            var roles = new Roles(mockRoleManager.Object, mockUserManager.Object);

            // Act
            await roles.AddToRoles("user-999");

            // Assert
            Assert.IsNotNull(capturedRoleNames);
            Assert.AreEqual(2, capturedRoleNames.Count);
            Assert.IsTrue(capturedRoleNames.Contains("SuperAdmin"));
            Assert.IsTrue(capturedRoleNames.Contains("PowerUser"));
        }
    }
}