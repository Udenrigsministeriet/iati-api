#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;

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
            var hosts = new List<string> {"iatiquery.um.dk", "iatiquery-test.um.dk"};

            var paths = new List<string>();
            paths.Add("$metadata");
            paths.Add("Aidtypes");
            paths.Add("Countries");
            //paths.Add("Currencies");
            //paths.Add("Finanslovs");
            //paths.Add("Organisations");
            //paths.Add("Sectors");
            //paths.Add("Channels");

            var urls = (from host in hosts from path in paths select string.Format(template, host, path)).ToList();

            foreach (
                var response in
                    urls.Select(url => (HttpWebRequest) WebRequest.Create(url))
                        .Select(request => (HttpWebResponse) request.GetResponse()))
            {
                Assert.That(response.StatusCode.Equals(HttpStatusCode.OK));
            }
        }
    }
}