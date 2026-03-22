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

            _driver = TransferFundsHelpers.InitDriverBypassCloudflare();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

            TransferFundsHelpers.WaitForCloudflare(_driver);

            RegisterPage registerPage = new RegisterPage(_driver);
            string dynamicUser = "user_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            registerPage.Register("Test", "BillPay", "123 St", "City", "State", "12345", "0123456789", "123-45", dynamicUser, "Pass123!");
            System.Threading.Thread.Sleep(2000);

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

        // =========================================================================
        // HÀM HELPER DÙNG CHUNG CHO CÁC KỊCH BẢN KIỂM TRA THÔNG BÁO (TEXT)
        // =========================================================================
        private void ExecuteStandardBillPayTest(string testCaseId, string name, string address, string city, string state, string zip, string phone, string account, string verifyAccount, string amount, string expectedKeyword)
        {
            TestContext.WriteLine($"[DEBUG] Bill Pay -> Amount: '{amount}', Chờ thông báo: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                _billPayPage.FillPayeeInfo(name, address, city, state, zip, phone, account, verifyAccount, amount);
                _billPayPage.ClickSendPayment();
                System.Threading.Thread.Sleep(2000);

                actualResult = _billPayPage.GetResultMessage();
                isSuccess = actualResult.ToLower().Contains(expectedKeyword.ToLower());

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Bắt được thông báo '{expectedKeyword}' trong '{actualResult}'");
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

        // =========================================================================
        // DANH SÁCH 6 TEST CASES ĐÃ ĐƯỢC TỐI ƯU
        // =========================================================================

        [Test]
        public void TC_BIL_011_ThanhToanHoaDonThanhCong()
        {
            // Truyền Full Data chuẩn
            ExecuteStandardBillPayTest("TC_BIL_011", "Nước sạch SG", "123 DBP", "HCM", "VN", "70000", "0901112222", "13579", "13579", "50", "successful");
        }

        [Test]
        public void TC_BIL_012_BoTrongPayeeName()
        {
            // Cố tình truyền Name là chuỗi rỗng ""
            ExecuteStandardBillPayTest("TC_BIL_012", "", "123 DBP", "HCM", "VN", "70000", "0901112222", "13579", "13579", "50", "Payee name is required");
        }

        [Test]
        public void TC_BIL_021_VerifyAccountKhongKhop()
        {
            // Cố tình truyền VerifyAccount = 99999
            ExecuteStandardBillPayTest("TC_BIL_021", "Nước sạch SG", "123 DBP", "HCM", "VN", "70000", "0901112222", "13579", "99999", "50", "The account numbers do not match");
        }

        [Test]
        public void TC_BIL_023_BoTrongAmount()
        {
            // Cố tình truyền Amount là chuỗi rỗng ""
            ExecuteStandardBillPayTest("TC_BIL_023", "Nước sạch SG", "123 DBP", "HCM", "VN", "70000", "0901112222", "13579", "13579", "", "The amount cannot be empty");
        }

        [Test]
        public void TC_BIL_027_TKNguonKhongDuTien()
        {
            // Cố tình truyền Amount siêu to 999999
            ExecuteStandardBillPayTest("TC_BIL_027", "Nước sạch SG", "123 DBP", "HCM", "VN", "70000", "0901112222", "13579", "13579", "999999", "insufficient");
        }

        [Test]
        public void TC_BIL_028_KiemTraTKNguonBiTruTien()
        {
            // GIỮ NGUYÊN CASE NÀY VÌ LOGIC LÀM TOÁN KHÁC BIỆT HOÀN TOÀN
            string testCaseId = "TC_BIL_028";
            string name = "Nước sạch SG", address = "123 DBP", city = "HCM", state = "VN", zip = "70000", phone = "0901112222", account = "13579", verifyAccount = "13579";
            string amountToPay = "50";
            string expectedKeyword = "Số dư bị trừ khớp chính xác";

            TestContext.WriteLine($"[DEBUG] Bill Pay -> Kịch bản làm toán. Sẽ thanh toán ${amountToPay} và check số dư.");
            string actualResult = "";
            bool isMathCorrect = false;

            try
            {
                // Bước 1: Lấy số dư đầu kỳ
                _driver.FindElement(By.LinkText("Accounts Overview")).Click();
                System.Threading.Thread.Sleep(1500);

                string accountId = _driver.FindElement(By.XPath("//table[@id='accountTable']/tbody/tr[1]/td[1]/a")).Text;
                string balanceTextBefore = _driver.FindElement(By.XPath("//table[@id='accountTable']/tbody/tr[1]/td[2]")).Text;
                balanceTextBefore = balanceTextBefore.Replace("$", "").Replace(",", "").Trim();
                double balanceBefore = double.Parse(balanceTextBefore, System.Globalization.CultureInfo.InvariantCulture);

                // Bước 2: Thanh toán
                _driver.FindElement(By.LinkText("Bill Pay")).Click();
                System.Threading.Thread.Sleep(1500);

                _billPayPage.FillPayeeInfo(name, address, city, state, zip, phone, account, verifyAccount, amountToPay);
                _billPayPage.ClickSendPayment();
                System.Threading.Thread.Sleep(2000);

                // Bước 3: Lấy số dư cuối kỳ
                _driver.FindElement(By.LinkText("Accounts Overview")).Click();
                System.Threading.Thread.Sleep(1500);

                string balanceTextAfter = _driver.FindElement(By.XPath($"//a[text()='{accountId}']/parent::td/following-sibling::td[1]")).Text;
                balanceTextAfter = balanceTextAfter.Replace("$", "").Replace(",", "").Trim();
                double balanceAfter = double.Parse(balanceTextAfter, System.Globalization.CultureInfo.InvariantCulture);

                // Bước 4: Làm toán
                double paymentAmount = double.Parse(amountToPay, System.Globalization.CultureInfo.InvariantCulture);
                isMathCorrect = Math.Abs(balanceAfter - (balanceBefore - paymentAmount)) < 0.01;

                actualResult = $"Số dư đầu: ${balanceBefore}, Sau thanh toán: ${balanceAfter}. Bị trừ: ${(balanceBefore - balanceAfter)}";

                if (isMathCorrect)
                {
                    TestContext.WriteLine($"Test PASSED: {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Toán sai! {actualResult}");
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

            Assert.IsTrue(isMathCorrect, $"[{testCaseId}] Thất bại. Số dư trừ không khớp. Thực tế: {actualResult}");
        }


        public void Dispose()
        {
            TearDown();
        }
    }
}