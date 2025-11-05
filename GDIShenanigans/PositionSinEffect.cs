using System;
using static GDIShenanigans.MainClass;

namespace GDIShenanigans;

public class PositionSinEffect
{
    public static unsafe void ApplyEffect(ref IntPtr pixelData, long currentTime)
    {
        uint* pixels = (uint*)pixelData.ToPointer(); // Change to uint*

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                uint color = pixels[index];

                byte r = (byte)(Math.Sin(x + (currentTime - startTime))*8+GetRValue(color));
                byte g = (byte)(Math.Sin(y + (currentTime - startTime))*8+GetGValue(color));
                byte b = (byte)(Math.Sin(x*y + (currentTime - startTime))*8+GetBValue(color));
                
                pixels[index] = RGB(r, g, b);
            }
        }
    }
}