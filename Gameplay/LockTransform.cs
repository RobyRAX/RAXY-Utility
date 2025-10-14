using UnityEngine;
using Sirenix.OdinInspector;

namespace RAXY.Utility.Gameplay
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class LockTransform : MonoBehaviour
    {
#if UNITY_EDITOR
        [Title("Lock Local Position")]
        [ToggleLeft] public bool lockPosX = true;
        [ToggleLeft] public bool lockPosY = true;
        [ToggleLeft] public bool lockPosZ = true;

        [Title("Lock Local Rotation")]
        [ToggleLeft] public bool lockRotX = true;
        [ToggleLeft] public bool lockRotY = true;
        [ToggleLeft] public bool lockRotZ = true;

        [Title("Lock Local Scale")]
        [ToggleLeft] public bool lockScaleX = false;
        [ToggleLeft] public bool lockScaleY = false;
        [ToggleLeft] public bool lockScaleZ = false;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                DoLockTransform();
            }
        }

        private void DoLockTransform()
        {
            // Lock Position
            Vector3 localPos = transform.localPosition;
            if (lockPosX) localPos.x = 0f;
            if (lockPosY) localPos.y = 0f;
            if (lockPosZ) localPos.z = 0f;
            transform.localPosition = localPos;

            // Lock Rotation
            Vector3 localEuler = transform.localEulerAngles;
            if (lockRotX) localEuler.x = 0f;
            if (lockRotY) localEuler.y = 0f;
            if (lockRotZ) localEuler.z = 0f;
            transform.localEulerAngles = localEuler;

            // Lock Scale
            Vector3 localScale = transform.localScale;
            if (lockScaleX) localScale.x = 1f;
            if (lockScaleY) localScale.y = 1f;
            if (lockScaleZ) localScale.z = 1f;
            transform.localScale = localScale;
        }
#endif
    }
}