using System.Security.Claims;

namespace TravelSync.Trip.API;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst("sub");

        return claim is not null && Guid.TryParse(claim.Value, out var userId)
            ? userId
            : null;
    }
}
