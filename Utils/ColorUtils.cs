using System.Drawing;

namespace CS2_DecoyXrayScanner.Utils;

public static class ColorUtils
{
    public static Color Parse(string? html, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(html)) return fallback;
        try { return ColorTranslator.FromHtml(html.Trim()); } catch { return fallback; }
    }
}