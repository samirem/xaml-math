using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfMath.Parsers;
using WpfMath.Rendering;
using XamlMath;
using XamlMath.Boxes;
using XamlMath.Exceptions;

namespace WpfMath.Controls;

public class FormulaControl : Control
{
    private static readonly TexFormulaParser _formulaParser = WpfTeXFormulaParser.Instance;
    private const int LayoutCacheCapacity = 128;
    private const int MaxCachedFormulaLength = 128;

    // Controls on one UI thread can reuse the frozen drawing without keeping each other alive.
    [ThreadStatic]
    private static Dictionary<Tuple<string, string, double, double, double, Color, double>, FormulaLayout>? _layouts;

    private readonly struct FormulaLayout
    {
        public readonly Size Size;
        public readonly double Baseline;

        public readonly DrawingGroup Drawing;

        public FormulaLayout(Size size, double baseline, DrawingGroup drawing)
        {
            Size = size;
            Baseline = baseline;
            Drawing = drawing;
        }
    }

    private TexFormula? texFormula;

    /// <summary>
    /// Distance, in the control's own (scaled) coordinate units, from the top of the
    /// rendered content down to the TeX baseline of the formula. Updated on every measure.
    /// </summary>
    /// <remarks>
    /// The control sizes to the ink bounding box (no baseline), so <see cref="VerticalAlignment"/>
    /// alone never lines a symbol up with adjacent text. Consumers that need true baseline
    /// alignment (e.g. a baseline-aware panel) read this value: the formula renders its
    /// baseline at this Y offset from the arrange-rect top, so placing the control at
    /// <c>commonBaseline - Baseline</c> lands every formula's baseline on a shared line,
    /// regardless of descenders or subscripts. Replaces per-symbol TranslateTransform fudge.
    /// </remarks>
    public double Baseline { get; private set; }

    public string Formula
    {
        get { return (string)GetValue(FormulaProperty); }
        set { SetValue(FormulaProperty, value); }
    }

    public double Scale
    {
        get { return (double)GetValue(ScaleProperty); }
        set { SetValue(ScaleProperty, value); }
    }

    public string SystemTextFontName
    {
        get => (string)GetValue(SystemTextFontNameProperty);
        set => SetValue(SystemTextFontNameProperty, value);
    }

    /// <summary>Width in DIPs of a hairline stroke laid over each glyph, to darken thin stems.</summary>
    public double SyntheticWeight
    {
        get => (double)GetValue(SyntheticWeightProperty);
        set => SetValue(SyntheticWeightProperty, value);
    }

    public bool HasError
    {
        get { return (bool)GetValue(HasErrorProperty); }
        private set { SetValue(HasErrorProperty, value); }
    }

    public ObservableCollection<Exception> Errors
    {
        get { return (ObservableCollection<Exception>)GetValue(ErrorsProperty); }
        private set { SetValue(ErrorsProperty, value); }
    }

    public ControlTemplate ErrorTemplate
    {
        get { return (ControlTemplate)GetValue(ErrorTemplateProperty); }
        set { SetValue(ErrorTemplateProperty, value); }
    }

    public int SelectionStart
    {
        get { return (int)GetValue(SelectionStartProperty); }
        set { SetValue(SelectionStartProperty, value); }
    }

    public int SelectionLength
    {
        get { return (int)GetValue(SelectionLengthProperty); }
        set { SetValue(SelectionLengthProperty, value); }
    }

