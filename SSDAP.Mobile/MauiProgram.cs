using Microsoft.Extensions.Logging;
using SSDAP.Mobile.Services;

namespace SSDAP.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<AlmacenMovil, AlmacenDispositivo>();
            builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
            builder.Services.AddSingleton<SesionMovil>();
            builder.Services.AddSingleton<NavegadorExterno, NavegadorDispositivo>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
