using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace NexCommerce.API.Middleware;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow access only if user is authenticated and has the Admin role
        return httpContext.User.Identity?.IsAuthenticated == true && 
               httpContext.User.IsInRole("Admin");
    }
}
