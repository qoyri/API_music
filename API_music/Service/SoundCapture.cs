using System.IO;
using NAudio.Wave;

namespace API_music.Service;

public class SoundCapture
{
    public async Task CaptureAudioAsync(int recordingTime, string outputFilePath)
    {
        if (WaveIn.DeviceCount == 0)
        {
            throw new InvalidOperationException("No audio devices available.");
        }

        // Use the first available audio input device
        int selectedDeviceNumber = 0;

        Console.WriteLine($"Recording audio for {recordingTime} seconds to {outputFilePath}");

        using (var waveIn = new WaveInEvent
               {
                   DeviceNumber = selectedDeviceNumber,
                   WaveFormat = new WaveFormat(44100, 16, 1)
               })
        {
            using (var writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat))
            {
                waveIn.DataAvailable += (sender, e) =>
                {
                    writer.Write(e.Buffer, 0, e.BytesRecorded);
                    writer.Flush();
                };

                waveIn.RecordingStopped += (sender, e) =>
                {
                    writer.Dispose();
                    Console.WriteLine("Recording stopped and writer disposed.");
                };

                waveIn.StartRecording();
                await Task.Delay(recordingTime * 1000);
                waveIn.StopRecording();
            }
        }

        var fileInfo = new FileInfo(outputFilePath);
        if (fileInfo.Exists)
        {
            Console.WriteLine($"Recording successful, size: {fileInfo.Length} bytes.");
        }
        else
        {
            Console.WriteLine("Recording failed: file not created.");
        }
    }
}