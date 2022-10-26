﻿using System;
using System.Collections.Generic;
 using UnitFramework.Utils;
 using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 代理工厂
    /// </summary>
    public static class ProxyCollections 
    {
        public static Dictionary<Type,Type> m_Type2proxiedType = new Dictionary<Type, Type>();
        // /// <summary>
        // /// 单例模式
        // /// </summary>
        // public static ProxyCollections Instance {
        //     get
        //     {
        //         if (ReferenceEquals(m_Instance, null))
        //         {
        //             m_Instance = new ProxyCollections();
        //         }
        //
        //         return m_Instance;
        //     }
        // }
        // private static ProxyCollections m_Instance;

        public static T CreateProxyInstance<T>() where  T : class
        {
            Type origin = typeof(T);
            BuildProxyType(origin, out Type proxiedType);
            return ReflectionTool.CreateInstance(proxiedType) as T;
        }
        public static T CreateProxyInstance<T>(params object[] objs) where  T : class
        {
            Type origin = typeof(T);
            BuildProxyType(origin, out Type proxiedType);
            return ReflectionTool.CreateInstance(proxiedType,objs) as T;
        }

        public static bool BuildProxyType(Type origin,out Type proxiedType)
        {

            if (m_Type2proxiedType.TryGetValue(origin, out proxiedType))
            {
                return true;
            }
            bool success =  ProxyBuilder.TryBuildProxyType(origin, out proxiedType);
            if (success)
            {
                m_Type2proxiedType.Add(origin,proxiedType);
            }
            
            return success;

        }
        /*public static bool TryGetProxiedType(Type origin,out Type proxiedType)
        {
            return type2proxiedType.TryGetValue(origin, out proxiedType);
        }*/
        public static void Clear()
        {
            m_Type2proxiedType.Clear();
        }
        
        
    }
}