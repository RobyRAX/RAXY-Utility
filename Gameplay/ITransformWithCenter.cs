using UnityEngine;

namespace RAXY.Utility.Gameplay
{
    public interface ITransformWithCenter
    {
        public Transform GetTransform { get; }
        public Vector3 CenterPoint { get; }
    }
}