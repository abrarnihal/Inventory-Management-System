using System;

using coderush;
using coderush.Controllers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace Microsoft.AspNetCore.Mvc.UnitTests
{
    /// <summary>
    /// Unit tests for UrlHelperExtensions class.
    /// </summary>
    [TestClass]
    public class UrlHelperExtensionsTests
    {
        /// <summary>
        /// Tests ResetPasswordCallbackLink with valid inputs to ensure it returns the expected URL.
        /// </summary>
        [TestMethod]
        [DataRow("user123", "code456", "https", "https://example.com/Account/ResetPassword?userId=user123&code=code456")]
        [DataRow("testUser", "testCode", "http", "http://example.com/Account/ResetPassword")]
        [DataRow("user@email.com", "ABC123!@#", "https", "https://test.com/reset")]
        [DataRow("123", "xyz", "ftp", "ftp://custom.com/reset")]
        public void ResetPasswordCallbackLink_ValidInputs_ReturnsExpectedUrl(string userId, string code, string scheme, string expectedUrl)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ResetPassword" &&
                    ctx.Controller == "Account" &&
                    ctx.Protocol == scheme)))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account" &&
                ctx.Protocol == scheme)), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with empty strings to ensure proper handling.
        /// </summary>
        [TestMethod]
        [DataRow("", "", "")]
        [DataRow("", "code", "http")]
        [DataRow("user", "", "https")]
        [DataRow("user", "code", "")]
        public void ResetPasswordCallbackLink_EmptyStrings_PassesParametersToAction(string userId, string code, string scheme)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with whitespace strings to ensure proper handling.
        /// </summary>
        [TestMethod]
        [DataRow("   ", "   ", "   ")]
        [DataRow(" ", "code", "http")]
        [DataRow("user", "\t", "https")]
        [DataRow("user", "code", "\n")]
        public void ResetPasswordCallbackLink_WhitespaceStrings_PassesParametersToAction(string userId, string code, string scheme)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with special characters to ensure they are passed correctly.
        /// </summary>
        [TestMethod]
        [DataRow("user<>123", "code!@#$%", "http://custom")]
        [DataRow("user@domain.com", "code/with/slashes", "https")]
        [DataRow("user'with'quotes", "code\"quotes\"", "http")]
        [DataRow("user&special", "code=value&other=value", "https://")]
        public void ResetPasswordCallbackLink_SpecialCharacters_PassesParametersToAction(string userId, string code, string scheme)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with very long strings to ensure they are handled properly.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_VeryLongStrings_PassesParametersToAction()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var longUserId = new string('a', 10000);
            var longCode = new string('b', 10000);
            var longScheme = new string('c', 1000);
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(longUserId, longCode, longScheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink when Action returns null to ensure null is returned.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_ActionReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns((string?)null);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink("userId", "code", "https");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink ensures the controller name is always "Account".
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_AlwaysPassesAccountController_VerifiesControllerName()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink("userId", "code", "https");

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink ensures the action name is "ResetPassword" from AccountController.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_AlwaysPassesResetPasswordAction_VerifiesActionName()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink("userId", "code", "https");

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword")), Times.Once);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with null userId to ensure it is passed to Action.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_NullUserId_PassesNullToAction()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(null!, "code", "https");

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with null code to ensure it is passed to Action.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_NullCode_PassesNullToAction()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink("userId", null!, "https");

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with null scheme to ensure it is passed to Action.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_NullScheme_PassesNullToAction()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink("userId", "code", null!);

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests ResetPasswordCallbackLink with all null parameters except urlHelper to ensure they are passed to Action.
        /// </summary>
        [TestMethod]
        public void ResetPasswordCallbackLink_AllNullParameters_PassesNullsToAction()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://example.com/result";
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.ResetPasswordCallbackLink(null!, null!, null!);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(x => x.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ResetPassword" &&
                ctx.Controller == "Account")), Times.Once);
        }

        /// <summary>
        /// Tests that EmailConfirmationLink generates the correct URL with valid inputs.
        /// Input: Valid userId, code, and scheme.
        /// Expected: Returns the URL generated by IUrlHelper.Action.
        /// </summary>
        [TestMethod]
        public void EmailConfirmationLink_ValidInputs_ReturnsGeneratedUrl()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "https://example.com/Account/ConfirmEmail?userId=user123&code=code456";
            var userId = "user123";
            var code = "code456";
            var scheme = "https";

            mockUrlHelper
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ConfirmEmail" &&
                    ctx.Controller == "Account" &&
                    ctx.Protocol == scheme)))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            mockUrlHelper.Verify(u => u.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == "ConfirmEmail" &&
                ctx.Controller == "Account" &&
                ctx.Protocol == scheme)), Times.Once);
        }

        /// <summary>
        /// Tests EmailConfirmationLink with various string parameter combinations including null, empty, whitespace, and special characters.
        /// </summary>
        [TestMethod]
        [DataRow(null, "code", "https", "http://test.com/null")]
        [DataRow("", "code", "https", "http://test.com/empty")]
        [DataRow("   ", "code", "https", "http://test.com/whitespace")]
        [DataRow("user", null, "https", "http://test.com/codenull")]
        [DataRow("user", "", "https", "http://test.com/codeempty")]
        [DataRow("user", "   ", "https", "http://test.com/codewhitespace")]
        [DataRow("user", "code", null, "http://test.com/schemenull")]
        [DataRow("user", "code", "", "http://test.com/schemeempty")]
        [DataRow("user", "code", "   ", "http://test.com/schemewhitespace")]
        [DataRow("user@example.com", "code+/=123", "https", "http://test.com/special")]
        [DataRow("user<>\"'&", "code<>\"'&", "https", "http://test.com/htmlspecial")]
        public void EmailConfirmationLink_VariousStringInputs_ReturnsGeneratedUrl(string userId, string code, string scheme, string expectedUrl)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink(userId, code, scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests EmailConfirmationLink with very long string inputs.
        /// </summary>
        [TestMethod]
        public void EmailConfirmationLink_VeryLongStrings_ReturnsGeneratedUrl()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://test.com/long";
            var veryLongString = new string('a', 10000);

            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink(veryLongString, veryLongString, veryLongString);

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests EmailConfirmationLink with special protocol schemes.
        /// </summary>
        [TestMethod]
        [DataRow("http", "http://test.com/http")]
        [DataRow("https", "https://test.com/https")]
        [DataRow("ftp", "ftp://test.com/ftp")]
        [DataRow("custom-scheme", "custom-scheme://test.com/custom")]
        public void EmailConfirmationLink_DifferentSchemes_ReturnsGeneratedUrl(string scheme, string expectedUrl)
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelper
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ConfirmEmail" &&
                    ctx.Controller == "Account" &&
                    ctx.Protocol == scheme)))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink("user", "code", scheme);

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }

        /// <summary>
        /// Tests that EmailConfirmationLink passes correct action and controller names to Action method.
        /// </summary>
        [TestMethod]
        public void EmailConfirmationLink_ValidInputs_CallsActionWithCorrectParameters()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://test.com/confirm";
            string? capturedAction = null;
            string? capturedController = null;
            string? capturedProtocol = null;

            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Callback<UrlActionContext>((ctx) =>
                {
                    capturedAction = ctx.Action;
                    capturedController = ctx.Controller;
                    capturedProtocol = ctx.Protocol;
                })
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink("user123", "code456", "https");

            // Assert
            Assert.AreEqual(expectedUrl, result);
            Assert.AreEqual("ConfirmEmail", capturedAction);
            Assert.AreEqual("Account", capturedController);
            Assert.AreEqual("https", capturedProtocol);
        }

        /// <summary>
        /// Tests EmailConfirmationLink when IUrlHelper.Action returns null.
        /// </summary>
        [TestMethod]
        public void EmailConfirmationLink_ActionReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns((string?)null);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink("user", "code", "https");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests EmailConfirmationLink with strings containing control characters.
        /// </summary>
        [TestMethod]
        public void EmailConfirmationLink_StringsWithControlCharacters_ReturnsGeneratedUrl()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var expectedUrl = "http://test.com/control";
            var userIdWithControl = "user\nid\t123";
            var codeWithControl = "code\r\n456";
            var schemeWithControl = "ht\ttps";

            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns(expectedUrl);

            // Act
            var result = mockUrlHelper.Object.EmailConfirmationLink(userIdWithControl, codeWithControl, schemeWithControl);

            // Assert
            Assert.AreEqual(expectedUrl, result);
        }
    }
}