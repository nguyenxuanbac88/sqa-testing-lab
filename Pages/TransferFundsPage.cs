using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_automation_testing.Pages
{
    public class TransferFundsPage
    {
        private readonly IWebDriver driver;

        // --- Region 1: Locators ---
        private readonly By amountInput = By.Id("amount");
        private readonly By fromAccountSelect = By.Id("fromAccountId");
        private readonly By toAccountSelect = By.Id("toAccountId");
        private readonly By transferButton = By.XPath("//input[@value='Transfer']");

        // Locator cho lỗi validation (ví dụ: Không nhập tiền) màu đỏ
        private readonly By validationError = By.CssSelector("span.error");

        // --- Region 2: Constructor ---
        public TransferFundsPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Region 3: Actions ---

        // Điền số tiền
        public void EnterAmount(string amount)
        {
            driver.FindElement(amountInput).Clear();
            driver.FindElement(amountInput).SendKeys(amount);
        }

        // Chọn tài khoản nguồn
        public void SelectFromAccount(string accountId)
        {
            // Thêm lệnh if: Chỉ chọn khi accountId có dữ liệu
            if (!string.IsNullOrEmpty(accountId))
            {
                SelectElement from = new SelectElement(driver.FindElement(fromAccountSelect));
                from.SelectByText(accountId);
            }
        }

        // Chọn tài khoản đích
        public void SelectToAccount(string accountId)
        {
            // Thêm lệnh if: Chỉ chọn khi accountId có dữ liệu
            if (!string.IsNullOrEmpty(accountId))
            {
                SelectElement to = new SelectElement(driver.FindElement(toAccountSelect));
                to.SelectByText(accountId);
            }
        }

        // Bấm nút Transfer
        public void ClickTransfer()
        {
            driver.FindElement(transferButton).Click();
        }

        // Method tổng hợp cho các testcases điền data
        public void PerformTransfer(string amount, string fromAccount, string toAccount)
        {
            EnterAmount(amount);
            SelectFromAccount(fromAccount);
            SelectToAccount(toAccount);
            ClickTransfer();
        }

        // ĐÃ NÂNG CẤP VỚI ID TỪ F12: Bắt chính xác thông báo đang hiển thị
        public string GetResultMessage()
        {
            try
            {
                // 1. Kiểm tra xem hộp Thành Công có đang được bật không
                IWebElement successBox = driver.FindElement(By.Id("showResult"));
                if (successBox.Displayed) // Nếu nó không bị ẩn (display: none)
                {
                    return successBox.Text; // Lấy toàn bộ chữ trong hộp này (Bao gồm "Transfer Complete!" và số tiền)
                }

                // 2. Kiểm tra xem hộp Lỗi Server (Error!) có đang được bật không
                IWebElement errorBox = driver.FindElement(By.Id("showError"));
                if (errorBox.Displayed)
                {
                    return errorBox.Text;
                }

                // 3. Nếu vẫn đang ở form điền tiền, tìm lỗi chữ đỏ (The amount cannot be empty)
                var validationErrors = driver.FindElements(By.CssSelector("p.error"));
                foreach (var err in validationErrors)
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

        // Hàm phụ trợ để lấy số tài khoản đầu tiên trong danh sách (dùng khi setup)
        public string GetFirstAccountIdFromDropdown()
        {
            SelectElement from = new SelectElement(driver.FindElement(fromAccountSelect));
            return from.Options[0].Text;
        }
    }
}