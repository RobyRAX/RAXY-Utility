using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RAXY.Utility
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Awake()
        {
            if (transform.parent != null)
                transform.parent = null;

            DontDestroyOnLoad(gameObject);
        }
    }
}