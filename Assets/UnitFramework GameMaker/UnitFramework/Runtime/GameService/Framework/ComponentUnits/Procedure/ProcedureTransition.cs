using System;

namespace UnitFramework.Runtime
{
    public abstract class ProcedureTransition<From, To> : IProcedureTransition
        where From : Procedure 
        where To : Procedure
    {
        private Type m_ProcedureTo;
        private Type m_ProcedureFrom;
        public int priority { get; set; } = 1;
        public bool enable { get; set; } = true;
        public Type procedureTo { get=>m_ProcedureTo; }
        public Type procedureFrom { get=>m_ProcedureFrom;}
        
        public ProcedureTransition()
        {
            m_ProcedureFrom = typeof(From);
            m_ProcedureTo = typeof(To);
        }

        public virtual void OnFrameStart()
        {
          
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnShutdown()
        {
            
        }

        public abstract bool IsTransitionReady();

    }
}