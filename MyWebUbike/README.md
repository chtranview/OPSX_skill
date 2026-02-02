# MyWebUbike 資料收集系統 - 使用指南

## 系統概述
自動收集台北市 YouBike 站點資料到 SQL Server 資料庫，支援定時收集、資料篩選與 CSV 匯出。

## 快速開始

### 1. 啟動應用程式
```powershell
dotnet run --project MyWebUbike
```
看到 `Now listening on: http://localhost:5206` 表示已啟動成功。

### 2. 訪問管理介面
- 瀏覽器開啟：http://localhost:5206
- 點選導覽列「資料收集」進入管理頁面

### 3. 操作流程
1. **啟動收集**：選擇行政區（可留空=全部），按「啟動收集」
2. **監控狀態**：狀態每 10 秒自動更新，顯示已收集筆數
3. **停止收集**：按「停止收集」結束本次 Session
4. **匯出資料**：選擇要匯出的行政區（可留空=全部），按「下載 CSV」

## 命令列操作（進階）

### 信任開發憑證（首次需要）
```powershell
dotnet dev-certs https --trust
```

### 啟動收集（全部行政區）
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/startJson" -UseBasicParsing
```

### 啟動收集（指定行政區）
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/startJson?areas=大安區&areas=中正區" -UseBasicParsing
```

### 查詢狀態
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/GetStatus" -UseBasicParsing | Select-Object -ExpandProperty Content
```

### 停止收集
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/stopJson" -UseBasicParsing
```

### 匯出 CSV（全部行政區）
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/ExportCsv" -OutFile ".\MyWebUbike\ubike_export.csv" -UseBasicParsing
```

### 匯出 CSV（指定行政區）
```powershell
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/ExportCsv?areas=大安區&areas=中正區" -OutFile ".\MyWebUbike\ubike_export.csv" -UseBasicParsing
```

## 完整測試流程（命令列）
```powershell
# 1. 啟動收集
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/startJson" -UseBasicParsing

# 2. 等待 65 秒讓背景服務執行一次收集
Start-Sleep -Seconds 65

# 3. 查詢狀態（recordCount 應 > 0）
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/GetStatus" -UseBasicParsing | Select-Object -ExpandProperty Content

# 4. 停止收集
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/stopJson" -UseBasicParsing

# 5. 匯出 CSV
Invoke-WebRequest -Uri "https://localhost:7224/DataCollection/ExportCsv" -OutFile ".\MyWebUbike\ubike_export.csv" -UseBasicParsing

# 6. 檢查匯出檔案
Get-Content .\MyWebUbike\ubike_export.csv -TotalCount 5
```

## 資料庫檢查（SSMS）

### 連線資訊
- Server: `.` (本機預設實例)
- Database: `TpiUbikeDB`
- 驗證: Windows 整合驗證

### 查詢範例
```sql
-- 查看最新收集的 50 筆資料
SELECT TOP 50 *
FROM dbo.TpiUbikeAreaRecords
ORDER BY CollectedTime DESC, Sarea, Sno;

-- 統計各行政區資料筆數
SELECT Sarea, COUNT(*) AS RecordCount
FROM dbo.TpiUbikeAreaRecords
GROUP BY Sarea
ORDER BY RecordCount DESC;

