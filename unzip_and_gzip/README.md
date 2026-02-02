# unzip_and_gzip

簡介：此為一個簡單的 .NET 主控台程式，會在目前目錄搜尋 `*.zip`，對每個 ZIP 檔解壓，並對每個被解出的檔案產生一個對應的 `.gz` 檔案（保留原始檔案與結構）。

建置與產生可執行檔（Windows x64 範例）：

```powershell
cd .\unzip_and_gzip
dotnet publish -c Release -r win-x64 -o ./publish --self-contained false /p:PublishSingleFile=false
```

執行：

1. 將欲處理的 `.zip` 檔放在同一個目錄（例如 `C:\work\zips`）。
2. 將 `unzip_and_gzip.exe` 或使用 `dotnet run` 放到該目錄，或從 publish 目錄執行。

範例（在含 .zip 的目錄）：

```powershell
# 如果已 publish 並有 exe
.\unzip_and_gzip.exe

# 或直接使用 dotnet run
dotnet run --project ..\unzip_and_gzip\unzip_and_gzip.csproj
```

輸出：
- 每個 ZIP 檔會被解壓到與 ZIP 同名的資料夾中。
- 每個解出的檔案旁會產生一個同名 `.gz` 檔案。
