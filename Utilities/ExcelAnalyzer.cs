using System;
using System.Data;
using System.IO;
using ExcelDataReader;

namespace sqa_automation_testing.Utilities
{
    public class ExcelAnalyzer
    {
        public static void AnalyzeExcelStructure()
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path}");
                    return;
                }

                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                        });

                        // Tìm sheet "TestCase"
                        DataTable testCaseSheet = null;
                        foreach (DataTable dt in result.Tables)
                        {
                            if (dt.TableName.Contains("TestCase"))
                            {
                                testCaseSheet = dt;
                                break;
                            }
                        }

                        if (testCaseSheet == null)
                        {
                            Console.WriteLine("Sheet 'TestCase' not found!");
                            Console.WriteLine($"Available sheets: {string.Join(", ", result.Tables.Cast<DataTable>().Select(dt => dt.TableName))}");
                            return;
                        }

                        Console.WriteLine($"\n=== EXCEL ANALYSIS ===");
                        Console.WriteLine($"Sheet Name: {testCaseSheet.TableName}");
                        Console.WriteLine($"Total Rows: {testCaseSheet.Rows.Count}");
                        Console.WriteLine($"Total Columns: {testCaseSheet.Columns.Count}");

                        // In 30 dòng ??u ?? hi?u c?u trúc
                        Console.WriteLine($"\n=== FIRST 30 ROWS (Rows 1-30) ===\n");
                        for (int i = 0; i < Math.Min(30, testCaseSheet.Rows.Count); i++)
                        {
                            DataRow row = testCaseSheet.Rows[i];
                            Console.Write($"Row {i + 1:D3} | ");

                            // In t?ng c?t (ch? 14 c?t vì d? li?u có 14 c?t)
                            for (int c = 0; c < testCaseSheet.Columns.Count; c++)
                            {
                                var value = row[c]?.ToString() ?? "";

                                // C?t ng?n n?u quá dài
                                if (value.Length > 25)
                                    value = value.Substring(0, 22) + "...";

                                Console.Write($"[{c:D2}: {value,-25}]");
                            }
                            Console.WriteLine();
                        }

                        // Tìm t?t c? dòng có "YES" ? b?t k? c?t nào
                        Console.WriteLine($"\n=== ROWS WITH 'YES' (Run Flag) ===\n");
                        int yesCount = 0;
                        for (int i = 0; i < testCaseSheet.Rows.Count; i++)
                        {
                            DataRow row = testCaseSheet.Rows[i];
                            for (int c = 0; c < testCaseSheet.Columns.Count; c++)
                            {
                                var value = row[c]?.ToString() ?? "";
                                if (value.Equals("YES", StringComparison.OrdinalIgnoreCase))
                                {
                                    // In toàn b? row này
                                    Console.WriteLine($">>> Row {i + 1}: Found 'YES' at Column {c}");
                                    Console.Write($"    ");
                                    for (int col = 0; col < testCaseSheet.Columns.Count; col++)
                                    {
                                        var v = row[col]?.ToString() ?? "";
                                        if (v.Length > 20) v = v.Substring(0, 17) + "...";
                                        Console.Write($"Col{col}: {v,-20} | ");
                                    }
                                    Console.WriteLine();
                                    yesCount++;
                                    break; // Ch? tìm YES ? t?ng hàng m?t l?n
                                }
                            }
                            if (yesCount >= 15) break; // Ch? l?y 15 dòng ??u có YES
                        }

                        Console.WriteLine($"\nTotal rows with 'YES': {yesCount}");
                    }
                }
            }
    }
}
