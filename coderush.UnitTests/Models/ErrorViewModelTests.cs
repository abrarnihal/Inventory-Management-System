using coderush.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace coderush.Models.UnitTests
{
    /// <summary>
    /// Tests for the ErrorViewModel class.
    /// </summary>
    [TestClass]
    public class ErrorViewModelTests
    {
        /// <summary>
        /// Tests that ShowRequestId returns the expected value based on RequestId state.
        /// Validates behavior for null, empty, whitespace, and valid RequestId values.
        /// </summary>
        /// <param name="requestId">The RequestId value to test.</param>
        /// <param name="expectedShowRequestId">The expected value of ShowRequestId.</param>
        [TestMethod]
        [DataRow(null, false, DisplayName = "ShowRequestId_RequestIdIsNull_ReturnsFalse")]
        [DataRow("", false, DisplayName = "ShowRequestId_RequestIdIsEmpty_ReturnsFalse")]
        [DataRow(" ", true, DisplayName = "ShowRequestId_RequestIdIsWhitespace_ReturnsTrue")]
        [DataRow("  ", true, DisplayName = "ShowRequestId_RequestIdIsMultipleWhitespaces_ReturnsTrue")]
        [DataRow("valid-request-id", true, DisplayName = "ShowRequestId_RequestIdIsValidString_ReturnsTrue")]
        [DataRow("123", true, DisplayName = "ShowRequestId_RequestIdIsNumericString_ReturnsTrue")]
        [DataRow("abc!@#$%^&*()_+=", true, DisplayName = "ShowRequestId_RequestIdHasSpecialCharacters_ReturnsTrue")]
        [DataRow("\t", true, DisplayName = "ShowRequestId_RequestIdIsTab_ReturnsTrue")]
        [DataRow("\n", true, DisplayName = "ShowRequestId_RequestIdIsNewline_ReturnsTrue")]
        public void ShowRequestId_VariousRequestIdValues_ReturnsExpectedResult(string? requestId, bool expectedShowRequestId)
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId!
            };

            // Act
            bool actualShowRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.AreEqual(expectedShowRequestId, actualShowRequestId);
        }

        /// <summary>
        /// Tests that ShowRequestId returns true for a very long RequestId string.
        /// Validates behavior with boundary-case string length.
        /// </summary>
        [TestMethod]
        public void ShowRequestId_RequestIdIsVeryLongString_ReturnsTrue()
        {
            // Arrange
            string veryLongRequestId = new string('a', 10000);
            var errorViewModel = new ErrorViewModel
            {
                RequestId = veryLongRequestId
            };

            // Act
            bool actualShowRequestId = errorViewModel.ShowRequestId;

            // Assert
            Assert.IsTrue(actualShowRequestId);
        }
    }
}