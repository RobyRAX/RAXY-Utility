using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

namespace RAXY.Utility.Gameplay
{
    public class ParentSetter : MonoBehaviour
    {
        [SerializeField] bool applyOnAwake;
        [SerializeField] bool autoFindParentOnAwake;

        [EnumToggleButtons]
        [OnValueChanged("Refresh")]
        [SerializeField] ParentMode parentMode;

        [ShowIf("@parentMode == ParentMode.Transform && !autoFindParentOnAwake")]
        [SerializeField] Transform rootTranform;

        [ShowIf("@parentMode == ParentMode.Animator && !autoFindParentOnAwake")]
        [SerializeField] Animator rootAnimator;

        [ListDrawerSettings(OnTitleBarGUI = "DrawRefreshBtn", CustomAddFunction = "NewEntry")]
        [SerializeField] List<ChildParentTargetEntry> entries;

        void Awake()
        {
            if (applyOnAwake)
            {
                ApplyParenting();
            }

            if (autoFindParentOnAwake)
            {
                rootTranform = transform.parent;
                rootAnimator = rootAnimator.GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Applies parenting of all entries.
        /// </summary>
        [Button]
        public void ApplyParenting()
        {
            if (entries == null) return;

            foreach (var entry in entries)
            {
                Transform parentTarget = GetParentTarget(entry);

                if (parentTarget == null)
                {
                    Debug.LogWarning($"[ParentSetter] Parent not found for entry on {gameObject.name}");
                    continue;
                }

                foreach (var child in entry.childs)
                {
                    if (child == null) continue;

                    child.SetParent(parentTarget, worldPositionStays: true);
                }
            }
        }

        Transform GetParentTarget(ChildParentTargetEntry entry)
        {
            switch (parentMode)
            {
                case ParentMode.Transform:
                    if (rootTranform == null) return null;

                    // Find by name under root
                    var found = CustomUtility.FindChildRecursive(rootTranform, entry.parentName);
                    return found;

                case ParentMode.Animator:
                    if (rootAnimator == null) return null;
                    return rootAnimator.GetBoneTransform(entry.humanBoneParent);

                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private void DrawRefreshBtn()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                Refresh();
            }
        }

        void Refresh()
        {
            foreach (var entry in entries)
                entry.SetParetMode(parentMode);
        }

        ChildParentTargetEntry NewEntry()
        {
            var newEntry = new ChildParentTargetEntry();
            newEntry.SetParetMode(parentMode);
            return newEntry;
        }
#endif
    }

    [Serializable]
    public class ChildParentTargetEntry
    {
        [ShowIf("@parentMode == ParentMode.Transform")]
        [PropertyOrder(-2)]
        [HorizontalGroup("Parent")]
        [LabelText("Parent Name | Picker")]
        public string parentName;

        [ShowIf("@parentMode == ParentMode.Animator")]
        public HumanBodyBones humanBoneParent;

        public List<Transform> childs = new List<Transform>();

#if UNITY_EDITOR
        [SerializeField]
        [OnValueChanged("OnPickerChangedHandler")]
        [ShowIf("@parentMode == ParentMode.Transform")]
        [PropertyOrder(-1)]
        [HorizontalGroup("Parent", 0.25f)]
        [HideLabel]
        Transform transformPicker;

        void OnPickerChangedHandler()
        {
            if (transformPicker != null)
                parentName = transformPicker.name;

            transformPicker = null;
        }

        ParentMode parentMode;

        public void SetParetMode(ParentMode parentMode)
        {
            this.parentMode = parentMode;
        }
#endif
    }

    public enum ParentMode
    {
        Transform,
        Animator
    }
}
