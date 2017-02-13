using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using Um.DataServices.XmlFilePaging;
using System;

namespace Um.DataServices.Test.Unit
{
    [TestFixture]
    public class XmlFilePagingTest
    {
        [Test]
        public void TestCalculatePageSize()
        {
            var actual = XmlFilePager.CalculatePageSize(0, 10);
            Assert.That(actual, Is.EqualTo(0));

            actual = XmlFilePager.CalculatePageSize(9, 10);
            Assert.That(actual, Is.EqualTo(1));

            actual = XmlFilePager.CalculatePageSize(11, 10);
            Assert.That(actual, Is.EqualTo(2));
        }

        [Test]
        public void TestReadRootAndElementsFromXmlDocument()
        {
            var xml = GetXmlExample();
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var actual = XmlFilePager.ReadRootAndElementsFromXmlDocument(xmlReader, "iati-activity");
                Assert.That(actual.DocumentRoot.Name.LocalName, Is.EqualTo("iati-activities"));
                Assert.That(actual.Elements.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void TestPageAndIndexElements()
        {
            var rootAndElements = ReadRootAndElements(GetXmlExample());
            var actual = XmlFilePager.PageAndIndexElements(rootAndElements, 1).ToList();
            Assert.That(actual.Count, Is.EqualTo(2));

            var actualEntry = actual.Single(x => x.Key == 1);
            Assert.That(actualEntry.Value, Is.InstanceOf(typeof(XDocument)));
        }

        [Test]
        public void TestCreateNewXDocumentFromExistingRoot()
        {
            var rootAndElements = ReadRootAndElements(GetXmlExample());
            var originalRootName = rootAndElements.DocumentRoot.Name.LocalName;
            var originalElementName = rootAndElements.Elements.First().Name.LocalName;
            var originalElementCount = rootAndElements.Elements.Count();

            var actual = XmlFilePager.CreateNewXDocumentFromExistingRoot(rootAndElements.DocumentRoot, rootAndElements.Elements);
            Assert.That(actual.Root.Name.LocalName, Is.EqualTo(originalRootName));

            var actualElements = actual.Descendants(originalElementName).ToList();
            Assert.That(actualElements.Count, Is.EqualTo(originalElementCount));
        }

        [Test]
        public void TestIEnumerablePage()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var actual = numbers.Page(1).ToArray();
            Assert.That(actual.Length, Is.EqualTo(numbers.Length));

            actual = numbers.Page(3).ToArray();
            Assert.That(actual.Length, Is.EqualTo(2));
            
            actual = numbers.Page(5).ToArray();
            Assert.That(actual.Length, Is.EqualTo(1));
        }
        
        [Test]
        public void TestUriCombine()
        {
            Assert.That(new Uri("test1", UriKind.RelativeOrAbsolute).Combine("test2").ToString(), Is.EqualTo("test1/test2"));
            Assert.That(new Uri("test1/", UriKind.RelativeOrAbsolute).Combine("test2").ToString(), Is.EqualTo("test1/test2"));
            Assert.That(new Uri("test1", UriKind.RelativeOrAbsolute).Combine("/test2").ToString(), Is.EqualTo("test1/test2"));
            Assert.That(new Uri("test1/", UriKind.RelativeOrAbsolute).Combine("/test2").ToString(), Is.EqualTo("test1/test2"));
            Assert.That(new Uri("/test1/", UriKind.RelativeOrAbsolute).Combine("/test2/").ToString(), Is.EqualTo("/test1/test2/"));
            Assert.That(new Uri("", UriKind.RelativeOrAbsolute).Combine("/test2/").ToString(), Is.EqualTo("/test2/"));
            Assert.That(new Uri("/test1/", UriKind.RelativeOrAbsolute).Combine("").ToString(), Is.EqualTo("/test1/"));
        }

        private static XmlFilePager.DocumentParts ReadRootAndElements(string xml)
        {
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                return XmlFilePager.ReadRootAndElementsFromXmlDocument(xmlReader, "iati-activity");
            }
        }

