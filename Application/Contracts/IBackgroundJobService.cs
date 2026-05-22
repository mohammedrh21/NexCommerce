using System.Threading.Tasks;

namespace NexCommerce.Application.Contracts;

/// <summary>
/// Defines Hangfire background recurring jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Scans for carts not updated in the last 24 hours that have items and emails the user.
    /// </summary>
    Task SendAbandonedCartEmailsAsync();

    /// <summary>
    /// Cancels orders that have been pending/unpaid for more than 24 hours.
    /// </summary>
    Task CancelUnpaidOrdersAsync();

    /// <summary>
    /// Clears stale or expired cache keys from Redis.
    /// </summary>
    Task ClearStaleRedisCacheAsync();
}
