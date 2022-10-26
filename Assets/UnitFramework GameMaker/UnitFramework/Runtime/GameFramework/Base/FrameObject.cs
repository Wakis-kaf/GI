using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public abstract class FrameObject : IFrameObject
    {
        private Type m_Type;
        private bool m_Disposed;
        
        public Type Type => m_Type;
        public bool Disposed => m_Disposed;

        public FrameObject()
        {
            m_Type = GetType();
        }
        ~FrameObject()
        {
            Dispose(false);
        }
       
        /// <summary>资源回收实现细节 </summary>
        /// <param name="isDisposeManagedResources">是否回收托管资源</param>
        protected virtual void Dispose(bool isDisposeManagedResources)
        {
            if (m_Disposed) return;
            m_Disposed = true;
            if (isDisposeManagedResources) 
                DisposeManagedResources();
            DisposeUnManagedResources();
        }
        /// <summary>
        /// 回收托管堆资源
        /// </summary>
        protected virtual void DisposeManagedResources() { }
        /// <summary>
        /// 回收未被托管堆管理的资源
        /// </summary>
        protected virtual void DisposeUnManagedResources() { }
        
        /// <summary>
        ///  手动调用回收资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    
    }
}
