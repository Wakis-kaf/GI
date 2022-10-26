using System;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 扩展基础
    /// 如果想要被扩展模块自动拾取并注册需要 实现 该接口并挂载 ExtendAttribute
    /// </summary>
    public interface IHelperBase : IDisposable
    {
        
    }
}