using AutoMapper;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
            .ForMember(d => d.ChangedAt, o => o.MapFrom(s => s.ChangedAt));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName,   o => o.MapFrom(s =>
                s.Product.Translations.FirstOrDefault()!.Name))
            .ForMember(d => d.ProductImage,  o => o.MapFrom(s =>
                s.Product.Images.FirstOrDefault(i => i.IsMain)!.ImageUrl))
            .ForMember(d => d.Size,          o => o.MapFrom(s => s.ProductVariant.Size))
            .ForMember(d => d.Color,         o => o.MapFrom(s => s.ProductVariant.Color))
            .ForMember(d => d.LineTotal,     o => o.MapFrom(s => s.Price * s.Quantity));

        CreateMap<Payment, PaymentSummaryDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName,     o => o.Ignore())   // resolved in service
            .ForMember(d => d.CustomerEmail,    o => o.Ignore())
            .ForMember(d => d.DiscountAmount,   o => o.Ignore())
            .ForMember(d => d.ShippingAddress,  o => o.Ignore())
            .ForMember(d => d.CouponCode,       o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Code : null))
            .ForMember(d => d.Items,            o => o.MapFrom(s => s.Items))
            .ForMember(d => d.StatusHistory,    o => o.MapFrom(s => s.StatusHistory))
            .ForMember(d => d.Payment,          o => o.MapFrom(s => s.Payment));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(d => d.CustomerName, o => o.Ignore())
            .ForMember(d => d.ItemCount,    o => o.MapFrom(s => s.Items.Count));
    }
}
