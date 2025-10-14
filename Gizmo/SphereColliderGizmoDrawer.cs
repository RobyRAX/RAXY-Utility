using UnityEngine;

namespace RAXY.Utility.Gizmo
{
    [ExecuteAlways]
    [RequireComponent(typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public class SphereColliderGizmoDrawer : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Gizmo Toggles")]
        public bool drawFilled = true;
        public bool drawWire = true;

        [Header("Gizmo Appearance")]
        public Color fillColor = new Color(0f, 0.5f, 1f, 0.25f);      // Transparent blue
        public Color wireColor = new Color(0f, 0.5f, 1f, 1f);          // Solid blue

        private void OnDrawGizmos()
        {
            var sphere = GetComponent<SphereCollider>();
            if (sphere == null || !sphere.enabled)
                return;

            // Apply world transform including scale (only uniform scale makes sense for spheres)
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Vector3 center = sphere.center;
            float radius = sphere.radius;

            if (drawFilled)
            {
                Gizmos.color = fillColor;
                Gizmos.DrawSphere(center, radius);
            }

            if (drawWire)
            {
                Gizmos.color = wireColor;
                Gizmos.DrawWireSphere(center, radius);
            }
        }
#endif
    }
}