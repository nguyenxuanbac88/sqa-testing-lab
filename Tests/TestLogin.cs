using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.TestData;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestLogin : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private string _currentTestName;

        // Setup - chạy trước mỗi test
        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            _loginPage = new LoginPage(_driver);
        }

        // TearDown - chạy sau mỗi test
        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        // Test Login với dữ liệu từ Excel (chỉ những test có Run = "YES")
        [TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetLoginData))]
        public void TestLoginWithValidCredentials(string username, string password, string expectedResult, string testCaseId)
        {
            string actualResult = "";
            string status = "";

            try
            {
                // Step 1: Perform login
                TestContext.WriteLine($"[Step 1] Login with username: {username}");
                _loginPage.Login(username, password);

                // Wait for login to complete
                System.Threading.Thread.Sleep(2000);

                // Step 2: Verify login success
                TestContext.WriteLine($"[Step 2] Verifying - Current URL: {_driver.Url}");

                // Kiểm tra URL hoặc thông báo thành công
                bool isLoginSuccess = _driver.Url.Contains("overview") || _driver.Url.Contains("accounts");

                // Kiểm tra có thông báo lỗi không
                bool hasErrorMessage = false;
                try
                {
                    var errorElement = _driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'Error')]"));
                    hasErrorMessage = errorElement.Count > 0;
                }
                catch { }

                // Step 3: Xác định kết quả thực tế
                if (isLoginSuccess && !hasErrorMessage)
                {
                    actualResult = "Login successful";
                    TestContext.WriteLine($"[Step 3] Actual Result: {actualResult}");
                }
                else
                {
                    actualResult = "Login failed";
                    TestContext.WriteLine($"[Step 3] Actual Result: {actualResult}");
                }

                // Step 4: So sánh Expected vs Actual
                TestContext.WriteLine($"[Step 4] Expected: '{expectedResult}' | Actual: '{actualResult}'");

                // Step 5: Ghi dữ liệu vào Excel
                // UpdateTestResult sẽ tự động so sánh và ghi PASS/FAIL
                ExcelHelper.UpdateTestResult(
                    testCaseId,
                    actualResult,
                    expectedResult,
                    "", // Để trống để hàm tự so sánh
                    ""
                );

                // Assertion
                Assert.That(isLoginSuccess && !hasErrorMessage, 
                    $"Login failed. Expected: {expectedResult}, Actual: {actualResult}");

                TestContext.WriteLine($"[Result] Test {testCaseId} PASSED");
            }
            catch (Exception ex)
            {
                actualResult = ex.Message;
                TestContext.WriteLine($"[Error] {ex.Message}");

                // Chụp screenshot khi test lỗi
                string screenshotPath = _loginPage.TakeScreenshot(_currentTestName);
                string screenshotFileName = !string.IsNullOrEmpty(screenshotPath) ? Path.GetFileName(screenshotPath) : "";

                TestContext.WriteLine($"[Screenshot] {screenshotFileName}");

                // Cập nhật Excel với kết quả lỗi
                ExcelHelper.UpdateTestResult(
                    testCaseId,
                    actualResult,
                    expectedResult,
                    "FAIL", // Ghi FAIL khi có exception
                    screenshotFileName
                );

                throw;
            }
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}
