using System.Text.Encodings.Web;
using System.Threading.Tasks;
using coderush.Services;

namespace coderush.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string? link) => emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link ?? string.Empty)}'>link</a>");
    }
}