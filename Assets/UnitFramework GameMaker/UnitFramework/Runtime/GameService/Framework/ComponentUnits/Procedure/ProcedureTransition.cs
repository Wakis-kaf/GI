using System;

namespace UnitFramework.Runtime
{
    public abstract class ProcedureTransition<From, To> : IProcedureTransition
        where From : Procedure 
        where To : Procedure
    {
        public Type procedureFrom { get=>m_ProcedureFrom;}
        public Type procedureTo { get=>m_ProcedureTo; }

        public int priority { get; set; } = 1;
        public bool enable { get; set; } = true;

        private Type m_ProcedureFrom;
        private Type m_ProcedureTo;
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