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

                // Bước 3: SMART ASSERT CHO LOGIN
                bool isMatch = false;

                // Để debug dễ hơn, in ra chữ in thường để so sánh
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();

                // Kịch bản 1: Đăng nhập THÀNH CÔNG
                if (expectedLower.Contains("thành công") || expectedLower.Contains("success") || expectedLower.Contains("overview"))
                {
                    isMatch = actualLower.Contains("accounts overview");
                }
                // Kịch bản 2: Đăng xuất THÀNH CÔNG
                else if (expectedLower.Contains("logout") || expectedLower.Contains("đăng xuất"))
                {
                    isMatch = actualLower.Contains("customer login");
                }
                // Kịch bản 3: Cố tình nhập sai (Báo lỗi)
                else
                {
                    // Kiểm tra xem chữ trên web có chứa từ khóa nào trong Excel không
                    isMatch = actualLower.Contains(expectedLower) || expectedLower.Contains(actualLower);

                    // Bổ sung: Nếu bạn ghi chung chung chữ "sai mật khẩu" trong Excel
                    if (!isMatch && actualLower.Contains("could not be verified") && expectedLower.Contains("sai"))
                    {
                        isMatch = true; // Châm chước cho Pass luôn!
                    }
                }

                // IN RA LÝ DO TẠI SAO PASS/FAIL ĐỂ KIỂM CHỨNG
                if (isMatch)
                {
                    TestContext.WriteLine($"=> KẾT LUẬN: Web báo đúng như Excel mong đợi -> Đánh giá PASS!");
                }
                else
                {
                    TestContext.WriteLine($"=> KẾT LUẬN: Web KHÔNG GIỐNG Excel mong đợi -> Đánh giá FAIL!");
                }

                Assert.IsTrue(isMatch, $"LỖI: Web hiển thị ('{actualResult}') KHÁC VỚI mong đợi ('{expectedResult}')");

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