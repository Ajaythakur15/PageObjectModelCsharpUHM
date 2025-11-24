using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base; // ✅ Required for Constants

namespace PageObjectModelCsharp.Tests
{
    [TestClass]
    [TestCategory(Constants.TestCategories.SMOKE)]
    public class EmailSmokeTest
    {
        [TestMethod]
        public void SendEmailTest()
        {
            EmailTestUtility.SendTestEmail();
        }
    }
}