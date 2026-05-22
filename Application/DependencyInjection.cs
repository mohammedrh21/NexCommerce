using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using NexCommerce.Application.Exceptions;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Services;

namespace NexCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // ── MediatR & Validation Pipeline ──────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ForbiddenException).Assembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(NexCommerce.Application.Behaviors.ValidationBehavior<,>));
        });

        // ── FluentValidation ───────────────────────────────────
        services.AddValidatorsFromAssembly(typeof(ForbiddenException).Assembly);

        // ── Application Services ───────────────────────────────
        services.AddScoped<ICategoryService,  CategoryService>();
        services.AddScoped<IProductService,   ProductService>();
        services.AddScoped<IVendorService,    VendorService>();
        services.AddScoped<IOrderService,     OrderService>();
        services.AddScoped<ICartService,      CartService>();
        services.AddScoped<ICouponService,    CouponService>();   // ← new
        services.AddScoped<IWishlistService,  WishlistService>();
        services.AddScoped<IReviewService,    ReviewService>();
        services.AddScoped<IChatService,      ChatService>();
        services.AddScoped<ISearchService,     SearchService>();
        services.AddScoped<ILanguageService,   LanguageService>();

        return services;
    }
}

