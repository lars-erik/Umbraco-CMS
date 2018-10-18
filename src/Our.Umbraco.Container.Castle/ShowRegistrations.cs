using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.Container.Castle
{
    public class ShowRegistrationsController : UmbracoAuthorizedApiController
    {
        private readonly IContainer container;

        public ShowRegistrationsController()
        {
            container = Current.Container;
        }

        public ShowRegistrationsController(IContainer container, IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, ProfilingLogger profilingLogger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContext, sqlContext, services, applicationCache, logger, profilingLogger, runtimeState)
        {
            this.container = container;
        }

        [HttpGet]
        public HttpResponseMessage Registrations()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<table>" +
                    container
                        .GetRegistered()
                        .OrderBy(x =>
                            x.Lifetime == Lifetime.Singleton ? 0 :
                            x.Lifetime == Lifetime.Scope ? 1 :
                            x.Lifetime == Lifetime.Request ? 2 :
                            3
                        )
                        .ThenByDescending(x => x.Dependencies?.Length)
                        .Aggregate("", (s, x) => s +
                                                 "<tr><td>" +
                                                 x.ServiceType +
                                                 "</td><td>" +
                                                 x.ImplementingType +
                                                 "</td><td>" +
                                                 x.Lifetime +
                                                 "</td><td>" +
                                                 x.ServiceName +
                                                 "</td></tr>" +
                                                 "<tr><td colspan=\"4\"><pre>" +
                                                 String.Join(", ", x.Dependencies?.Select(y => y.Name) ?? new string[0]) +
                                                 "</pre></td></tr>"
                                                 ) +
                     "</table>"
                    , Encoding.UTF8
                    , "text/html"
                )
            };
        }
    }
}
