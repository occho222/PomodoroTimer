# リリース手順

## 実装後

### 実装後ビルド
実装完了後は必ず `dotnet build` を実行し、コードが正常にコンパイルされることを確認する

## 自動リリースワークフロー

ユーザーがリリースを要求した場合、以下の手順を自動的に実行する:

1. **バージョン更新** (Verはデータ管理にもあります。)
   - `PomodoroTimer.csproj` の Version, AssemblyVersion, FileVersion を更新
   - `Models/AppSettings.cs` の DataVersion を更新

2. **最終リリースビルド**: バージョン更新後に `dotnet build -c Release`

3. **リリースファイルのパッケージ化**:
   ```bash
   mkdir -p "release/PomobanXXX"  # XXX = ドットを除いたバージョン番号
   cp -r "bin/Release/net6.0-windows/"* "release/PomobanXXX/"
   ```
   - パッケージ化したファイルサイズを確認する
   - `du -sh "release/PomobanXXX/"` でサイズ確認

4. **Gitにコミット・プッシュ**
   - 全ての変更をステージング: `git add .`
   - バージョン情報を含むコミット
   - リモートリポジトリにプッシュ: `git push origin main`