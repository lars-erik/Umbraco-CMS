using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.UI.Presentation.SurfaceControllers
{
    public class ToggleMeAroundSurfaceController : SurfaceController
    {
        private readonly IPublishedSnapshotService snapshotService;

        public ToggleMeAroundSurfaceController(IPublishedSnapshotService snapshotService, UmbracoContext umbracoContext, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, CacheHelper applicationCache, ILogger logger, ProfilingLogger profilingLogger) : base(umbracoContext, databaseFactory, services, applicationCache, logger, profilingLogger)
        {
            this.snapshotService = snapshotService;
        }

        public ActionResult AsList(IPublishedProperty property)
        {
            return View(property);
        }
    }
}
