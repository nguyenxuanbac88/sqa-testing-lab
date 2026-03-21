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
            _driver = TransferFundsHelpers.InitDriverBypassCloudflare();

            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

            // =========================================================
            // BỔ SUNG: GỌI HÀM CHỜ CLOUDFLARE NGAY SAU KHI MỞ TRANG WEB
            // =========================================================
            TransferFundsHelpers.WaitForCloudflare(_driver);

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

            // 1. GỌI HELPER ĐỂ LẤY DATA TỪ EXCEL THEO ĐÚNG ID
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;          // Sẽ tự lấy giá trị "100"
            string expectedKeyword = testData.ExpectedKeyword;  // Sẽ tự lấy chữ "Transfer Complete!"

            // THÊM DÒNG NÀY ĐỂ SOI DATA:
            TestContext.WriteLine($"[DEBUG] Data từ Excel -> Số tiền nhập: '{amountToTransfer}', Từ khóa chờ đợi: '{expectedKeyword}'");

            string actualResult = "";

            try
            {
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(2000);

                actualResult = _transferPage.GetResultMessage();

                // 2. SO SÁNH LINH HOẠT VỚI TỪ KHÓA TỪ EXCEL
                bool isSuccess = actualResult.ToLower().Contains(expectedKeyword.ToLower());

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Bắt được từ khóa '{expectedKeyword}' trong '{actualResult}'");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Không tìm thấy '{expectedKeyword}'. Thực tế là: '{actualResult}'");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "FAIL");
                }

                Assert.IsTrue(isSuccess, $"Giao dịch thất bại. Không tìm thấy từ khóa '{expectedKeyword}'.");
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, ex.Message, expectedKeyword, "FAIL", fileName);
                throw;
            }
        }

        [Test]
        public void TC_TRA_012_SoTienChuyenLonHonSoDu()
        {
            string testCaseId = "TC_TRA_012";

            // 1. Gọi Helper để lấy Data từ Excel (Bỏ qua số tiền cứng, chỉ lấy Keyword)
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string expectedKeyword = testData.ExpectedKeyword;

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // ====================================================================
                // BẮT ĐẦU ĐOẠN CODE "TRÙNG LẶP" ĐỂ NÉ CONFLICT FILE CỦA KHOA
                // ====================================================================

                // Click menu Accounts Overview bằng lệnh trực tiếp
                _driver.FindElement(By.LinkText("Accounts Overview")).Click();
                System.Threading.Thread.Sleep(1500);

                // Dùng XPath tìm đích danh ô số dư nằm cạnh ID tài khoản nguồn
                string balanceText = _driver.FindElement(By.XPath($"//a[text()='{_account1}']/parent::td/following-sibling::td[1]")).Text;

                // Xóa ký tự thừa và ép kiểu an toàn theo chuẩn quốc tế
                balanceText = balanceText.Replace("$", "").Replace(",", "").Trim();
                double currentBalance = double.Parse(balanceText, System.Globalization.CultureInfo.InvariantCulture);

                // CỘNG THÊM 500 ĐÔ VÀO SỐ DƯ HIỆN TẠI ĐỂ ĐẢM BẢO LUÔN VƯỢT QUÁ SỐ DƯ
                string dynamicAmountToTransfer = (currentBalance + 500).ToString(System.Globalization.CultureInfo.InvariantCulture);

                TestContext.WriteLine($"[DEBUG] Số dư thực tế: {currentBalance} -> Cố tình chuyển: {dynamicAmountToTransfer}. Chờ báo lỗi: '{expectedKeyword}'");

                // Click quay lại menu Transfer Funds
                _driver.FindElement(By.LinkText("Transfer Funds")).Click();
                System.Threading.Thread.Sleep(1500);

                // ====================================================================
                // KẾT THÚC ĐOẠN CODE TRÙNG LẶP
                // ====================================================================


                // 2. Thực hiện chuyển tiền bằng con số khổng lồ vừa tính được
                _transferPage.PerformTransfer(dynamicAmountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(2000);

                // 3. Lấy thông báo thực tế từ Web
                actualResult = _transferPage.GetResultMessage();

                // 4. So sánh
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
                // Chỉ lọt vào đây nếu web sập hoặc code không tìm thấy nút bấm
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, "Lỗi kịch bản Web: " + ex.Message, expectedKeyword, "FAIL", fileName);
                throw;
            }

            // Đưa Assert ra ngoài Try-Catch để Exception của NUnit không bị tóm nhầm!
            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy từ khóa '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_TRA_013_SoTienChuyenBangKhong()
        {
            string testCaseId = "TC_TRA_013";

            // 1. Gọi Helper để lấy Data từ Excel (Sẽ tự lấy số 0)
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;
            string expectedKeyword = testData.ExpectedKeyword;

            TestContext.WriteLine($"[DEBUG] Data từ Excel -> Số tiền nhập: '{amountToTransfer}', Từ khóa chờ đợi: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 2. Thực hiện chuyển tiền với số 0
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                // 3. Lấy thông báo thực tế từ Web
                actualResult = _transferPage.GetResultMessage();

                // 4. So sánh
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

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy từ khóa '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_TRA_014_SoTienChuyenLaSoAm()
        {
            string testCaseId = "TC_TRA_014";

            // 1. Gọi Helper để lấy Data từ Excel (Sẽ tự lấy số "-50")
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;
            string expectedKeyword = testData.ExpectedKeyword;

            TestContext.WriteLine($"[DEBUG] Data từ Excel -> Số tiền nhập: '{amountToTransfer}', Từ khóa chờ đợi: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 2. Thực hiện chuyển tiền với số âm
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                // 3. Lấy thông báo thực tế từ Web
                actualResult = _transferPage.GetResultMessage();

                // 4. So sánh
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

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy từ khóa '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_TRA_015_NhapChuVaoOAmount()
        {
            string testCaseId = "TC_TRA_015";

            // 1. Gọi Helper để lấy Data từ Excel (Sẽ tự lấy chữ "abc")
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;
            string expectedKeyword = testData.ExpectedKeyword;

            TestContext.WriteLine($"[DEBUG] Data từ Excel -> Số tiền nhập: '{amountToTransfer}', Từ khóa chờ đợi: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 2. Thực hiện chuyển tiền với ký tự chữ
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                // 3. Lấy thông báo thực tế từ Web
                actualResult = _transferPage.GetResultMessage();

                // 4. So sánh
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

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy từ khóa '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_TRA_016_FromVaToCungMotTaiKhoan()
        {
            string testCaseId = "TC_TRA_016";

            // 1. Chỉ lấy Keyword mong đợi từ Excel (Bỏ qua data "123 -> 123")
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string expectedKeyword = testData.ExpectedKeyword;

            // 2. Ép cứng số tiền hợp lệ để tránh bị báo lỗi thiếu Amount
            string amountToTransfer = "10";

            TestContext.WriteLine($"[DEBUG] Chuyển tiền cùng 1 tài khoản ({_account1}). Số tiền: '{amountToTransfer}', Chờ báo lỗi: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 3. TRUYỀN _account1 VÀO CẢ FROM VÀ TO ĐỂ ÉP TRÙNG NHAU
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account1);
                System.Threading.Thread.Sleep(1500);

                // 4. Lấy thông báo và kiểm tra kết quả
                actualResult = _transferPage.GetResultMessage();
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

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Không tìm thấy từ khóa '{expectedKeyword}'. Thực tế: {actualResult}");
        }

        [Test]
        public void TC_TRA_017_BoTrongAmount()
        {
            string testCaseId = "TC_TRA_017";

            // 1. GỌI HELPER ĐỂ LẤY DATA TỪ EXCEL 
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;          // Sẽ tự hiểu [Empty] là "" (rỗng)
            string expectedKeyword = testData.ExpectedKeyword;  // Sẽ tự bóc được chữ "error"

            // THÊM DÒNG NÀY ĐỂ SOI DATA:
            TestContext.WriteLine($"[DEBUG] Data từ Excel -> Số tiền nhập: '{amountToTransfer}', Từ khóa chờ đợi: '{expectedKeyword}'");

            string actualResult = "";

            try
            {
                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(1500);

                actualResult = _transferPage.GetResultMessage();

                // 2. SO SÁNH LINH HOẠT
                bool isSuccess = actualResult.ToLower().Contains(expectedKeyword.ToLower());

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Đã bắt được lỗi: {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Không tìm thấy từ khóa '{expectedKeyword}'. Thông báo nhận được: {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "FAIL");
                }

                Assert.IsTrue(isSuccess, $"Mong đợi thông báo lỗi nhưng không tìm thấy từ khóa '{expectedKeyword}'.");
            }
            catch (Exception ex)
            {
                string fileName = TransferFundsHelpers.TakeScreenshotOnFail(_driver, _currentTestName);
                TransferFundsHelpers.UpdateExcelResult(testCaseId, ex.Message, expectedKeyword, "FAIL", fileName);
                throw;
            }
        }

        [Test]
        public void TC_TRA_019_KiemTraTKNguonBiTruTien()
        {
            string testCaseId = "TC_TRA_019";

            // 1. Lấy số tiền và Keyword mong muốn từ Excel
            var testData = TransferFundsHelpers.GetTestDataById(testCaseId);
            string amountToTransfer = testData.Amount;          // Sẽ tự lấy số "50"
            string expectedKeyword = testData.ExpectedKeyword;

            TestContext.WriteLine($"[DEBUG] Kịch bản: Kiểm tra trừ tiền TK Nguồn. Số tiền chuyển: '{amountToTransfer}'");

            string actualResult = "";
            bool isMathCorrect = false;

            try
            {
                // =========================================================
                // BƯỚC 1: LẤY SỐ DƯ TÀI KHOẢN NGUỒN TRƯỚC KHI CHUYỂN
                // =========================================================
                _driver.FindElement(By.LinkText("Accounts Overview")).Click();
                System.Threading.Thread.Sleep(1500);

                string balanceTextBefore = _driver.FindElement(By.XPath($"//a[text()='{_account1}']/parent::td/following-sibling::td[1]")).Text;
                balanceTextBefore = balanceTextBefore.Replace("$", "").Replace(",", "").Trim();
                double balanceBefore = double.Parse(balanceTextBefore, System.Globalization.CultureInfo.InvariantCulture);


                // =========================================================
                // BƯỚC 2: TIẾN HÀNH CHUYỂN TIỀN
                // =========================================================
                _driver.FindElement(By.LinkText("Transfer Funds")).Click();
                System.Threading.Thread.Sleep(1500);

                _transferPage.PerformTransfer(amountToTransfer, _account1, _account2);
                System.Threading.Thread.Sleep(2000); // Chờ giao dịch xử lý xong


                // =========================================================
                // BƯỚC 3: LẤY SỐ DƯ TÀI KHOẢN NGUỒN SAU KHI CHUYỂN
                // =========================================================
                _driver.FindElement(By.LinkText("Accounts Overview")).Click();
                System.Threading.Thread.Sleep(1500);

                string balanceTextAfter = _driver.FindElement(By.XPath($"//a[text()='{_account1}']/parent::td/following-sibling::td[1]")).Text;
                balanceTextAfter = balanceTextAfter.Replace("$", "").Replace(",", "").Trim();
                double balanceAfter = double.Parse(balanceTextAfter, System.Globalization.CultureInfo.InvariantCulture);


                // =========================================================
                // BƯỚC 4: LÀM TOÁN SO SÁNH
                // =========================================================
                double transferAmount = double.Parse(amountToTransfer, System.Globalization.CultureInfo.InvariantCulture);

                // Dùng Math.Abs để tránh lỗi sai số thập phân của máy tính (vd: 100.00 - 50 = 49.9999999)
                isMathCorrect = Math.Abs(balanceAfter - (balanceBefore - transferAmount)) < 0.01;

                actualResult = $"Số dư đầu: ${balanceBefore}, Sau chuyển: ${balanceAfter}. Bị trừ: ${(balanceBefore - balanceAfter)}";

                if (isMathCorrect)
                {
                    TestContext.WriteLine($"Test PASSED: {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: {actualResult}");
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

            // Chốt hạ Assert
            Assert.IsTrue(isMathCorrect, $"[{testCaseId}] Thất bại. Tài khoản nguồn không bị trừ đúng ${amountToTransfer}. Thực tế: {actualResult}");
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}