﻿﻿using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Utils
{
    public static partial class Utility
    {
        public static class UnityTransform
        {
            public static T[] GetComponentsInChild<T>(Transform transform ) where  T: MonoBehaviour
            {
                int childCount = transform.childCount;
                List<T> res = new List<T>();
                for (int i = 0; i < childCount; i++)
                {
                    foreach (var component in transform.GetChild(i).GetComponents<T>())
                    {
                        res.Add(component);
                    }
               
                }
                return res.ToArray();
            }

            public static void DestroyChilds(Transform transform)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    GameObject.Destroy(child.gameObject);
                }
            }
            public static void DestroyChildsImmediate(Transform transform)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
        }
    }

}