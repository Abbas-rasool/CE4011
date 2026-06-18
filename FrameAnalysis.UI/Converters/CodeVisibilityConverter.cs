using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrameAnalysis.UI.Converters
{
    /// <summary>
    /// Shows an element only for the design code(s) it applies to. Bind to
    /// <c>Document.Design.Code</c> and pass the applicable codes as the converter parameter,
    /// e.g. <c>ConverterParameter=US</c> or <c>ConverterParameter=EC5,TR</c>. Returns
    /// <see cref="Visibility.Visible"/> when the current code is in the list, else
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class CodeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
                return Visibility.Collapsed;

            string current = value.ToString()!;
            string[] codes = ((string)parameter).Split(
                ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (string code in codes)
            {
                if (string.Equals(code, current, StringComparison.OrdinalIgnoreCase))
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
