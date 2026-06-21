using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Fluxor;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Http;
using Client.Services;
using Client.Services.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Local Storage & Configuration
builder.Services.AddBlazoredLocalStorage();

// Auth States
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped(sp => (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddScoped<IToastService, ToastService>();

// HTTP Pipeline
builder.Services.AddTransient<JwtAuthHandler>();
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7141/";
builder.Services.AddHttpClient("SecureClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<JwtAuthHandler>();

// Set default HttpClient to the secure one
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SecureClient"));

// Register API Services
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IProductsApiService, ProductsApiService>();
builder.Services.AddScoped<ICategoriesApiService, CategoriesApiService>();
builder.Services.AddScoped<IOrdersApiService, OrdersApiService>();
builder.Services.AddScoped<ICartApiService, CartApiService>();
builder.Services.AddScoped<IVendorsApiService, VendorsApiService>();
builder.Services.AddScoped<IUsersApiService, UsersApiService>();
builder.Services.AddScoped<IReviewsApiService, ReviewsApiService>();
builder.Services.AddScoped<IWishlistsApiService, WishlistsApiService>();
builder.Services.AddScoped<IAnalyticsApiService, AnalyticsApiService>();
builder.Services.AddScoped<ICouponsApiService, CouponsApiService>();
builder.Services.AddScoped<INotificationsApiService, NotificationsApiService>();
builder.Services.AddScoped<IPaymentsApiService, PaymentsApiService>();
builder.Services.AddScoped<ISearchApiService, SearchApiService>();
builder.Services.AddScoped<IRecommendationsApiService, RecommendationsApiService>();
builder.Services.AddScoped<IChatApiService, ChatApiService>();
builder.Services.AddScoped<ILanguagesApiService, LanguagesApiService>();
builder.Services.AddScoped<IStripeInteropService, StripeInteropService>();

// Register Validators
builder.Services.AddScoped<FluentValidation.IValidator<Client.Models.Auth.LoginModel>, Client.Validators.Auth.LoginModelValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Client.Models.Auth.RegisterModel>, Client.Validators.Auth.RegisterModelValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<Client.Models.Auth.RefreshTokenRequest>, Client.Validators.Auth.RefreshTokenRequestValidator>();

// Fluxor State Management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
});

await builder.Build().RunAsync();

