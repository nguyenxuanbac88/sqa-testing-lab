using System;
using System.Threading;
using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class FindTransactionPage
    {
        private readonly IWebDriver driver;

        // --- Locators Menu ---
        private readonly By _findTransMenuLink = By.LinkText("Find Transactions");

        // --- BỘ ID MỚI KHOA CUNG CẤP ---
        private readonly By _transactionIdInput = By.Id("transactionId");
        private readonly By _transactionDateInput = By.Id("transactionDate");
        private readonly By _fromDateInput = By.Id("fromDate");
        private readonly By _toDateInput = By.Id("toDate");
        private readonly By _amountInput = By.Id("amount");

        // --- Locators Nút Bấm (Bắt theo thứ tự 1,2,3,4 từ trên xuống) ---
        private readonly By _findByIdBtn = By.XPath("(//button[contains(text(), 'Find Transactions')])[1]");
        private readonly By _findByDateBtn = By.XPath("(//button[contains(text(), 'Find Transactions')])[2]");
        private readonly By _findByDateRangeBtn = By.XPath("(//button[contains(text(), 'Find Transactions')])[3]");
        private readonly By _findByAmountBtn = By.XPath("(//button[contains(text(), 'Find Transactions')])[4]");

        // --- Locators Kết Quả ---
        private readonly By _transactionTable = By.Id("transactionTable");
        private readonly By _errorMsg = By.XPath("//p[@class='error']");

        public FindTransactionPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void GoToPage()
        {
            Thread.Sleep(1000);
            driver.FindElement(_findTransMenuLink).Click();
            Thread.Sleep(1000); // Chờ trang load hoàn toàn
        }

        public string SearchTransaction(string type, string value, string value2 = "")
        {
            Thread.Sleep(1000);
            try
            {
                // Xóa sạch các ô trước khi nhập (để tránh bị dính data cũ)
                switch (type.ToUpper())
                {
                    case "ID":
                        driver.FindElement(_transactionIdInput).Clear();
                        driver.FindElement(_transactionIdInput).SendKeys(value);
                        driver.FindElement(_findByIdBtn).Click();
                        break;
                    case "DATE":
                        driver.FindElement(_transactionDateInput).Clear();
                        driver.FindElement(_transactionDateInput).SendKeys(value);
                        driver.FindElement(_findByDateBtn).Click();
                        break;
                    case "RANGE":
                        driver.FindElement(_fromDateInput).Clear();
                        driver.FindElement(_fromDateInput).SendKeys(value);
                        driver.FindElement(_toDateInput).Clear();
                        driver.FindElement(_toDateInput).SendKeys(value2);
                        driver.FindElement(_findByDateRangeBtn).Click();
                        break;
                    case "AMOUNT":
                        driver.FindElement(_amountInput).Clear();
                        driver.FindElement(_amountInput).SendKeys(value);
                        driver.FindElement(_findByAmountBtn).Click();
                        break;
                }

                Thread.Sleep(2500); // Chờ API trả kết quả về bảng

                // Kiểm tra kết quả hiển thị
                var tables = driver.FindElements(_transactionTable);
                if (tables.Count > 0 && tables[0].Displayed)
                {
                    // Đếm xem có bao nhiêu dòng kết quả
                    int rowCount = driver.FindElements(By.XPath("//table[@id='transactionTable']/tbody/tr")).Count;

                    // --- ÁP DỤNG LOGIC CỦA KHOA (> 0 LÀ TÌM THẤY, <= 0 LÀ KHÔNG TÌM THẤY) ---
                    if (rowCount > 0)
                    {
                        return $"Tìm thấy {rowCount} giao dịch thành công.";
                    }
                    else
                    {
                        // Nếu đếm ra 0 dòng, lập tức trả về câu này để KHỚP 100% VỚI EXCEL
                        return "Thông báo không tìm thấy kết quả.";
                    }
                }

                var errors = driver.FindElements(_errorMsg);
                if (errors.Count > 0) return $"Thông báo không tìm thấy kết quả: {errors[0].Text.Trim()}";

                return "Thông báo không tìm thấy kết quả.";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi thao tác: {ex.Message}";
            }
        }
    }
}