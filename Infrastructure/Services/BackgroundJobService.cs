using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using NexCommerce.Application.Contracts;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;
using NexCommerce.Infrastructure.Persistence;

namespace NexCommerce.Infrastructure.Services;

/// <summary>
/// Implementation of Hangfire recurring background jobs.
/// </summary>
public sealed class BackgroundJobService(
    NexCommerceDbContext db,
    IIdentityService identityService,
    IEmailService emailService,
    IConnectionMultiplexer redis,
    INotificationService notificationService,
    ILogger<BackgroundJobService> logger)
    : IBackgroundJobService
{
    /// <inheritdoc />
    public async Task SendAbandonedCartEmailsAsync()
    {
        logger.LogInformation("Running SendAbandonedCartEmailsAsync background job.");
        try
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

            // Find carts with items that haven't been updated in 24-48 hours
            var abandonedCarts = await db.Carts
                .Include(c => c.Items)
                .Where(c => c.Items.Any() && c.UpdatedAt <= oneDayAgo && c.UpdatedAt >= twoDaysAgo)
                .ToListAsync();

            logger.LogInformation("Found {Count} abandoned carts to process.", abandonedCarts.Count);

            foreach (var cart in abandonedCarts)
            {
                try
                {
                    var user = await identityService.GetUserInfoAsync(cart.UserId);
                    if (user is not null)
                    {
                        var subject = "You left items in your cart!";
                        var body = $"<p>Hello {user.FullName},</p><p>You left some items in your NexCommerce shopping cart! Don't miss out, visit your cart now and finish checking out.</p>";
                        
                        await emailService.SendAsync(user.Email, user.FullName, subject, body);
                        logger.LogInformation("Sent abandoned cart email to {Email} for cart {CartId}.", user.Email, cart.Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send abandoned cart email for cart {CartId}", cart.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing abandoned cart emails");
        }
    }

    /// <inheritdoc />
    public async Task CancelUnpaidOrdersAsync()
    {
        logger.LogInformation("Running CancelUnpaidOrdersAsync background job.");
        try
        {
            var threshold = DateTime.UtcNow.AddDays(-1);

            // Find orders in Pending status that are older than 24 hours
            var unpaidOrders = await db.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .Include(o => o.Payment)
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt <= threshold)
                .ToListAsync();

            logger.LogInformation("Found {Count} unpaid orders to cancel.", unpaidOrders.Count);

            if (unpaidOrders.Any())
            {
                // Collect variant IDs to restore stock
                var variantIds = unpaidOrders.SelectMany(o => o.Items).Select(i => i.ProductVariantId).Distinct().ToList();
                var variants = await db.ProductVariants
                    .Where(v => variantIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var order in unpaidOrders)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        OrderId   = order.Id,
                        Status    = OrderStatus.Cancelled,
                        Note      = "Order cancelled automatically due to non-payment within 24 hours.",
                        ChangedAt = DateTime.UtcNow
                    });

                    // Restore stock
                    foreach (var item in order.Items)
                    {
                        var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                        if (variant is not null)
                        {
                            variant.StockQuantity += item.Quantity;
                        }
                    }

                    // Send notification to customer
                    try
                    {
                        await notificationService.SendAsync(order.UserId, NotificationType.OrderUpdate,
                            $"Your order #{order.Id.ToString()[..8]} was cancelled automatically because it remained unpaid for over 24 hours.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send notification for automatically cancelled order {OrderId}", order.Id);
                    }

                    logger.LogInformation("Automatically cancelled order {OrderId} due to non-payment.", order.Id);
                }

                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing automatic order cancellations");
        }
    }

    /// <inheritdoc />
    public async Task ClearStaleRedisCacheAsync()
    {
        logger.LogInformation("Running ClearStaleRedisCacheAsync background job.");
        try
        {
            var endpoints = redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = redis.GetServer(endpoint);
                if (server.IsConnected)
                {
                    // Clears all keys in the current database
                    await server.FlushDatabaseAsync();
                    logger.LogInformation("Redis cache database cleared for endpoint {Endpoint}.", endpoint);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing Redis cache");
        }
    }
}
