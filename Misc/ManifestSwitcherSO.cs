#if UNITY_EDITOR

using System;
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

        [ShowInInspector, ReadOnly]
        [InfoBox("WARNING: Manifest is in LOCAL mode. Do not commit manifest.json.", InfoMessageType.Warning, "@IsLocalMode")]
        [InfoBox("Manifest is in REMOTE mode. Safe to commit.", InfoMessageType.Info, "@!IsLocalMode")]
        private bool IsLocalMode => CheckIfLocalMode();

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
                string localPath = pkg.GetLocalPath();
                if (string.IsNullOrEmpty(localPath))
                {
                    Debug.LogWarning($"Local path not configured for {pkg.packageKey}. Please set it in the inspector.");
                    continue;
                }

                if (!Directory.Exists(localPath))
                {
                    Debug.LogWarning($"Local package path does not exist: {localPath}");
                    continue;
                }

                string newValue = "file:" + localPath.Replace("\\", "/");
                SwitchManifest(pkg.packageKey, newValue, refresh: false);
            }

            AssetDatabase.Refresh();
            Debug.LogWarning("WARNING: Switched to LOCAL mode. Remember to switch back to Remote before committing.");
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
            Debug.Log("Switched to REMOTE mode. Safe to commit.");
        }

        /// <summary>
        /// Check if manifest is currently in local mode
        /// </summary>
        private bool CheckIfLocalMode()
        {
            if (!File.Exists(ManifestPath))
                return false;

            try
            {
                string json = File.ReadAllText(ManifestPath);
                return json.Contains("\"file:");
            }
            catch
            {
                return false;
            }
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
                string localPath = pkg.GetLocalPath();
                if (string.IsNullOrEmpty(localPath))
                {
                    Debug.LogWarning($"{pkg.packageKey} -> NO LOCAL PATH CONFIGURED");
                    continue;
                }
                string newValue = "file:" + localPath.Replace("\\", "/");
                bool exists = Directory.Exists(localPath);
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

        [TitleGroup("Git Protection")]
        [InfoBox("Install git hook to block commits when manifest is in local mode", InfoMessageType.Info)]
        [HorizontalGroup("Git Protection/Buttons")]
        [Button("Install Git Hook", ButtonSizes.Medium)]
        private void InstallGitHook()
        {
            GitHookInstaller.InstallPreCommitHook();
        }

        [HorizontalGroup("Git Protection/Buttons")]
        [Button("Test Hook", ButtonSizes.Medium)]
        private void TestGitHook()
        {
            GitHookInstaller.TestPreCommitHook();
        }
    }

    [System.Serializable]
    public class PackageEntry
    {
        [TableColumnWidth(200)]
        public string packageKey;

        [TableColumnWidth(200)]
        [FolderPath]
        [OnValueChanged("SaveLocalPath")]
        [Tooltip("Relative path like ../RAXY Animation or absolute path. Auto-saves as relative in EditorPrefs.")]
        public string localPath;

        [TableColumnWidth(200)]
        public string remoteVersion;

        private const string EDITORPREFS_PREFIX = "ManifestSwitcher_LocalPath_";

        /// <summary>
        /// Get local path (resolves relative to absolute)
        /// </summary>
        public string GetLocalPath()
        {
            string relativePath = GetStoredRelativePath();
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // Resolve relative path to absolute
            string projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        /// <summary>
        /// Get the relative path stored in EditorPrefs
        /// </summary>
        private string GetStoredRelativePath()
        {
            if (!string.IsNullOrEmpty(localPath))
                return localPath;

            return EditorPrefs.GetString(EDITORPREFS_PREFIX + packageKey, string.Empty);
        }

        /// <summary>
        /// Save local path to EditorPrefs as relative path (per-machine, not in git)
        /// </summary>
        private void SaveLocalPath()
        {
            if (!string.IsNullOrEmpty(packageKey) && !string.IsNullOrEmpty(localPath))
            {
                // Convert absolute path to relative before saving
                string projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
                string relativePath = MakeRelativePathForStorage(localPath, projectRoot);
                
                EditorPrefs.SetString(EDITORPREFS_PREFIX + packageKey, relativePath);
                Debug.Log($"Saved local path for {packageKey}: {relativePath}");
            }
        }

        /// <summary>
        /// Convert absolute path to relative path for storage
        /// </summary>
        private string MakeRelativePathForStorage(string fullPath, string basePath)
        {
            try
            {
                Uri fullUri = new System.Uri(fullPath);
                Uri baseUri = new System.Uri(basePath + Path.DirectorySeparatorChar);
                string relativePath = baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar);
                return relativePath;
            }
            catch
            {
                return fullPath;
            }
        }
    }
}
#endif