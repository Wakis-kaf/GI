using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitFramework.Runtime
{
    public abstract class Module : FrameObject, IModule
    {
        public virtual int Priority => 1;
        public virtual void OnFrameStart() { }
        public virtual void OnFrameUpdate() { }
        public virtual void OnFrameFixedUpdate() { }
        public virtual void OnFrameworkShutdown() { }
        
    }
}
