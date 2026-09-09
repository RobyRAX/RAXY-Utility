using UnityEditor;
using UnityEngine;

namespace RAXY.Utility.Editor.Hub
{
    /// <summary>
    /// Shared IMGUI helpers for RAXY Project Hub shell and modules.
    /// </summary>
    public static class RaxyHubGui
    {
        const float ButtonHeight = 22f;
        const float CardPadding = 6f;

        static GUIStyle _headerTitle;
        static GUIStyle _headerSubtitle;
        static GUIStyle _sidebarTitle;
        static GUIStyle _navItem;
        static GUIStyle _navItemSelected;
        static GUIStyle _cardTitle;
        static GUIStyle _mutedPath;
        static GUIStyle _badge;
        static GUIStyle _bannerTitle;
        static GUIStyle _bannerDetail;
        static GUIStyle _countChip;
        static GUIStyle _hint;
        static bool _stylesReady;
        static bool _stylesWerePro;

        static void EnsureStyles()
        {
            bool isPro = EditorGUIUtility.isProSkin;
            if (_stylesReady && _stylesWerePro == isPro)
                return;

            _stylesWerePro = isPro;
            _stylesReady = true;

            _headerTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 2, 0)
            };

            _headerSubtitle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                normal = { textColor = MutedTextColor() }
            };

            _sidebarTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(4, 4, 6, 2)
            };

            _navItem = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 8, 6, 6),
                margin = new RectOffset(2, 2, 1, 1),
                fixedHeight = 28f,
                fontStyle = FontStyle.Normal
            };

            _navItemSelected = new GUIStyle(_navItem)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = isPro ? new Color(0.92f, 0.95f, 1f) : new Color(0.05f, 0.15f, 0.35f),
                    background = MakeTex(SelectedNavColor())
                }
            };

            _cardTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };

            _mutedPath = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = MutedTextColor() }
            };

            _badge = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(6, 6, 2, 2),
                margin = new RectOffset(4, 0, 2, 2),
                normal =
                {
                    textColor = isPro ? new Color(0.85f, 0.9f, 1f) : new Color(0.15f, 0.25f, 0.45f),
                    background = MakeTex(BadgeBgColor())
                }
            };

            _bannerTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };

            _bannerDetail = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = MutedTextColor() }
            };

            _countChip = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 2, 2),
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = isPro ? new Color(0.8f, 0.85f, 0.95f) : new Color(0.2f, 0.25f, 0.4f),
                    background = MakeTex(ChipBgColor())
                }
            };

            _hint = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true,
                margin = new RectOffset(4, 4, 4, 2)
            };
        }

        static Color MutedTextColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.65f, 0.68f, 0.72f)
                : new Color(0.35f, 0.38f, 0.42f);

        static Color SelectedNavColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.35f, 0.55f, 1f)
                : new Color(0.72f, 0.82f, 0.95f, 1f);

        static Color BadgeBgColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.28f, 0.32f, 0.4f, 1f)
                : new Color(0.82f, 0.86f, 0.92f, 1f);

        static Color ChipBgColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.28f, 0.34f, 1f)
                : new Color(0.88f, 0.9f, 0.93f, 1f);

        static Color BannerOkColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.18f, 0.32f, 0.24f, 1f)
                : new Color(0.78f, 0.92f, 0.82f, 1f);

        static Color BannerWarnColor()
            => EditorGUIUtility.isProSkin
                ? new Color(0.38f, 0.32f, 0.16f, 1f)
                : new Color(0.98f, 0.93f, 0.78f, 1f);

        static Texture2D MakeTex(Color color)
        {
            var tex = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static void DrawWindowHeader(string title, string subtitle = null)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(title ?? "", _headerTitle);
            if (!string.IsNullOrEmpty(subtitle))
                EditorGUILayout.LabelField(subtitle, _headerSubtitle);
            DrawThinSeparator();
        }

        public static void DrawSidebarHeader(string title = "RAXY Hub")
        {
            EnsureStyles();
            EditorGUILayout.LabelField(title, _sidebarTitle);
            DrawThinSeparator();
        }

        public static bool DrawNavItem(string label, bool selected)
        {
            EnsureStyles();
            var style = selected ? _navItemSelected : _navItem;
            bool clicked = GUILayout.Button(label ?? "", style, GUILayout.ExpandWidth(true));
            return clicked && !selected;
        }

        public static void DrawStatusBanner(bool ok, string title, string detail = null)
        {
            EnsureStyles();
            var bg = ok ? BannerOkColor() : BannerWarnColor();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8f);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(title ?? "", _bannerTitle);
                    if (!string.IsNullOrEmpty(detail))
                        EditorGUILayout.LabelField(detail, _bannerDetail);
                }
            }
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                var r = GUILayoutUtility.GetLastRect();
                EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), bg);
            }
        }

        public static string DrawToolbarRow(string filter, out bool refreshClicked, string refreshLabel = "Refresh")
        {
            EnsureStyles();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Filter", GUILayout.Width(40f));
                filter = EditorGUILayout.TextField(filter ?? "");
                refreshClicked = SecondaryButton(refreshLabel, 90f);
            }

            return filter;
        }

        public static void DrawCountChip(string label)
        {
            EnsureStyles();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label ?? "", _countChip);
            }
        }

        public static void BeginCard()
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(CardPadding * 0.5f);
        }

        public static void EndCard()
        {
            GUILayout.Space(CardPadding * 0.5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        public static void DrawTitleRow(string title, string badge = null, string badge2 = null)
        {
            EnsureStyles();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(title ?? "", _cardTitle);
                GUILayout.FlexibleSpace();
                if (!string.IsNullOrEmpty(badge))
                    GUILayout.Label(badge, _badge, GUILayout.MinWidth(56f));
                if (!string.IsNullOrEmpty(badge2))
                    GUILayout.Label(badge2, _badge, GUILayout.MinWidth(72f));
            }
        }

        public static void DrawMutedPath(string folder)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(folder ?? "", _mutedPath);
        }

        public static void DrawHint(string text)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(text ?? "", _hint);
        }

        public static bool PrimaryButton(string label, float width = 0f)
        {
            if (width > 0f)
                return GUILayout.Button(label ?? "", GUILayout.Width(width), GUILayout.Height(ButtonHeight));
            return GUILayout.Button(label ?? "", GUILayout.Height(ButtonHeight));
        }

        public static bool SecondaryButton(string label, float width = 0f)
        {
            var style = EditorStyles.miniButton;
            if (width > 0f)
                return GUILayout.Button(label ?? "", style, GUILayout.Width(width), GUILayout.Height(ButtonHeight));
            return GUILayout.Button(label ?? "", style, GUILayout.Height(ButtonHeight));
        }

        public static void DrawThinSeparator()
        {
            EditorGUILayout.Space(2f);
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(0.35f, 0.35f, 0.35f, 1f)
                : new Color(0.65f, 0.65f, 0.65f, 1f));
            EditorGUILayout.Space(4f);
        }
    }
}
