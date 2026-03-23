using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClinicAdmin.Desktop.Converters;

public sealed class RegistrationStatusBackgroundConverter : IValueConverter
{
    private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(255, 245, 245));
    private static readonly Brush SuccessBrush = new SolidColorBrush(Color.FromRgb(236, 253, 245));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? ErrorBrush : SuccessBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