-- 查詢特定 Session 的資料
SELECT SessionId, MIN(CollectedTime) AS StartTime, MAX(CollectedTime) AS EndTime, COUNT(*) AS TotalRecords
FROM dbo.TpiUbikeAreaRecords
GROUP BY SessionId
ORDER BY StartTime DESC;
```

## 系統特性

### 背景服務
- **資料收集服務**：每 1 分鐘自動收集一次（可在 `DataCollectionService.cs` 調整）
- **資料清理服務**：每 1 小時清理 7 天前的舊資料（可在 `DataCleanupService.cs` 調整）

### 資料模型
- **資料表**：`TpiUbikeAreaRecords`
- **索引**：`CollectedTime`, `SessionId`, `Sarea`（優化查詢效能）
- **欄位**：站點編號、名稱、行政區、可借/可還數量、總停車格、GPS 座標等 19 個欄位

### API 端點
- `GET/POST /DataCollection/startJson` - 啟動收集（回傳 JSON）
- `GET/POST /DataCollection/stopJson` - 停止收集（回傳 JSON）
- `GET /DataCollection/GetStatus` - 查詢狀態
- `GET /DataCollection/ExportCsv?areas=xxx` - 匯出 CSV

## 設定檔案

### 連線字串 (appsettings.json)
```json
"ConnectionStrings": {
  "TpiUbikeDB": "Server=.;Database=TpiUbikeDB;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### YouBike API (appsettings.json)
```json
"tpiUbike": "https://tcgbusfs.blob.core.windows.net/dotapp/youbike/v2/youbike_immediate.json"
```

## 常見問題

### Q: 命令列出現連線錯誤
A: 確認應用程式已啟動且執行 `dotnet dev-certs https --trust` 信任開發憑證。

### Q: 收集筆數顯示 0
A: 可能外部 YouBike API 暫時無法連線，稍後再試或檢查網路連線。

### Q: 如何修改收集頻率
A: 編輯 `Services/DataCollectionService.cs`，修改 `Interval = TimeSpan.FromMinutes(1)` 為所需間隔。

### Q: 如何修改資料保留天數
A: 編輯 `Services/DataCleanupService.cs`，修改 `DateTime.UtcNow.AddDays(-7)` 為所需天數。

## 資料庫結構

### TpiUbikeDB 資料庫

#### 建立資料庫

應用程式啟動時會自動套用資料庫遷移（`Program.cs` 中的 `db.Database.Migrate()`）。如需手動建立或更新資料庫：

```powershell
# 建立/更新資料庫
dotnet ef database update --project MyWebUbike

# 查看資料庫狀態
dotnet ef migrations list --project MyWebUbike

# 產生新的遷移（修改 Model 後）
dotnet ef migrations add MigrationName --project MyWebUbike
```

#### 資料表：TpiUbikeAreaRecords

儲存 YouBike 站點的歷史收集資料。

| 欄位名稱 | 型別 | 說明 | 索引 |
|---------|------|------|------|
| `Id` | int | 主鍵（自動遞增） | PK |
| `SessionId` | uniqueidentifier | 收集 Session 識別碼 | ✓ |
| `CollectedTime` | datetime2 | 資料收集時間（UTC） | ✓ |
| `Sno` | nvarchar(32) | 站點編號 | |
| `Sna` | nvarchar(200) | 站點名稱（中文） | |
| `Snaen` | nvarchar(200) | 站點名稱（英文） | |
| `Sarea` | nvarchar(100) | 行政區（中文） | ✓ |
| `Sareaen` | nvarchar(100) | 行政區（英文） | |
| `Ar` | nvarchar(300) | 地址（中文） | |
| `Aren` | nvarchar(300) | 地址（英文） | |
| `Quantity` | int | 總停車格數量 | |
| `AvailableRentBikes` | int | 可借車輛數 | |
| `AvailableReturnBikes` | int | 可還空位數 | |
| `Act` | nvarchar(4) | 營運狀態（1=營運中） | |
| `Latitude` | float | 緯度 | |
| `Longitude` | float | 經度 | |
| `Mday` | nvarchar(50) | 資料更新日期 | |
| `SrcUpdateTime` | nvarchar(50) | 來源更新時間 | |
| `UpdateTime` | nvarchar(50) | 更新時間 | |
| `InfoTime` | nvarchar(50) | 資訊時間 | |
| `InfoDate` | nvarchar(50) | 資訊日期 | |

**索引說明**：
- `IX_TpiUbikeAreaRecords_CollectedTime`：優化時間範圍查詢
- `IX_TpiUbikeAreaRecords_SessionId`：優化 Session 篩選
- `IX_TpiUbikeAreaRecords_Sarea`：優化行政區篩選

**資料保留政策**：
- 自動清理 7 天前的資料（由 `DataCleanupService` 每小時執行）
- 可在 `appsettings.json` 的 `DataCollection:DataRetentionDays` 調整保留天數

## 檔案結構
```
MyWebUbike/
├── Controllers/
│   ├── DataCollectionController.cs  # 資料收集管理控制器
│   ├── HomeController.cs
│   └── UbikeController.cs
├── Data/
│   └── TpiUbikeDbContext.cs         # EF Core DbContext
├── DTOs/
│   └── UbikeStationDto.cs           # 資料傳輸物件
├── Models/
│   ├── TpiUbikeAreaRecord.cs        # 資料庫實體模型
│   ├── AppSettings.cs
│   └── ErrorViewModel.cs
├── Services/
│   ├── CollectionStateService.cs    # 收集狀態管理
│   ├── DataCollectionService.cs     # 背景資料收集服務
│   ├── DataCleanupService.cs        # 背景資料清理服務
│   └── UbikeService.cs              # YouBike API 服務
├── Views/
│   ├── DataCollection/
│   │   └── Index.cshtml             # 資料收集管理頁面
│   ├── Home/
│   └── Shared/
│       └── _Layout.cshtml           # 主版面配置
├── Migrations/
│   ├── 20260123085716_InitialCreate.cs  # 初始資料庫遷移
│   └── TpiUbikeDbContextModelSnapshot.cs
├── appsettings.json                 # 設定檔
└── Program.cs                        # 應用程式進入點
```

## 技術規格
- **.NET**: 8.0
- **EF Core**: 8.0.6
- **資料庫**: SQL Server 2016+
- **前端**: Bootstrap 5 + Vanilla JavaScript
- **背景服務**: IHostedService (BackgroundService)
