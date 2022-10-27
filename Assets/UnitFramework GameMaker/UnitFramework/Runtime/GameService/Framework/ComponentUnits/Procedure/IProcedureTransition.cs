using System;

namespace UnitFramework.Runtime
{
    public interface IProcedureTransition
    {
        Type procedureFrom { get; }
        Type procedureTo { get; }
        int priority { get; set; }
        bool enable { get; set; }
        void OnFrameStart();
        void OnUpdate();
        void OnShutdown();
        
        bool IsTransitionReady();
    }
}