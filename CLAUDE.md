# リリース手順

## 実装後

### 実装後ビルド
実装完了後は必ず `dotnet build` を実行し、コードが正常にコンパイルされることを確認する

## 自動リリースワークフロー

ユーザーがリリースを要求した場合、以下の手順を自動的に実行する:


1. **バージョン更新**(Verはデータ管理にもあります。)

2. **最終リリースビルド**: バージョン更新後に `dotnet build -c Release`

3. **リリースファイルのパッケージ化**:
   ```
   mkdir "release/PomobanXXX"  # XXX = ドットを除いたバージョン番号
   cp -r "bin/Release/net8.0-windows/"* "release/PomobanXXX/"

4.Gitにコミットプッシュ