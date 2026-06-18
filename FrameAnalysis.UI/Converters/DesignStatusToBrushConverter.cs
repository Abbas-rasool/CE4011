using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Converters
{
    /// <summary>
    /// Maps a timber <see cref="eDesignStatus"/> to a brush for the Design Results grid:
    /// Pass → green, Warning → orange, Fail → red. Used to color status cells / headers.
    /// </summary>
    public sealed class DesignStatusToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Pass = new(Color.FromRgb(0x2E, 0x7D, 0x32));
        private static readonly SolidColorBrush Warn = new(Color.FromRgb(0xE6, 0x7E, 0x22));
        private static readonly SolidColorBrush Fail = new(Color.FromRgb(0xC0, 0x39, 0x2B));
        private static readonly SolidColorBrush Unknown = Brushes.Gray;

        static DesignStatusToBrushConverter()
        {
            Pass.Freeze();
            Warn.Freeze();
            Fail.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                eDesignStatus.Pass => Pass,
                eDesignStatus.Warning => Warn,
                eDesignStatus.Fail => Fail,
                _ => Unknown,
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
