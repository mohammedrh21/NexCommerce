namespace NexCommerce.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }
}
