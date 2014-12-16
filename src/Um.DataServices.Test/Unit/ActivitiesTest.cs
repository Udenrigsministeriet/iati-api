#region

using System;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Unit
{
    [TestFixture]
    public class ActivitiesTest
    {
        [Test]
        public void TestFormatInputReturnsNullOnNull()
        {
            var actual = Activities.FormatInputString(null);
            Assert.That(actual, Is.EqualTo(null));
        }

        [Test]
        public void TestFormatInputReturnsNullOnWhitespace()
        {
            var actual = Activities.FormatInputString("               ");
            Assert.That(actual, Is.EqualTo(null));
        }

        [Test]
        public void TestFormatInputReturnsTrimmed()
        {
            var actual = Activities.FormatInputString("   aaa    ");
            Assert.That(actual, Is.EqualTo("AAA"));
        }

        [Test]
        public void TestFormatInputReturnsUpperOnLower()
        {
            var actual = Activities.FormatInputString("aaa");
            Assert.That(actual, Is.EqualTo("AAA"));
        }

        [Test]
        public void TestRemoveInvalidCharactersCanRemove1()
        {
            const string mixed = "',.<<::\\   :;>   >p...q,,,,y|||++==//?????fgc";
            var filtered = Activities.RemoveInvalidCharacters(mixed, @"[^a-zA-Z]");
            Assert.That(filtered, Is.EqualTo("pqyfgc"));
        }

        [Test]
        public void TestRemoveInvalidCharactersCanRemove2()
        {
            const string mixed = "',.<<::\\   :;>   >0...1,,,,y|||++==//?????f2c";
            var filtered = Activities.RemoveInvalidCharacters(mixed, @"[^0-9]");
            Assert.That(filtered, Is.EqualTo("012"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnLong()
        {
            Assert.Throws<ArgumentException>(() =>
                Activities.ValidateRecipientCountryCode("BBB"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnShort()
        {
            Assert.Throws<ArgumentException>(() =>
                Activities.ValidateRecipientCountryCode("B"));
        }

        [Test]
        public void TestValidateSectorThrowsOnLong()
        {
            Assert.Throws<ArgumentException>(() =>
                Activities.ValidateSector("BBB"));
        }

        [Test]
        public void TestValidateSectorThrowsOnShort()
        {
            Assert.Throws<ArgumentException>(() =>
                Activities.ValidateSector("B"));
        }
    }
}