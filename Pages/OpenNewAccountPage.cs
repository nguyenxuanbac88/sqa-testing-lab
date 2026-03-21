using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class OpenNewAccountPage
    {
        private readonly IWebDriver driver;

        // --- Locators ---
        // Nút mở tài khoản mới
        private readonly By openNewAccountButton = By.XPath("//input[@value='Open New Account']");
        // Lấy số tài khoản mới vừa tạo thành công
        private readonly By newAccountIdLink = By.Id("newAccountId");

        public OpenNewAccountPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Actions ---

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