//------------------------------------------------------------------------------
// <copyright file="WebDataService.svc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

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
        public string Activities(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException("countryCode");
            }
            var entities = new IatiDbEntities();
            var dataset = entities.GetActivitiesXml(countryCode).ToList();
            var sb = new StringBuilder();
            foreach (var item in dataset)
            {
                sb.Append(item.XML_F52E2B61_18A1_11d1_B105_00805F49916B);
            }
            return sb.ToString();
        }
    }
}