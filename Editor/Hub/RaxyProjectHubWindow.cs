using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RAXY.Utility.Editor.Hub
{
    public sealed class RaxyProjectHubWindow : EditorWindow
    {
        const string MenuPath = "Tools/RAXY/Project Hub";
        const float SidebarWidth = 180f;

        List<IRaxyHubModule> _modules = new();
        int _selectedIndex;
        Vector2 _sidebarScroll;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            var window = GetWindow<RaxyProjectHubWindow>();
            window.titleContent = new GUIContent("RAXY Project Hub");
            window.minSize = new Vector2(720f, 460f);
            window.Show();
        }

        void OnEnable()
        {
            ReloadModules();
        }

        void OnDisable()
        {
            DisableModules();
        }

        void ReloadModules()
        {
            DisableModules();
            _modules = RaxyHubModuleDiscovery.CreateModules();
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, _modules.Count - 1));

            foreach (var module in _modules)
                module.OnEnable();
        }

        void DisableModules()
        {
            if (_modules == null)
                return;

            foreach (var module in _modules)
                module.OnDisable();

            _modules.Clear();
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                DrawSidebar();
                DrawContent();
            }
        }

        void DrawSidebar()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true)))
            {
                RaxyHubGui.DrawSidebarHeader("RAXY Hub");

                if (RaxyHubGui.SecondaryButton("Refresh Modules", SidebarWidth - 12f))
                    ReloadModules();

                EditorGUILayout.Space(6f);

                _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll, GUILayout.ExpandHeight(true));

                if (_modules.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No hub modules found.\nInstall a RAXY package that registers an IRaxyHubModule.",
                        MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < _modules.Count; i++)
                    {
                        var module = _modules[i];
                        bool selected = i == _selectedIndex;

                        if (RaxyHubGui.DrawNavItem(module.DisplayName, selected))
                            _selectedIndex = i;
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawContent()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandHeight(true)))
            {
                if (_modules.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _modules.Count)
                {
                    RaxyHubGui.DrawWindowHeader("Project Hub", "Select a module from the sidebar.");
                    return;
                }

                var module = _modules[_selectedIndex];
                RaxyHubGui.DrawWindowHeader(module.DisplayName, $"Module id: {module.Id}");

                // Modules own their own scroll areas so lists can fill remaining height.
                module.OnGUI();
            }
        }
    }
}
