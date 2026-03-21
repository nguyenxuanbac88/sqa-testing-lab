using System;
using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class LoginPage
    {
        private IWebDriver _driver;

        // Constructor nhận driver từ Test Class truyền sang
        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // 1. Định nghĩa các Elements (Locators)
        private By _usernameInput = By.Name("username");
        private By _passwordInput = By.Name("password");
        private By _loginButton = By.CssSelector("input.button");

        // Locators để đọc kết quả sau khi bấm Login
        private By _errorMessage = By.ClassName("error");
        private By _successTitle = By.ClassName("title");

        // 2. Các hành động (Actions)
        public void EnterUsername(string username)
        {
            var element = _driver.FindElement(_usernameInput);
            element.Clear(); // Xóa trắng ô trước khi nhập
            element.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            var element = _driver.FindElement(_passwordInput);
            element.Clear();
            element.SendKeys(password);
        }

        public void ClickLoginButton()
        {
            _driver.FindElement(_loginButton).Click();
        }

        public void Login(string username, string password)
        {
            EnterUsername(username);
            EnterPassword(password);
            ClickLoginButton();
        }

        // HÀM: Đọc kết quả thực tế hiển thị trên web
        public string GetLoginResult()
        {
            // 1. Quét lỗi: Tìm tất cả thẻ có class "error", duyệt qua từng thẻ
            var errorElements = _driver.FindElements(_errorMessage);
            foreach (var err in errorElements)
            {
                // Nếu thẻ này thực sự có chứa chữ (không phải thẻ ẩn/trống) thì lụm luôn
                if (!string.IsNullOrWhiteSpace(err.Text))
                {
                    return err.Text.Trim();
                }
            }

            // 2. Quét thành công: Tương tự với thẻ tiêu đề "title"
            var titleElements = _driver.FindElements(_successTitle);
            foreach (var title in titleElements)
            {
                if (!string.IsNullOrWhiteSpace(title.Text))
                {
                    return title.Text.Trim();
                }
            }

            // Nếu quét nát trang web mà vẫn không thấy chữ nào
            return "Không lấy được thông báo từ web";
        }
    }
}