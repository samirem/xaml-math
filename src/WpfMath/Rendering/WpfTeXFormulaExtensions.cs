using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlMath;
using XamlMath.Rendering;

namespace WpfMath.Rendering;

public static class WpfTeXFormulaExtensions
{
    /// <summary>Default DPI for WPF.</summary>
    private const int DefaultDpi = 96;

    public static Geometry RenderToGeometry(
        this TexFormula formula,
        TexEnvironment environment,
        double scale = 20.0,
        double x = 0.0,
        double y = 0.0,
        double pixelsPerDip = 1.0)
    {
        var geometry = new GeometryGroup();
        var renderer = new GeometryElementRenderer(geometry, scale, pixelsPerDip);
        formula.RenderTo(renderer, environment, x, y);
        return geometry;
    }

    /// <summary>Renders the formula to a WPF bitmap.</summary>
    /// <param name="formula">The formula to render.</param>
    /// <param name="environment">The environment with rendering parameters.</param>
    /// <param name="scale">Formula text scale./</param>
    /// <param name="x">A physical X coordinate of the top left corner in the resulting bitmap.</param>
    /// <param name="y">A physical Y coordinate of the top left corner in the resulting bitmap.</param>
    /// <param name="dpi">The resulting image DPI.</param>
    public static BitmapSource RenderToBitmap(
        this TexFormula formula,
        TexEnvironment environment,
        double scale = 20.0,
        double x = 0,
        double y = 0,
        double dpi = DefaultDpi)
    {
        var visual = new DrawingVisual();
        RenderWithPositiveCoordinates(formula, environment, visual, scale, x, y, dpi / DefaultDpi);

        var bounds = visual.ContentBounds;
        var width = (int)Math.Ceiling((bounds.Right + x) * dpi / DefaultDpi);
        var height = (int)Math.Ceiling((bounds.Bottom + y) * dpi / DefaultDpi);
        var bitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);
        bitmap.Render(visual);

        return bitmap;
    }

    private static void RenderWithPositiveCoordinates(
        TexFormula formula,
        TexEnvironment environment,
        DrawingVisual visual,
        double scale,
        double x,
        double y,
        double pixelsPerDip)
    {
        using (var drawingContext = visual.RenderOpen())
            formula.RenderTo(drawingContext, environment, scale, x / scale, y / scale, pixelsPerDip);

        var bounds = visual.ContentBounds;
        if (bounds is { X: >= 0, Y: >= 0 }) return;

        using (var drawingContext = visual.RenderOpen())
        {
            drawingContext.PushTransform(
                new TranslateTransform(Math.Max(0.0, -bounds.X), Math.Max(0.0, -bounds.Y)));
            formula.RenderTo(drawingContext, environment, scale, x / scale, y / scale, pixelsPerDip);
        }
    }

    /// <summary>
    /// Renders the <paramref name="formula"/> to the <paramref name="drawingContext"/>.
    /// </summary>
    /// <param name="formula">The formula to render.</param>
    /// <param name="drawingContext">The target drawing context.</param>
    /// <param name="environment">The environment with rendering parameters.</param>
    /// <param name="scale">Formula text scale./</param>
    /// <param name="x">Logical X coordinate of the top left corner of the formula.</param>
    /// <param name="y">Logical Y coordinate of the top left corner of the formula.</param>
    /// <param name="pixelsPerDip">
    /// Device pixels per DIP of the target surface; see <see cref="Fonts.WpfCharInfoEx.GetGlyphRun"/>.
    /// </param>
    /// <param name="syntheticWeight">
    /// Width in DIPs of a hairline stroke laid over each glyph's fill. A high-contrast face such as
    /// Computer Modern has stems thinner than one device pixel at 12-16 px, which can only resolve as
    /// pale grey; the stroke restores the coverage. 0 draws the glyph unweighted.
    /// </param>
    public static System.Windows.Size RenderTo(
        this TexFormula formula,
        DrawingContext drawingContext,
        TexEnvironment environment,
        double scale = 20.0,
        double x = 0.0,
        double y = 0.0,
        double pixelsPerDip = 1.0,
        double syntheticWeight = 0.0)
    {
        var renderer = new WpfElementRenderer(drawingContext, scale, pixelsPerDip, syntheticWeight);
        formula.RenderTo(renderer, environment, x, y);
        return renderer.GetBounds();
    }

    /// <summary>Pre-1.0.7 arity of <see cref="RenderTo"/>, kept resolvable for compiled callers.</summary>
    /// <remarks>
    /// A call site bakes in the full parameter list it saw at compile time, so widening a method with
    /// optional parameters renames it as far as an already-built assembly is concerned: AutoStruct.Desktop,
    /// packaged against the old signature, dies with MissingMethodException at the first measurement.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static System.Windows.Size RenderTo(
        this TexFormula formula,
        DrawingContext drawingContext,
        TexEnvironment environment,
        double scale,
        double x,
        double y) => RenderTo(formula, drawingContext, environment, scale, x, y, 1.0, 0.0);

    /// <summary>Pre-1.0.7 arity of <see cref="RenderToGeometry"/>, kept resolvable for compiled callers.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Geometry RenderToGeometry(
        this TexFormula formula,
        TexEnvironment environment,
        double scale,
        double x,
        double y) => RenderToGeometry(formula, environment, scale, x, y, 1.0);
}
