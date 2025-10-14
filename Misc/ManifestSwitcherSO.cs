using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Utility
{
    [CreateAssetMenu(fileName = "Manifest Switcher", menuName = "RAXY/Manifest Switcher")]
    public class ManifestSwitcherSO : ScriptableObject
    {
        [SerializeField]
        private string ManifestPath = "Packages/manifest.json";

        [TitleGroup("Packages to Switch")]
        [TableList]
        public List<PackageEntry> packages = new();

        [HorizontalGroup("Packages to Switch/Button")]
        [Button]
        private void SwitchToLocal()
        {
            foreach (var pkg in packages)
            {
                SwitchManifest(pkg.packageKey, "file:" + pkg.localPath, refresh: false);
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        [HorizontalGroup("Packages to Switch/Button")]
        [Button]
        private void SwitchToRemote()
        {
            foreach (var pkg in packages)
            {
                SwitchManifest(pkg.packageKey, pkg.remoteVersion, refresh: false);
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Switch a single package in manifest
        /// </summary>
        private void SwitchManifest(string packageKey, string newValue, bool refresh = true)
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"Manifest not found at {ManifestPath}");
                return;
            }

            string json = File.ReadAllText(ManifestPath);
            var jObject = JObject.Parse(json);

            var dependencies = jObject["dependencies"] as JObject;
            if (dependencies == null)
            {
                Debug.LogError("Dependencies not found in manifest!");
                return;
            }

            if (dependencies.ContainsKey(packageKey))
            {
                dependencies[packageKey] = newValue;
                File.WriteAllText(ManifestPath, jObject.ToString());
                Debug.Log($"Switched {packageKey} to {newValue}");
#if UNITY_EDITOR
                if (refresh)
                    AssetDatabase.Refresh();
#endif
            }
            else
            {
                Debug.LogError($"Package {packageKey} not found in manifest!");
            }
        }

        [Button("Test Manifest Format")]
        private void TestManifest()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"Manifest not found at {ManifestPath}");
                return;
            }

            string json = File.ReadAllText(ManifestPath);

            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse manifest.json: {ex.Message}");
                return;
            }

            var dependencies = jObject["dependencies"] as JObject;
            if (dependencies == null)
            {
                Debug.LogWarning("No 'dependencies' field found in manifest!");
                return;
            }

            Debug.Log("Manifest dependencies:");
            foreach (var kvp in dependencies)
            {
                Debug.Log($"{kvp.Key} : {kvp.Value}");
            }
        }
    }

    [System.Serializable]
    public class PackageEntry
    {
        public string packageKey;
        [FolderPath]
        public string localPath;
        public string remoteVersion;       
    }
}
