using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace coderush.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender(IOptions<SendGridOptions> sendGridOptions,
        IFunctional functional,
        IOptions<SmtpOptions> smtpOptions) : IEmailSender
    {
        //dependency injection
        private SendGridOptions _sendGridOptions { get; } = sendGridOptions.Value;
        private IFunctional _functional { get; } = functional;
        private SmtpOptions _smtpOptions { get; } = smtpOptions.Value;

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (!IsValidEmailAddress(email))
            {
                return;
            }

            //sendgrid is become default
            if (_sendGridOptions.IsDefault &&
                HasConfiguredValue(_sendGridOptions.SendGridKey) &&
                IsValidEmailAddress(_sendGridOptions.FromEmail))
            {
                await _functional.SendEmailBySendGridAsync(_sendGridOptions.SendGridKey,
                                                           _sendGridOptions.FromEmail,
                                                           _sendGridOptions.FromFullName,
                                                           subject,
                                                           message,
                                                           email);
            }

            //smtp is become default
            if (_smtpOptions.IsDefault &&
                HasConfiguredValue(_smtpOptions.smtpHost) &&
                _smtpOptions.smtpPort > 0 &&
                IsValidEmailAddress(_smtpOptions.fromEmail))
            {
                await _functional.SendEmailByGmailAsync(_smtpOptions.fromEmail,
                                                        _smtpOptions.fromFullName,
                                                        subject,
                                                        message,
                                                        email,
                                                        email,
                                                        _smtpOptions.smtpUserName,
                                                        _smtpOptions.smtpPassword,
                                                        _smtpOptions.smtpHost,
                                                        _smtpOptions.smtpPort,
                                                        _smtpOptions.smtpSSL);
            }
        }

        private static bool HasConfiguredValue(string value) => !string.IsNullOrWhiteSpace(value) && !value.Contains("---");

        private static bool IsValidEmailAddress(string email)
        {
            if (!HasConfiguredValue(email))
            {
                return false;
            }

            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
