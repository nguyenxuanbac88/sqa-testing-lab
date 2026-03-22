using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class AccountOverviewPage
    {
        private readonly IWebDriver driver;

        // --- Locators ---
        private readonly By _accountTable = By.Id("accountTable");
        private readonly By _accountDetails = By.Id("accountDetails");

        private readonly By _accountLinks = By.XPath("//table[@id='accountTable']//a[contains(@href, 'activity.htm')]");
        private readonly By _firstAccountLink = By.CssSelector("#accountTable > tbody > tr:nth-child(1) > td:nth-child(1) > a");
        private readonly By _firstAccountBalance = By.CssSelector("#accountTable > tbody > tr:nth-child(1) > td:nth-child(2)");
        private readonly By _firstAccountAvailable = By.CssSelector("#accountTable > tbody > tr:nth-child(1) > td:nth-child(3)");

        // Lấy tất cả các ô Balance trong bảng (để tính tổng)
        // BỘ LOCATOR MỚI: Chỉ lấy Balance của các dòng là tài khoản thật (Né dòng Total ra)
        private readonly By _allBalances = By.XPath("//table[@id='accountTable']//tbody//tr[td/a]//td[2]");
        private readonly By _totalBalance = By.XPath("//b[contains(text(), 'Total')]/../following-sibling::td/b");

        private readonly By _detailsAccountId = By.Id("accountId");

        public AccountOverviewPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Actions ---
        public bool IsAccountTableDisplayed()
        {
            var elements = driver.FindElements(_accountTable);
            return elements.Count > 0 && elements[0].Displayed;
        }

        // 1. So sánh Balance và Available (Của tài khoản đầu tiên)
        public string CompareBalanceAndAvailable()
        {
            Thread.Sleep(2000);
            var balElements = driver.FindElements(_firstAccountBalance);
            var availElements = driver.FindElements(_firstAccountAvailable);

            if (balElements.Count > 0 && availElements.Count > 0)
            {
                string balance = balElements[0].Text.Trim();
                string available = availElements[0].Text.Trim();

                bool isMatch = balance.Equals(available, StringComparison.OrdinalIgnoreCase);
                return $"Balance: {balance} | Available Amount: {available} -> " + (isMatch ? "Khớp nhau" : "Không khớp");
            }
            return "Không lấy được dữ liệu cột Balance và Available Amount";
        }

        // 2. So sánh Tổng các Balance với cột Total ở dưới cùng
        public string CompareSumBalanceWithTotal()
        {
            Thread.Sleep(2000);
            var balances = driver.FindElements(_allBalances);
            var totalElement = driver.FindElements(_totalBalance);

            if (balances.Count > 0 && totalElement.Count > 0)
            {
                decimal sum = 0;
                foreach (var bal in balances)
                {
                    // Chuyển đổi chuỗi tiền tệ (VD: -$484.50) thành số thập phân để cộng
                    string text = bal.Text.Replace("$", "").Replace(",", "").Trim();
                    if (decimal.TryParse(text, out decimal val)) sum += val;
                }

                string totalText = totalElement[0].Text.Trim();
                string cleanTotal = totalText.Replace("$", "").Replace(",", "").Trim();
                decimal.TryParse(cleanTotal, out decimal totalVal);

                bool isMatch = (sum == totalVal);
                return $"Tổng Balance thực tế: ${sum} | Total web tính: {totalText} -> " + (isMatch ? "Khớp nhau" : "Không khớp");
            }
            return "Không lấy được dữ liệu để so sánh Total.";
        }

        public string GetAccountList()
        {
            Thread.Sleep(2000);
            var elements = driver.FindElements(_accountLinks);
            if (elements.Count == 0) return "Không có dữ liệu tài khoản";
            return string.Join(", ", elements.Select(e => e.Text.Trim()));
        }

        public void ClickFirstAccount()
        {
            for (int i = 0; i < 10; i++)
            {
                var elements = driver.FindElements(_firstAccountLink);
                if (elements.Count > 0)
                {
                    try
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].click();", elements[0]);
                        return;
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
            throw new Exception("Đã chờ 10 giây nhưng không bấm được vào ID tài khoản!");
        }

        public string GetAccountDetailsId()
        {
            Thread.Sleep(1500);
            var elements = driver.FindElements(_detailsAccountId);
            return elements.Count > 0 ? elements[0].Text.Trim() : "Không lấy được ID trong chi tiết";
        }
        // Trả lại hàm kiểm tra trang chi tiết tài khoản
        public bool IsAccountDetailsDisplayed()
        {
            Thread.Sleep(1500); // Đợi xíu cho trang load
            var elements = driver.FindElements(_accountDetails);
            return elements.Count > 0 && elements[0].Displayed;
        }
    }
}