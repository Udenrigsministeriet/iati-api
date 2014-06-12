#region

using System;
using System.Collections.Generic;
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

        [Test]
        public void TestValidateRegionCanValidate()
        {
            var codes = new List<string>
            {
                "798",
                "489",
                "88",
                "298",
                "998",
                "289",
                "189",
                "789",
                "389",
                "679",
                "498",
                "89",
                "889",
                "589",
                "689"
            };

            foreach (var code in codes)
            {
                Assert.DoesNotThrow(() => Activities.ValidateRegion(code));
            }
        }

        [Test]
        public void TestValidateRegionThrowsOnUnknown()
        {
            Assert.Throws<ArgumentException>(() => Activities.ValidateRegion("999"));
        }
    }
}