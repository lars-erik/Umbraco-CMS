using System;
using System.Globalization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a registered server in a multiple-servers environment.
    /// </summary>
    public class ServerRegistration : EntityBase, IServerRegistration
    {
        private string _serverAddress;
        private string _serverIdentity;
        private bool _isActive;
        private bool isPrimary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        public ServerRegistration()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        /// <param name="id">The unique id of the server registration.</param>
        /// <param name="serverAddress">The server URL.</param>
        /// <param name="serverIdentity">The unique server identity.</param>
        /// <param name="registered">The date and time the registration was created.</param>
        /// <param name="accessed">The date and time the registration was last accessed.</param>
        /// <param name="isActive">A value indicating whether the registration is active.</param>
        /// <param name="isPrimary">A value indicating whether the registration is primary server.</param>
        public ServerRegistration(int id, string serverAddress, string serverIdentity, DateTime registered, DateTime accessed, bool isActive, bool isPrimary)
        {
            UpdateDate = accessed;
            CreateDate = registered;
            Key = id.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            Id = id;
            ServerAddress = serverAddress;
            ServerIdentity = serverIdentity;
            IsActive = isActive;
            IsPrimary = isPrimary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistration"/> class.
        /// </summary>
        /// <param name="serverAddress">The server URL.</param>
        /// <param name="serverIdentity">The unique server identity.</param>
        /// <param name="registered">The date and time the registration was created.</param>
        public ServerRegistration(string serverAddress, string serverIdentity, DateTime registered)
        {
            CreateDate = registered;
            UpdateDate = registered;
            Key = 0.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
            ServerAddress = serverAddress;
            ServerIdentity = serverIdentity;
        }

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        public string ServerAddress
        {
            get => _serverAddress;
            set => SetPropertyValueAndDetectChanges(value, ref _serverAddress, nameof(ServerAddress));
        }

        /// <summary>
        /// Gets or sets the server unique identity.
        /// </summary>
        public string ServerIdentity
        {
            get => _serverIdentity;
            set => SetPropertyValueAndDetectChanges(value, ref _serverIdentity, nameof(ServerIdentity));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetPropertyValueAndDetectChanges(value, ref _isActive, nameof(IsActive));
        }

        [Obsolete("Replaced with IsPrimary. Will be deleted in a future version.")]
        public bool IsMaster
        {
            get => IsPrimary;
            set => IsPrimary = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is master.
        /// </summary>
        public bool IsPrimary
        {
            get => isPrimary;
#warning: This is a breaking change if somebody watches the change of "IsMaster".
            set => SetPropertyValueAndDetectChanges(value, ref isPrimary, nameof(IsPrimary));
        }

        /// <summary>
        /// Gets the date and time the registration was created.
        /// </summary>
        public DateTime Registered
        {
            get => CreateDate;
            set => CreateDate = value;
        }

        /// <summary>
        /// Gets the date and time the registration was last accessed.
        /// </summary>
        public DateTime Accessed
        {
            get => UpdateDate;
            set => UpdateDate = value;
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{\"{0}\", \"{1}\", {2}active, {3}master}}", ServerAddress, ServerIdentity, IsActive ? "" : "!", IsPrimary ? "" : "!");
        }
    }
}
