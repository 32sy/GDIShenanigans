using System;
using static GDIShenanigans.MainClass;

namespace GDIShenanigans
{
    static class RelativisticEffect
    {
        static double minVelocity = 0;
        static double maxVelocity = 0.99;
        static double cycleTime = 50;
        
        public static double CalculateCurrentVelocity(long currentTime)
        {
            double timeSec = (currentTime - startTime) / 1000.0 % cycleTime * 10;
            double cyclePos = timeSec % cycleTime / cycleTime;

            return minVelocity + (maxVelocity - minVelocity) * Math.Sin(cyclePos * 2 * Math.PI);
        }

        public static unsafe void ApplyEffect(ref IntPtr pixelData, double velocity)
        {
            uint* pixels = (uint*)pixelData.ToPointer(); // Change to uint*
            double absVel = Math.Abs(velocity);
            double dopplerFactor = Math.Sqrt((1.0 + absVel) / (1.0 - absVel));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    uint color = pixels[index];

                    byte r = GetRValue(color);
                    byte g = GetGValue(color);
                    byte b = GetBValue(color);

                    // Calculate luminance
                    double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;

                    // Calculate radial blur direction
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double dirX = dx / (distance + 0.1);
                    double dirY = dy / (distance + 0.1);

                    // Apply color shift
                    if (velocity > 0)
                    {
                        r = (byte)(255 * Math.Min(1.0, r / 255.0 * dopplerFactor * 0.8 + luminance * 0.2));
                        b = (byte)(255 * Math.Max(0.0, b / 255.0 / dopplerFactor * 0.7 + luminance * 0.3));
                        g = (byte)(255 * (g / 255.0 * 0.9 + luminance * 0.1));
                    }
                    else
                    {
                        b = (byte)(255 * Math.Min(1.0, b / 255.0 * dopplerFactor * 0.8 + luminance * 0.2));
                        r = (byte)(255 * Math.Max(0.0, r / 255.0 / dopplerFactor * 0.7 + luminance * 0.3));
                        g = (byte)(255 * (g / 255.0 * 0.9 + luminance * 0.1));
                    }

                    // Apply radial blur
                    double direction = velocity > 0 ? 1.0 : -1.0;
                    int blurAmount = (int)(Math.Abs(velocity) * 15 * (distance / (centerX)));
                    int targetX = x + (int)(dirX * blurAmount * direction);
                    int targetY = y + (int)(dirY * blurAmount * direction);

                    // Write back the modified pixel
                    pixels[index] = RGB(r, g, b);

                    // Apply blur to target pixel
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
                }
            }
        }
    }
}