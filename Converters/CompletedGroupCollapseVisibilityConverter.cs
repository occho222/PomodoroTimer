using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroTimer.Converters
{
    public class CompletedGroupCollapseVisibilityConverter : IValueConverter
    {
        public static Func<string, bool>? IsCompletedGroupCollapsed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category && IsCompletedGroupCollapsed != null)
            {
                var normalizedCategory = string.IsNullOrWhiteSpace(category) ? "その他" : category;
                return IsCompletedGroupCollapsed(normalizedCategory) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}