namespace Client.Models.Common;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4
}

public enum DiscountType
{
    Percentage = 1,
    Fixed = 2
}

public enum NotificationType
{
    OrderUpdate = 1,
    NewOffer = 2,
    LowStock = 3,
    Chat = 4
}
