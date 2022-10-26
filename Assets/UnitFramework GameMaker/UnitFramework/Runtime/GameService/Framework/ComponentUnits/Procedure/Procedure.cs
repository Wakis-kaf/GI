﻿using System;

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

        public bool first = true;

        public Procedure()
        {
            type = GetType();
            typeStr = type.ToString();
        }
        ~Procedure()
        {
            Dispose(false);
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
    }
}