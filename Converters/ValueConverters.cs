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
                    TaskPriority.Low => "��",
                    TaskPriority.Medium => "��",
                    TaskPriority.High => "��",
                    TaskPriority.Urgent => "�ً}",
                    _ => "�s��"
                };
            }
            return "�s��";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "��" => TaskPriority.Low,
                    "��" => TaskPriority.Medium,
                    "��" => TaskPriority.High,
                    "�ً}" => TaskPriority.Urgent,
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
}