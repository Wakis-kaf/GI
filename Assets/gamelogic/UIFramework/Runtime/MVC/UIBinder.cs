using System;
using System.Collections.Generic;
using UnityEngine;

namespace GFramework.UI
{
    public class UIBinder : MonoBehaviour
    {
        [ReadOnlyInInspector] public TextAsset cs = null;
        public UILayer layer = UILayer.Common;
        public UINode node = UINode.middle;
        [HideInInspector] public List<UIVar> varsArr = new List<UIVar>();

        public UIVar GetVar(int index)
        {
            if (index >= this.varsArr.Count)
                return null;
            return this.varsArr[index];
        }
    }
}