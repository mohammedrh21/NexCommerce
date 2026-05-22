using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Chat;
using NexCommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NexCommerce.Application.Services;

public sealed class ChatService(
    IChatRepository chatRepository,
    IIdentityService identityService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork,
    ILogger<ChatService> logger) : IChatService
{
    public async Task<PagedResult<ChatMessageDto>> GetConversationAsync(
        Guid userId, Guid partnerId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await chatRepository.GetConversationPagedAsync(userId, partnerId, page, pageSize, ct);

        var userName = "User";
        var partnerName = "User";
        
        try
        {
            var user = await identityService.GetUserInfoAsync(userId);
            userName = user?.FullName ?? "User";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve user info for sender {UserId}", userId);
        }

        try
        {
            var partner = await identityService.GetUserInfoAsync(partnerId);
            partnerName = partner?.FullName ?? "User";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve user info for partner {PartnerId}", partnerId);
        }

        var dtos = items.Select(m => new ChatMessageDto(
            Id: m.Id,
            SenderId: m.SenderId,
            SenderName: m.SenderId == userId ? userName : partnerName,
            ReceiverId: m.ReceiverId,
            ReceiverName: m.ReceiverId == userId ? userName : partnerName,
            Message: m.Message,
            SentAt: m.SentAt
        )).ToList();

        return new PagedResult<ChatMessageDto>
        {
            Data = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var summaries = await chatRepository.GetConversationSummariesAsync(userId, ct);
        var dtos = new List<ChatConversationDto>();

        foreach (var sum in summaries)
        {
            var partnerName = "User";
            try
            {
                var userInfo = await identityService.GetUserInfoAsync(sum.PartnerId);
                partnerName = userInfo?.FullName ?? "User";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to resolve user info for conversation partner {PartnerId}", sum.PartnerId);
            }

            dtos.Add(new ChatConversationDto(
                PartnerId: sum.PartnerId,
                PartnerName: partnerName,
                LastMessage: sum.LastMessage.Message,
                LastMessageAt: sum.LastMessage.SentAt,
                UnreadCount: sum.UnreadCount
            ));
        }

        return dtos;
    }

    public async Task<ChatMessageDto> SendMessageAsync(
        Guid senderId, SendChatMessageRequest request, CancellationToken ct = default)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Message = request.Message ?? string.Empty,
            SentAt = DateTime.UtcNow
        };

        chatRepository.Add(message);
        await unitOfWork.SaveChangesAsync(ct);

        var senderName = "User";
        var receiverName = "User";

        try
        {
            var sender = await identityService.GetUserInfoAsync(senderId);
            senderName = sender?.FullName ?? "User";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve sender name for user {UserId}", senderId);
        }

        try
        {
            var receiver = await identityService.GetUserInfoAsync(request.ReceiverId);
            receiverName = receiver?.FullName ?? "User";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve receiver name for user {UserId}", request.ReceiverId);
        }

        var dto = new ChatMessageDto(
            Id: message.Id,
            SenderId: senderId,
            SenderName: senderName,
            ReceiverId: request.ReceiverId,
            ReceiverName: receiverName,
            Message: message.Message,
            SentAt: message.SentAt
        );

        // Push real-time SignalR chat update via the abstract hub service
        await chatHubService.PushMessageAsync(senderId, request.ReceiverId, dto, ct);

        return dto;
    }
}
