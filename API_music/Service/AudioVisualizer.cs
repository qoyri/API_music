using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace API_music.Service;

public abstract class AudioVisualizer
{
    public WriteableBitmap Bitmap { get; }
    private int _width;
    private int _height;

    public AudioVisualizer(int width, int height)
    {
        _width = width;
        _height = height;
        Bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr32, null);
    }

    public void Update(byte[] buffer, int bytesRecorded)
    {
        int stride = Bitmap.Format.BitsPerPixel / 8 * _width;
        byte[] pixels = new byte[_height * stride];

        int maxSamples = Math.Min(bytesRecorded / 2, _height * stride / 4);

        // Fill pixels with sound data
        for (int i = 0; i < maxSamples; i++)
        {
            if (i * 2 + 1 < buffer.Length)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                int amplitude = (int)((sample + 32768.0) / 65536.0 * _height);

                amplitude = Math.Min(amplitude, _height - 1);

                for (int j = 0; j < _width; j++)
                {
                    int offset = amplitude * stride + j * 4;
                    if (offset >= 0 && offset + 3 < pixels.Length) // Checking bounds for pixel array
                    {
                        pixels[offset] = 0xFF; // Blue
                        pixels[offset + 1] = 0x00; // Green
                        pixels[offset + 2] = 0x00; // Red
                        pixels[offset + 3] = 0xFF; // Alpha
                    }
                }
            }
        }

        Bitmap.WritePixels(new Int32Rect(0, 0, _width, _height), pixels, stride, 0);
    }
}