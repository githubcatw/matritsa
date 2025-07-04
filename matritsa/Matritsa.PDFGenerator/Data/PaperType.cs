﻿namespace Matritsa.PDFGenerator.Data {
    public class PaperType {
        /// <summary>
        /// Размер страницы в миллиметрах.
        /// </summary>
        public Dimensions<float> Size;
        public Dimensions1D<float> Padding;
        public MeasurementUnit Unit;

        public Dimensions<float> GetBoundingBoxSize() {
            return new Dimensions<float>(Size.Width - 2 * Padding.Size, Size.Height - 2 * Padding.Size, Size.Unit);
        }

        public PaperType(Dimensions<float> size, Dimensions1D<float> padding) {
            this.Size = size;
            this.Padding = padding;
            this.Unit = size.Unit;
        }

        public PaperType(float width, float height, float padding, MeasurementUnit unit) {
            this.Size = new Dimensions<float>(width, height, unit);
            this.Padding = new Dimensions1D<float>(padding, unit);
            this.Unit = unit;
        }

        public static PaperType A4 = new PaperType(297, 210, 10, MeasurementUnit.Millimeter);
    }
}
