using PdfSharp.Drawing;

namespace Matritsa.PDFGenerator.Data {
    /// <summary>
    /// Цветовая палитра для кодов DataMatrix.
    /// </summary>
    public class MatrixPalette: TwoBitPalette {
        /// <summary>
        /// Цвет фона. Если null, у матрицы не будет фона.
        /// </summary>
        public XBrush? backgroundBrush;
        /// <summary>
        /// Цвет матрицы.
        /// </summary>
        public XBrush matrixBrush;

        public MatrixPalette(XBrush? backgroundBrush, XBrush matrixBrush) : base(matrixBrush, backgroundBrush) {
            this.backgroundBrush = backgroundBrush;
            this.matrixBrush = matrixBrush;
        }

        public static new MatrixPalette Default = new MatrixPalette(null, XBrushes.Black);
        public static new MatrixPalette DefaultW = new MatrixPalette(XBrushes.White, XBrushes.Black);
    }
}
