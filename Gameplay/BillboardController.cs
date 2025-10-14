using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility.Gameplay
{
    public class BillboardController : MonoBehaviour
    {
        [Tooltip("If true, updates every few seconds instead of every LateUpdate.")]
        public bool useDelay = false;

        [ShowIf("@useDelay")]
        [SuffixLabel("seconds")]
        public float updateDelay = 0.1f;

        [LabelText("Rotation Offset (Euler)")]
        public Vector3 rotationOffset;

        private Camera _targetCamera;
        private Coroutine billboardCoroutine;

        private void OnEnable()
        {
            if (_targetCamera == null)
                _targetCamera = Camera.main;

            if (useDelay)
            {
                // Stop existing coroutine if any
                if (billboardCoroutine != null)
                {
                    StopCoroutine(billboardCoroutine);
                    billboardCoroutine = null;
                }

                // Start new delayed update coroutine
                billboardCoroutine = StartCoroutine(UpdateBillboardDelayed());
            }
        }

        private void OnDisable()
        {
            if (billboardCoroutine != null)
            {
                StopCoroutine(billboardCoroutine);
                billboardCoroutine = null;
            }
        }

        private void LateUpdate()
        {
            if (!useDelay && _targetCamera != null)
            {
                FaceCamera();
            }
        }

        private IEnumerator UpdateBillboardDelayed()
        {
            // Keep updating the billboard with a delay
            while (true)
            {
                if (_targetCamera != null)
                {
                    FaceCamera();
                }

                yield return new WaitForSeconds(updateDelay);
            }
        }

        private void FaceCamera()
        {
            if (_targetCamera == null)
                return;

            // Make the object face the camera
            transform.forward = _targetCamera.transform.forward;

            // Apply additional rotation offset
            transform.rotation *= Quaternion.Euler(rotationOffset);
        }
    }
}
