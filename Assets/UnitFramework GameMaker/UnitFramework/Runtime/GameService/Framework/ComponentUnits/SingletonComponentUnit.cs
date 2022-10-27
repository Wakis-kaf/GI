using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class SingletonComponentUnit<T> : ComponentUnit where T : class
    {

        public static T Instance => m_Instance;
        private static T m_Instance;

        protected override void Awake()
        {
            if (!ReferenceEquals(m_Instance, null))
            {
                Destroy(gameObject);
                return;
            }

            m_Instance = this as T;
            base.Awake();
        }
        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            if (ReferenceEquals(m_Instance, this))
            {
                m_Instance = null;
            }
        }


    }

}

