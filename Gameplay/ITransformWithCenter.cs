using UnityEngine;

public interface ITransformWithCenter
{
    public Transform GetTransform { get; }
    public Vector3 CenterPoint { get; }
}
