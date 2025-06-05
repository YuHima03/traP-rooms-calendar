using Knoq.Extensions.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RoomsCalendar.Client;
using RoomsCalendar.Server.Configurations;
using RoomsCalendar.Server.Domain;
using RoomsCalendar.Server.Services;
using Traq;

namespace RoomsCalendar.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            {
                var config = builder.Configuration;

                if (config.GetValue<string>("env-files") is string envFiles && !string.IsNullOrWhiteSpace(envFiles))
                {
                    foreach (var path in envFiles.Split(';'))
                    {
                        config.AddIniStream(File.OpenRead(path));
                    }
                }
            }

            // Configure services.
            {
                var services = builder.Services;

                if (TimeZoneInfo.TryFindSystemTimeZoneById(builder.Configuration["TimeZone:Id"] ?? string.Empty, out var tzi))
                {
                    services.AddSingleton(tzi);
                }

                services.Configure<KnoqClientOptions>(builder.Configuration);
                services.Configure<TraqClientOptions>(builder.Configuration);
                services.AddSingleton<IConfigureOptions<TraqApiClientOptions>>(sp =>
                {
                    return new ConfigureOptions<TraqApiClientOptions>(opt =>
                    {
                        var baseOptions = sp.GetRequiredService<IOptions<TraqClientOptions>>().Value;
                        opt.BaseAddress = baseOptions.TraqApiBaseAddress ?? string.Empty;
                        opt.CookieAuthToken = baseOptions.TraqCookieAuthenticationToken;
                    });
                });
                services.AddAuthenticatedKnoqApiClient(
                    (sp, knoqOptions) =>
                    {
                        knoqOptions.BaseAddress = sp.GetRequiredService<IOptions<KnoqClientOptions>>().Value.KnoqApiBaseAddress ?? string.Empty;
                    },
                    (sp, traqAuthOptions) =>
                    {
                        traqAuthOptions.UseCookieAuthentication(
                            sp.GetRequiredService<IOptions<TraqClientOptions>>().Value.TraqCookieAuthenticationToken ?? throw new Exception("The cookie token for traQ service is not set.")
                        );
                    }
                );

                services.AddHostedService<RoomsCollector>();
                services.AddSingleton<IRoomsProvider, RoomsProvider>();

                services.AddScoped(sp => new HttpClient { BaseAddress = new(sp.GetRequiredService<NavigationManager>().BaseUri) });
            }

            // API controllers
            builder.Services.AddControllers();

            // Razor (View)
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode();
            app.MapControllers();

            app.Run();
        }
    }
}
