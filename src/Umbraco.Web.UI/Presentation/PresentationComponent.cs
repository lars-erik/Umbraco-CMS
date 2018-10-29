using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Presentation.RenderMvcControllers;
using Umbraco.Web.UI.Presentation.SurfaceControllers;

namespace Umbraco.Web.UI.Presentation
{
    public class PresentationComponent : UmbracoComponentBase, IUmbracoUserComponent
    {
        public override void Compose(Composition composition)
        {
            composition.Container.Register<ToggleMeAroundSurfaceController>();
            composition.Container.Register<PageController>();
        }
    }
}
