using UnityEngine;

namespace RAXY.Utility.Gizmo
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public class BoxColliderGizmoDrawer : MonoBehaviour
    {
#if UNITY_EDITOR

        [Header("Gizmo Toggles")]
        public bool drawFilled = true;
        public bool drawWire = true;

        [Header("Gizmo Appearance")]
        public Color fillColor = new Color(0f, 1f, 0f, 0.25f);      // Transparent green
        public Color wireColor = new Color(0f, 1f, 0f, 1f);          // Solid green

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null || !box.enabled)
                return;

            // Apply world transform to match position/rotation/scale
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            if (drawFilled)
            {
                Gizmos.color = fillColor;
                Gizmos.DrawCube(box.center, box.size);
            }

            if (drawWire)
            {
                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
#endif
    }
}