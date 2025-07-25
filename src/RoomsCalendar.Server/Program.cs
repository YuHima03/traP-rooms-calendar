using Knoq.Extensions.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoomsCalendar.Server.Configurations;
using RoomsCalendar.Server.Services;
using RoomsCalendar.Share;
using RoomsCalendar.Share.Configuration;
using RoomsCalendar.Share.Domain;
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

                services.AddHttpClient();

                services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));

                services.Configure<KnoqClientOptions>(builder.Configuration);
                services.Configure<TraqClientOptions>(builder.Configuration);
                services.AddSingleton<IOptions<ITraqClientConfiguration>>(sp => sp.GetRequiredService<IOptions<TraqClientOptions>>());
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

                services
                    .AddHostedService<RoomsAndEventsCollector>()
                    .AddSingleton<RoomsAndEventsProvider>()
                    .AddSingleton(sp => sp.GetRequiredService<RoomsAndEventsProvider>() as IEventsProvider)
                    .AddKeyedSingleton<IRoomsProvider, RoomsAndEventsProvider>(ProviderNames.Knoq, (sp, _) => sp.GetRequiredService<RoomsAndEventsProvider>());
                services
                    .Configure<TitechRoomsCollectorConfiguration>(builder.Configuration)
                    .AddSingleton<IConfigureOptions<TitechRoomsCollectorOptions>>(sp =>
                    {
                        var config = sp.GetRequiredService<IOptions<TitechRoomsCollectorConfiguration>>().Value;
                        return new ConfigureNamedOptions<TitechRoomsCollectorOptions>(Options.DefaultName, o => config.ConfigureTitechRoomsCollectorOptions(o, sp.GetService<TimeZoneInfo>()));
                    })
                    .AddHostedService<TitechRoomsCollector>()
                    .AddSingleton<TitechRoomsProvider>()
                    .AddKeyedSingleton<IRoomsProvider, TitechRoomsProvider>(ProviderNames.Titech, (sp, _) => sp.GetRequiredService<TitechRoomsProvider>());

                services.AddSingleton<RoomsCalendarProvider>();

                services.AddSingleton(UserProvider.GetProvider);

                services.AddHttpContextAccessor();

                services.AddScoped(sp => new HttpClient { BaseAddress = new(sp.GetRequiredService<NavigationManager>().BaseUri) });

                services.Configure<NsMySqlConfiguration>(builder.Configuration);
                services.AddDbContextFactory<Infrastructure.Repository.CalendarStreamsRepository>((sp, opt) =>
                {
                    var conf = sp.GetRequiredService<IOptions<NsMySqlConfiguration>>().Value;
                    opt.UseMySQL(conf.GetConnectionString());
                });
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
            app.MapRazorComponents<Client.App>()
                .AddInteractiveWebAssemblyRenderMode();

            var handler = new Handlers.Handler();
            handler.MapHandlers(app);

            app.Run();
        }
    }
}
