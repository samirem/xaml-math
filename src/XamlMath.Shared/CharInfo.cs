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
    /// Vertical correction for a glyph rendered at a per-symbol <c>scale</c>, in the same units as
    /// <see cref="Metrics"/>; 0 for an unscaled glyph. Negative moves the glyph UP.
    /// <para>
    /// A scale is applied about the BASELINE, so a relation like <c>\leq</c> - which is drawn centred on
    /// the math axis - has its optical centre pulled down to <c>axis*scale</c>, sitting low against the
    /// <c>=</c> and <c>+</c> beside it. This shift puts the centre back on the axis. It is deliberately
    /// computed for axis-centred symbols (relations and operators), which is what <c>scale</c> is for;
    /// scaling a glyph that sits on the baseline (a digit, a letter) would need a different rule.
    /// </para>
    /// </summary>
    internal double AxisCenteringShift
    {
        get;
        set;
    }

    public CharFont GetCharacterFont()
    {
        return new CharFont(Character, FontId);
    }
}
