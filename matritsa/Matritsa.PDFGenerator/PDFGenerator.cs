using DataMatrix.NetCore;
using Matritsa.PDFGenerator.Data;
using Matritsa.PDFGenerator.Util;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using System.Linq;
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
            var pixelSize = Options.MatrixSizeInPoints / data.GetLength(0);
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
                false,
                debugRainbow
            );
        }

        public PdfDocument GeneratePrintPreview(
            string name = "Коды продуктов",
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool debugRainbow = false
        ) {
            return GenerateMultiple(
                new string[0],
                name,
                codeGenerated,
                token,
                true,
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

        /// <summary>
        /// Генерирует документ с кодами.
        /// </summary>
        /// <param name="codes">Коды.</param>
        /// <param name="name">Название документа.</param>
        /// <param name="codeGenerated">Событие, которое вызывается при верстании, а потом при размещении каждого кода. Параметры: количество сверстанных кодов и прогресс.</param>
        /// <param name="token">Токен отмены (для работы на отдельном потоке).</param>
        /// <param name="printPreview">Активирует режим предпросмотра.</param>
        /// <param name="debugRainbow">Функция для отладки: каждый код будет иметь свой цвет.</param>
        /// <returns>Готовый документ.</returns>
        public PdfDocument GenerateMultiple(
            string[] codes,
            string name = "Коды продуктов",
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool printPreview = false,
            bool debugRainbow = false
        ) {
            var document = new PdfDocument();
            // метаинформация
            document.Info.Title = name + (printPreview ? " (preview)" : "");
            document.Info.Author = "Матрица";
            // верстаем коды
            try {
                var layout = LayoutMultiple(codes, codeGenerated, token, printPreview, debugRainbow);
                foreach (var blockPage in layout) {
                    PdfPage myPage = document.AddPage();
                    // размер
                    myPage.Width = XUnit.FromMillimeter(Options.PaperType.Size.Width);
                    myPage.Height = XUnit.FromMillimeter(Options.PaperType.Size.Height);
                    // создаем XGraphics
                    var gfx = XGraphics.FromPdfPage(myPage);
                    foreach (var block in blockPage) {
                        // если есть запрос отмены генерации:
                        if (token?.IsCancellationRequested == true) {
                            // освобождаем память
                            gfx.Dispose();
                            document.Dispose();
                            // даем ошибку
                            token?.ThrowIfCancellationRequested();
                        }
                        #if PDFGEN_DEBUG
                        Debug.WriteLine($"[PG.GenerateMultiple] drawing block {block}");
                        #endif
                        gfx.DrawBlock(block);//, (block.brush == XBrushes.White ? XBrushes.White : DebugBrushes[new Random().Next(0, DebugBrushes.Length)]));
                    }
                }
            } catch (OperationCanceledException e) {
                // освобождаем память
                document.Dispose();
                // заново возвращаем ошибку
                throw e;
            }
            return document;
        }

        /// <summary>
        /// Генерирует верстку кодов.
        /// </summary>
        /// <param name="codes">Коды.</param>
        /// <param name="codeGenerated">Событие, которое вызывается при верстании каждого кода. Параметры: количество сверстанных кодов и прогресс.</param>
        /// <param name="token">Токен отмены (для работы на отдельном потоке).</param>
        /// <param name="printPreview">Активирует режим предпросмотра.</param>
        /// <param name="debugRainbow">Функция для отладки: каждый код будет иметь свой цвет.</param>
        /// <returns>2D-массив с блоками. Первая координата указывает страницу, вторая указывает блок.</returns>
        public MatrixBlock[][] LayoutMultiple(
            string[] codes,
            Action<int, float>? codeGenerated = null,
            CancellationToken? token = null,
            bool printPreview = false,
            bool debugRainbow = false
        ) {

#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.LayoutMultiple] laying out {codes.Length} codes");
#endif
            int pageCount = 0;
            // посчитаем количество страниц
            Dimensions<int> matricesPerPage = CountCodesPerPage();
            int totalMatricesPerPage = matricesPerPage.Area();
#if PDFGEN_DEBUG
            Debug.WriteLine($"[PG.LayoutMultiple] h={matricesPerPage.Height} w={matricesPerPage.Width} S={totalMatricesPerPage}");
#endif
            if (printPreview) {
                pageCount = 1;
            }
            else {
                pageCount = (int)Math.Ceiling((float)(codes.Length / totalMatricesPerPage));
                if (pageCount == 0) {
                    pageCount = 1;
                }
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PG.LayoutMultiple] pages: {pageCount}");
#endif
            }
            // создаем 2D-массив
            var list = new MatrixBlock[pageCount][];
            int lastMatrix = 0;
            for (int page = 0; page < pageCount; page++) {
                // если есть запрос отмены генерации:
                if (token?.IsCancellationRequested == true) {
                    // даем ошибку
                    token?.ThrowIfCancellationRequested();
                }
#if PDFGEN_DEBUG
                Debug.WriteLine("[PG.LayoutMultiple] writing new page");
#endif
                // обьявляем координаты
                float x = PointConversion.ToPoints(Options.PaperType.Padding, Options.PaperType.Unit);
                float y = PointConversion.ToPoints(Options.PaperType.Padding, Options.PaperType.Unit);
                // берем каждый код, ограничиваясь количеством кодов, которые можно поместить на странице
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PG.GenerateMultiple] while ({lastMatrix} < Math.Min({codes.Length}, {totalMatricesPerPage * (page+1)}) [{Math.Min(codes.Length, totalMatricesPerPage * (page + 1))}])");
#endif
                int limit = printPreview ? totalMatricesPerPage : Math.Min(codes.Length, totalMatricesPerPage * (page + 1));
                // в режиме предпросмотра количество блоков известно заранее, а в режиме генерации кодов
                // оно еще не известно, но блоки добавляются через Concat
                list[page] = new MatrixBlock[printPreview ? totalMatricesPerPage : 0];
                while (lastMatrix < limit) {
                    // если есть запрос отмены генерации:
                    if (token?.IsCancellationRequested == true) {
                        // даем ошибку
                        token?.ThrowIfCancellationRequested();
                    }

                    // генерируем и добавляем код
                    var palette = MatrixPalette.Default;
                    if (debugRainbow) {
                        palette = new MatrixPalette(XBrushes.White, DebugBrushes[new Random().Next(0, DebugBrushes.Length)]);
                    }
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PG.LayoutMultiple] generating code for #{lastMatrix} @ x={x} y={y}");
#endif
                    if (printPreview) {
                        // если режим print preview, чертим квадрат
                        list[page][lastMatrix] = new MatrixBlock(new XRect(x, y, Options.MatrixSizeInPoints, Options.MatrixSizeInPoints), palette.matrixBrush);
#if PDFGEN_DEBUG
                        Debug.WriteLine($"[PG.LayoutMultiple] placed square #{lastMatrix} @ x={x} y={y}");
#endif
                    }
                    else {
                        list[page] = list[page].Concat(GenerateMatrix(codes[lastMatrix], x, y, palette)).ToArray();
#if PDFGEN_DEBUG
                        Debug.WriteLine($"[PG.LayoutMultiple] generated code for #{lastMatrix}");
                        Debug.WriteLine(code);
#endif
                    }
                    // если нет места по оси x, обновим y
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PG.LayoutMultiple] {x == Options.PaperType.GetBoundingBoxSize().Width - Options.MatrixFrameSizeInPoints.Width}");
                    Debug.WriteLine($"[PG.LayoutMultiple] {x} == {Options.PaperType.GetBoundingBoxSize().Width} - {Options.MatrixFrameSizeInPoints.Width}");
#endif
                    if (x >= Options.PaperType.GetBoundingBoxSize().ToPoints().Width - Options.MatrixFrameSizeInPoints.Width) {
#if PDFGEN_DEBUG
                        Debug.WriteLine($"[PG.LayoutMultiple] linebreak");
#endif
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
            return list;
        }
    }
}
