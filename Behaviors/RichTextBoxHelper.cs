using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PomodoroTimer.Behaviors
{
    /// <summary>
    /// RichTextBoxのDocumentプロパティをバインディング可能にするヘルパー
    /// </summary>
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                "Document",
                typeof(FlowDocument),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(null, OnDocumentChanged));

        public static FlowDocument? GetDocument(DependencyObject obj)
        {
            return (FlowDocument?)obj.GetValue(DocumentProperty);
        }

        public static void SetDocument(DependencyObject obj, FlowDocument? value)
        {
            obj.SetValue(DocumentProperty, value);
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.RichTextBox richTextBox)
            {
                var newDocument = e.NewValue as FlowDocument;
                if (newDocument != null)
                {
                    richTextBox.Document = newDocument;
                }
                else
                {
                    richTextBox.Document = new FlowDocument();
                }
            }
        }
    }
}