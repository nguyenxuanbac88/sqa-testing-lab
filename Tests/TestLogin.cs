using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;
using System;
using System.IO;
using System.Threading;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestLogin : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            // THÊM DÒNG NÀY: Xóa sạch toàn bộ session, cookie cũ trước khi bắt đầu test mới
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            _loginPage = new LoginPage(_driver);
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

        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetLoginData))]
        public void Test_Login_Function(string username, string password, string expectedResult, string testCaseId)
        {
            string actualResult = "Không lấy được dữ liệu web";

            try
            {
                // Bước 1: Login
                TestContext.WriteLine($"[Step 1] Đang thử đăng nhập với user: '{username}'");
                _loginPage.Login(username, password);
                Thread.Sleep(2000);

                // Bước 2: Cào Actual Result
                actualResult = _loginPage.GetLoginResult();
                TestContext.WriteLine($"[Step 2] Actual Result thu được từ web: '{actualResult}'");
                TestContext.WriteLine($"[Step 3] Đang so sánh thông minh với Expected: '{expectedResult}'...");

                // Bước 3: SMART ASSERT (Siêu đơn giản theo ý Khoa)
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();
                bool isMatch = false;

                // 1. Nếu Excel mong đợi "thành công" -> Web có "overview" là PASS
                if (expectedLower.Contains("thành công"))
                {
                    isMatch = actualLower.Contains("overview");
                }
                // 2. Các trường hợp còn lại (Báo lỗi sai pass, bỏ trống...)
                else
                {
                    // Gọt bỏ chữ "lỗi:" và dấu ngoặc kép (") mà bạn gõ trong Excel đi để so khớp cho chuẩn
                    string cleanExpected = expectedLower.Replace("lỗi:", "").Replace("\"", "").Trim();

                    // Chỉ cần chuỗi Actual chứa cụm Expected là chốt PASS
                    isMatch = actualLower.Contains(cleanExpected) || cleanExpected.Contains(actualLower);
                }

                Assert.IsTrue(isMatch, $"LỖI: Web hiện '{actualResult}' nhưng Excel đợi '{expectedResult}'");

                // Bước 4: PASS
                TestContext.WriteLine($"[Result] Test {testCaseId} PASSED");
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                // Bước 5: FAIL -> Chụp ảnh và cập nhật Excel
                TestContext.WriteLine($"[Error] Test rớt: {ex.Message}");

                // Gọi ScreenshotHelper thần thánh
                string screenshotFileName = ScreenshotHelper.TakeScreenshot(_driver, _currentTestName);
                TestContext.WriteLine($"[Screenshot] Đã lưu ảnh: {screenshotFileName}");

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