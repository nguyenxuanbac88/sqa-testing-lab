using System;
using System.IO;
using OpenQA.Selenium;

namespace sqa_automation_testing.Utilities
{
    public static class ScreenshotHelper
    {
        // Chuyển thành static method để có thể gọi trực tiếp tên class
        public static string TakeScreenshot(IWebDriver driver, string testName)
        {
            try
            {
                // Mẹo nhỏ: Đưa thư mục Screenshots ra ngoài thư mục gốc của project 
                // (giống cách làm file Excel) để bạn dễ nhìn thấy ảnh trong Visual Studio
                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string screenshotPath = Path.Combine(projectDir, "Screenshots");

                // Tạo folder nếu chưa tồn tại
                if (!Directory.Exists(screenshotPath))
                {
                    Directory.CreateDirectory(screenshotPath);
                }

                // Tạo tên file với timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string screenshotFileName = $"{testName}_{timestamp}.png";
                string screenshotFilePath = Path.Combine(screenshotPath, screenshotFileName);

                // Chụp ảnh và lưu
                ITakesScreenshot takesScreenshot = (ITakesScreenshot)driver;
                Screenshot screenshot = takesScreenshot.GetScreenshot();
                screenshot.SaveAsFile(screenshotFilePath);

                return screenshotFileName; // Chỉ trả về tên file để ghi vào Excel cho gọn
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI CHỤP ẢNH]: {ex.Message}");
                return "";
            }
        }
        // Hàm dọn dẹp ảnh cũ
        public static void CleanOldScreenshots()
        {
            try
            {
                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string screenshotPath = Path.Combine(projectDir, "Screenshots");

                if (Directory.Exists(screenshotPath))
                {
                    // Lấy tất cả các file .png trong thư mục và xóa
                    DirectoryInfo di = new DirectoryInfo(screenshotPath);
                    foreach (FileInfo file in di.GetFiles("*.png"))
                    {
                        file.Delete();
                    }
                    Console.WriteLine("-> Đã dọn sạch thư mục Screenshots cũ.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI DỌN DẸP ẢNH]: {ex.Message}");
            }
        }
    }
}