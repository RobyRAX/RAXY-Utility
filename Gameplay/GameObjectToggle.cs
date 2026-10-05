using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Utility.Gameplay
{
    public class GameObjectToggle : MonoBehaviour
    {
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<GameObject> targets = new();

        [Button]
        public void SetEnable()
        {
            SetActiveForAll(true);
        }

        [Button]
        public void SetDisable()
        {
            SetActiveForAll(false);
        }

        void SetActiveForAll(bool active)
        {
            if (targets == null)
                return;

            foreach (var target in targets)
            {
                if (target != null)
                    target.SetActive(active);
            }
        }
    }
}
