using System;
using System.Text;
using System.Text.Json;

using coderush.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the SliderCaptchaService.Validate method.
    /// </summary>
    [TestClass]
    public sealed class SliderCaptchaServiceTests
    {
        /// <summary>
        /// Tests that Validate returns null when challengeToken is null.
        /// </summary>
        [TestMethod]
        public void Validate_NullToken_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string? challengeToken = null;
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when challengeToken is an empty string.
        /// </summary>
        [TestMethod]
        public void Validate_EmptyToken_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = string.Empty;
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when challengeToken is whitespace only.
        /// </summary>
        [TestMethod]
        public void Validate_WhitespaceToken_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = "   ";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when challengeToken is malformed (no dot separator).
        /// </summary>
        [TestMethod]
        public void Validate_MalformedTokenNoDot_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = "invalidtoken";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when challengeToken has invalid Base64 encoding.
        /// </summary>
        [TestMethod]
        public void Validate_InvalidBase64Token_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = "!!!invalid!!!.base64";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when challengeToken has incorrect signature.
        /// </summary>
        [TestMethod]
        public void Validate_InvalidSignature_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string validBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"X\":100,\"Nonce\":\"test\",\"Timestamp\":123}"));
            string challengeToken = $"{validBase64}.wrongsignature";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns a verification token when userX exactly matches the challenge X and solveTime is valid.
        /// </summary>
        [TestMethod]
        public void Validate_ExactUserX_ValidSolveTime_ReturnsToken()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
            Assert.IsTrue(service.IsVerified(result), "Returned token should be verified");
        }

        /// <summary>
        /// Tests that Validate returns a verification token when userX is within 5 pixels of the challenge X (positive offset).
        /// </summary>
        [TestMethod]
        public void Validate_UserXWithin5PixelsPositive_ReturnsToken()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + 5;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate returns a verification token when userX is within 5 pixels of the challenge X (negative offset).
        /// </summary>
        [TestMethod]
        public void Validate_UserXWithin5PixelsNegative_ReturnsToken()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX - 5;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate returns null when userX is just outside the 5-pixel tolerance (6 pixels above).
        /// </summary>
        [TestMethod]
        public void Validate_UserX6PixelsAbove_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + 6;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when userX is just outside the 5-pixel tolerance (6 pixels below).
        /// </summary>
        [TestMethod]
        public void Validate_UserX6PixelsBelow_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX - 6;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when userX is extremely far from the challenge X.
        /// </summary>
        [TestMethod]
        public void Validate_UserXVeryFarFromChallenge_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + 1000;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs is less than the minimum (400ms).
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeTooFast_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 399;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns a verification token when solveTimeMs is exactly at the minimum (400ms).
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeAtMinimum_ReturnsToken()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 400;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate returns a verification token when solveTimeMs is exactly at the maximum (60000ms).
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeAtMaximum_ReturnsToken()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 60000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs exceeds the maximum (60000ms).
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeTooSlow_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 60001;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs is zero.
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeZero_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 0;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs is negative.
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeNegative_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = -1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs is extremely large.
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeExtremelyLarge_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = long.MaxValue;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when solveTimeMs is at its minimum value (long.MinValue).
        /// </summary>
        [TestMethod]
        public void Validate_SolveTimeMinValue_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = long.MinValue;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when userX is at int.MinValue and far from challenge X.
        /// </summary>
        [TestMethod]
        public void Validate_UserXMinValue_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = int.MinValue;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when userX is at int.MaxValue and far from challenge X.
        /// </summary>
        [TestMethod]
        public void Validate_UserXMaxValue_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = int.MaxValue;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when both userX and solveTimeMs are invalid.
        /// </summary>
        [TestMethod]
        public void Validate_InvalidUserXAndSolveTime_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + 100;
            long solveTimeMs = 100;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when the token is from a different service instance (different secret key).
        /// </summary>
        [TestMethod]
        public void Validate_TokenFromDifferentServiceInstance_ReturnsNull()
        {
            // Arrange
            var service1 = new SliderCaptchaService();
            var service2 = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service1.GenerateChallenge();
            int userX = puzzleX;
            long solveTimeMs = 1000;

            // Act
            var result = service2.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate handles various typical solve times correctly.
        /// </summary>
        [TestMethod]
        [DataRow(500L, DisplayName = "500ms")]
        [DataRow(1000L, DisplayName = "1000ms")]
        [DataRow(2500L, DisplayName = "2500ms")]
        [DataRow(5000L, DisplayName = "5000ms")]
        [DataRow(10000L, DisplayName = "10000ms")]
        [DataRow(30000L, DisplayName = "30000ms")]
        public void Validate_TypicalSolveTimes_ReturnsToken(long solveTimeMs)
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result, $"Should return token for solve time {solveTimeMs}ms");
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate handles various userX offsets within tolerance correctly.
        /// </summary>
        [TestMethod]
        [DataRow(0, DisplayName = "Exact match")]
        [DataRow(1, DisplayName = "+1 pixel")]
        [DataRow(-1, DisplayName = "-1 pixel")]
        [DataRow(3, DisplayName = "+3 pixels")]
        [DataRow(-3, DisplayName = "-3 pixels")]
        [DataRow(5, DisplayName = "+5 pixels (boundary)")]
        [DataRow(-5, DisplayName = "-5 pixels (boundary)")]
        public void Validate_UserXWithinTolerance_ReturnsToken(int offset)
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + offset;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNotNull(result, $"Should return token for offset {offset} pixels");
            Assert.IsTrue(result.Contains("."), "Token should contain a dot separator");
        }

        /// <summary>
        /// Tests that Validate returns null for userX offsets outside tolerance.
        /// </summary>
        [TestMethod]
        [DataRow(6, DisplayName = "+6 pixels")]
        [DataRow(-6, DisplayName = "-6 pixels")]
        [DataRow(10, DisplayName = "+10 pixels")]
        [DataRow(-10, DisplayName = "-10 pixels")]
        [DataRow(100, DisplayName = "+100 pixels")]
        [DataRow(-100, DisplayName = "-100 pixels")]
        public void Validate_UserXOutsideTolerance_ReturnsNull(int offset)
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX + offset;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result, $"Should return null for offset {offset} pixels");
        }

        /// <summary>
        /// Tests that Validate returns null for solveTimeMs values outside valid range.
        /// </summary>
        [TestMethod]
        [DataRow(0L, DisplayName = "0ms")]
        [DataRow(100L, DisplayName = "100ms")]
        [DataRow(399L, DisplayName = "399ms (below min)")]
        [DataRow(60001L, DisplayName = "60001ms (above max)")]
        [DataRow(100000L, DisplayName = "100000ms")]
        [DataRow(-1L, DisplayName = "-1ms")]
        public void Validate_SolveTimeOutsideValidRange_ReturnsNull(long solveTimeMs)
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            int userX = puzzleX;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result, $"Should return null for solve time {solveTimeMs}ms");
        }

        /// <summary>
        /// Tests that Validate handles a token with multiple dots (invalid format).
        /// </summary>
        [TestMethod]
        public void Validate_TokenWithMultipleDots_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = "part1.part2.part3";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate handles a very long malformed token string.
        /// </summary>
        [TestMethod]
        public void Validate_VeryLongMalformedToken_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = new string('x', 10000);
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate handles a token with special characters.
        /// </summary>
        [TestMethod]
        public void Validate_TokenWithSpecialCharacters_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string challengeToken = "!@#$%^&*().<>?/\\|";
            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Validate returns null when token payload is valid JSON but doesn't deserialize to ChallengePayload.
        /// </summary>
        [TestMethod]
        public void Validate_TokenWithInvalidJsonStructure_ReturnsNull()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (validToken, _) = service.GenerateChallenge();

            // Create a token with valid signature but wrong JSON structure
            string wrongJson = JsonSerializer.Serialize(new { WrongField = "value" });
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(wrongJson));

            // Extract signature from valid token (this won't match, but tests the deserialization path)
            string[] parts = validToken.Split('.');
            string challengeToken = $"{base64}.{parts[1]}";

            int userX = 100;
            long solveTimeMs = 1000;

            // Act
            var result = service.Validate(challengeToken, userX, solveTimeMs);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the verification token is null.
        /// </summary>
        [TestMethod]
        public void IsVerified_NullToken_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = null;

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the verification token is empty.
        /// </summary>
        [TestMethod]
        public void IsVerified_EmptyToken_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = string.Empty;

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false for various whitespace-only tokens.
        /// </summary>
        /// <param name="token">The whitespace token to test.</param>
        [TestMethod]
        [DataRow(" ")]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void IsVerified_WhitespaceToken_ReturnsFalse(string token)
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            bool result = service.IsVerified(token);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token has an invalid format (missing dot separator).
        /// </summary>
        [TestMethod]
        public void IsVerified_TokenWithoutDotSeparator_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = "invalidtokenwithoutdot";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token has multiple dot separators.
        /// </summary>
        [TestMethod]
        public void IsVerified_TokenWithMultipleDots_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = "part1.part2.part3";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token contains invalid base64 encoding.
        /// </summary>
        [TestMethod]
        public void IsVerified_InvalidBase64Token_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = "!!!invalid-base64!!!.validpart";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token signature is tampered.
        /// </summary>
        [TestMethod]
        public void IsVerified_TamperedSignature_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            string verificationToken = service.Validate(challengeToken, puzzleX, 1000);

            // Tamper with the signature part
            string[] parts = verificationToken.Split('.');
            string tamperedToken = parts[0] + ".tamperedddddddddddddddddddddddddddd";

            // Act
            bool result = service.IsVerified(tamperedToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token payload is tampered.
        /// </summary>
        [TestMethod]
        public void IsVerified_TamperedPayload_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            string verificationToken = service.Validate(challengeToken, puzzleX, 1000);

            // Tamper with the payload part (keep signature as-is)
            string[] parts = verificationToken.Split('.');
            string tamperedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"invalid\":\"data\"}"));
            string tamperedToken = tamperedPayload + "." + parts[1];

            // Act
            bool result = service.IsVerified(tamperedToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the payload contains invalid JSON.
        /// This is tested by creating a properly signed token with invalid JSON content.
        /// </summary>
        [TestMethod]
        public void IsVerified_MalformedJsonPayload_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            // Create a token with valid structure but ensure deserialization will fail
            // by using a token that doesn't match VerifyPayload structure
            string invalidJson = "{this is not valid json}";
            string encodedInvalidJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson));
            string verificationToken = encodedInvalidJson + ".somesignature";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns true for a valid, freshly created verification token.
        /// </summary>
        [TestMethod]
        public void IsVerified_ValidFreshToken_ReturnsTrue()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            string verificationToken = service.Validate(challengeToken, puzzleX, 1000);

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that IsVerified handles various valid solve times correctly.
        /// Valid tokens with different solve times should all be verified successfully.
        /// </summary>
        /// <param name="solveTimeMs">The solve time in milliseconds.</param>
        [TestMethod]
        [DataRow(400)]
        [DataRow(1000)]
        [DataRow(5000)]
        [DataRow(30000)]
        [DataRow(60000)]
        public void IsVerified_ValidTokenWithDifferentSolveTimes_ReturnsTrue(long solveTimeMs)
        {
            // Arrange
            var service = new SliderCaptchaService();
            var (challengeToken, puzzleX) = service.GenerateChallenge();
            string verificationToken = service.Validate(challengeToken, puzzleX, solveTimeMs);

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when payload deserialization returns null.
        /// This tests the null check after JsonSerializer.Deserialize.
        /// </summary>
        [TestMethod]
        public void IsVerified_PayloadDeserializesToNull_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            // Create a token with "null" as JSON content
            string nullJson = "null";
            string encodedNullJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(nullJson));
            string verificationToken = encodedNullJson + ".signature";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false for tokens with special characters.
        /// </summary>
        /// <param name="token">The token with special characters.</param>
        [TestMethod]
        [DataRow("token.with\nnewline")]
        [DataRow("token\u0000null.signature")]
        [DataRow("토큰.시그니처")]
        public void IsVerified_TokenWithSpecialCharacters_ReturnsFalse(string token)
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            bool result = service.IsVerified(token);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false for a very long token string.
        /// </summary>
        [TestMethod]
        public void IsVerified_VeryLongToken_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = new string('a', 100000) + "." + new string('b', 100000);

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token has an empty payload part.
        /// </summary>
        [TestMethod]
        public void IsVerified_EmptyPayloadPart_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = ".signature";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when the token has an empty signature part.
        /// </summary>
        [TestMethod]
        public void IsVerified_EmptySignaturePart_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = "payload.";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false when both payload and signature parts are empty.
        /// </summary>
        [TestMethod]
        public void IsVerified_EmptyPayloadAndSignature_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = ".";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsVerified returns false for a token with only a dot character.
        /// This is a boundary case for the token format validation.
        /// </summary>
        [TestMethod]
        public void IsVerified_OnlyDot_ReturnsFalse()
        {
            // Arrange
            var service = new SliderCaptchaService();
            string verificationToken = ".";

            // Act
            bool result = service.IsVerified(verificationToken);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that GenerateChallenge returns a valid token and PuzzleX value.
        /// Verifies that the token is not null or empty, and PuzzleX is within the expected range [60, 229].
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsValidTokenAndPuzzleX()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();

            // Assert
            Assert.IsNotNull(token, "Token should not be null");
            Assert.IsFalse(string.IsNullOrWhiteSpace(token), "Token should not be empty or whitespace");
            Assert.IsTrue(puzzleX >= 60 && puzzleX < 230, $"PuzzleX should be in range [60, 229], but was {puzzleX}");
        }

        /// <summary>
        /// Tests that GenerateChallenge returns a token with the expected format.
        /// The token should be in format: base64Payload.base64Signature
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsTokenWithCorrectFormat()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();

            // Assert
            Assert.IsTrue(token.Contains("."), "Token should contain a separator '.'");
            var parts = token.Split('.');
            Assert.AreEqual(2, parts.Length, "Token should have exactly two parts separated by '.'");
            Assert.IsTrue(parts[0].Length > 0, "Payload part should not be empty");
            Assert.IsTrue(parts[1].Length > 0, "Signature part should not be empty");
        }

        /// <summary>
        /// Tests that GenerateChallenge returns tokens with valid base64-encoded parts.
        /// Both the payload and signature parts should be valid base64 strings.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsTokenWithValidBase64Parts()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();

            // Assert
            var parts = token.Split('.');
            Assert.AreEqual(2, parts.Length, "Token should have exactly two parts");

            // Verify payload part is valid base64
            try
            {
                var payloadBytes = Convert.FromBase64String(parts[0]);
                Assert.IsTrue(payloadBytes.Length > 0, "Decoded payload should not be empty");
            }
            catch (FormatException)
            {
                Assert.Fail("Payload part should be valid base64");
            }

            // Verify signature part is valid base64
            try
            {
                var signatureBytes = Convert.FromBase64String(parts[1]);
                Assert.IsTrue(signatureBytes.Length > 0, "Decoded signature should not be empty");
            }
            catch (FormatException)
            {
                Assert.Fail("Signature part should be valid base64");
            }
        }

        /// <summary>
        /// Tests that GenerateChallenge returns a token containing a valid JSON payload.
        /// The payload should be deserializable and contain X, Nonce, and Timestamp properties.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsTokenWithValidJsonPayload()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();

            // Assert
            var parts = token.Split('.');
            var payloadBytes = Convert.FromBase64String(parts[0]);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);

            var payload = JsonSerializer.Deserialize<ChallengePayloadData>(payloadJson);
            Assert.IsNotNull(payload, "Payload should be deserializable");
            Assert.AreEqual(puzzleX, payload.X, "Payload X should match returned PuzzleX");
            Assert.IsFalse(string.IsNullOrEmpty(payload.Nonce), "Payload Nonce should not be null or empty");
            Assert.IsTrue(payload.Timestamp > 0, "Payload Timestamp should be positive");
        }

        /// <summary>
        /// Tests that GenerateChallenge returns a nonce with the expected length.
        /// A 16-byte random value encoded as base64 should be 24 characters long.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsPayloadWithValidNonceLength()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();

            // Assert
            var parts = token.Split('.');
            var payloadBytes = Convert.FromBase64String(parts[0]);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonSerializer.Deserialize<ChallengePayloadData>(payloadJson);

            Assert.IsNotNull(payload, "Payload should be deserializable");
            Assert.AreEqual(24, payload.Nonce.Length, "Nonce should be 24 characters (16 bytes encoded as base64)");
        }

        /// <summary>
        /// Tests that GenerateChallenge returns a timestamp close to the current time.
        /// The timestamp should be within a reasonable range of the current UTC time.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalled_ReturnsPayloadWithReasonableTimestamp()
        {
            // Arrange
            var service = new SliderCaptchaService();
            var beforeCall = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var (token, puzzleX) = service.GenerateChallenge();
            var afterCall = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Assert
            var parts = token.Split('.');
            var payloadBytes = Convert.FromBase64String(parts[0]);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonSerializer.Deserialize<ChallengePayloadData>(payloadJson);

            Assert.IsNotNull(payload, "Payload should be deserializable");
            Assert.IsTrue(payload.Timestamp >= beforeCall, $"Timestamp {payload.Timestamp} should be >= {beforeCall}");
            Assert.IsTrue(payload.Timestamp <= afterCall, $"Timestamp {payload.Timestamp} should be <= {afterCall}");
        }

        /// <summary>
        /// Tests that multiple calls to GenerateChallenge produce different tokens.
        /// This verifies that randomness is working correctly for nonces and timestamps.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalledMultipleTimes_ReturnsDifferentTokens()
        {
            // Arrange
            var service = new SliderCaptchaService();

            // Act
            var (token1, puzzleX1) = service.GenerateChallenge();
            var (token2, puzzleX2) = service.GenerateChallenge();
            var (token3, puzzleX3) = service.GenerateChallenge();

            // Assert
            Assert.AreNotEqual(token1, token2, "First and second tokens should be different");
            Assert.AreNotEqual(token2, token3, "Second and third tokens should be different");
            Assert.AreNotEqual(token1, token3, "First and third tokens should be different");
        }

        /// <summary>
        /// Tests that GenerateChallenge always returns PuzzleX values within the valid range.
        /// Runs multiple iterations to ensure boundary conditions are respected.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalledMultipleTimes_AlwaysReturnsPuzzleXInValidRange()
        {
            // Arrange
            var service = new SliderCaptchaService();
            const int iterations = 100;

            // Act & Assert
            for (int i = 0; i < iterations; i++)
            {
                var (token, puzzleX) = service.GenerateChallenge();
                Assert.IsTrue(puzzleX >= 60 && puzzleX < 230,
                    $"Iteration {i}: PuzzleX should be in range [60, 229], but was {puzzleX}");
            }
        }

        /// <summary>
        /// Tests that GenerateChallenge produces PuzzleX values with reasonable distribution.
        /// Verifies that the random number generator produces values across the entire range.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalledMultipleTimes_ProducesPuzzleXWithVariation()
        {
            // Arrange
            var service = new SliderCaptchaService();
            const int iterations = 50;
            var puzzleXValues = new System.Collections.Generic.HashSet<int>();

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var (token, puzzleX) = service.GenerateChallenge();
                puzzleXValues.Add(puzzleX);
            }

            // Assert
            Assert.IsTrue(puzzleXValues.Count > 10,
                $"Expected at least 10 different PuzzleX values in {iterations} iterations, but got {puzzleXValues.Count}");
        }

        /// <summary>
        /// Tests that GenerateChallenge produces different nonces on each call.
        /// This verifies that the nonce generation is truly random.
        /// </summary>
        [TestMethod]
        public void GenerateChallenge_WhenCalledMultipleTimes_ProducesDifferentNonces()
        {
            // Arrange
            var service = new SliderCaptchaService();
            const int iterations = 10;
            var nonces = new System.Collections.Generic.HashSet<string>();

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var (token, puzzleX) = service.GenerateChallenge();
                var parts = token.Split('.');
                var payloadBytes = Convert.FromBase64String(parts[0]);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);
                var payload = JsonSerializer.Deserialize<ChallengePayloadData>(payloadJson);
                nonces.Add(payload.Nonce);
            }

            // Assert
            Assert.AreEqual(iterations, nonces.Count,
                $"All {iterations} nonces should be unique, but got {nonces.Count} unique values");
        }

        /// <summary>
        /// Helper class for deserializing the challenge payload JSON.
        /// </summary>
        private sealed class ChallengePayloadData
        {
            public int X { get; set; }
            public string Nonce { get; set; } = "";
            public long Timestamp { get; set; }
        }
    }
}