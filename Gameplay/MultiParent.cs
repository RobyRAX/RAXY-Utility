using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;

namespace RAXY.Utility.Gameplay
{
    public class MultiParent : MonoBehaviour
    {
        [TitleGroup("Find Target on Runtime", "used if the object is spawned in runtime 'cause it doesn't have direct reference to the targets")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label")]
        public List<MultiParentTargetFinder> targetFinders;

        [TitleGroup("Find Target on Runtime")]
        [Button]
        public void FindFollowTargets(Transform parent)
        {
            followTargets.Clear();
            foreach (var targetFinder in targetFinders)
            {
                followTargets.Add(targetFinder.FindTarget(parent));
            }
        }

        [TitleGroup("Target Settings")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<Transform> followTargets;

        [TitleGroup("Target Settings")]
        public Vector3 positionOffset;
        [TitleGroup("Target Settings")]
        public Vector3 rotationOffset;

        // Current target reference
        [TitleGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        protected Transform currentTarget;

        // Set the target to follow by index
        [TitleGroup("Target Settings")]
        [Button]
        public virtual void SetTarget(int index)
        {
            if (index >= 0 && index < followTargets.Count)
            {
                currentTarget = followTargets[index];
            }
            else
            {
                CustomDebug.LogWarning("Invalid target index: " + index);
            }
        }

        // Detach from the current target
        public virtual void Detach()
        {
            currentTarget = null;
        }

        protected virtual void LateUpdate()
        {
            if (currentTarget != null)
            {
                transform.position = currentTarget.position + currentTarget.rotation * positionOffset;
                transform.rotation = currentTarget.rotation * Quaternion.Euler(rotationOffset);
            }
        }
    }

    [Serializable]
    public class MultiParentTargetFinder
    {
        string Label
        {
            get
            {
                if (mode == TargetFindMode.ByName)
                {
                    return $"ByName - {targetName}";
                }
                else if (mode == TargetFindMode.HumanoidRig)
                {
                    return $"HumanoidRig - {targetBone}";
                }

                return "";
            }
        }

        public enum TargetFindMode
        {
            ByName,
            HumanoidRig
        }

        [EnumToggleButtons]
        public TargetFindMode mode;

        [ShowIf("@mode == TargetFindMode.ByName")]
        public string targetName;

        [ShowIf("@mode == TargetFindMode.HumanoidRig")]
        public HumanBodyBones targetBone;

        public Transform FindTarget(Transform parent)
        {
            if (mode == TargetFindMode.ByName)
            {
                return CustomUtility.FindChildRecursive(parent, targetName);
            }
            else if (mode == TargetFindMode.HumanoidRig)
            {
                return parent.GetComponent<Animator>().GetBoneTransform(targetBone);
            }

            CustomDebug.LogWarning($"Target '{targetName}' not found in {parent.name}");
            return null;
        }
    }
}