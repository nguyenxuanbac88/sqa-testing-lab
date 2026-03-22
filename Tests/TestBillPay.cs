using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestBillPay : IDisposable
    {
        private IWebDriver _driver;
        private string _currentTestName;
        private BillPayPage _billPayPage;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;

            // Tận dụng Driver tàng hình của nhánh Transfer Funds
            _driver = TransferFundsHelpers.InitDriverBypassCloudflare();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

            TransferFundsHelpers.WaitForCloudflare(_driver);

            RegisterPage registerPage = new RegisterPage(_driver);
            string dynamicUser = "user_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            registerPage.Register("Test", "BillPay", "123 St", "City", "State", "12345", "0123456789", "123-45", dynamicUser, "Pass123!");
            System.Threading.Thread.Sleep(2000);

            // Bấm trực tiếp vào Menu Bill Pay để né việc đụng file AccountOverviewPage của team
            _driver.FindElement(By.LinkText("Bill Pay")).Click();
            System.Threading.Thread.Sleep(1500);

            _billPayPage = new BillPayPage(_driver);
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
        public void TC_BIL_011_ThanhToanHoaDonThanhCong()
        {
            string testCaseId = "TC_BIL_011";

            // Data Test gán cứng theo yêu cầu
            string name = "Nước sạch SG";
            string address = "123 DBP";
            string city = "HCM";
            string state = "VN";
            string zip = "70000";
            string phone = "0901112222";
            string account = "13579";
            string verifyAccount = "13579";
            string amount = "50";

            // ĐÃ SỬA: Rút gọn từ khóa để né cái ID tài khoản động của Parabank
            string expectedKeyword = "successful";

            TestContext.WriteLine($"[DEBUG] Bill Pay -> Gửi cho: {name}, Số tiền: ${amount}");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 1. Điền Form và Gửi
                _billPayPage.FillPayeeInfo(name, address, city, state, zip, phone, account, verifyAccount, amount);
                _billPayPage.ClickSendPayment();

                System.Threading.Thread.Sleep(2000); // Đợi server xử lý hóa đơn

                // 2. Lấy thông báo
                actualResult = _billPayPage.GetResultMessage();

                // 3. So sánh
                isSuccess = actualResult.ToLower().Contains(expectedKeyword.ToLower());

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Bắt được từ khóa '{expectedKeyword}' trong '{actualResult}'");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Không tìm thấy '{expectedKeyword}'. Thực tế là: '{actualResult}'");
                    string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "FAIL", fileName);
                }
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, "Lỗi kịch bản Web: " + ex.Message, expectedKeyword, "FAIL", fileName);
                throw;
            }

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy thông báo '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_BIL_012_BoTrongPayeeName()
        {
            string testCaseId = "TC_BIL_012";

            // Data Test gán cứng theo yêu cầu (Xử lý [Empty] thành chuỗi rỗng)
            string name = "";
            string address = "123 DBP";
            string city = "HCM";
            string state = "VN";
            string zip = "70000";
            string phone = "0901112222";
            string account = "13579";
            string verifyAccount = "13579";
            string amount = "50";

            // Từ khóa mong đợi dựa trên kịch bản Excel
            string expectedKeyword = "Payee name is required";

            TestContext.WriteLine($"[DEBUG] Bill Pay -> Bỏ trống Payee Name, Số tiền: ${amount}");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 1. Điền Form và Gửi
                _billPayPage.FillPayeeInfo(name, address, city, state, zip, phone, account, verifyAccount, amount);
                _billPayPage.ClickSendPayment();

                System.Threading.Thread.Sleep(1500); // Đợi Validation hiển thị

                // 2. Lấy thông báo lỗi chữ đỏ từ web
                actualResult = _billPayPage.GetResultMessage();

                // 3. So sánh
                isSuccess = actualResult.ToLower().Contains(expectedKeyword.ToLower());

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Bắt được thông báo lỗi '{expectedKeyword}' trong '{actualResult}'");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Không tìm thấy '{expectedKeyword}'. Thực tế là: '{actualResult}'");
                    string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "FAIL", fileName);
                }
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, "Lỗi kịch bản Web: " + ex.Message, expectedKeyword, "FAIL", fileName);
                throw;
            }

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy thông báo '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_Temp_DungImDeLayXPath()
        {
            // Data Test gán cứng 
            string name = "Nước sạch SG";
            string address = "123 DBP";
            string city = "HCM";
            string state = "VN";
            string zip = "70000";
            string phone = "0901112222";
            string account = "13579";
            string verifyAccount = "13579";
            string amount = "50";

            TestContext.WriteLine("[DEBUG] Đang thực hiện gửi thanh toán...");

            // 1. Điền Form và Gửi
            _billPayPage.FillPayeeInfo(name, address, city, state, zip, phone, account, verifyAccount, amount);
            _billPayPage.ClickSendPayment();

            // 2. CHẶN ĐỨNG TRÌNH DUYỆT TRONG 3 PHÚT (180,000 ms)
            TestContext.WriteLine("Đã gửi thanh toán thành công! Browser sẽ đứng im 3 phút để bạn soi XPath.");
            TestContext.WriteLine("Bạn hãy bấm F12, dùng nút mũi tên (Inspect) chỉ vào dòng thông báo để lấy locator nhé.");

            System.Threading.Thread.Sleep(180000);

            // Hết 3 phút nó mới chạy qua đây và tự đóng browser
            TestContext.WriteLine("Đã hết thời gian dừng.");
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}