using System.Globalization;
using System.Windows.Data;
using ModernWpf.Controls;
using PlcDiff.Core.Models;

namespace PlcDiff.App.Converters;

public sealed class ChangeKindToSymbolConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ChangeKind changeKind)
        {
            return Symbol.Help;
        }

        return changeKind switch
        {
            ChangeKind.Added => Symbol.Add,
            ChangeKind.Removed => Symbol.Remove,
            ChangeKind.Modified => Symbol.Edit,
            ChangeKind.Moved => Symbol.Switch,
            _ => Symbol.Help
        };
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
