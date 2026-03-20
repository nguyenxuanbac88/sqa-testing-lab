using System;
using System.IO;
using OpenQA.Selenium;

namespace sqa_automation_testing.Utilities
{
    public class ScreenshotHelper
    {
        private readonly string _screenshotPath;

        public ScreenshotHelper()
        {
            // T?o ???ng d?n folder Screenshots
            _screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");

            // T?o folder n?u ch?a t?n t?i
            if (!Directory.Exists(_screenshotPath))
            {
                Directory.CreateDirectory(_screenshotPath);
            }
        }

        /// <summary>
        /// Ch?p ?nh và l?u vào folder Screenshots
        /// </summary>
        public string TakeScreenshot(IWebDriver driver, string testName)
        {
            try
            {
                // T?o tên file v?i timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string screenshotFileName = $"{testName}_{timestamp}.png";
                string screenshotFilePath = Path.Combine(_screenshotPath, screenshotFileName);

                // Ch?p ?nh
                ITakesScreenshot takesScreenshot = (ITakesScreenshot)driver;
                Screenshot screenshot = takesScreenshot.GetScreenshot();
                screenshot.SaveAsFile(screenshotFilePath);

                return screenshotFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error taking screenshot: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// L?y ???ng d?n ??y ?? c?a folder Screenshots
        /// </summary>
        public string GetScreenshotFolderPath()
        {
            return _screenshotPath;
        }
    }
}
