#region

using System;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Unit
{
    [TestFixture]
    public class ODataServiceTest
    {
        [Test]
        public void TestFormatInputReturnsNullOnNull()
        {
            var actual = ODataService.FormatInput(null);
            Assert.That(actual, Is.EqualTo(null));
        }

        [Test]
        public void TestFormatInputReturnsNullOnWhitespace()
        {
            var actual = ODataService.FormatInput("               ");
            Assert.That(actual, Is.EqualTo(null));
        }

        [Test]
        public void TestFormatInputReturnsTrimmed()
        {
            var actual = ODataService.FormatInput("   aaa    ");
            Assert.That(actual, Is.EqualTo("AAA"));
        }

        [Test]
        public void TestFormatInputReturnsUpperOnLower()
        {
            var actual = ODataService.FormatInput("aaa");
            Assert.That(actual, Is.EqualTo("AAA"));
        }

        [Test]
        public void TestRemoveInvalidCharactersCanRemove()
        {
            const string mixed = "',.<<::\\   :;>   >p...q,,,,y|||++==//?????fgc";
            var filtered = ODataService.RemoveInvalidCharacters(mixed);
            Assert.That(filtered, Is.EqualTo("pqyfgc"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnLong()
        {
            Assert.Throws<ArgumentException>(() =>
                ODataService.ValidateRecipientCountryCode("BBB"));
        }

        [Test]
        public void TestValidateRecipientCountryCodeThrowsOnShort()
        {
            Assert.Throws<ArgumentException>(() =>
                ODataService.ValidateRecipientCountryCode("B"));
        }

        [Test]
        public void TestValidateSectorThrowsOnLong()
        {
            Assert.Throws<ArgumentException>(() =>
                ODataService.ValidateSector("BBB"));
        }

        [Test]
        public void TestValidateSectorThrowsOnShort()
        {
            Assert.Throws<ArgumentException>(() =>
                ODataService.ValidateSector("B"));
        }



    }
}