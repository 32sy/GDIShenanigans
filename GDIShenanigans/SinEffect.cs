using System;

using static GDIShenanigans.MainClass;

namespace GDIShenanigans;

public static class SinEffect
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

                byte r = (byte)(Math.Sin(GetRValue(color) + (currentTime - startTime))*255);
                byte g = (byte)(Math.Sin(GetGValue(color) + (currentTime - startTime))*255);
                byte b = (byte)(Math.Sin(GetBValue(color) + (currentTime - startTime))*255);
                
                pixels[index] = RGB(r, g, b);
            }
        }
    }
}