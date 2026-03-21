using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.TestData;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestRegister : IDisposable
    {
        private IWebDriver _driver;
        private RegisterPage _registerPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");
            _registerPage = new RegisterPage(_driver);
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

        // Debug test ?? xem c?u trúc d? li?u t? Excel cho Register
        [Test]
        public void DebugTestRegisterDataReading()
        {
            var testCases = ExcelHelper.GetRegisterData().ToList();
            TestContext.WriteLine($"Found {testCases.Count} register test cases");
            foreach (var tc in testCases)
            {
                TestContext.WriteLine($"  - {tc}");
            }
            Assert.That(testCases.Count, Is.GreaterThan(0), "No register test cases found in Excel");
        }

        // Test Register v?i d? li?u t? Excel
        [TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetRegisterData))]
        public void TestRegisterWithValidData(string firstName, string lastName, string address, 
                                             string city, string state, string zipCode, string phone, 
                                             string ssn, string username, string password, 
                                             string expectedResult, string testCaseId)
        {
            string actualResult = "";
            string status = "";

            try
            {
                // Step 1: Perform register
                TestContext.WriteLine($"[Step 1] Registering with username: {username}");
                _registerPage.Register(firstName, lastName, address, city, state, zipCode, phone, ssn, username, password);

                // Wait for registration to complete
                System.Threading.Thread.Sleep(2000);

                // Step 2: Verify registration success
                TestContext.WriteLine($"[Step 2] Verifying registration - Current URL: {_driver.Url}");

                // Ki?m tra URL ho?c thông báo thành công
                bool isRegisterSuccess = _driver.Url.Contains("overview") || _driver.Url.Contains("register");

                // Ki?m tra có thông báo l?i không
                bool hasErrorMessage = false;
                try
                {
                    var errorElement = _driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'Error')]"));
                    hasErrorMessage = errorElement.Count > 0;
                }
                catch { }

                // Step 3: Xác ??nh k?t qu? th?c t?
                if (isRegisterSuccess && !hasErrorMessage)
                {
                    actualResult = "Registration successful";
                    TestContext.WriteLine($"[Step 3] Actual Result: {actualResult}");
                }
                else
                {
                    actualResult = "Registration failed";
                    TestContext.WriteLine($"[Step 3] Actual Result: {actualResult}");
                }

                // Step 4: So sánh Expected vs Actual
                TestContext.WriteLine($"[Step 4] Expected: '{expectedResult}' | Actual: '{actualResult}'");

                // Step 5: Ghi d? li?u vào Excel
                // UpdateTestResult s? t? ??ng so sánh và ghi PASS/FAIL
                ExcelHelper.UpdateTestResult(
                    testCaseId,
                    actualResult,
                    expectedResult,
                    "", // ?? tr?ng ?? hàm t? so sánh
                    ""
                );

                // Assertion
                Assert.That(isRegisterSuccess && !hasErrorMessage, 
                    $"Registration failed. Expected: {expectedResult}, Actual: {actualResult}");

                TestContext.WriteLine($"[Result] Test {testCaseId} PASSED");
            }
            catch (Exception ex)
            {
                actualResult = ex.Message;
                TestContext.WriteLine($"[Error] {ex.Message}");

                // Ch?p screenshot khi test l?i
                string screenshotPath = _registerPage.TakeScreenshot(_currentTestName);
                string screenshotFileName = !string.IsNullOrEmpty(screenshotPath) ? Path.GetFileName(screenshotPath) : "";

                TestContext.WriteLine($"[Screenshot] {screenshotFileName}");

                // C?p nh?t Excel v?i k?t qu? l?i
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
