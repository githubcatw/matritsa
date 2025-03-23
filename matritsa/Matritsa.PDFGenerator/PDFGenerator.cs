using DataMatrix.NetCore;
using Matritsa.PDFGenerator.Data;
using Matritsa.PDFGenerator.Util;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using System.Threading;
#if PDFGEN_DEBUG
using System.Diagnostics;
using System.Text;
#endif

namespace Matritsa.PDFGenerator {
    /// <summary>
    /// Генерирует PDF-файлы.
    /// </summary>
    public partial class PDFGenerator {
        public PDFOptions Options;

        private readonly DmtxImageEncoder dmtxEncoder;

        public PDFGenerator(PDFOptions options) {
            this.Options = options;
            this.dmtxEncoder = new DmtxImageEncoder();
        }

        /// <summary>
        /// Возвращает количество кодов, которые помещаются в странице.
        /// </summary>
        public Dimensions<int> CountCodesPerPage() {
            // получаем размер безопасной зоны
            var boundingBoxSize = Options.PaperType.GetBoundingBoxSize();
            // разделяем размер страницы на размер кода
            Debug.WriteLine($"Sizes = {boundingBoxSize}, {Options.MatrixFrameSize}");
            int codesInWidth = (int)Math.Floor(boundingBoxSize.Width / Options.MatrixFrameSize.Width);
            int codesInHeight = (int)Math.Floor(boundingBoxSize.Height / Options.MatrixFrameSize.Height);
            Debug.WriteLine($"Codes = {codesInWidth}, {codesInHeight}");
            // возвращаем результат
            return new Dimensions<int>(codesInWidth, codesInHeight, MeasurementUnit.None);
        }

        /// <summary>
        /// Генерирует один код DataMatrix для добавления в PDF.
        /// <br/>
        /// Размер пикселей решается на основе указанного в <see cref="Options"/> размера матрицы.
        /// </summary>
        /// <param name="code">Содержимое кода.</param>
        /// <param name="startX">X-координата начала кода.</param>
        /// <param name="startY">Y-координата начала кода.</param>
        /// <param name="palette">Цветовая палитра.</param>
        /// <returns>Список блоков, которые надо добавить в PDF. В координатах блоков учтены указанные координаты начала кода.</returns>
        public MatrixBlock[] GenerateMatrix(string code, float startX, float startY, MatrixPalette palette) {
            // генерируем datamatrix в виде двухмерного списка (т.е. матрицы) значений
            var data = dmtxEncoder.EncodeRawData(code);
            //var data = new bool[,] { { true, false, true }, { false, true, false }, { true, true, true } };
            // получаем размер пикселя из размера матрицы и количества пикселей в строке
            var pixelSize = PointConversion.ToPoints(Options.MatrixSize, MeasurementUnit.Millimeter) / data.GetLength(0);
            // превращаем в блоки
            return PDFBlockGenerator.Get2BitImageAsBlocks(data, pixelSize, startX, startY, palette);
        }

        public static XBrush[] DebugBrushes = {
            XBrushes.Aqua, XBrushes.Black, XBrushes.Blue, XBrushes.BlueViolet, XBrushes.Orange, XBrushes.OrangeRed, XBrushes.Red, XBrushes.SaddleBrown
        };


        public PdfDocument Generate(
            string[] codes,
            string name = "Коды продуктов",
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool onePerPage = false,
            bool debugRainbow = false
        ) {
            if (onePerPage) return GenerateOnePerPage(
                codes,
                name,
                codeGenerated,
                token,
                debugRainbow
            );
            return GenerateMultiple(
                codes,
                name,
                codeGenerated,
                token,
                debugRainbow
            );
        }

