using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ExcelDataReader;
using OfficeOpenXml;

namespace sqa_automation_testing.Utilities // Hoặc .TestData tùy bạn đang để ở đâu
{
    public class ExcelHelper
    {
        // Định nghĩa các chỉ số cột trong Excel (0-based index)
        private const int COL_NO = 0;
        private const int COL_REQUIREMENT_ID = 1;
        private const int COL_TEST_CASE_ID = 2;
        private const int COL_TEST_OBJECTIVE = 3;
        private const int COL_PRECONDITIONS = 4;
        private const int COL_TEST_STEPS = 5;
        private const int COL_STEP_ACTION = 6;
        private const int COL_DATA = 7;
        private const int COL_EXPECTED_RESULT = 8;
        private const int COL_ACTUAL_RESULT = 9;
        private const int COL_NOTES = 10;
        private const int COL_SCREENSHOT = 11;
        private const int COL_BY = 12;
        private const int COL_RUN = 13;

        private const int DATA_START_ROW = 8; // Data bắt đầu từ Row 9 (Index 8)

        // ==========================================
        // 1. HÀM LẤY DỮ LIỆU LOGIN
        // ==========================================
        public static IEnumerable<TestCaseData> GetLoginData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });

                    DataTable table = result.Tables.Cast<DataTable>().FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 30; i++) // Tăng lên 30 cho thoải mái
                        {
                            DataRow row = table.Rows[i];

                            // Bảo vệ chống văng lỗi nếu dòng Excel bị thiếu cột
                            if (row.ItemArray.Length <= COL_RUN) continue;

                            string runFlag = row[COL_RUN]?.ToString().Trim() ?? "";

                            if (runFlag.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString().Trim() ?? "";

                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId))
                                    continue;

                                if (!testCaseId.StartsWith("TC_LOG", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                processedTestCases.Add(testCaseId);

                                string testData = row[COL_DATA]?.ToString() ?? "";
                                string username = ExtractValueFromTestData(testData, "User");
                                string password = ExtractValueFromTestData(testData, "Pass");

                                // Đọc Expected Result ở dòng hiện tại
                                string expectedResult = row[COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";

                                // --- BẮT ĐẦU ĐOẠN DÒ TÌM EXPECTED RESULT XUỐNG CÁC DÒNG DƯỚI ---
                                if (string.IsNullOrWhiteSpace(expectedResult))
                                {
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        if (table.Rows[j].ItemArray.Length <= COL_TEST_CASE_ID) continue;

                                        // Nếu đụng ID của bài test khác -> Dừng lại ngay
                                        string nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString().Trim() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId)) break;

                                        // Nếu vẫn cùng 1 bài test, lôi Expected Result ra
                                        if (table.Rows[j].ItemArray.Length > COL_EXPECTED_RESULT)
                                        {
                                            string nextExpectedResult = table.Rows[j][COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";
                                            if (!string.IsNullOrWhiteSpace(nextExpectedResult))
                                            {
                                                expectedResult = nextExpectedResult; // Lụm được rồi!
                                                break; // Thoát vòng lặp
                                            }
                                        }
                                    }
                                }
                                // --- KẾT THÚC ĐOẠN DÒ TÌM ---

                                // Bắt buộc nạp test case vào hệ thống dù User/Pass có bị trống
                                var testCaseData = new TestCaseData(username, password, expectedResult, testCaseId)
                                    .SetName($"Login_{testCaseId}");

                                yield return testCaseData;
                                yesCount++;
                            }
                        }
                    }
                }
            }
        }

        // ==========================================
        // 2. HÀM LẤY DỮ LIỆU REGISTER
        // ==========================================
        public static IEnumerable<TestCaseData> GetRegisterData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });

                    DataTable table = result.Tables.Cast<DataTable>().FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 30; i++)
                        {
                            DataRow row = table.Rows[i];

                            if (row.ItemArray.Length <= COL_RUN) continue;

                            string runFlag = row[COL_RUN]?.ToString().Trim() ?? "";

                            if (runFlag.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString().Trim() ?? "";

                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId))
                                    continue;

                                if (!testCaseId.StartsWith("TC_REG", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                processedTestCases.Add(testCaseId);

                                string testData = row[COL_DATA]?.ToString() ?? "";

                                // Xử lý gộp dòng (nếu data nằm trên nhiều dòng)
                                if (string.IsNullOrWhiteSpace(testData))
                                {
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        if (table.Rows[j].ItemArray.Length <= COL_TEST_CASE_ID) continue;
                                        var nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId)) break;

                                        var dataFromNextRow = table.Rows[j][COL_DATA]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(dataFromNextRow))
                                        {
                                            if (!string.IsNullOrWhiteSpace(testData)) testData += ", ";
                                            testData += dataFromNextRow;
                                        }
                                    }
                                }

                                string firstName = ExtractValueFromTestData(testData, "F.Name");
                                string lastName = ExtractValueFromTestData(testData, "L.Name");
                                string username = ExtractValueFromTestData(testData, "User");
                                string password = ExtractValueFromTestData(testData, "Pass");
                                string address = ExtractValueFromTestData(testData, "Address");
                                string city = ExtractValueFromTestData(testData, "City");
                                string state = ExtractValueFromTestData(testData, "State");
                                string zipCode = ExtractValueFromTestData(testData, "ZipCode") ?? ExtractValueFromTestData(testData, "Zip Code");
                                string phone = ExtractValueFromTestData(testData, "Phone");
                                string ssn = ExtractValueFromTestData(testData, "SSN");

                                string expectedResult = row[COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";
                                if (string.IsNullOrWhiteSpace(expectedResult))
                                {
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        if (table.Rows[j].ItemArray.Length <= COL_TEST_CASE_ID) continue;
                                        var nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId)) break;

                                        expectedResult = table.Rows[j][COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";
                                        if (!string.IsNullOrWhiteSpace(expectedResult)) break;
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(expectedResult)) expectedResult = "Registration successful";

                                // ĐÃ XÓA LỆNH IF CHẶN USERNAME/PASSWORD RỖNG
                                var testCaseData = new TestCaseData(firstName, lastName, address, city, state,
                                                                    zipCode, phone, ssn, username, password,
                                                                    expectedResult, testCaseId)
                                    .SetName($"Register_{testCaseId}");

                                yield return testCaseData;
                                yesCount++;
                            }
                        }
                    }
                }
            }
        }

        // ==========================================
        // 3. HÀM TRÍCH XUẤT TỪ KHÓA
        // ==========================================
        private static string ExtractValueFromTestData(string testData, string key)
        {
            if (string.IsNullOrEmpty(testData) || !testData.Contains(key, StringComparison.OrdinalIgnoreCase))
                return "";

            try
            {
                int keyIndex = testData.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                int startIndex = keyIndex + key.Length;

                while (startIndex < testData.Length && (testData[startIndex] == ':' || testData[startIndex] == '=' || char.IsWhiteSpace(testData[startIndex])))
                    startIndex++;

                int endIndex = testData.IndexOf(',', startIndex);
                if (endIndex == -1) endIndex = testData.Length;

                return testData.Substring(startIndex, endIndex - startIndex).Trim();
            }
            catch
            {
                return "";
            }
        }

        // ==========================================
        // 4. HÀM CẬP NHẬT KẾT QUẢ VÀO EXCEL (ĐÃ CÓ CHỨC NĂNG XÓA ẢNH CŨ)
        // ==========================================
        public static void UpdateTestResult(string testCaseId, string actualResult, string status, string screenshotPath = "")
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string path = Path.Combine(projectDir, "TestData", "Testcase.xlsx");

                if (!File.Exists(path))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");
                }

                if (!File.Exists(path)) return;

                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("TestCase"));

                    if (worksheet != null)
                    {
                        int foundRow = -1;
                        for (int row = 9; row <= worksheet.Dimension?.Rows; row++)
                        {
                            var cellValue = worksheet.Cells[row, 3]?.Value?.ToString();
                            if (cellValue == testCaseId)
                            {
                                foundRow = row;
                                break;
                            }
                        }

                        if (foundRow > 0)
                        {
                            worksheet.Cells[foundRow, 10].Value = actualResult; // Cột Actual
                            worksheet.Cells[foundRow, 11].Value = status;       // Cột Status

                            if (!string.IsNullOrEmpty(screenshotPath))
                            {
                                worksheet.Cells[foundRow, 12].Value = screenshotPath; // Ghi tên ảnh
                            }
                            else
                            {
                                worksheet.Cells[foundRow, 12].Value = ""; // Xóa ảnh cũ nếu test PASS
                            }

                            package.Save();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi ngầm khi ghi Excel để không làm crash test
            }
        }
        // ==========================================
        // HÀM LẤY DỮ LIỆU ACCOUNT OVERVIEW (Dò Step Action)
        // ==========================================
        public static IEnumerable<TestCaseData> GetAccountOverviewData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });

                    DataTable table = result.Tables.Cast<DataTable>().FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 30; i++)
                        {
                            DataRow row = table.Rows[i];
                            if (row.ItemArray.Length <= COL_RUN) continue;

                            string runFlag = row[COL_RUN]?.ToString().Trim() ?? "";

                            if (runFlag.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString().Trim() ?? "";

                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId))
                                    continue;

                                // --- CHÚ Ý TIỀN TỐ TẠI ĐÂY ---
                                if (!testCaseId.StartsWith("TC_ACC", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                processedTestCases.Add(testCaseId);

                                // Lấy dữ liệu ở dòng hiện tại (Dòng chứa ID)
                                string expectedResult = row[COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";
                                string stepAction = row[COL_STEP_ACTION]?.ToString().Trim() ?? "";

                                // THUẬT TOÁN DÒ MÌN VÀ VÉT MÁNG (Đã fix lỗi ghi đè)
                                // Nếu 1 trong 2 ô bị trống thì mới phải đi tìm ở các dòng dưới
                                if (string.IsNullOrWhiteSpace(expectedResult) || string.IsNullOrWhiteSpace(stepAction))
                                {
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        if (table.Rows[j].ItemArray.Length <= COL_TEST_CASE_ID) continue;
                                        string nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString().Trim() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId)) break; // Sang bài test khác -> Dừng

                                        // Nếu Expected đang trống, cố gắng lượm ở dòng này
                                        if (string.IsNullOrWhiteSpace(expectedResult) && table.Rows[j].ItemArray.Length > COL_EXPECTED_RESULT)
                                        {
                                            string nextExpected = table.Rows[j][COL_EXPECTED_RESULT]?.ToString().Trim() ?? "";
                                            if (!string.IsNullOrWhiteSpace(nextExpected)) expectedResult = nextExpected;
                                        }

                                        // Nếu Action đang trống, cố gắng lượm ở dòng này
                                        if (string.IsNullOrWhiteSpace(stepAction) && table.Rows[j].ItemArray.Length > COL_STEP_ACTION)
                                        {
                                            string nextAction = table.Rows[j][COL_STEP_ACTION]?.ToString().Trim() ?? "";
                                            if (!string.IsNullOrWhiteSpace(nextAction)) stepAction = nextAction;
                                        }

                                        // Lượm đủ cả 2 món rồi thì thoát vòng lặp cho nhẹ máy
                                        if (!string.IsNullOrWhiteSpace(expectedResult) && !string.IsNullOrWhiteSpace(stepAction))
                                        {
                                            break;
                                        }
                                    }
                                }

                                var testCaseData = new TestCaseData(stepAction, expectedResult, testCaseId)
                                    .SetName($"Overview_{testCaseId}");

                                yield return testCaseData;
                                yesCount++;
                            }
                        }
                    }
                }
            }
        }
        // ==========================================
        // HÀM LẤY DỮ LIỆU OPEN NEW ACCOUNT (HỖ TRỢ MULTI-STEP CỦA KHOA)
        // ==========================================
        public static IEnumerable<TestCaseData> GetOpenNewAccountData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });

                    DataTable table = result.Tables.Cast<DataTable>().FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 30; i++)
                        {
                            DataRow row = table.Rows[i];
                            if (row.ItemArray.Length <= COL_RUN) continue;

                            string runFlag = row[COL_RUN]?.ToString().Trim() ?? "";

                            if (runFlag.Equals("YES", StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString().Trim() ?? "";

                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId)) continue;
                                if (!testCaseId.StartsWith("TC_OPN", StringComparison.OrdinalIgnoreCase)) continue;

                                processedTestCases.Add(testCaseId);

                                // --- CHIẾN THUẬT MỚI: GOM TOÀN BỘ DATA CỦA CÁC STEP ---
                                string fullStepAction = "";
                                string fullTestData = "";
                                string expectedResult = "";

                                // Quét từ dòng hiện tại trở xuống để gom dồn các step lại
                                for (int j = i; j < table.Rows.Count; j++)
                                {
                                    if (table.Rows[j].ItemArray.Length <= COL_TEST_CASE_ID) continue;

                                    string currentTcId = table.Rows[j][COL_TEST_CASE_ID]?.ToString().Trim() ?? "";

                                    // Nếu đụng phải ID của bài Test Case khác thì dừng gom
                                    if (!string.IsNullOrWhiteSpace(currentTcId) && currentTcId != testCaseId && j > i) break;

                                    string step = table.Rows[j].ItemArray.Length > COL_STEP_ACTION ? table.Rows[j][COL_STEP_ACTION]?.ToString().Trim() ?? "" : "";
                                    string data = table.Rows[j].ItemArray.Length > COL_DATA ? table.Rows[j][COL_DATA]?.ToString().Trim() ?? "" : "";
                                    string exp = table.Rows[j].ItemArray.Length > COL_EXPECTED_RESULT ? table.Rows[j][COL_EXPECTED_RESULT]?.ToString().Trim() ?? "" : "";

                                    // Nối dồn vào chuỗi tổng
                                    if (!string.IsNullOrWhiteSpace(step)) fullStepAction += step + " ";
                                    if (!string.IsNullOrWhiteSpace(data)) fullTestData += data + " ";
                                    if (!string.IsNullOrWhiteSpace(exp)) expectedResult += exp + " ";
                                }

                                // BÓC TÁCH DỮ LIỆU TỪ CHUỖI TỔNG
                                string accountType = "CHECKING"; // Mặc định

                                // Quét chữ "SAVINGS" trong cả cột Data ("Loại: SAVINGS") lẫn cột Action
                                if (fullTestData.ToUpper().Contains("SAVINGS") || fullStepAction.ToUpper().Contains("SAVINGS"))
                                {
                                    accountType = "SAVINGS";
                                }

                                // (Dự phòng cho TC_OPN_003) Tìm ID tài khoản nguồn nếu có chữ "ID: "
                                string fromAccount = "";
                                if (fullTestData.Contains("ID:"))
                                {
                                    // Cắt lấy đoạn mã ID phía sau chữ "ID:"
                                    int idIndex = fullTestData.IndexOf("ID:") + 3;
                                    fromAccount = fullTestData.Substring(idIndex).Trim().Split(' ')[0];
                                }

                                // Đẩy dữ liệu xịn sò này xuống cho hàm Test
                                var testCaseData = new TestCaseData(accountType, fromAccount, fullStepAction, expectedResult.Trim(), testCaseId)
                                    .SetName($"OpenAccount_{testCaseId}");

                                yield return testCaseData;
                                yesCount++;
                            }
                        }
                    }
                }
            }
        }

    }
}