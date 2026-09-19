using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DocBookKeeping.Converters;

public class ActiveMenuBackgroundConverter : IValueConverter
{
    public static readonly ActiveMenuBackgroundConverter Instance = new();
    private static readonly IBrush Active = new SolidColorBrush(Color.Parse("#26344D"));
    private static readonly IBrush Inactive = Brushes.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString() ? Active : Inactive;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ActiveMenuIconForegroundConverter : IValueConverter
{
    public static readonly ActiveMenuIconForegroundConverter Instance = new();
    private static readonly IBrush Active = Brushes.White;
    private static readonly IBrush Inactive = new SolidColorBrush(Color.Parse("#94A3B8"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString() ? Active : Inactive;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ActiveMenuTextForegroundConverter : IValueConverter
{
    public static readonly ActiveMenuTextForegroundConverter Instance = new();
    private static readonly IBrush Active = Brushes.White;
    private static readonly IBrush Inactive = new SolidColorBrush(Color.Parse("#CBD5E1"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString() ? Active : Inactive;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}