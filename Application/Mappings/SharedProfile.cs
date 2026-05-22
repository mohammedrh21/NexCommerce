using AutoMapper;
using NexCommerce.Application.DTOs.Reviews;
using NexCommerce.Application.DTOs.Notifications;
using NexCommerce.Application.DTOs.Chat;
using NexCommerce.Application.DTOs.Coupons;
using NexCommerce.Application.DTOs.Payments;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Mappings;

public class SharedProfile : Profile
{
    public SharedProfile()
    {
        // Review
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.ReviewerName, o => o.Ignore()); // resolved in service

        // Notification
        CreateMap<Notification, NotificationDto>();

        // Chat
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(d => d.SenderName,   o => o.Ignore())
            .ForMember(d => d.ReceiverName, o => o.Ignore());

        // Coupon
        CreateMap<CouponTranslation, CouponTranslationDto>()
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code));

        CreateMap<Coupon, CouponDto>()
            .ForMember(d => d.UsedCount,    o => o.MapFrom(s => s.Usages.Count))
            .ForMember(d => d.IsExpired,    o => o.MapFrom(s => s.ExpiryDate < DateTime.UtcNow))
            .ForMember(d => d.Translations, o => o.MapFrom(s => s.Translations));

        // Payment
        CreateMap<Payment, PaymentDto>();
    }
}
