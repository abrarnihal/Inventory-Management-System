using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using coderush.Controllers;
using coderush.Models;
using coderush.Models.ManageViewModels;
using coderush.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the ManageController class.
    /// </summary>
    [TestClass]
    public class ManageControllerTests
    {
        /// <summary>
        /// Tests that Index POST returns View with model when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task Index_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Username = "testuser"
            };

            var controller = CreateController();
            controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await controller.Index(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
        }

        /// <summary>
        /// Tests that Index POST redirects when email and phone unchanged.
        /// </summary>
        [TestMethod]
        public async Task Index_NoChanges_RedirectsToIndex()
        {
            // Arrange
            var existingEmail = "test@example.com";
            var existingPhone = "1234567890";

            var model = new IndexViewModel
            {
                Email = existingEmail,
                PhoneNumber = existingPhone,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = existingEmail,
                PhoneNumber = existingPhone,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.Index), redirectResult.ActionName);
            Assert.AreEqual("Your profile has been updated", controller.StatusMessage);
        }

        /// <summary>
        /// Tests that Index POST updates email successfully when email changed.
        /// </summary>
        [TestMethod]
        public async Task Index_EmailChanged_UpdatesEmailAndRedirects()
        {
            // Arrange
            var oldEmail = "old@example.com";
            var newEmail = "new@example.com";

            var model = new IndexViewModel
            {
                Email = newEmail,
                PhoneNumber = "1234567890",
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = oldEmail,
                PhoneNumber = "1234567890",
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetEmailAsync(user, newEmail))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetEmailAsync(user, newEmail), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.Index), redirectResult.ActionName);
            Assert.AreEqual("Your profile has been updated", controller.StatusMessage);
        }

        /// <summary>
        /// Tests that Index POST updates phone number successfully when phone changed.
        /// </summary>
        [TestMethod]
        public async Task Index_PhoneNumberChanged_UpdatesPhoneAndRedirects()
        {
            // Arrange
            var oldPhone = "1234567890";
            var newPhone = "0987654321";

            var model = new IndexViewModel
            {
                Email = "test@example.com",
                PhoneNumber = newPhone,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = "test@example.com",
                PhoneNumber = oldPhone,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetPhoneNumberAsync(user, newPhone))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetPhoneNumberAsync(user, newPhone), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.Index), redirectResult.ActionName);
            Assert.AreEqual("Your profile has been updated", controller.StatusMessage);
        }

        /// <summary>
        /// Tests that Index POST updates both email and phone successfully when both changed.
        /// </summary>
        [TestMethod]
        public async Task Index_BothEmailAndPhoneChanged_UpdatesBothAndRedirects()
        {
            // Arrange
            var oldEmail = "old@example.com";
            var newEmail = "new@example.com";
            var oldPhone = "1234567890";
            var newPhone = "0987654321";

            var model = new IndexViewModel
            {
                Email = newEmail,
                PhoneNumber = newPhone,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = oldEmail,
                PhoneNumber = oldPhone,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetEmailAsync(user, newEmail))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.SetPhoneNumberAsync(user, newPhone))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetEmailAsync(user, newEmail), Times.Once);
            userManagerMock.Verify(x => x.SetPhoneNumberAsync(user, newPhone), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.Index), redirectResult.ActionName);
            Assert.AreEqual("Your profile has been updated", controller.StatusMessage);
        }

        /// <summary>
        /// Tests that Index POST handles empty email string.
        /// </summary>
        [TestMethod]
        public async Task Index_EmptyEmail_UpdatesEmailAndRedirects()
        {
            // Arrange
            var oldEmail = "test@example.com";
            var newEmail = "";

            var model = new IndexViewModel
            {
                Email = newEmail,
                PhoneNumber = "1234567890",
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = oldEmail,
                PhoneNumber = "1234567890",
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetEmailAsync(user, newEmail))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetEmailAsync(user, newEmail), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        /// <summary>
        /// Tests that Index POST handles null phone number.
        /// </summary>
        [TestMethod]
        public async Task Index_NullPhoneNumber_UpdatesPhoneAndRedirects()
        {
            // Arrange
            var oldPhone = "1234567890";

            var model = new IndexViewModel
            {
                Email = "test@example.com",
                PhoneNumber = null,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = "test@example.com",
                PhoneNumber = oldPhone,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetPhoneNumberAsync(user, null))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetPhoneNumberAsync(user, null), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        /// <summary>
        /// Tests that Index POST handles user with null email initially.
        /// </summary>
        [TestMethod]
        public async Task Index_UserEmailIsNull_UpdatesEmailAndRedirects()
        {
            // Arrange
            var newEmail = "new@example.com";

            var model = new IndexViewModel
            {
                Email = newEmail,
                PhoneNumber = "1234567890",
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = null,
                PhoneNumber = "1234567890",
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetEmailAsync(user, newEmail))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetEmailAsync(user, newEmail), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        /// <summary>
        /// Tests that Index POST handles user with null phone number initially.
        /// </summary>
        [TestMethod]
        public async Task Index_UserPhoneIsNull_UpdatesPhoneAndRedirects()
        {
            // Arrange
            var newPhone = "1234567890";

            var model = new IndexViewModel
            {
                Email = "test@example.com",
                PhoneNumber = newPhone,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = "test@example.com",
                PhoneNumber = null,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetPhoneNumberAsync(user, newPhone))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetPhoneNumberAsync(user, newPhone), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        /// <summary>
        /// Tests that Index POST handles very long email string.
        /// </summary>
        [TestMethod]
        public async Task Index_VeryLongEmail_UpdatesEmailAndRedirects()
        {
            // Arrange
            var oldEmail = "test@example.com";
            var newEmail = new string('a', 500) + "@example.com";

            var model = new IndexViewModel
            {
                Email = newEmail,
                PhoneNumber = "1234567890",
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = oldEmail,
                PhoneNumber = "1234567890",
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetEmailAsync(user, newEmail))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetEmailAsync(user, newEmail), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        /// <summary>
        /// Tests that Index POST handles phone number with special characters.
        /// </summary>
        [TestMethod]
        public async Task Index_PhoneWithSpecialCharacters_UpdatesPhoneAndRedirects()
        {
            // Arrange
            var oldPhone = "1234567890";
            var newPhone = "+1 (555) 123-4567";

            var model = new IndexViewModel
            {
                Email = "test@example.com",
                PhoneNumber = newPhone,
                Username = "testuser"
            };

            var user = new ApplicationUser
            {
                Email = "test@example.com",
                PhoneNumber = oldPhone,
                UserName = "testuser"
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.SetPhoneNumberAsync(user, newPhone))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock);

            // Act
            var result = await controller.Index(model);

            // Assert
            userManagerMock.Verify(x => x.SetPhoneNumberAsync(user, newPhone), Times.Once);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var optionsAccessor = Microsoft.Extensions.Options.Options.Create(new IdentityOptions());
            var mock = new Mock<UserManager<ApplicationUser>>(
                store.Object, optionsAccessor, null, null, null, null, null, null, null);
            return mock;
        }

        private static ManageController CreateController(Mock<UserManager<ApplicationUser>>? userManagerMock = null)
        {
            var wasNull = userManagerMock == null;
            userManagerMock ??= CreateUserManagerMock();
            if (wasNull)
            {
                userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(new ApplicationUser { Email = "test@example.com", PhoneNumber = "1234567890" });
            }

            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user123")
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return controller;
        }

        /// <summary>
        /// Tests that EnableAuthenticator returns a ViewResult with EnableAuthenticatorViewModel 
        /// when the user is successfully retrieved.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_UserExists_ReturnsViewResultWithModel()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            mockUserManager.Setup(x => x.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user)).ReturnsAsync("TESTKEY12345");

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.EnableAuthenticator();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(EnableAuthenticatorViewModel));
        }

        /// <summary>
        /// Tests that EnableAuthenticator correctly handles when authenticator key is null 
        /// and needs to be reset.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_AuthenticatorKeyIsNull_ResetsKeyAndReturnsView()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            mockUserManager.Setup(x => x.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
            mockUserManager.SetupSequence(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync((string?)null)
                .ReturnsAsync("NEWKEY12345");
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.EnableAuthenticator();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator correctly handles when authenticator key is empty 
        /// and needs to be reset.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_AuthenticatorKeyIsEmpty_ResetsKeyAndReturnsView()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            mockUserManager.Setup(x => x.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
            mockUserManager.SetupSequence(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync(string.Empty)
                .ReturnsAsync("NEWKEY67890");
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.EnableAuthenticator();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator populates the model with SharedKey and AuthenticatorUri.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_ValidUser_PopulatesModelWithSharedKeyAndUri()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            var testKey = "TESTKEY12345";
            mockUserManager.Setup(x => x.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user)).ReturnsAsync(testKey);
            mockUrlEncoder.Setup(x => x.Encode(It.IsAny<string>())).Returns<string>(s => s);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.EnableAuthenticator();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (EnableAuthenticatorViewModel)viewResult.Model;
            Assert.IsNotNull(model.SharedKey);
            Assert.IsNotNull(model.AuthenticatorUri);
        }

        /// <summary>
        /// Creates a mock UserManager for testing purposes.
        /// </summary>
        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            return mockUserManager;
        }

        /// <summary>
        /// Creates a mock SignInManager for testing purposes.
        /// </summary>
        private static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
            return mockSignInManager;
        }

        /// <summary>
        /// Tests that ChangePassword redirects to SetPassword when user has no password.
        /// Input: Valid user with HasPasswordAsync returning false
        /// Expected: RedirectToActionResult to SetPassword action
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_UserHasNoPassword_RedirectsToSetPassword()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(um => um.HasPasswordAsync(user))
                .ReturnsAsync(false);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.ChangePassword();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("SetPassword", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ChangePassword returns view with model when user has password and StatusMessage is null.
        /// Input: Valid user with HasPasswordAsync returning true, StatusMessage is null
        /// Expected: ViewResult with ChangePasswordViewModel containing null StatusMessage
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_UserHasPasswordWithNullStatusMessage_ReturnsViewWithModel()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(um => um.HasPasswordAsync(user))
                .ReturnsAsync(true);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = null
            };

            // Act
            var result = await controller.ChangePassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ChangePasswordViewModel;
            Assert.IsNotNull(model);
            Assert.IsNull(model.StatusMessage);
        }

        /// <summary>
        /// Tests that ChangePassword returns view with model when user has password and StatusMessage has value.
        /// Input: Valid user with HasPasswordAsync returning true, StatusMessage has specific value
        /// Expected: ViewResult with ChangePasswordViewModel containing the StatusMessage
        /// </summary>
        [TestMethod]
        [DataRow("Password changed successfully")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("Very long status message that contains many characters to test boundary conditions and ensure that the view model properly handles long strings without any issues")]
        public async Task ChangePassword_UserHasPasswordWithStatusMessage_ReturnsViewWithModelContainingStatusMessage(string statusMessage)
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(um => um.HasPasswordAsync(user))
                .ReturnsAsync(true);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = statusMessage
            };

            // Act
            var result = await controller.ChangePassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ChangePasswordViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(statusMessage, model.StatusMessage);
        }

        /// <summary>
        /// Tests that ChangePassword verifies GetUserAsync is called with correct ClaimsPrincipal.
        /// Input: Valid setup with ClaimsPrincipal
        /// Expected: GetUserAsync is called exactly once with the User property
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_ValidUser_CallsGetUserAsyncWithCorrectPrincipal()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(um => um.HasPasswordAsync(user))
                .ReturnsAsync(true);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            await controller.ChangePassword();

            // Assert
            userManagerMock.Verify(um => um.GetUserAsync(claimsPrincipal), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword verifies HasPasswordAsync is called with correct user.
        /// Input: Valid user setup
        /// Expected: HasPasswordAsync is called exactly once with the retrieved user
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_ValidUser_CallsHasPasswordAsyncWithCorrectUser()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(um => um.HasPasswordAsync(user))
                .ReturnsAsync(true);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            await controller.ChangePassword();

            // Assert
            userManagerMock.Verify(um => um.HasPasswordAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator returns ViewResult with model when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("test-key");

            var model = new EnableAuthenticatorViewModel { Code = "123456" };
            controller.ModelState.AddModelError("Code", "Required");

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
            userManagerMock.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator strips spaces from verification code.
        /// </summary>
        [TestMethod]
        [DataRow("123 456", "123456")]
        [DataRow("12 34 56", "123456")]
        [DataRow("   123456   ", "123456")]
        public async Task EnableAuthenticator_CodeWithSpaces_StripsSpaces(string inputCode, string expectedCode)
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            string? capturedCode = null;
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string, string>((u, provider, code) => capturedCode = code)
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = inputCode };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            Assert.AreEqual(expectedCode, capturedCode);
        }

        /// <summary>
        /// Tests that EnableAuthenticator strips hyphens from verification code.
        /// </summary>
        [TestMethod]
        [DataRow("123-456", "123456")]
        [DataRow("12-34-56", "123456")]
        [DataRow("---123456---", "123456")]
        public async Task EnableAuthenticator_CodeWithHyphens_StripsHyphens(string inputCode, string expectedCode)
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            string? capturedCode = null;
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string, string>((u, provider, code) => capturedCode = code)
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = inputCode };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            Assert.AreEqual(expectedCode, capturedCode);
        }

        /// <summary>
        /// Tests that EnableAuthenticator strips both spaces and hyphens from verification code.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_CodeWithSpacesAndHyphens_StripsBoth()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            string? capturedCode = null;
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string, string>((u, provider, code) => capturedCode = code)
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = "12 3-45 6" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            Assert.AreEqual("123456", capturedCode);
        }

        /// <summary>
        /// Tests that EnableAuthenticator returns ViewResult with ModelState error when token verification fails.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_InvalidToken_ReturnsViewWithModelStateError()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("test-key");

            var model = new EnableAuthenticatorViewModel { Code = "123456" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsTrue(controller.ModelState.ContainsKey("Code"));
            Assert.AreEqual("Verification code is invalid.", controller.ModelState["Code"].Errors[0].ErrorMessage);
            userManagerMock.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator successfully enables 2FA, generates recovery codes, and redirects when token is valid.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_ValidToken_EnablesTwoFactorAndRedirects()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var recoveryCodes = new List<string> { "code1", "code2", "code3", "code4", "code5", "code6", "code7", "code8", "code9", "code10" };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(recoveryCodes);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = "123456" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.ShowRecoveryCodes), redirectResult.ActionName);

            userManagerMock.Verify(x => x.SetTwoFactorEnabledAsync(user, true), Times.Once);
            userManagerMock.Verify(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10), Times.Once);

            var storedCodes = controller.TempData["RecoveryCodesKey"] as string[];
            Assert.IsNotNull(storedCodes);
            Assert.AreEqual(10, storedCodes.Length);
            CollectionAssert.AreEqual(recoveryCodes.ToArray(), storedCodes);
        }

        /// <summary>
        /// Tests that EnableAuthenticator logs information with user ID when successfully enabling 2FA.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_ValidToken_LogsInformation()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id-123", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = "123456" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("test-user-id-123") && v.ToString().Contains("enabled 2FA")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that EnableAuthenticator handles empty verification code.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_EmptyCode_VerifiesWithEmptyString()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            string? capturedCode = null;
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string, string>((u, provider, code) => capturedCode = code)
                .ReturnsAsync(false);
            userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("test-key");

            var tokenOptions = new Mock<IOptions<IdentityOptions>>();
            var identityOptions = new IdentityOptions();
            tokenOptions.Setup(x => x.Value).Returns(identityOptions);
            userManagerMock.Object.Options = identityOptions;

            var model = new EnableAuthenticatorViewModel { Code = "" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            Assert.AreEqual("", capturedCode);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        /// <summary>
        /// Tests that EnableAuthenticator handles whitespace-only verification code.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_WhitespaceOnlyCode_VerifiesWithEmptyString()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            string? capturedCode = null;
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string, string>((u, provider, code) => capturedCode = code)
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            var tokenOptions = new Mock<IOptions<IdentityOptions>>();
            var identityOptions = new IdentityOptions();
            tokenOptions.Setup(x => x.Value).Returns(identityOptions);
            userManagerMock.Object.Options = identityOptions;

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = "     " };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            Assert.AreEqual("", capturedCode);
        }

        /// <summary>
        /// Tests that EnableAuthenticator correctly requests exactly 10 recovery codes.
        /// </summary>
        [TestMethod]
        public async Task EnableAuthenticator_ValidToken_RequestsTenRecoveryCodes()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(new List<string> { "code1", "code2" });

            var tokenOptions = new Mock<IOptions<IdentityOptions>>();
            var identityOptions = new IdentityOptions();
            tokenOptions.Setup(x => x.Value).Returns(identityOptions);
            userManagerMock.Object.Options = identityOptions;

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new EnableAuthenticatorViewModel { Code = "123456" };

            // Act
            var result = await controller.EnableAuthenticator(model);

            // Assert
            userManagerMock.Verify(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10), Times.Once);
        }

        private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                userManager,
                contextAccessor.Object,
                claimsFactory.Object,
                null,
                null,
                null,
                null);
            return mockSignInManager;
        }


        /// <summary>
        /// Tests that Index returns a ViewResult with correctly populated IndexViewModel when user exists.
        /// Input: Valid authenticated user with all properties set.
        /// Expected: ViewResult with model containing user properties and StatusMessage.
        /// </summary>
        [TestMethod]
        public async Task Index_ValidUser_ReturnsViewResultWithCorrectModel()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                PhoneNumber = "123-456-7890",
                EmailConfirmed = true
            };

            var claimsPrincipal = new ClaimsPrincipal();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                StatusMessage = "Test status message",
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNotNull(viewResult.Model);
            var model = viewResult.Model as IndexViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("testuser", model.Username);
            Assert.AreEqual("test@example.com", model.Email);
            Assert.AreEqual("123-456-7890", model.PhoneNumber);
            Assert.IsTrue(model.IsEmailConfirmed);
            Assert.AreEqual("Test status message", model.StatusMessage);
        }

        /// <summary>
        /// Tests that Index handles user with null properties correctly.
        /// Input: User exists but has null UserName, Email, and PhoneNumber.
        /// Expected: ViewResult with model containing null properties.
        /// </summary>
        [TestMethod]
        public async Task Index_UserWithNullProperties_ReturnsViewResultWithNullProperties()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var testUser = new ApplicationUser
            {
                UserName = null,
                Email = null,
                PhoneNumber = null,
                EmailConfirmed = false
            };

            var claimsPrincipal = new ClaimsPrincipal();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as IndexViewModel;
            Assert.IsNotNull(model);
            Assert.IsNull(model.Username);
            Assert.IsNull(model.Email);
            Assert.IsNull(model.PhoneNumber);
            Assert.IsFalse(model.IsEmailConfirmed);
        }

        /// <summary>
        /// Tests that Index correctly propagates StatusMessage to view model.
        /// Input: Valid user with StatusMessage set on controller.
        /// Expected: ViewResult with model.StatusMessage matching controller.StatusMessage.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Success message")]
        [DataRow("Error occurred!")]
        public async Task Index_VariousStatusMessages_PropagatesStatusMessageToModel(string? statusMessage)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var testUser = new ApplicationUser
            {
                UserName = "user",
                Email = "user@test.com",
                PhoneNumber = "555-0100",
                EmailConfirmed = true
            };

            var claimsPrincipal = new ClaimsPrincipal();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                StatusMessage = statusMessage,
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as IndexViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(statusMessage, model.StatusMessage);
        }

        /// <summary>
        /// Tests that Index correctly maps EmailConfirmed property from user to model.
        /// Input: Users with EmailConfirmed set to true and false.
        /// Expected: Model.IsEmailConfirmed matches user.EmailConfirmed.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Index_VariousEmailConfirmedValues_MapsEmailConfirmedCorrectly(bool emailConfirmed)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@test.com",
                PhoneNumber = "555-1234",
                EmailConfirmed = emailConfirmed
            };

            var claimsPrincipal = new ClaimsPrincipal();
            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as IndexViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(emailConfirmed, model.IsEmailConfirmed);
        }

        /// <summary>
        /// Tests that LinkLogin returns a ChallengeResult with correct provider and properties when valid provider is supplied.
        /// Input: Valid provider string "Google".
        /// Expected: ChallengeResult with provider "Google" and configured authentication properties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_ValidProvider_ReturnsChallengeResultWithCorrectProviderAndProperties()
        {
            // Arrange
            var provider = "Google";
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser());

            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            signInManagerMock.Setup(x => x.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContext = new DefaultHttpContext();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            httpContext.User = claimsPrincipal;

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ChallengeResult));
            var challengeResult = (ChallengeResult)result;
            Assert.AreEqual(provider, challengeResult.AuthenticationSchemes[0]);
            Assert.AreEqual(authProperties, challengeResult.Properties);

            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles null provider parameter.
        /// Input: Null provider.
        /// Expected: Method executes and passes null to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_NullProvider_PassesNullToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            string? provider = null;
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider!);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                null!, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles empty string provider parameter.
        /// Input: Empty string provider.
        /// Expected: Method executes and passes empty string to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_EmptyProvider_PassesEmptyStringToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            var provider = string.Empty;
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                string.Empty, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles whitespace-only provider parameter.
        /// Input: Whitespace-only string provider "   ".
        /// Expected: Method executes and passes whitespace string to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_WhitespaceProvider_PassesWhitespaceToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            var provider = "   ";
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles very long provider string.
        /// Input: Provider string with 10000 characters.
        /// Expected: Method executes and passes long string to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_VeryLongProvider_PassesLongStringToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            var provider = new string('A', 10000);
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles provider with special characters.
        /// Input: Provider string with special characters "<>&\"'".
        /// Expected: Method executes and passes special characters string to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_ProviderWithSpecialCharacters_PassesSpecialCharactersToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            var provider = "<>&\"'";
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin calls SignOutAsync with correct ExternalScheme parameter.
        /// Input: Valid provider "Facebook".
        /// Expected: SignOutAsync is called with IdentityConstants.ExternalScheme.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_ValidProvider_CallsSignOutAsyncWithExternalScheme()
        {
            // Arrange
            var provider = "Facebook";
            var userId = "test-user-id";
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            await controller.LinkLogin(provider);

            // Assert
            authServiceMock.Verify(a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                IdentityConstants.ExternalScheme,
                It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin uses correct redirect URL from Url.Action.
        /// Input: Valid provider "Twitter".
        /// Expected: ConfigureExternalAuthenticationProperties is called with redirect URL from Url.Action.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_ValidProvider_UsesRedirectUrlFromUrlAction()
        {
            // Arrange
            var provider = "Twitter";
            var userId = "test-user-id";
            var expectedRedirectUrl = "/custom/redirect/path";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedRedirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(authProperties);

            // Act
            await controller.LinkLogin(provider);

            // Assert
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, expectedRedirectUrl, userId), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLogin handles null userId from GetUserId.
        /// Input: Valid provider "Microsoft", but GetUserId returns null.
        /// Expected: Method processes and passes null userId to ConfigureExternalAuthenticationProperties.
        /// </summary>
        [TestMethod]
        public async Task LinkLogin_GetUserIdReturnsNull_PassesNullUserIdToConfigureExternalAuthenticationProperties()
        {
            // Arrange
            var provider = "Microsoft";
            string? userId = null;
            var redirectUrl = "/Manage/LinkLoginCallback";
            var authProperties = new AuthenticationProperties();

            var userManagerMock = CreateMockUserManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(redirectUrl);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
            controller.Url = urlHelperMock.Object;

            userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId);

            signInManagerMock.Setup(sm => sm.ConfigureExternalAuthenticationProperties(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(authProperties);

            // Act
            var result = await controller.LinkLogin(provider);

            // Assert
            Assert.IsNotNull(result);
            signInManagerMock.Verify(sm => sm.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl, null), Times.Once);
        }


        /// <summary>
        /// Tests that ResetAuthenticator successfully resets authenticator settings for a valid user
        /// and redirects to EnableAuthenticator action.
        /// </summary>
        [TestMethod]
        public async Task ResetAuthenticator_ValidUser_ResetsAuthenticatorAndRedirectsToEnableAuthenticator()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser@example.com" };
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<coderush.Services.IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller);

            // Act
            var result = await controller.ResetAuthenticator();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(ManageController.EnableAuthenticator), redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ResetAuthenticator calls SetTwoFactorEnabledAsync with false to disable two-factor authentication.
        /// </summary>
        [TestMethod]
        public async Task ResetAuthenticator_ValidUser_DisablesTwoFactorAuthentication()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser@example.com" };
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<coderush.Services.IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller);

            // Act
            await controller.ResetAuthenticator();

            // Assert
            mockUserManager.Verify(x => x.SetTwoFactorEnabledAsync(user, false), Times.Once);
        }

        /// <summary>
        /// Tests that ResetAuthenticator calls ResetAuthenticatorKeyAsync to reset the authenticator key.
        /// </summary>
        [TestMethod]
        public async Task ResetAuthenticator_ValidUser_ResetsAuthenticatorKey()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser@example.com" };
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<coderush.Services.IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller);

            // Act
            await controller.ResetAuthenticator();

            // Assert
            mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that ResetAuthenticator logs information about the reset operation with the user's ID.
        /// </summary>
        [TestMethod]
        public async Task ResetAuthenticator_ValidUser_LogsInformationWithUserId()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser@example.com" };
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<coderush.Services.IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller);

            // Act
            await controller.ResetAuthenticator();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("test-user-id")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Helper method to setup the controller context with a mock user.
        /// </summary>
        private static void SetupControllerContext(Controller controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
        private Mock<UserManager<ApplicationUser>> _mockUserManager = null!;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager = null!;
        private Mock<IEmailSender> _mockEmailSender = null!;
        private Mock<ILogger<ManageController>> _mockLogger = null!;
        private Mock<UrlEncoder> _mockUrlEncoder = null!;
        private ManageController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            // Setup SignInManager mock
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                null, null, null, null);

            _mockEmailSender = new Mock<IEmailSender>();
            _mockLogger = new Mock<ILogger<ManageController>>();
            _mockUrlEncoder = new Mock<UrlEncoder>();

            _controller = new ManageController(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockEmailSender.Object,
                _mockLogger.Object,
                _mockUrlEncoder.Object);

            // Setup controller context
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));
            httpContext.User = claimsPrincipal;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Setup Url helper
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("https://test.com/callback");
            _controller.Url = mockUrlHelper.Object;

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>());
        }

        /// <summary>
        /// Tests that SendVerificationEmail returns a ViewResult with the model when ModelState is invalid.
        /// Input: Invalid ModelState with a model.
        /// Expected: ViewResult containing the provided model.
        /// </summary>
        [TestMethod]
        public async Task SendVerificationEmail_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "test@example.com",
                Username = "TestUser"
            };
            _controller.ModelState.AddModelError("Email", "Invalid email");

            // Act
            var result = await _controller.SendVerificationEmail(model);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreSame(model, viewResult.Model);
        }

        /// <summary>
        /// Tests that SendVerificationEmail successfully sends verification email and redirects to Index.
        /// Input: Valid ModelState and existing user.
        /// Expected: Email sent, StatusMessage set, and RedirectToActionResult to Index.
        /// </summary>
        [TestMethod]
        public async Task SendVerificationEmail_ValidModel_SendsEmailAndRedirectsToIndex()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "test@example.com",
                Username = "TestUser"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "user@example.com",
                UserName = "TestUser"
            };

            var confirmationToken = "test-confirmation-token";
            var callbackUrl = "https://test.com/confirm?userId=user-123&code=test-confirmation-token";

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(confirmationToken);
            _mockEmailSender.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(callbackUrl);
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.SendVerificationEmail(model);

            // Assert
            Assert.IsNotNull(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockUserManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);
            _mockEmailSender.Verify(x => x.SendEmailAsync(
                user.Email,
                "Confirm your email",
                It.Is<string>(msg => msg.Contains(callbackUrl))),
                Times.Once);

            Assert.AreEqual("Verification email sent. Please check your email.", _controller.StatusMessage);
        }

        /// <summary>
        /// Tests that SendVerificationEmail correctly uses the user's email address when sending verification email.
        /// Input: Valid model with user having specific email.
        /// Expected: Email sent to the user's email address.
        /// </summary>
        [TestMethod]
        public async Task SendVerificationEmail_ValidUser_UsesUserEmailAddress()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "model@example.com",
                Username = "TestUser"
            };

            var userEmail = "actual-user@example.com";
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = userEmail,
                UserName = "TestUser"
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("token");
            _mockEmailSender.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendVerificationEmail(model);

            // Assert
            _mockEmailSender.Verify(x => x.SendEmailAsync(
                userEmail,
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that SendVerificationEmail includes the callback URL in the email message.
        /// Input: Valid model and user.
        /// Expected: Email message contains the callback URL with link tag.
        /// </summary>
        [TestMethod]
        public async Task SendVerificationEmail_ValidUser_EmailContainsCallbackUrl()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "test@example.com",
                Username = "TestUser"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "user@example.com",
                UserName = "TestUser"
            };

            var callbackUrl = "https://test.com/confirm?userId=user-123&code=test-token";

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("test-token");
            _mockEmailSender.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(callbackUrl);
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.SendVerificationEmail(model);

            // Assert
            _mockEmailSender.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(msg =>
                    msg.Contains(callbackUrl) &&
                    msg.Contains("<a href=") &&
                    msg.Contains("link</a>"))),
                Times.Once);
        }

        /// <summary>
        /// Tests that SendVerificationEmail correctly generates email confirmation token for the user.
        /// Input: Valid model and user.
        /// Expected: GenerateEmailConfirmationTokenAsync is called with the correct user.
        /// </summary>
        [TestMethod]
        public async Task SendVerificationEmail_ValidUser_GeneratesConfirmationToken()
        {
            // Arrange
            var model = new IndexViewModel
            {
                Email = "test@example.com",
                Username = "TestUser"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "user@example.com",
                UserName = "TestUser"
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("generated-token");
            _mockEmailSender.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendVerificationEmail(model);

            // Assert
            _mockUserManager.Verify(
                x => x.GenerateEmailConfirmationTokenAsync(
                    It.Is<ApplicationUser>(u => u.Id == user.Id)),
                Times.Once);
        }

        /// <summary>
        /// Tests that RemoveLogin successfully removes external login, signs in user, sets status message, and redirects to ExternalLogins.
        /// Input: Valid model with LoginProvider and ProviderKey, existing user, successful removal.
        /// Expected: User signed in, StatusMessage set, redirects to ExternalLogins action.
        /// </summary>
        [TestMethod]
        public async Task RemoveLogin_ValidModelAndSuccessfulRemoval_SignsInUserAndRedirectsToExternalLogins()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var successResult = IdentityResult.Success;
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, "TestProvider", "TestKey"))
                .ReturnsAsync(successResult);

            mockSignInManager.Setup(sm => sm.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetControllerContext(controller, claimsPrincipal);

            var model = new RemoveLoginViewModel
            {
                LoginProvider = "TestProvider",
                ProviderKey = "TestKey"
            };

            // Act
            var result = await controller.RemoveLogin(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(ManageController.ExternalLogins), redirectResult.ActionName);
            Assert.AreEqual("The external login was removed.", controller.StatusMessage);

            mockUserManager.Verify(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            mockUserManager.Verify(um => um.RemoveLoginAsync(user, "TestProvider", "TestKey"), Times.Once);
            mockSignInManager.Verify(sm => sm.SignInAsync(user, false, null), Times.Once);
        }

        /// <summary>
        /// Tests RemoveLogin with various edge case values for LoginProvider and ProviderKey.
        /// Input: Different combinations of LoginProvider and ProviderKey including empty strings, whitespace, and special characters.
        /// Expected: Successfully processes the request with the provided values.
        /// </summary>
        [TestMethod]
        [DataRow("", "", DisplayName = "Empty LoginProvider and ProviderKey")]
        [DataRow("   ", "   ", DisplayName = "Whitespace LoginProvider and ProviderKey")]
        [DataRow("Provider!@#$%", "Key!@#$%", DisplayName = "Special characters in LoginProvider and ProviderKey")]
        [DataRow("VeryLongProviderNameThatExceedsNormalLength1234567890", "VeryLongProviderKeyThatExceedsNormalLength1234567890", DisplayName = "Very long LoginProvider and ProviderKey")]
        public async Task RemoveLogin_EdgeCaseLoginProviderAndProviderKey_ProcessesSuccessfully(string loginProvider, string providerKey)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var successResult = IdentityResult.Success;
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, loginProvider, providerKey))
                .ReturnsAsync(successResult);

            mockSignInManager.Setup(sm => sm.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetControllerContext(controller, claimsPrincipal);

            var model = new RemoveLoginViewModel
            {
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            };

            // Act
            var result = await controller.RemoveLogin(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual("The external login was removed.", controller.StatusMessage);

            mockUserManager.Verify(um => um.RemoveLoginAsync(user, loginProvider, providerKey), Times.Once);
        }

        /// <summary>
        /// Tests RemoveLogin with multiple different provider names.
        /// Input: Common external login providers like Google, Facebook, Microsoft.
        /// Expected: Successfully processes the request for each provider.
        /// </summary>
        [TestMethod]
        [DataRow("Google", "google-key-123")]
        [DataRow("Facebook", "facebook-key-456")]
        [DataRow("Microsoft", "microsoft-key-789")]
        [DataRow("Twitter", "twitter-key-abc")]
        public async Task RemoveLogin_DifferentProviders_ProcessesSuccessfully(string provider, string providerKey)
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id", Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var successResult = IdentityResult.Success;
            mockUserManager.Setup(um => um.RemoveLoginAsync(user, provider, providerKey))
                .ReturnsAsync(successResult);

            mockSignInManager.Setup(sm => sm.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetControllerContext(controller, claimsPrincipal);

            var model = new RemoveLoginViewModel
            {
                LoginProvider = provider,
                ProviderKey = providerKey
            };

            // Act
            var result = await controller.RemoveLogin(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual(nameof(ManageController.ExternalLogins), redirectResult.ActionName);
            Assert.AreEqual("The external login was removed.", controller.StatusMessage);
        }

        private static void SetControllerContext(ManageController controller, ClaimsPrincipal user)
        {
            var httpContext = new DefaultHttpContext { User = user };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        /// <summary>
        /// Tests that ShowRecoveryCodes redirects to TwoFactorAuthentication when TempData contains null recovery codes.
        /// Input: TempData[RecoveryCodesKey] is null.
        /// Expected: RedirectToActionResult pointing to TwoFactorAuthentication action.
        /// </summary>
        [TestMethod]
        public void ShowRecoveryCodes_NullRecoveryCodes_RedirectsToTwoFactorAuthentication()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["RecoveryCodesKey"] = null;
            controller.TempData = tempData;

            // Act
            var result = controller.ShowRecoveryCodes();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("TwoFactorAuthentication", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ShowRecoveryCodes returns ViewResult with model when TempData contains valid recovery codes.
        /// Input: TempData[RecoveryCodesKey] contains a string array with multiple recovery codes.
        /// Expected: ViewResult with model containing user properties and StatusMessage.
        /// Expected: ViewResult with ShowRecoveryCodesViewModel containing the recovery codes.
        /// </summary>
        [TestMethod]
        [DataRow(new string[] { "RecoveryCode1", "RecoveryCode2", "RecoveryCode3" }, DisplayName = "Multiple codes")]
        [DataRow(new string[] { "SingleRecoveryCode" }, DisplayName = "Single code")]
        [DataRow(new string[] { "CODE1", "CODE2", "CODE3" }, DisplayName = "Multiple codes")]
        [DataRow(new string[] { "SINGLE_CODE" }, DisplayName = "Single code")]
        [DataRow(new string[] { }, DisplayName = "Empty array")]
        public void ShowRecoveryCodes_ValidRecoveryCodes_ReturnsViewWithModel(string[] recoveryCodes)
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["RecoveryCodesKey"] = recoveryCodes;
            controller.TempData = tempData;

            // Act
            var result = controller.ShowRecoveryCodes();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ShowRecoveryCodesViewModel;
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.RecoveryCodes);
            Assert.AreEqual(recoveryCodes.Length, model.RecoveryCodes.Length);
            CollectionAssert.AreEqual(recoveryCodes, model.RecoveryCodes);
        }

        /// <summary>
        /// Tests that ShowRecoveryCodes redirects to TwoFactorAuthentication when TempData key does not exist.
        /// Input: TempData does not contain RecoveryCodesKey.
        /// Expected: RedirectToActionResult pointing to TwoFactorAuthentication action.
        /// </summary>
        [TestMethod]
        public void ShowRecoveryCodes_KeyNotInTempData_RedirectsToTwoFactorAuthentication()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Act
            var result = controller.ShowRecoveryCodes();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("TwoFactorAuthentication", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ShowRecoveryCodes returns ViewResult with model containing special characters in recovery codes.
        /// Input: TempData[RecoveryCodesKey] contains recovery codes with special characters, whitespace, and edge cases.
        /// Expected: ViewResult with ShowRecoveryCodesViewModel containing all recovery codes exactly as provided.
        /// </summary>
        [TestMethod]
        public void ShowRecoveryCodes_RecoveryCodesWithSpecialCharacters_ReturnsViewWithModel()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var recoveryCodes = new string[]
            {
                "CODE@123!",
                "  SPACES  ",
                "",
                "UTF8-テスト-🔑",
                "Very-Long-Code-With-Many-Characters-1234567890-ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            };
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["RecoveryCodesKey"] = recoveryCodes;
            controller.TempData = tempData;

            // Act
            var result = controller.ShowRecoveryCodes();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ShowRecoveryCodesViewModel;
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.RecoveryCodes);
            Assert.AreEqual(recoveryCodes.Length, model.RecoveryCodes.Length);
            CollectionAssert.AreEqual(recoveryCodes, model.RecoveryCodes);
        }

        /// <summary>
        /// Tests SetPassword method when ModelState is invalid.
        /// Should return View with the provided model without calling any user manager methods.
        /// Tests that SetPassword redirects to ChangePassword when user has a password.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_WhenUserHasPassword_RedirectsToChangePassword()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.SetPassword();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.ChangePassword), redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that SetPassword returns View with SetPasswordViewModel when user does not have a password and StatusMessage is null.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_WhenUserDoesNotHavePasswordAndStatusMessageIsNull_ReturnsViewWithModel()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = null
            };

            // Act
            var result = await controller.SetPassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetPasswordViewModel));
            var model = viewResult.Model as SetPasswordViewModel;
            Assert.IsNotNull(model);
            Assert.IsNull(model.StatusMessage);
        }

        /// <summary>
        /// Tests that SetPassword returns View with SetPasswordViewModel when user does not have a password and StatusMessage has a value.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_WhenUserDoesNotHavePasswordAndStatusMessageHasValue_ReturnsViewWithModelContainingStatusMessage()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var statusMessage = "Test status message";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = statusMessage
            };

            // Act
            var result = await controller.SetPassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetPasswordViewModel));
            var model = viewResult.Model as SetPasswordViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(statusMessage, model.StatusMessage);
        }

        /// <summary>
        /// Tests that SetPassword returns View with SetPasswordViewModel when StatusMessage is empty string.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_WhenUserDoesNotHavePasswordAndStatusMessageIsEmpty_ReturnsViewWithModelContainingEmptyStatusMessage()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = string.Empty
            };

            // Act
            var result = await controller.SetPassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetPasswordViewModel));
            var model = viewResult.Model as SetPasswordViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(string.Empty, model.StatusMessage);
        }

        /// <summary>
        /// Tests that Disable2faWarning returns a ViewResult with the correct view name when user exists and has TwoFactorEnabled set to true.
        /// Input: Valid user with TwoFactorEnabled = true.
        /// Expected: Returns ViewResult with ViewName = "Disable2fa".
        /// Tests that SetPassword returns View with SetPasswordViewModel when StatusMessage contains whitespace.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_WhenUserDoesNotHavePasswordAndStatusMessageIsWhitespace_ReturnsViewWithModelContainingWhitespaceStatusMessage()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var whitespaceMessage = "   ";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                },
                StatusMessage = whitespaceMessage
            };

            // Act
            var result = await controller.SetPassword();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetPasswordViewModel));
            var model = viewResult.Model as SetPasswordViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(whitespaceMessage, model.StatusMessage);
        }

        /// <summary>
        /// Tests that Disable2fa successfully disables two-factor authentication 
        /// for a valid user and redirects to TwoFactorAuthentication action.
        /// </summary>
        [TestMethod]
        public async Task Disable2fa_ValidUser_SuccessfullyDisables2FA_RedirectsToTwoFactorAuthentication()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser { Id = "test-user-id" };
            var claimsPrincipal = new ClaimsPrincipal();
            var successResult = IdentityResult.Success;

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, false))
                .ReturnsAsync(successResult);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Act
            var result = await controller.Disable2fa();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("TwoFactorAuthentication", redirectResult.ActionName);
            mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            mockUserManager.Verify(x => x.SetTwoFactorEnabledAsync(user, false), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // (Duplicate TestInitialize removed - merged into Initialize method above)


        /// <summary>
        /// Tests that ExternalLogins returns correct view model when user has no logins and no password.
        /// Input: User with no current logins, no password, and some available auth schemes.
        /// Expected: ViewResult with model containing empty CurrentLogins, all schemes in OtherLogins, ShowRemoveButton false.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_NoLoginsNoPassword_ReturnsCorrectViewModel()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>();
            var authSchemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Facebook", "Facebook", typeof(IAuthenticationHandler))
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(authSchemes);

            _controller.StatusMessage = "Test Status";

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(ExternalLoginsViewModel));
            var model = (ExternalLoginsViewModel)viewResult.Model;

            Assert.IsNotNull(model.CurrentLogins);
            Assert.AreEqual(0, model.CurrentLogins.Count);
            Assert.IsNotNull(model.OtherLogins);
            Assert.AreEqual(2, model.OtherLogins.Count);
            Assert.IsFalse(model.ShowRemoveButton);
            Assert.AreEqual("Test Status", model.StatusMessage);
        }

        /// <summary>
        /// Tests that ExternalLogins sets ShowRemoveButton to true when user has one login and has password.
        /// Input: User with one current login and a password.
        /// Expected: ViewResult with ShowRemoveButton set to true.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_OneLoginWithPassword_ShowRemoveButtonIsTrue()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google")
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsTrue(model.ShowRemoveButton);
        }

        /// <summary>
        /// Tests that ExternalLogins sets ShowRemoveButton to false when user has one login and no password.
        /// Input: User with one current login and no password.
        /// Expected: ViewResult with ShowRemoveButton set to false.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_OneLoginNoPassword_ShowRemoveButtonIsFalse()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google")
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsFalse(model.ShowRemoveButton);
        }

        /// <summary>
        /// Tests that ExternalLogins sets ShowRemoveButton to true when user has multiple logins regardless of password.
        /// Input: User with multiple current logins.
        /// Expected: ViewResult with ShowRemoveButton set to true.
        /// </summary>
        [TestMethod]
        [DataRow(true, DisplayName = "Multiple logins with password")]
        [DataRow(false, DisplayName = "Multiple logins without password")]
        public async Task ExternalLogins_MultipleLogins_ShowRemoveButtonIsTrue(bool hasPassword)
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google"),
                new UserLoginInfo("Facebook", "facebook-key", "Facebook")
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(hasPassword);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsTrue(model.ShowRemoveButton);
            Assert.AreEqual(2, model.CurrentLogins.Count);
        }

        /// <summary>
        /// Tests that ExternalLogins correctly filters OtherLogins to exclude already configured logins.
        /// Input: User with Google login, available auth schemes include Google, Facebook, and Twitter.
        /// Expected: OtherLogins contains only Facebook and Twitter.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_FiltersOtherLoginsCorrectly_ExcludesCurrentLogins()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google")
            };
            var authSchemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Facebook", "Facebook", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Twitter", "Twitter", typeof(IAuthenticationHandler))
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(authSchemes);

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.AreEqual(2, model.OtherLogins.Count);
            Assert.IsFalse(model.OtherLogins.Any(x => x.Name == "Google"));
            Assert.IsTrue(model.OtherLogins.Any(x => x.Name == "Facebook"));
            Assert.IsTrue(model.OtherLogins.Any(x => x.Name == "Twitter"));
        }

        /// <summary>
        /// Tests that ExternalLogins returns empty OtherLogins when all auth schemes are already configured.
        /// Input: User with Google and Facebook logins, available auth schemes are only Google and Facebook.
        /// Expected: OtherLogins is empty.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_AllSchemesAlreadyConfigured_OtherLoginsIsEmpty()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google"),
                new UserLoginInfo("Facebook", "facebook-key", "Facebook")
            };
            var authSchemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Facebook", "Facebook", typeof(IAuthenticationHandler))
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(authSchemes);

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.AreEqual(0, model.OtherLogins.Count);
        }

        /// <summary>
        /// Tests that ExternalLogins returns all schemes in OtherLogins when user has no current logins.
        /// Input: User with no current logins, multiple auth schemes available.
        /// Expected: All auth schemes appear in OtherLogins.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_NoCurrentLogins_AllSchemesInOtherLogins()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>();
            var authSchemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Facebook", "Facebook", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Twitter", "Twitter", typeof(IAuthenticationHandler))
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(authSchemes);

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.AreEqual(3, model.OtherLogins.Count);
        }

        /// <summary>
        /// Tests that ExternalLogins correctly passes StatusMessage to the view model.
        /// Input: Controller StatusMessage set to null.
        /// Expected: Model StatusMessage is null.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_StatusMessageIsNull_ModelStatusMessageIsNull()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(new List<UserLoginInfo>());
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            _controller.StatusMessage = null;

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsNull(model.StatusMessage);
        }

        /// <summary>
        /// Tests that ExternalLogins correctly passes non-empty StatusMessage to the view model.
        /// Input: Controller StatusMessage set to a specific value.
        /// Expected: Model StatusMessage matches controller StatusMessage.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_StatusMessageIsSet_ModelStatusMessageMatches()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var expectedStatusMessage = "External login was successfully added.";

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(new List<UserLoginInfo>());
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            _controller.StatusMessage = expectedStatusMessage;

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.AreEqual(expectedStatusMessage, model.StatusMessage);
        }

        /// <summary>
        /// Tests that ExternalLogins handles empty authentication schemes list correctly.
        /// Input: GetExternalAuthenticationSchemesAsync returns empty list.
        /// Expected: OtherLogins is empty.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_EmptyAuthSchemes_OtherLoginsIsEmpty()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(new List<UserLoginInfo>());
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.AreEqual(0, model.OtherLogins.Count);
        }

        /// <summary>
        /// Tests that ExternalLogins sets ShowRemoveButton to true when user has no password but has multiple logins.
        /// Input: User with two logins and no password.
        /// Expected: ShowRemoveButton is true due to multiple logins.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_NoPasswordButMultipleLogins_ShowRemoveButtonIsTrue()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>
            {
                new UserLoginInfo("Google", "google-key", "Google"),
                new UserLoginInfo("Facebook", "facebook-key", "Facebook")
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsTrue(model.ShowRemoveButton);
        }

        /// <summary>
        /// Tests that ExternalLogins sets ShowRemoveButton to true when user has password but no logins.
        /// Input: User with password but no external logins.
        /// Expected: ShowRemoveButton is true due to password.
        /// </summary>
        [TestMethod]
        public async Task ExternalLogins_HasPasswordButNoLogins_ShowRemoveButtonIsTrue()
        {
            // Arrange
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var currentLogins = new List<UserLoginInfo>();

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(currentLogins);
            _mockUserManager.Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);
            _mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            // Act
            var result = await _controller.ExternalLogins();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ExternalLoginsViewModel)viewResult.Model;
            Assert.IsTrue(model.ShowRemoveButton);
        }

        /// <summary>
        /// Tests that Disable2faWarning returns a ViewResult with the correct view name when user exists and has TwoFactorEnabled set to true.
        /// Input: Valid user with TwoFactorEnabled = true.
        /// Expected: Returns ViewResult with ViewName = "Disable2fa".
        /// </summary>
        [TestMethod]
        public async Task Disable2faWarning_UserExistsAndTwoFactorEnabled_ReturnsViewResult()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                TwoFactorEnabled = true
            };

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var claimsPrincipal = new ClaimsPrincipal();

            userManagerMock.Setup(x => x.GetUserAsync(claimsPrincipal))
                .ReturnsAsync(user);

            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.Disable2faWarning();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("Disable2fa", viewResult.ViewName);
        }

        /// <summary>
        /// Tests that GenerateRecoveryCodesWarning returns ViewResult with correct view name when user has TwoFactorEnabled.
        /// Input: User exists and TwoFactorEnabled is true.
        /// Expected: ViewResult with ViewName "GenerateRecoveryCodes".
        /// </summary>
        [TestMethod]
        public async Task GenerateRecoveryCodesWarning_TwoFactorEnabled_ReturnsViewResult()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<System.Text.Encodings.Web.UrlEncoder>();

            var userId = "test-user-id";
            var applicationUser = new ApplicationUser { Id = userId, TwoFactorEnabled = true };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) }));

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(applicationUser);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GenerateRecoveryCodesWarning();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("GenerateRecoveryCodes", viewResult.ViewName);
        }

        /// <summary>
        /// Tests SetPassword method when ModelState is invalid.
        /// Should return View with the provided model without calling any user manager methods.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            controller.ModelState.AddModelError("NewPassword", "Password is required");

            var model = new SetPasswordViewModel { NewPassword = "" };

            // Act
            var result = await controller.SetPassword(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
            userManagerMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
        }

        /// <summary>
        /// Tests SetPassword method when AddPasswordAsync fails.
        /// Should add errors to ModelState and return View with model.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_AddPasswordFails_ReturnsViewWithModelAndErrors()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var identityError = new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" };
            var failedResult = IdentityResult.Failed(identityError);
            userManagerMock.Setup(x => x.AddPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            var model = new SetPasswordViewModel { NewPassword = "short" };

            // Act
            var result = await controller.SetPassword(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsTrue(controller.ModelState.ErrorCount > 0);
            signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests SetPassword method when password is successfully set.
        /// Should sign in the user, set status message, and redirect to SetPassword action.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_Success_SignsInUserAndRedirects()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            userManagerMock.Setup(x => x.AddPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            signInManagerMock.Setup(x => x.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var model = new SetPasswordViewModel { NewPassword = "ValidPassword123!" };

            // Act
            var result = await controller.SetPassword(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.SetPassword), redirectResult.ActionName);
            Assert.AreEqual("Your password has been set.", controller.StatusMessage);
            signInManagerMock.Verify(x => x.SignInAsync(user, false, null), Times.Once);
        }

        /// <summary>
        /// Tests SetPassword method with null model.
        /// Should handle null model gracefully (ModelState validation should fail).
        /// </summary>
        [TestMethod]
        public async Task SetPassword_NullModel_ReturnsViewWithNull()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            controller.ModelState.AddModelError("", "Model is null");

            // Act
            var result = await controller.SetPassword(null!);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        /// <summary>
        /// Tests SetPassword method when AddPasswordAsync fails with multiple errors.
        /// Should add all errors to ModelState.
        /// </summary>
        [TestMethod]
        public async Task SetPassword_AddPasswordFailsWithMultipleErrors_AddsAllErrorsToModelState()
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var errors = new[]
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
                new IdentityError { Code = "PasswordRequiresDigit", Description = "Password must have at least one digit" }
            };
            var failedResult = IdentityResult.Failed(errors);
            userManagerMock.Setup(x => x.AddPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            var model = new SetPasswordViewModel { NewPassword = "short" };

            // Act
            var result = await controller.SetPassword(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(2, controller.ModelState.ErrorCount);
        }

        /// <summary>
        /// Tests SetPassword method with various valid password formats.
        /// Should successfully add password for all valid formats.
        /// </summary>
        [TestMethod]
        [DataRow("Password123!")]
        [DataRow("VeryL0ngP@ssw0rdWith$pecialCh@racters123")]
        [DataRow("Simple1!")]
        public async Task SetPassword_ValidPasswordFormats_SuccessfullyAddsPassword(string password)
        {
            // Arrange
            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);
            var emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<ManageController>>();
            var urlEncoderMock = new Mock<UrlEncoder>();

            var controller = new ManageController(
                userManagerMock.Object,
                signInManagerMock.Object,
                emailSenderMock.Object,
                loggerMock.Object,
                urlEncoderMock.Object);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            userManagerMock.Setup(x => x.AddPasswordAsync(user, password))
                .ReturnsAsync(IdentityResult.Success);

            signInManagerMock.Setup(x => x.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            var model = new SetPasswordViewModel { NewPassword = password };

            // Act
            var result = await controller.SetPassword(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            userManagerMock.Verify(x => x.AddPasswordAsync(user, password), Times.Once);
        }

        private static Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<ApplicationUser>> GetMockSignInManager(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(
                userManager,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with user having authenticator, 2FA enabled, and recovery codes.
        /// Verifies the model properties are correctly populated.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_UserWithAuthenticatorAnd2faEnabledAndRecoveryCodes_ReturnsCorrectModel()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = true };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("authenticator-key");
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(5);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(TwoFactorAuthenticationViewModel));
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.IsTrue(model.HasAuthenticator);
            Assert.IsTrue(model.Is2faEnabled);
            Assert.AreEqual(5, model.RecoveryCodesLeft);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with user having no authenticator, 2FA disabled, and no recovery codes.
        /// Verifies all boolean flags are false and recovery codes count is zero.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_UserWithNoAuthenticatorAnd2faDisabledAndNoRecoveryCodes_ReturnsCorrectModel()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = false };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync((string?)null);
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(TwoFactorAuthenticationViewModel));
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.IsFalse(model.HasAuthenticator);
            Assert.IsFalse(model.Is2faEnabled);
            Assert.AreEqual(0, model.RecoveryCodesLeft);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with various recovery codes counts including boundary values.
        /// Verifies that the recovery codes count is correctly reflected in the model.
        /// </summary>
        [TestMethod]
        [DataRow(0, DisplayName = "Zero recovery codes")]
        [DataRow(1, DisplayName = "One recovery code")]
        [DataRow(10, DisplayName = "Ten recovery codes")]
        [DataRow(100, DisplayName = "One hundred recovery codes")]
        [DataRow(int.MaxValue, DisplayName = "Maximum int value recovery codes")]
        public async Task TwoFactorAuthentication_VariousRecoveryCodesCounts_ReturnsCorrectCount(int recoveryCodesCount)
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = true };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("key");
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(recoveryCodesCount);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.AreEqual(recoveryCodesCount, model.RecoveryCodesLeft);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with authenticator key as empty string.
        /// Verifies that empty string is treated as having an authenticator (not null).
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_AuthenticatorKeyIsEmptyString_HasAuthenticatorIsTrue()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = false };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync(string.Empty);
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.IsTrue(model.HasAuthenticator);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with user having authenticator but 2FA disabled.
        /// Verifies the combination of HasAuthenticator=true and Is2faEnabled=false.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_UserWithAuthenticatorBut2faDisabled_ReturnsCorrectModel()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = false };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("some-key");
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(3);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.IsTrue(model.HasAuthenticator);
            Assert.IsFalse(model.Is2faEnabled);
            Assert.AreEqual(3, model.RecoveryCodesLeft);
        }

        /// <summary>
        /// Tests TwoFactorAuthentication with user having no authenticator but 2FA enabled.
        /// Verifies the combination of HasAuthenticator=false and Is2faEnabled=true.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_UserWithNoAuthenticatorBut2faEnabled_ReturnsCorrectModel()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = true };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync((string?)null);
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(7);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (TwoFactorAuthenticationViewModel)viewResult.Model!;
            Assert.IsFalse(model.HasAuthenticator);
            Assert.IsTrue(model.Is2faEnabled);
            Assert.AreEqual(7, model.RecoveryCodesLeft);
        }

        /// <summary>
        /// Tests that TwoFactorAuthentication returns a ViewResult.
        /// Verifies the return type is correct.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_ValidUser_ReturnsViewResult()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = true };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("key");
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(5);

            // Act
            var result = await _controller.TwoFactorAuthentication();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that TwoFactorAuthentication calls GetUserAsync with the correct User principal.
        /// Verifies proper interaction with UserManager.
        /// </summary>
        [TestMethod]
        public async Task TwoFactorAuthentication_CallsGetUserAsyncWithCorrectPrincipal_VerifiesInteraction()
        {
            // Arrange
            var user = new ApplicationUser { TwoFactorEnabled = true };
            var claimsPrincipal = new ClaimsPrincipal();
            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;

            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                .ReturnsAsync("key");
            _mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user))
                .ReturnsAsync(5);

            // Act
            await _controller.TwoFactorAuthentication();

            // Assert
            _mockUserManager.Verify(x => x.GetUserAsync(claimsPrincipal), Times.Once);
            _mockUserManager.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Once);
            _mockUserManager.Verify(x => x.CountRecoveryCodesAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateRecoveryCodes successfully generates recovery codes and returns a ViewResult
        /// when the user has 2FA enabled. Validates that the correct view name and model are returned.
        /// </summary>
        [TestMethod]
        public async Task GenerateRecoveryCodes_TwoFactorEnabled_ReturnsViewResultWithRecoveryCodes()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId,
                TwoFactorEnabled = true
            };
            var recoveryCodes = new List<string> { "CODE1", "CODE2", "CODE3", "CODE4", "CODE5", "CODE6", "CODE7", "CODE8", "CODE9", "CODE10" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(recoveryCodes);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller, claimsPrincipal);

            // Act
            var result = await controller.GenerateRecoveryCodes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            Assert.AreEqual("ShowRecoveryCodes", viewResult.ViewName);
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ShowRecoveryCodesViewModel));

            var model = (ShowRecoveryCodesViewModel)viewResult.Model;
            Assert.IsNotNull(model.RecoveryCodes);
            Assert.AreEqual(10, model.RecoveryCodes.Length);
            CollectionAssert.AreEqual(recoveryCodes, model.RecoveryCodes);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId) && v.ToString()!.Contains("generated new 2FA recovery codes")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that GenerateRecoveryCodes handles empty recovery codes collection.
        /// This validates that the method can handle the edge case where no codes are generated.
        /// </summary>
        [TestMethod]
        public async Task GenerateRecoveryCodes_EmptyRecoveryCodes_ReturnsViewResultWithEmptyArray()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                TwoFactorEnabled = true
            };
            var recoveryCodes = new List<string>();
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(recoveryCodes);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller, claimsPrincipal);

            // Act
            var result = await controller.GenerateRecoveryCodes();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            var model = (ShowRecoveryCodesViewModel)viewResult.Model!;
            Assert.IsNotNull(model.RecoveryCodes);
            Assert.AreEqual(0, model.RecoveryCodes.Length);
        }

        /// <summary>
        /// Tests that GenerateRecoveryCodes correctly handles a single recovery code.
        /// This validates the boundary case of minimal recovery codes.
        /// </summary>
        [TestMethod]
        public async Task GenerateRecoveryCodes_SingleRecoveryCode_ReturnsViewResultWithOneCode()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                TwoFactorEnabled = true
            };
            var recoveryCodes = new List<string> { "SINGLE-CODE" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(recoveryCodes);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller, claimsPrincipal);

            // Act
            var result = await controller.GenerateRecoveryCodes();

            // Assert
            var viewResult = (ViewResult)result;
            var model = (ShowRecoveryCodesViewModel)viewResult.Model!;
            Assert.AreEqual(1, model.RecoveryCodes.Length);
            Assert.AreEqual("SINGLE-CODE", model.RecoveryCodes[0]);
        }

        /// <summary>
        /// Tests that GenerateRecoveryCodes verifies GenerateNewTwoFactorRecoveryCodesAsync is called with correct parameters.
        /// This validates that the method requests exactly 10 recovery codes.
        /// </summary>
        [TestMethod]
        public async Task GenerateRecoveryCodes_CallsGenerateNewTwoFactorRecoveryCodesAsync_WithCorrectParameters()
        {
            // Arrange
            var mockUserManager = CreateMockUserManager();
            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                TwoFactorEnabled = true
            };
            var recoveryCodes = new List<string> { "CODE1", "CODE2" };
            var claimsPrincipal = new ClaimsPrincipal();

            mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))
                .ReturnsAsync(recoveryCodes);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            SetupControllerContext(controller, claimsPrincipal);

            // Act
            await controller.GenerateRecoveryCodes();

            // Assert
            mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(user, 10), Times.Once);
        }

        private static void SetupControllerContext(ManageController controller, ClaimsPrincipal claimsPrincipal)
        {
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }


        /// <summary>
        /// Tests that ChangePassword returns a ViewResult with the model when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };
            _controller!.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreSame(model, viewResult.Model);
            _mockUserManager!.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
        }

        /// <summary>
        /// Tests that ChangePassword returns ViewResult with model when password change fails.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_PasswordChangeFails_ReturnsViewWithModelAndErrors()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var userId = "test-user-id";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var identityErrors = new[]
            {
                new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }
            };
            var failedResult = IdentityResult.Failed(identityErrors);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreSame(model, viewResult.Model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0);
            _mockSignInManager!.Verify(x => x.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ChangePassword successfully changes password, signs in user, logs information, sets status message, and redirects.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_ValidRequest_ChangesPasswordAndRedirects()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var userId = "test-user-id";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager!.Setup(x => x.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ManageController.ChangePassword), redirectResult.ActionName);
            Assert.AreEqual("Your password has been changed.", _controller.StatusMessage);

            _mockUserManager.Verify(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword), Times.Once);
            _mockSignInManager.Verify(x => x.SignInAsync(user, false, null), Times.Once);
            _mockLogger!.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("User changed their password successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword handles empty string passwords appropriately through model validation.
        /// </summary>
        [TestMethod]
        [DataRow("", "NewPass123!", "NewPass123!")]
        [DataRow("OldPass123!", "", "")]
        [DataRow("", "", "")]
        public async Task ChangePassword_EmptyPasswords_HandledByModelValidation(string oldPassword, string newPassword, string confirmPassword)
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };
            _controller!.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreSame(model, viewResult.Model);
        }

        /// <summary>
        /// Tests that ChangePassword handles null model gracefully (ModelState would be invalid).
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_NullModel_ReturnsViewResult()
        {
            // Arrange
            ChangePasswordViewModel? model = null;
            _controller!.ModelState.AddModelError("Model", "Model is required");

            // Act
            var result = await _controller.ChangePassword(model!);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        /// <summary>
        /// Tests that ChangePassword verifies SignInAsync is called with isPersistent set to false.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_Success_SignsInUserWithIsPersistentFalse()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager!.Setup(x => x.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

            // Act
            await _controller.ChangePassword(model);

            // Assert
            _mockSignInManager.Verify(x => x.SignInAsync(user, false, null), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword with multiple identity errors adds all errors to ModelState.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_MultiplePasswordErrors_AddsAllErrorsToModelState()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = "OldPass123!",
                NewPassword = "weak",
                ConfirmPassword = "weak"
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var identityErrors = new[]
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Password is too short." },
                new IdentityError { Code = "PasswordRequiresDigit", Description = "Password must contain a digit." },
                new IdentityError { Code = "PasswordRequiresUpper", Description = "Password must contain an uppercase letter." }
            };
            var failedResult = IdentityResult.Failed(identityErrors);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(3, _controller.ModelState.ErrorCount);
        }

        /// <summary>
        /// Tests that ChangePassword with special characters in passwords is handled correctly.
        /// </summary>
        [TestMethod]
        [DataRow("Old!@#$%^&*()", "New!@#$%^&*()", "New!@#$%^&*()")]
        [DataRow("Pass with spaces", "New Pass with spaces", "New Pass with spaces")]
        [DataRow("Unicode密码", "NewUnicode密码", "NewUnicode密码")]
        public async Task ChangePassword_SpecialCharactersInPasswords_ProcessedCorrectly(string oldPassword, string newPassword, string confirmPassword)
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager!.Setup(x => x.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            _mockUserManager.Verify(x => x.ChangePasswordAsync(user, oldPassword, newPassword), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePassword with very long passwords (edge case for string length) is handled.
        /// </summary>
        [TestMethod]
        public async Task ChangePassword_VeryLongPasswords_ProcessedCorrectly()
        {
            // Arrange
            var longPassword = new string('a', 1000) + "A1!";
            var model = new ChangePasswordViewModel
            {
                OldPassword = longPassword,
                NewPassword = longPassword + "New",
                ConfirmPassword = longPassword + "New"
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }));

            _controller!.ControllerContext.HttpContext.User = claimsPrincipal;
            _mockUserManager!.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager!.Setup(x => x.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsNotNull(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            _mockUserManager.Verify(x => x.ChangePasswordAsync(user, longPassword, longPassword + "New"), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLoginCallback successfully adds external login when all operations succeed.
        /// Input: Valid user, valid external login info, successful AddLoginAsync.
        /// Expected: Sets StatusMessage, signs out from external scheme, redirects to ExternalLogins.
        /// </summary>
        [TestMethod]
        public async Task LinkLoginCallback_ValidUserAndExternalLoginInfo_ReturnsRedirectToExternalLogins()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal();
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "google-key", "Google");
            var identityResult = IdentityResult.Success;

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            mockUserManager.Setup(x => x.AddLoginAsync(user, externalLoginInfo)).ReturnsAsync(identityResult);

            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            mockSignInManager.Setup(x => x.GetExternalLoginInfoAsync(userId)).ReturnsAsync(externalLoginInfo);

            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockHttpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthenticationService.Object);
            mockAuthenticationService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), IdentityConstants.ExternalScheme, null))
                .Returns(Task.CompletedTask);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            controller.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("callbackUrl");
            controller.Url = mockUrlHelper.Object;

            // Act
            var result = await controller.LinkLoginCallback();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("ExternalLogins", redirectResult.ActionName);
            Assert.AreEqual("The external login was added.", controller.StatusMessage);
            mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            mockSignInManager.Verify(x => x.GetExternalLoginInfoAsync(userId), Times.Once);
            mockUserManager.Verify(x => x.AddLoginAsync(user, externalLoginInfo), Times.Once);
        }

        /// <summary>
        /// Tests that LinkLoginCallback handles different user IDs correctly.
        /// Input: Various user ID formats including empty string, special characters, and long IDs.
        /// Expected: Method processes correctly with different user ID formats.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("user-with-special-chars-!@#$%")]
        [DataRow("very-long-user-id-with-many-characters-1234567890-abcdefghijklmnopqrstuvwxyz")]
        public async Task LinkLoginCallback_DifferentUserIdFormats_ProcessesCorrectly(string userId)
        {
            // Arrange
            var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
            var claimsPrincipal = new ClaimsPrincipal();
            var externalLoginInfo = new ExternalLoginInfo(claimsPrincipal, "Google", "google-key", "Google");
            var identityResult = IdentityResult.Success;

            var mockUserManager = CreateMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            mockUserManager.Setup(x => x.AddLoginAsync(user, externalLoginInfo)).ReturnsAsync(identityResult);

            var mockSignInManager = CreateMockSignInManager(mockUserManager.Object);
            mockSignInManager.Setup(x => x.GetExternalLoginInfoAsync(userId)).ReturnsAsync(externalLoginInfo);

            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockHttpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthenticationService.Object);
            mockAuthenticationService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), IdentityConstants.ExternalScheme, null))
                .Returns(Task.CompletedTask);

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            controller.ControllerContext.HttpContext.User = claimsPrincipal;
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.LinkLoginCallback();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual("The external login was added.", controller.StatusMessage);
        }

        /// <summary>
        /// Tests that ResetAuthenticatorWarning returns a ViewResult with the correct view name "ResetAuthenticator".
        /// This verifies the method correctly redirects to the reset authenticator view.
        /// </summary>
        [TestMethod]
        public void ResetAuthenticatorWarning_Always_ReturnsViewResultWithResetAuthenticatorViewName()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                mockUserManager.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var mockEmailSender = new Mock<IEmailSender>();
            var mockLogger = new Mock<ILogger<ManageController>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var controller = new ManageController(
                mockUserManager.Object,
                mockSignInManager.Object,
                mockEmailSender.Object,
                mockLogger.Object,
                mockUrlEncoder.Object);

            // Act
            IActionResult result = controller.ResetAuthenticatorWarning();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("ResetAuthenticator", viewResult.ViewName);
        }
    }
}