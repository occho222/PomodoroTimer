using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroTimer.Models;

namespace PomodoroTimer.Converters
{
    /// <summary>
    /// �^�X�N�D��x��F�ɕϊ�����R���o�[�^�[
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
    /// �^�X�N�D��x����{��e�L�X�g�ɕϊ�����R���o�[�^�[
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
    /// �`���[�g�l�𕝂ɕϊ�����R���o�[�^�[
    /// </summary>
    public class ValueToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double value && values[1] is double maxWidth)
            {
                // �ő�l��10�Ƃ��Đ��K���i�|���h�[�����̏ꍇ�j
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
    /// �u�[���l�������ɕϊ�����R���o�[�^�[
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ������̏ꍇ�͋�łȂ����`�F�b�N
            if (value is string stringValue)
            {
                return !string.IsNullOrEmpty(stringValue) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            
            // �u�[���l�̏ꍇ
            if (value is bool boolValue)
            {
                return boolValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            
            // ���̑��̃I�u�W�F�N�g�̏ꍇ��null�`�F�b�N
            return value != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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
    /// �u�[���l�𔽓]�����ɕϊ�����R���o�[�^�[
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

    /// <summary>
    /// ブール値をチェックマークに変換するコンバーター
    /// </summary>
    public class BooleanToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "✓" : "□";
            }
            return "□";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Null以外の値をbooleanに変換するコンバーター
    /// </summary>
    public class NotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Null以外の値をVisibilityに変換するコンバーター
    /// </summary>
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// NullをVisibilityに変換するコンバーター
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// カウント値をVisibilityに変換するコンバーター
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}