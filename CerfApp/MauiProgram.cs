using CerfApp.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace CerfApp;

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
#if ANDROID
				Microsoft.Maui.Handlers.RadioButtonHandler.Mapper.AppendToMapping("ThemeColor", (h, _) =>
				{
					void UpdateTint()
					{
						var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
						var color = isDark ? Android.Graphics.Color.White : Android.Graphics.Color.Black;
						var states = new int[][] {
							new[] { Android.Resource.Attribute.StateChecked },
							new[] { -Android.Resource.Attribute.StateChecked }
						};
						var colors = new int[] { (int)color, (int)color };
						var tint = new Android.Content.Res.ColorStateList(states, colors);
						if (h.PlatformView is Android.Widget.CompoundButton cb)
							cb.ButtonTintList = tint;
					}
					UpdateTint();
					if (Application.Current != null)
						Application.Current.RequestedThemeChanged += (_, _) => UpdateTint();
				});

#endif
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

		builder.Services.AddSingleton<Services.ICerfaPageRenderer, PdfRenderService>();

		return builder.Build();
	}
}
