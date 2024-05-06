#if FEATURE_SERVICES_INSTANCES
using System;

namespace Unity.Services.Core
{
    /// <summary>
    /// Builder contract for a services instance.
    /// </summary>
    public interface IUnityServicesBuilder
    {
        /// <summary>
        /// The identifier of the instance
        /// </summary>
        string InstanceId { get; }

        /// <summary>
        /// Provide the instance identifier. Defaults to a guid.
        /// </summary>
        /// <param name="instanceId">The identifier of the instance</param>
        /// <returns>The instance builder</returns>
        IUnityServicesBuilder WithInstanceId(string instanceId);

        /// <summary>
        /// Build the services instance.
        /// </summary>
        /// <returns>The services instance.</returns>
        /// <exception cref="ServicesCreationException">
        /// Occurs when the services instance could not be created.
        /// </exception>
        IUnityServices Build();
    }

    class UnityServicesBuilder : IUnityServicesBuilder
    {
        internal delegate IUnityServices CreationDelegate(UnityServicesBuilder builder);
        internal static CreationDelegate InstanceCreationDelegate { get; set; }

        public string InstanceId { get; private set; }

        internal UnityServicesBuilder()
        {
            InstanceId = Guid.NewGuid().ToString();
        }

        public IUnityServicesBuilder WithInstanceId(string instanceId)
        {
            InstanceId = instanceId;
            return this;
        }

        public IUnityServices Build()
        {
            var instance = InstanceCreationDelegate(this);

            if (string.IsNullOrEmpty(InstanceId))
            {
                throw new ServicesCreationException($"The services instance id cannot be null or empty.");
            }

            if (UnityServices.Instances.ContainsKey(InstanceId))
            {
                throw new ServicesCreationException($"The services instance id '{InstanceId}' is already registered.");
            }

            UnityServices.Instances[InstanceId] = instance;
            return instance;
        }
    }
}
#endif
