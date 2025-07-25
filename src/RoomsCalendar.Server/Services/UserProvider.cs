using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Services
{
    static class UserProvider
    {
        static public IUserProvider GetProvider(IServiceProvider services)
        {
#if DEBUG
            return new TestUserProvider();
#else
            return new NsUserProvider();
#endif
        }
    }

    /// <summary>
    /// An implementation of <see cref="IUserProvider"/> for the NeoShowcase container.
    /// </summary>
    file sealed class NsUserProvider : IUserProvider
    {
        public ValueTask<string?> TryGetUsernameAsync(HttpContext ctx, CancellationToken ct)
        {
            ctx.Request.Headers.TryGetValue("X-Forwarded-User", out var value);
            if (value.Count != 1)
            {
                return ValueTask.FromResult<string?>(null);
            }
            var username = value[0];
            if (string.IsNullOrWhiteSpace(username))
            {
                return ValueTask.FromResult<string?>(null);
            }
            return ValueTask.FromResult<string?>(username);
        }
    }

    /// <summary>
    /// A test implementation of <see cref="IUserProvider"/> that always returns a fixed username.
    /// </summary>
    file sealed class TestUserProvider : IUserProvider
    {
        public ValueTask<string?> TryGetUsernameAsync(HttpContext ctx, CancellationToken ct)
        {
            return ValueTask.FromResult<string?>("testuser");
        }
    }
}
