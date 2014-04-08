//------------------------------------------------------------------------------
// <copyright file="WebDataService.svc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region

using System;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Um.DataServices.Web
{
    public class ODataService : EntityFrameworkDataService<IatiDbEntities>
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.AllRead);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }


        [WebGet]
        public string Activities(string recipientCountryCode, string sector)
        {
            //TODO Implement logging

            var formattedRecipientCountryCode = FormatInput(recipientCountryCode);
            var formattedSector = FormatInput(sector);

            // Validate input parameters and throw exception if not valid
            ValidateRecipientCountryCode(formattedRecipientCountryCode);
            ValidateSector(formattedSector);

            var entities = new IatiDbEntities();
            var dataset = entities.GetActivitiesXml(formattedRecipientCountryCode, formattedSector).ToList();
            var sb = new StringBuilder();
            foreach (var item in dataset)
            {
                sb.Append(item.XML_F52E2B61_18A1_11d1_B105_00805F49916B);
            }
            return sb.ToString();
        }

        public static string FormatInput(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var filtered = RemoveInvalidCharacters(input);
                return filtered.Trim().ToUpperInvariant();
            }
            return null;
        }

        public static string RemoveInvalidCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var filtered = Regex.Replace(text, "[^a-zA-Z]", string.Empty);

            return filtered;
        }

        public static void ValidateRecipientCountryCode(string recipientCountryCode)
        {
            if (string.IsNullOrEmpty(recipientCountryCode))
            {
                return;
            }
            //TODO Enable exceptions once correct country code format has been implemented
            const int length = 3;
            if (recipientCountryCode.Length != length)
            {
                var msg =
                    string.Format(
                        "Length of 'RecipientCountry' parameter is invalid. The length of the specified value was '{0}'. The value of 'RecipientCountry' must be exactly '{1}' char",
                        recipientCountryCode.Length, length);
                //throw new ArgumentException(msg);
            }

            // Lookup the specified country code in the Countries view. The specified country code must exist, 
            // for validation to succeed
            var entities = new IatiDbEntities();

            var match =
                entities.Countries.FirstOrDefault(
                    c => c.country_code.Equals(recipientCountryCode, StringComparison.InvariantCultureIgnoreCase));

            if (match == null)
            {
                var msg = string.Format("The specified RecipientCountryCode '{0}' is unknown", recipientCountryCode);
                //throw new ArgumentException(msg);
            }
        }

        public static void ValidateSector(string sector)
        {
            if (string.IsNullOrEmpty(sector))
            {
                return;
            }

            if (sector.Length != 2)
            {
                var msg =
                    string.Format(
                        "Length of 'sector' parameter is invalid. The length of the specified value was '{0}'. The value of 'sector' must be exactly '2' char",
                        sector.Length);
                throw new ArgumentException(msg);
            }

            throw new NotImplementedException();
        }
    }
}