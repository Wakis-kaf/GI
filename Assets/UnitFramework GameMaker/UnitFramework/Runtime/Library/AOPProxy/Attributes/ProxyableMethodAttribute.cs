﻿using System;

namespace UnitFramework.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]   
    public sealed class ProxyableMethodAttribute : System.Attribute
    {
        public string proxyMethodKey { get; private set; }

        public ProxyableMethodAttribute(string methodProxyKey)
        {
            proxyMethodKey = methodProxyKey;
        }
    }
}