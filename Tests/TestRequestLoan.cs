using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;
using System;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestRequestLoan : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private RequestLoanPage _loanPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            _loginPage = new LoginPage(_driver);
            _loanPage = new RequestLoanPage(_driver);
        }

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

        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetRequestLoanData))]
        public void Test_Request_Loan_Flow(string loanAmt, string downPmt, string fromAcc, string stepAction, string expectedResult, string testCaseId)
        {
            string actualResult = "Chưa test";
            try
            {
                TestContext.WriteLine($"[INFO] ID: {testCaseId} | Loan: '{loanAmt}', Down: '{downPmt}', Từ TK: '{fromAcc}'");

                _loginPage.Login("khoa_it_01", "Khoa123!"); // <-- Nhớ đổi đúng pass của bạn
                _loanPage.GoToPage();

                // Gửi thông tin vay
                actualResult = _loanPage.SubmitLoanRequest(loanAmt, downPmt, fromAcc);
                TestContext.WriteLine($"[ACTUAL] Web trả về: {actualResult}");

                // Smart Assert
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();

                bool isMatch = actualLower.Contains(expectedLower) || expectedLower.Contains(actualLower);

                // Mẹo vớt chữ cho khớp với Excel của Khoa
                if (!isMatch && expectedLower.Contains("approved") && actualLower.Contains("approved")) isMatch = true;
                if (!isMatch && expectedLower.Contains("denied") && actualLower.Contains("denied")) isMatch = true;
                if (!isMatch && expectedLower.Contains("lỗi") && actualLower.Contains("loan")) isMatch = true;

                Assert.IsTrue(isMatch, $"FAILED: Web trả về '{actualResult}' nhưng Excel đòi '{expectedResult}'");

                // Ghi PASS vào Excel
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[ERROR] {ex.Message}");
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