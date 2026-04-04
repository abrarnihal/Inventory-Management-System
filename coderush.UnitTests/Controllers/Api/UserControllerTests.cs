using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the UserController class.
    /// </summary>
    [TestClass]
    public class UserControllerTests
    {
        /// <summary>
        /// Tests Insert method when passwords match and user creation succeeds.
        /// Verifies that the UserProfile is added to the database and saved correctly.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordsMatchAndCreationSucceeds_ReturnsOkWithUserProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;
            var createdUser = new ApplicationUser
            {
                Id = "user-id-123",
                Email = userProfile.Email,
                PasswordHash = "hashed-password"
            };

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = createdUser.Id;
                    user.PasswordHash = createdUser.PasswordHash;
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual(createdUser.PasswordHash, returnedProfile.Password);
            Assert.AreEqual(createdUser.PasswordHash, returnedProfile.ConfirmPassword);
            Assert.AreEqual(createdUser.Id, returnedProfile.ApplicationUserId);

            mockUserProfileDbSet.Verify(x => x.Add(It.IsAny<UserProfile>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests Insert method when passwords do not match.
        /// Verifies that user creation is skipped and database operations are not performed.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordsDoNotMatch_SkipsUserCreationAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual("Password123!", returnedProfile.Password);
            Assert.AreEqual("DifferentPassword!", returnedProfile.ConfirmPassword);

            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            mockUserProfileDbSet.Verify(x => x.Add(It.IsAny<UserProfile>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests Insert method when passwords match but user creation fails.
        /// Verifies that database operations are not performed when user creation fails.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordsMatchButCreationFails_DoesNotSaveToDatabase()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityError = new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" };
            var identityResult = IdentityResult.Failed(identityError);

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual("Password123!", returnedProfile.Password);
            Assert.AreEqual("Password123!", returnedProfile.ConfirmPassword);

            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            mockUserProfileDbSet.Verify(x => x.Add(It.IsAny<UserProfile>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests Insert method with empty password strings.
        /// Verifies that the method handles empty password strings correctly.
        /// </summary>
        [TestMethod]
        public async Task Insert_EmptyPasswordsMatch_CreatesUserWithEmptyPassword()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "",
                ConfirmPassword = "",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "user-id-123";
                    user.PasswordHash = "hashed-empty-password";
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), ""), Times.Once);
        }

        /// <summary>
        /// Tests Insert method with whitespace-only password strings.
        /// Verifies that passwords with only whitespace are treated as distinct values.
        /// </summary>
        [TestMethod]
        public async Task Insert_WhitespaceOnlyPasswordsDoNotMatch_SkipsUserCreation()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "   ",
                ConfirmPassword = "  ",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            // Act
            var result = await controller.Insert(payload);

            // Assert
            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests Insert method with special characters in email.
        /// Verifies that emails with special characters are processed correctly.
        /// </summary>
        [TestMethod]
        public async Task Insert_EmailWithSpecialCharacters_CreatesUserSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test+special@example.co.uk",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "user-id-123";
                    user.PasswordHash = "hashed-password";
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            mockUserManager.Verify(x => x.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == "test+special@example.co.uk" && u.UserName == "test+special@example.co.uk"),
                "Password123!"), Times.Once);
        }

        /// <summary>
        /// Tests Insert method with very long password.
        /// Verifies that long passwords are handled correctly.
        /// </summary>
        [TestMethod]
        public async Task Insert_VeryLongPassword_CreatesUserSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var longPassword = new string('a', 1000);
            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = longPassword,
                ConfirmPassword = longPassword,
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "user-id-123";
                    user.PasswordHash = "hashed-long-password";
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), longPassword), Times.Once);
        }

        /// <summary>
        /// Tests Insert method with password containing special characters.
        /// Verifies that passwords with special characters are processed correctly.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordWithSpecialCharacters_CreatesUserSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var specialPassword = "P@$$w0rd!#%&*()[]{}|;:',.<>?/\\`~";
            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = specialPassword,
                ConfirmPassword = specialPassword,
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "user-id-123";
                    user.PasswordHash = "hashed-special-password";
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), specialPassword), Times.Once);
        }

        /// <summary>
        /// Tests Insert method with case-sensitive password matching.
        /// Verifies that password comparison is case-sensitive.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordsDifferInCase_SkipsUserCreation()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            // Act
            var result = await controller.Insert(payload);

            // Assert
            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests Insert method verifies EmailConfirmed is set to true.
        /// Verifies that the ApplicationUser is created with EmailConfirmed set to true.
        /// </summary>
        [TestMethod]
        public async Task Insert_PasswordsMatch_SetsEmailConfirmedToTrue()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            var identityResult = IdentityResult.Success;

            mockUserManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult)
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    user.Id = "user-id-123";
                    user.PasswordHash = "hashed-password";
                });

            mockContext.Setup(x => x.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await controller.Insert(payload);

            // Assert
            mockUserManager.Verify(x => x.CreateAsync(
                It.Is<ApplicationUser>(u => u.EmailConfirmed == true),
                It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Helper method to create a mock UserManager.
        /// </summary>
        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// Tests that GetUser returns an empty list with count zero when the database contains no user profiles.
        /// Input: Empty UserProfile collection.
        /// Expected: OkObjectResult with Items as empty list and Count as 0.
        /// </summary>
        [TestMethod]
        public void GetUser_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<UserProfile>();
            var mockDbSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockDbSet.Object);
            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetUser();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<UserProfile>;
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, value.Count);
        }

        /// <summary>
        /// Tests that GetUser returns a list with one user profile and count one when the database contains a single user.
        /// Input: UserProfile collection with one item.
        /// Expected: OkObjectResult with Items containing one UserProfile and Count as 1.
        /// </summary>
        [TestMethod]
        public void GetUser_SingleUser_ReturnsListWithOneUserAndCountOne()
        {
            // Arrange
            var testUser = new UserProfile
            {
                UserProfileId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                ApplicationUserId = "user-123"
            };
            var userData = new List<UserProfile> { testUser };
            var mockDbSet = CreateMockDbSet(userData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockDbSet.Object);
            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetUser();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<UserProfile>;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual(testUser.UserProfileId, items[0].UserProfileId);
            Assert.AreEqual(testUser.Email, items[0].Email);
        }

        /// <summary>
        /// Tests that GetUser returns a list with multiple user profiles and correct count when the database contains multiple users.
        /// Input: UserProfile collection with multiple items.
        /// Expected: OkObjectResult with Items containing all UserProfiles and Count matching the number of users.
        /// </summary>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public void GetUser_MultipleUsers_ReturnsListWithAllUsersAndCorrectCount(int userCount)
        {
            // Arrange
            var userData = new List<UserProfile>();
            for (int i = 1; i <= userCount; i++)
            {
                userData.Add(new UserProfile
                {
                    UserProfileId = i,
                    FirstName = $"FirstName{i}",
                    LastName = $"LastName{i}",
                    Email = $"user{i}@example.com",
                    ApplicationUserId = $"user-{i}"
                });
            }
            var mockDbSet = CreateMockDbSet(userData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockDbSet.Object);
            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetUser();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<UserProfile>;
            Assert.IsNotNull(items);
            Assert.AreEqual(userCount, items.Count);
            Assert.AreEqual(userCount, value.Count);

            for (int i = 0; i < userCount; i++)
            {
                Assert.AreEqual(userData[i].UserProfileId, items[i].UserProfileId);
                Assert.AreEqual(userData[i].Email, items[i].Email);
            }
        }

        /// <summary>
        /// Creates a mock DbSet from a list of entities that can be enumerated and queried.
        /// </summary>
        private static Mock<DbSet<UserProfile>> CreateMockDbSet(List<UserProfile> data)
        {
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<UserProfile>>();

            mockDbSet.As<IQueryable<UserProfile>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<UserProfile>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<UserProfile>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<UserProfile>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockDbSet;
        }

        /// <summary>
        /// Tests that GetByApplicationUserId returns one UserProfile when a matching id is found.
        /// Input: Valid application user id that exists in the database.
        /// Expected: Returns OkObjectResult with Items containing one UserProfile and Count equals 1.
        /// </summary>
        [TestMethod]
        [DataRow("user-123")]
        [DataRow("550e8400-e29b-41d4-a716-446655440000")]
        [DataRow("test@example.com")]
        public void GetByApplicationUserId_ExistingId_ReturnsOneUserProfile(string id)
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<UserProfile>;
            var count = (int)(countProperty.GetValue(value) ?? 0);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(id, items[0].ApplicationUserId);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId returns empty list when no matching id is found.
        /// Input: Application user id that does not exist in the database.
        /// Expected: Returns OkObjectResult with empty Items list and Count equals 0.
        /// </summary>
        [TestMethod]
        [DataRow("nonexistent-id")]
        [DataRow("00000000-0000-0000-0000-000000000000")]
        public void GetByApplicationUserId_NonExistingId_ReturnsEmptyList(string id)
        {
            // Arrange
            var userProfiles = new List<UserProfile>
            {
                new UserProfile
                {
                    UserProfileId = 1,
                    ApplicationUserId = "different-id",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<UserProfile>;
            var count = (int)(countProperty.GetValue(value) ?? 0);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, items.Count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId handles null id parameter correctly.
        /// Input: Null id parameter.
        /// Expected: Returns OkObjectResult with empty Items list and Count equals 0 (or matches profile with null ApplicationUserId if exists).
        /// </summary>
        [TestMethod]
        public void GetByApplicationUserId_NullId_ReturnsEmptyList()
        {
            // Arrange
            var userProfiles = new List<UserProfile>
            {
                new UserProfile
                {
                    UserProfileId = 1,
                    ApplicationUserId = "valid-id",
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice@example.com"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(null);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty);

            var count = (int)(countProperty.GetValue(value) ?? 0);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId handles empty string id parameter.
        /// Input: Empty string id.
        /// Expected: Returns OkObjectResult with empty Items list and Count equals 0 (or matches profile with empty ApplicationUserId if exists).
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void GetByApplicationUserId_EmptyOrWhitespaceId_ReturnsEmptyList(string id)
        {
            // Arrange
            var userProfiles = new List<UserProfile>
            {
                new UserProfile
                {
                    UserProfileId = 1,
                    ApplicationUserId = "valid-id",
                    FirstName = "Bob",
                    LastName = "Brown",
                    Email = "bob@example.com"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty);

            var count = (int)(countProperty.GetValue(value) ?? 0);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId handles very long string id parameter.
        /// Input: Very long string id (edge case for string parameter).
        /// Expected: Returns OkObjectResult with empty Items list and Count equals 0.
        /// </summary>
        [TestMethod]
        public void GetByApplicationUserId_VeryLongId_ReturnsEmptyList()
        {
            // Arrange
            var veryLongId = new string('a', 10000);
            var userProfiles = new List<UserProfile>
            {
                new UserProfile
                {
                    UserProfileId = 1,
                    ApplicationUserId = "normal-id",
                    FirstName = "Charlie",
                    LastName = "Davis",
                    Email = "charlie@example.com"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(veryLongId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty);

            var count = (int)(countProperty.GetValue(value) ?? 0);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId handles special characters in id parameter.
        /// Input: Id string containing special characters.
        /// Expected: Returns OkObjectResult with appropriate result based on match.
        /// </summary>
        [TestMethod]
        [DataRow("user-id-with-special-!@#$%")]
        [DataRow("user\nid\twith\rcontrol")]
        [DataRow("user'id\"with<>quotes")]
        public void GetByApplicationUserId_SpecialCharactersInId_HandlesCorrectly(string id)
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = id,
                FirstName = "Special",
                LastName = "User",
                Email = "special@example.com"
            };

            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(countProperty);

            var count = (int)(countProperty.GetValue(value) ?? 0);
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId returns empty list when database has no UserProfiles.
        /// Input: Any id when database is empty.
        /// Expected: Returns OkObjectResult with empty Items list and Count equals 0.
        /// </summary>
        [TestMethod]
        public void GetByApplicationUserId_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var userProfiles = new List<UserProfile>().AsQueryable();
            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId("any-id");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<UserProfile>;
            var count = (int)(countProperty.GetValue(value) ?? 0);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, items.Count);
        }

        /// <summary>
        /// Tests that GetByApplicationUserId returns correct UserProfile when multiple profiles exist.
        /// Input: Id that matches one specific profile among multiple profiles.
        /// Expected: Returns OkObjectResult with Items containing only the matching UserProfile and Count equals 1.
        /// </summary>
        [TestMethod]
        public void GetByApplicationUserId_MultipleProfilesInDatabase_ReturnsCorrectOne()
        {
            // Arrange
            var targetId = "target-user-id";
            var userProfiles = new List<UserProfile>
            {
                new UserProfile
                {
                    UserProfileId = 1,
                    ApplicationUserId = "user-id-1",
                    FirstName = "User1",
                    LastName = "Test1",
                    Email = "user1@example.com"
                },
                new UserProfile
                {
                    UserProfileId = 2,
                    ApplicationUserId = targetId,
                    FirstName = "User2",
                    LastName = "Test2",
                    Email = "user2@example.com"
                },
                new UserProfile
                {
                    UserProfileId = 3,
                    ApplicationUserId = "user-id-3",
                    FirstName = "User3",
                    LastName = "Test3",
                    Email = "user3@example.com"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userProfiles);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = controller.GetByApplicationUserId(targetId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");

            var items = itemsProperty?.GetValue(value) as List<UserProfile>;
            var count = (int)(countProperty?.GetValue(value) ?? 0);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(targetId, items[0].ApplicationUserId);
            Assert.AreEqual(2, items[0].UserProfileId);
        }

        /// <summary>
        /// Creates a mock DbSet for UserProfile that supports LINQ queries.
        /// </summary>
        /// <param name="data">The queryable data source.</param>
        /// <returns>A mocked DbSet.</returns>
        private static Mock<DbSet<UserProfile>> CreateMockDbSet(IQueryable<UserProfile> data)
        {
            var mockSet = new Mock<DbSet<UserProfile>>();
            mockSet.As<IQueryable<UserProfile>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UserProfile>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UserProfile>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UserProfile>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Tests that ChangePassword successfully changes password when passwords match and user exists.
        /// Input: Valid payload with matching Password and ConfirmPassword.
        /// Expected: Password is changed via UserManager, profile is retrieved from database and returned in Ok result.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_MatchingPasswordsAndValidUser_ReturnsOkWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var oldPassword = "OldPass123!";
            var newPassword = "NewPass123!";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = newPassword,
                ConfirmPassword = newPassword,
                OldPassword = oldPassword,
                Email = "test@test.com"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };
            var identityResult = IdentityResult.Success;

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, oldPassword, newPassword))
                .ReturnsAsync(identityResult);

            var dbProfile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Email = "test@test.com"
            };

            var userProfiles = new[] { dbProfile }.AsQueryable();
            var mockDbSet = CreateMockDbSet(userProfiles);
            mockContext.Setup(x => x.UserProfile).Returns(mockDbSet.Object);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(UserProfile));
            var returnedProfile = (UserProfile)okResult.Value;
            Assert.AreEqual(applicationUserId, returnedProfile.ApplicationUserId);

            mockUserManager.Verify(x => x.FindByIdAsync(applicationUserId), Times.Once);
            mockUserManager.Verify(x => x.ChangePasswordAsync(applicationUser, oldPassword, newPassword), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword returns BadRequest when passwords do not match.
        /// Input: Payload with Password and ConfirmPassword that don't match.
        /// Expected: ChangePasswordAsync is not called, BadRequest is returned.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_PasswordsDoNotMatch_SkipsPasswordChangeAndReturnsProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!",
                OldPassword = "OldPass123!",
                Email = "test@test.com"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
            mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ChangePassword returns Ok with null when profile is not found in database.
        /// Input: Valid payload but profile does not exist in database.
        /// Expected: Ok result with null value is returned.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_ProfileNotFoundInDatabase_ReturnsOkWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var newPassword = "NewPass123!";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = newPassword,
                ConfirmPassword = newPassword,
                OldPassword = "OldPass123!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            var emptyUserProfiles = Enumerable.Empty<UserProfile>().AsQueryable();
            var mockDbSet = CreateMockDbSet(emptyUserProfiles);
            mockContext.Setup(x => x.UserProfile).Returns(mockDbSet.Object);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that ChangePassword returns BadRequest when password change fails.
        /// Input: Valid payload but ChangePasswordAsync returns failed result.
        /// Expected: BadRequest with error description is returned.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_PasswordChangeFails_StillReturnsProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var newPassword = "NewPass123!";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = newPassword,
                ConfirmPassword = newPassword,
                OldPassword = "WrongOldPassword!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };
            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Incorrect password" });

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, newPassword))
                .ReturnsAsync(failedResult);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("Incorrect password", badRequestResult.Value);
        }

        /// <summary>
        /// Tests that ChangePassword rejects empty string passwords.
        /// Input: Profile with empty string Password and ConfirmPassword (matching).
        /// Expected: BadRequest is returned and ChangePasswordAsync is not called.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_EmptyStringPasswords_AttemptsPasswordChange()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var emptyPassword = "";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = emptyPassword,
                ConfirmPassword = emptyPassword,
                OldPassword = "OldPass123!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("New password is required.", badRequestResult.Value);
            mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ChangePassword handles whitespace-only passwords.
        /// Input: Profile with whitespace Password and ConfirmPassword (matching).
        /// Expected: Password change is attempted with whitespace strings.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_WhitespacePasswords_AttemptsPasswordChange()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var whitespacePassword = "   ";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = whitespacePassword,
                ConfirmPassword = whitespacePassword,
                OldPassword = "OldPass123!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, whitespacePassword))
                .ReturnsAsync(IdentityResult.Success);

            var dbProfile = new UserProfile
            {
                ApplicationUserId = applicationUserId
            };

            var userProfiles = new[] { dbProfile }.AsQueryable();
            var mockDbSet = CreateMockDbSet(userProfiles);
            mockContext.Setup(x => x.UserProfile).Returns(mockDbSet.Object);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            mockUserManager.Verify(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, whitespacePassword), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword handles very long passwords correctly.
        /// Input: Profile with very long Password and ConfirmPassword strings (matching).
        /// Expected: Password change is attempted with long strings.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_VeryLongPasswords_AttemptsPasswordChange()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var longPassword = new string('a', 10000);

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = longPassword,
                ConfirmPassword = longPassword,
                OldPassword = "OldPass123!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, longPassword))
                .ReturnsAsync(IdentityResult.Success);

            var dbProfile = new UserProfile
            {
                ApplicationUserId = applicationUserId
            };

            var userProfiles = new[] { dbProfile }.AsQueryable();
            var mockDbSet = CreateMockDbSet(userProfiles);
            mockContext.Setup(x => x.UserProfile).Returns(mockDbSet.Object);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            mockUserManager.Verify(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, longPassword), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword handles special characters in passwords.
        /// Input: Profile with passwords containing special characters.
        /// Expected: Password change is attempted with special character strings.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_SpecialCharactersInPasswords_AttemptsPasswordChange()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockUserManager = MockUserManager();

            var applicationUserId = "user123";
            var specialPassword = "P@$$w0rd!#$%^&*(){}[]|\\:;\"'<>,.?/~`";

            var profile = new UserProfile
            {
                ApplicationUserId = applicationUserId,
                Password = specialPassword,
                ConfirmPassword = specialPassword,
                OldPassword = "OldPass123!"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            var applicationUser = new ApplicationUser { Id = applicationUserId };

            mockUserManager.Setup(x => x.FindByIdAsync(applicationUserId))
                .ReturnsAsync(applicationUser);
            mockUserManager.Setup(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, specialPassword))
                .ReturnsAsync(IdentityResult.Success);

            var dbProfile = new UserProfile
            {
                ApplicationUserId = applicationUserId
            };

            var userProfiles = new[] { dbProfile }.AsQueryable();
            var mockDbSet = CreateMockDbSet(userProfiles);
            mockContext.Setup(x => x.UserProfile).Returns(mockDbSet.Object);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.ChangePassword(payload);

            // Assert
            Assert.IsNotNull(result);
            mockUserManager.Verify(x => x.ChangePasswordAsync(applicationUser, profile.OldPassword, specialPassword), Times.Once);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            return mockUserManager;
        }

        private static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Tests that ChangeRole returns OkObjectResult with the profile when given a valid payload.
        /// Input: Valid CrudViewModel with a valid UserProfile.
        /// Expected: Returns OkObjectResult containing the same UserProfile.
        /// </summary>
        [TestMethod]
        public void ChangeRole_ValidPayload_ReturnsOkWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                ApplicationUserId = "user123"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                action = "changeRole",
                value = userProfile
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(userProfile, okResult.Value);
        }

        /// <summary>
        /// Tests that ChangeRole returns OkObjectResult with null when payload.value is null.
        /// Input: CrudViewModel with null value property.
        /// Expected: Returns OkObjectResult containing null.
        /// </summary>
        [TestMethod]
        public void ChangeRole_PayloadWithNullValue_ReturnsOkWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var payload = new CrudViewModel<UserProfile>
            {
                action = "changeRole",
                value = null!
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that ChangeRole returns the exact profile object passed in the payload.
        /// Input: Valid payload with UserProfile containing various property values.
        /// Expected: Returns OkObjectResult with the same UserProfile instance.
        /// </summary>
        [TestMethod]
        public void ChangeRole_ValidPayloadWithAllProperties_ReturnsSameProfileInstance()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                UserProfileId = 999,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                Password = "SecurePassword123!",
                ConfirmPassword = "SecurePassword123!",
                OldPassword = "OldPassword456",
                ProfilePicture = "/upload/custom.jpg",
                ApplicationUserId = "app-user-456"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                action = "update",
                key = 999,
                antiForgery = "token123",
                value = userProfile
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(userProfile, okResult.Value);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual(999, returnedProfile.UserProfileId);
            Assert.AreEqual("Jane", returnedProfile.FirstName);
            Assert.AreEqual("Smith", returnedProfile.LastName);
            Assert.AreEqual("jane.smith@test.com", returnedProfile.Email);
            Assert.AreEqual("app-user-456", returnedProfile.ApplicationUserId);
        }

        /// <summary>
        /// Tests that ChangeRole handles UserProfile with empty strings correctly.
        /// Input: Valid payload with UserProfile containing empty string properties.
        /// Expected: Returns OkObjectResult with the UserProfile preserving empty strings.
        /// </summary>
        [TestMethod]
        public void ChangeRole_UserProfileWithEmptyStrings_ReturnsOkWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                UserProfileId = 0,
                FirstName = "",
                LastName = "",
                Email = "",
                Password = "",
                ConfirmPassword = "",
                ApplicationUserId = ""
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(userProfile, okResult.Value);
        }

        /// <summary>
        /// Tests that ChangeRole handles UserProfile with special characters in string properties.
        /// Input: Valid payload with UserProfile containing special characters, unicode, and long strings.
        /// Expected: Returns OkObjectResult with the UserProfile preserving all special characters.
        /// </summary>
        [TestMethod]
        public void ChangeRole_UserProfileWithSpecialCharacters_ReturnsOkWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                UserProfileId = int.MaxValue,
                FirstName = "John<script>alert('xss')</script>",
                LastName = "O'Brien-Smith & Associates",
                Email = "test@例え.jp",
                Password = "P@ssw0rd!#$%^&*()",
                ApplicationUserId = "user_123-456/789"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual(int.MaxValue, returnedProfile.UserProfileId);
            Assert.AreEqual("John<script>alert('xss')</script>", returnedProfile.FirstName);
        }

        /// <summary>
        /// Tests that ChangeRole handles UserProfile with boundary value for UserProfileId.
        /// Input: Valid payload with UserProfile having UserProfileId set to int.MinValue.
        /// Expected: Returns OkObjectResult with the UserProfile preserving the boundary value.
        /// </summary>
        [TestMethod]
        public void ChangeRole_UserProfileWithMinIntId_ReturnsOkWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
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

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var userProfile = new UserProfile
            {
                UserProfileId = int.MinValue,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                ApplicationUserId = "testUserId"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = userProfile
            };

            // Act
            var result = controller.ChangeRole(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedProfile = okResult.Value as UserProfile;
            Assert.IsNotNull(returnedProfile);
            Assert.AreEqual(int.MinValue, returnedProfile.UserProfileId);
        }

        /// <summary>
        /// Tests that Remove method handles null key by converting it to 0 and searching for UserProfileId == 0.
        /// If no UserProfile with id 0 exists, method returns Ok without error.
        /// </summary>
        [TestMethod]
        public async Task Remove_NullKey_ReturnsOk()
        {
            // Arrange
            var userProfiles = new List<UserProfile>().AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var payload = new CrudViewModel<UserProfile> { key = null };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Remove method returns Ok when UserProfile is not found.
        /// Expected behavior: Method completes successfully without performing any deletion.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(999)]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public async Task Remove_UserProfileNotFound_ReturnsOk(int userProfileId)
        {
            // Arrange
            var userProfiles = new List<UserProfile>().AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var payload = new CrudViewModel<UserProfile> { key = userProfileId };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            mockUserManager.Verify(um => um.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        /// <summary>
        /// Tests that Remove method handles case when UserProfile exists but ApplicationUser is not found.
        /// Expected behavior: Calls DeleteAsync with null user, method should handle gracefully or throw.
        /// </summary>
        [TestMethod]
        public async Task Remove_UserProfileExistsButApplicationUserNotFound_CallsDeleteAsyncWithNull()
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = "user123",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var applicationUsers = new List<ApplicationUser>().AsQueryable();
            var mockApplicationUserSet = CreateMockDbSet(applicationUsers);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);
            mockContext.Setup(c => c.Users).Returns(mockApplicationUserSet.Object);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.DeleteAsync(null!))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User cannot be null" }));

            var controller = new UserController(mockContext.Object, mockUserManager.Object);
            var payload = new CrudViewModel<UserProfile> { key = 1 };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            mockUserManager.Verify(um => um.DeleteAsync(null!), Times.Once);
            mockContext.Verify(c => c.Remove(It.IsAny<UserProfile>()), Times.Never);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that Remove method successfully deletes UserProfile when both UserProfile and ApplicationUser exist
        /// and DeleteAsync succeeds.
        /// Expected behavior: UserProfile is removed and SaveChangesAsync is called.
        /// </summary>
        [TestMethod]
        public async Task Remove_ValidUserProfileAndUser_DeleteSucceeds_RemovesUserProfileAndSavesChanges()
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = 5,
                ApplicationUserId = "user123",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var applicationUser = new ApplicationUser { Id = "user123", UserName = "jane@example.com" };
            var applicationUsers = new List<ApplicationUser> { applicationUser }.AsQueryable();
            var mockApplicationUserSet = CreateMockDbSet(applicationUsers);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);
            mockContext.Setup(c => c.Users).Returns(mockApplicationUserSet.Object);
            mockContext.Setup(c => c.Remove(It.IsAny<UserProfile>()));
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);
            var payload = new CrudViewModel<UserProfile> { key = 5 };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            mockUserManager.Verify(um => um.DeleteAsync(applicationUser), Times.Once);
            mockContext.Verify(c => c.Remove(userProfile), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Remove method does not remove UserProfile when DeleteAsync fails.
        /// Expected behavior: UserProfile is not removed and SaveChangesAsync is not called, but method returns Ok.
        /// </summary>
        [TestMethod]
        public async Task Remove_DeleteAsyncFails_DoesNotRemoveUserProfile()
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = 10,
                ApplicationUserId = "user456",
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob@example.com"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var applicationUser = new ApplicationUser { Id = "user456", UserName = "bob@example.com" };
            var applicationUsers = new List<ApplicationUser> { applicationUser }.AsQueryable();
            var mockApplicationUserSet = CreateMockDbSet(applicationUsers);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);
            mockContext.Setup(c => c.Users).Returns(mockApplicationUserSet.Object);

            var mockUserManager = CreateMockUserManager();
            var failureResult = IdentityResult.Failed(new IdentityError { Description = "Cannot delete user" });
            mockUserManager.Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(failureResult);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);
            var payload = new CrudViewModel<UserProfile> { key = 10 };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            mockUserManager.Verify(um => um.DeleteAsync(applicationUser), Times.Once);
            mockContext.Verify(c => c.Remove(It.IsAny<UserProfile>()), Times.Never);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that Remove method handles integer key values correctly, including boundary values.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(999999)]
        public async Task Remove_ValidIntegerKey_HandlesCorrectly(int keyValue)
        {
            // Arrange
            var userProfiles = new List<UserProfile>().AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);

            var mockUserManager = CreateMockUserManager();
            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            var payload = new CrudViewModel<UserProfile> { key = keyValue };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        /// <summary>
        /// Tests that Remove method correctly converts string key to integer.
        /// </summary>
        [TestMethod]
        [DataRow("1", 1)]
        [DataRow("42", 42)]
        [DataRow("0", 0)]
        [DataRow("-1", -1)]
        public async Task Remove_StringKeyConvertibleToInt_ConvertsAndSearchesCorrectly(string keyString, int expectedId)
        {
            // Arrange
            var userProfile = new UserProfile
            {
                UserProfileId = expectedId,
                ApplicationUserId = "user789",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();
            var mockUserProfileSet = CreateMockDbSet(userProfiles);

            var applicationUser = new ApplicationUser { Id = "user789", UserName = "test@example.com" };
            var applicationUsers = new List<ApplicationUser> { applicationUser }.AsQueryable();
            var mockApplicationUserSet = CreateMockDbSet(applicationUsers);

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileSet.Object);
            mockContext.Setup(c => c.Users).Returns(mockApplicationUserSet.Object);
            mockContext.Setup(c => c.Remove(It.IsAny<UserProfile>()));
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(um => um.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);
            var payload = new CrudViewModel<UserProfile> { key = keyString };

            // Act
            var result = await controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            mockUserManager.Verify(um => um.DeleteAsync(applicationUser), Times.Once);
        }

        /// <summary>
        /// Tests that Update method successfully updates a user profile and returns OkObjectResult.
        /// Input: Valid payload with a populated UserProfile.
        /// Expected: Update is called, SaveChangesAsync is called, and Ok result is returned with the profile.
        /// </summary>
        [TestMethod]
        public async Task Update_ValidPayload_ReturnsOkResultWithProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var profile = new UserProfile
            {
                UserProfileId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                ApplicationUserId = "user123"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreSame(profile, okResult.Value);
            mockUserProfileDbSet.Verify(db => db.Update(profile), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null payload.value appropriately.
        /// Input: Payload with null value property.
        /// Expected: Update is called with null, SaveChangesAsync is called, and Ok result is returned with null.
        /// </summary>
        [TestMethod]
        public async Task Update_NullPayloadValue_ReturnsOkResultWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var payload = new CrudViewModel<UserProfile>
            {
                value = null!
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockUserProfileDbSet.Verify(db => db.Update(null!), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles UserProfile with minimum valid data.
        /// Input: UserProfile with only required fields populated.
        /// Expected: Update succeeds and returns Ok result.
        /// </summary>
        [TestMethod]
        public async Task Update_MinimalUserProfile_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var profile = new UserProfile
            {
                UserProfileId = 0
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(profile, okResult.Value);
            mockUserProfileDbSet.Verify(db => db.Update(profile), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles UserProfile with all fields populated.
        /// Input: UserProfile with all properties set.
        /// Expected: Update succeeds and returns Ok result with complete profile.
        /// </summary>
        [TestMethod]
        public async Task Update_CompleteUserProfile_ReturnsOkResultWithCompleteProfile()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var profile = new UserProfile
            {
                UserProfileId = 123,
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                OldPassword = "OldPass456!",
                ProfilePicture = "/upload/profile.jpg",
                ApplicationUserId = "app-user-456"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(profile, okResult.Value);
            Assert.AreEqual(123, ((UserProfile)okResult.Value!).UserProfileId);
            Assert.AreEqual("Alice", ((UserProfile)okResult.Value!).FirstName);
            mockUserProfileDbSet.Verify(db => db.Update(profile), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles UserProfile with special characters in string fields.
        /// Input: UserProfile with special characters, unicode, and edge-case string values.
        /// Expected: Update succeeds and returns Ok result.
        /// </summary>
        [TestMethod]
        public async Task Update_UserProfileWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var profile = new UserProfile
            {
                UserProfileId = 99,
                FirstName = "José",
                LastName = "O'Brien-Smith",
                Email = "user+test@domain.co.uk",
                ApplicationUserId = "id_with-special.chars@123"
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockUserProfileDbSet.Verify(db => db.Update(profile), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles UserProfile with empty strings.
        /// Input: UserProfile with empty string values.
        /// Expected: Update succeeds and returns Ok result.
        /// </summary>
        [TestMethod]
        public async Task Update_UserProfileWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockUserManager = CreateMockUserManager();
            var mockUserProfileDbSet = new Mock<DbSet<UserProfile>>();

            var profile = new UserProfile
            {
                UserProfileId = 50,
                FirstName = "",
                LastName = "",
                Email = "",
                Password = "",
                ApplicationUserId = ""
            };

            var payload = new CrudViewModel<UserProfile>
            {
                value = profile
            };

            mockContext.Setup(c => c.UserProfile).Returns(mockUserProfileDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new UserController(mockContext.Object, mockUserManager.Object);

            // Act
            var result = await controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockUserProfileDbSet.Verify(db => db.Update(profile), Times.Once);
        }

    }
}