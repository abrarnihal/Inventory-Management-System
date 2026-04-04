using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using coderush.Extensions;
using coderush.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace coderush.Extensions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="EmailSenderExtensions"/> class.
    /// </summary>
    [TestClass]
    public class EmailSenderExtensionsTests
    {
        /// <summary>
        /// Tests that SendEmailConfirmationAsync calls SendEmailAsync with correct parameters for valid inputs.
        /// </summary>
        /// <param name="email">The email address to test.</param>
        /// <param name="link">The confirmation link to test.</param>
        [TestMethod]
        [DataRow("test@example.com", "https://example.com/confirm")]
        [DataRow("user@domain.co.uk", "http://localhost/verify")]
        [DataRow("admin@test.org", "https://example.com/path?param=value")]
        public async Task SendEmailConfirmationAsync_ValidInputs_CallsSendEmailAsyncWithCorrectParameters(string email, string link)
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            Task result = mockEmailSender.Object.SendEmailConfirmationAsync(email, link);
            await result;

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync properly HTML-encodes special characters in the link.
        /// </summary>
        /// <param name="link">The link containing special characters.</param>
        /// <param name="expectedEncoded">The expected HTML-encoded link.</param>
        [TestMethod]
        [DataRow("https://example.com?param=<script>", "https://example.com?param=&lt;script&gt;")]
        [DataRow("https://example.com?a=1&b=2", "https://example.com?a=1&amp;b=2")]
        [DataRow("https://example.com?quote=\"test\"", "https://example.com?quote=&quot;test&quot;")]
        [DataRow("https://example.com?apostrophe='test'", "https://example.com?apostrophe=&#x27;test&#x27;")]
        public async Task SendEmailConfirmationAsync_LinkWithSpecialCharacters_EncodesLinkProperly(string link, string expectedEncoded)
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "test@example.com";
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncoded}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles null email parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_NullEmail_CallsSendEmailAsyncWithNull()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string? email = null;
            string link = "https://example.com/confirm";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email!, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email!, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email!, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles empty email parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_EmptyEmail_CallsSendEmailAsyncWithEmptyString()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = string.Empty;
            string link = "https://example.com/confirm";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles whitespace-only email parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_WhitespaceEmail_CallsSendEmailAsyncWithWhitespace()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "   ";
            string link = "https://example.com/confirm";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles null link parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_NullLink_CallsSendEmailAsyncWithNullEncoded()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "test@example.com";
            string? link = null;
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link ?? string.Empty);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles empty link parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_EmptyLink_CallsSendEmailAsyncWithEmptyEncodedLink()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "test@example.com";
            string link = string.Empty;
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles whitespace-only link parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_WhitespaceLink_CallsSendEmailAsyncWithWhitespaceEncoded()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "test@example.com";
            string link = "   ";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles very long email and link strings.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_VeryLongStrings_CallsSendEmailAsyncWithLongStrings()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = new string('a', 1000) + "@example.com";
            string link = "https://example.com/" + new string('b', 1000);
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync returns the Task returned by SendEmailAsync.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_ValidInputs_ReturnsTaskFromSendEmailAsync()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "test@example.com";
            string link = "https://example.com/confirm";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";
            Task expectedTask = Task.CompletedTask;

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(expectedTask);

            // Act
            Task result = mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            Assert.AreSame(expectedTask, result);
        }

        /// <summary>
        /// Tests that SendEmailConfirmationAsync handles special email characters that don't need encoding in email parameter.
        /// </summary>
        [TestMethod]
        public async Task SendEmailConfirmationAsync_EmailWithSpecialCharacters_CallsSendEmailAsyncWithSpecialCharacters()
        {
            // Arrange
            Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
            string email = "user+test@sub-domain.example.com";
            string link = "https://example.com/confirm";
            string expectedEncodedLink = HtmlEncoder.Default.Encode(link);
            string expectedMessage = $"Please confirm your account by clicking this link: <a href='{expectedEncodedLink}'>link</a>";

            mockEmailSender.Setup(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailConfirmationAsync(email, link);

            // Assert
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Confirm your email", expectedMessage), Times.Once);
        }
    }
}