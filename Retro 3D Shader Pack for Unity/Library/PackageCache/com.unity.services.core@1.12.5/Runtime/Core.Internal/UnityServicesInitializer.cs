using UnityEngine;

namespace Unity.Services.Core.Internal
{
    static class UnityServicesInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void CreateStaticInstance()
        {
#if FEATURE_SERVICES_INSTANCES
            UnityServicesBuilder.InstanceCreationDelegate = CreateInstance;
#endif

            var corePackageRegistry = new CorePackageRegistry();
            var coreRegistry = new CoreRegistry(corePackageRegistry.Registry);

            CorePackageRegistry.Instance = corePackageRegistry;
            CoreRegistry.Instance = coreRegistry;
            var coreMetrics = new CoreMetrics();
            var coreDiagnostics = new CoreDiagnostics();

            UnityServices.Instance = new UnityServicesInternal(coreRegistry, coreMetrics, coreDiagnostics);
            UnityServices.InstantiationCompletion?.TrySetResult(null);
            CoreMetrics.Instance = coreMetrics;
            CoreDiagnostics.Instance = coreDiagnostics;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static async void EnableServicesInitializationAsync()
        {
            var instance = (UnityServicesInternal)UnityServices.Instance;
            await instance.EnableInitializationAsync();
        }

#if FEATURE_SERVICES_INSTANCES
        internal static IUnityServices CreateInstance(UnityServicesBuilder builder)
        {
            var registry = new CoreRegistry(CorePackageRegistry.Instance.Registry, ServicesType.Instance, builder.InstanceId);
            var instance = new UnityServicesInternal(registry, CoreMetrics.Instance, CoreDiagnostics.Instance);
            instance.EnableInitialization();
            return instance;
        }
#endif
    }
}
