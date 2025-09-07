using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 日付範囲選択ダイアログ
    /// </summary>
    public partial class DateRangeSelectionDialog : Window
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-6);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool IsExportRequested { get; private set; } = false;

        public DateRangeSelectionDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            UpdateDatePickers();
        }

        private void YesterdayButton_Click(object sender, RoutedEventArgs e)
        {
            StartDate = DateTime.Today.AddDays(-1);
            EndDate = DateTime.Today.AddDays(-1);
            UpdateDatePickers();
        }

        private void Last3DaysButton_Click(object sender, RoutedEventArgs e)
        {
            StartDate = DateTime.Today.AddDays(-2);
            EndDate = DateTime.Today;
            UpdateDatePickers();
        }

        private void ThisWeekButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var startOfWeek = dayOfWeek == 0 ? today.AddDays(-6) : today.AddDays(-(dayOfWeek - 1));
            
            StartDate = startOfWeek;
            EndDate = today;
            UpdateDatePickers();
        }

        private void LastWeekButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var startOfLastWeek = dayOfWeek == 0 ? today.AddDays(-13) : today.AddDays(-(dayOfWeek - 1) - 7);
            var endOfLastWeek = startOfLastWeek.AddDays(6);
            
            StartDate = startOfLastWeek;
            EndDate = endOfLastWeek;
            UpdateDatePickers();
        }

        private void Last7DaysButton_Click(object sender, RoutedEventArgs e)
        {
            StartDate = DateTime.Today.AddDays(-6);
            EndDate = DateTime.Today;
            UpdateDatePickers();
        }

        private void ThisMonthButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = today;
            UpdateDatePickers();
        }

        private void LastMonthButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);
            
            StartDate = firstDayOfLastMonth;
            EndDate = lastDayOfLastMonth;
            UpdateDatePickers();
        }

        private void UpdateDatePickers()
        {
            StartDatePicker.SelectedDate = StartDate;
            EndDatePicker.SelectedDate = EndDate;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // 日付の検証
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("開始日と終了日を選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StartDate = StartDatePicker.SelectedDate.Value.Date;
            EndDate = EndDatePicker.SelectedDate.Value.Date;

            if (StartDate > EndDate)
            {
                System.Windows.MessageBox.Show("開始日は終了日より前の日付を選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EndDate > DateTime.Today)
            {
                System.Windows.MessageBox.Show("終了日は今日以前の日付を選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var totalDays = (EndDate - StartDate).Days + 1;
            if (totalDays > 365)
            {
                System.Windows.MessageBox.Show("分析期間は365日以内で設定してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsExportRequested = true;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}