namespace WpfMath.Tests

open System
open System.Runtime.ExceptionServices
open System.Threading
open System.Windows
open System.Windows.Media
open System.Windows.Media.Imaging

open Foq
open Xunit

open WpfMath.Controls
open WpfMath.Rendering
open XamlMath
open XamlMath.Boxes
open XamlMath.Rendering

type WpfRendererTests() =
    static do Utils.initializeFontResourceLoading()

    let group = DrawingGroup()
    let context = group.Open()
    let renderer = WpfElementRenderer(context, 1.0) :> IElementRenderer

    [<Fact>]
    member _.``WpfElementRenderer.RenderElement delegates to element.RenderTo``() : unit =
        let box = Mock.Of<Box>()
        renderer.RenderElement(box, 1.0, 2.0)
        Mock.Verify(<@ box.RenderTo(renderer, 1.0, 2.0) @>, once)

    [<Fact>]
    member _.``Formula controls share layout without carrying the previous foreground``() =
        let mutable failure = None
        let thread = Thread(ThreadStart(fun () ->
            try
                let render (control: FormulaControl) =
                    let bounds = System.Windows.Size(80.0, 60.0)
                    control.Measure(bounds)
                    control.Arrange(Rect(bounds))
                    control.UpdateLayout()
                    let bitmap = RenderTargetBitmap(80, 60, 96.0, 96.0, PixelFormats.Pbgra32)
                    bitmap.Render(control)
                    let pixels = Array.zeroCreate<byte> (80 * 60 * 4)
                    bitmap.CopyPixels(pixels, 80 * 4, 0)
                    control.DesiredSize, control.Baseline, pixels

                let formula foreground = FormulaControl(Formula = @"\frac{x_1}{y}", Foreground = foreground, Scale = 14.0)
                let size, baseline, black = render (formula Brushes.Black)
                let redSize, redBaseline, red = render (formula Brushes.Red)
                let cachedSize, cachedBaseline, cachedBlack = render (formula Brushes.Black)
                Assert.True(baseline > 0.0)
                Assert.Equal(size, redSize)
                Assert.Equal(baseline, redBaseline)
                Assert.False((black = red))
                Assert.Equal(size, cachedSize)
                Assert.Equal(baseline, cachedBaseline)
                Assert.True((black = cachedBlack))
                let mutableInk = SolidColorBrush(Colors.Black)
                let themed = formula mutableInk
                let _, _, initial = render themed
                mutableInk.Color <- Colors.Red
                let _, _, repainted = render themed
                Assert.True((black = initial))
                Assert.True((red = repainted))
            with ex ->
                failure <- Some(ExceptionDispatchInfo.Capture(ex))))
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        thread.Join()
        failure |> Option.iter (fun error -> error.Throw())

    interface IDisposable with
        member _.Dispose() = (context :> IDisposable).Dispose()
