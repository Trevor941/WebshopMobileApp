using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Syncfusion.Maui.Toolkit.Hosting;
using WebshopMobileApp.Pages.TabbedPages;
namespace WebshopMobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                    EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                    {
#if ANDROID
                        handler.PlatformView.Background = null;
#endif
                    });
#if IOS || MACCATALYST
    				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            
           
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProductsListPageModel>();
            builder.Services.AddSingleton<LoginRepository>();
            builder.Services.AddSingleton<ProductRepository>();
            builder.Services.AddSingleton<CartRepository>();
            //builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            //builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageModel>();
            builder.Services.AddTransient<TabbedParentPage>();
            builder.Services.AddTransient<LoginPageModel>();
            builder.Services.AddSingleton<IAppNavigationService, AppNavigationService>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<Catalog>();
            return builder.Build();
        }
    }
}
