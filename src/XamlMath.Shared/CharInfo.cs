using XamlMath.Fonts;

namespace XamlMath;

/// <summary>Single character together with information about font and metrics.</summary>
public class CharInfo
{
    public CharInfo(char character, IFontTypeface font, double size, int fontId, TeXFontMetrics metrics)
    {
        this.Character = character;
        Font = font;
        this.Size = size;
        FontId = fontId;
        this.Metrics = metrics;
    }

    public char Character
    {
        get;
        set;
    }

    public IFontTypeface Font
    {
        get;
    }

    public double Size
    {
        get;
        set;
    }

    public TeXFontMetrics Metrics
    {
        get;
        set;
    }

    public int FontId
    {
        get;
    }

    /// <summary>
    /// Vertical correction for a glyph rendered at a per-symbol scale, in the same units as
    /// <see cref="Metrics"/>; 0 for an unscaled glyph. Negative moves the glyph UP.
    /// <para>
    /// A scale is applied about the BASELINE, so the top of a shrunken glyph drops while its foot stays
    /// put. Two kinds of glyph want two different corrections, and both land here:
    /// </para>
    /// <para>
    /// An axis-centred relation such as <c>\leq</c> has its optical centre pulled down to
    /// <c>axis*scale</c>, sitting low against the <c>=</c> and <c>+</c> beside it; the correction puts
    /// that centre back on the math axis (see <c>DefaultTexFont</c>).
    /// </para>
    /// <para>
    /// A baseline glyph such as the permille sign has no axis to return to: leaving it on the baseline is
    /// legitimate, and so is lifting it to sit optically centred against the digits. That choice is a
    /// fraction of the height the scaling removed, passed to <see cref="Scaled"/> by the caller.
    /// </para>
    /// <para>
    /// The name is narrower than the two jobs it now does; it stays as it is because the parser approval
    /// snapshots inherited from upstream serialize it by name.
    /// </para>
    /// </summary>
    internal double AxisCenteringShift
    {
        get;
        set;
    }

    /// <summary>
    /// A copy of this character drawn at <paramref name="glyphScale"/> of its size, box metrics included,
    /// so the layout box shrinks with the glyph exactly as it does for a <c>SymbolMapping</c> declaring a
    /// <c>scale</c> attribute.
    /// </summary>
    /// <param name="glyphScale">Size factor; 1 returns this character unchanged.</param>
    /// <param name="lift">
    /// How much of the height removed by the scaling to give back as an upward shift: 0 leaves the glyph
    /// standing on the baseline, 0.5 centres the shrunken glyph on the original glyph's middle, 1 hangs
    /// it from the original glyph's top.
    /// </param>
    internal CharInfo Scaled(double glyphScale, double lift) =>
        glyphScale == 1d
            ? this
            : new CharInfo(
                this.Character,
                this.Font,
                this.Size*glyphScale,
                this.FontId,
                new TeXFontMetrics(
                    this.Metrics.Width,
                    this.Metrics.Height,
                    this.Metrics.Depth,
                    this.Metrics.Italic,
                    glyphScale))
            {
                AxisCenteringShift = -this.Metrics.Height*(1d - glyphScale)*lift
            };

    public CharFont GetCharacterFont()
    {
        return new CharFont(Character, FontId);
    }
}
