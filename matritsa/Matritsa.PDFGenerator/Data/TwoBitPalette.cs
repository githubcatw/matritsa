using PdfSharp.Drawing;

namespace Matritsa.PDFGenerator.Data {
    /// <summary>
    /// Цветовая палитра для 2-битных изображений.
    /// </summary>
    public class TwoBitPalette {
        /// <summary>
        /// Цвет блоков, для которых получено значение true. Если null, блоки для значения true не создаются.
        /// </summary>
        public XBrush? trueBrush;
        /// <summary>
        /// Цвет блоков, для которых получено значение false. Если null, блоки для значения false не создаются.
        /// </summary>
        public XBrush? falseBrush;

        public TwoBitPalette(XBrush? trueBrush, XBrush? falseBrush) {
            this.trueBrush = trueBrush;
            this.falseBrush = falseBrush;
        }

        public XBrush? GetBrush(bool value) {
            return value ? trueBrush : falseBrush;
        }

        public static TwoBitPalette Default = new TwoBitPalette(XBrushes.Black, XBrushes.White);
    }
}
