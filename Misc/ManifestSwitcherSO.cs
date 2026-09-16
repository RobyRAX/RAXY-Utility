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
    public enum PackageManifestMode
    {
        Local,
        Remote,
        Missing
    }

    [CreateAssetMenu(fileName = "Manifest Switcher", menuName = "RAXY/Editor/Manifest Switcher")]
    public class ManifestSwitcherSO : ScriptableObject
    {
        [SerializeField]
        private string ManifestPath = "Packages/manifest.json";

        [ShowInInspector, ReadOnly]
        [InfoBox("WARNING: Manifest is in LOCAL mode. Do not commit manifest.json.", InfoMessageType.Warning, "@IsLocalMode")]
        [InfoBox("Manifest is in REMOTE mode. Safe to commit.", InfoMessageType.Info, "@!IsLocalMode")]
        private bool IsLocalMode => IsManifestInLocalMode();

        [TitleGroup("Packages to Switch")]
        [TableList]
        [OnCollectionChanged(After = nameof(BindPackageEntries))]
        public List<PackageEntry> packages = new();

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            BindPackageEntries();
        }

        private void OnValidate()
        {
            BindPackageEntries();
        }

        private void BindPackageEntries()
        {
            if (packages == null)
                return;

            foreach (var pkg in packages)
                pkg?.Bind(this);
        }

        public string GetManifestPath() => ManifestPath;

        public bool IsManifestInLocalMode()
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

        public bool TryGetDependency(string packageKey, out string value)
        {
            value = null;

            if (string.IsNullOrEmpty(packageKey) || !File.Exists(ManifestPath))
                return false;

            try
            {
                string json = File.ReadAllText(ManifestPath);
                var jObject = JObject.Parse(json);
                var dependencies = jObject["dependencies"] as JObject;
                if (dependencies == null || !dependencies.TryGetValue(packageKey, out var token) || token == null)
                    return false;

                value = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read dependency '{packageKey}': {ex.Message}");
                return false;
            }
        }

        public PackageManifestMode GetEntryMode(PackageEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.packageKey))
                return PackageManifestMode.Missing;

            if (!TryGetDependency(entry.packageKey, out string value) || string.IsNullOrEmpty(value))
                return PackageManifestMode.Missing;

            return value.StartsWith("file:", StringComparison.Ordinal)
                ? PackageManifestMode.Local
                : PackageManifestMode.Remote;
        }

        public bool SwitchEntryToLocal(PackageEntry entry, bool refresh = true)
        {
            if (entry == null)
                return false;

            entry.Bind(this);

            if (!TryBuildLocalDependencyValue(entry, out string newValue))
                return false;

            bool ok = SwitchManifest(entry.packageKey, newValue, refresh: false);
            if (ok && refresh)
            {
                AssetDatabase.Refresh();
                Debug.LogWarning($"WARNING: Switched {entry.packageKey} to LOCAL. Remember to switch back to Remote before committing.");
            }

            return ok;
        }

        public bool SwitchEntryToRemote(PackageEntry entry, bool refresh = true)
        {
            if (entry == null)
                return false;

            entry.Bind(this);

            if (string.IsNullOrEmpty(entry.remoteVersion))
            {
                Debug.LogWarning($"Remote version not configured for {entry.packageKey}.");
                return false;
            }

            bool ok = SwitchManifest(entry.packageKey, entry.remoteVersion, refresh: false);
            if (ok && refresh)
            {
                AssetDatabase.Refresh();
                Debug.Log($"Switched {entry.packageKey} to REMOTE.");
            }

            return ok;
        }

        [HorizontalGroup("Packages to Switch/Button")]
        [Button]
        private void SwitchToLocal()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"Manifest not found at {ManifestPath}");
                return;
            }

            BindPackageEntries();
            bool any = false;

            foreach (var pkg in packages)
            {
                if (SwitchEntryToLocal(pkg, refresh: false))
                    any = true;
            }

            if (!any)
                return;

            AssetDatabase.Refresh();
            Debug.LogWarning("WARNING: Switched to LOCAL mode. Remember to switch back to Remote before committing.");
        }

        [HorizontalGroup("Packages to Switch/Button")]
        [Button]
        private void SwitchToRemote()
        {
            BindPackageEntries();
            bool any = false;

            foreach (var pkg in packages)
            {
                if (SwitchEntryToRemote(pkg, refresh: false))
                    any = true;
            }

            if (!any)
                return;

            AssetDatabase.Refresh();
            Debug.Log("Switched to REMOTE mode. Safe to commit.");
        }

        private bool TryBuildLocalDependencyValue(PackageEntry entry, out string newValue)
        {
            newValue = null;

            string localPath = entry.GetLocalPath();
            if (string.IsNullOrEmpty(localPath))
            {
                Debug.LogWarning($"Local path not configured for {entry.packageKey}. Please set it in the inspector.");
                return false;
            }

            if (!Directory.Exists(localPath))
            {
                Debug.LogWarning($"Local package path does not exist: {localPath}");
                return false;
            }

            newValue = "file:" + localPath.Replace("\\", "/");
            return true;
        }

        /// <summary>
        /// Switch a single package in manifest
        /// </summary>
        private bool SwitchManifest(string packageKey, string newValue, bool refresh = true)
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"Manifest not found at {ManifestPath}");
                return false;
            }

            string json = File.ReadAllText(ManifestPath);
            var jObject = JObject.Parse(json);

            var dependencies = jObject["dependencies"] as JObject;
            if (dependencies == null)
            {
                Debug.LogError("Dependencies not found in manifest!");
                return false;
            }

            if (dependencies.ContainsKey(packageKey))
            {
                dependencies[packageKey] = newValue;
                File.WriteAllText(ManifestPath, jObject.ToString());
                Debug.Log($"Switched {packageKey} to {newValue}");
                if (refresh)
                    AssetDatabase.Refresh();
                return true;
            }

            Debug.LogError($"Package {packageKey} not found in manifest!");
            return false;
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
        [InfoBox("Export package list as JSON to move config between Unity projects.", InfoMessageType.Info)]
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

        [InfoBox("Import package list as JSON to move config between Unity projects.", InfoMessageType.Info)]
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

            BindPackageEntries();
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

    [Serializable]
    public class PackageEntry
    {
        [NonSerialized]
        private ManifestSwitcherSO _owner;

        [TableColumnWidth(180)]
        public string packageKey;

        [ShowInInspector, ReadOnly]
        [TableColumnWidth(80)]
        [GUIColor("@StatusColor")]
        private string Status => GetStatusLabel();

        private Color StatusColor
        {
            get
            {
                if (_owner == null)
                    return Color.gray;

                return _owner.GetEntryMode(this) switch
                {
                    PackageManifestMode.Local => new Color(1f, 0.75f, 0.35f),
                    PackageManifestMode.Remote => new Color(0.45f, 0.85f, 0.55f),
                    _ => new Color(1f, 0.45f, 0.45f)
                };
            }
        }

        [TableColumnWidth(200)]
        [FolderPath]
        [OnValueChanged("SaveLocalPath")]
        [Tooltip("Relative path like ../RAXY Animation or absolute path. Auto-saves as relative in EditorPrefs.")]
        public string localPath;

        [TableColumnWidth(200)]
        public string remoteVersion;

        [Button("Toggle")]
        [TableColumnWidth(70)]
        private void ToggleLocalRemote()
        {
            var owner = EnsureOwner();
            if (owner == null)
                return;

            if (owner.GetEntryMode(this) == PackageManifestMode.Local)
                owner.SwitchEntryToRemote(this);
            else
                owner.SwitchEntryToLocal(this);
        }

        private const string EDITORPREFS_PREFIX = "ManifestSwitcher_LocalPath_";

        public void Bind(ManifestSwitcherSO owner)
        {
            _owner = owner;
        }

        private ManifestSwitcherSO EnsureOwner()
        {
            if (_owner != null)
                return _owner;

            Debug.LogWarning($"Manifest Switcher owner not bound for {packageKey}. Re-select the asset.");
            return null;
        }

        private string GetStatusLabel()
        {
            if (_owner == null)
                return "-";

            return _owner.GetEntryMode(this) switch
            {
                PackageManifestMode.Local => "LOCAL",
                PackageManifestMode.Remote => "REMOTE",
                _ => "MISSING"
            };
        }

        public string GetLocalPath()
        {
            string relativePath = GetStoredRelativePath();
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            string projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        private string GetStoredRelativePath()
        {
            if (!string.IsNullOrEmpty(localPath))
                return localPath;

            return EditorPrefs.GetString(EDITORPREFS_PREFIX + packageKey, string.Empty);
        }

        public string GetRelativeLocalPathForExport() => GetStoredRelativePath();

        public void ImportLocalPath(string relativePath)
        {
            localPath = relativePath ?? string.Empty;

            if (!string.IsNullOrEmpty(packageKey) && !string.IsNullOrEmpty(localPath))
                EditorPrefs.SetString(EDITORPREFS_PREFIX + packageKey, localPath);
        }

        private void SaveLocalPath()
        {
            if (!string.IsNullOrEmpty(packageKey) && !string.IsNullOrEmpty(localPath))
            {
                string projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
                string relativePath = MakeRelativePathForStorage(localPath, projectRoot);

                EditorPrefs.SetString(EDITORPREFS_PREFIX + packageKey, relativePath);
                Debug.Log($"Saved local path for {packageKey}: {relativePath}");
            }
        }

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
