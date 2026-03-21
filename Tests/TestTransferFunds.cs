using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.TestData;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestTransferFunds : IDisposable
    {
        private IWebDriver _driver;
        private string _currentTestName;
        private TransferFundsPage _transferPage;
        private AccountOverviewPage _overviewPage;

        private string _account1;
        private string _account2;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _driver = DriverFactory.InitDriver();

            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

            RegisterPage registerPage = new RegisterPage(_driver);
            string dynamicUser = "user_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            registerPage.Register("Test", "Transfer", "123 St", "City", "State", "12345", "0123456789", "123-45", dynamicUser, "Pass123!");
            System.Threading.Thread.Sleep(2000);

            _overviewPage = new AccountOverviewPage(_driver);

            _transferPage = _overviewPage.ClickTransferFundsMenu();
            System.Threading.Thread.Sleep(1500);
            _account1 = _transferPage.GetFirstAccountIdFromDropdown();

            _overviewPage.ClickOpenNewAccountMenu();
            OpenNewAccountPage openAccPage = new OpenNewAccountPage(_driver);
            _account2 = openAccPage.OpenAccountAndGetId();

            // SỬA LỖI FAIL 2: Ngủ thêm 3 giây để trang "Account Opened" load xong 100% cái menu
            System.Threading.Thread.Sleep(3000);

            _transferPage = _overviewPage.ClickTransferFundsMenu();
            System.Threading.Thread.Sleep(1500);
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

        [Test]
        public void TC_TRA_011_ChuyenTienThanhCong()
        {
            string testCaseId = "TC_TRA_011";
            string expectedResult = "Hiển thị \"Transfer Complete!\"";
            string actualResult = "";
            string status = "";

            try
            {
                string amountToTransfer = "100";
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);

                System.Threading.Thread.Sleep(2000);

                actualResult = _transferPage.GetResultMessage();

                // KIỂM TRA LINH HOẠT THEO KEYWORD: Chứa chữ Complete hoặc transferred
                bool isSuccess = actualResult.Contains("Complete") || actualResult.Contains("transferred") || actualResult.Contains("thành công");

                if (isSuccess)
                {
                    status = "PASS";
                    TestContext.WriteLine($"Test PASSED: {actualResult}");
                }
                else
                {
                    status = "FAIL";
                    TestContext.WriteLine($"Test FAILED. Expected to contain 'Complete', but got: {actualResult}");
                }

                ExcelHelper.UpdateTestResult(testCaseId, actualResult, expectedResult, status, "");
                Assert.IsTrue(isSuccess, "Giao dịch chuyển tiền không thành công.");
            }
            catch (Exception ex)
            {
                actualResult = "Lỗi Exception: " + ex.Message;
                RegisterPage rp = new RegisterPage(_driver);
                string screenshotPath = rp.TakeScreenshot(_currentTestName);
                string fileName = Path.GetFileName(screenshotPath);
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, expectedResult, "FAIL", fileName);
                throw;
            }
        }

        [Test]
        public void TC_TRA_017_BoTrongAmount()
        {
            string testCaseId = "TC_TRA_017";
            string expectedResult = "Báo lỗi yêu cầu nhập số tiền";
            string actualResult = "";
            string status = "FAIL";

            try
            {
                _transferPage.PerformTransfer("", _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                actualResult = _transferPage.GetResultMessage();

                // KIỂM TRA LINH HOẠT THEO KEYWORD: Parabank thường báo lỗi có chữ 'empty'
                bool isSuccess = actualResult.Contains("empty") || actualResult.Contains("required") || actualResult.Contains("error");

                if (isSuccess)
                {
                    status = "PASS";
                    TestContext.WriteLine($"Test PASSED: {actualResult}");
                }
                else
                {
                    status = "FAIL";
                    TestContext.WriteLine($"Test FAILED. Expected error message, but got: {actualResult}");
                }

                ExcelHelper.UpdateTestResult(testCaseId, actualResult, expectedResult, status, "");
                Assert.IsTrue(isSuccess, "Không bắt được lỗi bỏ trống số tiền.");
            }
            catch (Exception ex)
            {
                actualResult = ex.Message;
                RegisterPage rp = new RegisterPage(_driver);
                string screenshotPath = rp.TakeScreenshot(_currentTestName);
                string fileName = Path.GetFileName(screenshotPath);
                ExcelHelper.UpdateTestResult(testCaseId, actualResult, expectedResult, "FAIL", fileName);
                throw;
            }
        }

        // ==========================================================
        // CÁC TEST TẠM THỜI ĐỂ DỪNG MÀN HÌNH LẤY XPATH
        // ==========================================================

        [Test]
        public void Z_TamThoi_DungHinh_Lay_XPath_ThanhCong()
        {
            // 1. Tự động chuyển 100$ để ra màn hình Thành công
            _transferPage.PerformTransfer("100", _account1, _account2);

            // 2. ĐÓNG BĂNG TRÌNH DUYỆT 60 GIÂY
            TestContext.WriteLine(">>> TRÌNH DUYỆT ĐANG DỪNG 60S. HÃY BẤM F12 ĐỂ LẤY XPATH CHỮ 'TRANSFER COMPLETE!' <<<");
            System.Threading.Thread.Sleep(60000);
        }

        [Test]
        public void Z_TamThoi_DungHinh_Lay_XPath_LoiDo()
        {
            // 1. Tự động chuyển rỗng để ra màn hình Lỗi
            _transferPage.PerformTransfer("", _account1, _account2);

            // 2. ĐÓNG BĂNG TRÌNH DUYỆT 60 GIÂY
            TestContext.WriteLine(">>> TRÌNH DUYỆT ĐANG DỪNG 60S. HÃY BẤM F12 ĐỂ LẤY XPATH CHỮ ĐỎ! <<<");
            System.Threading.Thread.Sleep(60000);
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}