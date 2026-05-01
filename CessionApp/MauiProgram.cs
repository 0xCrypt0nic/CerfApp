using CessionApp.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace CessionApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// ⚠️ Doit être défini AVANT tout accès au getter de GlobalFontSettings.FontResolver.
		// Le getter par défaut instancie PdfSharpCore.Utils.LinuxSystemFontResolver qui appelle
		// fc-list/fontconfig — inexistant sur Android/iOS → TypeInitializationException.
		GlobalFontSettings.FontResolver = new CerfaFontResolver();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.UseMauiCommunityToolkitCamera()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CameraView, CameraViewHandler>();
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("fa-solid-900.ttf", "FaSolid");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
