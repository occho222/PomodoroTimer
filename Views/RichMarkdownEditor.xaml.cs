using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// JIRA/Confluence風のリアルタイムマークダウンエディタ
    /// </summary>
    public partial class RichMarkdownEditor : System.Windows.Controls.UserControl
    {
        private bool _isUpdating = false;
        private DispatcherTimer _formatTimer;
        private TextPointer? _lastCaretPosition;

        public static readonly DependencyProperty PlainTextProperty =
            DependencyProperty.Register(nameof(PlainText), typeof(string), typeof(RichMarkdownEditor),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPlainTextChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(RichMarkdownEditor),
                new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

        public static readonly DependencyProperty ShowToolbarProperty =
            DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(RichMarkdownEditor),
                new FrameworkPropertyMetadata(true, OnShowToolbarChanged));

        /// <summary>
        /// 画像ペーストイベント
        /// </summary>
        public event EventHandler<ImagePasteEventArgs>? ImagePasted;

        public string PlainText
        {
            get => (string)GetValue(PlainTextProperty);
            set => SetValue(PlainTextProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool ShowToolbar
        {
            get => (bool)GetValue(ShowToolbarProperty);
            set => SetValue(ShowToolbarProperty, value);
        }

        public RichMarkdownEditor()
        {
            InitializeComponent();
            
            // 遅延フォーマットタイマーの設定
            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // より短い間隔でリアルタイム感を向上
            };
            _formatTimer.Tick += FormatTimer_Tick;
            
            // 初期ドキュメント設定
            RichEditor.Document = new FlowDocument();
            
            // Loadedイベントでツールバーの初期状態を設定
            Loaded += RichMarkdownEditor_Loaded;
        }

        private void RichMarkdownEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // ツールバーの初期表示状態を設定
            UpdateToolbarVisibility();
        }

        private void UpdateToolbarVisibility()
        {
            var toolbar = FindName("ToolbarBorder") as Border;
            var editorBorder = FindName("EditorBorder") as Border;
            
            if (toolbar != null)
            {
                toolbar.Visibility = ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
            }
            
            if (editorBorder != null)
            {
                // ツールバーが非表示の場合、エディターボーダーを全体に適用
                editorBorder.CornerRadius = ShowToolbar ? new CornerRadius(0, 0, 6, 6) : new CornerRadius(6);
            }
        }

        private static void OnPlainTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichMarkdownEditor editor && !editor._isUpdating)
            {
                var newValue = (string)e.NewValue ?? string.Empty;
                var oldValue = (string)e.OldValue ?? string.Empty;
                
                // 値が実際に変更された場合のみ処理
                if (!string.Equals(oldValue.Trim(), newValue.Trim(), StringComparison.Ordinal))
                {
                    editor.SetTextFromPlainText(newValue);
                }
            }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichMarkdownEditor editor)
            {
                editor.RichEditor.IsReadOnly = (bool)e.NewValue;
            }
        }

        private void SetTextFromPlainText(string text)
        {
            _isUpdating = true;
            try
            {
                // 現在のテキストと同じ場合は何もしない
                var currentText = new TextRange(RichEditor.Document.ContentStart, RichEditor.Document.ContentEnd).Text;
                if (string.Equals(currentText?.Trim(), text?.Trim(), StringComparison.Ordinal))
                {
                    return;
                }

                var document = new FlowDocument();
                if (!string.IsNullOrEmpty(text))
                {
                    var paragraph = new Paragraph(new Run(text));
                    document.Blocks.Add(paragraph);
                }
                else
                {
                    // 空のテキストでも空のParagraphを追加
                    var emptyParagraph = new Paragraph(new Run(""));
                    document.Blocks.Add(emptyParagraph);
                }
                
                RichEditor.Document = document;
                
                // フォーマットを適用（次のDispatcherサイクルで実行）
                Dispatcher.BeginInvoke(() => 
                {
                    ApplyMarkdownFormatting();
                }, DispatcherPriority.Loaded);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void RichEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            // プレーンテキストを更新（マークダウン構文を保持）
            _isUpdating = true;
            try
            {
                var newText = ExtractMarkdownText();
                // 実際にテキストが変更された場合のみプロパティを更新
                if (!string.Equals(PlainText?.Trim(), newText?.Trim(), StringComparison.Ordinal))
                {
                    PlainText = newText;
                }
            }
            finally
            {
                _isUpdating = false;
            }

            // キャレット位置を保存
            _lastCaretPosition = RichEditor.CaretPosition;

            // 遅延フォーマット開始
            _formatTimer.Stop();
            _formatTimer.Start();
        }

        private void RichEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _lastCaretPosition = RichEditor.CaretPosition;
        }

        private void RichEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl+V（画像ペースト）の処理
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (HandleImagePaste())
                {
                    e.Handled = true; // 画像をペーストした場合はデフォルト動作を停止
                    return;
                }
            }

            // Enterキーの場合は特別処理
            if (e.Key == Key.Enter)
            {
                HandleEnterKey();
                return;
            }
            
            // スペースキーでリアルタイムフォーマット
            if (e.Key == Key.Space)
            {
                // キャレット位置を保存
                _lastCaretPosition = RichEditor.CaretPosition;
                
                // 即座にフォーマット適用
                Dispatcher.BeginInvoke(() => 
                {
                    ApplyMarkdownFormatting();
                    
                    // フォーマット後にキャレット位置を復元
                    if (_lastCaretPosition != null)
                    {
                        try
                        {
                            RichEditor.CaretPosition = _lastCaretPosition;
                        }
                        catch
                        {
                            // 復元に失敗した場合は現在位置を維持
                        }
                    }
                }, DispatcherPriority.Input);
            }
        }

        private void HandleEnterKey()
        {
            // Enterキー後の処理を遅延実行
            Dispatcher.BeginInvoke(() =>
            {
                var caretPosition = RichEditor.CaretPosition;
                var currentParagraph = caretPosition.Paragraph;
                
                if (currentParagraph != null)
                {
                    // 新しい段落をデフォルトスタイルにリセット
                    ResetParagraphFormatting(currentParagraph);
                    
                    // カーソル位置にプレースホルダーテキストがあれば削除
                    var textRange = new TextRange(currentParagraph.ContentStart, currentParagraph.ContentEnd);
                    var text = textRange.Text.Trim();
                    
                    // 空の段落や改行だけの段落の場合、完全にリセット
                    if (string.IsNullOrWhiteSpace(text) || text == "\r" || text == "\n")
                    {
                        currentParagraph.Inlines.Clear();
                        currentParagraph.Inlines.Add(new Run(""));
                    }
                }
            }, DispatcherPriority.Loaded);
        }

        private void FormatTimer_Tick(object? sender, EventArgs e)
        {
            _formatTimer.Stop();
            ApplyMarkdownFormatting();
        }

        private void ApplyMarkdownFormatting()
        {
            if (_isUpdating || RichEditor.Document == null) return;

            _isUpdating = true;
            try
            {
                var document = RichEditor.Document;
                
                foreach (var block in document.Blocks.OfType<Paragraph>().ToList())
                {
                    ApplyParagraphFormatting(block);
                }
            }
            finally
            {
                _isUpdating = false;
                
                // キャレット位置を復元
                if (_lastCaretPosition != null)
                {
                    try
                    {
                        // 可能な場合は同じ位置に復元
                        var currentDocument = RichEditor.Document;
                        if (_lastCaretPosition.DocumentStart == currentDocument.ContentStart.DocumentStart)
                        {
                            RichEditor.CaretPosition = _lastCaretPosition;
                        }
                        else
                        {
                            // ドキュメントが変更された場合は、適切な位置に設定
                            RichEditor.CaretPosition = RichEditor.Document.ContentEnd;
                        }
                    }
                    catch
                    {
                        // 復元に失敗した場合は末尾に設定
                        RichEditor.CaretPosition = RichEditor.Document.ContentEnd;
                    }
                }
            }
        }

        private void ApplyParagraphFormatting(Paragraph paragraph)
        {
            var text = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.TrimEnd('\r', '\n');
            
            if (string.IsNullOrWhiteSpace(text)) return;

            // 既存のフォーマットをクリア
            ResetParagraphFormatting(paragraph);

            // ヘッダーの処理
            if (text.StartsWith("# "))
            {
                ApplyHeaderFormatting(paragraph, 1, text.Substring(2));
            }
            else if (text.StartsWith("## "))
            {
                ApplyHeaderFormatting(paragraph, 2, text.Substring(3));
            }
            else if (text.StartsWith("### "))
            {
                ApplyHeaderFormatting(paragraph, 3, text.Substring(4));
            }
            else if (text.StartsWith("#### "))
            {
                ApplyHeaderFormatting(paragraph, 4, text.Substring(5));
            }
            // リストの処理
            else if (text.StartsWith("- ") || text.StartsWith("* "))
            {
                ApplyListFormatting(paragraph, text.Substring(2));
            }
            // 番号付きリストの処理
            else if (Regex.IsMatch(text, @"^\d+\. "))
            {
                ApplyNumberedListFormatting(paragraph, text);
            }
            // 引用の処理
            else if (text.StartsWith("> "))
            {
                ApplyQuoteFormatting(paragraph, text.Substring(2));
            }
            else
            {
                // インライン要素の処理
                ApplyInlineFormatting(paragraph);
            }
        }

        private void ResetParagraphFormatting(Paragraph paragraph)
        {
            paragraph.FontSize = 14;
            paragraph.FontWeight = FontWeights.Normal;
            paragraph.FontStyle = FontStyles.Normal;
            paragraph.Margin = new Thickness(0, 2, 0, 2);
            paragraph.Padding = new Thickness(0);
            paragraph.Background = null;
            paragraph.BorderBrush = null;
            paragraph.BorderThickness = new Thickness(0);
            paragraph.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x37, 0x41, 0x51));
        }

        private void ApplyHeaderFormatting(Paragraph paragraph, int level, string content)
        {
            paragraph.FontWeight = FontWeights.Bold;
            paragraph.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1F, 0x29, 0x37));
            
            switch (level)
            {
                case 1:
                    paragraph.FontSize = 24;
                    paragraph.Margin = new Thickness(0, 16, 0, 8);
                    break;
                case 2:
                    paragraph.FontSize = 20;
                    paragraph.Margin = new Thickness(0, 14, 0, 6);
                    break;
                case 3:
                    paragraph.FontSize = 16;
                    paragraph.Margin = new Thickness(0, 12, 0, 4);
                    break;
                case 4:
                    paragraph.FontSize = 14;
                    paragraph.Margin = new Thickness(0, 10, 0, 3);
                    break;
            }

            // マークダウン構文を完全に隠してコンテンツのみ表示
            paragraph.Inlines.Clear();
            
            // コンテンツ部分のみをヘッダースタイルで表示
            var contentRun = new Run(content)
            {
                FontSize = paragraph.FontSize,
                FontWeight = paragraph.FontWeight,
                Foreground = paragraph.Foreground
            };
            
            paragraph.Inlines.Add(contentRun);
        }

        private void ApplyListFormatting(Paragraph paragraph, string content)
        {
            paragraph.Margin = new Thickness(16, 2, 0, 2);
            
            // マークダウン構文を置き換えてブレット表示
            paragraph.Inlines.Clear();
            
            var bulletRun = new Run("• ")
            {
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x72, 0x80))
            };
            
            var contentRun = new Run(content);
            
            paragraph.Inlines.Add(bulletRun);
            paragraph.Inlines.Add(contentRun);
        }

        private void ApplyNumberedListFormatting(Paragraph paragraph, string content)
        {
            paragraph.Margin = new Thickness(16, 2, 0, 2);
            
            // 番号付きリストはそのまま表示
            paragraph.Inlines.Clear();
            paragraph.Inlines.Add(new Run(content));
        }

        private void ApplyQuoteFormatting(Paragraph paragraph, string content)
        {
            paragraph.Margin = new Thickness(16, 4, 0, 4);
            paragraph.Padding = new Thickness(8, 4, 8, 4);
            paragraph.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF9, 0xFA, 0xFB));
            paragraph.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x72, 0x80));
            paragraph.BorderThickness = new Thickness(2, 0, 0, 0);
            paragraph.FontStyle = FontStyles.Italic;
            
            // 引用マークを隠してコンテンツのみ表示
            paragraph.Inlines.Clear();
            
            var quoteRun = new Run(content)
            {
                FontStyle = FontStyles.Italic
            };
            
            paragraph.Inlines.Add(quoteRun);
        }

        private void ApplyInlineFormatting(Paragraph paragraph)
        {
            var textRange = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
            var text = textRange.Text;

            // インライン要素の処理（太字、斜体、コードなど）
            var runs = new List<Run>();
            var currentIndex = 0;

            // **太字** の処理
            var boldMatches = Regex.Matches(text, @"\*\*(.*?)\*\*");
            foreach (Match match in boldMatches)
            {
                if (match.Index > currentIndex)
                {
                    runs.Add(new Run(text.Substring(currentIndex, match.Index - currentIndex)));
                }
                
                var boldRun = new Run(match.Groups[1].Value)
                {
                    FontWeight = FontWeights.Bold
                };
                runs.Add(boldRun);
                currentIndex = match.Index + match.Length;
            }

            // *斜体* の処理
            var italicMatches = Regex.Matches(text, @"\*(.*?)\*");
            foreach (Match match in italicMatches)
            {
                // 太字と重複しないかチェック
                bool isPartOfBold = boldMatches.Cast<Match>().Any(bm => 
                    match.Index >= bm.Index && match.Index + match.Length <= bm.Index + bm.Length);
                
                if (!isPartOfBold && match.Index >= currentIndex)
                {
                    if (match.Index > currentIndex)
                    {
                        runs.Add(new Run(text.Substring(currentIndex, match.Index - currentIndex)));
                    }
                    
                    var italicRun = new Run(match.Groups[1].Value)
                    {
                        FontStyle = FontStyles.Italic
                    };
                    runs.Add(italicRun);
                    currentIndex = match.Index + match.Length;
                }
            }

            // `インラインコード` の処理
            var codeMatches = Regex.Matches(text, @"`(.*?)`");
            foreach (Match match in codeMatches)
            {
                if (match.Index >= currentIndex)
                {
                    if (match.Index > currentIndex)
                    {
                        runs.Add(new Run(text.Substring(currentIndex, match.Index - currentIndex)));
                    }
                    
                    var codeRun = new Run(match.Groups[1].Value)
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Cascadia Code, Consolas, Courier New"),
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF3, 0xF4, 0xF6)),
                        Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xDC, 0x26, 0x26))
                    };
                    runs.Add(codeRun);
                    currentIndex = match.Index + match.Length;
                }
            }

            // 残りのテキスト
            if (currentIndex < text.Length)
            {
                runs.Add(new Run(text.Substring(currentIndex)));
            }

            // Runが生成された場合のみ適用
            if (runs.Count > 1 || (runs.Count == 1 && runs[0].FontWeight != FontWeights.Normal))
            {
                paragraph.Inlines.Clear();
                foreach (var run in runs)
                {
                    paragraph.Inlines.Add(run);
                }
            }
        }

        // ツールバーボタンのイベントハンドラー
        private void HeaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string levelStr && int.TryParse(levelStr, out int level))
            {
                InsertMarkdownSyntax(new string('#', level) + " ", "");
            }
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMarkdownSyntax("**", "**");
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMarkdownSyntax("*", "*");
        }

        private void CodeButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMarkdownSyntax("`", "`");
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMarkdownSyntax("- ", "");
        }

        private void QuoteButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMarkdownSyntax("> ", "");
        }

        private void InsertMarkdownSyntax(string prefix, string suffix)
        {
            var selection = RichEditor.Selection;
            
            if (selection != null)
            {
                var selectedText = selection.Text;
                var newText = prefix + selectedText + suffix;
                
                // 選択されたテキストを新しいテキストで置き換え
                selection.Text = newText;
                
                // カーソル位置を調整（選択がない場合は prefix と suffix の間に配置）
                if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(suffix))
                {
                    var start = selection.Start.GetPositionAtOffset(-suffix.Length);
                    if (start != null)
                    {
                        RichEditor.CaretPosition = start;
                    }
                }
                
                // フォーマットを適用
                ApplyMarkdownFormatting();
                
                // エディターにフォーカスを戻す
                RichEditor.Focus();
            }
        }

        /// <summary>
        /// RichTextBoxの内容からマークダウンテキストを抽出する
        /// </summary>
        private string ExtractMarkdownText()
        {
            try
            {
                if (RichEditor.Document == null) return string.Empty;

                var result = new List<string>();
                
                foreach (var block in RichEditor.Document.Blocks.OfType<Paragraph>())
                {
                    var paragraphText = ExtractParagraphMarkdown(block);
                    if (!string.IsNullOrEmpty(paragraphText))
                    {
                        result.Add(paragraphText);
                    }
                }

                return string.Join("\r\n", result);
            }
            catch
            {
                // エラーが発生した場合はプレーンテキストを返す
                return new TextRange(RichEditor.Document.ContentStart, RichEditor.Document.ContentEnd).Text;
            }
        }

        /// <summary>
        /// 段落からマークダウンテキストを抽出する
        /// </summary>
        private string ExtractParagraphMarkdown(Paragraph paragraph)
        {
            try
            {
                var text = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.TrimEnd('\r', '\n');
                
                if (string.IsNullOrWhiteSpace(text)) return string.Empty;

                // フォーマットから逆算してマークダウン構文を復元
                if (paragraph.FontWeight == FontWeights.Bold && paragraph.FontSize > 14)
                {
                    // ヘッダーの場合
                    if (paragraph.FontSize >= 24) return "# " + text;
                    if (paragraph.FontSize >= 20) return "## " + text;
                    if (paragraph.FontSize >= 16) return "### " + text;
                    return "#### " + text;
                }
                else if (paragraph.Margin.Left > 10)
                {
                    // インデントがある場合（リストまたは引用）
                    if (paragraph.FontStyle == FontStyles.Italic)
                    {
                        return "> " + text; // 引用
                    }
                    else
                    {
                        // リストの場合
                        if (text.StartsWith("• "))
                        {
                            return "- " + text.Substring(2);
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\."))
                        {
                            return text; // 番号付きリストはそのまま
                        }
                        else
                        {
                            return "- " + text; // デフォルトでリスト扱い
                        }
                    }
                }
                else
                {
                    // インライン要素の処理
                    return ExtractInlineMarkdown(paragraph);
                }
            }
            catch
            {
                // エラーが発生した場合はプレーンテキストを返す
                return new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.TrimEnd('\r', '\n');
            }
        }

        /// <summary>
        /// インライン要素からマークダウンテキストを抽出する
        /// </summary>
        private string ExtractInlineMarkdown(Paragraph paragraph)
        {
            try
            {
                var result = new System.Text.StringBuilder();
                
                foreach (var inline in paragraph.Inlines)
                {
                    if (inline is Run run)
                    {
                        var text = run.Text;
                        
                        // フォーマットに基づいてマークダウン構文を追加
                        if (run.FontWeight == FontWeights.Bold)
                        {
                            text = "**" + text + "**";
                        }
                        else if (run.FontStyle == FontStyles.Italic)
                        {
                            text = "*" + text + "*";
                        }
                        else if (run.FontFamily?.Source.Contains("Consolas") == true || 
                                 run.FontFamily?.Source.Contains("Courier") == true ||
                                 run.FontFamily?.Source.Contains("Cascadia") == true)
                        {
                            text = "`" + text + "`";
                        }
                        
                        result.Append(text);
                    }
                }
                
                return result.ToString();
            }
            catch
            {
                // エラーが発生した場合はプレーンテキストを返す
                return new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.TrimEnd('\r', '\n');
            }
        }

        /// <summary>
        /// 画像ペーストを処理する
        /// </summary>
        private bool HandleImagePaste()
        {
            try
            {
                if (System.Windows.Clipboard.ContainsImage())
                {
                    var image = System.Windows.Clipboard.GetImage();
                    if (image != null)
                    {
                        // 画像ペーストイベントを発生させる
                        var args = new ImagePasteEventArgs(image);
                        ImagePasted?.Invoke(this, args);
                        
                        // イベントハンドラーが画像を処理した場合
                        if (args.IsHandled && !string.IsNullOrEmpty(args.ImagePath))
                        {
                            // マークダウン形式で画像リンクをテキストに挿入
                            var imageMarkdown = $"![画像]({args.ImagePath})";
                            InsertTextAtCaret(imageMarkdown);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"画像の貼り付けに失敗しました: {ex.Message}", "エラー", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            
            return false;
        }

        /// <summary>
        /// カーソル位置にテキストを挿入する
        /// </summary>
        private void InsertTextAtCaret(string text)
        {
            try
            {
                var caretPosition = RichEditor.CaretPosition;
                if (caretPosition != null)
                {
                    caretPosition.InsertTextInRun(text);
                    
                    // テキスト変更イベントを発生させる
                    RichEditor_TextChanged(RichEditor, new TextChangedEventArgs(System.Windows.Controls.RichTextBox.TextChangedEvent, UndoAction.None));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テキストの挿入に失敗しました: {ex.Message}", "エラー", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ShowToolbarプロパティが変更された時の処理
        /// </summary>
        private static void OnShowToolbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichMarkdownEditor editor)
            {
                editor.UpdateToolbarVisibility();
            }
        }
    }

    /// <summary>
    /// 画像ペーストイベントの引数
    /// </summary>
    public class ImagePasteEventArgs : EventArgs
    {
        public System.Windows.Media.Imaging.BitmapSource Image { get; }
        public string ImagePath { get; set; } = string.Empty;
        public bool IsHandled { get; set; } = false;

        public ImagePasteEventArgs(System.Windows.Media.Imaging.BitmapSource image)
        {
            Image = image;
        }
    }
}