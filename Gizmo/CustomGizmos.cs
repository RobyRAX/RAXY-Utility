using UnityEngine;

namespace RAXY.Utility.Gizmo
{
    public static class CustomGizmos
    {
        public static void DrawSphereCastGizmos(Vector3 origin, Vector3 direction,
                                            float distance, float radius,
                                            Color startColor = default, Color endColor = default)
        {
            if (startColor == default)
                startColor = Color.red;
            if (endColor == default)
                endColor = Color.red;

            Vector3 startPos = origin;
            Vector3 endPos = origin + direction.normalized * distance;

            Gizmos.color = startColor;
            Gizmos.DrawWireSphere(startPos, radius);

            Gizmos.color = endColor;
            Gizmos.DrawWireSphere(endPos, radius);
        }
    }
}