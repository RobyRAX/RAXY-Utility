using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace RAXY.Utility.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class CollisionEventDispatcher : MonoBehaviour
    {
        [Title("Trigger Settings")]
        [ToggleLeft] public bool useOnTriggerEnter = true;
        [ToggleLeft] public bool useOnTriggerExit = false;

        [Title("Collision Settings")]
        [ToggleLeft] public bool useOnCollisionEnter = false;
        [ToggleLeft] public bool useOnCollisionExit = false;

        [Title("Character Controller Settings")]
        [ToggleLeft] public bool useOnControllerHit = false;

        [Title("Filtering")]
        [ToggleLeft] public bool filterByTag = false;
        [ShowIf("filterByTag")] public string[] allowedTags;

        [ToggleLeft] public bool filterByLayer = false;
        [ShowIf("filterByLayer")] public LayerMask allowedLayers;

        [ToggleLeft] public bool ignoreSelfCollisions = true;

        [Title("Trigger Events")]
        [ShowIf("useOnTriggerEnter")] public UnityEvent<Collider> onTriggerEnter;
        [ShowIf("useOnTriggerExit")] public UnityEvent<Collider> onTriggerExit;

        [Title("Collision Events")]
        [ShowIf("useOnCollisionEnter")] public UnityEvent<Collision> onCollisionEnter;
        [ShowIf("useOnCollisionExit")] public UnityEvent<Collision> onCollisionExit;

        [Title("Controller Events")]
        [ShowIf("useOnControllerHit")] public UnityEvent<ControllerColliderHit> onControllerHit;

        private Collider selfCollider;

        private void Awake()
        {
            selfCollider = GetComponent<Collider>();
        }

        private bool PassesTagFilter(GameObject obj)
        {
            if (!filterByTag) return true;
            foreach (var tag in allowedTags)
            {
                if (obj.CompareTag(tag))
                    return true;
            }
            return false;
        }

        private bool PassesLayerFilter(GameObject obj)
        {
            if (!filterByLayer) return true;
            return ((1 << obj.layer) & allowedLayers) != 0;
        }

        private bool PassesSelfCollisionCheck(GameObject obj)
        {
            if (!ignoreSelfCollisions) return true;
            return obj != gameObject && obj.transform.root != transform.root;
        }

        private bool PassesAllFilters(GameObject obj)
        {
            return PassesSelfCollisionCheck(obj) && PassesTagFilter(obj) && PassesLayerFilter(obj);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (useOnTriggerEnter && PassesAllFilters(other.gameObject))
                onTriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (useOnTriggerExit && PassesAllFilters(other.gameObject))
                onTriggerExit?.Invoke(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (useOnCollisionEnter && PassesAllFilters(collision.gameObject))
                onCollisionEnter?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (useOnCollisionExit && PassesAllFilters(collision.gameObject))
                onCollisionExit?.Invoke(collision);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (useOnControllerHit && PassesAllFilters(hit.gameObject))
                onControllerHit?.Invoke(hit);
        }
    }
}