using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class BillPayPage
    {
        private readonly IWebDriver driver;

        // --- Region 1: Locators ---
        private readonly By payeeNameInput = By.Name("payee.name");
        private readonly By payeeAddressInput = By.Name("payee.address.street");
        private readonly By payeeCityInput = By.Name("payee.address.city");
        private readonly By payeeStateInput = By.Name("payee.address.state");
        private readonly By payeeZipInput = By.Name("payee.address.zipCode");
        private readonly By payeePhoneInput = By.Name("payee.phoneNumber");
        private readonly By payeeAccountInput = By.Name("payee.accountNumber");
        private readonly By verifyAccountInput = By.Name("verifyAccount");
        private readonly By amountInput = By.Name("amount");

        private readonly By sendPaymentBtn = By.XPath("//input[@value='Send Payment']");

        // --- Region 2: Constructor ---
        public BillPayPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Region 3: Actions ---
        public void FillPayeeInfo(string name, string address, string city, string state, string zip, string phone, string account, string verifyAccount, string amount)
        {
            driver.FindElement(payeeNameInput).SendKeys(name);
            driver.FindElement(payeeAddressInput).SendKeys(address);
            driver.FindElement(payeeCityInput).SendKeys(city);
            driver.FindElement(payeeStateInput).SendKeys(state);
            driver.FindElement(payeeZipInput).SendKeys(zip);
            driver.FindElement(payeePhoneInput).SendKeys(phone);
            driver.FindElement(payeeAccountInput).SendKeys(account);
            driver.FindElement(verifyAccountInput).SendKeys(verifyAccount);
            driver.FindElement(amountInput).SendKeys(amount);
        }

        public void ClickSendPayment()
        {
            driver.FindElement(sendPaymentBtn).Click();
        }

        // ĐÃ NÂNG CẤP DỰA TRÊN HTML THỰC TẾ BẠN CUNG CẤP
        public string GetResultMessage()
        {
            try
            {
                // 1. Tìm cái thẻ div chứa ID "billpayResult"
                var successBoxes = driver.FindElements(By.Id("billpayResult"));
                if (successBoxes.Count > 0 && successBoxes[0].Displayed)
                {
                    // Trả về toàn bộ text bên trong thẻ div đó (Bao gồm cả thẻ h1 và các thẻ p)
                    return successBoxes[0].Text;
                }

                // 2. Nếu không thành công, bắt các thông báo lỗi chữ đỏ (Validation error)
                var errors = driver.FindElements(By.CssSelector(".error"));
                foreach (var err in errors)
                {
                    if (err.Displayed && !string.IsNullOrEmpty(err.Text))
                    {
                        return err.Text;
                    }
                }

                return "Không tìm thấy thông báo nào đang hiển thị!";
            }
            catch (Exception ex)
            {
                return "Lỗi khi lấy thông báo: " + ex.Message;
            }
        }
    }
}