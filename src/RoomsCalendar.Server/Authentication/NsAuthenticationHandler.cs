using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace RoomsCalendar.Server.Authentication
{
    sealed class NsAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var username = GetUsername(Context);
            if (string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        [ new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username) ]
                    )
                ),
                NsAuthenticationDefaults.AuthenticationScheme
            )));
        }

        static string? GetUsername(HttpContext ctx)
        {
#if DEBUG
            return "testuser";
#else
            if (ctx.Request.Headers.TryGetValue("X-Forwarded-User", out var value) && value.Count == 1)
            {
                return value[0];
            }
            return null;
#endif
        }
    }
}
