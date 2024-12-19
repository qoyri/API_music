using System.Diagnostics;
using System.IO;

namespace API_music.Service;

public class FFmpegConverter
{
    public static async Task<FileInfo> ConvertToRawWithLimitAsync(FileInfo wavFile, long maxBytes)
    {
        var rawFile = new FileInfo(Path.Combine(wavFile.DirectoryName, "converted.raw"));
        int duration = 5;

        do
        {
            Console.WriteLine($"Starting conversion for duration: {duration}s");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg", // Assuming ffmpeg is in the PATH
                    Arguments =
                        $"-y -i \"{wavFile.FullName}\" -t {duration} -f s16le -acodec pcm_s16le -ar 44100 -ac 1 \"{rawFile.FullName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("FFmpeg Error: " + error);
                throw new Exception("FFmpeg conversion failed: " + error);
            }

            if (!rawFile.Exists)
            {
                throw new FileNotFoundException("The converted.raw file was not created. FFmpeg Error: " + error);
            }

            Console.WriteLine($"converted.raw exists, size: {rawFile.Length} bytes");

            if (rawFile.Length < maxBytes)
                duration *= 2;
            else if (rawFile.Length > maxBytes)
                duration /= 2;
        } while (rawFile.Length > maxBytes);

        if (rawFile.Length > maxBytes)
            throw new Exception("Cannot reduce the file size within the limit");

        return rawFile;
    }
}