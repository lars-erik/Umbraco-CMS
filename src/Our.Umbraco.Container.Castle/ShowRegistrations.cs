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
        private Dictionary<Lifetime, int> lifetimeValues = new Dictionary<Lifetime, int>
        {
            { Lifetime.Singleton, 0 },
            { Lifetime.Scope, 1 },
            { Lifetime.Request, 2 },
            { Lifetime.Transient, 3 }
        };

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
                    "<tr><th>Service type</th><th>Implementing type</th><th>Lifetime</th><th>Service name</th></tr>" +
                    container
                        .GetRegistered()
                        .OrderBy(x => lifetimeValues[x.Lifetime])
                        .ThenBy(x => x.Dependencies?.Length)
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
                                                 "<tr><td colspan=\"4\">" +
                                                 DependencyTable(x) +
                                                 "</td></tr>"
                                                 ) +
                     "</table>"
                    , Encoding.UTF8
                    , "text/html"
                )
            };
        }

        private string DependencyTable(Registration registration)
        {
            if (!registration.Dependencies.Any())
            {
                return "";
            }

            return "<table style=\"margin-left: 50px\">" +
                   "<tr><th>Service</th><th>First impl. lifecycle</th><th>First impl. type</th></tr>" +
                   registration.Dependencies.Aggregate("", (s, t) =>
                   {
                       var dependency = container.GetRegistered(t).FirstOrDefault()
                           ?? new Registration(t, "Not registered", null) { Lifetime = Lifetime.Transient, Dependencies = new Type[0] };

                       string bg = "transparent";
                       if (lifetimeValues[dependency.Lifetime] > lifetimeValues[registration.Lifetime])
                       {
                           bg = "orange";
                       }

                       return s +
                              "<tr style=\"background-color: " +
                              bg +
                              "\"><td>" +
                              t.Name +
                              "</td><td>" +
                              dependency.Lifetime +
                              "</td><td>" +
                              dependency.ImplementingType +
                              "</td></tr>";
                   }) +
                   "</table>";
        }
    }
}
