using System;

namespace UnitFramework.Runtime
{
    public abstract class Procedure : FrameObject
    {
        public Type type
        {
            get;
            private set;
        }
        public string typeStr
        {
            get;
            private set;
        }
        /// <summary>
        /// 是否启用该流程
        /// </summary>
        public bool enable = true;

        private bool m_HasNotEntered = true;

        public bool HasNotEntered => m_HasNotEntered;
        public Procedure()
        {
            type = GetType();
            typeStr = type.ToString();
        }
        public virtual void OnFirstEnter(ProcedureComponent procedureComponent)
        {
            
        }
        public virtual void OnEnter(ProcedureComponent procedureComponent)
        {
        }

        public virtual void OnStay(ProcedureComponent procedureComponent)
        {
        }

        public virtual void OnExit(ProcedureComponent procedureComponent)
        {
        }

        public virtual void OnUnLoad(ProcedureComponent procedureComponent)
        {
            
        }

        public virtual void OnLoad(ProcedureComponent procedureComponent)
        {
            
        }

        public virtual void OnQuit(ProcedureComponent procedureComponent)
        {
            
        }

        public void EnteredMark()
        {
            m_HasNotEntered = false;
        }
    }
}