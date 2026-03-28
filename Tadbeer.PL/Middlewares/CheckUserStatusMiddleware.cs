using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Middlewares;

public class CheckUserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public CheckUserStatusMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                var cacheKey = $"UserLockout_{userIdStr}";
                
                if (!_cache.TryGetValue(cacheKey, out bool isLockedOut))
                {
                    var user = await userManager.FindByIdAsync(userIdStr);
                    isLockedOut = user != null && await userManager.IsLockedOutAsync(user);
                    
                    // Cache the result for 30 seconds to avoid DB hit on every single API request
                    _cache.Set(cacheKey, isLockedOut, TimeSpan.FromSeconds(30));
                }

                if (isLockedOut)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"User account is blocked. Access revoked.\"}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
