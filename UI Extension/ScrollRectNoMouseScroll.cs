using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RAXY.Utility.UI
{
    public class ScrollRectNoMouseScroll : ScrollRect
    {
        public override void OnScroll(PointerEventData data)
        {
        }
    }
}