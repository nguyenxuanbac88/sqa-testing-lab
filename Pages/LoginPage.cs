using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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

        // Định nghĩa các Elements (Ví dụ cho ParaBank)
        private By _usernameInput = By.Name("username");
        private By _passwordInput = By.Name("password");
        private By _loginButton = By.CssSelector("input.button");

        // Hàm nhập username
        public void EnterUsername(string username)
        {
            _driver.FindElement(_usernameInput).SendKeys(username);
        }

        // Hàm nhập password
        public void EnterPassword(string password)
        {
            _driver.FindElement(_passwordInput).SendKeys(password);
        }

        // Hàm click nút Login
        public void ClickLoginButton()
        {
            _driver.FindElement(_loginButton).Click();
        }

        // Hàm login tổng hợp
        public void Login(string username, string password)
        {
            EnterUsername(username);
            EnterPassword(password);
            ClickLoginButton();
        }

        // Hàm chụp screenshot
        public string TakeScreenshot(string testName)
        {
            try
            {
                // Tạo thư mục Screenshots nếu chưa tồn tại
                string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }

                // Tạo tên file screenshot với timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string fileName = $"{testName}_{timestamp}.png";
                string filePath = Path.Combine(screenshotFolder, fileName);

                // Capture screenshot
                ITakesScreenshot screenshotDriver = (ITakesScreenshot)_driver;
                Screenshot screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error taking screenshot: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
