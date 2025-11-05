using System;

using static GDIShenanigans.MainClass;

namespace GDIShenanigans;

public static class RadialBlurEffect
{
    public static unsafe void ApplyEffect(ref IntPtr pixelData, double velocity)
    {
        uint* pixels = (uint*)pixelData.ToPointer(); // Change to uint*

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                uint color = pixels[index];

                byte r = GetRValue(color);
                byte g = GetGValue(color);
                byte b = GetBValue(color);
                
                double dx = x - centerX;
                double dy = y - centerY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                double dirX = dx / (distance + 0.1);
                double dirY = dy / (distance + 0.1);
                
                double direction = velocity > 0 ? 1.0 : -1.0;
                int blurAmount = (int)(Math.Abs(velocity) * 15 * (distance / (centerX)));
                int targetX = x + (int)(dirX * blurAmount * direction);
                int targetY = y + (int)(dirY * blurAmount * direction);

                if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                {
                    int targetIndex = targetY * width + targetX;
                    uint targetColor = pixels[targetIndex];

                    byte tr = GetRValue(targetColor);
                    byte tg = GetGValue(targetColor);
                    byte tb = GetBValue(targetColor);

                    pixels[targetIndex] = RGB(
                        (byte)((tr + r) / 2),
                        (byte)((tg + g) / 2),
                        (byte)((tb + b) / 2)
                    );
                }
                
                pixels[index] = RGB(r, g, b);
            }
        }
    }
}