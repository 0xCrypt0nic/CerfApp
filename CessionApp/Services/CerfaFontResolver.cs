using PdfSharpCore.Fonts;

namespace CessionApp.Services;

/// <summary>
/// Font resolver pour PdfSharpCore dans MAUI.
/// Charge OpenSans depuis les MauiAssets (Resources/Raw/).
/// </summary>
public class CerfaFontResolver : IFontResolver
{
    private static byte[]? _regularData;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public string DefaultFontName => "OpenSans-Regular";

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // On redirige tout vers OpenSans-Regular
        return new FontResolverInfo("OpenSans-Regular");
    }

    public byte[] GetFont(string faceName)
    {
        if (_regularData != null) return _regularData;

        _lock.Wait();
        try
        {
            if (_regularData != null) return _regularData;

            using var stream = FileSystem.OpenAppPackageFileAsync("OpenSans-Regular.ttf")
                                         .GetAwaiter().GetResult();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            _regularData = ms.ToArray();
            return _regularData;
        }
        finally
        {
            _lock.Release();
        }
    }
}