    public Brush? SelectionBrush
    {
        get => (Brush)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public static readonly DependencyProperty FormulaProperty = DependencyProperty.Register(
        nameof(Formula), typeof(string), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            "",
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            coerceValueCallback: CoerceFormula));

    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
        nameof(Scale), typeof(double), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            20d,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            coerceValueCallback: CoerceScaleValue));

    public static readonly DependencyProperty SystemTextFontNameProperty = DependencyProperty.Register(
        nameof(SystemTextFontName), typeof(string), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            "Arial",
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SyntheticWeightProperty = DependencyProperty.Register(
        nameof(SyntheticWeight), typeof(double), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            0.0,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
        nameof(SelectionStart), typeof(int), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            0,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
        nameof(SelectionLength), typeof(int), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            0,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
        nameof(SelectionBrush), typeof(Brush), typeof(FormulaControl),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register(
        nameof(HasError), typeof(bool), typeof(FormulaControl),
        new PropertyMetadata(false));

    public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(
        nameof(Errors), typeof(ObservableCollection<Exception>), typeof(FormulaControl),
        new PropertyMetadata(new ObservableCollection<Exception>()));

    public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.Register(
        nameof(ErrorTemplate), typeof(ControlTemplate), typeof(FormulaControl),
        new PropertyMetadata(new ControlTemplate()));

    static FormulaControl()
    {
        ForegroundProperty.OverrideMetadata(typeof(FormulaControl),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FormulaControl),
            new FrameworkPropertyMetadata(typeof(FormulaControl)));
    }

    /// <summary>Re-measures and repaints when the control moves to a monitor with a different DPI.</summary>
    /// <remarks>The glyph runs are rasterised for a fixed pixels-per-DIP, so a stale one renders soft.</remarks>
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        // Create formula object from input text.
        if (texFormula == null)
        {
            return;
        }

        var pixelsPerDip = PixelsPerDip();
        if (_layouts is not null && CacheKey(pixelsPerDip) is { } key && _layouts.TryGetValue(key, out var layout))
        {
            drawingContext.DrawDrawing(layout.Drawing);
            return;
        }

        // Omit passing the background, since the control background will be effectively used anyway.
        var environment = WpfTeXEnvironment.Create(
            scale: Scale,
            systemTextFontName: SystemTextFontName,
            foreground: Foreground);

        var formulaSource = texFormula.Source;
        if (formulaSource != null)
        {
            var selectionBrush = SelectionBrush;
            if (selectionBrush != null)
            {
                var formulaBox = texFormula.CreateBox(environment);
                var allBoxes = new List<Box>(formulaBox.Children);
                var selectionStart = SelectionStart;
                var selectionEnd = selectionStart+SelectionLength;
                for (var idx = 0; idx<allBoxes.Count; idx++)
                {
                    var box = allBoxes[idx];
                    allBoxes.AddRange(box.Children);
                    var source = box.Source;
                    if (source == null ||
                        !source.SourceName.Equals(formulaSource.SourceName, StringComparison.Ordinal) ||
                        !source.Source.Equals(formulaSource.Source, StringComparison.Ordinal)) continue;

                    if (selectionStart<source.Start+source.Length
                        && source.Start<selectionEnd
                        && box is CharBox)
                    {
                        box.Background = WpfBrush.FromBrush(selectionBrush);
                    }
                }
            }
        }

        texFormula.RenderTo(
            drawingContext, environment, Scale,
            pixelsPerDip: pixelsPerDip,
            syntheticWeight: SyntheticWeight);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (texFormula == null)
        {
            Baseline = 0;
            return new Size();
        }

        var pixelsPerDip = PixelsPerDip();
        if (CacheKey(pixelsPerDip) is not { } key)
        {
            using var context = new DrawingVisual().RenderOpen();
            return MeasureFormula(texFormula, context, Foreground, pixelsPerDip);
        }

        var layouts = _layouts ??= new();
        if (!layouts.TryGetValue(key, out var layout))
        {
            var foreground = new SolidColorBrush(key.Item6) { Opacity = key.Item7 };
            foreground.Freeze();
            var drawing = new DrawingGroup();
            Size size;
            using (var context = drawing.Open())
                size = MeasureFormula(texFormula, context, foreground, pixelsPerDip);
            if (!drawing.CanFreeze)
                return size;
            drawing.Freeze();

            layout = new FormulaLayout(size, Baseline, drawing);
            if (layouts.Count == LayoutCacheCapacity)
                layouts.Clear();
            layouts.Add(key, layout);
        }

        Baseline = layout.Baseline;
        return layout.Size;
    }

    private Tuple<string, string, double, double, double, Color, double>? CacheKey(double pixelsPerDip)
    {
        if (Formula.Length > MaxCachedFormulaLength || SelectionBrush != null || Foreground is not (null or SolidColorBrush))
            return null;

        var foreground = (SolidColorBrush?)Foreground ?? Brushes.Black;
        if (!foreground.IsFrozen || foreground.Color.ColorContext is not null
            || !foreground.Transform.Value.IsIdentity || !foreground.RelativeTransform.Value.IsIdentity)
            return null;
        return Tuple.Create(Formula, SystemTextFontName, Scale, pixelsPerDip, SyntheticWeight, foreground.Color, foreground.Opacity);
    }

    private Size MeasureFormula(TexFormula formula, DrawingContext context, Brush? foreground, double pixelsPerDip)
    {
        // The root box's Height is the logical distance from the box top to the baseline;
        // the renderer draws content at literal (scaled) coordinates, so the baseline lands
        // at this offset from the arrange-rect top. Scale matches RenderTo's returned size.
        // CreateBox MUTATES the environment's LastFontId (RowAtom/UnderOverAtom), so this
        // uses its OWN throwaway environment and never shares the instance handed to RenderTo.
        Baseline = formula.CreateBox(
                WpfTeXEnvironment.Create(
                    scale: Scale,
                    systemTextFontName: SystemTextFontName,
                    foreground: foreground))
            .Height * Scale;

        return formula.RenderTo(
            context,
            WpfTeXEnvironment.Create(
                scale: Scale,
                systemTextFontName: SystemTextFontName,
                foreground: foreground),
            Scale,
            pixelsPerDip: pixelsPerDip,
            syntheticWeight: SyntheticWeight);
    }

    /// <summary>Device pixels per DIP of the surface this control renders onto.</summary>
    /// <remarks>
    /// Measure and render must agree, or the ink box the layout reserves does not match the ink the
    /// glyph run paints. Falls back to 96 dpi before the control joins a presentation source.
    /// </remarks>
    private double PixelsPerDip()
    {
        try
        {
            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            return dpi > 0 ? dpi : 1.0;
        }
        catch (InvalidOperationException)
        {
            return 1.0;
        }
    }

    private static object CoerceFormula(DependencyObject d, object baseValue)
    {
        var control = (FormulaControl)d;
        var formula = (string)baseValue;
        try
        {
            control.HasError = false;
            control.Errors.Clear();
            control.texFormula = _formulaParser.Parse(formula);
            return baseValue;
        }
        catch (TexException e)
        {
            control.SetError(e);
            return "";
        }
    }

    private static object CoerceScaleValue(DependencyObject d, object baseValue)
    {
        var val = (double)baseValue;
        return val < 1d ? 1d : val;
    }

    private void SetError(TexException exception)
    {
        Errors.Add(exception);
        HasError=true;
        texFormula=null;
    }
}
