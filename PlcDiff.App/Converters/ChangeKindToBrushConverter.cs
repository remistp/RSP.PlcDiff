using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Converters;

public sealed class ChangeKindToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ChangeKind changeKind)
        {
            return Brushes.Transparent;
        }

        return changeKind switch
        {
            ChangeKind.Added => Brushes.ForestGreen,
            ChangeKind.Removed => Brushes.IndianRed,
            ChangeKind.Modified => Brushes.Goldenrod,
            ChangeKind.Moved => Brushes.DodgerBlue,
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
