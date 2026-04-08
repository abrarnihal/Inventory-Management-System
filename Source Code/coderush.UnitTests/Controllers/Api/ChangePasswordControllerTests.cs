using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using coderush;
using coderush.Controllers;
using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.ManageViewModels;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the ChangePasswordController class.
    /// </summary>
    [TestClass]
    public class ChangePasswordControllerTests
    {
        /// <summary>
        /// Tests that Update method calls ChangePasswordAsync when user exists and passwords match.
        /// Input: Valid payload with matching passwords, existing user.
        /// Expected: ChangePasswordAsync is called with correct parameters and Ok result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_UserExistsAndPasswordsMatch_CallsChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";
            var oldPassword = "OldPass123!";
            var newPassword = "NewPass123!";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(user, oldPassword, newPassword), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Update method does not call ChangePasswordAsync when user is not found.
        /// Input: Valid payload but user does not exist in database.
        /// Expected: ChangePasswordAsync is not called and NotFound result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_UserNotFound_DoesNotCallChangePasswordAsync()
        {
            // Arrange
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = "nonexistent",
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var users = new ApplicationUser[0].AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        /// <summary>
        /// Tests that Update method does not call ChangePasswordAsync when passwords do not match.
        /// Input: Valid payload with user existing but NewPassword != ConfirmPassword.
        /// Expected: ChangePasswordAsync is not called and BadRequest result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_PasswordsDoNotMatch_DoesNotCallChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "DifferentPass123!"
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that Update method does not call ChangePasswordAsync when user not found and passwords do not match.
        /// Input: Invalid payload with non-existent user and mismatched passwords.
        /// Expected: ChangePasswordAsync is not called and BadRequest result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_UserNotFoundAndPasswordsMismatch_DoesNotCallChangePasswordAsync()
        {
            // Arrange
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = "nonexistent",
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "DifferentPass123!"
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var users = new ApplicationUser[0].AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that Update method handles empty string passwords correctly.
        /// Input: Valid payload with empty string passwords.
        /// Expected: ChangePasswordAsync is never called because empty strings are rejected by validation, and BadRequest is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_EmptyStringPasswords_CallsChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = string.Empty,
                NewPassword = string.Empty,
                ConfirmPassword = string.Empty
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that Update method handles whitespace-only passwords correctly.
        /// Input: Valid payload with whitespace-only passwords.
        /// Expected: ChangePasswordAsync is called (whitespace strings are equal) and Ok result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_WhitespacePasswords_CallsChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";
            var whitespacePassword = "   ";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = whitespacePassword,
                NewPassword = whitespacePassword,
                ConfirmPassword = whitespacePassword
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(user, whitespacePassword, whitespacePassword), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Update method handles very long passwords correctly.
        /// Input: Valid payload with very long passwords (1000 characters).
        /// Expected: ChangePasswordAsync is called and Ok result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_VeryLongPasswords_CallsChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";
            var longPassword = new string('a', 1000);

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = "OldPass123!",
                NewPassword = longPassword,
                ConfirmPassword = longPassword
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(user, "OldPass123!", longPassword), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Update method handles special characters in passwords correctly.
        /// Input: Valid payload with special characters in passwords.
        /// Expected: ChangePasswordAsync is called and Ok result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_PasswordsWithSpecialCharacters_CallsChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";
            var specialPassword = "P@$$w0rd!#$%^&*()_+-=[]{}|;:',.<>?/~`";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = "OldPass123!",
                NewPassword = specialPassword,
                ConfirmPassword = specialPassword
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(user, "OldPass123!", specialPassword), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Update method returns BadRequest when ChangePasswordAsync fails.
        /// Input: Valid payload with matching passwords, ChangePasswordAsync returns failure.
        /// Expected: BadRequest result is returned with error description.
        /// </summary>
        [TestMethod]
        public async Task Update_ChangePasswordAsyncFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = "user123";
            var oldPassword = "OldPass123!";
            var newPassword = "NewPass123!";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager.Setup(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password change failed" }));

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that Update method handles null Id correctly by searching for user with null Id.
        /// Input: Valid payload with null Id.
        /// Expected: Query executes without exception and Ok result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_NullId_ReturnsOk()
        {
            // Arrange
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = null,
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var users = new ApplicationUser[0].AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        /// <summary>
        /// Tests that Update method handles case-sensitive password comparison correctly.
        /// Input: Valid payload with NewPassword and ConfirmPassword differing only in case.
        /// Expected: ChangePasswordAsync is not called and BadRequest result is returned.
        /// </summary>
        [TestMethod]
        public async Task Update_PasswordsDifferInCase_DoesNotCallChangePasswordAsync()
        {
            // Arrange
            var userId = "user123";

            var changePasswordViewModel = new ChangePasswordViewModel
            {
                Id = userId,
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "newpass123!"
            };

            var payload = new CrudViewModel<ChangePasswordViewModel>
            {
                value = changePasswordViewModel
            };

            var user = new ApplicationUser { Id = userId };
            var users = new[] { user }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that GetChangePassword returns an OkObjectResult with an empty list and zero count
        /// when the Users DbSet contains no users.
        /// </summary>
        [TestMethod]
        public void GetChangePassword_EmptyUserList_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var users = new List<ApplicationUser>();
            var mockContext = CreateMockContext(users);
            var mockUserManager = CreateMockUserManager();
            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetChangePassword();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<ApplicationUser>;
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetChangePassword returns an OkObjectResult with a single user and count of one
        /// when the Users DbSet contains exactly one user.
        /// </summary>
        [TestMethod]
        public void GetChangePassword_SingleUser_ReturnsSingleUserWithCountOne()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", UserName = "testuser1", Email = "test1@example.com" }
            };
            var mockContext = CreateMockContext(users);
            var mockUserManager = CreateMockUserManager();
            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetChangePassword();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<ApplicationUser>;
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("user1", items[0].Id);
            Assert.AreEqual("testuser1", items[0].UserName);
        }

        /// <summary>
        /// Tests that GetChangePassword returns an OkObjectResult with all users and correct count
        /// when the Users DbSet contains multiple users.
        /// </summary>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public void GetChangePassword_MultipleUsers_ReturnsAllUsersWithCorrectCount(int userCount)
        {
            // Arrange
            var users = new List<ApplicationUser>();
            for (int i = 0; i < userCount; i++)
            {
                users.Add(new ApplicationUser
                {
                    Id = $"user{i}",
                    UserName = $"testuser{i}",
                    Email = $"test{i}@example.com"
                });
            }
            var mockContext = CreateMockContext(users);
            var mockUserManager = CreateMockUserManager();
            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetChangePassword();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<ApplicationUser>;
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(userCount, items.Count);
            Assert.AreEqual(userCount, count);

            for (int i = 0; i < userCount; i++)
            {
                Assert.AreEqual($"user{i}", items[i].Id);
            }
        }

        /// <summary>
        /// Tests that GetChangePassword returns the exact same user objects from the context
        /// to ensure no transformation or filtering occurs.
        /// </summary>
        [TestMethod]
        public void GetChangePassword_UsersWithVariousProperties_ReturnsExactUserObjects()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                Id = "id1",
                UserName = "user1",
                Email = "user1@test.com",
                EmailConfirmed = true,
                PhoneNumber = "1234567890"
            };
            var user2 = new ApplicationUser
            {
                Id = "id2",
                UserName = "user2",
                Email = null,
                EmailConfirmed = false,
                PhoneNumber = null
            };
            var user3 = new ApplicationUser
            {
                Id = "id3",
                UserName = "",
                Email = "",
                EmailConfirmed = false
            };

            var users = new List<ApplicationUser> { user1, user2, user3 };
            var mockContext = CreateMockContext(users);
            var mockUserManager = CreateMockUserManager();
            var controller = new ChangePasswordController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetChangePassword();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<ApplicationUser>;

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreSame(user1, items[0]);
            Assert.AreSame(user2, items[1]);
            Assert.AreSame(user3, items[2]);
        }

        private static Mock<ApplicationDbContext> CreateMockContext(List<ApplicationUser> users)
        {
            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<ApplicationUser>>();

            var queryable = users.AsQueryable();
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            return mockContext;
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            return mockUserManager;
        }
    }
}