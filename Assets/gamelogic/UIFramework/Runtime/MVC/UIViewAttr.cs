using System;

namespace GFramework.Extern
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class UIViewAttr : Attribute
    {
        public bool isSingleton = false;
        public bool isOverMask = false;
    }
}