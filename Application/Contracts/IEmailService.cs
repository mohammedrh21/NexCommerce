namespace NexCommerce.Application.Contracts;

/// <summary>Email dispatch — implemented in Infrastructure using MailKit.</summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string fullName, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string toEmail, string fullName, Guid orderId, CancellationToken cancellationToken = default);
    Task SendOrderStatusUpdateAsync(string toEmail, string fullName, Guid orderId, string status, CancellationToken cancellationToken = default);
}
