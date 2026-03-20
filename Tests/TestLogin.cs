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

        // Debug test
        [Test]
        public void DebugTestExcelReading()
        {
            ExcelHelper.DebugExcelStructure();
            var testCases = ExcelHelper.GetLoginData().ToList();
            TestContext.WriteLine($"Found {testCases.Count} test cases");
            foreach (var tc in testCases)
            {
                TestContext.WriteLine($"  - {tc}");
            }
            Assert.That(testCases.Count, Is.GreaterThan(0), "No test cases found in Excel");
        }

        // Test Login với dữ liệu từ Excel (chỉ những test có Run = "YES")
        [TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetLoginData))]
        public void TestLoginWithValidCredentials(string username, string password)
        {
            try
            {
                // Perform login
                _loginPage.Login(username, password);

                // Wait for login to complete
                System.Threading.Thread.Sleep(2000);

                // Verify login success - kiểm tra URL có chứa '/overview' hoặc tương tự
                Assert.That(_driver.Url.Contains("overview") || !_driver.Url.Contains("index.htm"),
                    "Login failed - user should be redirected to overview page");

                // Update Excel - Test Pass
                string testCaseId = ExtractTestCaseId(_currentTestName);
                ExcelHelper.UpdateTestResult(testCaseId, "PASS", "");
                TestContext.WriteLine($"Test {testCaseId} PASSED");
            }
            catch (Exception ex)
            {
                // Capture screenshot khi test lỗi
                string screenshotPath = _loginPage.TakeScreenshot(_currentTestName);
                TestContext.AddTestAttachment(screenshotPath, "Test Failed Screenshot");

                // Update Excel - Test Fail với screenshot path
                string testCaseId = ExtractTestCaseId(_currentTestName);
                ExcelHelper.UpdateTestResult(testCaseId, "FAIL", screenshotPath);

                // Log lỗi
                TestContext.WriteLine($"Test failed with error: {ex.Message}");
                TestContext.WriteLine($"Screenshot saved at: {screenshotPath}");

                throw;
            }
        }

        // Helper function để extract TestCaseID từ test name
        private string ExtractTestCaseId(string testName)
        {
            // Format: "Login_TC001" -> "TC001"
            if (testName.Contains("_"))
            {
                return testName.Split('_').Last();
            }
            return testName;
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}
