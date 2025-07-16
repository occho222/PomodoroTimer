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


# WPF MVVM開発指針

このドキュメントは、PomodoroTimerプロジェクトにおけるWPF MVVM開発のベストプラクティスをまとめたものです。

## 1. MVVMパターンの徹底

### 基本原則
- **Model-View-ViewModel（MVVM）** によって、ビュー（XAML）とロジック（ViewModel/Model）を明確に分離する
- ViewModelにはUIに関するロジックのみを記述し、ビジネスロジックやデータ処理はModel側に分離する
- テスト容易性と開発効率の向上を重視する

### 実装ガイドライン
- `ObservableObject` を継承したViewModelクラスを作成
- CommunityToolkit.Mvvm の `[ObservableProperty]` 属性を活用
- ビジネスロジックは専用のServiceクラスやModelクラスに分離

## 2. Data Binding と INotifyPropertyChanged の活用

### バインディング戦略
- XAMLによる **宣言的UI定義（Data Binding）** を最大限活用
- ViewModel や Model に `INotifyPropertyChanged` を実装して、UI更新の自動化とリアルタイムな可視化を担保
- OneTime / OneWay / TwoWay といったバインドモードを適切に使い分け

### パフォーマンス配慮
- 過剰なバインドによるパフォーマンス低下に注意
- `UpdateSourceTrigger` を適切に設定
- 不要な双方向バインドは避ける

### 実装例
```csharp
[ObservableProperty]
private string timeRemaining = "25:00";

[ObservableProperty]
private bool isRunning = false;
```

## 3. コマンドパターンの活用

### コマンド実装方針
- UI上の操作（ボタン押下など）は `ICommand` を使ったデータバインドで実装
- コード・ビハインドを極力排除
- CommunityToolkit.Mvvm の `[RelayCommand]` 属性を活用

### 実装例
```csharp
[RelayCommand]
private void StartPause()
{
    // タイマーの開始/一時停止ロジック
}

[RelayCommand]
private void AddTask()
{
    // タスク追加ロジック
}
```

## 4. XAMLスタイルとテンプレート設計

### スタイル管理
- `App.xaml` でアプリケーション全体のスタイルを定義
- Styles / Templates / DataTemplates を駆使して、UIの再利用性やテーマ対応を強化
- カラーパレットをリソースディクショナリで一元管理

### カスタムコントロール設計
- カスタムControl作成時はテンプレート契約を極力緩やかにする
- テンプレート未定義時に例外を投げない設計にする
- 再利用可能なスタイルを作成

### 実装例
```xaml
<Style x:Key="RoundButton" TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}" 
                        CornerRadius="20">
                    <ContentPresenter HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## 5. ViewModelへのコード・ビハインド禁止の原則

### 基本方針
- UIロジックが必要な場合はできるだけXAMLとバインディング／コマンドで表現
- コード・ビハインドは最後の手段として使用
- 例外的なケース（ドラッグ&ドロップ、特殊なUI操作）のみコード・ビハインドを許可

### 許可される例外
- ホットキーの登録
- 複雑なドラッグ&ドロップ処理
- ウィンドウライフサイクル管理

## 6. DI（依存性注入）とアーキテクチャ

### DI戦略
- Microsoft.Extensions.DependencyInjection を活用（将来的に導入を検討）
- モジュール化・テスト性・疎結合設計を促進
- サービスクラスの依存性注入を実装

### アーキテクチャ構成
```
├── Models/          # データモデル
├── ViewModels/      # ビューモデル
├── Views/           # ビュー（XAML）
├── Services/        # ビジネスロジック
└── Resources/       # リソース（スタイル、翻訳等）
```

## 7. ViewModelの責務分割とSOLID原則

### 単一責任原則
- ViewModel はあくまで UI ロジックに集中
- ビジネスロジックは Model に移すことで単一責任の原則を明確化
- 各クラスは一つの責務のみを持つ

### 依存関係逆転原則
- インターフェースを通じた疎結合設計
- テスト容易性の確保

## 8. アプリ構成とプロジェクト構造

### 段階的発展
1. **初期段階**: 単一プロジェクトで開始
2. **成長段階**: Control/View/ViewModel/Model単位で分割
3. **成熟段階**: 多機能・再利用性の高い構成に進化

### フォルダ構成
- 機能別または責務別にフォルダを分割
- 名前空間とフォルダ構造を一致させる

## 9. 視覚ツリーの最適化とパフォーマンス

### パフォーマンス考慮事項
- StackPanelやGridの多重ネストを避ける
- 視覚ツリーをできるだけ浅く平坦に保つ
- 必要に応じて UIVirtualization を活用
- 非同期処理で描画負荷をコントロール

### 最適化手法
- `VirtualizingStackPanel` の使用
- `ItemsControl` での仮想化有効化
- 重い処理の非同期化

## 10. アクセシビリティ・国際化を意識

### アクセシビリティ
- WPF の UI Automation を活用
- キーボード操作への対応
- 適切なフォーカス制御の実装
- スクリーンリーダー対応

### 国際化（i18n）
- Resources や .resx を利用した多言語対応
- 文字列のハードコーディングを避ける
- カルチャーに応じた日付・数値フォーマット対応

### 実装例
```csharp
// ホットキーの登録（アクセシビリティ向上）
var startPauseCommand = new RoutedCommand();
startPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
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

## 現在のプロジェクト状況

### 適用済みベストプラクティス
? CommunityToolkit.Mvvm の活用  
? ObservableObject継承とObservableProperty属性の使用  
? RelayCommand属性によるコマンド実装  
? Data Bindingの活用  
? XAMLスタイルの定義  
? MVVMパターンの基本実装  

### 改善検討項目
?? ビジネスロジックのService層への分離  
?? DI（依存性注入）の導入  
?? 設定管理機能の実装  
?? 多言語対応の準備  
?? 単体テストの導入  

## 開発時の注意事項

1. **新機能開発時**: 必ずMVVMパターンに従う
2. **コード・ビハインド追加時**: 必要性を十分検討する
3. **パフォーマンス**: 定期的にメモリ使用量と描画パフォーマンスを確認
4. **テスト**: ViewModelの単体テストを作成
5. **ドキュメント**: 複雑なロジックは必ずコメントを記載

この指針に従うことで、保守性・テスト容易性・拡張性に優れたWPFアプリケーションを開発できます。