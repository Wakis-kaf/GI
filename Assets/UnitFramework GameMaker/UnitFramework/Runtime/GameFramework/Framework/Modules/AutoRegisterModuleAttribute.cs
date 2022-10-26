using System;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 添加了AutoRegisterModule的Module 类在框架启动时会自动注册到框架的模块中
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]    
    
    public class AutoRegisterModuleAttribute : System.Attribute
    {
    }
}