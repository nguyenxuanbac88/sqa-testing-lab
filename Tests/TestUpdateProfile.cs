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

        // 1. THÊM BIẾN NÀY ĐỂ LƯU TÊN USER DÙNG CHO VIỆC LOGIN LẠI
        private string _dynamicUser;

        [SetUp]
        public void Setup()
        {
            _currentTestName = TestContext.CurrentContext.Test.Name;

            _driver = TransferFundsHelpers.InitDriverBypassCloudflare();
            _driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");
            TransferFundsHelpers.WaitForCloudflare(_driver);

            RegisterPage registerPage = new RegisterPage(_driver);

            // 2. GÁN GIÁ TRỊ VÀO BIẾN TOÀN CỤC THAY VÌ BIẾN CỤC BỘ
            _dynamicUser = "user_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            registerPage.Register("OldName", "OldLast", "Old Street", "Old City", "Old State", "11111", "0111111111", "123-45", _dynamicUser, "Pass123!");
            System.Threading.Thread.Sleep(2000);

            _driver.FindElement(By.LinkText("Update Contact Info")).Click();
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
        public void TC_UPD_019_LuuTruDB()
        {
            string testCaseId = "TC_UPD_019";

            // Data Test dùng tên độc lạ để kiểm chứng
            string fName = "KhoaDB";
            string lName = "NguyenDB";
            string address = "789 Data St";
            string city = "Cloud City";
            string state = "VN";
            string zip = "99999";
            string phone = "0123456789";

            string expectedKeyword = "Dữ liệu mới vừa cập nhật";

            TestContext.WriteLine($"[DEBUG] Cập nhật thành '{fName}' -> Đăng xuất -> Đăng nhập lại kiểm tra DB.");

            string actualResult = "";
            bool isSuccess = false;

            try
            {
                // Bước 1: Cập nhật thông tin và Lưu
                _updateProfilePage.UpdateInfo(fName, lName, address, city, state, zip, phone);
                _updateProfilePage.ClickUpdateProfile();
                System.Threading.Thread.Sleep(2000);

                // Bước 2: Đăng xuất khỏi hệ thống
                _driver.FindElement(By.LinkText("Log Out")).Click();
                System.Threading.Thread.Sleep(1500);

                // Bước 3: Đăng nhập lại bằng đúng User vừa tạo ở hàm Setup
                _driver.FindElement(By.Name("username")).SendKeys(_dynamicUser);
                _driver.FindElement(By.Name("password")).SendKeys("Pass123!");
                _driver.FindElement(By.XPath("//input[@value='Log In']")).Click();
                System.Threading.Thread.Sleep(2000);

                // Bước 4: Chui lại vào trang Update Contact Info
                _driver.FindElement(By.LinkText("Update Contact Info")).Click();
                System.Threading.Thread.Sleep(2000); // Chờ Angular đổ data cũ từ DB lên form

                // Bước 5: "Móc" dữ liệu từ thẻ input ra xem có đúng chữ mình đã nhập không
                // Dùng .GetAttribute("value") để lấy chữ nằm bên trong ô nhập liệu
                string actualFirstName = _driver.FindElement(By.Id("customer.firstName")).GetAttribute("value");
                string actualLastName = _driver.FindElement(By.Id("customer.lastName")).GetAttribute("value");

                // Bước 6: So sánh
                isSuccess = (actualFirstName == fName && actualLastName == lName);
                actualResult = $"First Name trên web: '{actualFirstName}', Last Name: '{actualLastName}'";

                if (isSuccess)
                {
                    TestContext.WriteLine($"Test PASSED: Dữ liệu đã lưu thành công vào Database. {actualResult}");
                    TransferFundsHelpers.UpdateExcelResult(testCaseId, actualResult, expectedKeyword, "PASS");
                }
                else
                {
                    TestContext.WriteLine($"Test FAILED: Lỗi mất dữ liệu DB! Kì vọng: '{fName}', Thực tế: '{actualFirstName}'");
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

            Assert.IsTrue(isSuccess, $"[{testCaseId}] Thất bại. Dữ liệu từ DB không khớp. Thực tế: {actualResult}");
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}