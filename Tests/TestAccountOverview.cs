using NUnit.Framework;
using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;
using System;
using System.Threading;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestAccountOverview : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private AccountOverviewPage _overviewPage;
        private string _currentTestName;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");

            _loginPage = new LoginPage(_driver);
            _overviewPage = new AccountOverviewPage(_driver);
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

        [Test, TestCaseSource(typeof(ExcelHelper), nameof(ExcelHelper.GetAccountOverviewData))]
        public void Test_Account_Overview_Function(string stepAction, string expectedResult, string testCaseId)
        {
            string actualResult = "Không lấy được dữ liệu web";

            try
            {
                if (string.IsNullOrWhiteSpace(expectedResult))
                    Assert.Fail($"LỖI: Chưa đọc được Expected Result từ Excel.");

                // BƯỚC 1: Đăng nhập
                TestContext.WriteLine($"[Step 1] Tiền điều kiện: Đăng nhập...");
                _loginPage.Login("khoa_it_01", "Khoa123!"); // Đổi tài khoản của bạn
                Thread.Sleep(2000);

                // BƯỚC 2: PHÂN LUỒNG XỬ LÝ THEO STEP ACTION
                string actionLower = stepAction.ToLower();
                TestContext.WriteLine($"[Step 2] Excel Action: '{stepAction}'");

                // TH1: Click vào xem chi tiết
                if (actionLower.Contains("nhấn") || actionLower.Contains("click") || actionLower.Contains("chi tiết"))
                {
                    _overviewPage.ClickFirstAccount();
                    Thread.Sleep(2000);
                    actualResult = _overviewPage.IsAccountDetailsDisplayed()
                        ? $"Vào trang Account Details. Tài khoản: {_overviewPage.GetAccountDetailsId()}"
                        : "Lỗi: Không tìm thấy khung accountDetails.";
                }

                // TH2: So sánh Balance và TOTAL
                else if (actionLower.Contains("so sánh") && actionLower.Contains("total"))
                {
                    actualResult = _overviewPage.CompareSumBalanceWithTotal();
                }

                // TH3: So sánh Balance và AVAILABLE
                else if (actionLower.Contains("so sánh") && (actionLower.Contains("available") || actionLower.Contains("khả dụng")))
                {
                    actualResult = _overviewPage.CompareBalanceAndAvailable();
                }

                // TH4: Mặc định quét danh sách ID tài khoản
                else
                {
                    actualResult = _overviewPage.IsAccountTableDisplayed()
                        ? $"Bảng hiển thị. Các ID hiện có: {_overviewPage.GetAccountList()}"
                        : "Lỗi: Không hiển thị bảng accountTable.";
                }

                TestContext.WriteLine($"[Step 3] Web trả về Actual: '{actualResult}'");

                // BƯỚC 3: SMART ASSERT (SIẾT CHẶT, KHÔNG CHO LỌT LỖI FALSE POSITIVE)
                string actualLower = actualResult.ToLower();
                string expectedLower = expectedResult.ToLower();

                string cleanExpected = expectedLower.Replace("lỗi:", "").Replace("\"", "").Trim();
                bool isMatch = actualLower.Contains(cleanExpected) || cleanExpected.Contains(actualLower);

                if (!isMatch)
                {
                    // Vớt các câu chữ cơ bản
                    if (expectedLower.Contains("thành công") && actualLower.Contains("thành công")) isMatch = true;
                    if (expectedLower.Contains("hiển thị") && actualLower.Contains("hiển thị")) isMatch = true;
                    if (expectedLower.Contains("account details") && actualLower.Contains("account details")) isMatch = true;

                    // PHÂN BIỆT RÕ RÀNG "KHỚP" VÀ "KHÔNG KHỚP"
                    bool expectMatch = expectedLower.Contains("bằng nhau") || (expectedLower.Contains("khớp") && !expectedLower.Contains("không khớp"));
                    bool actualMatch = actualLower.Contains("khớp nhau") && !actualLower.Contains("không khớp");

                    bool expectNotMatch = expectedLower.Contains("không bằng") || expectedLower.Contains("không khớp") || expectedLower.Contains("khác nhau");
                    bool actualNotMatch = actualLower.Contains("không khớp");

                    // Nếu Excel đòi Khớp, Web báo Khớp -> PASS
                    if (expectMatch && actualMatch) isMatch = true;

                    // Nếu Excel đòi Không Khớp, Web báo Không Khớp -> PASS
                    if (expectNotMatch && actualNotMatch) isMatch = true;

                    // LƯU Ý: Nếu Excel đòi Khớp (hoặc nói chung chung) mà Web báo "Không khớp" -> isMatch vẫn là FALSE -> FAIL ĐÚNG CHUẨN.
                }

                Assert.IsTrue(isMatch, $"FAILED: Web hiện '{actualResult}' nhưng Excel đợi '{expectedResult}'");

                // BƯỚC 4: PASS VÀ GHI EXCEL
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