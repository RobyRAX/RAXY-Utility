#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

using UnityEditor;

namespace RAXY.Utility
{
    [CreateAssetMenu(fileName = "Manifest Switcher", menuName = "RAXY/Editor/Manifest Switcher")]
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
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"Manifest not found at {ManifestPath}");
                return;
            }

            foreach (var pkg in packages)
            {
                if (!Directory.Exists(pkg.absolutePath))
                {
                    Debug.LogWarning($"Local package path does not exist: {pkg.absolutePath}");
                    continue;
                }

                string newValue = "file:" + pkg.absolutePath.Replace("\\", "/");
                SwitchManifest(pkg.packageKey, newValue, refresh: false);
            }

            AssetDatabase.Refresh();
        }

        [HorizontalGroup("Packages to Switch/Button")]
        [Button]
        private void SwitchToRemote()
        {
            foreach (var pkg in packages)
            {
                SwitchManifest(pkg.packageKey, pkg.remoteVersion, refresh: false);
            }
            AssetDatabase.Refresh();
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
                if (refresh)
                    AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Package {packageKey} not found in manifest!");
            }
        }

        [TitleGroup("Test")]
        [Button("Test Switch to Local")]
        private void Test_SwitchToLocal()
        {
            if (packages == null || packages.Count == 0)
            {
                Debug.LogWarning("No packages defined to switch.");
                return;
            }

            Debug.Log("=== Test Switch to Local ===");
            foreach (var pkg in packages)
            {
                string newValue = "file:" + pkg.absolutePath.Replace("\\", "/");
                bool exists = Directory.Exists(pkg.absolutePath);
                Debug.Log($"{pkg.packageKey} -> {newValue} (Exists: {exists})");
            }
        }

        [TitleGroup("Test")]
        [Button("Test Switch to Remote")]
        private void Test_SwitchToRemote()
        {
            if (packages == null || packages.Count == 0)
            {
                Debug.LogWarning("No packages defined to switch.");
                return;
            }

            Debug.Log("=== Test Switch to Remote ===");
            foreach (var pkg in packages)
            {
                string newValue = pkg.remoteVersion;
                bool valid = !string.IsNullOrEmpty(newValue);
                Debug.Log($"{pkg.packageKey} -> {newValue} (Valid: {valid})");
            }
        }
    }

    [System.Serializable]
    public class PackageEntry
    {
        public string packageKey;
        public string absolutePath;
        public string remoteVersion;       
    }
}
#endif