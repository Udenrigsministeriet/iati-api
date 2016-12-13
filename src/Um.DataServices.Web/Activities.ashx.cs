#region

using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

#endregion

namespace Um.DataServices.Web
{
    public class Activities : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            //var page = new OutputCachedPage(new OutputCacheParameters
            //{
            //    // Reload the cache after 24 hours
            //    Duration = int.Parse(ConfigurationManager.AppSettings[Schema.ServerSideCacheLifetime]),
            //    Location = OutputCacheLocation.Server,
            //    VaryByParam = "sector;RecipientCountryCode;region"
            //});
            //page.ProcessRequest(HttpContext.Current);

            //TODO Implement logging

            var recipientCountryCode = context.Request.Params.Get(@"RecipientCountryCode");
            var sector = context.Request.Params.Get(@"sector");
            var region = context.Request.Params.Get(@"region");
            var project = context.Request.Params.Get(@"project");

            var activities = GetActivities(recipientCountryCode, region, sector,project);

            context.Response.BufferOutput = true;
            context.Response.ContentType = @"text/xml";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            // Ask the client to cache the result
            //context.Response.Cache.SetCacheability(HttpCacheability.Public);
            //context.Response.Cache.SetExpires(
            //    DateTime.Now.AddSeconds(int.Parse(ConfigurationManager.AppSettings[Schema.ClientSideCacheLifetime])));
            //context.Response.Cache.SetMaxAge(new TimeSpan(0, 0,
            //    int.Parse(ConfigurationManager.AppSettings[Schema.ClientSideCacheLifetime])));

            context.Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            context.Response.Write(activities);
        }

        public static string GetActivities(string recipientCountryCode, string region, string sector, string projektId)
        {
            var formattedRecipientCountryCode = FormatInputString(recipientCountryCode);
            var formattedSector = FormatInputInteger(sector);
            var formattedRegion = FormatInputInteger(region);
            var formattedProjektId = FormatInputInteger(projektId);

            // Validate input parameters and throw exception if not valid
            ValidateSector(formattedSector);
            ValidateRecipientCountryCode(formattedRecipientCountryCode);
            ValidateRegion(formattedRegion);
            //ValidateProjekt(formattedProjektId);

            var entities = new IatiDbEntities();
            var dataset = entities.GetActivitiesXml(formattedRecipientCountryCode, formattedRegion, formattedSector, formattedProjektId).ToList();
            var sb = new StringBuilder();
            foreach (var item in dataset)
            {
                sb.Append(item.TypedXML);
            }
            return sb.ToString();
        }

        public bool IsReusable => false;

        public static string FormatInputString(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var filtered = RemoveInvalidCharacters(input, @"[^a-zA-Z]");
                return filtered.Trim().ToUpperInvariant();
            }
            return null;
        }

        public static string FormatInputInteger(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var filtered = RemoveInvalidCharacters(input, @"[^0-9]");
                return filtered.Trim().ToUpperInvariant();
            }
            return null;
        }

        public static string RemoveInvalidCharacters(string text, string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var filtered = Regex.Replace(text, filter, string.Empty);

            return filtered;
        }

        public static void ValidateRecipientCountryCode(string recipientCountryCode)
        {
            if (string.IsNullOrEmpty(recipientCountryCode))
            {
                return;
            }
            //TODO Enable exceptions once correct country code format has been implemented
            const int length = 2;
            if (recipientCountryCode.Length != length)
            {
                var msg =
                    $"Length of 'RecipientCountry' parameter is invalid. The length of the specified value was '{recipientCountryCode.Length}'. The value of 'RecipientCountry' must be exactly '{length}' char";
                throw new ArgumentException(msg);
            }

            // Lookup the specified country code in the Countries view. The specified country code must exist, 
            // for validation to succeed
            var entities = new IatiDbEntities();

            var match =
                entities.Countries.FirstOrDefault(
                    c => c.country_code_iati.Equals(recipientCountryCode, StringComparison.InvariantCultureIgnoreCase));

            if (match == null)
            {
                var msg = $"The specified RecipientCountryCode '{recipientCountryCode}' is unknown";
                throw new ArgumentException(msg);
            }
        }

        public static void ValidateSector(string sector)
        {
            if (string.IsNullOrEmpty(sector))
            {
                return;
            }

            int parsedSector;
            if (!int.TryParse(sector, out parsedSector))
            {
                var msg =
                    string.Format(
                        "Value of 'Sector' parameter is invalid. It cannot be parsed as an integer. The value of 'Sector' must be between 0 and 999");
                throw new ArgumentException(msg);
            }

            // Lookup the specified sector in the Sectors view. The specified sector must exist, 
            // for validation to succeed
            var entities = new IatiDbEntities();

            var match =
                entities.Sectors.FirstOrDefault(
                    c => c.sector_code == parsedSector);

            if (match == null)
            {
                var msg = $"The specified Sector '{parsedSector}' is unknown";
                throw new ArgumentException(msg);
            }
        }

        public static void ValidateRegion(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                return;
            }

            int parsedRegion;
            if (!int.TryParse(region, out parsedRegion))
            {
                var msg =
                    string.Format(
                        "Value of 'Region' parameter is invalid. It cannot be parsed as an integer. The value of 'Region' must be between 0 and 999");
                throw new ArgumentException(msg);
            }

            // Lookup the specified region code in the Regions view. The specified region must exist, 
            // for validation to succeed
            var entities = new IatiDbEntities();

            var match =
                entities.Regions.FirstOrDefault(
                    c => c.code == parsedRegion);

            if (match == null)
            {
                var msg = $"The specified Region '{parsedRegion}' is unknown";
                throw new ArgumentException(msg);
            }
        }

        private sealed class OutputCachedPage : Page
        {
            private readonly OutputCacheParameters _cacheSettings;

            public OutputCachedPage(OutputCacheParameters cacheSettings)
            {
                ID = Guid.NewGuid().ToString();
                _cacheSettings = cacheSettings;
            }

            protected override void FrameworkInitialize()
            {
                base.FrameworkInitialize();
                InitOutputCache(_cacheSettings);
            }
        }
    }
}