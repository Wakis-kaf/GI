using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Utils;

namespace UnitFramework.Runtime
{
    public class SingletonUnit<T> : Unit where T :class
    {
        private static T m_Instance;
        public static T Instance => m_Instance;
        public static T CreateInstance()
        {
            if (!ReferenceEquals(m_Instance, null)) return m_Instance;
            m_Instance =  ReflectionTool.CreateInstance<T>();
            return m_Instance;
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            if (ReferenceEquals(m_Instance, this)) m_Instance = null;
        }
    }
}
