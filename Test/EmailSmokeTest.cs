using NUnit.Framework;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base; // ✅ Required for Constants

namespace PageObjectModelCsharp.Tests
{
    [TestFixture]
    [Category(Constants.TestCategories.SMOKE)]
    public class EmailSmokeTest
    {
        [Test]
        public void SendEmailTest()
        {
            EmailTestUtility.SendTestEmail();
        }
    }
}