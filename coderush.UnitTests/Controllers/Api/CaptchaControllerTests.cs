using System;

using coderush.Controllers.Api;
using coderush.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="CaptchaController"/> class.
    /// </summary>
    [TestClass]
    public class CaptchaControllerTests
    {
        /// <summary>
        /// Tests that GetChallenge returns OkObjectResult with correct token and puzzleX values
        /// when the captcha service generates a challenge successfully.
        /// </summary>
        /// <param name="token">The token value returned by the service.</param>
        /// <param name="puzzleX">The puzzleX value returned by the service.</param>
        [TestMethod]
        [DataRow("validToken123", 100)]
        [DataRow("anotherToken456", 0)]
        [DataRow("edgeCaseToken", -1)]
        [DataRow("token", int.MaxValue)]
        [DataRow("minToken", int.MinValue)]
        [DataRow("", 50)]
        [DataRow("veryLongTokenStringWithManyCharactersThatExceedsNormalLengthExpectationsAndTestsHandlingOfLargeStrings", 200)]
        public void GetChallenge_WithVariousTokenAndPuzzleXValues_ReturnsOkResultWithCorrectValues(string token, int puzzleX)
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns((token, puzzleX));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            var result = controller.GetChallenge();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var resultValue = okResult.Value;
            var tokenProperty = resultValue.GetType().GetProperty("Token");
            var puzzleXProperty = resultValue.GetType().GetProperty("PuzzleX");

            Assert.IsNotNull(tokenProperty);
            Assert.IsNotNull(puzzleXProperty);
            Assert.AreEqual(token, tokenProperty.GetValue(resultValue));
            Assert.AreEqual(puzzleX, puzzleXProperty.GetValue(resultValue));
        }

        /// <summary>
        /// Tests that GetChallenge calls the captcha service's GenerateChallenge method exactly once.
        /// </summary>
        [TestMethod]
        public void GetChallenge_WhenCalled_CallsGenerateChallengeOnce()
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns(("testToken", 100));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            controller.GetChallenge();

            // Assert
            mockCaptchaService.Verify(s => s.GenerateChallenge(), Times.Once);
        }

        /// <summary>
        /// Tests that GetChallenge returns OkObjectResult with status code 200.
        /// </summary>
        [TestMethod]
        public void GetChallenge_WhenCalled_ReturnsOkStatusCode()
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns(("token", 150));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            var result = controller.GetChallenge();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that GetChallenge returns an anonymous object with Token and PuzzleX properties
        /// that match the values from the service.
        /// </summary>
        [TestMethod]
        public void GetChallenge_WhenCalled_ReturnsAnonymousObjectWithTokenAndPuzzleXProperties()
        {
            // Arrange
            const string expectedToken = "secureToken789";
            const int expectedPuzzleX = 175;

            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns((expectedToken, expectedPuzzleX));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            var result = controller.GetChallenge();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var resultValue = okResult.Value;

            Assert.IsNotNull(resultValue);

            var properties = resultValue.GetType().GetProperties();
            Assert.AreEqual(2, properties.Length);
            Assert.IsTrue(Array.Exists(properties, p => p.Name == "Token"));
            Assert.IsTrue(Array.Exists(properties, p => p.Name == "PuzzleX"));
        }

        /// <summary>
        /// Tests that GetChallenge handles boundary values for puzzleX including zero,
        /// negative values, and maximum integer values.
        /// </summary>
        /// <param name="puzzleX">The boundary puzzleX value to test.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(1)]
        [DataRow(999999)]
        public void GetChallenge_WithBoundaryPuzzleXValues_ReturnsCorrectPuzzleX(int puzzleX)
        {
            // Arrange
            const string token = "boundaryTestToken";

            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns((token, puzzleX));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            var result = controller.GetChallenge();

            // Assert
            var okResult = (OkObjectResult)result;
            var resultValue = okResult.Value;
            var puzzleXProperty = resultValue.GetType().GetProperty("PuzzleX");

            Assert.IsNotNull(puzzleXProperty);
            Assert.AreEqual(puzzleX, puzzleXProperty.GetValue(resultValue));
        }

        /// <summary>
        /// Tests that GetChallenge handles various token string edge cases including
        /// empty strings, whitespace, and strings with special characters.
        /// </summary>
        /// <param name="token">The edge case token value to test.</param>
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\n")]
        [DataRow("token with spaces")]
        [DataRow("token\nwith\nnewlines")]
        [DataRow("token\twith\ttabs")]
        [DataRow("!@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [DataRow("unicode-тест-测试-🔐")]
        public void GetChallenge_WithEdgeCaseTokenValues_ReturnsCorrectToken(string token)
        {
            // Arrange
            const int puzzleX = 100;

            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService
                .Setup(s => s.GenerateChallenge())
                .Returns((token, puzzleX));

            var controller = new CaptchaController(mockCaptchaService.Object);

            // Act
            var result = controller.GetChallenge();

            // Assert
            var okResult = (OkObjectResult)result;
            var resultValue = okResult.Value;
            var tokenProperty = resultValue.GetType().GetProperty("Token");

            Assert.IsNotNull(tokenProperty);
            Assert.AreEqual(token, tokenProperty.GetValue(resultValue));
        }

        /// <summary>
        /// Tests that Verify returns Success = false when the request is null.
        /// </summary>
        [TestMethod]
        public void Verify_NullRequest_ReturnsSuccessFalse()
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            var controller = new CaptchaController(mockCaptchaService.Object);
            CaptchaVerifyRequest? request = null;

            // Act
            var result = controller.Verify(request!);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsFalse(value!.Success);
        }

        /// <summary>
        /// Tests that Verify returns Success = false when the captcha service validation fails (returns null).
        /// </summary>
        [TestMethod]
        public void Verify_ValidationFails_ReturnsSuccessFalse()
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                              .Returns((string?)null);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = "test-token",
                UserX = 100,
                SolveTimeMs = 1000
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsFalse(value!.Success);

            mockCaptchaService.Verify(s => s.Validate("test-token", 100, 1000), Times.Once);
        }

        /// <summary>
        /// Tests that Verify returns Success = true with verification token when validation succeeds.
        /// </summary>
        [TestMethod]
        public void Verify_ValidationSucceeds_ReturnsSuccessTrueWithToken()
        {
            // Arrange
            var expectedToken = "verified-token-12345";
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                              .Returns(expectedToken);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = "test-token",
                UserX = 150,
                SolveTimeMs = 2000
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsTrue(value!.Success);
            Assert.AreEqual(expectedToken, value!.VerificationToken);

            mockCaptchaService.Verify(s => s.Validate("test-token", 150, 2000), Times.Once);
        }

        /// <summary>
        /// Tests that Verify correctly passes various Token edge case values to the captcha service.
        /// Validates edge cases: null token, empty string, whitespace, very long string, and special characters.
        /// </summary>
        [TestMethod]
        [DataRow(null, DisplayName = "Null token")]
        [DataRow("", DisplayName = "Empty token")]
        [DataRow("   ", DisplayName = "Whitespace token")]
        [DataRow("a very long token string with lots of characters to test boundary conditions and ensure proper handling of extended input values", DisplayName = "Very long token")]
        [DataRow("!@#$%^&*()_+-=[]{}|;':\",./<>?", DisplayName = "Special characters token")]
        public void Verify_TokenEdgeCases_PassesCorrectlyToService(string? token)
        {
            // Arrange
            var expectedVerificationToken = "verification-result";
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(token!, It.IsAny<int>(), It.IsAny<long>()))
                              .Returns(expectedVerificationToken);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = token!,
                UserX = 100,
                SolveTimeMs = 1000
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsTrue(value!.Success);
            Assert.AreEqual(expectedVerificationToken, value!.VerificationToken);

            mockCaptchaService.Verify(s => s.Validate(token!, 100, 1000), Times.Once);
        }

        /// <summary>
        /// Tests that Verify correctly passes various UserX edge case values to the captcha service.
        /// Validates edge cases: int.MinValue, int.MaxValue, 0, negative values, and positive values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, DisplayName = "int.MinValue")]
        [DataRow(int.MaxValue, DisplayName = "int.MaxValue")]
        [DataRow(0, DisplayName = "Zero")]
        [DataRow(-1, DisplayName = "Negative value")]
        [DataRow(-999999, DisplayName = "Large negative value")]
        [DataRow(1, DisplayName = "Positive value")]
        [DataRow(999999, DisplayName = "Large positive value")]
        public void Verify_UserXEdgeCases_PassesCorrectlyToService(int userX)
        {
            // Arrange
            var expectedVerificationToken = "verification-result";
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(It.IsAny<string>(), userX, It.IsAny<long>()))
                              .Returns(expectedVerificationToken);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = "test-token",
                UserX = userX,
                SolveTimeMs = 1000
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsTrue(value!.Success);
            Assert.AreEqual(expectedVerificationToken, value!.VerificationToken);

            mockCaptchaService.Verify(s => s.Validate("test-token", userX, 1000), Times.Once);
        }

        /// <summary>
        /// Tests that Verify correctly passes various SolveTimeMs edge case values to the captcha service.
        /// Validates edge cases: long.MinValue, long.MaxValue, 0, negative values, and positive values.
        /// </summary>
        [TestMethod]
        [DataRow(long.MinValue, DisplayName = "long.MinValue")]
        [DataRow(long.MaxValue, DisplayName = "long.MaxValue")]
        [DataRow(0L, DisplayName = "Zero")]
        [DataRow(-1L, DisplayName = "Negative value")]
        [DataRow(-999999999L, DisplayName = "Large negative value")]
        [DataRow(1L, DisplayName = "Positive value")]
        [DataRow(999999999L, DisplayName = "Large positive value")]
        public void Verify_SolveTimeMsEdgeCases_PassesCorrectlyToService(long solveTimeMs)
        {
            // Arrange
            var expectedVerificationToken = "verification-result";
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(It.IsAny<string>(), It.IsAny<int>(), solveTimeMs))
                              .Returns(expectedVerificationToken);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = "test-token",
                UserX = 100,
                SolveTimeMs = solveTimeMs
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsTrue(value!.Success);
            Assert.AreEqual(expectedVerificationToken, value!.VerificationToken);

            mockCaptchaService.Verify(s => s.Validate("test-token", 100, solveTimeMs), Times.Once);
        }

        /// <summary>
        /// Tests that Verify correctly handles the case where all request properties are at extreme values
        /// and validation fails (service returns null).
        /// </summary>
        [TestMethod]
        public void Verify_ExtremeValuesAndValidationFails_ReturnsSuccessFalse()
        {
            // Arrange
            var mockCaptchaService = new Mock<ISliderCaptchaService>();
            mockCaptchaService.Setup(s => s.Validate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
                              .Returns((string?)null);

            var controller = new CaptchaController(mockCaptchaService.Object);
            var request = new CaptchaVerifyRequest
            {
                Token = "",
                UserX = int.MinValue,
                SolveTimeMs = long.MaxValue
            };

            // Act
            var result = controller.Verify(request);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsFalse(value!.Success);

            mockCaptchaService.Verify(s => s.Validate("", int.MinValue, long.MaxValue), Times.Once);
        }
    }
}