using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FluffiesEngine.Infrastructure;

/// <summary>Color -&gt; SolidColorBrush (for live preview swatches).</summary>
public sealed class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Color c ? new SolidColorBrush(c) : Brushes.Transparent;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is SolidColorBrush b ? b.Color : Colors.Transparent;
}

/// <summary>Color -&gt; "#RRGGBB" hex string.</summary>
public sealed class ColorToHexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Color c ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

/// <summary>Returns true when the bound enum value matches the ConverterParameter; round-trips for two-way radio buttons.</summary>
public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true && parameter is not null
            ? Enum.Parse(targetType, parameter.ToString()!)
            : Binding.DoNothing;
}
