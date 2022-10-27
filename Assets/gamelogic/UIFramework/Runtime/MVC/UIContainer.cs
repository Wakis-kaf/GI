using System;
using System.Collections.Generic;

using UnityEngine;

namespace GFramework.UI
{

    [Serializable]
    public class UIVar
    {
        // 字段名字
        public string fieldName;
        public GameObject gameObject;
        public Component component;
    }

    public class UIContainer : MonoBehaviour
    {
        public UILayer layer;
        public UINode node;
        [HideInInspector] public string bindingViewPath = null;
        [HideInInspector] public string bindingViewType = null;
        [HideInInspector] public List<UIVar> varsArr = new List<UIVar>();

        public UIVar GetVar(int index)
        {
            if (index >= this.varsArr.Count)
                return null;
            return this.varsArr[index];
        }
    }
}