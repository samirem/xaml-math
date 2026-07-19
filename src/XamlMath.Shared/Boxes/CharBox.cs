using System.Collections.Generic;
using XamlMath.Rendering;

namespace XamlMath.Boxes;

/// <summary>Box representing single character.</summary>
internal sealed class CharBox : Box
{
    public CharBox(TexEnvironment environment, CharInfo charInfo)
        : base(environment)
    {
        this.Character = charInfo;
        this.Width = charInfo.Metrics.Width;
        this.Height = charInfo.Metrics.Height;
        this.Depth = charInfo.Metrics.Depth;
        this.Italic = charInfo.Metrics.Italic;

        // Non-zero only for a per-symbol scaled glyph (see CharInfo.AxisCenteringShift). The containing
        // HorizontalBox both applies this when rendering and folds it into its own Height/Depth, so the
        // lifted glyph stays inside the line box.
        this.Shift = charInfo.AxisCenteringShift;
    }

    public CharInfo Character { get; }

    public override void RenderTo(IElementRenderer renderer, double x, double y)
    {
        renderer.RenderCharacter(Character, x, y, this.Foreground);
    }

    public override int GetLastFontId()
    {
        return this.Character.FontId;
    }
}
