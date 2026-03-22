using OpenQA.Selenium;
using sqa_automation_testing.Pages;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class TestUpdateProfile : IDisposable
    {
        private IWebDriver _driver;
        private string _currentTestName;
        private UpdateProfilePage _updateProfilePage;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;

            // 1. Tận dụng Driver tàng hình
            _driver = TransferFundsHelpers.InitDriverBypassCloudflare();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");
            TransferFundsHelpers.WaitForCloudflare(_driver);

            // 2. Tạo User mới để đảm bảo có trạng thái Login hợp lệ
            RegisterPage registerPage = new RegisterPage(_driver);
            string dynamicUser = "user_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            registerPage.Register("OldName", "OldLast", "Old Street", "Old City", "Old State", "11111", "0111111111", "123-45", dynamicUser, "Pass123!");
            System.Threading.Thread.Sleep(2000);

            // 3. Bấm sang menu Update Contact Info
            _driver.FindElement(By.LinkText("Update Contact Info")).Click();

            // Angular của Parabank đôi khi load data cũ hơi chậm, cho nó 2s để nạp dữ liệu lên form
            System.Threading.Thread.Sleep(2000);

            _updateProfilePage = new UpdateProfilePage(_driver);
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
        public void TC_UPD_011_CapNhatThongTinThanhCong()
        {
            string testCaseId = "TC_UPD_011";

            // Data Test gán cứng từ Excel
            string fName = "Khoa";
            string lName = "Nguyen";
            string address = "456 Le Loi";
            string city = "Da Nang";
            string state = "VN";
            string zip = "55000";
            string phone = "0988888888";

            string expectedKeyword = "Profile Updated";

            TestContext.WriteLine($"[DEBUG] Update Profile -> Đang cập nhật thành User: {fName} {lName}");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 1. Xóa dữ liệu cũ, điền dữ liệu mới và Lưu
                _updateProfilePage.UpdateInfo(fName, lName, address, city, state, zip, phone);
                _updateProfilePage.ClickUpdateProfile();

                System.Threading.Thread.Sleep(2000); // Đợi server xử lý

                // 2. Lấy thông báo thực tế
                actualResult = _updateProfilePage.GetResultMessage();

                // 3. So sánh
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

        [Test]
        public void TC_UPD_012_XoaTrangFirstName()
        {
            string testCaseId = "TC_UPD_012";

            // Data Test gán cứng (Biến đổi [Empty] thành chuỗi rỗng)
            string fName = "";
            string lName = "Nguyen";
            string address = "456 Le Loi";
            string city = "Da Nang";
            string state = "VN";
            string zip = "55000";
            string phone = "0988888888";

            // Từ khóa mong đợi từ file Excel
            string expectedKeyword = "First name is required";

            TestContext.WriteLine($"[DEBUG] Update Profile -> Cố tình xóa trắng First Name, chờ lỗi: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 1. Xóa dữ liệu cũ, điền dữ liệu mới (First Name = rỗng) và Bấm Lưu
                _updateProfilePage.UpdateInfo(fName, lName, address, city, state, zip, phone);
                _updateProfilePage.ClickUpdateProfile();

                System.Threading.Thread.Sleep(1500); // Đợi Validation hiển thị

                // 2. Lấy thông báo lỗi chữ đỏ từ web
                actualResult = _updateProfilePage.GetResultMessage();

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
        public void TC_UPD_017_SuaZipCodeThanhChuCai()
        {
            string testCaseId = "TC_UPD_017";

            // Data Test gán cứng - Bẫy Zip Code bằng chữ cái
            string fName = "Khoa";
            string lName = "Nguyen";
            string address = "456 Le Loi";
            string city = "Da Nang";
            string state = "VN";
            string zip = "ABCD"; // <--- Bẫy ở đây
            string phone = "0988888888";

            // Từ khóa tiếng Anh mong đợi cho lỗi sai định dạng số
            // (Thường là numeric, invalid format...)
            string expectedKeyword = "numeric";

            TestContext.WriteLine($"[DEBUG] Update Profile -> Nhập Zip Code là chữ '{zip}', chờ lỗi chứa từ khóa: '{expectedKeyword}'");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // 1. Điền Form và Bấm Lưu
                _updateProfilePage.UpdateInfo(fName, lName, address, city, state, zip, phone);
                _updateProfilePage.ClickUpdateProfile();

                System.Threading.Thread.Sleep(1500); // Đợi server/validation xử lý

                // 2. Lấy thông báo thực tế từ web
                actualResult = _updateProfilePage.GetResultMessage();

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
            // Data Test gán cứng từ Excel
            string fName = "Khoa";
            string lName = "Nguyen";
            string address = "456 Le Loi";
            string city = "Da Nang";
            string state = "VN";
            string zip = "55000";
            string phone = "0988888888";

            TestContext.WriteLine("[DEBUG] Đang thực hiện cập nhật thông tin...");

            // 1. Xóa dữ liệu cũ, điền dữ liệu mới và Lưu
            _updateProfilePage.UpdateInfo(fName, lName, address, city, state, zip, phone);
            _updateProfilePage.ClickUpdateProfile();

            // 2. CHẶN ĐỨNG TRÌNH DUYỆT TRONG 3 PHÚT (180,000 ms)
            TestContext.WriteLine("Đã bấm Update thành công! Browser sẽ đứng im 3 phút để bạn soi HTML.");
            TestContext.WriteLine("Bạn hãy bấm F12, dùng nút mũi tên (Inspect) chỉ vào dòng thông báo thành công để copy đoạn HTML nhé.");

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