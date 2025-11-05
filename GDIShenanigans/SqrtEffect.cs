using System;

using static GDIShenanigans.MainClass;

namespace GDIShenanigans;

public static class SqrtEffect
{
    public static unsafe void ApplyEffect(ref IntPtr pixelData)
    {
        uint* pixels = (uint*)pixelData.ToPointer(); // Change to uint*

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                uint color = pixels[index];

                byte r = (byte)Math.Sqrt(GetRValue(color) * GetGValue(color));
                byte g = (byte)Math.Sqrt(GetGValue(color) * GetBValue(color));
                byte b = (byte)Math.Sqrt(GetBValue(color) * GetRValue(color));
                
                pixels[index] = RGB(r, g, b);
            }
        }
    }
}