using System;
using System.IO;
using System.Linq;
using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class RegisterPage
    {
        private IWebDriver _driver;

        public RegisterPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // 1. ??nh ngh?a các Elements cho Register
        private By _firstNameInput = By.Id("customer.firstName");
        private By _lastNameInput = By.Id("customer.lastName");
        private By _addressInput = By.Id("customer.address.street");
        private By _cityInput = By.Id("customer.address.city");
        private By _stateInput = By.Id("customer.address.state");
        private By _zipCodeInput = By.Id("customer.address.zipCode");
        private By _phoneInput = By.Id("customer.phoneNumber");
        private By _ssnInput = By.Id("customer.ssn");
        private By _usernameInput = By.Id("customer.username");
        private By _passwordInput = By.Id("customer.password");
        private By _confirmPasswordInput = By.Id("repeatedPassword");
        private By _registerButton = By.XPath("//input[@value='Register']");

        // Element cào k?t qu? tr? v?
        private By _errorMessage = By.ClassName("error");
        private By _successMessage = By.CssSelector("#rightPanel p");

        // 2. Các hành ??ng (Actions) - ?ã thêm Clear()
        public void EnterFirstName(string firstName)
        {
            var el = _driver.FindElement(_firstNameInput);
            el.Clear();
            el.SendKeys(firstName);
        }

        public void EnterLastName(string lastName)
        {
            var el = _driver.FindElement(_lastNameInput);
            el.Clear();
            el.SendKeys(lastName);
        }

        public void EnterAddress(string address)
        {
            var el = _driver.FindElement(_addressInput);
            el.Clear();
            el.SendKeys(address);
        }

        public void EnterCity(string city)
        {
            var el = _driver.FindElement(_cityInput);
            el.Clear();
            el.SendKeys(city);
        }

        public void EnterState(string state)
        {
            var el = _driver.FindElement(_stateInput);
            el.Clear();
            el.SendKeys(state);
        }

        public void EnterZipCode(string zipCode)
        {
            var el = _driver.FindElement(_zipCodeInput);
            el.Clear();
            el.SendKeys(zipCode);
        }

        public void EnterPhone(string phone)
        {
            var el = _driver.FindElement(_phoneInput);
            el.Clear();
            el.SendKeys(phone);
        }

        public void EnterSSN(string ssn)
        {
            var el = _driver.FindElement(_ssnInput);
            el.Clear();
            el.SendKeys(ssn);
        }

        public void EnterUsername(string username)
        {
            var el = _driver.FindElement(_usernameInput);
            el.Clear();
            el.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            var el = _driver.FindElement(_passwordInput);
            el.Clear();
            el.SendKeys(password);
        }

        public void EnterConfirmPassword(string confirmPassword)
        {
            var el = _driver.FindElement(_confirmPasswordInput);
            el.Clear();
            el.SendKeys(confirmPassword);
        }

        public void ClickRegisterButton()
        {
            _driver.FindElement(_registerButton).Click();
        }

        // Hàm Register hoàn ch?nh
        public void Register(string firstName, string lastName, string address, string city,
                             string state, string zipCode, string phone, string ssn,
                             string username, string password)
        {
            EnterFirstName(firstName);
            EnterLastName(lastName);
            EnterAddress(address);
            EnterCity(city);
            EnterState(state);
            EnterZipCode(zipCode);
            EnterPhone(phone);
            EnterSSN(ssn);
            EnterUsername(username);
            EnterPassword(password);
            EnterConfirmPassword(password); // ? ?ây b?n ?ang truy?n m?t kh?u trùng nhau ?? pass qua b??c xác nh?n
            ClickRegisterButton();
        }

        // HÀM M?I: ??c k?t qu? sau khi b?m ??ng ký
        public string GetRegisterResult()
        {
            // Tìm các thông báo l?i (ParaBank có th? tr? v? nhi?u l?i cùng lúc n?u b? tr?ng nhi?u ô)
            var errorElements = _driver.FindElements(_errorMessage);
            if (errorElements.Count > 0)
            {
                // Ghép t?t c? các l?i l?i thành m?t chu?i (ví d?: "First name is required. | Last name is required.")
                return string.Join(" | ", errorElements.Select(e => e.Text));
            }

            // N?u không có l?i, tìm ?o?n text thông báo t?o tài kho?n thành công
            var successElements = _driver.FindElements(_successMessage);
            if (successElements.Count > 0)
            {
                return successElements[0].Text;
            }

            return "Không l?y ???c thông báo t? web";
        }

        // Hàm ch?p screenshot (Gi? nguyên)
        public string TakeScreenshot(string testName)
        {
            try
            {
                string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string fileName = $"{testName}_{timestamp}.png";
                string filePath = Path.Combine(screenshotFolder, fileName);

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