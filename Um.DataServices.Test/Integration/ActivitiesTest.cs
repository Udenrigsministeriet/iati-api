#region

using System;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Integration
{
    [TestFixture]
    public class ActivitiesTest
    {
        [Test]
        public void TestValidateRecipientCountryCodeCanValidate()
        {
            Assert.Throws<ArgumentException>(() => Activities.ValidateRecipientCountryCode("SYA"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnUnknown()
        {
            Assert.DoesNotThrow(() => Activities.ValidateRecipientCountryCode("BD"));
        }
    }
}