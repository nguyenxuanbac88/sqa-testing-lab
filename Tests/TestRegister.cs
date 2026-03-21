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

        // Test Register dùng dữ liệu từ Excel
        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetRegisterData))]
        public void Test_Register_Function(string firstName, string lastName, string address,
                                           string city, string state, string zipCode, string phone,
                                           string ssn, string username, string password,
                                           string expectedResult, string testCaseId)
        {
            string actualResult = "Không lấy được dữ liệu web";

            try
            {
                // Bước 1: Thực hiện điền form đăng ký
                TestContext.WriteLine($"[Step 1] Đang thử đăng ký tài khoản với user: '{username}'");
                _registerPage.Register(firstName, lastName, address, city, state, zipCode, phone, ssn, username, password);

                // Chờ web xử lý dữ liệu
                Thread.Sleep(2000);

                // Bước 2: Cào (Scrape) kết quả thực tế từ web (Báo lỗi hoặc Báo thành công)
                actualResult = _registerPage.GetRegisterResult();

                TestContext.WriteLine($"[Step 2] Actual Result thu được từ web: '{actualResult}'");
                TestContext.WriteLine($"[Step 3] Đang so sánh với Expected: '{expectedResult}'...");

                
                // BƯỚC 3: SMART ASSERT (So sánh theo từ khóa)
                bool isMatch = false;

                // Kịch bản 1: Nếu Excel yêu cầu kết quả là "Thành công"
                if (expectedResult.Contains("thành công", StringComparison.OrdinalIgnoreCase) ||
                    expectedResult.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    // Chỉ cần chữ cào được từ web có chứa "success" hoặc "successfully" là PASS
                    isMatch = actualResult.Contains("success", StringComparison.OrdinalIgnoreCase);
                }
                // Kịch bản 2: Các trường hợp test báo lỗi (ví dụ: trùng user, bỏ trống ô...)
                else
                {
                    // Kiểm tra chuỗi thực tế trên web có chứa từ khóa trong Excel không, HOẶC ngược lại
                    isMatch = actualResult.Contains(expectedResult, StringComparison.OrdinalIgnoreCase) ||
                              expectedResult.Contains(actualResult, StringComparison.OrdinalIgnoreCase);
                }

                // Giao cho NUnit phán xử dựa trên biến isMatch
                Assert.IsTrue(isMatch, $"LỖI: Web hiển thị ('{actualResult}') nhưng Excel lại mong đợi ('{expectedResult}')");

                // Bước 4: Nếu code lọt qua được Assert -> Nghĩa là test PASS
                TestContext.WriteLine($"[Result] Test {testCaseId} PASSED");
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, "PASS");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Error] Test rớt: {ex.Message}");

                // Gọi ScreenshotHelper thần thánh ở đây! Truyền _driver và tên test vào.
                string screenshotFileName = ScreenshotHelper.TakeScreenshot(_driver, _currentTestName);
                TestContext.WriteLine($"[Screenshot] Đã lưu ảnh: {screenshotFileName}");

                // Cập nhật Excel với trạng thái FAIL
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