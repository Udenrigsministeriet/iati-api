#region

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

#endregion

namespace Um.DataServices.Web
{
    public class Activities : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            //TODO Implement logging

            var recipientCountryCode = context.Request.Params.Get(@"RecipientCountryCode");
            var sector = context.Request.Params.Get(@"sector");

            var formattedRecipientCountryCode = FormatInputString(recipientCountryCode);
            var formattedSector = FormatInputInteger(sector);

            // Validate input parameters and throw exception if not valid
            ValidateSector(formattedSector);
            ValidateRecipientCountryCode(formattedRecipientCountryCode);

            var entities = new IatiDbEntities();
            var dataset = entities.GetActivitiesXml(formattedRecipientCountryCode, formattedSector).ToList();
            var sb = new StringBuilder();
            foreach (var item in dataset)
            {
                sb.Append(item.XML_F52E2B61_18A1_11d1_B105_00805F49916B);
            }

            context.Response.ContentType = @"text/xml";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            context.Response.Write(sb.ToString());
        }

        public bool IsReusable
        {
            get { return false; }
        }

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
                throw new ArgumentNullException(@"filter");
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
                    string.Format(
                        "Length of 'RecipientCountry' parameter is invalid. The length of the specified value was '{0}'. The value of 'RecipientCountry' must be exactly '{1}' char",
                        recipientCountryCode.Length, length);
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
                var msg = string.Format("The specified RecipientCountryCode '{0}' is unknown", recipientCountryCode);
                throw new ArgumentException(msg);
            }
        }

        public static void ValidateSector(string sector)
        {
            if (string.IsNullOrEmpty(sector))
            {
                return;
            }
            const int length = 3;
            if (sector.Length != length)
            {
                var msg =
                    string.Format(
                        "Length of 'Sector' parameter is invalid. The length of the specified value was '{0}'. The value of 'Sector' must be exactly '{1}' char",
                        sector.Length, length);
                throw new ArgumentException(msg);
            }
            int parsedSector;
            if (!int.TryParse(sector, out parsedSector))
            {
                var msg =
                    string.Format(
                        "Value of 'Sector' parameter is invalid. It cannot be parsed as an integer. The value of 'Sector' must be between 0 and 999");
                throw new ArgumentException(msg);
            }
            if (parsedSector < 0 || parsedSector > 999)
            {
                var msg =
                    string.Format(
                        "Value of 'sector' parameter is invalid. The value of 'sector' must be between 000 and 999",
                        sector);
                throw new ArgumentException(msg);
            }

            // Lookup the specified country code in the Countries view. The specified sector must exist, 
            // for validation to succeed
            var entities = new IatiDbEntities();

            var match =
                entities.Sectors.FirstOrDefault(
                    c => c.category_code == parsedSector);

            if (match == null)
            {
                var msg = string.Format("The specified Sector '{0}' is unknown", sector);
                throw new ArgumentException(msg);
            }
        }
    }
}