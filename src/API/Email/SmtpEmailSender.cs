using System.Net;
using System.Net.Mail;
using Application.Authentication;

namespace API.Email;

public sealed class SmtpEmailSender(SmtpOptions options) : IEmailSender
{
    public async Task SendSignInCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken)
    {
        using var message = new MailMessage(
            new MailAddress(options.FromAddress, options.FromName),
            new MailAddress(email))
        {
            Subject = "Milliy Chat tasdiqlash kodi",
            Body = $"Tasdiqlash kodingiz: {code}. Kod {expiresAt:O} gacha amal qiladi."
        };
        using var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl,
            Credentials = new NetworkCredential(options.Username, options.Password)
        };

        using var registration = cancellationToken.Register(client.SendAsyncCancel);
        await client.SendMailAsync(message);
    }
}
