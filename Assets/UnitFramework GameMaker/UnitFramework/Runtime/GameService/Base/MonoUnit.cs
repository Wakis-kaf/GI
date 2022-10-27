using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public partial class MonoUnit : MonoFrameObject, IUnit
    {
        protected virtual void Awake()
        {
            UnitCommon.InitUnitEnableEvent(ref m_UnitEnabledEventArgs, this);
            UnitCommon.RegisterUnitToFrame(this);
        }

        private void OnEnable()
        {
            UnitEnable();
        }

        public void UnitEnable()
        {

            UnitCommon.HandleUnitEnable(ref m_IsUnitEnable, ref m_IsFirstEnabled, ref m_UnitEnabledEventArgs);
        }

        private void OnDisable()
        {
            UnitDisable();
        }

        public void UnitDisable()
        {

            UnitCommon.HandleUnitDisable(ref m_IsUnitEnable, ref m_UnitEnabledEventArgs);
        }
        

        protected override void Dispose(bool isDisposeManagedResources)
        {
            if (Disposed) return;
            // 如果当前为启用 禁用
            if (m_IsUnitEnable)
            {
                UnitDisable();
            }

            base.Dispose(isDisposeManagedResources);
            UnitCommon.DeRegisterUnitFromFrame(this);
        }
    }

    public partial class MonoUnit
    {
        private bool m_IsUnitEnable = true;
        private bool m_IsFirstEnabled = false;
        private List<IUnit> m_ChildUnits = new List<IUnit>();
        private UnitEnabledEventArgs m_UnitEnabledEventArgs;
        private IUnit m_Parent;
        public IUnit OwnerUnit => this;
        public IUnit Parent => m_Parent;
        public virtual int UnitPriority => 0;
        public virtual string UnitName => "Unit";
        public int ChildCount => m_ChildUnits.Count;
        public bool IsUnitEnable => m_IsUnitEnable;
    }

    /// <summary>
    /// Mono Unit and unit public function
    /// </summary>
    public partial class MonoUnit
    {
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            UnitCommon.DisposeManagedResources(ref m_ChildUnits, this);
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            UnitCommon.DisposeUnManagedResources(ref m_ChildUnits, ref m_UnitEnabledEventArgs);
        }

        public bool TryGetUnits<T>(out T[] units) where T : IUnit
        {

            return UnitCommon.TryGetUnits<T>(ref m_ChildUnits, out units);
        }

        public bool TryGetUnit<T>(out T unit) where T : IUnit
        {
       
            return UnitCommon.TryGetUnit<T>(ref m_ChildUnits, out unit);
        }

        public IUnit SetParent(IUnit parent)
        {
         
            return UnitCommon.SetParent(this, parent, ref m_Parent);
        }

        public void RemoveChildUnit(IUnit unit)
        {
         
            UnitCommon.RemoveChildUnit(this, unit);
        }

        public void RemoveChildUnit<T>() where T : IUnit
        {
           
            UnitCommon.RemoveChildUnit<T>(this, ref m_ChildUnits);
        }

        public void RemoveChildUnitAt(int index)
        {
            UnitCommon.RemoveChildUnitAt(ref m_ChildUnits, index);
        }

        public int ChildIndexOf(IUnit unit)
        {
            return UnitCommon.ChildIndexOf(ref m_ChildUnits, unit);
        }

        public T AddChildUnit<T>() where T : IUnit
        {
            
            return UnitCommon.AddChildUnit<T>(this);
        }

        public T AddChildUnit<T>(T unit) where T : IUnit
        {
           
            return UnitCommon.AddChildUnit(this, unit, ref m_ChildUnits);
        }

        public bool HasChild(IUnit child)
        {
          
            return UnitCommon.HasChild(ref m_ChildUnits, child);
        }

        public T GetUnit<T>() where T : IUnit
        {

            return UnitCommon.GetUnit<T>(this);
        }
    }
}