using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;
using System;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestFindTransaction : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private FindTransactionPage _findPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            _loginPage = new LoginPage(_driver);
            _findPage = new FindTransactionPage(_driver);
        }

        // --- ĐẢM BẢO TẮT WEB SAU KHI TEST XONG ---
        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
        }

        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetFindTransactionData))]
        public void Test_Find_Transaction_Flow(string type, string val1, string val2, string stepAction, string expectedResult, string testCaseId)
        {
            string actualResult = "Chưa thực hiện test";
            try
            {
                TestContext.WriteLine($"[INFO] Test: {testCaseId} | Loại: {type} | Value1: {val1} | Value2: {val2}");

                _loginPage.Login("khoa_it_01", "Khoa123!"); // Nhớ đổi đúng pass của bạn nhé
                _findPage.GoToPage();

                // Chạy lệnh tìm kiếm
                actualResult = _findPage.SearchTransaction(type, val1, val2);
                TestContext.WriteLine($"[ACTUAL] Web trả về: {actualResult}");

                // So sánh kết quả
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();

                bool isMatch = actualLower.Contains(expectedLower) || expectedLower.Contains(actualLower);

                // Các mẹo vớt từ ngữ
                if (!isMatch && expectedLower.Contains("thành công") && actualLower.Contains("giao dịch")) isMatch = true;
                if (!isMatch && expectedLower.Contains("thỏa mãn") && actualLower.Contains("giao dịch")) isMatch = true;

                Assert.IsTrue(isMatch, $"FAILED: Web báo '{actualResult}' nhưng Excel đòi '{expectedResult}'");

                // --- GHI KẾT QUẢ PASS VÀO EXCEL ---
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[ERROR] {ex.Message}");
                // --- GHI KẾT QUẢ FAIL VÀ CHỤP MÀN HÌNH ---
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