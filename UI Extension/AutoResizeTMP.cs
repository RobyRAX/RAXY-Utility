using UnityEngine;
using TMPro;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Utility.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class AutoResizeTMP : MonoBehaviour
    {
        public enum ResizeMode
        {
            DynamicWidth,
            DynamicHeight,
            DynamicWidthAndHeight
        }

        [Tooltip("Choose whether to resize the width, height, or both to fit the text")]
        public ResizeMode resizeMode = ResizeMode.DynamicWidth;

        [Tooltip("If true, updates every frame (Editor/Play); otherwise, only updates on text change or manual call.")]
        [FormerlySerializedAs("useRealtimeUpdate")]
        public bool useUpdate = true;

        [Tooltip("Optional minimum size (in pixels)")]
        public float minSize = 0f;

        [Tooltip("Optional maximum size (in pixels). Set 0 for no limit")]
        public float maxSize = 0f;

        private TextMeshProUGUI tmpText;
        private string _lastText;
        private Coroutine resizeCoroutine;

        void Awake()
        {
            tmpText = GetComponent<TextMeshProUGUI>();
            SafeRefresh();
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && useUpdate)
                EditorApplication.update += EditorUpdate;
#endif
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            SafeRefresh();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorApplication.update -= EditorUpdate;
#endif
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            if (resizeCoroutine != null)
            {
                StopCoroutine(resizeCoroutine);
                resizeCoroutine = null;
            }
        }

        private void OnTextChanged(Object obj)
        {
            if (obj == tmpText)
                SafeRefresh();
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (!useUpdate || tmpText == null) 
                return;

            if (_lastText != tmpText.text)
            {
                _lastText = tmpText.text;
                SafeRefresh();
            }
        }
#endif

        private void SafeRefresh()
        {
            if (Application.isPlaying)
            {
                if (resizeCoroutine != null)
                    StopCoroutine(resizeCoroutine);

                resizeCoroutine = StartCoroutine(DelayedRefresh());
            }
            else
            {
                Refresh(); // Safe in editor
            }
        }

        private IEnumerator DelayedRefresh()
        {
            yield return null; // Wait one frame
            Refresh();
            resizeCoroutine = null;
        }

        [Button]
        public void Refresh()
        {
            if (tmpText == null)
                return;

            Canvas.ForceUpdateCanvases(); // Ensure layout is up to date
            Vector2 preferredSize = tmpText.GetPreferredValues();
            var rt = tmpText.rectTransform;

            if (resizeMode == ResizeMode.DynamicWidth || resizeMode == ResizeMode.DynamicWidthAndHeight)
            {
                float width = preferredSize.x;
                if (maxSize > 0f)
                    width = Mathf.Clamp(width, minSize, maxSize);
                else
                    width = Mathf.Max(width, minSize);

                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            if (resizeMode == ResizeMode.DynamicHeight || resizeMode == ResizeMode.DynamicWidthAndHeight)
            {
                float height = preferredSize.y;
                if (maxSize > 0f)
                    height = Mathf.Clamp(height, minSize, maxSize);
                else
                    height = Mathf.Max(height, minSize);

                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }
    }
}
