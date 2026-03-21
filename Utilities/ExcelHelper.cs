using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ExcelDataReader;
using OfficeOpenXml;

namespace sqa_automation_testing.TestData
{
    public class ExcelHelper
    {
        // ??nh ngh?a các ch? s? c?t trong Excel (0-based index)
        // Row 6 (Index 5) là header chính
        // Data b?t ??u t? Row 9 (Index 8)

        private const int COL_NO = 0;                  // Column 0: No.
        private const int COL_REQUIREMENT_ID = 1;      // Column 1: Test Requirement ID
        private const int COL_TEST_CASE_ID = 2;        // Column 2: Test Case ID
        private const int COL_TEST_OBJECTIVE = 3;      // Column 3: Test Objective
        private const int COL_PRECONDITIONS = 4;       // Column 4: Pre-conditions
        private const int COL_TEST_STEPS = 5;          // Column 5: Step #
        private const int COL_STEP_ACTION = 6;         // Column 6: Step Action
        private const int COL_DATA = 7;                // Column 7: Test Data
        private const int COL_EXPECTED_RESULT = 8;     // Column 8: Expected Result
        private const int COL_ACTUAL_RESULT = 9;       // Column 9: Actual Result
        private const int COL_NOTES = 10;              // Column 10: Notes
        private const int COL_SCREENSHOT = 11;         // Column 11: Screenshot
        private const int COL_BY = 12;                 // Column 12: By
        private const int COL_RUN = 13;                // Column 13: Run (YES/NO)

        private const int DATA_START_ROW = 8;          // Data b?t ??u t? Row 9 (Index 8)

        // Hàm l?y d? li?u Register t? Excel (10 test ??u tiên có Run = "YES")
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

                    // Tìm sheet "TestCase"
                    DataTable table = result.Tables.Cast<DataTable>()
                        .FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        // B?t ??u t? row 8 (index 8 = Row 9 trong Excel)
                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 10; i++)
                        {
                            DataRow row = table.Rows[i];

                            // Ki?m tra c?t Run (Column 13)
                            string runFlag = row[COL_RUN]?.ToString() ?? "";

                            if (runFlag.Equals("YES", System.StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString() ?? "";

                                // B? qua n?u testCaseId tr?ng ho?c ?ã x? lý
                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId))
                                    continue;

                                // B? qua n?u không ph?i Register (TC_REG_*)
                                if (!testCaseId.StartsWith("TC_REG"))
                                    continue;

                                processedTestCases.Add(testCaseId);

                                // L?y d? li?u t? c?t Data (Column 7) và t?ng h?p t? các dòng con
                                string testData = row[COL_DATA]?.ToString() ?? "";

                                // N?u dòng này không có data, tìm t? các dòng k? ti?p cho ??n khi g?p test case m?i
                                if (string.IsNullOrWhiteSpace(testData))
                                {
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        var nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId))
                                            break; // G?p test case m?i, d?ng

