using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PomodoroTimer.Views
{
    public partial class InputDialog : Window, INotifyPropertyChanged
    {
        private string _dialogTitle = string.Empty;
        private string _message = string.Empty;
        private string _inputText = string.Empty;

        public string DialogTitle
        {
            get => _dialogTitle;
            set
            {
                _dialogTitle = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public InputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            DataContext = this;
            
            DialogTitle = title;
            Message = message;
            InputText = defaultValue;
            
            // ダイアログが表示された後にテキストボックスにフォーカスを設定
            Loaded += (s, e) =>
            {
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
            
            // Enterキーでも確定できるように
            InputTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    OkButton_Click(this, new RoutedEventArgs());
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    CancelButton_Click(this, new RoutedEventArgs());
                }
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}