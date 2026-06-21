using Client.Models.Chat;
using Client.Models.Common;

namespace Client.Services.Http;

public interface IChatApiService
{
    Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(CancellationToken ct = default);
    Task<PagedResult<ChatMessageDto>> GetConversationThreadAsync(Guid partnerId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ChatMessageDto> SendMessageAsync(SendChatMessageRequest request, CancellationToken ct = default);
}

public class ChatApiService : BaseApiService, IChatApiService
{
    public ChatApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<ChatConversationDto>>("api/v1/chat", ct);
    }

    public async Task<PagedResult<ChatMessageDto>> GetConversationThreadAsync(Guid partnerId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<ChatMessageDto>>($"api/v1/chat/thread/{partnerId}?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendChatMessageRequest request, CancellationToken ct = default)
    {
        return await PostAsync<SendChatMessageRequest, ChatMessageDto>("api/v1/chat/messages", request, ct);
    }
}
