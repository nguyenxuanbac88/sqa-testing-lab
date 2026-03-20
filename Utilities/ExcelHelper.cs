using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ExcelDataReader;
using OfficeOpenXml;

namespace sqa_automation_testing.TestData
{
    public class ExcelHelper
    {
        // Hàm debug để xem cấu trúc Excel
        public static void DebugExcelStructure()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    Console.WriteLine($"Total sheets: {result.Tables.Count}");

                    foreach (DataTable dt in result.Tables)
                    {
                        Console.WriteLine($"\n=== Sheet: '{dt.TableName}' ===");
                        Console.WriteLine($"Rows: {dt.Rows.Count}, Columns: {dt.Columns.Count}");

                        // Print header
                        Console.Write("Header: ");
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            Console.Write($"Col{i}('{dt.Columns[i].ColumnName}') | ");
                        }
                        Console.WriteLine();

                        // Print first 10 data rows starting from row 5
                        if (dt.TableName.Contains("TestCase"))
                        {
                            Console.WriteLine("\n--- Rows from index 4 onwards (starting from row 5 in Excel) ---");
                            for (int r = 4; r < Math.Min(14, dt.Rows.Count); r++)
                            {
                                Console.Write($"Row {r+2} (Index {r}): ");
                                for (int c = 0; c < dt.Columns.Count; c++)
                                {
                                    var val = dt.Rows[r][c];
                                    Console.Write($"[{val}] | ");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
        }
        // Hàm này sẽ trả về danh sách dữ liệu để NUnit chạy (chỉ các test có Run = "YES")
        public static IEnumerable<TestCaseData> GetLoginData()
        {
            // Bắt buộc phải có dòng này khi dùng ExcelDataReader với .NET 6 hoặc .NET 8
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Tự động tìm đường dẫn tới file Testcase.xlsx trong thư mục TestData
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Đọc data thành bảng, coi dòng đầu tiên là Tiêu đề (Header)
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    // Tìm sheet "TestCase"
                    DataTable table = null;
                    foreach (DataTable dt in result.Tables)
                    {
                        if (dt.TableName.Contains("TestCase"))
                        {
                            table = dt;
                            break;
                        }
                    }

                    if (table != null && table.Rows.Count > 0)
                    {
                        // Duyệt qua từng dòng trong Excel (bắt đầu từ index 4 tức row 6 để bỏ qua header)
                        for (int i = 4; i < table.Rows.Count; i++)
                        {
                            DataRow row = table.Rows[i];

                            // Kiểm tra cột Run (Col13)
                            string runFlag = row[13]?.ToString() ?? "";

                            if (runFlag.Equals("YES", System.StringComparison.OrdinalIgnoreCase))
                            {
                                // Col2 = Test Case ID (TC_REG_001)
                                string testCaseId = row[2]?.ToString() ?? "";

                                // Bỏ qua dòng trống hoặc dòng chỉ dẫn
                                if (string.IsNullOrWhiteSpace(testCaseId))
                                {
                                    continue;
                                }

                                // Col7 = Test Data (chứa username và password)
                                // Format: "F.Name: Khoa, L.Name: Nguyen, ..., User: khoa_it_01, Pass: Khoa123!, ..."
                                string testDataStr = row[7]?.ToString() ?? "";

                                // Parse username và password từ test data
                                string username = ExtractValueFromTestData(testDataStr, "User:");
                                string password = ExtractValueFromTestData(testDataStr, "Pass:");

                                if (!string.IsNullOrWhiteSpace(username))
                                {
                                    // Đóng gói data và gửi cho NUnit
                                    yield return new TestCaseData(username, password).SetName($"Login_{testCaseId}");
                                }
                            }
                        }
                    }
                }
            }
        }

        // Helper function để lấy giá trị từ Test Data string
        private static string ExtractValueFromTestData(string testData, string key)
        {
            if (string.IsNullOrEmpty(testData) || !testData.Contains(key))
                return "";

            int startIndex = testData.IndexOf(key) + key.Length;
            int endIndex = testData.IndexOf(',', startIndex);

            if (endIndex == -1)
            {
                endIndex = testData.Length;
            }

            string value = testData.Substring(startIndex, endIndex - startIndex).Trim();
            return value;
        }

        // Hàm cập nhật kết quả test vào Excel
        public static void UpdateTestResult(string testCaseId, string status, string screenshotPath = "")
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    // Tìm worksheet "TestCase"
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("TestCase"));

                    if (worksheet != null)
                    {
                        // Tìm dòng có Test Case ID tương ứng (Col2)
                        // Bắt đầu từ dòng 7 (index 6 vì dòng 1 là index 0)
                        for (int row = 7; row <= worksheet.Dimension?.Rows; row++)
                        {
                            var cellValue = worksheet.Cells[row, 3]?.Value?.ToString(); // Col2 (Test Case ID) là cột 3
                            if (cellValue == testCaseId)
                            {
                                // Cập nhật cột Pass/Failed (Col10 - cột 11)
                                worksheet.Cells[row, 11].Value = status;

                                // Cập nhật cột Screenshot (Col11 - cột 12) nếu có lỗi
                                if (!string.IsNullOrEmpty(screenshotPath))
                                {
                                    worksheet.Cells[row, 12].Value = screenshotPath;
                                }

                                break;
                            }
                        }
                    }

                    package.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Excel: {ex.Message}");
            }
        }
    }
}

