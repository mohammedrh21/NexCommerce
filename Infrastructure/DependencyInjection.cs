using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Infrastructure.Identity;
using NexCommerce.Infrastructure.Persistence;
using NexCommerce.Infrastructure.Persistence.Repositories;
using NexCommerce.Infrastructure.Services;
using StackExchange.Redis;

namespace NexCommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL / EF Core ───────────────────────────────
        services.AddDbContext<NexCommerceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(NexCommerceDbContext).Assembly.FullName);
                    // Prevent Cartesian explosions when including multiple collections
                    npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));

        // ── ASP.NET Core Identity ──────────────────────────────
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength          = 8;
            options.Password.RequireDigit            = true;
            options.Password.RequireUppercase        = true;
            options.Password.RequireNonAlphanumeric  = false;
            options.User.RequireUniqueEmail          = true;
            options.SignIn.RequireConfirmedEmail      = false; // flip to true when email is ready
        })
        .AddEntityFrameworkStores<NexCommerceDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Authentication ─────────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
                        ?? throw new InvalidOperationException("Jwt:Secret is not configured."))),
                ValidateIssuer   = true,
                ValidIssuer      = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience    = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew        = TimeSpan.Zero,
            };

            // Allow SignalR to authenticate via query string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path        = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // ── Redis ──────────────────────────────────────────────
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("Redis connection string is not configured.")));

        // ── Hangfire ───────────────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("DefaultConnection is not configured."));
            }));

        services.AddHangfireServer();

        // ── Repositories & Unit of Work ────────────────────────
        services.AddScoped<IUnitOfWork,                UnitOfWork>();
        services.AddScoped<ILanguageRepository,        LanguageRepository>();
        services.AddScoped<IRefreshTokenRepository,    RefreshTokenRepository>();
        services.AddScoped<IProductRepository,         ProductRepository>();
        services.AddScoped<ICategoryRepository,        CategoryRepository>();
        services.AddScoped<IVendorRepository,          VendorRepository>();
        services.AddScoped<IOrderRepository,           OrderRepository>();
        services.AddScoped<ICartRepository,            CartRepository>();
        services.AddScoped<IWishlistRepository,        WishlistRepository>();
        services.AddScoped<IReviewRepository,          ReviewRepository>();
        services.AddScoped<ICouponRepository,          CouponRepository>();
        services.AddScoped<INotificationRepository,    NotificationRepository>();
        services.AddScoped<IChatRepository,            ChatRepository>();

        // ── Identity & JWT Services ────────────────────────────
        services.AddScoped<IIdentityService,           IdentityService>();
        services.AddScoped<IJwtTokenService,           JwtTokenService>();

        // ── External / Infrastructure Services ────────────────
        services.AddScoped<IEmailService,              EmailService>();
        services.AddScoped<IPaymentService,            PaymentService>();
        services.AddScoped<IFileStorageService,        FileStorageService>();
        services.AddScoped<INotificationService,       NotificationService>();
        services.AddScoped<IBackgroundJobService,      BackgroundJobService>();
        services.AddScoped<IAnalyticsService,          AnalyticsService>();
        services.AddScoped<IRecommendationService,     RecommendationService>();
        services.AddScoped<IChatHubService,            ChatHubService>();

        return services;
    }
}
