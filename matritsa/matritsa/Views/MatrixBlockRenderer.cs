using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia;
using System;
using Matritsa.PDFGenerator.Data;
using System.Diagnostics;
using PdfSharp.UniversalAccessibility;

namespace matritsa {
    public class MatrixBlockRenderer : Control {
        static MatrixBlockRenderer() {
            AffectsRender<MatrixBlockRenderer>(MatrixBlocksProperty);
            AffectsRender<MatrixBlockRenderer>(TargetPageDimensionsProperty);
        }

        public MatrixBlockRenderer() {
            /*var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1 / 60.0);
            //timer.Tick += (sender, e) => Angle += Math.PI / 360;
            timer.Start();*/
        }

        public static readonly StyledProperty<MatrixBlock[]> MatrixBlocksProperty =
            AvaloniaProperty.Register<MatrixBlockRenderer, MatrixBlock[]>(nameof(MatrixBlocks));
        public static readonly StyledProperty<Dimensions<float>> TargetPageDimensionsProperty =
            AvaloniaProperty.Register<MatrixBlockRenderer, Dimensions<float>>(nameof(TargetPageDimensions));

        public MatrixBlock[] MatrixBlocks {
            get => GetValue(MatrixBlocksProperty);
            set => SetValue(MatrixBlocksProperty, value);
        }

        public Dimensions<float> TargetPageDimensions {
            get => GetValue(TargetPageDimensionsProperty);
            set => SetValue(TargetPageDimensionsProperty, value);
        }

        public override void Render(DrawingContext drawingContext) {
            var efWidth = double.IsNaN(Width) ? Bounds.Width : Width;
            var efHeight = double.IsNaN(Height) ? Bounds.Height : Height;
            var tpd = TargetPageDimensions.ToPoints();
            // считаем соотношение ширины и высоты
            double wDiff = efWidth / tpd.Width;
            double hDiff = efHeight / tpd.Height;
            double pageRatio = TargetPageDimensions.Width / TargetPageDimensions.Height;
            // если соотношение размера окна больше, чем соотношение страницы,
            // оставляем неизменной высоту
            bool leaveHeight = (efWidth / efHeight) > (TargetPageDimensions.Width / TargetPageDimensions.Height);

            double startX = Bounds.X;
            double startY = Bounds.Y;
            double pageHeightTransformed, pageWidthTransformed = 0;
            if (leaveHeight) {
                // устанавливаем ширину
                pageHeightTransformed = efHeight;
                // получаем высоту относительно ширины страницы
                pageWidthTransformed = pageRatio * pageHeightTransformed;
                // считаем X
                startX += (efWidth - pageWidthTransformed) / 2;
            } else {
                // устанавливаем высоту
                pageWidthTransformed = efWidth;
                // получаем ширину относительно высоты страницы
                pageHeightTransformed = (1 / pageRatio) * pageWidthTransformed;
                // считаем Y
                startY += (efHeight - pageHeightTransformed) / 2;
            }
            // устанавливаем соотношение ширины и высоты
            double wRatio = pageWidthTransformed / tpd.Width;
            double hRatio = pageHeightTransformed / tpd.Height;
            Debug.WriteLine($"======= Rendering =========");
            Debug.WriteLine($"Dimen = {Width}x{Height}");
            Debug.WriteLine($"Bound = {Bounds}");
            Debug.WriteLine($"Leave = {leaveHeight}");
            Debug.WriteLine($"eSize = {efWidth}x{efHeight}");
            Debug.WriteLine($"tpDim = {tpd}");
            Debug.WriteLine($"tpDOU = {TargetPageDimensions}");
            Debug.WriteLine($"Diffs = {wDiff}x{hDiff}");
            Debug.WriteLine($"Ratio = {wRatio}x{hRatio}");
            Debug.WriteLine($"Start = {startX}x{startY}");
            Debug.WriteLine($"Xform = {pageWidthTransformed}x{pageHeightTransformed}");
            drawingContext.DrawRectangle(Brushes.PapayaWhip, null, new Rect(startX, startY, pageWidthTransformed, pageHeightTransformed));
            foreach (var block in MatrixBlocks) {
                drawingContext.DrawRectangle(Brushes.Black, null, new Rect(
                    startX + block.rect.X * wRatio,
                    startY + block.rect.Y * hRatio,
                    block.rect.Width * wRatio,
                    block.rect.Height * wRatio
                ));
            }
        }
    }

}
