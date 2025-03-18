using Matritsa.PDFGenerator.Data;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
#if PDFGEN_DEBUG
using System.Diagnostics;
using System.Text;
#endif

namespace Matritsa.PDFGenerator {

    public class PDFBlockGenerator {
        /// <summary>
        /// Превращает 2-битное изображение в блоки для добавления в PDF.
        /// </summary>
        /// <param name="data">Содержимое изображения, где true - черный, false - белый.</param>
        /// <param name="pixelSize">Размер пикселя.</param>
        /// <param name="startX">X-координата начала.</param>
        /// <param name="startY">Y-координата начала.</param>
        /// <param name="palette">Цветовая палитра.</param>
        /// <returns>Список блоков, которые надо добавить в PDF. В координатах блоков учтены указанные координаты начала.</returns>
        public static MatrixBlock[] Get2BitImageAsBlocks(
            bool[,] data, float pixelSize, float startX, float startY, TwoBitPalette palette
        ) {
            var rects = new List<MatrixBlock>();
            for (int y = 0; y < data.GetLength(0); y++) {
                bool lastValue = true;
                int blockLength = 0;
                int offsetIntoData = 0;
                for (int x = 0; x < data.GetLength(1); x++) {
                    // если значение поменялось, обьявляем новый блок
                    if (data[x, y] != lastValue) {
#if PDFGEN_DEBUG
                        Debug.WriteLine($"[PBG.Get2BIAB] diff value detected! at x={x} y={y}, {data[x, y]} != {lastValue} (len={blockLength})");
#endif
                        var sbrush = palette.GetBrush(lastValue);
                        if (sbrush != null) {
#if PDFGEN_DEBUG
                            Debug.WriteLine($"[PBG.Get2BIAB] pushing block");
                            Debug.WriteLine($"[PBG.Get2BIAB] X: {Math.Max(startX + (offsetIntoData - blockLength) * pixelSize, 0)}");
                            Debug.WriteLine($"[PBG.Get2BIAB] X: Math.Max({startX} + ({offsetIntoData} - {blockLength}) * {pixelSize}, 0)");
                            Debug.WriteLine($"[PBG.Get2BIAB] Y: {startY + y * pixelSize}");
                            Debug.WriteLine($"[PBG.Get2BIAB] Y: {startY} + {y} * {pixelSize}");
                            Debug.WriteLine($"[PBG.Get2BIAB] W: {pixelSize * blockLength}");
                            Debug.WriteLine($"[PBG.Get2BIAB] W: {pixelSize} * {blockLength}");
                            Debug.WriteLine($"[PBG.Get2BIAB] H: {pixelSize}");
#endif
                            rects.Add(
                                new MatrixBlock(
                                    new XRect(
                                        // стартовое значение + конец прошлого блока (offsetIntoData включает размер данного блока)
                                        Math.Max(startX + (offsetIntoData - blockLength) * pixelSize, 0),
                                        startY + y * pixelSize,
                                        // задаем размер
                                        pixelSize * blockLength,
                                        pixelSize
                                    ),
                                    sbrush
                                )
                            );
                        }
                        // обнуляем длину блока
                        blockLength = 0;
                        lastValue = data[x, y];
                    }
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PBG.Get2BIAB]updating blen and oid (was len={blockLength} oid={offsetIntoData})");
#endif
                    // обновляем переменные
                    blockLength++;
                    offsetIntoData++;
                }
#if PDFGEN_DEBUG
                Debug.WriteLine($"[PBG.Get2BIAB] loop done (len={blockLength} oid={offsetIntoData})");
#endif
                // добавляем оставшееся в блок
                var brush = palette.GetBrush(lastValue);
                if (brush != null) {
#if PDFGEN_DEBUG
                    Debug.WriteLine($"[PBG.Get2BIAB] creating sub");
#endif
                    rects.Add(
                        new MatrixBlock(
                            new XRect(
                                // стартовое значение + конец прошлого блока (offsetIntoData включает размер данного блока)
                                startX + (offsetIntoData - blockLength) * pixelSize,
                                startY + y * pixelSize,
                                // задаем размер
                                pixelSize * blockLength,
                                pixelSize
                            ),
                            brush
                        )
                    );
                }
            }
            return rects.ToArray();
        }

        /// <summary>
        /// Превращает 2-битное изображение в блоки для добавления в PDF.
        /// </summary>
        /// <param name="data">Содержимое изображения, где true - черный, false - белый.</param>
        /// <param name="pixelSize">Размер пикселя.</param>
        /// <param name="startX">X-координата начала.</param>
        /// <param name="startY">Y-координата начала.</param>
        /// <param name="palette">Цветовая палитра.</param>
        /// <returns>Список блоков, которые надо добавить в PDF. В координатах блоков учтены указанные координаты начала.</returns>
        public static MatrixBlock[] Get2BitImageAsBlocksSimple(
            bool[,] data, float pixelSize, float startX, float startY, TwoBitPalette palette
        ) {
            var rects = new List<MatrixBlock>();
            for (int y = 0; y < data.GetLength(0); y++) {
                for (int x = 0; x < data.GetLength(1); x++) {
                    // если значение поменялось, обьявляем новый блок
                    var sbrush = palette.GetBrush(data[x, y]);
                    if (sbrush != null) {
                        rects.Add(
                            new MatrixBlock(
                                new XRect(
                                    // стартовое значение + конец прошлого блока (offsetIntoData включает размер данного блока)
                                    Math.Max(startX + x * pixelSize, 0),
                                    startY + y * pixelSize,
                                    // задаем размер
                                    pixelSize,
                                    pixelSize
                                ),
                                sbrush
                            )
                        );
                    }
                }
            }
            return rects.ToArray();
        }
    }
}
