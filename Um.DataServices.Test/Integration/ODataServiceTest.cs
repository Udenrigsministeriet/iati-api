#region

using System;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Integration
{
    [TestFixture]
    public class ODataServiceTest
    {
        [Test]
        public void TestValidateRecipientCountryCodeCanValidate()
        {
            Assert.DoesNotThrow(() => ODataService.ValidateRecipientCountryCode("SYA"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnUnknown()
        {
            Assert.Throws<ArgumentException>(() => ODataService.ValidateRecipientCountryCode("BD"));
        }
    }
}