        private static string GetXmlExample()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<iati-activities version=""1.03"" generated-datetime=""2014-10-26T22:40:01.617"">
  <iati-activity version=""1.03"" last-updated-datetime=""2014-10-26T04:57:22+00:00"" xml:lang=""en"" default-currency=""DKK"" hierarchy=""1"">
    <iati-identifier>DK-1-1000</iati-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Ministry of Foreign Affairs, Denmark"">104.BKF.24-25.</other-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Status"">Projektets formål er at styrke det forebyggende arbejde mod omskæring af kvinder såvel gennem den nationale komité som gennem de lokale provinskomiteer. I bevillingen er der lagt særlig vægt på at stille midler til rådighed for afholdelse af målrettede oplysningskampagner. Sideløbende hermed er der i 10 provinser, hvor kvindelig omskæring er mest udbredt, blevet afholdt intensive seminarer. I bevillingen har der endvidere været afsat midler til afholdelse af uddannelsesseminarer. Målgrupperne for disse har særligt været paramedicinsk personale og socialhjælpere i provinserne samt landsbyjorde-mødre og repræsentanter for kvinde-NGO'er. Projektet forløb godt, og det afsluttedes i 1999. Der er herefter startet en fase II (104.BKF.24-88).</other-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Risk Development"" />
    <title>Appui au Comité National de Lutte contre la Pratique de l'Excision des femmes (CNLPE)</title>
    <activity-status>Completed</activity-status>
    <activity-date type=""start-planned"" iso-date=""1996-01-01"">1996-01-01</activity-date>
    <activity-date type=""end-planned"" iso-date=""1998-12-31"">1998-12-31</activity-date>
    <reporting-org ref=""DK-1"" type=""10"" xml:lang=""en"">Ministry of Foreign Affairs, Denmark</reporting-org>
    <participating-org role=""Extending"" ref=""DK-1"" type=""10"" xml:lang=""en"">Ministry of Foreign Affairs, Denmark</participating-org>
    <recipient-country percentage=""100"" code=""BF"" xml:lang=""en"">Burkina Faso</recipient-country>
    <collaboration-type code=""1"">Bilateral</collaboration-type>
    <default-flow-type code=""10"">ODA</default-flow-type>
    <default-aid-type code=""C01"">Project-type interventions</default-aid-type>
    <default-finance-type code=""110"">Aid grant excluding debt reorganisation</default-finance-type>
    <related-activity type=""2"" ref=""DK-1-1000-803"" xml:lang=""en"">Appui au Comité National de Lutte contre la Pratique de l'Excision des femmes (CNLPE)</related-activity>
  </iati-activity>
  <iati-activity version=""1.03"" last-updated-datetime=""2014-10-26T04:57:22+00:00"" xml:lang=""en"" default-currency=""DKK"" hierarchy=""1"">
    <iati-identifier>DK-1-100023</iati-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Ministry of Foreign Affairs, Denmark"">104.Vietnam.30.m/53</other-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Status"">Alle aktiviteter er fysisk afsluttet. Der ventes i øjeblikket på, at provinserne skal refunderer renterne fra de penge, der er blevet investeret.</other-identifier>
    <other-identifier owner-ref=""DK-1"" owner-name=""Risk Development"">Ingen væsentlige risikoelementer.</other-identifier>
    <title>Support to Improvement of Sanitary Conditions in Peri-urban Areas of Buon Ma Thuot with Focus on the Poor and Ethnic Minorities</title>
    <description type=""1"" xml:lang=""en"">Projektet har til formål at forbedre levevilkårene for fattige og etniske minoriteter, som bor i randområderne til byen Buon Ma Thuot gennem forbedret adgang til sanitære forhold. Projektet omfatter aktiviteter såsom konstruktion af simple latriner for private husholdninger, sanitet og vand for skoler, træning og uddannelse af lokalt personale, oplysningskampagner og andre aktiviteter. 
Det forventes, at 66000 mennesker har fået adgang til forbedret sanitære forhold ved projektets afslutning. Projektet blev startet i december 2003.</description>
    <activity-status>Completed</activity-status>
    <activity-date type=""start-planned"" iso-date=""2003-01-01"">2003-01-01</activity-date>
    <activity-date type=""end-planned"" iso-date=""2007-12-31"">2007-12-31</activity-date>
    <reporting-org ref=""DK-1"" type=""10"" xml:lang=""en"">Ministry of Foreign Affairs, Denmark</reporting-org>
    <participating-org role=""Extending"" ref=""DK-1"" type=""10"" xml:lang=""en"">Ministry of Foreign Affairs, Denmark</participating-org>
    <recipient-country percentage=""100"" code=""VN"" xml:lang=""en"">Vietnam</recipient-country>
    <collaboration-type code=""1"">Bilateral</collaboration-type>
    <default-flow-type code=""10"">ODA</default-flow-type>
    <default-aid-type code=""C01"">Project-type interventions</default-aid-type>
    <default-finance-type code=""110"">Aid grant excluding debt reorganisation</default-finance-type>
    <sector vocabulary=""DAC"" code=""14030"" percentage=""100"" xml:lang=""en"">Basic drinking water supply and basic sanitation</sector>
    <related-activity type=""2"" ref=""DK-1-100023-14033"" xml:lang=""en"">Support to Improvement of Sanitary Conditions in Peri-urban Areas of Buon Ma Thuot with Focus on the Poor and Ethnic Minorities</related-activity>
  </iati-activity>
</iati-activities>
";
        }
    }
}
