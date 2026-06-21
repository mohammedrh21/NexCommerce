using System.Text;
using Hangfire;
using Microsoft.OpenApi.Models;
using NexCommerce.API.Middleware;
using NexCommerce.Infrastructure;
using NexCommerce.Infrastructure.Hubs;
using NexCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;

// Ensure the Windows console can render Unicode/emoji characters in Serilog output
Console.OutputEncoding = Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// ── Services ─────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(ApiResponse<object>.Fail(errors));
        };
    });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSignalR();

// ── Swagger / OpenAPI ─────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "NexCommerce API",
        Version     = "v1",
        Description = "Multi-Vendor Smart Marketplace — REST API",
        Contact     = new OpenApiContact { Name = "NexCommerce Team" }
    });

    // JWT Bearer auth in Swagger UI
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Description  = "Enter: ** {your JWT token} **",
        In           = ParameterLocation.Header,
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        Reference    = new OpenApiReference
        {
            Id   = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// ── Application (AutoMapper, MediatR, FluentValidation, Services) ──
builder.Services.AddApplication(builder.Configuration);

// ── Infrastructure (EF Core, Identity, JWT, Redis, Hangfire) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── CORS — allow Blazor WASM client origin ────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins:Client").Get<string[]>();
        if (origins == null || origins.Length == 0)
        {
            origins = new[] { "https://localhost:7001", "https://localhost:7156", "http://localhost:5029" };
        }
        
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Build ─────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed database on startup ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NexCommerceDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedRolesAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ───────────────────────────────────────
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexCommerce API v1");
        options.RoutePrefix      = "swagger";
        options.DocumentTitle    = "NexCommerce API";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard — Admin-only
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthFilter()]
});

// Seed Hangfire Background Jobs
using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    
    jobManager.AddOrUpdate<IBackgroundJobService>(
        "AbandonedCartEmails",
        service => service.SendAbandonedCartEmailsAsync(),
        Cron.Daily);

    jobManager.AddOrUpdate<IBackgroundJobService>(
        "CancelUnpaidOrders",
        service => service.CancelUnpaidOrdersAsync(),
        Cron.Hourly);

    jobManager.AddOrUpdate<IBackgroundJobService>(
        "ClearStaleRedisCache",
        service => service.ClearStaleRedisCacheAsync(),
        Cron.Daily);
}

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

