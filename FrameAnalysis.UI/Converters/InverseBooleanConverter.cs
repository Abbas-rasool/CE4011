using System;
using System.Globalization;
using System.Windows.Data;

namespace FrameAnalysis.UI.Converters
{
    /// <summary>Returns the logical negation of a bool. Used to drive a cell's
    /// <c>IsReadOnly</c> from <c>ManualOverride</c> (read-only unless override is on).</summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : true;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : false;
    }
}
