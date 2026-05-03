#if ANDROID
using Android.Gms.Extensions;
#endif

namespace CerfApp.Services;

public static class OcrService
{
    public static Task<string?> ReconnaitreTexteAsync(byte[] bytes)
    {
#if ANDROID
        return ReconnaitreAndroidAsync(bytes);
#elif IOS || MACCATALYST
        return ReconnaitreIosAsync(bytes);
#else
        return Task.FromResult<string?>(null);
#endif
    }

#if ANDROID
    private static async Task<string?> ReconnaitreAndroidAsync(byte[] bytes)
    {
        using var bitmap = await Android.Graphics.BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length);
        if (bitmap == null) return null;

        using var image = Xamarin.Google.MLKit.Vision.Common.InputImage.FromBitmap(bitmap, 0);
        using var recognizer = Xamarin.Google.MLKit.Vision.Text.TextRecognition.GetClient(
            Xamarin.Google.MLKit.Vision.Text.Latin.TextRecognizerOptions.DefaultOptions);

        var result = await recognizer.Process(image)
            .AsAsync<Xamarin.Google.MLKit.Vision.Text.Text>();
        if (result == null) return null;

        return string.Join("\n", result.TextBlocks
            .SelectMany(b => b.Lines)
            .Select(l => l.Text ?? ""));
    }
#endif

#if IOS || MACCATALYST
    private static Task<string?> ReconnaitreIosAsync(byte[] bytes)
    {
        var tcs = new TaskCompletionSource<string?>();

        using var data = Foundation.NSData.FromArray(bytes);
        using var uiImage = UIKit.UIImage.LoadFromData(data);
        if (uiImage?.CGImage == null) { tcs.SetResult(null); return tcs.Task; }

        var request = new Vision.VNRecognizeTextRequest((req, err) =>
        {
            var obs = (req as Vision.VNRecognizeTextRequest)?.Results
                      as Vision.VNRecognizedTextObservation[];
            var texte = string.Join("\n", obs?
                .Select(o => o.TopCandidates(1).FirstOrDefault()?.String ?? "")
                .Where(s => s.Length > 0) ?? []);
            tcs.SetResult(texte);
        });
        request.RecognitionLevel = Vision.VNRequestTextRecognitionLevel.Fast;

        var handler = new Vision.VNImageRequestHandler(uiImage.CGImage, new Foundation.NSDictionary());
        handler.Perform([request], out _);

        return tcs.Task;
    }
#endif
}
