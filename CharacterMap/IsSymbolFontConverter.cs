using System;
using System.Globalization;
using System.Windows.Data;

namespace CharacterMap {

    public sealed class IsSymbolFontConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            dynamic d = value;
            try {
                if (d?.IsSymbol == true)
                    return "Segoe UI";

                return d?.Name;
            } catch (Exception ex) {

            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}