using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

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

            var gzipFiles = Directory.GetFiles(dir, "*.gz");

            if (gzipFiles.Length == 0)
            {
                Console.WriteLine("未找到任何 .gz 檔案。");
                return 0;
            }

            Console.WriteLine($"找到 {gzipFiles.Length} 個 GZIP 檔案。\n");

            int successCount = 0;
            int failCount = 0;

            foreach (var gzPath in gzipFiles)
            {
                var gzFileName = Path.GetFileName(gzPath);
                Console.WriteLine($"處理: {gzFileName}");

                try
                {
                    ProcessSingleGzipToZip(gzPath, dir);
                    successCount++;
                    Console.WriteLine($"完成: {gzFileName}\n");
                }
                catch (Exception ex)
                {
                    failCount++;
                    Console.Error.WriteLine($"處理 {gzFileName} 時發生錯誤: {ex.Message}\n");
                }
            }

            Console.WriteLine($"處理完成！成功: {successCount}, 失敗: {failCount}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"發生錯誤: {ex.Message}");
            Console.Error.WriteLine("用法: ungzip_and_zip.exe [目錄路徑]");
            Console.Error.WriteLine("  若不指定目錄，則使用目前目錄。");
            return 2;
        }
    }

    private static void ProcessSingleGzipToZip(string gzPath, string outputDir)
    {
        var gzFileName = Path.GetFileName(gzPath);
        
        // 將 .gz 副檔名改為 .zip，保留原始檔名
        var zipFileName = gzFileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? gzFileName.Substring(0, gzFileName.Length - 3) + ".zip"
            : gzFileName + ".zip";

        var zipPath = Path.Combine(outputDir, zipFileName);

        // 如果 ZIP 檔案已存在，刪除它
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        long originalGzSize = new FileInfo(gzPath).Length;
        long decompressedSize = 0;
        long processedSize = 0;
        long nullCount = 0;

        // 建立 ZIP 檔案
        using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            // 移除 .gz 副檔名作為 ZIP 內的檔案名稱
            var entryName = gzFileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
                ? gzFileName.Substring(0, gzFileName.Length - 3)
                : gzFileName;

            var entry = zipArchive.CreateEntry(entryName, CompressionLevel.Optimal);

            // 使用串流方式邊解壓邊處理邊寫入 ZIP
            using (var gzFileStream = File.OpenRead(gzPath))
            using (var gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gzipStream, System.Text.Encoding.UTF8, true, 8192))
            using (var entryStream = entry.Open())
            using (var streamWriter = new StreamWriter(entryStream, System.Text.Encoding.UTF8, 8192, leaveOpen: true))
            {
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    decompressedSize += System.Text.Encoding.UTF8.GetByteCount(line + "\n");
                    
                    // 清理單行 NULL 值
                    var cleanedLine = CleanLineNullValues(line);
                    nullCount += CountNullValues(line, cleanedLine);
                    
                    // 轉換科學記號
                    cleanedLine = ConvertScientificNotation(cleanedLine);
                    
                    streamWriter.WriteLine(cleanedLine);
                    processedSize += System.Text.Encoding.UTF8.GetByteCount(cleanedLine + "\n");
                }
            }
        }

        Console.WriteLine($"  ✓ {gzFileName} → {zipFileName}");
        Console.WriteLine($"    GZIP: {FormatFileSize(originalGzSize)}, 解壓後: {FormatFileSize(decompressedSize)}, 清理後: {FormatFileSize(processedSize)}");
        if (nullCount > 0)
        {
            Console.WriteLine($"    → 清理了 {nullCount} 個 NULL 值");
        }
    }

    private static string CleanLineNullValues(string line)
    {
        // 將單行中的 NULL（不區分大小寫）替換為空字符串
        var cleanedLine = Regex.Replace(
            line,
            @"\b[Nn][Uu][Ll][Ll]\b",
            ""
        );

        // 處理引號包圍的 NULL
        cleanedLine = Regex.Replace(
            cleanedLine,
            @"""(\s*[Nn][Uu][Ll][Ll]\s*)""",
            "\"\""
        );

        return cleanedLine;
    }

    private static long CountNullValues(string originalLine, string cleanedLine)
    {
        // 計算被移除的 NULL 值個數
        return Regex.Matches(originalLine, @"\b[Nn][Uu][Ll][Ll]\b").Count +
               Regex.Matches(originalLine, @"""(\s*[Nn][Uu][Ll][Ll]\s*)""").Count;
    }

    private static string ConvertScientificNotation(string line)
    {
        // 將科學記號的數值轉換為真實數值
        return Regex.Replace(
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
    }


    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
