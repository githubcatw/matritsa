using Matritsa.PDFGenerator.Util;

namespace Matritsa.PDFGenerator.Data {
    public class Dimensions<T> {
        /// <summary>
        /// Ширина.
        /// </summary>
        public T Width;
        /// <summary>
        /// Высота.
        /// </summary>
        public T Height;
        /// <summary>
        /// Единица измерения.
        /// </summary>
        public MeasurementUnit Unit;

        public Dimensions(T width, T height, MeasurementUnit unit) {
            Width = width;
            Height = height;
            Unit = unit;
        }

        public override string ToString() {
            return $"{Width}x{Height} {Unit.ToString().ToLower()}";
        }
    }

    public enum MeasurementUnit {
        /// <summary>
        /// Не указана.
        /// <br></br>
        /// Конвертирование в поинты с помощью <see cref="PointConversion.ToPoints(float, MeasurementUnit, bool)"/> может не сработать.
        /// </summary>
        None,
        /// <summary>
        /// Миллиметр.
        /// </summary>
        Millimeter,
        /// <summary>
        /// Сантиметр.
        /// </summary>
        Centimeter,
        /// <summary>
        /// Дюйм.
        /// </summary>
        Inch,
        /// <summary>
        /// Поинт (1/72 дюйма).
        /// </summary>
        Point
    }

    /// <summary>
    /// Добавляет математические действия для измерений, обьявленных
    /// с помощью класса <see cref="Dimensions{T}"/>.
    /// </summary>
    public static class DimensionMathExtensions {

        /// <summary>
        /// Считает площадь.
        /// </summary>
        public static int Area(this Dimensions<int> dims) {
            return dims.Height * dims.Width;
        }

        /// <summary>
        /// Считает площадь.
        /// </summary>
        public static float Area(this Dimensions<float> dims) {
            return dims.Height * dims.Width;
        }

        /// <summary>
        /// Превращает размер в поинты (1/72 дюйма).
        /// </summary>
        public static Dimensions<float> ToPoints(this Dimensions<float> dims) {
            return new Dimensions<float>(
                PointConversion.ToPoints(dims.Width, dims.Unit),
                PointConversion.ToPoints(dims.Height, dims.Unit),
                MeasurementUnit.Point
            );
        }
    }
}
