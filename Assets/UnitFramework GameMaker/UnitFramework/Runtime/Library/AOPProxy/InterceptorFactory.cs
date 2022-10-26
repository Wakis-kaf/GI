﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public static  class InterceptorFactory
    {
        private static Dictionary<Type,IInterceptor> m_Type2Interceptor = new Dictionary<Type, IInterceptor>();
        public static IInterceptor Create(Type type)
        {
            //Debug.Log( "尝试创建拦截器: " +type);
            if (m_Type2Interceptor.ContainsKey(type))
            {
                return m_Type2Interceptor[type];
            }
            Debug.Log( "创建拦截器: " +type);
            IInterceptor interceptor = (IInterceptor) System.Activator.CreateInstance(type);
            m_Type2Interceptor.Add(type,interceptor);
            return interceptor;
        }
    }
}