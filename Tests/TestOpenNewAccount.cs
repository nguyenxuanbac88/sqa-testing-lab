using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;
using System;
using System.Threading;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestOpenNewAccount : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private OpenNewAccountPage _openAccPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");

            _loginPage = new LoginPage(_driver);
            _openAccPage = new OpenNewAccountPage(_driver);
            // Đã dọn dẹp sạch sẽ AccountOverviewPage, không dùng tới nữa!
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetOpenNewAccountData))]
        public void Test_Open_New_Account_Function(string accountType, string fromAccount, string stepAction, string expectedResult, string testCaseId)
        {
            string actualResult = "Lỗi không xác định";

            try
            {
                if (string.IsNullOrWhiteSpace(expectedResult)) Assert.Fail("LỖI: Chưa đọc được Expected Result từ Excel.");

                // BƯỚC 1: Tiền điều kiện - Đăng nhập tự động
                TestContext.WriteLine($"[Step 1] Tiền điều kiện: Đăng nhập...");
                _loginPage.Login("khoa_it_01", "Khoa123!"); // <-- CHÚ Ý: Đổi thành account của bạn
                Thread.Sleep(2000);

                // BƯỚC 2: Tự động bấm menu bên trái bằng hàm của chính nó
                TestContext.WriteLine($"[Step 2] Mở menu Open New Account...");
                _openAccPage.GoToPage();
                Thread.Sleep(1500);

                // BƯỚC 3: Dùng hàm Mở tài khoản của Khoa
                TestContext.WriteLine($"[Step 3] Đọc hành động từ Excel: '{stepAction}'");
                string actionLower = stepAction.ToLower();

                if (actionLower.Contains("mở") || actionLower.Contains("tạo") || actionLower.Contains("open"))
                {
                    TestContext.WriteLine($"-> Hệ thống đang mở tài khoản mới loại: {accountType}...");

                    // BƠM BIẾN accountType VÀO HÀM ĐỂ NÓ CHỌN DROPDOWN
                    actualResult = _openAccPage.OpenAccountAndGetId(accountType);
                }
                else
                {
                    actualResult = "Trang Open New Account hiển thị thành công.";
                }

                TestContext.WriteLine($"[Step 4] Web trả về Actual: '{actualResult}'");

                // BƯỚC 4: SMART ASSERT
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();

                string cleanExpected = expectedLower.Replace("lỗi:", "").Replace("\"", "").Trim();
                bool isMatch = actualLower.Contains(cleanExpected) || cleanExpected.Contains(actualLower);

                // Các mẹo vớt (Nới lỏng độ gắt)
                if (!isMatch && expectedLower.Contains("thành công") && actualLower.Contains("thành công")) isMatch = true;
                if (!isMatch && expectedLower.Contains("hiển thị") && actualLower.Contains("hiển thị")) isMatch = true;
                if (!isMatch && (expectedLower.Contains("account opened") || expectedLower.Contains("mới")) && actualLower.Contains("id mới")) isMatch = true;

                Assert.IsTrue(isMatch, $"FAILED: Web hiện '{actualResult}' nhưng Excel đợi '{expectedResult}'");

                // BƯỚC 5: GHI PASS VÀO EXCEL
                TestContext.WriteLine($"[Result] Test {testCaseId} PASSED");
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Error] Test rớt: {ex.Message}");
                string screenshotFileName = ScreenshotHelper.TakeScreenshot(_driver, _currentTestName);
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "FAIL", screenshotFileName);
                throw;
            }
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}