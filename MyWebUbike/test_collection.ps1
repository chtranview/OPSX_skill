# MyWebUbike Test Script

Write-Host "==================== MyWebUbike Data Collection Test ====================" -ForegroundColor Cyan
Write-Host ""

# Check if application is running
Write-Host "[1/6] Checking application status..." -ForegroundColor Yellow
$portTest = Test-NetConnection -ComputerName localhost -Port 5206 -WarningAction SilentlyContinue
if (-not $portTest.TcpTestSucceeded) {
    Write-Host "X Application not running on port 5206. Please run: dotnet run --project MyWebUbike" -ForegroundColor Red
    exit 1
}
Write-Host "OK Application is running" -ForegroundColor Green
Write-Host ""

# Test API connection
Write-Host "[2/6] Testing YouBike API..." -ForegroundColor Yellow
try {
    $apiTest = Invoke-WebRequest -Uri "http://localhost:5206/api/Ubike/areaQry?area=大安區" -UseBasicParsing -TimeoutSec 10
    $apiData = $apiTest.Content | ConvertFrom-Json
    Write-Host "OK API working, station count: $($apiData.count)" -ForegroundColor Green
} catch {
    Write-Host "X API test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   External YouBike API may be unavailable" -ForegroundColor Yellow
}
Write-Host ""

# Start collection
Write-Host "[3/6] Starting data collection..." -ForegroundColor Yellow
try {
    $startResp = Invoke-WebRequest -Uri "http://localhost:5206/DataCollection/Start" -Method POST -UseBasicParsing
    Write-Host "OK Collection started" -ForegroundColor Green
    Write-Host "   Response: $($startResp.Content)" -ForegroundColor Gray
} catch {
    Write-Host "X Start failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Wait for first collection
Write-Host "[4/6] Waiting 70 seconds for background service..." -ForegroundColor Yellow
Write-Host "   (Background service runs every 60 seconds)" -ForegroundColor Gray
for ($i = 70; $i -gt 0; $i--) {
    Write-Host -NoNewline "`r   Remaining: $i seconds   "
    Start-Sleep -Seconds 1
}
Write-Host ""
Write-Host ""

# Query status
Write-Host "[5/6] Checking collection status..." -ForegroundColor Yellow
try {
    $statusResp = Invoke-WebRequest -Uri "http://localhost:5206/DataCollection/GetStatus" -UseBasicParsing
    $status = $statusResp.Content | ConvertFrom-Json
    
    Write-Host "   Running: $($status.isRunning)" -ForegroundColor Cyan
    Write-Host "   Session ID: $($status.sessionId)" -ForegroundColor Cyan
    Write-Host "   Start Time: $($status.sessionStartTimeUtc)" -ForegroundColor Cyan
    Write-Host "   Record Count: $($status.recordCount)" -ForegroundColor Cyan
    
    if ($status.recordCount -eq 0) {
        Write-Host ""
        Write-Host "X WARNING: Record count is 0" -ForegroundColor Red
        Write-Host "   Possible causes:" -ForegroundColor Yellow
        Write-Host "   1. External YouBike API is unavailable" -ForegroundColor Yellow
        Write-Host "   2. Background service exception (check dotnet run terminal)" -ForegroundColor Yellow
        Write-Host "   3. Database write failed" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   Suggested actions:" -ForegroundColor Yellow
        Write-Host "   1. Check dotnet run terminal for error messages" -ForegroundColor Yellow
        Write-Host "   2. Verify network access to: https://tcgbusfs.blob.core.windows.net/dotapp/youbike/v2/youbike_immediate.json" -ForegroundColor Yellow
        Write-Host "   3. Use SSMS to connect Server=. and query TpiUbikeDB.dbo.TpiUbikeAreaRecords" -ForegroundColor Yellow
    } else {
        Write-Host "OK Successfully collected $($status.recordCount) records" -ForegroundColor Green
    }
} catch {
    Write-Host "X Status query failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Stop collection
Write-Host "[6/6] Stopping collection..." -ForegroundColor Yellow
try {
    $stopResp = Invoke-WebRequest -Uri "http://localhost:5206/DataCollection/Stop" -Method POST -UseBasicParsing
    Write-Host "OK Collection stopped" -ForegroundColor Green
} catch {
    Write-Host "X Stop failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Export CSV if data exists
if ($status.recordCount -gt 0) {
    Write-Host "[Extra] Exporting CSV..." -ForegroundColor Yellow
    try {
        $csvPath = ".\ubike_export_$(Get-Date -Format 'yyyyMMdd_HHmmss').csv"
        Invoke-WebRequest -Uri "http://localhost:5206/DataCollection/ExportCsv" -OutFile $csvPath -UseBasicParsing
        $csvSize = (Get-Item $csvPath).Length
        $csvSizeFormatted = "{0:N0}" -f $csvSize
        Write-Host "OK CSV exported: $csvPath ($csvSizeFormatted bytes)" -ForegroundColor Green
        Write-Host ""
        Write-Host "First 5 lines:" -ForegroundColor Gray
        Get-Content $csvPath -TotalCount 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    } catch {
        Write-Host "X Export failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "==================== Test Complete ====================" -ForegroundColor Cyan
