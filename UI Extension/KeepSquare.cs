using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class KeepSquare : MonoBehaviour
    {
        public enum SquareMode
        {
            WidthMatchesHeight,
            HeightMatchesWidth,
            FitSmallerSide,
            FitLargerSide
        }

        [SerializeField]
        private SquareMode mode = SquareMode.WidthMatchesHeight;

        [SerializeField]
        private bool useUpdate = true; // toggle for Update

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (!useUpdate)
            {
                Refresh(); // initial sizing if Update is off
            }
        }

        private void Update()
        {
            if (!useUpdate || rectTransform == null) 
                return;

            Refresh();
        }

        /// <summary>
        /// Manually refresh the square sizing.
        /// </summary>
        [Button]
        public void Refresh()
        {
            if (rectTransform == null) return;

            Vector2 size = rectTransform.rect.size;

            switch (mode)
            {
                case SquareMode.WidthMatchesHeight:
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.y);
                    break;

                case SquareMode.HeightMatchesWidth:
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.x);
                    break;

                case SquareMode.FitSmallerSide:
                    float smaller = Mathf.Min(size.x, size.y);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, smaller);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, smaller);
                    break;

                case SquareMode.FitLargerSide:
                    float larger = Mathf.Max(size.x, size.y);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, larger);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, larger);
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (!useUpdate)
                Refresh();
        }
#endif
    }
}
