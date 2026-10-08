using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility.Gameplay
{
    public class GameObjectDestroyer : MonoBehaviour
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<GameObject> targets = new();

        [SuffixLabel("seconds")]
        [MinValue(0f)]
        [Tooltip("Delay before destroying each target. Set per instance for different enemy timings.")]
        public float delay;

        [Button]
        public void DestroyTargets()
        {
            if (targets == null)
                return;

            foreach (var target in targets)
            {
                if (target != null)
                    Destroy(target, delay);
            }
        }
    }
}
