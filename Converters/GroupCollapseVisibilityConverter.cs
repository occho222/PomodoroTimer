using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroTimer.Converters
{
    public class GroupCollapseVisibilityConverter : IValueConverter
    {
        public static Func<string, bool>? IsGroupCollapsed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category && IsGroupCollapsed != null)
            {
                var normalizedCategory = string.IsNullOrWhiteSpace(category) ? "その他" : category;
                return IsGroupCollapsed(normalizedCategory) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}