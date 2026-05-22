namespace NexCommerce.Application.Contracts;

public interface IChatHubService
{
    Task PushMessageAsync(Guid senderId, Guid receiverId, object messageDto, CancellationToken ct = default);
}
