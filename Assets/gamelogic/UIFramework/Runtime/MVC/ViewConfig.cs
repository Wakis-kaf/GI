using System;

namespace UGFramework.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ViewConfig : Attribute
    {
        public bool isSingleton = false;
        public bool isOverMask = false;
    }
}