using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NexCommerce.Application.Contracts;

namespace NexCommerce.Infrastructure.Services;

/// <summary>
/// Email dispatch via MailKit/SMTP.
/// When <c>Email:SmtpHost</c> is absent (local dev), all sends are logged only — no real mail is sent.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── Core send ─────────────────────────────────────────────────────────────
    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var host = _config["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(host))
        {
            // Dev / CI — log instead of sending
            _logger.LogInformation(
                "[EMAIL STUB] To={To} Subject={Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:SenderName"]    ?? "NexCommerce",
            _config["Email:SenderAddress"] ?? "no-reply@nexcommerce.com"));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        var port      = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
        var useSsl    = bool.TryParse(_config["Email:UseSsl"], out var s) && s;
        var socketOpt = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(host, port, socketOpt, cancellationToken);

        var username = _config["Email:Username"];
        if (!string.IsNullOrEmpty(username))
            await client.AuthenticateAsync(username, _config["Email:Password"] ?? string.Empty, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent → {Email} | Subject: {Subject}", toEmail, subject);
    }

    // ── Convenience wrappers ──────────────────────────────────────────────────
    public Task SendWelcomeEmailAsync(string toEmail, string fullName, CancellationToken cancellationToken = default)
        => SendAsync(
            toEmail, fullName,
            "Welcome to NexCommerce! 🎉",
            $"<h1>Welcome, {fullName}!</h1><p>Your account has been created successfully. Start shopping now!</p>",
            cancellationToken);

    public Task SendOrderConfirmationAsync(
        string toEmail, string fullName, Guid orderId, CancellationToken cancellationToken = default)
        => SendAsync(
            toEmail, fullName,
            $"Order Confirmed — #{orderId}",
            $"<h1>Order Confirmed ✔</h1><p>Hi {fullName}, your order <strong>#{orderId}</strong> has been placed successfully. We'll notify you when it ships.</p>",
            cancellationToken);

    public Task SendOrderStatusUpdateAsync(
        string toEmail, string fullName, Guid orderId, string status, CancellationToken cancellationToken = default)
        => SendAsync(
            toEmail, fullName,
            $"Order #{orderId} — Status Updated",
            $"<h1>Order Update</h1><p>Hi {fullName}, your order <strong>#{orderId}</strong> is now <strong>{status}</strong>.</p>",
            cancellationToken);
}
