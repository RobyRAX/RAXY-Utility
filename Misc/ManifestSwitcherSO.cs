#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        [HorizontalGroup("Test/Test")]
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

        [HorizontalGroup("Test/Test")]
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
        [InfoBox("Test git hook to check if manifest contains local paths", InfoMessageType.Info)]
        [Button("Test Hook", ButtonSizes.Medium)]
        private void TestGitHook()
        {
            GitHookInstaller.TestPreCommitHook();
        }

        [TitleGroup("Transfer")]
        [InfoBox("Export/import package list as JSON to move config between Unity projects.", InfoMessageType.Info)]
        [HorizontalGroup("Transfer/Buttons")]
        [Button("Export to JSON", ButtonSizes.Medium)]
        private void ExportToJson()
        {
            var data = BuildExportData();
            string defaultName = $"{name}-manifest-switcher.json";
            string path = EditorUtility.SaveFilePanel("Export Manifest Switcher", "", defaultName, "json");
            if (string.IsNullOrEmpty(path))
                return;

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"Exported {data.packages.Count} package(s) to {path}");
        }

        [HorizontalGroup("Transfer/Buttons")]
        [Button("Import from JSON", ButtonSizes.Medium)]
        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Import Manifest Switcher", "", "json");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                var data = JsonConvert.DeserializeObject<ManifestSwitcherExportData>(File.ReadAllText(path));
                if (data == null)
                {
                    Debug.LogError("Import failed: JSON is empty or invalid.");
                    return;
                }

                ApplyImportData(data);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                Debug.Log($"Imported {packages.Count} package(s) from {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Import failed: {ex.Message}");
            }
        }

        private ManifestSwitcherExportData BuildExportData()
        {
            var data = new ManifestSwitcherExportData
            {
                manifestPath = ManifestPath,
                packages = new List<PackageEntryExport>()
            };

            foreach (var pkg in packages)
            {
                data.packages.Add(new PackageEntryExport
                {
                    packageKey = pkg.packageKey,
                    localPath = pkg.GetRelativeLocalPathForExport(),
                    remoteVersion = pkg.remoteVersion
                });
            }

            return data;
        }

        private void ApplyImportData(ManifestSwitcherExportData data)
        {
            if (!string.IsNullOrEmpty(data.manifestPath))
                ManifestPath = data.manifestPath;

            packages ??= new List<PackageEntry>();
            packages.Clear();

            if (data.packages == null)
                return;

            foreach (var entry in data.packages)
            {
                var pkg = new PackageEntry
                {
                    packageKey = entry.packageKey,
                    remoteVersion = entry.remoteVersion
                };
                pkg.ImportLocalPath(entry.localPath);
                packages.Add(pkg);
            }
        }
    }

    [Serializable]
    public class ManifestSwitcherExportData
    {
        public string manifestPath = "Packages/manifest.json";
        public List<PackageEntryExport> packages = new();
    }

    [Serializable]
    public class PackageEntryExport
    {
        public string packageKey;
        public string localPath;
        public string remoteVersion;
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
        /// Relative local path for JSON export (asset field or EditorPrefs fallback).
        /// </summary>
        public string GetRelativeLocalPathForExport() => GetStoredRelativePath();

        /// <summary>
        /// Apply imported relative path to asset field and EditorPrefs.
        /// </summary>
        public void ImportLocalPath(string relativePath)
        {
            localPath = relativePath ?? string.Empty;

            if (!string.IsNullOrEmpty(packageKey) && !string.IsNullOrEmpty(localPath))
                EditorPrefs.SetString(EDITORPREFS_PREFIX + packageKey, localPath);
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