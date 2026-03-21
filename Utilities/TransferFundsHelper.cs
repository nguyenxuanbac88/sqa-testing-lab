using ExcelDataReader;
using OfficeOpenXml; // Thư viện EPPlus để đọc/ghi Excel
using OpenQA.Selenium;

namespace sqa_automation_testing.Utilities
{
    public class TransferFundsHelpers
    {
        public static void UpdateExcelResult(string testCaseId, string actualResult, string expectedResult, string status = "", string screenshotFileName = "")
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Tự động tìm đường dẫn thư mục gốc của dự án (Project Root)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Đi ngược lên 3 cấp từ bin/Debug/net... để về thư mục project
                DirectoryInfo projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent;
                string excelPath = Path.Combine(projectRoot.FullName, "TestData", "Testcase.xlsx");

                if (!File.Exists(excelPath))
                {
                    // Nếu không tìm thấy ở gốc, dùng lại đường dẫn base cũ
                    excelPath = Path.Combine(baseDir, "TestData", "Testcase.xlsx");
                }

                using (var package = new ExcelPackage(new FileInfo(excelPath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("TestCase"));
                    if (worksheet != null)
                    {
                        int foundRow = -1;
                        // EPPlus dùng Index 1 cho Row và Column
                        for (int row = 9; row <= worksheet.Dimension.End.Row; row++)
                        {
                            if (worksheet.Cells[row, 3].Text == testCaseId)
                            {
                                foundRow = row;
                                break;
                            }
                        }

                        if (foundRow > 0)
                        {
                            worksheet.Cells[foundRow, 10].Value = actualResult; // Cột J
                            string finalStatus = string.IsNullOrWhiteSpace(status)
                                ? (CompareExpectedAndActual(expectedResult, actualResult) ? "PASS" : "FAIL")
                                : status;
                            worksheet.Cells[foundRow, 11].Value = finalStatus; // Cột K
                            if (!string.IsNullOrEmpty(screenshotFileName))
                                worksheet.Cells[foundRow, 12].Value = screenshotFileName; // Cột L
                        }
                    }
                    package.Save(); // NHỚ ĐÓNG FILE EXCEL TRƯỚC KHI CHẠY
                }
            }
            catch (Exception ex)
            {
                // Ghi lỗi ra màn hình Output của Test để bạn biết tại sao không ghi được
                TestContext.WriteLine($"Lỗi ghi Excel: {ex.Message}");
            }
        }
        private static bool CompareExpectedAndActual(string expectedResult, string actualResult)
        {
            if (string.IsNullOrWhiteSpace(expectedResult) || string.IsNullOrWhiteSpace(actualResult))
                return false;

            // Xóa khoảng trắng thừa và chuyển về chữ thường để so sánh
            string exp = expectedResult.Trim().ToLower();
            string act = actualResult.Trim().ToLower();

            // Chỉ cần chuỗi thực tế (act) có CHỨA nội dung mong đợi (exp) là Pass
            return act.Contains(exp);
        }

        /// <summary>
        /// Chụp màn hình khi Testcase bị FAIL và lưu vào thư mục Screenshots
        /// </summary>
        public static string TakeScreenshotOnFail(IWebDriver driver, string testName)
        {
            try
            {
                string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");

                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }

                // Tên file: TestName_NamThangNgay_GioPhutGiay.png
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{testName}_{timestamp}.png";
                string fullPath = Path.Combine(screenshotFolder, fileName);

                ITakesScreenshot screenshotDriver = (ITakesScreenshot)driver;
                Screenshot screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(fullPath);

                return fileName; // Chỉ trả về tên file để ghi vào Excel cho gọn
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể chụp màn hình: {ex.Message}");
                return string.Empty;
            }
        }

        public static IEnumerable<TestCaseData> GetTransferTestData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Đi ngược lên để tìm thư mục gốc Project
            DirectoryInfo projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent;
            string path = Path.Combine(projectRoot.FullName, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet().Tables[0];
                    // Duyệt từ dòng 9 (index 8)
                    for (int i = 8; i < result.Rows.Count; i++)
                    {
                        var row = result.Rows[i];

                        // Kiểm tra điều kiện Run=YES và TestCaseID bắt đầu bằng TC_TRA
                        if (row[13]?.ToString()?.Equals("YES", StringComparison.OrdinalIgnoreCase) == true &&
                            row[2]?.ToString()?.StartsWith("TC_TRA") == true)
                        {
                            yield return new TestCaseData(
                                row[7]?.ToString() ?? "", // Amount
                                row[8]?.ToString() ?? "", // Expected Result
                                row[2]?.ToString() ?? ""  // TestCase ID
                            ).SetName(row[2]?.ToString() ?? $"Row_{i}");
                        }
                    }
                }
            }
        }
    }
}