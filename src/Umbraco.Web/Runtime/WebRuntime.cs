﻿using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.Runtime
{
    /// <summary>
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related runtime aspects of Umbraco.</remarks>
    public class WebRuntime : CoreRuntime
    {
        private IProfiler _webProfiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(UmbracoApplicationBase umbracoApplication)
            : base(umbracoApplication)
        { }

        /// <inheritdoc/>
        public override void Boot(IContainer container)
        {
            // create and start asap to profile boot
            var debug = GlobalSettings.DebugMode;
            if (debug)
            {
                _webProfiler = new WebProfiler();
                _webProfiler.Start();
            }
            else
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                _webProfiler = new VoidProfiler();
            }

            base.Boot(container);

            // now (and only now) is the time to switch over to perWebRequest scopes
            container.EnablePerWebRequestScope();
        }

        /// <inheritdoc/>
        public override void Compose(IContainer container)
        {
            base.Compose(container);

            container.Register<UmbracoInjectedModule>();

            // replace CoreRuntime's IProfiler registration
            container.RegisterSingleton(_ => _webProfiler);

            // replace CoreRuntime's CacheHelper registration
            container.RegisterSingleton(
                "WebCacheHelper",
                _ => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache)),
                new StaticCacheProvider(),
                // we need request based cache when running in web-based context
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()))));

            container.RegisterSingleton<IHttpContextAccessor, AspNetHttpContextAccessor>(); // required for hybrid accessors
        }

        #region Getters

        //protected override IProfiler GetProfiler() => new WebProfiler();

        //protected override CacheHelper GetApplicationCache() => new CacheHelper(
        //        // we need to have the dep clone runtime cache provider to ensure
        //        // all entities are cached properly (cloned in and cloned out)
        //        new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache)),
        //        new StaticCacheProvider(),
        //        // we need request based cache when running in web-based context
        //        new HttpRequestCacheProvider(),
        //        new IsolatedRuntimeCache(type =>
        //            // we need to have the dep clone runtime cache provider to ensure
        //            // all entities are cached properly (cloned in and cloned out)
        //            new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));

        #endregion
    }
}

