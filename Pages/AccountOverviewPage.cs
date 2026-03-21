using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class AccountOverviewPage
    {
        private readonly IWebDriver driver;

        // --- Region 1: Locators ---
        private readonly By transferFundsMenuLink = By.LinkText("Transfer Funds");

        // BỔ SUNG: Locator cho menu Open New Account
        private readonly By openNewAccountMenuLink = By.LinkText("Open New Account");

        // Locator động cho Account ID Link (dùng để click xem chi tiết hoặc get balance)
        private By AccountLinkId(string accountId) => By.XPath($"//a[text()='{accountId}']");

        // Locator động cho ô Balance của 1 account cụ thể
        private By AccountBalanceCell(string accountId) => By.XPath($"//a[text()='{accountId}']/parent::td/following-sibling::td[1]");


        // --- Region 2: Constructor ---
        public AccountOverviewPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Region 3: Actions ---

        // BỔ SUNG: Hàm click mở menu Open New Account
        public void ClickOpenNewAccountMenu()
        {
            driver.FindElement(openNewAccountMenuLink).Click();
        }

        // Chuyển hướng sang trang Transfer
        public TransferFundsPage ClickTransferFundsMenu()
        {
            driver.FindElement(transferFundsMenuLink).Click();
            // Trả về object của Page tiếp theo (TransferFundsPage)
            return new TransferFundsPage(driver);
        }

        // Click xem chi tiết 1 tài khoản
        public void ClickAccountDetails(string accountId)
        {
            driver.FindElement(AccountLinkId(accountId)).Click();
        }

        // Lấy số dư của 1 account cụ thể (Cần cho TC_TRA_019/020)
        public double GetAccountBalance(string accountId)
        {
            string balanceText = driver.FindElement(AccountBalanceCell(accountId)).Text;
            // Xử lý text "$100.00" -> 100.00
            balanceText = balanceText.Replace("$", "").Trim();
            return double.Parse(balanceText);
        }
    }
}