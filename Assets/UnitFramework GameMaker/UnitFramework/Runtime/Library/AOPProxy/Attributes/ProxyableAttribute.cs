﻿using System;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 可装饰属性,只有挂载了当前属性的类才能创建代理
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]   
    public  sealed  class ProxyableAttribute : System.Attribute
    {
        
    }
}