# ungzip_and_zip

將指定目錄下所有 GZIP 檔案解壓縮並重新打包為單一 ZIP 檔案的命令列工具。

## 功能特點

- **串流處理**：直接從 GZIP 解壓並寫入 ZIP 條目，不將檔案寫入磁碟，避免大檔案造成的記憶體和磁碟空間問題
- **自動命名**：輸出 ZIP 檔案名稱包含時間戳記（`ungzipped_20260116_143025.zip`）
- **獨立處理**：每個 .gz 檔案解壓後生成對應的獨立 ZIP 檔案
- **壓縮統計**：顯示每個檔案的 GZIP 大小、解壓後大小和 ZIP 壓縮後大小
- **錯誤處理**：個別檔案失敗不影響其他檔案的處理

## 使用方法

### 建置專案

```powershell
dotnet build -c Release
```

### 發佈為單一可執行檔

```powershell
dotnet publish -c Release -r win-x64 -o ./publish --self-contained false
```

### 執行

```powershell
# 處理目前目錄下的所有 .gz 檔案
.\publish\ungzip_and_zip.exe

# 處理指定目錄下的所有 .gz 檔案
.\publish\ungzip_and_zip.exe "C:\data\compressed"
```

## 輸出格式

每個 .gz 檔案會生成對應的獨立 ZIP 檔案：
- `data.csv.gz` → `data.csv.zip` (包含 `data.csv`)
- `report.txt.gz` → `report.txt.zip` (包含 `report.txt`)
- `logs.json.gz` → `logs.json.zip` (包含 `logs.json`)

## 技術細節

- **.NET 8.0** 控制台應用程式
- 使用 `GZipStream` 進行解壓縮（CompressionMode.Decompress）
- 使用 `ZipArchive` 進行 ZIP 打包（CompressionLevel.Optimal）
- 80KB 緩衝區大小以平衡效能和記憶體使用
- 不修改原始 .gz 檔案

## 處理流程

1. 對每個 .gz 檔案：
   - 建立對應的 ZIP 檔案（移除 .gz 副檔名後加上 .zip）
   - 串流解壓縮（GZipStream）
   - 直接寫入 ZIP 條目（ZipArchive）
   - 顯示大小統計
3. 完成後顯示成功/失敗統計
4. 完成後顯示最終 ZIP 檔案大小

## 適用場景

- 將 GZIP 壓縮檔案轉換為 ZIP 格式以便在 Windows 環境下直接使用
- 處理大型 GZIP 檔案但磁碟空間有限（不需要完整解壓到磁碟）
- 批次轉換壓縮格式（每個 .gz 對應一個 .zip）
- 保持檔案獨立性的格式轉換需求
