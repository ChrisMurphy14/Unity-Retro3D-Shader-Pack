#if UNITY_EDITOR
#if FEATURE_SERVICES_INSTANCES
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using Debug = UnityEngine.Debug;

namespace Unity.Services.Core.Internal
{
    static class UnityServicesEditorInitializer
    {
        [InitializeOnLoadMethod]
        static void CreateStaticInstance()
        {
            UnityServicesBuilder.InstanceCreationDelegate = CreateInstance;

            var corePackageRegistry = new CorePackageRegistry();
            var coreRegistry = new CoreRegistry(corePackageRegistry.Registry);

            CorePackageRegistry.Instance = corePackageRegistry;
            CoreRegistry.Instance = coreRegistry;

            var coreDiagnostics = new CoreDiagnostics();
            CoreDiagnostics.Instance = coreDiagnostics;

            RegisterInitializers(corePackageRegistry);
        }

        static void RegisterInitializers(CorePackageRegistry registry)
        {
            try
            {
                var initializers = GetInitializersV2();

                foreach (var initializer in initializers)
                {
                    try
                    {
                        initializer.Register(registry);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        static IEnumerable<IInitializablePackageV2> GetInitializersV2()
        {
            var packages = TypeCache.GetTypesDerivedFrom<IInitializablePackageV2>();

            return packages.Where(type => !type.IsAbstract && typeof(IInitializablePackageV2).IsAssignableFrom(type))
                .Select(type => (IInitializablePackageV2)Activator.CreateInstance(type));
        }

        internal static IUnityServices CreateInstance(UnityServicesBuilder builder)
        {
            var registry = new CoreRegistry(CorePackageRegistry.Instance.Registry, ServicesType.Instance, builder.InstanceId);
            var instance = new UnityServicesInternal(registry, CoreMetrics.Instance, CoreDiagnostics.Instance);
            instance.EnableInitialization();
            return instance;
        }
    }
}
#endif
#endif
