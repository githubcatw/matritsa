using Matritsa.PDFGenerator.Data;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Threading;
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
using System.Diagnostics;
using System.Text;
#endif

namespace Matritsa.PDFGenerator {
    /// <summary>
    /// Генерирует PDF-файлы.
    /// </summary>
    public partial class PDFGenerator {
        public PDFOptions Options { get; private set; }

        public CodeLayoutEngine LayoutEngine;

        public PDFGenerator(PDFOptions options) {
            this.Options = options;
            this.LayoutEngine = new CodeLayoutEngine(options);
        }

        /// <summary>
        /// Устанавливает параметры генерации.
        /// </summary>
        public void SetOptions(PDFOptions options) {
            this.Options = options;
            this.LayoutEngine.Options = options;
        }

        /// <summary>
        /// Генерирует документ с кодами.
        /// </summary>
        /// <param name="codes">Коды.</param>
        /// <param name="name">Название документа.</param>
        /// <param name="codeGenerated">Событие, которое вызывается при верстании, а потом при размещении каждого кода.</param>
        /// <param name="token">Токен отмены (для работы на отдельном потоке).</param>
        /// <param name="onePerPage">Включает специальный режим генерации документа с одним кодом на каждой странице.</param>
        /// <param name="debugRainbow">Функция для отладки: каждый код будет иметь свой цвет.</param>
        /// <returns>Готовый документ.</returns>
        public PdfDocument Generate(
            string[] codes,
            string name = "Коды продуктов",
            Action<PDFGenerationUpdate>? codeGenerated = null,
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
            Action<PDFGenerationUpdate>? codeGenerated = null,
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

        public MatrixBlock[] GeneratePrintPreviewData(
            Action<PDFGenerationUpdate>? codeGenerated = null,
            CancellationToken? token = null,
            bool debugRainbow = false
        ) {
            var layout = LayoutEngine.LayoutMultiple(
                new string[0],
                codeGenerated,
                token,
                true,
                debugRainbow
            );
            if (layout.Length == 0) throw new NullReferenceException("No layout was returned.");
            return layout[0];
        }

        /// <summary>
        /// Генерирует документ с кодами, где на странице может быть только один код.
        /// <br/>
        /// Эта функция немного быстрее <see cref="GenerateMultiple(string[], string, Action{int, float}?, CancellationToken?, bool, bool)"/>.
        /// </summary>
        /// <param name="codes">Коды.</param>
        /// <param name="name">Название документа.</param>
        /// <param name="codeGenerated">Событие, которое вызывается при верстании, а потом при размещении каждого кода.</param>
        /// <param name="token">Токен отмены (для работы на отдельном потоке).</param>
        /// <param name="debugRainbow">Функция для отладки: каждый код будет иметь свой цвет.</param>
        /// <returns>Готовый документ.</returns>
        public PdfDocument GenerateOnePerPage(
            string[] codes,
            string name = "Коды продуктов",
            Action<PDFGenerationUpdate>? codeGenerated = null,
            CancellationToken? token = null,
            bool debugRainbow = false
        ) {
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
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
                    palette = new MatrixPalette(XBrushes.White, CodeLayoutEngine.DebugBrushes[new Random().Next(0, CodeLayoutEngine.DebugBrushes.Length)]);
                }
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
                Debug.WriteLine($"[PG.GenerateOnePerPage] generating code for #{done}");
#endif
                var codeData = LayoutEngine.GenerateMatrix(code, 0, 0, palette);
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
                Debug.WriteLine($"[PG.GenerateOnePerPage] generated code for #{done}");
                Debug.WriteLine(code);
#endif
                foreach (var block in codeData) {
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
                    Debug.WriteLine($"[PG.GenerateOnePerPage] drawing block {block}");
#endif
                    gfx.DrawBlock(block);//, (block.brush == XBrushes.White ? XBrushes.White : DebugBrushes[new Random().Next(0, DebugBrushes.Length)]));
                }
                // отправляем сигнал
                done++;
                codeGenerated?.Invoke(new PDFGenerationUpdate(
                    PDFGenerationStage.Layout,
                    done,
                    (float)(done + 1) / codes.Length,
                    codes.Length
                ));
            }
            return document;
        }

        /// <summary>
        /// Генерирует документ с кодами, где на странице может быть несколько кодов.
        /// </summary>
        /// <param name="codes">Коды.</param>
        /// <param name="name">Название документа.</param>
        /// <param name="codeGenerated">Событие, которое вызывается при верстании, а потом при размещении каждого кода.</param>
        /// <param name="token">Токен отмены (для работы на отдельном потоке).</param>
        /// <param name="printPreview">Активирует режим предпросмотра.</param>
        /// <param name="debugRainbow">Функция для отладки: каждый код будет иметь свой цвет.</param>
        /// <returns>Готовый документ.</returns>
        public PdfDocument GenerateMultiple(
            string[] codes,
            string name = "Коды продуктов",
            Action<PDFGenerationUpdate>? codeGenerated = null,
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
                var layout = LayoutEngine.LayoutMultiple(codes, codeGenerated, token, printPreview, debugRainbow);
                for (int pageIndex = 0; pageIndex < layout.Length; pageIndex++) {
                    PdfPage myPage = document.AddPage();
                    MatrixBlock[] blockPage = layout[pageIndex];
                    // размер
                    myPage.Width = XUnit.FromMillimeter(Options.PaperType.Size.Width);
                    myPage.Height = XUnit.FromMillimeter(Options.PaperType.Size.Height);
                    // создаем XGraphics
                    var gfx = XGraphics.FromPdfPage(myPage);
                    for (int i = 0; i < blockPage.Length; i++) {
                        // если есть запрос отмены генерации:
                        if (token?.IsCancellationRequested == true) {
                            // освобождаем память
                            gfx.Dispose();
                            document.Dispose();
                            // даем ошибку
                            token?.ThrowIfCancellationRequested();
                        }
#if PDFGEN_DEBUG || PDFGEN_MAIN_DEBUG
                        Debug.WriteLine($"[PG.GenerateMultiple] drawing block {block}");
#endif
                        gfx.DrawBlock(blockPage[i]);//, (block.brush == XBrushes.White ? XBrushes.White : DebugBrushes[new Random().Next(0, DebugBrushes.Length)]));
                        codeGenerated?.Invoke(new PDFGenerationUpdate(
                            PDFGenerationStage.Render,
                            i,
                            (float)i / blockPage.Length,
                            layout.Length,
                            pageIndex
                        ));
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
    }
}
