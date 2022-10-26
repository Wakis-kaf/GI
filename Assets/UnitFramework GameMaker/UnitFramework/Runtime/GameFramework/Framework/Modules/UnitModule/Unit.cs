using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public partial class Unit : FrameObject, IUnit
    {
        public Unit()
        {
            m_ChildUnits = new List<IUnit>();
            m_UnitEnabledEventArgs = GameFramework.Event.CreateEventArgs<UnitEnabledEventArgs>(this);
            GameFramework.Unit.RegisterUnit(this);
            UnitEnableCheck();
        }

        private void UnitEnableCheck()
        {
            if (m_IsUnitEnable && !m_IsFirstEnabled)
            {
                UnitEnable();
                m_IsFirstEnabled = true;
            }
        }
        public void UnitEnable()
        {
            if (m_IsUnitEnable && m_IsFirstEnabled) return;
            m_IsUnitEnable = true;
            m_IsFirstEnabled = true;
            m_UnitEnabledEventArgs.enable = true;
            GameFramework.Event.Dispatch((int) UnitHandleType.UnitEnable, m_UnitEnabledEventArgs);
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
            // 注销单元
            GameFramework.Unit.DeRegisterUnit(this);
        }

        public void UnitDisable()
        {
            if (!m_IsUnitEnable) return;
            m_IsUnitEnable = false;
            m_UnitEnabledEventArgs.enable = false;
            GameFramework.Event.Dispatch((int) UnitHandleType.UnitDisable, m_UnitEnabledEventArgs);
        }

      
    }

    /// <summary>
    /// Mono Unit and unit public field
    /// </summary>
    public partial class Unit
    {
        private bool m_IsUnitEnable = true;
        private bool m_IsFirstEnabled = false;
        private List<IUnit> m_ChildUnits;
        private UnitEnabledEventArgs m_UnitEnabledEventArgs;
        private IUnit m_Parent;

        public IUnit OwnerUnit
        {
            get => this;
        }

        public virtual string UnitName => "EKF Unit";

        public virtual int UnitPriority
        {
            get => 0;
        }

        public int ChildCount => m_ChildUnits.Count;
        public bool IsUnitEnable => m_IsUnitEnable;

        public IUnit Parent
        {
            get => m_Parent;
        }


        /// <summary>
        /// 按照优先级进行降序排序
        /// </summary>
        /// <param name="unit1"></param>
        /// <param name="unit2"></param>
        /// <returns></returns>
        private int UnitSortCmp(IUnit unit1, IUnit unit2)
        {
            return unit1.UnitPriority < unit2.UnitPriority ? 1 : -1;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            int count = ChildCount;
            for (int i = count - 1; i >= 0; i--)
            {
                m_ChildUnits[i].Dispose();
            }
        }

        protected override void DisposeUnManagedResources()
        {
            base.DisposeUnManagedResources();
            m_ChildUnits.Clear();
            m_UnitEnabledEventArgs = null;
        }

        /// <summary>
        /// 设置父单元
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IUnit SetParent(IUnit parent)
        {
            if (ReferenceEquals(parent, m_Parent)) return parent;
            if (m_Parent != null) m_Parent.RemoveChildUnit(this);
            if (parent != null) parent.AddChildUnit(this);
            m_Parent = parent;
            return m_Parent;
        }

        /// <summary>
        /// 尝试获取单元
        /// </summary>
        /// <param name="unit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 获取单元
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUnit<T>() where T : IUnit
        {
            TryGetUnit<T>(out T res);
            return res;
        }

        /// <summary>
        /// 获取单元
        /// </summary>
        /// <param name="units"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 添加子单元 
        /// </summary>
        /// <param name="unit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 移除子单元
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// 移除子单元
        /// </summary>
        /// <param name="unit"></param>
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