using LottieSharp.WPF;

namespace API_music.Service;

public abstract class LoadAnimations
{
    
    private static string _tempJsonFile;
    
    public static void LoadAnimation(LottieAnimationView animationView, string resourceName)
    {
        _tempJsonFile = LottieHelper.LoadJsonResource(resourceName);

        if (!string.IsNullOrEmpty(_tempJsonFile))
        {
            animationView.FileName = _tempJsonFile;

            // Optional: Force refresh (depends on the Lottie implementation)
            animationView.InvalidateVisual();
        }
        else
        {
            Console.WriteLine($"Failed to load animation: {resourceName}");
        }
    }
}