using OpenQA.Selenium;
using System;

namespace sqa_automation_testing.Pages
{
    public class RegisterPage
    {
        private IWebDriver _driver;

        public RegisterPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // ??nh ngh?a các Elements cho Register
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

        // Method nh?p First Name
        public void EnterFirstName(string firstName)
        {
            _driver.FindElement(_firstNameInput).SendKeys(firstName);
        }

        // Method nh?p Last Name
        public void EnterLastName(string lastName)
        {
            _driver.FindElement(_lastNameInput).SendKeys(lastName);
        }

        // Method nh?p Address
        public void EnterAddress(string address)
        {
            _driver.FindElement(_addressInput).SendKeys(address);
        }

        // Method nh?p City
        public void EnterCity(string city)
        {
            _driver.FindElement(_cityInput).SendKeys(city);
        }

        // Method nh?p State
        public void EnterState(string state)
        {
            _driver.FindElement(_stateInput).SendKeys(state);
        }

        // Method nh?p ZipCode
        public void EnterZipCode(string zipCode)
        {
            _driver.FindElement(_zipCodeInput).SendKeys(zipCode);
        }

        // Method nh?p Phone
        public void EnterPhone(string phone)
        {
            _driver.FindElement(_phoneInput).SendKeys(phone);
        }

        // Method nh?p SSN
        public void EnterSSN(string ssn)
        {
            _driver.FindElement(_ssnInput).SendKeys(ssn);
        }

        // Method nh?p Username
        public void EnterUsername(string username)
        {
            _driver.FindElement(_usernameInput).SendKeys(username);
        }

        // Method nh?p Password
        public void EnterPassword(string password)
        {
            _driver.FindElement(_passwordInput).SendKeys(password);
        }

        // Method nh?p Confirm Password
        public void EnterConfirmPassword(string confirmPassword)
        {
            _driver.FindElement(_confirmPasswordInput).SendKeys(confirmPassword);
        }

        // Method click Register button
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
            EnterConfirmPassword(password);
            ClickRegisterButton();
        }

        // Method ch?p screenshot
        public string TakeScreenshot(string testName)
        {
            try
            {
                string screenshotFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }

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
