using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RAXY.Utility
{
    public static class CustomUtility
    {
        /// <summary>
        /// Replacement for UniTask.WaitUntil using Task.Delay loop.
        /// </summary>
        // public static async Task WaitUntil(Func<bool> condition, int checkIntervalMs = 10)
        // {
        //     while (!condition())
        //     {
        //         await Task.Delay(checkIntervalMs);
        //     }
        // }

        public static string GetObjectNameWithout_Clone(string name)
        {
            if (string.IsNullOrEmpty(name)) 
                return "";

            const string cloneSuffix = "(Clone)";

            if (name.EndsWith(cloneSuffix)) 
                name = name.Substring(0, name.Length - cloneSuffix.Length).Trim();

            return name;
        }


        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;

            return Mathf.Clamp(angle, min, max);
        }

        public static Transform FindChildRecursive(Transform parent, string name)
        {
            if (name == string.Empty || name == null)
                return null;

            Transform result = parent.Find(name);
            if (result != null)
                return result;
            foreach (Transform child in parent)
            {
                result = FindChildRecursive(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void CleanChildren(Transform container)
        {
            if (container == null)
                return;

            // First collect all children into a list to avoid modifying the hierarchy while iterating
            var children = new List<Transform>();
            foreach (Transform child in container)
            {
                children.Add(child);
            }

            foreach (var child in children)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(child.gameObject);
                else
#endif
                    Object.Destroy(child.gameObject);
            }
        }

        public static Vector3 GetTransformReferenceOnWorldPoint(Transform transformReference, Transform Owner)
        {
            // Transform pivot = transformReference.projectileSpawnPoint;
            Transform pivot = transformReference;

            // Get the local position and rotation of the pivot relative to the character
            Vector3 spawnPointLocalPos = pivot.localPosition;

            // Calculate the world position and rotation of the slash effect
            return Owner.TransformPoint(spawnPointLocalPos);
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }

        public static List<int> GenerateSteppedNumber(int step, int offset)
        {
            if (step <= 0)
                return null;

            var result = new List<int>();

            int count = 24 / step; // total elements to generate
            int hour = offset % 24;

            for (int i = 0; i < count; i++)
            {
                result.Add(hour);
                hour = (hour + step) % 24;
            }

            return result;
        }

        public static List<T> GetOverriddenList<T>(List<T> defaultList,
                                                    Dictionary<int, T> overrideDict,
                                                    bool useOverride)
                                                    where T : class
        {
            if (defaultList == null)
                return new List<T>();

            if (!useOverride || overrideDict == null || overrideDict.Count == 0)
                return defaultList;

            var result = new List<T>(defaultList.Count);

            for (int i = 0; i < defaultList.Count; i++)
            {
                if (overrideDict.TryGetValue(i, out var overrideValue) && overrideValue != null)
                    result.Add(overrideValue);
                else
                    result.Add(defaultList[i]);
            }

            return result;
        }

        /// <summary>
        /// Auto fills float array. 
        /// Uses non-zero values as anchors and interpolates missing ones.
        /// </summary>
        public static float[] AutoFillFloat(float[] values)
        {
            if (values == null || values.Length == 0)
                return values;

            int lastAnchor = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != 0f) // treat "0" as empty
                {
                    if (lastAnchor >= 0)
                    {
                        float startVal = values[lastAnchor];
                        float endVal = values[i];
                        int steps = i - lastAnchor;

                        for (int j = 1; j < steps; j++)
                        {
                            float t = (float)j / steps;
                            values[lastAnchor + j] = Mathf.Lerp(startVal, endVal, t);
                        }
                    }
                    lastAnchor = i;
                }
            }

            return values;
        }

        /// <summary>
        /// Auto fills int array.
        /// Uses non-zero values as anchors and interpolates missing ones.
        /// </summary>
        public static int[] AutoFillInt(int[] values)
        {
            if (values == null || values.Length == 0)
                return values;

            int lastAnchor = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != 0) // treat "0" as empty
                {
                    if (lastAnchor >= 0)
                    {
                        int startVal = values[lastAnchor];
                        int endVal = values[i];
                        int steps = i - lastAnchor;

                        for (int j = 1; j < steps; j++)
                        {
                            float t = (float)j / steps;
                            values[lastAnchor + j] = Mathf.RoundToInt(Mathf.Lerp(startVal, endVal, t));
                        }
                    }
                    lastAnchor = i;
                }
            }

            return values;
        }

        public static void DestroyAllChildren(Transform parent)
        {
#if UNITY_EDITOR
            bool useImmediate = !Application.isPlaying;
#else
            bool useImmediate = false;
#endif

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject;

                if (useImmediate)
                    Object.DestroyImmediate(child);
                else
                    Object.Destroy(child);
            }
        }

        public static void DestroyAllChildren(GameObject parentObj)
        {
            DestroyAllChildren(parentObj.transform);
        }
    }
}
