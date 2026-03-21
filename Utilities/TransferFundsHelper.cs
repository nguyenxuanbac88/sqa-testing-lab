using ExcelDataReader;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome; // BỔ SUNG: Thư viện để cấu hình Chrome ẩn danh
using OpenQA.Selenium.Support.UI;

namespace sqa_automation_testing.Utilities
{
    public class TransferFundsHelpers
    {
        // ==================================================================
        // BỔ SUNG: HÀM KHỞI TẠO DRIVER "TÀNG HÌNH" (Không đụng DriverFactory)
        // ==================================================================
        public static IWebDriver InitDriverBypassCloudflare()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized"); // Giữ nguyên code của Khoa

            // --- MA THUẬT ẨN DANH VƯỢT CLOUDFLARE CHỈ CÓ Ở NHÁNH CỦA BẠN ---
            // 1. Tắt cờ báo hiệu Automation của trình duyệt
            options.AddArgument("--disable-blink-features=AutomationControlled");
            // 2. Ẩn dòng chữ "Chrome is being controlled..."
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10); // Giữ nguyên code của Khoa

            return driver;
        }

        public static void UpdateExcelResult(string testCaseId, string actualResult, string expectedResult, string status = "", string screenshotFileName = "")
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent;
                string excelPath = Path.Combine(projectRoot.FullName, "TestData", "Testcase.xlsx");

                if (!File.Exists(excelPath))
                {
                    excelPath = Path.Combine(baseDir, "TestData", "Testcase.xlsx");
                }

                using (var package = new ExcelPackage(new FileInfo(excelPath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("TestCase"));
                    if (worksheet != null)
                    {
                        int foundRow = -1;
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
                            worksheet.Cells[foundRow, 10].Value = actualResult;
                            string finalStatus = string.IsNullOrWhiteSpace(status)
                                ? (CompareExpectedAndActual(expectedResult, actualResult) ? "PASS" : "FAIL")
                                : status;
                            worksheet.Cells[foundRow, 11].Value = finalStatus;
                            if (!string.IsNullOrEmpty(screenshotFileName))
                                worksheet.Cells[foundRow, 12].Value = screenshotFileName;
                        }
                    }
                    package.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi Excel: {ex.Message}");
            }
        }

        private static bool CompareExpectedAndActual(string expectedResult, string actualResult)
        {
            if (string.IsNullOrWhiteSpace(expectedResult) || string.IsNullOrWhiteSpace(actualResult)) return false;
            return actualResult.Trim().ToLower().Contains(expectedResult.Trim().ToLower());
        }

        public static string TakeScreenshotOnFail(IWebDriver driver, string testName)
        {
            try
            {
                string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{testName}_{timestamp}.png";
                string fullPath = Path.Combine(screenshotFolder, fileName);

                ITakesScreenshot screenshotDriver = (ITakesScreenshot)driver;
                Screenshot screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(fullPath);

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể chụp màn hình: {ex.Message}");
                return string.Empty;
            }
        }

        public static (string Amount, string ExpectedKeyword) GetTestDataById(string testCaseId)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectRoot = Directory.GetParent(baseDir).Parent.Parent.Parent;
            string path = Path.Combine(projectRoot.FullName, "TestData", "Testcase.xlsx");
            if (!File.Exists(path)) path = Path.Combine(baseDir, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();

                    var table = result.Tables.Cast<System.Data.DataTable>().FirstOrDefault(t => t.TableName.Contains("TestCase"));

                    if (table != null)
                    {
                        for (int i = 8; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];
                            string currentId = row[2]?.ToString()?.Trim() ?? "";

                            if (currentId.Equals(testCaseId, StringComparison.OrdinalIgnoreCase))
                            {
                                string rawAmount = "";
                                string rawExpected = "";

                                for (int j = i; j < table.Rows.Count; j++)
                                {
                                    string nextId = table.Rows[j][2]?.ToString()?.Trim() ?? "";
                                    if (j > i && !string.IsNullOrEmpty(nextId)) break;

                                    string colH = table.Rows[j][7]?.ToString() ?? "";
                                    string colI = table.Rows[j][8]?.ToString() ?? "";

                                    if (!string.IsNullOrWhiteSpace(colH)) rawAmount += colH + " ";
                                    if (!string.IsNullOrWhiteSpace(colI)) rawExpected += colI + " ";
                                }

                                string finalAmount = "";
                                if (rawAmount.Contains("[Empty]", StringComparison.OrdinalIgnoreCase))
                                    finalAmount = "";
                                else if (rawAmount.Contains("Amount:", StringComparison.OrdinalIgnoreCase))
                                {
                                    int idx = rawAmount.IndexOf("Amount:", StringComparison.OrdinalIgnoreCase) + 7;
                                    finalAmount = rawAmount.Substring(idx).Trim().Split(' ')[0];
                                }
                                else if (!string.IsNullOrWhiteSpace(rawAmount))
                                    finalAmount = rawAmount.Trim();

                                string expectedKeyword = rawExpected;
                                int startIdx = rawExpected.IndexOf('"');
                                int endIdx = rawExpected.LastIndexOf('"');

                                if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                                    expectedKeyword = rawExpected.Substring(startIdx + 1, endIdx - startIdx - 1);
                                else
                                    expectedKeyword = expectedKeyword.Trim();

                                return (finalAmount, expectedKeyword);
                            }
                        }
                    }
                }
            }
            return ("", "");
        }

        // Vẫn giữ lại hàm này làm phương án dự phòng bảo vệ lớp thứ 2
        public static void WaitForCloudflare(IWebDriver driver, int timeoutInSeconds = 30)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(d => !d.Title.Contains("Just a moment", StringComparison.OrdinalIgnoreCase));
                System.Threading.Thread.Sleep(1500);
            }
            catch
            {
                Console.WriteLine("Cảnh báo: Đã đợi Cloudflare 30s nhưng chưa qua.");
            }
        }
    }
}