﻿using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a redirect url.
    /// </summary>
    public interface IRedirectUrl : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the identifier of the content item.
        /// </summary>
        [DataMember]
        int ContentId { get; set; }

        /// <summary>
        /// Gets or sets the unique key identifying the content item.
        /// </summary>
        [DataMember]
        Guid ContentKey { get; set; }

        /// <summary>
        /// Gets or sets the redirect url creation date.
        /// </summary>
        [DataMember]
        DateTime CreateDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the redirect url route.
        /// </summary>
        /// <remarks>Is a proper Umbraco route eg /path/to/foo or 123/path/tofoo.</remarks>
        [DataMember]
        string Url { get; set; }
    }
}
