using Microsoft.AspNetCore.Http;

namespace RoomsCalendar.Share.Domain
{
    public interface IUserProvider
    {
        ValueTask<string?> TryGetUsernameAsync(HttpContext ctx, CancellationToken ct);
    }
}
