using System.IO;
using System.Reflection;

namespace API_music.Service;

public static class LottieHelper
{
    public static string LoadJsonResource(string resourceName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"API_music.Assets.{resourceName}";

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (resourceStream == null)
                {
                    throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly manifest.");
                }

                var tempJsonFile = Path.Combine(Path.GetTempPath(), resourceName);
                using (var fileStream = new FileStream(tempJsonFile, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }

                return tempJsonFile;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading JSON resource: {ex.Message}");
            return null;
        }
    }
}