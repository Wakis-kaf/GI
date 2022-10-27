using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GFramework.UI
{
    /// <summary>
    /// 可绑定model接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBinding<T>
    {
        T BindingContext
        {
            get; set;
        }
    }
}