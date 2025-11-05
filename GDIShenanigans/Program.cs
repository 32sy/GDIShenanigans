using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Vanara.PInvoke;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.User32;

namespace GDIShenanigans;

[SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
class MainClass
{
    static SafeReleaseHDC Screen;
    static SafeHDC Memory;
    static SafeHBITMAP DIB;
    static IntPtr pixelData;

    public static int width;
    public static int height;
    public static int centerX;
    public static int centerY;
    public static int startTime;
    static int refreshRate = 32;
    static bool shouldClose;
    
    // Effect tracking
    static List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    static Random random = new Random();
    static int lastEffectTime = 0;
    static int effectInterval = 3000; // 2 seconds between potential new effects
    
    public static byte GetRValue(uint rgb) => (byte)(rgb & 0xFF);
    public static byte GetGValue(uint rgb) => (byte)((rgb >> 8) & 0xFF);
    public static byte GetBValue(uint rgb) => (byte)((rgb >> 16) & 0xFF);
    
    // And to create RGB value from components
    public static uint RGB(byte r, byte g, byte b) => (uint)(r | (g << 8) | (b << 16));

    static void Main()
    {
        width = GetSystemMetrics(SystemMetric.SM_CXSCREEN);
        height = GetSystemMetrics(SystemMetric.SM_CYSCREEN);
            
        Screen = GetDC(HWND.NULL);
        Memory = CreateCompatibleDC(Screen);
            
        var bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                biWidth = width,
                biHeight = -height, // Negative for top-down
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BitmapCompressionMode.BI_RGB,
                biSizeImage = (uint)(width * height * 4)
            }
        };
            
        centerX = width / 2;
        centerY = height / 2;
            
        DIB = CreateDIBSection(Memory, bmi, DIBColorMode.DIB_RGB_COLORS, 
            out IntPtr pixeldata);
        pixelData = pixeldata;
            
        SelectObject(Memory, DIB);

        startTime = Environment.TickCount;

        while (!shouldClose)
        {
            if ((GetAsyncKeyState(17) & 0x8000) != 0 &&
                GetAsyncKeyState(18) != 0 && // ALT key
                GetAsyncKeyState((int)ConsoleKey.X) != 0)
            {
                shouldClose = true;
            }

            BitBlt(Memory, 0, 0, width, height, Screen, 0, 0, RasterOperationMode.SRCCOPY);

            var currentTime = Environment.TickCount;

            // Remove expired effects
            activeEffects.RemoveAll(effect => currentTime - effect.startTime > 12000);

            // Randomly add new effects
            if (currentTime - lastEffectTime > effectInterval)
            {
                AddRandomEffect(currentTime);
                lastEffectTime = currentTime;
            }

            // Apply all active effects
            ApplyAllEffects(currentTime);
                
            BitBlt(Screen, 0, 0, width, height, Memory, 0, 0, RasterOperationMode.SRCCOPY);
            
            Thread.Sleep(refreshRate);
        }
        
        DIB?.Dispose();
        Memory?.Dispose();
        Screen?.Dispose();
    }

    static void AddRandomEffect(int currentTime)
    {
        var effectType = random.Next(0, 5);
        ActiveEffect newEffect;

        switch (effectType)
        {
            case 0:
                newEffect = new ActiveEffect 
                { 
                    type = EffectType.Relativistic, 
                    startTime = currentTime 
                };
                break;
            case 1:
                newEffect = new ActiveEffect 
                { 
                    type = EffectType.Sqrt, 
                    startTime = currentTime 
                };
                break;
            case 2:
                newEffect = new ActiveEffect 
                { 
                    type = EffectType.Sin, 
                    startTime = currentTime 
                };
                break;
            case 3:
                newEffect = new ActiveEffect 
                { 
                    type = EffectType.PositionSin, 
                    startTime = currentTime 
                };
                break;
            case 4:
                newEffect = new ActiveEffect 
                { 
                    type = EffectType.RadialBlur, 
                    startTime = currentTime 
                };
                break;
            default:
                return;
        }

        // Check if this effect type is already active
        if (activeEffects.All(e => e.type != newEffect.type))
        {
            activeEffects.Add(newEffect);
        }
    }

    static void ApplyAllEffects(int currentTime)
    {
        // If no effects are active, apply a default or do nothing
        if (activeEffects.Count == 0)
        {
            // You could apply a default effect here if desired
            return;
        }

        // Apply each active effect
        foreach (var effect in activeEffects)
        {
            switch (effect.type)
            {
                case EffectType.Relativistic:
                    double velocity = RelativisticEffect.CalculateCurrentVelocity(currentTime);
                    RelativisticEffect.ApplyEffect(ref pixelData, velocity);
                    break;
                case EffectType.Sqrt:
                    SqrtEffect.ApplyEffect(ref pixelData);
                    break;
                case EffectType.Sin:
                    SinEffect.ApplyEffect(ref pixelData, currentTime);
                    break;
                case EffectType.PositionSin:
                    PositionSinEffect.ApplyEffect(ref pixelData, currentTime);
                    break;
                case EffectType.RadialBlur:
                    double v = RelativisticEffect.CalculateCurrentVelocity(currentTime);
                    RadialBlurEffect.ApplyEffect(ref pixelData, v);
                    break;
            }
        }
    }
}

// Supporting classes and enums
public enum EffectType
{
    Relativistic,
    Sqrt,
    Sin,
    PositionSin,
    RadialBlur
}

public class ActiveEffect
{
    public EffectType type { get; set; }
    public int startTime { get; set; }
}