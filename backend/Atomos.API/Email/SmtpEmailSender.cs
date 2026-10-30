using System.Net;
using System.Net.Mail;
using Atomos.Application.Common;

namespace Atomos.API.Email;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var host = configuration["Email:Smtp:Host"];
        var fromEmail = configuration["Email:Smtp:FromEmail"];
        var fromName = configuration["Email:Smtp:FromName"] ?? "Atomos";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new EmailDeliveryException("SMTP email has not been configured.");
        }

        var port = ReadInt("Email:Smtp:Port", 587);
        var enableSsl = ReadBool("Email:Smtp:EnableSsl", true);
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
        {
            mailMessage.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    message.PlainTextBody,
                    null,
                    "text/plain"));
        }

        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            smtpClient.Credentials = new NetworkCredential(username, password);
        }

        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException)
        {
            logger.LogError(exception, "Failed to send email to {EmailAddress}", message.To);
            throw new EmailDeliveryException("Could not send email.", exception);
        }
    }

    private int ReadInt(string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }

    private bool ReadBool(string key, bool fallback)
    {
        return bool.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
