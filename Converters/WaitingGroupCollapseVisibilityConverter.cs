using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PomodoroTimer.Converters
{
    public class WaitingGroupCollapseVisibilityConverter : IValueConverter
    {
        public static Func<string, bool>? IsWaitingGroupCollapsed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category && IsWaitingGroupCollapsed != null)
            {
                var normalizedCategory = string.IsNullOrWhiteSpace(category) ? "その他" : category;
                return IsWaitingGroupCollapsed(normalizedCategory) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}