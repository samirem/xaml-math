﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     GenAPI Version: 7.0.8.6004
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace WpfMath
{
    public static partial class Extensions
    {
        public static byte[] RenderToPng(this WpfMath.TexFormula texForm, double scale, double x, double y, string systemTextFontName) { throw null; }
    }
    [System.ObsoleteAttribute("Use extension methods on WpfMath.TexFormula instead.")]
    public partial class TexRenderer
    {
        internal TexRenderer() { }
        public double Baseline { get { throw null; } }
        public WpfMath.Boxes.Box Box { [System.Runtime.CompilerServices.CompilerGeneratedAttribute] get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute] set { } }
        public System.Windows.Size RenderSize { get { throw null; } }
        public double Scale { [System.Runtime.CompilerServices.CompilerGeneratedAttribute] get { throw null; } }
        [System.ObsoleteAttribute("Use WpfMath.Rendering.WpfTeXFormulaExtensions::RenderTo instead.")]
        public void Render(System.Windows.Media.DrawingContext drawingContext, double x, double y) { }
        [System.ObsoleteAttribute("Use WpfMath.Rendering.TeXFormulaExtensions::RenderTo instead.")]
        public void RenderFormulaTo(WpfMath.Rendering.IElementRenderer renderer, double x, double y) { }
        [System.ObsoleteAttribute("Use WpfMath.Rendering.WpfTeXFormulaExtensions::RenderToBitmap instead.")]
        public System.Windows.Media.Imaging.BitmapSource RenderToBitmap(double x, double y) { throw null; }
        [System.ObsoleteAttribute("Use WpfMath.Rendering.WpfTeXFormulaExtensions::RenderToBitmap instead.")]
        public System.Windows.Media.Imaging.BitmapSource RenderToBitmap(double x, double y, double dpi) { throw null; }
        [System.ObsoleteAttribute("Use WpfMath.Rendering.WpfTeXFormulaExtensions::RenderToGeometry instead.")]
        public System.Windows.Media.Geometry RenderToGeometry(double x, double y) { throw null; }
    }
}
namespace WpfMath.Controls
{
    public partial class FormulaControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector
    {
        public static readonly System.Windows.DependencyProperty ErrorsProperty;
        public static readonly System.Windows.DependencyProperty ErrorTemplateProperty;
        public static readonly System.Windows.DependencyProperty FormulaProperty;
        public static readonly System.Windows.DependencyProperty HasErrorProperty;
        public static readonly System.Windows.DependencyProperty ScaleProperty;
        public static readonly System.Windows.DependencyProperty SelectionBrushProperty;
        public static readonly System.Windows.DependencyProperty SelectionLengthProperty;
        public static readonly System.Windows.DependencyProperty SelectionStartProperty;
        public static readonly System.Windows.DependencyProperty SystemTextFontNameProperty;
        public FormulaControl() { }
        public System.Collections.ObjectModel.ObservableCollection<System.Exception> Errors { get { throw null; } }
        public System.Windows.Controls.ControlTemplate ErrorTemplate { get { throw null; } set { } }
        public string Formula { get { throw null; } set { } }
        public bool HasError { get { throw null; } }
        public double Scale { get { throw null; } set { } }
        public System.Windows.Media.Brush? SelectionBrush { get { throw null; } set { } }
        public int SelectionLength { get { throw null; } set { } }
        public int SelectionStart { get { throw null; } set { } }
        public string SystemTextFontName { get { throw null; } set { } }
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.2.0")]
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void InitializeComponent() { }
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.2.0")]
        [System.ComponentModel.EditorBrowsableAttribute(1)]
        [System.Diagnostics.DebuggerNonUserCodeAttribute]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) { }
    }
    public partial class VisualContainerElement : System.Windows.FrameworkElement
    {
        public VisualContainerElement() { }
        public System.Windows.Media.DrawingVisual? Visual { get { throw null; } set { } }
        protected override int VisualChildrenCount { get { throw null; } }
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize) { throw null; }
        protected override System.Windows.Media.Visual? GetVisualChild(int index) { throw null; }
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize) { throw null; }
        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved) { }
    }
}
namespace WpfMath.Converters
{
    public partial class SVGConverter
    {
        public SVGConverter() { }
        public string ConvertGeometry(System.Windows.Media.Geometry geometry) { throw null; }
    }
}
namespace WpfMath.Parsers
{
    public static partial class WpfTeXFormulaParser
    {
        public static WpfMath.TexFormulaParser Instance { [System.Runtime.CompilerServices.CompilerGeneratedAttribute] get { throw null; } }
    }
}
namespace WpfMath.Rendering
{
    public partial class GeometryElementRenderer : WpfMath.Rendering.IElementRenderer
    {
        public GeometryElementRenderer(System.Windows.Media.GeometryGroup geometry, double scale) { }
        public void FinishRendering() { }
        public void RenderCharacter(WpfMath.CharInfo info, double x, double y, WpfMath.Rendering.IBrush? foreground) { }
        public void RenderElement(WpfMath.Boxes.Box box, double x, double y) { }
        public void RenderRectangle(WpfMath.Rendering.Rectangle rectangle, WpfMath.Rendering.IBrush? foreground) { }
        public void RenderTransformed(WpfMath.Boxes.Box box, System.Collections.Generic.IEnumerable<WpfMath.Rendering.Transformations.Transformation> transforms, double x, double y) { }
    }
    public partial class WpfBrush : WpfMath.Rendering.GenericBrush<System.Windows.Media.Brush>, System.IEquatable<WpfMath.Rendering.WpfBrush>
    {
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        protected WpfBrush(WpfMath.Rendering.WpfBrush original) { }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        protected override System.Type EqualityContract { [System.Runtime.CompilerServices.CompilerGeneratedAttribute] get { throw null; } }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public override bool Equals(object? obj) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public sealed override bool Equals(WpfMath.Rendering.GenericBrush<System.Windows.Media.Brush>? other) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public virtual bool Equals(WpfMath.Rendering.WpfBrush? other) { throw null; }
        public static WpfMath.Rendering.WpfBrush FromBrush(System.Windows.Media.Brush value) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public override int GetHashCode() { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public static bool operator ==(WpfMath.Rendering.WpfBrush? left, WpfMath.Rendering.WpfBrush? right) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public static bool operator !=(WpfMath.Rendering.WpfBrush? left, WpfMath.Rendering.WpfBrush? right) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        protected override bool PrintMembers(System.Text.StringBuilder builder) { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public override string ToString() { throw null; }
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
        public override WpfMath.Rendering.GenericBrush<System.Windows.Media.Brush> <Clone>$() { throw null; }
    }
    public static partial class WpfBrushExtensions
    {
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute("brush")]
        public static WpfMath.Rendering.IBrush? ToPlatform(this System.Windows.Media.Brush? brush) { throw null; }
        public static System.Windows.Media.Brush? ToWpf(this WpfMath.Rendering.IBrush? brush) { throw null; }
    }
    public partial class WpfBrushFactory : WpfMath.Rendering.IBrushFactory
    {
        public static WpfMath.Rendering.WpfBrushFactory Instance;
        public WpfBrushFactory() { }
        public WpfMath.Rendering.IBrush FromColor(WpfMath.Colors.RgbaColor color) { throw null; }
    }
    public static partial class WpfTeXEnvironment
    {
        public static WpfMath.TexEnvironment Create(WpfMath.TexStyle style = 0, double scale = 20, string systemTextFontName = "Arial", System.Windows.Media.Brush? foreground = null, System.Windows.Media.Brush? background = null) { throw null; }
    }
    public static partial class WpfTeXFormulaExtensions
    {
        public static void RenderTo(this WpfMath.TexFormula formula, System.Windows.Media.DrawingContext drawingContext, WpfMath.TexEnvironment environment, double scale = 20, double x = 0, double y = 0) { }
        public static System.Windows.Media.Imaging.BitmapSource RenderToBitmap(this WpfMath.TexFormula formula, WpfMath.TexEnvironment environment, double scale = 20, double x = 0, double y = 0, double dpi = 96) { throw null; }
        public static System.Windows.Media.Geometry RenderToGeometry(this WpfMath.TexFormula formula, WpfMath.TexEnvironment environment, double scale = 20, double x = 0, double y = 0) { throw null; }
    }
}
