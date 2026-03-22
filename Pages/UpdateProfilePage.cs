using OpenQA.Selenium;

namespace sqa_automation_testing.Pages
{
    public class UpdateProfilePage
    {
        private readonly IWebDriver driver;

        // --- Region 1: Locators ---
        private readonly By firstNameInput = By.Id("customer.firstName");
        private readonly By lastNameInput = By.Id("customer.lastName");
        private readonly By addressInput = By.Id("customer.address.street");
        private readonly By cityInput = By.Id("customer.address.city");
        private readonly By stateInput = By.Id("customer.address.state");
        private readonly By zipInput = By.Id("customer.address.zipCode");
        private readonly By phoneInput = By.Id("customer.phoneNumber");

        private readonly By updateBtn = By.XPath("//input[@value='Update Profile']");

        // --- Region 2: Constructor ---
        public UpdateProfilePage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // --- Region 3: Actions ---
        public void UpdateInfo(string fName, string lName, string address, string city, string state, string zip, string phone)
        {
            ClearAndSendKeys(firstNameInput, fName);
            ClearAndSendKeys(lastNameInput, lName);
            ClearAndSendKeys(addressInput, address);
            ClearAndSendKeys(cityInput, city);
            ClearAndSendKeys(stateInput, state);
            ClearAndSendKeys(zipInput, zip);
            ClearAndSendKeys(phoneInput, phone);
        }

        // HÀM HELPER: Xóa trắng ô text rồi mới nhập (Cực kỳ quan trọng ở Form Update)
        private void ClearAndSendKeys(By locator, string text)
        {
            var element = driver.FindElement(locator);
            element.Clear();
            System.Threading.Thread.Sleep(200); // Nghỉ một chút để web nhận diện đã xóa
            element.SendKeys(text);
        }

        public void ClickUpdateProfile()
        {
            driver.FindElement(updateBtn).Click();
        }

        // ĐÃ NÂNG CẤP DỰA TRÊN CODE HTML THỰC TẾ
        public string GetResultMessage()
        {
            try
            {
                // 1. Bắt thông báo cập nhật thành công
                var successBox = driver.FindElements(By.Id("updateProfileResult"));
                if (successBox.Count > 0 && successBox[0].Displayed)
                {
                    // Lấy toàn bộ chữ trong div (bao gồm "Profile Updated" và câu xác nhận bên dưới)
                    return successBox[0].Text.Trim();
                }

                // 2. Bắt lỗi Validation (bỏ trống ô)
                var errors = driver.FindElements(By.CssSelector("span.error"));
                foreach (var err in errors)
                {
                    if (err.Displayed && !string.IsNullOrWhiteSpace(err.Text))
                    {
                        return err.Text.Trim();
                    }
                }

                // 3. Bắt lỗi hệ thống server (nếu web bị sập ngầm)
                var sysError = driver.FindElements(By.Id("updateProfileError"));
                if (sysError.Count > 0 && sysError[0].Displayed)
                {
                    return sysError[0].Text.Trim();
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