using Matritsa.PDFGenerator.Data;
using System;

namespace Matritsa.PDFGenerator.Util {
    /// <summary>
    /// Функции для превращения размеров в поинты.
    /// </summary>
    public static class PointConversion {
        public static float GetCoordinateFromInch(float inches) {
            return inches * 72;
        }
        public static float GetCoordinateFromCentimeter(float centimeters) {
            return centimeters * 0.3937f * 72;
        }
        public static float GetCoordinateFromMillimeter(float millimeters) {
            return (millimeters / 10) * 0.3937f * 72;
        }

        /// <summary>
        /// Превращает координату в поинты (1/72 дюйма).
        /// </summary>
        /// <param name="coordinate">Координата.</param>
        /// <param name="unit">Единица измерения.</param>
        /// <param name="enforceUnit">Если true, при неуказанной или неизвестной величине будет возвращаться ошибка, а если false - изначальная координата.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float ToPoints(float coordinate, MeasurementUnit unit, bool enforceUnit = true) {
            switch (unit) {
                case MeasurementUnit.Centimeter:
                    return GetCoordinateFromCentimeter(coordinate);
                case MeasurementUnit.Millimeter:
                    return GetCoordinateFromMillimeter(coordinate);
                case MeasurementUnit.Inch:
                    return GetCoordinateFromInch(coordinate);
                case MeasurementUnit.Point:
                    return coordinate;
                case MeasurementUnit.None:
                    if (enforceUnit)  throw new ArgumentException("Unit is None and unit enforcement is enabled; either specify a unit or set enforceUnit to false");
                    return coordinate;
                default:
                    if (enforceUnit) throw new ArgumentException("Unit is unknown and unit enforcement is enabled; either specify a unit or set enforceUnit to false");
                    return coordinate;
            }
        }
    }
}
