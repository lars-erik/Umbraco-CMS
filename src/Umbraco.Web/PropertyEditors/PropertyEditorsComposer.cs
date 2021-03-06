﻿using Umbraco.Core;
using Umbraco.Core.Components;

namespace Umbraco.Web.PropertyEditors
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    internal class PropertyEditorsComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<PropertyEditorsComponent>();
        }
    }
}
