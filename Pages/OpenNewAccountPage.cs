using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI; // <--- Bắt buộc phải có cái này để trị Dropdown

namespace sqa_automation_testing.Pages
{
    public class OpenNewAccountPage
    {
        private readonly IWebDriver driver;

        // --- Locators ---
        private readonly By openNewAccountMenuLink = By.LinkText("Open New Account");

        // LOCATOR CỦA DROPDOWN TYPE
        private readonly By typeDropdown = By.Id("type");

        private readonly By openNewAccountButton = By.XPath("//input[@value='Open New Account']");
        private readonly By newAccountIdLink = By.Id("newAccountId");

        public OpenNewAccountPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Actions ---

        public void GoToPage()
        {
            Thread.Sleep(1000);
            driver.FindElement(openNewAccountMenuLink).Click();
        }

        // TRUYỀN THÊM BIẾN accountType TỪ EXCEL VÀO ĐÂY
        public string OpenAccountAndGetId(string accountType)
        {
            Thread.Sleep(1500);

            // TÍNH NĂNG CHỌN DROPDOWN (CHECKING / SAVINGS)
            if (!string.IsNullOrWhiteSpace(accountType))
            {
                // Dùng SelectElement chuyên trị thẻ <select>
                var selectType = new SelectElement(driver.FindElement(typeDropdown));

                // ParaBank dùng chữ in hoa toàn bộ, nên ta ép ToUpper() cho chắc ăn
                selectType.SelectByText(accountType.ToUpper());

                Thread.Sleep(500); // Đợi nửa giây cho web nhận diện cú click dropdown
            }
            else
            {
                // Nếu Excel bỏ trống, mặc định gán là CHECKING để in log cho đẹp
                accountType = "CHECKING";
            }

            // Bấm nút Open
            driver.FindElement(openNewAccountButton).Click();
            Thread.Sleep(2000);

            try
            {
                string newId = driver.FindElement(newAccountIdLink).Text;
                return $"Mở tài khoản {accountType} thành công. ID mới: {newId}";
            }
            catch
            {
                try
                {
                    string errorMsg = driver.FindElement(By.ClassName("error")).Text;
                    return $"Lỗi hiển thị: {errorMsg}";
                }
                catch
                {
                    return "Không mở được tài khoản và cũng không thấy thông báo lỗi.";
                }
            }
        }

        // Bấm mở tài khoản mới (dùng cấu hình mặc định là CHECKING)
        public string OpenAccountAndGetId()
        {
            // Parabank đôi khi load dropdown chậm ở trang này, nên đợi 1 xíu
            System.Threading.Thread.Sleep(1500);

            // Bấm nút Open
            driver.FindElement(openNewAccountButton).Click();

            // Đợi kết quả và trả về số tài khoản mới
            System.Threading.Thread.Sleep(1500);
            return driver.FindElement(newAccountIdLink).Text;
        }
    }
}