using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace matritsa.Util {
    public class SafeFloatConverter : IValueConverter {
        public static readonly SafeFloatConverter Instance = new();

        public object? ConvertBack(object? value, Type targetType,
                                        object? parameter, CultureInfo culture) {
            if (value is string st) {
                var nr = st.GetNumbers();
                if (nr == "") return null;
                return float.Parse(nr);
            }
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object Convert(object? value, Type targetType,
                                        object? parameter, CultureInfo culture) {
            if (value is float fl) {
                return fl.ToString();
            }
            if (value is null) {
                return "";
            }
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }
}
