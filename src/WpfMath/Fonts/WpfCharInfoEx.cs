using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using XamlMath;
using XamlMath.Exceptions;

namespace WpfMath.Fonts;

public static class WpfCharInfoEx
{
    /// <param name="pixelsPerDip">
    /// Device pixels per DIP of the target surface (1.0 at 96 dpi, 1.5 at 150% scaling). WPF hints
    /// and antialiases the glyph for this resolution, so a wrong value renders a soft, washed-out
    /// glyph: the run is rasterised for one device and painted on another.
    /// </param>
    public static GlyphRun GetGlyphRun(this CharInfo info, double x, double y, double scale, double pixelsPerDip = 1.0)
    {
        var typeface = ((WpfGlyphTypeface)info.Font).Typeface;
        var characterInt = (int)info.Character;
        if (!typeface.CharacterToGlyphMap.TryGetValue(characterInt, out var glyphIndex))
        {
            var fontName = typeface.FamilyNames.Values.First();
            var characterHex = characterInt.ToString("X4");
            throw new TexCharacterMappingNotFoundException(
                $"The {fontName} font does not support '{info.Character}' (U+{characterHex}) character.");
        }

        var glyphRun = new GlyphRun((float)pixelsPerDip);
        ((ISupportInitialize)glyphRun).BeginInit();
        glyphRun.GlyphTypeface = typeface;
        glyphRun.FontRenderingEmSize = info.Size * scale;
        glyphRun.GlyphIndices = new[] { glyphIndex };
        glyphRun.BaselineOrigin = new Point(x * scale, y * scale);
        glyphRun.AdvanceWidths = new[] { typeface.AdvanceWidths[glyphIndex] };
        ((ISupportInitialize)glyphRun).EndInit();

        return glyphRun;
    }
}
