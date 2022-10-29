using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GFramework.UI
{
    [Serializable]
    public class UIVar
    {
        // 字段名字
        public string fieldName;
        public GameObject gameObject;
        public UnityEngine.Component component;
    }
}