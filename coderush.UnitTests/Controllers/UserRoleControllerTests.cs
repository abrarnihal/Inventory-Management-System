using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using coderush.Controllers;
using coderush.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="UserRoleController"/> class.
    /// </summary>
    [TestClass]
    public class UserRoleControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            Mock<IUserStore<ApplicationUser>> userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            Mock<UserManager<ApplicationUser>> userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            UserRoleController controller = new UserRoleController(userManagerMock.Object);

            // Act
            IActionResult result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that ChangePassword method returns a ViewResult.
        /// Verifies that the method executes successfully and returns the expected view result type.
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            Mock<UserManager<ApplicationUser>> mockUserManager = CreateMockUserManager();
            UserRoleController controller = new UserRoleController(mockUserManager.Object);

            // Act
            IActionResult result = controller.ChangePassword();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that ChangePassword method returns a ViewResult with default view name.
        /// Verifies that the returned ViewResult uses convention-based view discovery (null view name).
        /// </summary>
        [TestMethod]
        public void ChangePassword_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            Mock<UserManager<ApplicationUser>> mockUserManager = CreateMockUserManager();
            UserRoleController controller = new UserRoleController(mockUserManager.Object);

            // Act
            IActionResult result = controller.ChangePassword();

            // Assert
            ViewResult? viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNull(viewResult.ViewName);
        }

        /// <summary>
        /// Creates a mock UserManager instance for testing purposes.
        /// UserManager has complex dependencies that need to be mocked.
        /// </summary>
        /// <returns>A mock UserManager instance.</returns>
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            Mock<IUserStore<ApplicationUser>> mockStore = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                mockStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        /// <summary>
        /// Tests that the Role method returns a ViewResult.
        /// </summary>
        [TestMethod]
        public void Role_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
            var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var errorsMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            var userManager = new UserManager<ApplicationUser>(
                userStoreMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                lookupNormalizerMock.Object,
                errorsMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object);

            var controller = new UserRoleController(userManager);

            // Act
            var result = controller.Role();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Role method returns a ViewResult with null ViewName (default view).
        /// </summary>
        [TestMethod]
        public void Role_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
            var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var errorsMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            var userManager = new UserManager<ApplicationUser>(
                userStoreMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                lookupNormalizerMock.Object,
                errorsMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object);

            var controller = new UserRoleController(userManager);

            // Act
            var result = controller.Role() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }

        /// <summary>
        /// Tests that UserProfile returns a ViewResult with the user model when the user exists.
        /// Input: Valid ClaimsPrincipal, GetUserAsync returns a valid ApplicationUser.
        /// Expected: ViewResult containing the ApplicationUser as the model.
        /// </summary>
        [TestMethod]
        public async Task UserProfile_WhenUserExists_ReturnsViewResultWithUser()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            var expectedUser = new ApplicationUser { Id = "user123", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            }));

            userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expectedUser);

            var controller = new UserRoleController(userManagerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.UserProfile();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ApplicationUser));
            var actualUser = (ApplicationUser)viewResult.Model;
            Assert.AreEqual(expectedUser.Id, actualUser.Id);
            Assert.AreEqual(expectedUser.UserName, actualUser.UserName);
        }

        /// <summary>
        /// Tests that UserProfile returns a ViewResult with null model when the user is not found.
        /// Input: Valid ClaimsPrincipal, GetUserAsync returns null.
        /// Expected: ViewResult with null model.
        /// </summary>
        [TestMethod]
        public async Task UserProfile_WhenUserNotFound_ReturnsViewResultWithNullModel()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "nonexistent")
            }));

            userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            var controller = new UserRoleController(userManagerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.UserProfile();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNull(viewResult.Model);
        }

        /// <summary>
        /// Tests that UserProfile calls GetUserAsync with the correct ClaimsPrincipal.
        /// Input: Specific ClaimsPrincipal set on controller context.
        /// Expected: GetUserAsync is called with the same ClaimsPrincipal instance.
        /// </summary>
        [TestMethod]
        public async Task UserProfile_CallsGetUserAsyncWithControllerUser()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user456")
            }));

            var expectedUser = new ApplicationUser { Id = "user456" };
            ClaimsPrincipal capturedPrincipal = null;

            userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .Callback<ClaimsPrincipal>(p => capturedPrincipal = p)
                .ReturnsAsync(expectedUser);

            var controller = new UserRoleController(userManagerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            await controller.UserProfile();

            // Assert
            userManagerMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            Assert.AreSame(claimsPrincipal, capturedPrincipal);
        }

        /// <summary>
        /// Tests that the ChangeRole method returns a ViewResult when called.
        /// This verifies the basic functionality of the controller action.
        /// </summary>
        [TestMethod]
        public void ChangeRole_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var mockUserValidators = new List<IUserValidator<ApplicationUser>>();
            var mockPasswordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var mockLookupNormalizer = new Mock<ILookupNormalizer>();
            var mockErrorDescriber = new Mock<IdentityErrorDescriber>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            mockOptions.Setup(o => o.Value).Returns(new IdentityOptions());

            var userManager = new UserManager<ApplicationUser>(
                mockUserStore.Object,
                mockOptions.Object,
                mockPasswordHasher.Object,
                mockUserValidators,
                mockPasswordValidators,
                mockLookupNormalizer.Object,
                mockErrorDescriber.Object,
                mockServiceProvider.Object,
                mockLogger.Object);

            var controller = new UserRoleController(userManager);

            // Act
            var result = controller.ChangeRole();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the ChangeRole method returns a ViewResult with a null or empty ViewName,
        /// indicating that the default view (based on the action name) should be rendered.
        /// </summary>
        [TestMethod]
        public void ChangeRole_WhenCalled_ReturnsDefaultView()
        {
            // Arrange
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var mockUserValidators = new List<IUserValidator<ApplicationUser>>();
            var mockPasswordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var mockLookupNormalizer = new Mock<ILookupNormalizer>();
            var mockErrorDescriber = new Mock<IdentityErrorDescriber>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            mockOptions.Setup(o => o.Value).Returns(new IdentityOptions());

            var userManager = new UserManager<ApplicationUser>(
                mockUserStore.Object,
                mockOptions.Object,
                mockPasswordHasher.Object,
                mockUserValidators,
                mockPasswordValidators,
                mockLookupNormalizer.Object,
                mockErrorDescriber.Object,
                mockServiceProvider.Object,
                mockLogger.Object);

            var controller = new UserRoleController(userManager);

            // Act
            var result = controller.ChangeRole() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName));
        }
    }
}