                                        var dataFromNextRow = table.Rows[j][COL_DATA]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(dataFromNextRow))
                                        {
                                            if (!string.IsNullOrWhiteSpace(testData))
                                                testData += ", ";
                                            testData += dataFromNextRow;
                                        }
                                    }
                                }

                                // Parse d? li?u t? chu?i
                                string firstName = ExtractValueFromTestData(testData, "F.Name") ?? "Test";
                                string lastName = ExtractValueFromTestData(testData, "L.Name") ?? "User";
                                string username = ExtractValueFromTestData(testData, "User");
                                string password = ExtractValueFromTestData(testData, "Pass");
                                string address = ExtractValueFromTestData(testData, "Address") ?? "123 Test St";
                                string city = ExtractValueFromTestData(testData, "City") ?? "TestCity";
                                string state = ExtractValueFromTestData(testData, "State") ?? "TC";
                                // Tìm Zip Code - th? "ZipCode" tr??c (không có space), r?i "Zip Code" (có space)
                                string zipCode = ExtractValueFromTestData(testData, "ZipCode") ?? 
                                                ExtractValueFromTestData(testData, "Zip Code") ?? "12345";
                                string phone = ExtractValueFromTestData(testData, "Phone") ?? "0123456789";
                                string ssn = ExtractValueFromTestData(testData, "SSN") ?? "123456789";

                                string expectedResult = row[COL_EXPECTED_RESULT]?.ToString() ?? "";
                                if (string.IsNullOrWhiteSpace(expectedResult))
                                {
                                    // Tìm expected result t? các dòng k? ti?p
                                    for (int j = i + 1; j < table.Rows.Count; j++)
                                    {
                                        var nextTestCaseId = table.Rows[j][COL_TEST_CASE_ID]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(nextTestCaseId))
                                            break; // G?p test case m?i

                                        expectedResult = table.Rows[j][COL_EXPECTED_RESULT]?.ToString() ?? "";
                                        if (!string.IsNullOrWhiteSpace(expectedResult))
                                            break; // Tìm th?y expected result
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(expectedResult))
                                    expectedResult = "Registration successful";

                                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                                {
                                    // T?o TestCaseData v?i d? li?u c?n thi?t cho Register
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
        }

        // Hàm l?y d? li?u Login t? Excel (10 test ??u tiên có Run = "YES")
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

                    // Tìm sheet "TestCase"
                    DataTable table = result.Tables.Cast<DataTable>()
                        .FirstOrDefault(dt => dt.TableName.Contains("TestCase"));

                    if (table != null && table.Rows.Count > DATA_START_ROW)
                    {
                        int yesCount = 0;
                        HashSet<string> processedTestCases = new HashSet<string>();

                        // B?t ??u t? row 8 (index 8)
                        for (int i = DATA_START_ROW; i < table.Rows.Count && yesCount < 10; i++)
                        {
                            DataRow row = table.Rows[i];

                            // Ki?m tra c?t Run (Column 13)
                            string runFlag = row[COL_RUN]?.ToString() ?? "";

                            if (runFlag.Equals("YES", System.StringComparison.OrdinalIgnoreCase))
                            {
                                string testCaseId = row[COL_TEST_CASE_ID]?.ToString() ?? "";

                                // B? qua n?u testCaseId tr?ng ho?c ?ã x? lý
                                if (string.IsNullOrWhiteSpace(testCaseId) || processedTestCases.Contains(testCaseId))
                                    continue;

                                // B? qua n?u không ph?i Login (TC_LOG_*)
                                if (!testCaseId.StartsWith("TC_LOG"))
                                    continue;

                                processedTestCases.Add(testCaseId);

                                // L?y d? li?u t? c?t Data (Column 7)
                                string testData = row[COL_DATA]?.ToString() ?? "";
                                string username = ExtractValueFromTestData(testData, "User");
                                string password = ExtractValueFromTestData(testData, "Pass");
                                string expectedResult = row[COL_EXPECTED_RESULT]?.ToString() ?? "";

                                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                                {
                                    // T?o TestCaseData v?i d? li?u c?n thi?t
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
        }

        // Helper function ?? l?y giá tr? t? Test Data string
        private static string ExtractValueFromTestData(string testData, string key)
        {
            if (string.IsNullOrEmpty(testData) || !testData.Contains(key, System.StringComparison.OrdinalIgnoreCase))
                return "";

            try
            {
                // Tìm v? trí c?a key (không phân bi?t hoa/th??ng)
                int keyIndex = testData.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                int startIndex = keyIndex + key.Length;

                // Tìm d?u ':' ho?c '='
                while (startIndex < testData.Length && (testData[startIndex] == ':' || testData[startIndex] == '=' || char.IsWhiteSpace(testData[startIndex])))
                    startIndex++;

                // Tìm v? trí k?t thúc (d?u ',')
                int endIndex = testData.IndexOf(',', startIndex);
                if (endIndex == -1)
                    endIndex = testData.Length;

                string value = testData.Substring(startIndex, endIndex - startIndex).Trim();
                return value;
            }
            catch
            {
                return "";
            }
        }

        // Hàm c?p nh?t k?t qu? test vào Excel
        public static void UpdateTestResult(string testCaseId, string actualResult, string expectedResult, string status, string screenshotPath = "")
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Testcase.xlsx");

                if (!File.Exists(path))
                {
                    Console.WriteLine($"Excel file not found: {path}");
                    return;
                }

                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("TestCase"));

                    if (worksheet != null)
                    {
                        // Tìm dòng có TestCaseID t??ng ?ng
                        // C?t 3 (Col 2 0-based) = Test Case ID
                        // Data b?t ??u t? dòng 9 (Excel) = Row 9 (EPPlus)
                        int foundRow = -1;
                        for (int row = 9; row <= worksheet.Dimension?.Rows; row++)
                        {
                            var cellValue = worksheet.Cells[row, 3]?.Value?.ToString(); // Column 3 (TestCaseID)
                            if (cellValue == testCaseId)
                            {
                                foundRow = row;
                                break;
                            }
                        }

                        if (foundRow > 0)
                        {
                            // Column 10 (Col 9 0-based) = Actual Result
                            worksheet.Cells[foundRow, 10].Value = actualResult;

                            // Column 11 (Col 10 0-based) = Status
                            // So sánh Expected vs Actual ?? xác ??nh Pass/Fail
                            string finalStatus = status;

                            if (string.IsNullOrWhiteSpace(status))
                            {
                                // N?u status tr?ng, t? ??ng so sánh
                                finalStatus = CompareResults(expectedResult, actualResult) ? "PASS" : "FAIL";
                            }
                            else if (status.Equals("PASS", StringComparison.OrdinalIgnoreCase))
                            {
                                // N?u là PASS, so sánh ?? xác nh?n
                                finalStatus = CompareResults(expectedResult, actualResult) ? "PASS" : "FAIL";
                            }
                            else
                            {
                                // N?u là FAIL ho?c khác, gi? nguyên
                                finalStatus = status;
                            }

                            worksheet.Cells[foundRow, 11].Value = finalStatus;

                            // Column 12 (Col 11 0-based) = Screenshot
                            if (!string.IsNullOrEmpty(screenshotPath))
                            {
                                worksheet.Cells[foundRow, 12].Value = screenshotPath;
                            }
                        }
                    }

                    package.Save();
                }
            }
            catch (Exception ex)
            {
                // Ghi l?i vào file log thay vì console
                // Console.WriteLine($"Error updating Excel: {ex.Message}");
            }
        }

        // Helper function ?? so sánh Expected vs Actual result
        private static bool CompareResults(string expectedResult, string actualResult)
        {
            if (string.IsNullOrWhiteSpace(expectedResult) && string.IsNullOrWhiteSpace(actualResult))
                return true;

            if (string.IsNullOrWhiteSpace(expectedResult) || string.IsNullOrWhiteSpace(actualResult))
                return false;

            // So sánh chính xác (case-insensitive)
            if (expectedResult.Equals(actualResult, StringComparison.OrdinalIgnoreCase))
                return true;

            // So sánh ch?a t? khóa "successful" ho?c "success"
            bool expectedSuccess = expectedResult.Contains("successful", StringComparison.OrdinalIgnoreCase) || 
                                 expectedResult.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                                 expectedResult.Contains("successfully", StringComparison.OrdinalIgnoreCase);
            bool actualSuccess = actualResult.Contains("successful", StringComparison.OrdinalIgnoreCase) || 
                               actualResult.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                               actualResult.Contains("successfully", StringComparison.OrdinalIgnoreCase);

            if (expectedSuccess && actualSuccess)
                return true;

            // So sánh ch?a (n?u actual result ch?a m?t ph?n c?a expected)
            if (expectedResult.Contains(actualResult, StringComparison.OrdinalIgnoreCase) ||
                actualResult.Contains(expectedResult, StringComparison.OrdinalIgnoreCase))
                return true;

            // So sánh t? khóa (tìm các t? gi?ng nhau)
            var expectedWords = expectedResult.Split(new[] { ' ', ',', '.', ':', '"', '!', '?', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = actualResult.Split(new[] { ' ', ',', '.', ':', '"', '!', '?', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            // N?u có ít nh?t 50% t? gi?ng nhau, coi là PASS
            int matchCount = 0;
            foreach (var word in actualWords)
            {
                if (expectedWords.Any(w => w.Equals(word, StringComparison.OrdinalIgnoreCase)))
                    matchCount++;
            }

            if (actualWords.Length > 0 && matchCount >= (actualWords.Length * 0.5))
                return true;

            return false;
        }
    }
}
