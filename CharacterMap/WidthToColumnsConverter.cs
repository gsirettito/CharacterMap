using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CharacterMap {
    public sealed class WidthToColumnsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is double actualWidth) {
                double itemWidth = 84;
                double margin = 5;

                double availableWidth = actualWidth - margin * 2 - SystemParameters.VerticalScrollBarWidth;

                int columns = Math.Max(1, (int)(availableWidth / itemWidth));

                if(parameter != null && int.TryParse(parameter.ToString(), out int maxColumns)) {
                    return Math.Min(columns, maxColumns);
                }

                return columns;
            }
            return 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public sealed class HeightToRowsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double actualHeight) {
                double itemHeight = 84;
                double margin = 5;

                double availableHeight = actualHeight - margin * 2 - SystemParameters.HorizontalScrollBarHeight;

                int rows = Math.Max(1, (int)(availableHeight / itemHeight));

                if (parameter != null && int.TryParse(parameter.ToString(), out int maxRows)) {
                    return Math.Min(rows, maxRows);
                }

                return rows;
            }
            return 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}