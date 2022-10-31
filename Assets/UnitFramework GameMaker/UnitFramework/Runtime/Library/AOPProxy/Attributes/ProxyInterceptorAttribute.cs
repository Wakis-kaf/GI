﻿﻿using System;
using System.Net.Sockets;

namespace UnitFramework.Runtime
{
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)] 
    public sealed class ProxyInterceptorAttribute : System.Attribute
    {
        public Type proxyableClassType { get; private set; }
        public string proxyMethodKey{ get; private set; }
        
        public ProxyInterceptorAttribute(Type proxyableClassType,string proxyMethodKey)
        {
            this.proxyableClassType = proxyableClassType;
            this.proxyMethodKey = proxyMethodKey;
        }
    }
}