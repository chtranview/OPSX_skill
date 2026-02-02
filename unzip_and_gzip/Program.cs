using System;
using System.IO;
using System.IO.Compression;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            // 處理引數：若提供目錄路徑，則使用該目錄；否則使用目前目錄
            var dir = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                dir = args[0];
                if (!Directory.Exists(dir))
                {
                    Console.Error.WriteLine($"錯誤：指定的目錄不存在: {dir}");
                    return 1;
                }
                Console.WriteLine($"目錄已變更為: {dir}");
            }

            var zipFiles = Directory.GetFiles(dir, "*.zip");

            if (zipFiles.Length == 0)
            {
                Console.WriteLine("未找到任何 .zip 檔案。");
            }
            else
            {
                foreach (var zipPath in zipFiles)
                {
                    var zipFileName = Path.GetFileName(zipPath);
                    Console.WriteLine($"處理: {zipFileName}");
                    
                    // 檢查 zip 檔名是否以 imsi_imei_test 開頭
                    var isImsiImeiTestZip = zipFileName.StartsWith("imsi_imei_test", StringComparison.OrdinalIgnoreCase);
                    
                    // 不再為每個 ZIP 建立獨立資料夾，直接在目前目錄（或 ZIP 內的相對路徑）解出
                    var extractDir = dir;

                    using (var archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                // 目錄條目，確保資料夾存在
                                var folderPath = Path.Combine(extractDir, entry.FullName);
                                Directory.CreateDirectory(folderPath);
                                continue;
                            }

                            var destPath = Path.Combine(extractDir, entry.FullName);
                            var destDir = Path.GetDirectoryName(destPath);
                            if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

                            entry.ExtractToFile(destPath, overwrite: true);

                            var ext = Path.GetExtension(destPath);
                            var isCsv = ext != null && ext.Equals(".csv", StringComparison.OrdinalIgnoreCase);

                            // 如果是 imsi_imei_test zip，保留 csv 檔案
                            if (isImsiImeiTestZip && isCsv)
                            {
                                Console.WriteLine($"  保留（imsi_imei_test）: {Path.GetRelativePath(dir, destPath)}");
                            }
                            else
                            {
                                // 其他 zip 或非 csv 檔案，建立 .gz
                                // 如果是 csv 檔案，先清理 NULL 值
                                if (isCsv)
                                {
                                    try
                                    {
                                        CleanCsvNullValues(destPath);
                                        Console.WriteLine($"  已清理 csv NULL 值: {Path.GetRelativePath(dir, destPath)}");
                                    }
                                    catch (Exception cleanEx)
                                    {
                                        Console.Error.WriteLine($"  清理 csv 時出錯: {cleanEx.Message}");
                                    }
                                }

                                var gzPath = destPath + ".gz";
                                try
                                {
                                    CompressWithSplitting(destPath, gzPath, 4L * 1024 * 1024 * 1024); // 4GB limit
                                    Console.WriteLine($"  已建立 gzip 檔案: {Path.GetRelativePath(dir, gzPath)}");
                                }
                                catch (Exception gzEx)
                                {
                                    Console.Error.WriteLine($"  壓縮時出錯: {gzEx.Message}");
                                    continue;
                                }

                                // 如果是 csv 檔案，刪除原始檔案（保留 .gz）
                                if (isCsv)
                                {
                                    try
                                    {
                                        File.Delete(destPath);
                                        Console.WriteLine($"  已刪除原始 csv: {Path.GetRelativePath(dir, destPath)}");
                                    }
                                    catch (Exception delEx)
                                    {
                                        Console.Error.WriteLine($"  無法刪除 {destPath}: {delEx.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // 處理完該 zip 的所有檔案後，刪除原始 zip 檔案
                    try
                    {
                        File.Delete(zipPath);
                        Console.WriteLine($"已刪除原始 zip: {Path.GetFileName(zipPath)}");
                    }
                    catch (Exception delEx)
                    {
                        Console.Error.WriteLine($"無法刪除 {zipPath}: {delEx.Message}");
                    }
                }

                Console.WriteLine("完成。所有 zip 的內容皆已解壓，.csv 已刪除（保留 .gz），zip 檔案已移除。");
            }
            
            // 處理指定目錄下現有的 csv 檔案
            Console.WriteLine("\n開始處理目錄中的 csv 檔案...");
            ProcessCsvFilesInDirectory(dir);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"發生錯誤: {ex.Message}");
            Console.Error.WriteLine("用法: unzip_and_gzip.exe [目錄路徑]");
            Console.Error.WriteLine("  若不指定目錄，則使用目前目錄。");
            return 2;
        }
    }

    private static void ProcessCsvFilesInDirectory(string dir)
    {
        try
        {
            var csvFiles = Directory.GetFiles(dir, "*.csv", SearchOption.TopDirectoryOnly);
            
            if (csvFiles.Length == 0)
            {
                Console.WriteLine("未找到任何 csv 檔案。");
                return;
            }

            foreach (var csvPath in csvFiles)
            {
                var fileName = Path.GetFileName(csvPath);
                
                // 檢查檔名是否以 imsi_imei_test 開頭，若是則跳過
                if (fileName.StartsWith("imsi_imei_test", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"跳過（保留）: {fileName}");
                    continue;
                }

                // 建立 .gz 檔案
                var gzPath = csvPath + ".gz";
                try
                {
                    // 先清理 NULL 值
                    CleanCsvNullValues(csvPath);
                    Console.WriteLine($"  已清理 csv NULL 值: {fileName}");

                    CompressWithSplitting(csvPath, gzPath, 4L * 1024 * 1024 * 1024); // 4GB limit
                    Console.WriteLine($"  已建立 gzip 檔案: {fileName}");

                    // 建立 .gz 後，刪除原始 csv
                    File.Delete(csvPath);
                    Console.WriteLine($"  已刪除: {fileName}");
                }
                catch (Exception gzEx)
                {
                    Console.Error.WriteLine($"  處理 {fileName} 時出錯: {gzEx.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"處理 csv 檔案時出錯: {ex.Message}");
        }
    }

    private static void CleanCsvNullValues(string csvPath)
    {
        // 大檔案處理：逐行讀取和處理，避免 OutOfMemoryException
        var tempPath = csvPath + ".tmp";
        long nullCount = 0;

        try
        {
            using (var reader = new System.IO.StreamReader(csvPath, System.Text.Encoding.UTF8))
            using (var writer = new System.IO.StreamWriter(tempPath, false, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // 1. 將 NULL（不區分大小寫）替換為空字符串
                    var originalLine = line;
                    
                    // 處理多種 CSV 格式中的 NULL：
                    // - 逗號之間的 NULL：,NULL, -> ,,
                    // - 引號包圍的 NULL：,"NULL", -> ,"",
                    // - 引號包圍且有空格：," NULL ", -> ,"",
                    line = System.Text.RegularExpressions.Regex.Replace(
                        line,
                        @"(?<=^|,)\s*""?\s*NULL\s*""?\s*(?=,|$)",
                        "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    
                    // 額外處理：替換任何獨立的 NULL 值（前後是分隔符或空白）
                    line = System.Text.RegularExpressions.Regex.Replace(
                        line,
                        @"\bNULL\b",
                        "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    
                    nullCount += (originalLine.Length - line.Length);

                    // 2. 將科學記號的數值轉換為真實數值
                    // 例如：1.17566256E8 -> 117566256，-2.5e-3 -> -0.0025
                    line = System.Text.RegularExpressions.Regex.Replace(
                        line,
                        @"(?<![0-9a-zA-Z])([-+]?\d+\.?\d*[eE][-+]?\d+)(?![0-9a-zA-Z])",
                        match =>
                        {
                            try
                            {
                                var scientificNotation = match.Groups[1].Value;
                                if (double.TryParse(scientificNotation, System.Globalization.NumberStyles.Float, 
                                    System.Globalization.CultureInfo.InvariantCulture, out var number))
                                {
                                    // 如果是整數，輸出為整數格式；否則保留小數點
                                    if (number == Math.Floor(number) && !double.IsInfinity(number))
                                    {
                                        return ((long)number).ToString();
                                    }
                                    else
                                    {
                                        return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                                return match.Value; // 如果轉換失敗，保持原值
                            }
                            catch
                            {
                                return match.Value; // 如果出錯，保持原值
                            }
                        }
                    );

                    writer.WriteLine(line);
                }
            }

            // 替換原始檔案
            File.Delete(csvPath);
            File.Move(tempPath, csvPath);

            if (nullCount > 0)
            {
                Console.WriteLine($"    → 替換了 {nullCount} 個字元的 NULL 值");
            }
        }
        catch (Exception ex)
        {
            // 清理臨時檔案
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); }
                catch { }
            }
            throw new Exception($"清理 CSV 檔案失敗: {ex.Message}", ex);
        }
    }

    private static void CompressWithSplitting(string sourcePath, string basePath, long maxSizePerFile)
    {
        // basePath 例如: "file.csv.gz"，分割後會變成 "file1.csv.gz", "file2.csv.gz" 等
        const int bufferSize = 65536; // 64KB buffer
        int partNumber = 1;
        long currentSize = 0;
        
        var dir = Path.GetDirectoryName(basePath) ?? ".";
        var fileName = Path.GetFileName(basePath); // "file.csv.gz"
        var baseName = fileName.Substring(0, fileName.Length - 7); // 移除 ".csv.gz"
        
        using (var inStream = File.OpenRead(sourcePath))
        {
            string currentGzPath = Path.Combine(dir, $"{baseName}{partNumber}.csv.gz");
            FileStream currentOutStream = File.Create(currentGzPath);
            GZipStream currentGz = new GZipStream(currentOutStream, CompressionLevel.Optimal);
            
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            try
            {
                while ((bytesRead = inStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    currentGz.Write(buffer, 0, bytesRead);
                    currentSize += bytesRead;

                    // 檢查是否超過 4GB，如果是則開啟新檔案
                    if (currentSize >= maxSizePerFile)
                    {
                        currentGz.Dispose();
                        currentOutStream.Dispose();
                        Console.WriteLine($"  已建立分割檔: {Path.GetFileName(currentGzPath)}");

                        partNumber++;
                        currentGzPath = Path.Combine(dir, $"{baseName}{partNumber}.csv.gz");
                        currentOutStream = File.Create(currentGzPath);
                        currentGz = new GZipStream(currentOutStream, CompressionLevel.Optimal);
                        currentSize = 0;
                    }
                }

                currentGz.Dispose();
                currentOutStream.Dispose();
                Console.WriteLine($"  已建立分割檔: {Path.GetFileName(currentGzPath)}");
            }
            catch
            {
                currentGz?.Dispose();
                currentOutStream?.Dispose();
                throw;
            }
        }
    }
}
