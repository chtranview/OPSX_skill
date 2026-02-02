# user_volte_qos_hour

CSV 欄位格式驗證與清洗工具，專門處理 `user_volte_qos_hour` 開頭的 CSV 檔案。

## 功能

- 掃描指定目錄下所有 `user_volte_qos_hour*.csv` 檔案
- 逐列驗證 24 個欄位的格式
- **直接對原檔案進行清洗**，刪除格式錯誤的列
- 錯誤列記錄至 `{原檔名}.error.log`

## 欄位格式規範

| 欄位編號 | 資料型態 | 欄位編號 | 資料型態 | 欄位編號 | 資料型態 | 欄位編號 | 資料型態 |
|---------|---------|---------|---------|---------|---------|---------|---------|
| 1       | INTEGER | 7       | STRING  | 13      | FLOAT   | 19      | FLOAT   |
| 2       | STRING  | 8       | FLOAT   | 14      | STRING  | 20      | FLOAT   |
| 3       | STRING  | 9       | FLOAT   | 15      | FLOAT   | 21      | FLOAT   |
| 4       | STRING  | 10      | STRING  | 16      | FLOAT   | 22      | FLOAT   |
| 5       | FLOAT   | 11      | FLOAT   | 17      | FLOAT   | 23      | FLOAT   |
| 6       | FLOAT   | 12      | FLOAT   | 18      | FLOAT   | 24      | FLOAT   |

## 使用方式

### 編譯
```powershell
cd user_volte_qos_hour
dotnet build -c Release
```

### 執行
```powershell
# 方式一：使用 dotnet run
dotnet run --project user_volte_qos_hour <目錄路徑>

# 方式二：直接執行編譯後的程式
.\bin\Release\net8.0\user_volte_qos_hour.exe <目錄路徑>

# 範例
dotnet run --project user_volte_qos_hour C:\Data
```

### 發佈為單一可執行檔
```powershell
cd user_volte_qos_hour
dotnet publish -c Release -r win-x64 -o ./publish --self-contained false
.\publish\user_volte_qos_hour.exe C:\Data
```

## 輸出說明

### 原檔案清洗
原始 CSV 檔案會被直接清洗，只保留格式正確的列。

### 錯誤日誌
每個處理的 CSV 檔案會生成對應的錯誤日誌：
- 原始檔案：`user_volte_qos_hour_test_20260116231.csv`
- 錯誤日誌：`user_volte_qos_hour_test_20260116231.csv.error.log`

錯誤日誌記錄所有被刪除的列，包含：
- 時間戳
- 檔案名稱
- 列號
- 錯誤原因
- 原始內容

範例：
```
[2026-01-19 14:30:45] 檔案: user_volte_qos_hour_test_20260116231.csv, 列: 123
錯誤: 欄位5 格式錯誤 (預期 FLOAT): ABC
內容: 1,test,data,value,ABC,1.23,str,4.56,7.89,...
```

## 驗證規則

- **INTEGER**：整數值（允許空字串）
- **STRING**：任意字串（允許空字串）
- **FLOAT**：浮點數值，使用不變文化特性解析（允許空字串）
- **欄位數量**：必須恰好 24 個欄位（使用逗號分隔）

## 技術細節

- **.NET 版本**：8.0
- **CSV 分隔符**：逗號 (`,`)
- **編碼**：UTF-8
- **錯誤處理**：逐列驗證，單列錯誤不影響其他列處理
