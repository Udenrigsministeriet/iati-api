#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Integration
{
    [TestFixture]
    public class DeploymentTests
    {
        //http://iatiquery.um.dk/ODataService.svc/Countries?$format=json
        [Test]
        public void TestAllUrls()
        {
            const string template = @"http://{0}/ODataService.svc/{1}";
            var hosts = new List<string> {"test", "iatiquery.um.dk", "iatiquery-test.um.dk"};

            var paths = new List<string>();
            paths.Add("$metadata");
            paths.Add("Aidtypes");
            paths.Add("Countries");
            paths.Add("Currencies");
            paths.Add("Finanslovs");
            paths.Add("Organisations");
            paths.Add("Sectors");
            paths.Add("Channels");

            var urls = (from host in hosts from path in paths select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                        .Select(request => (HttpWebResponse) request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
            }
        }

        // http://iatiquery.um.dk/Activities.ashx?RecipientCountryCode='et-eller-andet'
        [Test]
        public void TestGetActivitiesCanGet()
        {
            const string template = @"http://{0}/{1}";
            var hosts = new List<string> {"test"};

            var paths = new List<string>();
            paths.Add(@"/Activities.ashx?RecipientCountryCode='BF'");

            var urls = (from host in hosts from path in paths select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                        .Select(request => (HttpWebResponse) request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
            }
        }

        // http://iatiquery.um.dk/Activities.ashx?RecipientCountryCode='et-eller-andet'
        [Test]
        public void TestGetActivitiesCanGetForeachCountryCode()
        {
            const string template = @"http://{0}/{1}";
            var hosts = new List<string> { "test" };

            var entities = new IatiDbEntities();
            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();
            var paths =
                (from countryCode in countryCodes
                 select string.Format(@"/Activities.ashx?RecipientCountryCode={0}", countryCode)).ToList();

            var urls = (from host in hosts
                        from path in paths
                        select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest)WebRequest.Create(url))
                        .Select(request => (HttpWebResponse)request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
                Console.WriteLine(response.ResponseUri);
            }
        }

        [Test]
        [Ignore]
        public void TestGetActivitiesCanGetForeachSector()
        {
            var random = new Random();

            const string template = @"http://{0}/{1}";
            var hosts = new List<string> { "test" };

            var entities = new IatiDbEntities();
            var sectors = entities.Sectors;
            var sectorCodes =
                sectors.Where(s => s.category_code > 0 && s.category_code < 999).Select(s => s.category_code).ToList();

            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();

            var countryCodeIndex = random.Next(countryCodes.Count);
            var countryCode = countryCodes[countryCodeIndex];

            var paths =
                (from sectorCode in sectorCodes
                 select string.Format(@"Activities.ashx?RecipientConutryCode={0}&Sector={1}", countryCode, sectorCode)).ToList();

            var urls = (from host in hosts
                        from path in paths
                        select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest)WebRequest.Create(url))
                        .Select(request => (HttpWebResponse)request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
                Console.WriteLine(response.ResponseUri);
            }
        }

        // http://iatiquery.um.dk/Activities.ashx?RecipientCountryCode='et-eller-andet'
        [Test]
        [Ignore]
        public void TestGetActivitiesCanGetForeachCountryCodeAndSector()
        {
            const string template = @"http://{0}/{1}";
            var hosts = new List<string> {"test"};

            var entities = new IatiDbEntities();
            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();

            var sectors = entities.Sectors;
            var sectorCodes =
                sectors.Where(s => s.category_code > 0 && s.category_code < 999).Select(s => s.category_code).ToList();
            var paths =
                (from countryCode in countryCodes
                    from sectorCode in sectorCodes
                    select
                        string.Format(@"Activities.ashx?RecipientCountryCode={0}&sector={1}", countryCode, sectorCode))
                    .ToList();
            Console.WriteLine("Total number of paths: '{0}'", paths.Count);

            var urls = (from host in hosts
                from path in paths
                select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                        .Select(request => (HttpWebResponse) request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
                Console.WriteLine(response.ResponseUri);
            }
        }
    }
}