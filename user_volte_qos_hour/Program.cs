using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== user_volte_qos_hour CSV 欄位格式驗證工具 ===\n");

        if (args.Length == 0)
        {
            Console.WriteLine("使用方式: user_volte_qos_hour <目錄路徑>");
            Console.WriteLine("範例: user_volte_qos_hour C:\\Data");
            return;
        }

        var targetDirectory = args[0];

        if (!Directory.Exists(targetDirectory))
        {
            Console.WriteLine($"錯誤：目錄不存在 - {targetDirectory}");
            return;
        }

        Console.WriteLine($"掃描目錄: {targetDirectory}\n");

        var csvFiles = Directory.GetFiles(targetDirectory, "user_volte_qos_hour*.csv");

        if (csvFiles.Length == 0)
        {
            Console.WriteLine("未找到符合條件的 CSV 檔案 (user_volte_qos_hour*.csv)");
            return;
        }

        Console.WriteLine($"找到 {csvFiles.Length} 個檔案\n");

        foreach (var csvFile in csvFiles)
        {
            ProcessCsvFile(csvFile);
        }

        Console.WriteLine("\n處理完成！");
    }

    private static void ProcessCsvFile(string csvFilePath)
    {
        var fileName = Path.GetFileName(csvFilePath);
        Console.WriteLine($"處理檔案: {fileName}");

        var directory = Path.GetDirectoryName(csvFilePath) ?? ".";
        var errorLogPath = Path.Combine(directory, $"{fileName}.error.log");

        int totalLines = 0;
        int validLines = 0;
        int errorLines = 0;

        try
        {
            // 讀取所有有效列到記憶體
            var validLinesList = new List<string>();
            var errorLogEntries = new List<string>();

            using (var reader = new StreamReader(csvFilePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    totalLines++;

                    if (ValidateLine(line, out var errorMessage))
                    {
                        validLinesList.Add(line);
                        validLines++;
                    }
                    else
                    {
                        // 記錄錯誤列
                        errorLogEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 檔案: {fileName}, 列: {totalLines}");
                        errorLogEntries.Add($"錯誤: {errorMessage}");
                        errorLogEntries.Add($"內容: {line}");
                        errorLogEntries.Add("");
                        errorLines++;
                    }
                }
            }

            // 寫回原檔案
            using (var writer = new StreamWriter(csvFilePath, append: false))
            {
                foreach (var line in validLinesList)
                {
                    writer.WriteLine(line);
                }
            }

            // 寫入錯誤日誌
            if (errorLines > 0)
            {
                using var errorWriter = new StreamWriter(errorLogPath, append: true);
                foreach (var entry in errorLogEntries)
                {
                    errorWriter.WriteLine(entry);
                }
            }

            Console.WriteLine($"  總列數: {totalLines}");
            Console.WriteLine($"  有效列數: {validLines}");
            Console.WriteLine($"  錯誤列數: {errorLines}");

            if (errorLines > 0)
            {
                Console.WriteLine($"  錯誤日誌: {fileName}.error.log");
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  錯誤：處理檔案時發生異常 - {ex.Message}\n");
        }
    }

    private static bool ValidateLine(string line, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(line))
        {
            errorMessage = "空白列";
            return false;
        }

        var fields = line.Split(',');

        if (fields.Length != 24)
        {
            errorMessage = $"欄位數量錯誤 (預期 24 個，實際 {fields.Length} 個)";
            return false;
        }

        // 驗證每個欄位格式
        // 欄位1: INTEGER
        if (!IsValidInteger(fields[0]))
        {
            errorMessage = $"欄位1 格式錯誤 (預期 INTEGER): {fields[0]}";
            return false;
        }

        // 欄位2-4: STRING
        if (!IsValidString(fields[1]))
        {
            errorMessage = $"欄位2 格式錯誤 (預期 STRING): {fields[1]}";
            return false;
        }
        if (!IsValidString(fields[2]))
        {
            errorMessage = $"欄位3 格式錯誤 (預期 STRING): {fields[2]}";
            return false;
        }
        if (!IsValidString(fields[3]))
        {
            errorMessage = $"欄位4 格式錯誤 (預期 STRING): {fields[3]}";
            return false;
        }

        // 欄位5-6: FLOAT
        if (!IsValidFloat(fields[4]))
        {
            errorMessage = $"欄位5 格式錯誤 (預期 FLOAT): {fields[4]}";
            return false;
        }
        if (!IsValidFloat(fields[5]))
        {
            errorMessage = $"欄位6 格式錯誤 (預期 FLOAT): {fields[5]}";
            return false;
        }

        // 欄位7: STRING
        if (!IsValidString(fields[6]))
        {
            errorMessage = $"欄位7 格式錯誤 (預期 STRING): {fields[6]}";
            return false;
        }

        // 欄位8-9: FLOAT
        if (!IsValidFloat(fields[7]))
        {
            errorMessage = $"欄位8 格式錯誤 (預期 FLOAT): {fields[7]}";
            return false;
        }
        if (!IsValidFloat(fields[8]))
        {
            errorMessage = $"欄位9 格式錯誤 (預期 FLOAT): {fields[8]}";
            return false;
        }

        // 欄位10: STRING
        if (!IsValidString(fields[9]))
        {
            errorMessage = $"欄位10 格式錯誤 (預期 STRING): {fields[9]}";
            return false;
        }

        // 欄位11-13: FLOAT
        if (!IsValidFloat(fields[10]))
        {
            errorMessage = $"欄位11 格式錯誤 (預期 FLOAT): {fields[10]}";
            return false;
        }
        if (!IsValidFloat(fields[11]))
        {
            errorMessage = $"欄位12 格式錯誤 (預期 FLOAT): {fields[11]}";
            return false;
        }
        if (!IsValidFloat(fields[12]))
        {
            errorMessage = $"欄位13 格式錯誤 (預期 FLOAT): {fields[12]}";
            return false;
        }

        // 欄位14: STRING
        if (!IsValidString(fields[13]))
        {
            errorMessage = $"欄位14 格式錯誤 (預期 STRING): {fields[13]}";
            return false;
        }

        // 欄位15-24: FLOAT
        for (int i = 14; i < 24; i++)
        {
            if (!IsValidFloat(fields[i]))
            {
                errorMessage = $"欄位{i + 1} 格式錯誤 (預期 FLOAT): {fields[i]}";
                return false;
            }
        }

        return true;
    }

    private static bool IsValidInteger(string value)
    {
        // 允許空字串（某些情況下可能需要）
        if (string.IsNullOrEmpty(value))
            return true;

        return int.TryParse(value, out _);
    }

    private static bool IsValidFloat(string value)
    {
        // 允許空字串
        if (string.IsNullOrEmpty(value))
            return true;

        // 使用不變文化特性解析 FLOAT（避免區域設定問題）
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsValidString(string value)
    {
        // STRING 類型接受任何值（包括空字串）
        return true;
    }
}
