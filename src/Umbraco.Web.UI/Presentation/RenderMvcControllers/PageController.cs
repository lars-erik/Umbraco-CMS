using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.UI.Presentation.RenderMvcControllers
{
    public class PageController : RenderMvcController
    {
        public PageController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, ProfilingLogger profilingLogger) : base(globalSettings, umbracoContext, services, applicationCache, logger, profilingLogger)
        {
        }

        public override ActionResult Index(ContentModel model)
        {
            ViewData["message"] = "Hello from RenderMvcController";

            return base.Index(model);
        }
    }
}
