using PdfSharp.Drawing;
using System;

namespace Matritsa.PDFGenerator.Data {
    public class MatrixBlock {
        public XRect rect;
        public XBrush brush;

        public MatrixBlock(XRect rect, XBrush brush) {
            this.rect = rect;
            this.brush = brush ?? throw new ArgumentNullException(nameof(brush));
        }

        public MatrixBlock(XRect rect, bool brush) {
            this.rect = rect;
            this.brush = brush ? XBrushes.Black : XBrushes.White;
        }

        public override string ToString() {
            return $"MatrixBlock(rect={rect}, brush={brush})";
        }
    }

    public static class MatrixBlockExtensions {

        public static void DrawBlock(this XGraphics gfx, MatrixBlock block, XBrush? brush = null) {
            gfx.DrawRectangle(brush ?? block.brush, block.rect);
        }
    }
}
