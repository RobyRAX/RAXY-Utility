using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RAXY.Utility.Editor.Hub
{
    public static class RaxyHubModuleDiscovery
    {
        public static List<IRaxyHubModule> CreateModules()
        {
            var modules = new List<IRaxyHubModule>();

            foreach (var type in TypeCache.GetTypesDerivedFrom<IRaxyHubModule>())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[RAXY Hub] Module '{type.FullName}' has no parameterless constructor; skipped.");
                    continue;
                }

                try
                {
                    modules.Add((IRaxyHubModule)Activator.CreateInstance(type));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(
                        $"[RAXY Hub] Failed to create module '{type.FullName}': {e.Message}");
                }
            }

            return modules.OrderBy(m => m.Order).ThenBy(m => m.DisplayName).ToList();
        }
    }
}
