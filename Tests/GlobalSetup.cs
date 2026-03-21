using NUnit.Framework;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [SetUpFixture] // Thuộc tính này báo cho NUnit biết đây là file thiết lập tổng
    public class GlobalSetup
    {
        [OneTimeSetUp] // Chạy ĐÚNG 1 LẦN trước khi bắt đầu toàn bộ dự án test
        public void RunBeforeAnyTests()
        {
            TestContext.WriteLine("=== KHỞI ĐỘNG DỰ ÁN TEST ===");
            ScreenshotHelper.CleanOldScreenshots();
        }
    }
}