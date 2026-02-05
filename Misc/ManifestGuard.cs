#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;

namespace RAXY.Utility
{
    /// <summary>
    /// Auto-validates manifest.json on Unity startup to prevent accidental commits of local file paths
    /// </summary>
    [InitializeOnLoad]
    public static class ManifestGuard
    {
        private const string MANIFEST_PATH = "Packages/manifest.json";
        private const string PREF_KEY_WARNED = "ManifestGuard_WarnedThisSession";

        static ManifestGuard()
        {
            EditorApplication.delayCall += () =>
            {
                ValidateManifest();
            };

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ValidateManifest();
            }
        }

        private static void ValidateManifest()
        {
            if (!File.Exists(MANIFEST_PATH))
                return;

            if (SessionState.GetBool(PREF_KEY_WARNED, false))
                return;

            try
            {
                string json = File.ReadAllText(MANIFEST_PATH);

                if (json.Contains("\"file:"))
                {
                    Debug.LogWarning("[Manifest Guard] manifest.json contains LOCAL file paths. " +
                                     "Switch to Remote before committing to avoid breaking other machines.");

                    SessionState.SetBool(PREF_KEY_WARNED, true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ManifestGuard failed to read manifest: {ex.Message}");
            }
        }

        [MenuItem("RAXY/Validate Manifest")]
        private static void ManualValidate()
        {
            SessionState.SetBool(PREF_KEY_WARNED, false);
            ValidateManifest();

            if (!SessionState.GetBool(PREF_KEY_WARNED, false))
            {
                Debug.Log("Manifest is clean - no local file paths detected.");
            }
        }
    }
}

#endif
