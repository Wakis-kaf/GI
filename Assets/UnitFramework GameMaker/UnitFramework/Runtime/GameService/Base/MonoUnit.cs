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
            InitUnitEnableEvent();
            RegisterUnitToFrame();
        }

        private void InitUnitEnableEvent()
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
            {
                m_UnitEnabledEventArgs = GameFramework.Event.CreateEventArgs<UnitEnabledEventArgs>(this);
            }
        }

        private void RegisterUnitToFrame()
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Unit, null))
            {
                // 注册单元
                GameFramework.Unit.RegisterUnit(this);
            }
        }

        private void OnEnable()
        {
            UnitEnable();
        }

        public void UnitEnable()
        {
            if (m_IsUnitEnable && m_IsFirstEnabled) return;
            m_IsUnitEnable = true;
            m_IsFirstEnabled = true;
            DispatchUnitEnableEvent();
        }

        private void DispatchUnitEnableEvent()
        {
            if (ReferenceEquals(m_UnitEnabledEventArgs, null)) return;
            m_UnitEnabledEventArgs.enable = true;
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
                GameFramework.Event.Dispatch((int) UnitHandleType.UnitEnable, m_UnitEnabledEventArgs);
        }

        private void OnDisable()
        {
            UnitDisable();
        }

        public void UnitDisable()
        {
            if (!m_IsUnitEnable) return;
            m_IsUnitEnable = false;
            DispatchUnitDisableEvent();
        }

        private void DispatchUnitDisableEvent()
        {
            if (ReferenceEquals(m_UnitEnabledEventArgs, null)) return;
            m_UnitEnabledEventArgs.enable = false;
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
                GameFramework.Event.Dispatch((int) UnitHandleType.UnitDisable, m_UnitEnabledEventArgs);
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
            DeRegisterUnitFromFrame();
        }

        private void DeRegisterUnitFromFrame()
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Unit, null))
            {
                // 注销单元
                GameFramework.Unit.DeRegisterUnit(this);
            }
        }
    }

    public partial class MonoUnit
    {
        private bool m_IsUnitEnable = true;
        private bool m_IsFirstEnabled = false;
        private UnitEnabledEventArgs m_UnitEnabledEventArgs;
        private List<IUnit> m_ChildUnits = new List<IUnit>();
        public virtual string UnitName => "MonoUnit";
        public int ChildCount => m_ChildUnits.Count;


        public virtual int UnitPriority
        {
            get => 0;
        }

        public bool IsUnitEnable => m_IsUnitEnable;

        public IUnit OwnerUnit
        {
            get => this;
        }

        public IUnit Parent
        {
            get => m_Parent;
        }

        private IUnit m_Parent;

        public IUnit SetParent(IUnit parent)
        {
            if (ReferenceEquals(parent, m_Parent)) return parent;
            if (m_Parent != null) m_Parent.RemoveChildUnit(this);
            if (parent != null) parent.AddChildUnit(this);
            m_Parent = parent;
            return m_Parent;
        }


        protected override void DisposeManagedRes()
        {
            base.DisposeManagedRes();
            int count = ChildCount;
            for (int i = count - 1; i >= 0; i--)
            {
                m_ChildUnits[i].Dispose();
            }
        }

        protected override void DisposeUnManagedRes()
        {
            base.DisposeUnManagedRes();
            m_ChildUnits.Clear();
        }

        public bool TryGetUnit<T>(out T unit) where T : IUnit
        {
            int count = m_ChildUnits.Count;
            unit = default;
            for (int i = 0; i < count; i++)
            {
                if (m_ChildUnits[i] is T res)
                {
                    unit = res;
                    return true;
                }
            }

            return false;
        }

        public T GetUnit<T>() where T : IUnit
        {
            TryGetUnit<T>(out T res);
            return res;
        }

        public bool TryGetUnits<T>(out T[] units) where T : IUnit
        {
            List<T> result = new List<T>();
            int count = m_ChildUnits.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_ChildUnits[i] is T res)
                {
                    result.Add(res);
                }
            }

            units = result.ToArray();
            return true;
        }

        public T AddChildUnit<T>(T unit) where T : IUnit
        {
            if (m_ChildUnits.Contains(unit)) return unit;
            m_ChildUnits.Add(unit);
            if (unit.UnitPriority != 0)
            {
                m_ChildUnits.Sort(UnitSortCmp);
            }

            unit.SetParent(this);
            return unit;
        }

        /// <summary>
        /// 添加子单元
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddChildUnit<T>() where T : IUnit
        {
            T unit = (T) System.Activator.CreateInstance(typeof(T));
            return AddChildUnit<T>(unit);
        }

        // 按照优先级进行降序排序
        private int UnitSortCmp(IUnit unit1, IUnit unit2)
        {
            return unit1.UnitPriority < unit2.UnitPriority ? 1 : -1;
        }

        public void RemoveChildUnit<T>() where T : IUnit
        {
            int count = m_ChildUnits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (m_ChildUnits[i] is T res)
                {
                    m_ChildUnits.RemoveAt(i);
                    m_ChildUnits[i].SetParent(null);
                }
            }
        }

        public void RemoveChildUnit(IUnit unit)
        {
            int index = m_ChildUnits.IndexOf(unit);
            if (index != -1)
            {
                m_ChildUnits.RemoveAt(index);
                unit.SetParent(null);
            }
        }
    }
}