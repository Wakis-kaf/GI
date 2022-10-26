using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public class SingletonMonoUnit<T> : MonoUnit where T : class
    {
        public static T Instance => mInstance;
        private static T mInstance;

        protected override void Awake()
        {
            if(!ReferenceEquals(mInstance,null)){
                Destroy(gameObject);
                return;
            }
            mInstance = this as T;
            base.Awake();
        }
        protected override void DisposeUnManagedRes()
        {
            base.DisposeUnManagedRes();
            if (ReferenceEquals(mInstance, this))
            {
                mInstance = null;
            }
        }

    }
   
}
