@echo off
setlocal

REM 処理対象のディレクトリ（このバッチファイルの場所）
set "TARGET_DIR=%~dp0"

echo UTF-8 BOM 付きで .cs と .xaml を変換中...

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Get-ChildItem -Path '%TARGET_DIR%' -Recurse -Include *.cs,*.xaml | ForEach-Object { $path = $_.FullName; $content = Get-Content -LiteralPath $path -Raw; [System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true)) }"

echo 完了しました。
pause