using NUnit.Framework;
using sqa_automation_testing.Utilities;

namespace sqa_automation_testing.Tests
{
    [TestFixture]
    public class ExcelAnalysisTest
    {
        [Test]
        public void AnalyzeExcelFile()
        {
            ExcelAnalyzer.AnalyzeExcelStructure();
        }
    }
}
