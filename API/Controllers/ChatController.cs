using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Chat;

namespace NexCommerce.API.Controllers;

[Authorize]
public class ChatController(IChatService chatService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetConversations(CancellationToken ct = default)
    {
        var result = await chatService.GetConversationsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<ChatConversationDto>>.Ok(result));
    }

    [HttpGet("thread/{partnerId:guid}")]
    public async Task<IActionResult> GetConversation(
        [FromRoute] Guid partnerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await chatService.GetConversationAsync(CurrentUserId, partnerId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ChatMessageDto>>.Ok(result));
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendChatMessageRequest request,
        CancellationToken ct = default)
    {
        var result = await chatService.SendMessageAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<ChatMessageDto>.Ok(result));
    }
}
