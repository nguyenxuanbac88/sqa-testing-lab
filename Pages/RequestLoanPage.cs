using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace sqa_automation_testing.Pages
{
    public class RequestLoanPage
    {
        private readonly IWebDriver driver;

        // --- Locators ---
        private readonly By _requestLoanMenu = By.LinkText("Request Loan");
        private readonly By _loanAmountInput = By.Id("amount");
        private readonly By _downPaymentInput = By.Id("downPayment");
        private readonly By _fromAccountIdDropdown = By.Id("fromAccountId");
        private readonly By _applyNowButton = By.XPath("//input[@value='Apply Now']");

        // --- Kết quả ---
        private readonly By _errorMessages = By.ClassName("error"); // Các lỗi đỏ khi bỏ trống hoặc nhập chữ
        private readonly By _loanStatus = By.Id("loanStatus"); // Trạng thái Approved / Denied
        private readonly By _responseMessage = By.XPath("//div[@ng-if='loanResponse.providerName']//p[1]"); // Lời nhắn chi tiết của hệ thống

        public RequestLoanPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void GoToPage()
        {
            Thread.Sleep(1000);
            driver.FindElement(_requestLoanMenu).Click();
            Thread.Sleep(1500); // Chờ load danh sách tài khoản
        }

        public string SubmitLoanRequest(string loanAmount, string downPayment, string fromAccount)
        {
            Thread.Sleep(1000);
            try
            {
                // 1. Nhập Loan Amount (Bỏ qua nếu thấy chữ [Empty])
                driver.FindElement(_loanAmountInput).Clear();
                if (!string.IsNullOrWhiteSpace(loanAmount) && !loanAmount.Contains("[Empty]"))
                    driver.FindElement(_loanAmountInput).SendKeys(loanAmount);

                // 2. Nhập Down Payment (Bỏ qua nếu thấy chữ [Empty])
                driver.FindElement(_downPaymentInput).Clear();
                if (!string.IsNullOrWhiteSpace(downPayment) && !downPayment.Contains("[Empty]"))
                    driver.FindElement(_downPaymentInput).SendKeys(downPayment);

                // 3. Chọn tài khoản (Nếu có data trong Excel)
                if (!string.IsNullOrWhiteSpace(fromAccount))
                {
                    var select = new SelectElement(driver.FindElement(_fromAccountIdDropdown));
                    select.SelectByText(fromAccount);
                }

                // 4. Submit
                driver.FindElement(_applyNowButton).Click();

                // Chờ xíu cho Angular load kết quả
                Thread.Sleep(3000);

                // --- 5. BẮT KẾT QUẢ BẰNG BỘ SELECTOR XỊN CỦA KHOA ---

                // TH1: Lỗi bỏ trống ô (Form Error)
                var emptyErrors = driver.FindElements(By.CssSelector("#requestLoanError > p"));
                if (emptyErrors.Count > 0 && emptyErrors[0].Displayed)
                {
                    return $"Lỗi form: {emptyErrors[0].Text.Trim()}";
                }

                // TH2: Form hợp lệ, đã gửi đi và có trạng thái duyệt
                var statusElements = driver.FindElements(_loanStatus); // Vẫn dùng ID loanStatus để lấy chữ Approved/Denied
                if (statusElements.Count > 0 && statusElements[0].Displayed)
                {
                    string status = statusElements[0].Text.Trim(); // Approved hoặc Denied
                    string detailMsg = "";

                    // Nếu Approved -> Bắt thẻ thành công
                    if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        var approvedMsg = driver.FindElements(By.CssSelector("#loanRequestApproved > p:nth-child(1)"));
                        if (approvedMsg.Count > 0) detailMsg = approvedMsg[0].Text.Trim();
                    }
                    // Nếu Denied -> Bắt thẻ thất bại
                    else if (status.Equals("Denied", StringComparison.OrdinalIgnoreCase))
                    {
                        var deniedMsg = driver.FindElements(By.CssSelector("#loanRequestDenied > p"));
                        if (deniedMsg.Count > 0) detailMsg = deniedMsg[0].Text.Trim();
                    }

                    return $"Loan Request {status}. Lời nhắn: {detailMsg}";
                }

                return "Không xác định được kết quả. Có thể do web tải quá chậm.";
            }
            catch (Exception ex)
            {
                return $"Lỗi thao tác: {ex.Message}";
            }
        }
    }
}