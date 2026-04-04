using System;
using System.Linq;
using System.Windows.Media;
using WpfMath.Fonts;
using XamlMath;

namespace WpfMath.Rendering;

public static class WpfTeXEnvironment
{
    /// <summary>Creates an instance of <see cref="TexEnvironment"/> for a WPF program.</summary>
    /// <param name="style">Initial style for the formula content.</param>
    /// <param name="scale">Formula font size.</param>
    /// <param name="systemTextFontName">
    /// Name of the system font or embedded font path for <c>\text</c> blocks.
    /// Supports system font names (e.g., "Arial") and application resource paths
    /// with fragment syntax (e.g., "./Fonts/#CMU Serif") which resolve relative
    /// to the application's pack URI.
    /// </param>
    /// <param name="foreground">Foreground color. Black if not specified.</param>
    /// <param name="background">Background color.</param>
    public static TexEnvironment Create(
        TexStyle style = TexStyle.Display,
        double scale = 20.0,
        string systemTextFontName = "Arial",
        Brush? foreground = null,
        Brush? background = null)
    {
        var mathFont = new DefaultTexFont(WpfMathFontProvider.Instance, scale);
        var textFont = GetSystemFont(systemTextFontName, scale);

        return new TexEnvironment(
            style,
            mathFont,
            textFont,
            background.ToPlatform(),
            foreground.ToPlatform());
    }

    private static WpfSystemFont GetSystemFont(string fontName, double size)
    {
        var fontFamily = System.Windows.Media.Fonts.SystemFontFamilies
            .FirstOrDefault(ff => ff.ToString() == fontName || ff.FamilyNames.Values?.Contains(fontName) == true);

        if (fontFamily == null)
        {
            // Support embedded font resources via fragment syntax (e.g., "./Fonts/#CMU Serif").
            // WPF requires a base URI + relative path to resolve application-embedded fonts.
            if (fontName.Contains('#'))
                fontFamily = new FontFamily(new Uri("pack://application:,,,/"), fontName);
            else
                fontFamily = new FontFamily(fontName);
        }

        return new WpfSystemFont(size, fontFamily);
    }
}
