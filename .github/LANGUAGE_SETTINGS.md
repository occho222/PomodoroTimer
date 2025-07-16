# 回答言語設定

このプロジェクトでは、以下の言語設定を採用します：

## 基本方針
- **全ての回答は日本語で行う**
- **コメントも日本語で記載する**
- **変数名・クラス名は英語とする**（国際的な保守性を考慮）

## 具体的なガイドライン

### コードコメント
```csharp
// ? 良い例
/// <summary>
/// ポモドーロタイマーのメインビューモデル
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // タイマーの状態管理用
    private DispatcherTimer _timer;
    
    // 現在の残り時間
    [ObservableProperty]
    private string timeRemaining = "25:00";
}

// ? 悪い例
/// <summary>
/// Main view model for pomodoro timer
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // Timer state management
    private DispatcherTimer _timer;
}
```

### XAML内の表示テキスト
```xaml
<!-- ? 良い例 -->
<TextBlock Text="今日のタスク" FontSize="20" FontWeight="Bold"/>
<Button Content="開始" Command="{Binding StartCommand}"/>

<!-- ? 悪い例 -->
<TextBlock Text="Today's Tasks" FontSize="20" FontWeight="Bold"/>
<Button Content="Start" Command="{Binding StartCommand}"/>
```

### エラーメッセージ・通知
```csharp
// ? 良い例
MessageBox.Show("タスク名を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);

// ? 悪い例
MessageBox.Show("Please enter task name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
```

### 変数名・メソッド名
```csharp
// ? 良い例（英語）
public partial class PomodoroTask : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;
    
    [ObservableProperty]
    private int estimatedPomodoros = 1;
    
    public void StartTask()
    {
        // タスクを開始する処理
    }
}

// ? 悪い例（日本語）
public partial class PomodoroTask : ObservableObject
{
    [ObservableProperty]
    private string タイトル = string.Empty;
    
    public void タスク開始()
    {
        // 処理
    }
}
```

## コミットルール

### 基本原則
- **明確性**: 変更内容が一目で分かる適切な日本語のコメントでコミットしてください
- **タイミング**: コミットは「コミットの命令」があったときのみ実行してください
- **安全性**: エラーを避けるため特殊文字の使用は控えてください

### 重要な注意事項
- **禁止文字**: コロン（`:`）、セミコロン（`;`）、括弧などはPowerShellでエラーの原因となります
- **推奨文字**: 日本語、英数字、ハイフン（`-`）、アンダースコア（`_`）を使用してください

### コミットメッセージの例

#### 良い例
```
git commit -m "タイマー機能の追加"
git commit -m "UIレイアウトの改善"
git commit -m "設定画面の実装"
git commit -m "バグ修正_タスク削除機能"
git commit -m "ドキュメント更新-開発指針"
```

#### 避けるべき例
```
git commit -m "機能追加: タイマー実装"  # コロンはエラーの原因
git commit -m "修正(緊急)"            # 括弧はエラーの原因
git commit -m "Update; fix bugs"      # セミコロンはエラーの原因
```

### コミットメッセージのガイドライン
1. **動作を表す動詞を使用**: 追加、修正、削除、更新、実装
2. **具体的な変更内容を記載**: 何を変更したかを明確に
3. **50文字以内**: 簡潔で分かりやすく
4. **特殊文字を避ける**: PowerShellエラーを防ぐ

### タイミングルール
- コミットは開発者が「コミット」や「保存」を明示的に指示したときのみ実行
- 自動的なコミットは行わない
- 変更完了後、動作確認を行ってからコミット

## AI回答時の方針

### 説明・解説
- 技術的な説明は日本語で行う
- コードの解説も日本語で記載
- ベストプラクティスの説明も日本語

### コード提案
- コメントは日本語
- 変数名・メソッド名は英語
- UI表示テキストは日本語

### エラー対応
- エラーメッセージの説明は日本語
- 解決方法の提案も日本語
- ユーザー向けメッセージは日本語

この設定により、日本語での開発・保守を行いながら、国際的な開発チームでも理解しやすいコードベースを維持します。