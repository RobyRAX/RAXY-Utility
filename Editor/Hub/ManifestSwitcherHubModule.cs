using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RAXY.Utility.Editor.Hub
{
    public sealed class ManifestSwitcherHubModule : IRaxyHubModule
    {
        struct SwitcherEntry
        {
            public ManifestSwitcherSO Asset;
            public string AssetPath;
            public string FolderPath;
            public string DisplayName;
        }

        readonly List<SwitcherEntry> _entries = new();
        Vector2 _scroll;
        string _filter = "";

        public string Id => "manifest-switcher";
        public string DisplayName => "Manifest Switcher";
        public int Order => 10;

        public void OnEnable()
        {
            RefreshList();
        }

        public void OnDisable()
        {
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
                DrawManifestBanner();
                EditorGUILayout.Space(10f);
                DrawToolbar();
                EditorGUILayout.Space(6f);
                DrawList();
                EditorGUILayout.EndScrollView();
            }
        }

        void DrawManifestBanner()
        {
            bool isLocal = IsProjectManifestLocal();
            if (isLocal)
            {
                RaxyHubGui.DrawStatusBanner(
                    false,
                    "Manifest is in LOCAL mode",
                    "One or more packages use file: paths. Do not commit Packages/manifest.json until switched back to Remote.");
            }
            else
            {
                RaxyHubGui.DrawStatusBanner(
                    true,
                    "Manifest is in REMOTE mode",
                    "Safe to commit Packages/manifest.json.");
            }

            RaxyHubGui.DrawHint("Open a Manifest Switcher asset to switch packages Local/Remote per entry.");
        }

        void DrawToolbar()
        {
            _filter = RaxyHubGui.DrawToolbarRow(_filter, out bool refresh);
            if (refresh)
                RefreshList();

            RaxyHubGui.DrawCountChip($"{_entries.Count} switchers");
        }

        void DrawList()
        {
            if (_entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No ManifestSwitcherSO assets found. Create one via Assets > Create > RAXY > Editor > Manifest Switcher.",
                    MessageType.Info);
                return;
            }

            int shown = 0;
            foreach (var entry in _entries)
            {
                if (entry.Asset == null)
                    continue;

                if (!PassesFilter(entry))
                    continue;

                shown++;
                DrawRow(entry);
            }

            if (shown == 0)
                RaxyHubGui.DrawHint("No Manifest Switcher assets match the current filter.");
        }

        bool PassesFilter(SwitcherEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_filter))
                return true;

            string q = _filter.Trim();
            return entry.DisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.AssetPath.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.FolderPath.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void DrawRow(SwitcherEntry entry)
        {
            int packageCount = entry.Asset.packages != null ? entry.Asset.packages.Count : 0;

            RaxyHubGui.BeginCard();
            RaxyHubGui.DrawTitleRow(entry.DisplayName, $"{packageCount} pkgs");
            RaxyHubGui.DrawMutedPath(entry.AssetPath);

            EditorGUILayout.Space(4f);
            if (RaxyHubGui.PrimaryButton("Select", 72f))
            {
                Selection.activeObject = entry.Asset;
                EditorGUIUtility.PingObject(entry.Asset);
            }

            RaxyHubGui.EndCard();
        }

        void RefreshList()
        {
            _entries.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ManifestSwitcherSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ManifestSwitcherSO>(path);
                if (asset == null)
                    continue;

                _entries.Add(new SwitcherEntry
                {
                    Asset = asset,
                    AssetPath = path,
                    FolderPath = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? path,
                    DisplayName = asset.name
                });
            }

            _entries.Sort((a, b) =>
                string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
        }

        static bool IsProjectManifestLocal()
        {
            const string manifestPath = "Packages/manifest.json";
            if (!File.Exists(manifestPath))
                return false;

            try
            {
                return File.ReadAllText(manifestPath).Contains("\"file:");
            }
            catch
            {
                return false;
            }
        }
    }
}
