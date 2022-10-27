using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public abstract class MonoFrameObject : MonoBehaviour, IFrameObject
    {
        private Type m_Type;
        private bool m_Disposed;
        
        public Type Type => m_Type;
        public bool Disposed => m_Disposed;

        public MonoFrameObject()
        {
            m_Type = GetType();
        }
       
        protected virtual void OnDestroy()
        {
            try
            {
                Dispose(false);
            }
            catch (Exception e)
            {
                Log.FatalFormat("MonoUnit Dispose  Error {0}", e.Message);
            }
        }
        /// <summary>
        /// 资源回收
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>资源回收实现细节 </summary>
        /// <param name="isDisposeManagedResources">是否回收托管资源</param>
        protected virtual void Dispose(bool isDisposeManagedResources)
        {
            if (m_Disposed) return;
            m_Disposed = true;
            if (isDisposeManagedResources)
            {
                Destroy(gameObject);
                DisposeManagedResources();
            }
            DisposeUnManagedResources();
        }
        
        /// <summary>
        /// 回收托管堆资源
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }
       
        /// <summary>
        /// 回收非托管堆资源
        /// </summary>
        protected virtual void DisposeUnManagedResources()
        {
        }
        
      

      
    }
}