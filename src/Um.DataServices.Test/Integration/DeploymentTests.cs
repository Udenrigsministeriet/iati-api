#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Um.DataServices.Web;

#endregion

namespace Um.DataServices.Test.Integration
{
    [TestFixture]
    public class DeploymentTests
    {
        private const string HostDev = "iatiquery-dev.um.dk";
        private const string HostTest = "iatiquery-test.um.dk";
        private const string HostProd = "iatiquery.um.dk";

        private readonly List<string> _hosts = new List<string> {HostDev};

        //http://iatiquery.um.dk/v2/ODataService.svc/Countries?$format=json
        [Test]
        public void TestAllUrls()
        {
            const string template = @"http://{0}/v2/ODataService.svc/{1}";

            var paths = new List<string>
            {
                "$metadata",
                "Aidtypes",
                "Activities",
                "Countries",
                "Currencies",
                "Finanslovs",
                "Organisations",
                "Sectors",
                "Regions",
                "Channels"
            };

            var urls = (from host in _hosts from path in paths select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                    .Select(request => (HttpWebResponse) request.GetResponse()))
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        // http://iatiquery.um.dk/v2/Activities.ashx?RecipientCountryCode='et-eller-andet'
        [Test]
        public void TestGetActivitiesCanGet()
        {
            const string template = @"http://{0}/v2/{1}";

            var paths = new List<string> {@"Activities.ashx?RecipientCountryCode='BF'"};

            var urls = (from host in _hosts from path in paths select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                    .Select(request => (HttpWebResponse) request.GetResponse()))
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
        }

        // http://iatiquery.um.dk/v2/Activities.ashx?RecipientCountryCode='et-eller-andet'
        [Test]
        public void TestGetActivitiesCanGetForeachCountryCode()
        {
            const string template = @"http://{0}/v2/{1}";

            var entities = new IatiDbEntities();
            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();
            var paths =
            (from countryCode in countryCodes
                select $@"Activities.ashx?RecipientCountryCode={countryCode}").ToList();

            var urls = (from host in _hosts
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

        [Test]
        [Ignore]
        public void TestGetActivitiesCanGetForeachCountryCodeAndSector()
        {
            const string template = @"http://{0}/v2/{1}";

            var entities = new IatiDbEntities();
            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();

            var sectors = entities.Sectors;
            var sectorCodes =
                sectors.Where(s => (s.category_code > 0) && (s.category_code < 999))
                    .Select(s => s.category_code)
                    .ToList();
            var paths =
                (from countryCode in countryCodes
                        from sectorCode in sectorCodes
                        select
                        $@"Activities.ashx?RecipientCountryCode={countryCode}&sector={sectorCode}")
                    .ToList();
            Console.WriteLine("Total number of paths: '{0}'", paths.Count);

            var urls = (from host in _hosts
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

        // http://iatiquery.um.dk/Activities.ashx?Region='something'
        [Test]
        public void TestGetActivitiesCanGetForeachRegion()
        {
            const string template = @"http://{0}/v2/{1}";

            var entities = new IatiDbEntities();
            var regions = entities.Regions;
            var regionCodes =
                regions.Select(c => c.code).ToList();
            var paths =
            (from regionCode in regionCodes
                select $@"Activities.ashx?Region={regionCode}").ToList();

            var urls = (from host in _hosts
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

        // http://iatiquery.um.dk/Activities.ashx?RecipientCountryCode=something&sector=something
        [Test]
        [Ignore]
        public void TestGetActivitiesCanGetForeachSector()
        {
            var random = new Random();

            const string template = @"http://{0}/v2/{1}";

            var entities = new IatiDbEntities();
            var sectors = entities.Sectors;
            var sectorCodes =
                sectors.Where(s => (s.category_code > 0) && (s.category_code < 999))
                    .Select(s => s.category_code)
                    .ToList();

            var countries = entities.Countries;
            var countryCodes =
                countries.Where(c => c.country_code_iati.Length == 2).Select(c => c.country_code_iati).ToList();

            // Uses random to pick a random country from the list of countries
            var countryCodeIndex = random.Next(countryCodes.Count);
            var countryCode = countryCodes[countryCodeIndex];

            var paths =
                (from sectorCode in sectorCodes
                        select
                        $@"Activities.ashx?RecipientConutryCode={countryCode}&Sector={sectorCode}")
                    .ToList();

            var urls = (from host in _hosts
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

        // http://iatiquery.um.dk/Activities.ashx?RecipientCountryCode='et-eller-andet'
    }
}