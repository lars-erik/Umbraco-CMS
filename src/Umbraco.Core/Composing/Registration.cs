﻿using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a service registration in the container.
    /// </summary>
    public class Registration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Registration"/> class.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceName"></param>
        /// <param name="implementingType"></param>
        public Registration(Type serviceType, string serviceName, Type implementingType)
        {
            ServiceType = serviceType;
            ServiceName = serviceName;
            ImplementingType = implementingType;
        }

        /// <summary>
        /// Gets the service type.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName { get; }

        public Lifetime Lifetime { get; set; }

        /// <summary>
        /// Gets the concrete type implementing the service.
        /// </summary>
        public Type ImplementingType { get; }

        public Type[] Dependencies { get; set; }
    }
}
