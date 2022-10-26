using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnitFramework.Runtime
{
    public class CanvasView : View
    {
        public List<View> Views => GetViewComponentsInChildren<View>();

        public bool isDontDestroy = false;
    }
}