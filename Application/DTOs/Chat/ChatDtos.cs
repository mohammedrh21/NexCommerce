namespace NexCommerce.Application.DTOs.Chat;

public record SendChatMessageRequest(
    Guid ReceiverId,
    string Message);

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid ReceiverId,
    string ReceiverName,
    string Message,
    DateTime SentAt);

public record ChatConversationDto(
    Guid PartnerId,
    string PartnerName,
    string LastMessage,
    DateTime LastMessageAt,
    int UnreadCount);
