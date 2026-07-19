using XamlMath.Fonts;
using XamlMath.Utils;

namespace XamlMath.Atoms;

/// <summary>
/// A text-style character drawn at a fraction of the surrounding size, produced by <c>\scaletext</c>.
/// The math fonts get the same knob per symbol through a <c>SymbolMapping</c>'s <c>scale</c> attribute;
/// this is its counterpart for characters that only exist in the system text font, such as the permille
/// sign, which that font draws larger than the math digits beside it.
/// </summary>
/// <remarks>
/// Deliberately a separate type rather than two more properties on <see cref="CharAtom"/>: the parser
/// approval tests serialize every property of every atom, so widening <see cref="CharAtom"/> would
/// rewrite ~130 approved snapshots inherited from upstream and make every future upstream merge conflict
/// on them.
/// </remarks>
internal sealed record ScaledCharAtom : CharSymbol
{
    public ScaledCharAtom(SourceSpan? source, char character, string textStyle, double glyphScale, double glyphLift)
        : base(source)
    {
        this.Character = character;
        this.TextStyle = textStyle;
        this.GlyphScale = glyphScale;
        this.GlyphLift = glyphLift;
    }

    public char Character { get; }

    public string TextStyle { get; }

    /// <summary>Size factor for the glyph and its layout box together; 1 leaves it untouched.</summary>
    public double GlyphScale { get; }

    /// <summary>
    /// How much of the height removed by <see cref="GlyphScale"/> to give back as an upward shift; see
    /// <c>CharInfo.Scaled</c>. 0 keeps the shrunken character standing on the baseline.
    /// </summary>
    public double GlyphLift { get; }

    public override ITeXFont GetStyledFont(TexEnvironment environment) =>
        this.TextStyle == TexUtilities.TextStyleName ? environment.TextFont : base.GetStyledFont(environment);

    protected override Result<CharInfo> GetCharInfo(ITeXFont texFont, TexStyle style) =>
        texFont.GetCharInfo(this.Character, this.TextStyle, style)
            .Map(charInfo => charInfo.Scaled(this.GlyphScale, this.GlyphLift));

    public override Result<CharFont> GetCharFont(ITeXFont texFont) =>
        // Style is irrelevant here.
        this.GetCharInfo(texFont, TexStyle.Display).Map(ci => ci.GetCharacterFont());
}
