using System;

namespace GFramework.UI
{
    public class BaseViewModel
    {
        public IView bindingView;

        public virtual void Init<T>(T data) where T : BaseData
        {

        }
    }
}