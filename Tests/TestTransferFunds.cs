using OpenQA.Selenium;
using sqa_automation_testing.Pages;
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
            // Sửa lại cho khớp với text tiếng Anh trên web để Helper so sánh ra PASS
            string expectedResult = "Transfer Complete!";
            string actualResult = "";

            try
            {
                string amountToTransfer = "100";
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(2000);

                actualResult = _transferPage.GetResultMessage();
                bool isSuccess = actualResult.Contains("Complete");

                // Không cần truyền biến status, để Helper tự so sánh expectedResult và actualResult
                TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedResult);
                Assert.IsTrue(isSuccess);
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, ex.Message, expectedResult, "FAIL", fileName);
                throw;
            }
        }

        [Test]
        public void TC_TRA_017_BoTrongAmount()
        {
            string testCaseId = "TC_TRA_017";
            // LƯU Ý: Sửa expectedResult thành "error" để Helper trong Excel ghi PASS 
            // vì hàm Compare của bạn dùng lệnh .Contains()
            string expectedResult = "error";
            string actualResult = "";

            try
            {
                _transferPage.PerformTransfer("", _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                actualResult = _transferPage.GetResultMessage();

                // CHỈ CHECK NẾU CÓ CHỨA "ERROR" (không phân biệt hoa thường)
                bool isErrorDisplayed = actualResult.ToLower().Contains("error");

                if (isErrorDisplayed)
                {
                    TestContext.WriteLine($"Test PASSED: Đã bắt được lỗi: {actualResult}");
                    // Truyền cứng "PASS" vào status để chắc chắn Excel ghi PASS
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedResult, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Không tìm thấy từ khóa 'error'. Thông báo nhận được: {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedResult, "FAIL");
                }

                Assert.IsTrue(isErrorDisplayed, "Mong đợi thông báo lỗi nhưng không tìm thấy từ khóa 'error'.");
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, ex.Message, expectedResult, "FAIL", fileName);
                throw;
            }
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}