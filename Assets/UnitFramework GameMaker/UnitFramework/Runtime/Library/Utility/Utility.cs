﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnitFramework.Utils
{
    public  static partial class Utility
    {
        /// <summary>程序集相关的实用函数。</summary>
        public static class Assembly
        {
            private static readonly System.Reflection.Assembly[] s_Assemblies = (System.Reflection.Assembly[]) null;
    
            private static readonly Dictionary<string, Type> s_CachedTypes =
                new Dictionary<string, Type>((IEqualityComparer<string>) StringComparer.Ordinal);
    
            static Assembly()
            {
                Utility.Assembly.s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
    
            /// <summary>获取已加载的程序集。</summary>
            /// <returns>已加载的程序集。</returns>
            public static System.Reflection.Assembly[] GetAssemblies()
            {
                return Utility.Assembly.s_Assemblies;
            }
    
            /// <summary>获取已加载的程序集中的所有类型。</summary>
            /// <returns>已加载的程序集中的所有类型。</returns>
            public static Type[] GetTypes()
            {
                List<Type> typeList = new List<Type>();
                foreach (System.Reflection.Assembly assembly in Utility.Assembly.s_Assemblies)
                    typeList.AddRange((IEnumerable<Type>) assembly.GetTypes());
                return typeList.ToArray();
            }
    
            /// <summary>获取已加载的程序集中的所有类型。</summary>
            /// <param name="results">已加载的程序集中的所有类型。</param>
            public static void GetTypes(List<Type> results)
            {
                if (results == null)
                    throw new Exception("Results is invalid.");
                results.Clear();
                foreach (System.Reflection.Assembly assembly in Utility.Assembly.s_Assemblies)
                    results.AddRange((IEnumerable<Type>) assembly.GetTypes());
            }
    
            /// <summary>获取已加载的程序集中的指定类型。</summary>
            /// <param name="typeName">要获取的类型名。</param>
            /// <returns>已加载的程序集中的指定类型。</returns>
            public static Type GetType(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                    throw new Exception("Type name is invalid.");
                Type type1 = (Type) null;
                if (Utility.Assembly.s_CachedTypes.TryGetValue(typeName, out type1))
                    return type1;
                Type type2 = Type.GetType(typeName);
                if (type2 != null)
                {
                   
                    Utility.Assembly.s_CachedTypes.Add(typeName, type2);
                    return type2;
                }
    
                foreach (System.Reflection.Assembly assembly in Utility.Assembly.s_Assemblies)
                {
                    Type type3 = Type.GetType(string.Format("{0}, {1}", (object) typeName, (object) assembly.FullName));
                    
                    if (type3 != null)
                    {
                        Utility.Assembly.s_CachedTypes.Add(typeName, type3);
                        return type3;
                    }
                }
    
                return (Type) null;
            }
        }
        
    }
}