#nullable enable

using System;
using System.Threading.Tasks;
using coderush.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the EmailSender class.
    /// </summary>
    [TestClass]
    public class EmailSenderTests
    {
        /// <summary>
        /// Tests that SendEmailAsync returns early without calling any email service when the email address is invalid.
        /// Validates various invalid email formats including null, empty, whitespace, contains "---", and malformed addresses.
        /// </summary>
        /// <param name="invalidEmail">The invalid email address to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null email")]
        [DataRow("", DisplayName = "Empty email")]
        [DataRow("   ", DisplayName = "Whitespace email")]
        [DataRow("---", DisplayName = "Email with ---")]
        [DataRow("test---@example.com", DisplayName = "Email containing ---")]
        [DataRow("notanemail", DisplayName = "Malformed email without @")]
        [DataRow("@nodomain.com", DisplayName = "Email missing local part")]
        [DataRow("missingdomain@", DisplayName = "Email missing domain")]
        public async Task SendEmailAsync_InvalidEmail_ReturnsWithoutCallingServices(string? invalidEmail)
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-key",
                FromEmail = "sender@example.com",
                FromFullName = "Sender Name"
            });

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "user",
                smtpPassword = "pass",
                smtpSSL = true
            });

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync(invalidEmail!, "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call any email service when neither SendGrid nor SMTP is configured as default.
        /// </summary>
        [TestMethod]
        public async Task SendEmailAsync_NoServicesConfiguredAsDefault_DoesNotCallAnyService()
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions
            {
                IsDefault = false,
                SendGridKey = "valid-key",
                FromEmail = "sender@example.com",
                FromFullName = "Sender Name"
            });

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions
            {
                IsDefault = false,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "user",
                smtpPassword = "pass",
                smtpSSL = true
            });

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync successfully calls SendGrid service when properly configured.
        /// Validates that the correct parameters are passed to SendEmailBySendGridAsync.
        /// </summary>
        [TestMethod]
        public async Task SendEmailAsync_SendGridConfiguredProperly_CallsSendGridService()
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                "valid-sendgrid-key",
                "sendgrid@example.com",
                "SendGrid Sender",
                "Test Subject",
                "Test Message",
                "recipient@example.com"), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call SendGrid service when SendGrid key is not configured or contains "---".
        /// </summary>
        /// <param name="sendGridKey">The SendGrid key value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null SendGrid key")]
        [DataRow("", DisplayName = "Empty SendGrid key")]
        [DataRow("   ", DisplayName = "Whitespace SendGrid key")]
        [DataRow("---", DisplayName = "SendGrid key with ---")]
        [DataRow("---invalid---", DisplayName = "SendGrid key containing ---")]
        public async Task SendEmailAsync_SendGridKeyInvalid_DoesNotCallSendGridService(string? sendGridKey)
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = sendGridKey!,
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call SendGrid service when SendGrid FromEmail is invalid.
        /// </summary>
        /// <param name="fromEmail">The invalid FromEmail value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null FromEmail")]
        [DataRow("", DisplayName = "Empty FromEmail")]
        [DataRow("   ", DisplayName = "Whitespace FromEmail")]
        [DataRow("invalid-email", DisplayName = "Malformed FromEmail")]
        [DataRow("@nodomain.com", DisplayName = "FromEmail missing local part")]
        public async Task SendEmailAsync_SendGridFromEmailInvalid_DoesNotCallSendGridService(string? fromEmail)
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = fromEmail!,
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync successfully calls SMTP service when properly configured.
        /// Validates that the correct parameters are passed to SendEmailByGmailAsync.
        /// </summary>
        [TestMethod]
        public async Task SendEmailAsync_SmtpConfiguredProperly_CallsSmtpService()
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions { IsDefault = false });

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = true
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                "smtp@example.com",
                "SMTP Sender",
                "Test Subject",
                "Test Message",
                "recipient@example.com",
                "recipient@example.com",
                "smtpuser",
                "smtppass",
                "smtp.example.com",
                587,
                true), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call SMTP service when SMTP host is not configured or contains "---".
        /// </summary>
        /// <param name="smtpHost">The SMTP host value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null SMTP host")]
        [DataRow("", DisplayName = "Empty SMTP host")]
        [DataRow("   ", DisplayName = "Whitespace SMTP host")]
        [DataRow("---", DisplayName = "SMTP host with ---")]
        [DataRow("---invalid---", DisplayName = "SMTP host containing ---")]
        public async Task SendEmailAsync_SmtpHostInvalid_DoesNotCallSmtpService(string? smtpHost)
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions { IsDefault = false });

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = smtpHost!,
                smtpPort = 587,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = true
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call SMTP service when SMTP port is invalid (0 or negative).
        /// </summary>
        /// <param name="smtpPort">The SMTP port value to test.</param>
        [TestMethod]
        [DataRow(0, DisplayName = "SMTP port is 0")]
        [DataRow(-1, DisplayName = "SMTP port is -1")]
        [DataRow(-100, DisplayName = "SMTP port is -100")]
        [DataRow(int.MinValue, DisplayName = "SMTP port is int.MinValue")]
        public async Task SendEmailAsync_SmtpPortInvalid_DoesNotCallSmtpService(int smtpPort)
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions { IsDefault = false });

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = smtpPort,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = true
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync does not call SMTP service when SMTP fromEmail is invalid.
        /// </summary>
        /// <param name="fromEmail">The invalid fromEmail value to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null fromEmail")]
        [DataRow("", DisplayName = "Empty fromEmail")]
        [DataRow("   ", DisplayName = "Whitespace fromEmail")]
        [DataRow("invalid-email", DisplayName = "Malformed fromEmail")]
        [DataRow("@nodomain.com", DisplayName = "fromEmail missing local part")]
        public async Task SendEmailAsync_SmtpFromEmailInvalid_DoesNotCallSmtpService(string? fromEmail)
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions { IsDefault = false });

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                fromEmail = fromEmail!,
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = true
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that SendEmailAsync calls both SendGrid and SMTP services when both are properly configured.
        /// Both services should execute independently.
        /// </summary>
        [TestMethod]
        public async Task SendEmailAsync_BothServicesConfigured_CallsBothServices()
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = false
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            functionalMock.Setup(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                "valid-sendgrid-key",
                "sendgrid@example.com",
                "SendGrid Sender",
                "Test Subject",
                "Test Message",
                "recipient@example.com"), Times.Once);
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                "smtp@example.com",
                "SMTP Sender",
                "Test Subject",
                "Test Message",
                "recipient@example.com",
                "recipient@example.com",
                "smtpuser",
                "smtppass",
                "smtp.example.com",
                587,
                false), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync handles valid boundary SMTP port values correctly.
        /// </summary>
        /// <param name="smtpPort">The SMTP port value to test.</param>
        [TestMethod]
        [DataRow(1, DisplayName = "SMTP port is 1 (minimum valid)")]
        [DataRow(25, DisplayName = "SMTP port is 25 (standard SMTP)")]
        [DataRow(465, DisplayName = "SMTP port is 465 (SMTPS)")]
        [DataRow(587, DisplayName = "SMTP port is 587 (submission)")]
        [DataRow(65535, DisplayName = "SMTP port is 65535 (maximum)")]
        [DataRow(int.MaxValue, DisplayName = "SMTP port is int.MaxValue")]
        public async Task SendEmailAsync_SmtpPortBoundaryValues_CallsSmtpServiceForPositiveValues(int smtpPort)
        {
            // Arrange
            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(new SendGridOptions { IsDefault = false });

            var smtpOptions = new SmtpOptions
            {
                IsDefault = true,
                smtpHost = "smtp.example.com",
                smtpPort = smtpPort,
                fromEmail = "smtp@example.com",
                fromFullName = "SMTP Sender",
                smtpUserName = "smtpuser",
                smtpPassword = "smtppass",
                smtpSSL = true
            };

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(smtpOptions);

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailByGmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailByGmailAsync(
                "smtp@example.com",
                "SMTP Sender",
                "Test Subject",
                "Test Message",
                "recipient@example.com",
                "recipient@example.com",
                "smtpuser",
                "smtppass",
                "smtp.example.com",
                smtpPort,
                true), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync handles edge case values for subject and message parameters.
        /// The method should accept and pass through these values regardless of their content.
        /// </summary>
        /// <param name="subject">The subject value to test.</param>
        /// <param name="message">The message value to test.</param>
        [TestMethod]
        [DataRow(null, null, DisplayName = "Null subject and message")]
        [DataRow("", "", DisplayName = "Empty subject and message")]
        [DataRow("   ", "   ", DisplayName = "Whitespace subject and message")]
        [DataRow("Special <>&\"'", "Special <>&\"'", DisplayName = "Special characters")]
        public async Task SendEmailAsync_SubjectAndMessageEdgeCases_PassesParametersCorrectly(string? subject, string? message)
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", subject!, message!);

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                "valid-sendgrid-key",
                "sendgrid@example.com",
                "SendGrid Sender",
                subject!,
                message!,
                "recipient@example.com"), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync correctly handles a very long subject and message.
        /// The method should pass these through without truncation or error.
        /// </summary>
        [TestMethod]
        public async Task SendEmailAsync_VeryLongSubjectAndMessage_PassesParametersCorrectly()
        {
            // Arrange
            var longSubject = new string('S', 10000);
            var longMessage = new string('M', 50000);

            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync("recipient@example.com", longSubject, longMessage);

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                "valid-sendgrid-key",
                "sendgrid@example.com",
                "SendGrid Sender",
                longSubject,
                longMessage,
                "recipient@example.com"), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmailAsync correctly handles valid email addresses with various formats.
        /// </summary>
        /// <param name="recipientEmail">The valid email address to test.</param>
        [TestMethod]
        [DataRow("user@example.com", DisplayName = "Simple email")]
        [DataRow("user.name@example.com", DisplayName = "Email with dot in local part")]
        [DataRow("user+tag@example.com", DisplayName = "Email with plus sign")]
        [DataRow("user_name@example.com", DisplayName = "Email with underscore")]
        [DataRow("user@subdomain.example.com", DisplayName = "Email with subdomain")]
        [DataRow("user@example.co.uk", DisplayName = "Email with country code domain")]
        public async Task SendEmailAsync_ValidEmailFormats_CallsServices(string recipientEmail)
        {
            // Arrange
            var sendGridOptions = new SendGridOptions
            {
                IsDefault = true,
                SendGridKey = "valid-sendgrid-key",
                FromEmail = "sendgrid@example.com",
                FromFullName = "SendGrid Sender"
            };

            var sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            sendGridOptionsMock.Setup(x => x.Value).Returns(sendGridOptions);

            var smtpOptionsMock = new Mock<IOptions<SmtpOptions>>();
            smtpOptionsMock.Setup(x => x.Value).Returns(new SmtpOptions { IsDefault = false });

            var functionalMock = new Mock<IFunctional>();
            functionalMock.Setup(x => x.SendEmailBySendGridAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var emailSender = new EmailSender(sendGridOptionsMock.Object, functionalMock.Object, smtpOptionsMock.Object);

            // Act
            await emailSender.SendEmailAsync(recipientEmail, "Test Subject", "Test Message");

            // Assert
            functionalMock.Verify(x => x.SendEmailBySendGridAsync(
                "valid-sendgrid-key",
                "sendgrid@example.com",
                "SendGrid Sender",
                "Test Subject",
                "Test Message",
                recipientEmail), Times.Once);
        }
    }
}