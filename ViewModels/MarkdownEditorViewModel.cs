using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// マークダウンエディタのViewModel
    /// </summary>
    public partial class MarkdownEditorViewModel : ObservableObject
    {
        /// <summary>
        /// エディタモードかどうか
        /// </summary>
        [ObservableProperty]
        private bool isEditorMode = true;

        /// <summary>
        /// マークダウンテキスト
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RenderedMarkdown))]
        private string markdownText = string.Empty;

        /// <summary>
        /// レンダリングされたマークダウン（FlowDocument）
        /// </summary>
        public FlowDocument RenderedMarkdown
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(MarkdownText))
                    {
                        return CreateEmptyDocument();
                    }

                    return ConvertMarkdownToFlowDocument(MarkdownText);
                }
                catch
                {
                    return CreateEmptyDocument();
                }
            }
        }

        /// <summary>
        /// エディタモードに切り替えるコマンド
        /// </summary>
        [RelayCommand]
        private void SwitchToEditor()
        {
            IsEditorMode = true;
        }

        /// <summary>
        /// ビューアモードに切り替えるコマンド
        /// </summary>
        [RelayCommand]
        private void SwitchToViewer()
        {
            IsEditorMode = false;
        }

        /// <summary>
        /// 空のドキュメントを作成
        /// </summary>
        private FlowDocument CreateEmptyDocument()
        {
            var document = new FlowDocument();
            var paragraph = new Paragraph(new Run("マークダウンで説明を記述してください。"));
            paragraph.Foreground = System.Windows.Media.Brushes.Gray;
            paragraph.FontStyle = FontStyles.Italic;
            document.Blocks.Add(paragraph);
            return document;
        }

        /// <summary>
        /// マークダウンテキストを直接FlowDocumentに変換
        /// </summary>
        private FlowDocument ConvertMarkdownToFlowDocument(string markdown)
        {
            var document = new FlowDocument();
            
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return CreateEmptyDocument();
            }

            var lines = markdown.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    // 空行はスペースを追加
                    var emptyParagraph = new Paragraph(new Run(" "));
                    emptyParagraph.FontSize = 4;
                    document.Blocks.Add(emptyParagraph);
                    continue;
                }
                
                var paragraph = new Paragraph();
                
                // 見出しの処理
                if (trimmedLine.StartsWith("# "))
                {
                    paragraph.FontSize = 24;
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.Margin = new Thickness(0, 16, 0, 8);
                    paragraph.Inlines.Add(new Run(trimmedLine.Substring(2)));
                }
                else if (trimmedLine.StartsWith("## "))
                {
                    paragraph.FontSize = 20;
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.Margin = new Thickness(0, 14, 0, 6);
                    paragraph.Inlines.Add(new Run(trimmedLine.Substring(3)));
                }
                else if (trimmedLine.StartsWith("### "))
                {
                    paragraph.FontSize = 16;
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.Margin = new Thickness(0, 12, 0, 4);
                    paragraph.Inlines.Add(new Run(trimmedLine.Substring(4)));
                }
                else if (trimmedLine.StartsWith("#### "))
                {
                    paragraph.FontSize = 14;
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.Margin = new Thickness(0, 10, 0, 3);
                    paragraph.Inlines.Add(new Run(trimmedLine.Substring(5)));
                }
                // リストアイテムの処理
                else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                {
                    paragraph.Margin = new Thickness(16, 2, 0, 2);
                    paragraph.Inlines.Add(new Run("• " + trimmedLine.Substring(2)));
                }
                // 番号付きリストの処理
                else if (System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^\d+\. "))
                {
                    paragraph.Margin = new Thickness(16, 2, 0, 2);
                    paragraph.Inlines.Add(new Run(trimmedLine));
                }
                // コードブロックの処理
                else if (trimmedLine.StartsWith("```"))
                {
                    // コードブロックは別途処理が必要だが、ここでは簡単に
                    paragraph.FontFamily = new System.Windows.Media.FontFamily("Cascadia Code, Consolas, Courier New");
                    paragraph.Background = System.Windows.Media.Brushes.LightGray;
                    paragraph.Padding = new Thickness(8, 4, 8, 4);
                    paragraph.Inlines.Add(new Run(trimmedLine));
                }
                // 引用の処理
                else if (trimmedLine.StartsWith("> "))
                {
                    paragraph.Margin = new Thickness(16, 4, 0, 4);
                    paragraph.Padding = new Thickness(8, 0, 0, 0);
                    paragraph.BorderBrush = System.Windows.Media.Brushes.Gray;
                    paragraph.BorderThickness = new Thickness(2, 0, 0, 0);
                    paragraph.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                    paragraph.Inlines.Add(new Run(trimmedLine.Substring(2)));
                }
                // 通常のテキストの処理（太字、斜体、コードなど）
                else
                {
                    ProcessInlineMarkdown(paragraph, trimmedLine);
                }
                
                document.Blocks.Add(paragraph);
            }
            
            return document;
        }

        /// <summary>
        /// インラインマークダウン（太字、斜体、コードなど）を処理
        /// </summary>
        private void ProcessInlineMarkdown(Paragraph paragraph, string text)
        {
            var parts = SplitByMarkdown(text);
            
            foreach (var part in parts)
            {
                if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
                {
                    // 太字
                    var run = new Run(part.Substring(2, part.Length - 4));
                    run.FontWeight = FontWeights.Bold;
                    paragraph.Inlines.Add(run);
                }
                else if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2)
                {
                    // 斜体
                    var run = new Run(part.Substring(1, part.Length - 2));
                    run.FontStyle = FontStyles.Italic;
                    paragraph.Inlines.Add(run);
                }
                else if (part.StartsWith("`") && part.EndsWith("`") && part.Length > 2)
                {
                    // インラインコード
                    var run = new Run(part.Substring(1, part.Length - 2));
                    run.FontFamily = new System.Windows.Media.FontFamily("Cascadia Code, Consolas, Courier New");
                    run.Background = System.Windows.Media.Brushes.LightGray;
                    paragraph.Inlines.Add(run);
                }
                else
                {
                    // 通常のテキスト
                    paragraph.Inlines.Add(new Run(part));
                }
            }
        }

        /// <summary>
        /// マークダウン記号で文字列を分割
        /// </summary>
        private List<string> SplitByMarkdown(string text)
        {
            var parts = new List<string>();
            var current = "";
            var i = 0;
            
            while (i < text.Length)
            {
                if (i < text.Length - 1 && text[i] == '*' && text[i + 1] == '*')
                {
                    // **太字**
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                    var endIndex = text.IndexOf("**", i + 2);
                    if (endIndex > i + 2)
                    {
                        parts.Add(text.Substring(i, endIndex - i + 2));
                        i = endIndex + 2;
                    }
                    else
                    {
                        current += text[i];
                        i++;
                    }
                }
                else if (text[i] == '*')
                {
                    // *斜体*
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                    var endIndex = text.IndexOf("*", i + 1);
                    if (endIndex > i + 1)
                    {
                        parts.Add(text.Substring(i, endIndex - i + 1));
                        i = endIndex + 1;
                    }
                    else
                    {
                        current += text[i];
                        i++;
                    }
                }
                else if (text[i] == '`')
                {
                    // `コード`
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                    var endIndex = text.IndexOf("`", i + 1);
                    if (endIndex > i + 1)
                    {
                        parts.Add(text.Substring(i, endIndex - i + 1));
                        i = endIndex + 1;
                    }
                    else
                    {
                        current += text[i];
                        i++;
                    }
                }
                else
                {
                    current += text[i];
                    i++;
                }
            }
            
            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current);
            }
            
            return parts;
        }

        /// <summary>
        /// HTMLをFlowDocumentに変換（簡易版）
        /// </summary>
        private FlowDocument ConvertHtmlToFlowDocument(string html)
        {
            // HTMLからマークダウンに戻して処理する簡易的な方法
            // 実際のプロダクションでは、HTMLパーサーを使用することを推奨
            return ConvertMarkdownToFlowDocument(MarkdownText);
        }
    }
}