        public PdfDocument GenerateOnePerPage(
            string[] codes,
            string name = "Коды продуктов",
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool debugRainbow = false
        ) {
#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.GenerateOnePerPage] laying out {codes.Length} codes");
#endif
            var document = new PdfDocument();
            var done = 0;
            // метаинформация
            document.Info.Title = name;
            document.Info.Author = "Матрица";
            foreach (var code in codes) {
                // если есть запрос отмены генерации:
                if (token?.IsCancellationRequested == true) {
                    // освобождаем память
                    document.Dispose();
                    // даем ошибку
                    token?.ThrowIfCancellationRequested();
                }
                PdfPage myPage = document.AddPage();
                // размер
                myPage.Width = XUnit.FromMillimeter(Options.MatrixFrameSize.Width);
                myPage.Height = XUnit.FromMillimeter(Options.MatrixFrameSize.Height);
                // создаем XGraphics
                var gfx = XGraphics.FromPdfPage(myPage);

                // генерируем и добавляем код
                var palette = MatrixPalette.Default;
                if (debugRainbow) {
                    palette = new MatrixPalette(XBrushes.White, DebugBrushes[new Random().Next(0, DebugBrushes.Length)]);
                }
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PG.GenerateOnePerPage] generating code for #{done}");
#endif
                var codeData = GenerateMatrix(code, 0, 0, palette);
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PG.GenerateOnePerPage] generated code for #{done}");
                Debug.WriteLine(code);
#endif
                foreach (var block in codeData) {
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PG.GenerateOnePerPage] drawing block {block}");
#endif
                    gfx.DrawBlock(block);//, (block.brush == XBrushes.White ? XBrushes.White : DebugBrushes[new Random().Next(0, DebugBrushes.Length)]));
                }
                // отправляем сигнал
                done++;
                codeGenerated?.Invoke(done, (float)(done + 1) / codes.Length);
            }
            return document;
        }

        public PdfDocument GenerateMultiple(
            string[] codes,
            string name = "Коды продуктов",
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool debugRainbow = false
        ) {
#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.GenerateMultiple] laying out {codes.Length} codes");
#endif
            var document = new PdfDocument();
            // посчитаем количество страниц
            Dimensions<int> matricesPerPage = CountCodesPerPage();
            int totalMatricesPerPage = matricesPerPage.Area();
#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.GenerateMultiple] h={matricesPerPage.Height} w={matricesPerPage.Width} S={totalMatricesPerPage}");
#endif
            int pageCount = (int)Math.Ceiling((float)(codes.Length / totalMatricesPerPage));
            if (pageCount == 0) {
                pageCount = 1;
            }
#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.GenerateMultiple] pages: {pageCount}");
#endif
            int lastMatrix = 0;
            // метаинформация
            document.Info.Title = name;
            document.Info.Author = "Матрица";
            for (int page = 0; page < pageCount; page++) {
                // если есть запрос отмены генерации:
                if (token?.IsCancellationRequested == true) {
                    // освобождаем память
                    document.Dispose();
                    // даем ошибку
                    token?.ThrowIfCancellationRequested();
                }
#if PDFGEN_DEBUG
                Debug.WriteLine("[PG.GenerateMultiple] writing new page");
#endif
                PdfPage myPage = document.AddPage();
                // размер
                myPage.Width = XUnit.FromMillimeter(Options.PaperType.Size.Width);
                myPage.Height = XUnit.FromMillimeter(Options.PaperType.Size.Height);
                // создаем XGraphics
                var gfx = XGraphics.FromPdfPage(myPage);
                // обьявляем координаты
                float x = PointConversion.ToPoints(Options.PaperType.Padding, Options.PaperType.Unit);
                float y = PointConversion.ToPoints(Options.PaperType.Padding, Options.PaperType.Unit);
                // берем каждый код, ограничиваясь количеством кодов, которые можно поместить на странице
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PG.GenerateMultiple] while ({lastMatrix} < Math.Min({codes.Length}, {totalMatricesPerPage * (page+1)}) [{Math.Min(codes.Length, totalMatricesPerPage * (page + 1))}])");
#endif
                while (lastMatrix < Math.Min(codes.Length, totalMatricesPerPage * (page+1))) {
                    // если есть запрос отмены генерации:
                    if (token?.IsCancellationRequested == true) {
                        // освобождаем память
                        gfx.Dispose();
                        document.Dispose();
                        // даем ошибку
                        token?.ThrowIfCancellationRequested();
                    }

                    // генерируем и добавляем код
                    var palette = MatrixPalette.Default;
                    if (debugRainbow) {
                        palette = new MatrixPalette(XBrushes.White, DebugBrushes[new Random().Next(0, DebugBrushes.Length)]);
                    }
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PG.GenerateMultiple] generating code for #{lastMatrix} @ x={x} y={y}");
#endif
                    var code = GenerateMatrix(codes[lastMatrix], x, y, palette);
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PG.GenerateMultiple] generated code for #{lastMatrix}");
                    Debug.WriteLine(code);
#endif
                    foreach (var block in code) {
#if PDFGEN_DEBUG
                        Debug.WriteLine($"[PG.GenerateMultiple] drawing block {block}");
#endif
                        gfx.DrawBlock(block);//, (block.brush == XBrushes.White ? XBrushes.White : DebugBrushes[new Random().Next(0, DebugBrushes.Length)]));
                    }
                    // если нет места по оси x, обновим y'
                    Debug.WriteLine($"[PG.GenerateMultiple] {x == Options.PaperType.GetBoundingBoxSize().Width - Options.MatrixFrameSizeInPoints.Width}");
                    Debug.WriteLine($"[PG.GenerateMultiple] {x} == {Options.PaperType.GetBoundingBoxSize().Width} - {Options.MatrixFrameSizeInPoints.Width}");
                    if (x >= Options.PaperType.GetBoundingBoxSize().ToPoints().Width - Options.MatrixFrameSizeInPoints.Width) {
                        Debug.WriteLine($"[PG.GenerateMultiple] linebreak");
                        x = PointConversion.ToPoints(Options.PaperType.Padding, Options.PaperType.Unit);
                        y += Options.MatrixFrameSizeInPoints.Height;
                    }
                    // иначе обновим x
                    else {
                        x += Options.MatrixFrameSizeInPoints.Width;
                    }
                    // отправляем сигнал
                    codeGenerated?.Invoke(lastMatrix, (float)(lastMatrix + 1) / codes.Length);
                    // переходим на следующий код
                    lastMatrix++;
                }
            }
            return document;
        }
    }
}
