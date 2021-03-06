﻿using Umbraco.Core;
using Umbraco.Core.Components;

namespace Umbraco.Web.Redirects
{
    /// <summary>
    /// Implements an Application Event Handler for managing redirect urls tracking.
    /// </summary>
    /// <remarks>
    /// <para>when content is renamed or moved, we want to create a permanent 301 redirect from it's old url</para>
    /// <para>not managing domains because we don't know how to do it - changing domains => must create a higher level strategy using rewriting rules probably</para>
    /// <para>recycle bin = moving to and from does nothing: to = the node is gone, where would we redirect? from = same</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    [Disable] // fixme - re-enable when we fix redirect tracking with variants
    public class RedirectTrackingComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<RedirectTrackingComponent>();
        }
    }
}
