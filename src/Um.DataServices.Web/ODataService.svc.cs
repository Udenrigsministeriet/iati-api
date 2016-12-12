//------------------------------------------------------------------------------
// <copyright file="WebDataService.svc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region

using System.Configuration;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

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

        //protected override void OnStartProcessingRequest(ProcessRequestArgs args)
        //{
        //    base.OnStartProcessingRequest(args);
        //    var c = HttpContext.Current.Response.Cache;
        //    c.SetCacheability(HttpCacheability.ServerAndPrivate);
        //    var lifetime = int.Parse(ConfigurationManager.AppSettings[Schema.ServerSideCacheLifetime]);
        //    c.SetExpires(HttpContext.Current.Timestamp.AddSeconds(lifetime));
        //    c.VaryByHeaders["Accept"] = true;
        //    c.VaryByHeaders["Accept-Charset"] = true;
        //    c.VaryByHeaders["Accept-Encoding"] = true;
        //    c.VaryByParams["*"] = true;
        //}
    }
}