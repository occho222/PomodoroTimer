using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroTimer.Models;

namespace PomodoroTimer.Converters
{
    /// <summary>
    /// タスク優先度を色に変換するコンバーター
    /// </summary>
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Low => Colors.Green,
                    TaskPriority.Medium => Colors.Orange,
                    TaskPriority.High => Colors.Red,
                    TaskPriority.Urgent => Colors.DarkRed,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// タスク優先度を日本語テキストに変換するコンバーター
    /// </summary>
    public class PriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Low => "低",
                    TaskPriority.Medium => "中",
                    TaskPriority.High => "高",
                    TaskPriority.Urgent => "緊急",
                    _ => "不明"
                };
            }
            return "不明";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "低" => TaskPriority.Low,
                    "中" => TaskPriority.Medium,
                    "高" => TaskPriority.High,
                    "緊急" => TaskPriority.Urgent,
                    _ => TaskPriority.Medium
                };
            }
            return TaskPriority.Medium;
        }
    }

    /// <summary>
    /// チャート値を幅に変換するコンバーター
    /// </summary>
    public class ValueToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double value && values[1] is double maxWidth)
            {
                // 最大値を10として正規化（ポモドーロ数の場合）
                var normalizedValue = Math.Min(value / 10.0, 1.0);
                return maxWidth * normalizedValue;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ブール値を可視性に変換するコンバーター
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Visibility visibility)
            {
                return visibility == System.Windows.Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// ブール値を反転可視性に変換するコンバーター
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Visibility visibility)
            {
                return visibility == System.Windows.Visibility.Collapsed;
            }
            return true;
        }
    }
}