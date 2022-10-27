using System.Collections.Generic;

namespace UnitFramework.Runtime
{
    public static class UnitCommon
    {

        public static void InitUnitEnableEvent(ref UnitEnabledEventArgs enabledEventArgs,IUnit owner)
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
            {
                enabledEventArgs = GameFramework.Event.CreateEventArgs<UnitEnabledEventArgs>(owner);
            }
        }
        public static void RegisterUnitToFrame(IUnit unit)
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Unit, null))
            {
                // 注册单元
                GameFramework.Unit.RegisterUnit(unit);
            }
        }
        public static void DeRegisterUnitFromFrame(IUnit unit)
        {
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Unit, null))
            {
                // 注销单元
                GameFramework.Unit.DeRegisterUnit(unit);
            }
        }
        public static void HandleUnitEnable(ref bool isUnitEnable, ref bool isUnitFirstEnabled,ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            if (isUnitEnable && isUnitFirstEnabled) return;
            isUnitEnable = true;
            isUnitFirstEnabled = true;
            DispatchUnitEnableEvent(ref unitEnabledEventArgs);
        }
        private static void DispatchUnitEnableEvent(ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            if (ReferenceEquals(unitEnabledEventArgs, null)) return;
            unitEnabledEventArgs.enable = true;
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
                GameFramework.Event.Dispatch((int) UnitHandleType.UnitEnable, unitEnabledEventArgs);
        }
        public static void HandleUnitDisable(ref bool isUnitEnable, ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            if (!isUnitEnable) return;
            isUnitEnable = false;
            DispatchUnitDisableEvent(ref unitEnabledEventArgs);
        }

        private static void DispatchUnitDisableEvent(ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            if (ReferenceEquals(unitEnabledEventArgs, null)) return;
            unitEnabledEventArgs.enable = false;
            if (!ReferenceEquals(GameFramework.Instance, null) && !ReferenceEquals(GameFramework.Event, null))
                GameFramework.Event.Dispatch((int) UnitHandleType.UnitDisable, unitEnabledEventArgs);
        }
        /// <summary>按照优先级进行降序排序/// </summary>
        public static int UnitSortCmp(IUnit unit1, IUnit unit2)
        {
            return unit1.UnitPriority < unit2.UnitPriority ? 1 : -1;
        }

        public static IUnit SetParent(IUnit child, IUnit newParent)
        {
            return child.SetParent(newParent);
        }
        public static IUnit SetParent(IUnit child, IUnit newParent, ref IUnit oldParent)
        {
            if (ReferenceEquals(newParent, oldParent)) return newParent;
            if (!ReferenceEquals(oldParent, null)) RemoveChildUnit(oldParent,child);
            if (!ReferenceEquals(newParent, null)) AddChildUnit(newParent, child);
            oldParent = newParent;
            return oldParent;
        }

        public static void RemoveChildUnit(IUnit owner, IUnit child)
        {
            int index = ChildIndexOf(owner, child);
            if (index != -1)
            {
                RemoveChildUnitAt(owner, index);
                SetParent(child, null);
            }
        }
        public static void RemoveChildUnitAt(IUnit parent, int index)
        {
            parent.RemoveChildUnitAt(index);
        }
        public static void RemoveChildUnitAt(ref List<IUnit> mChildUnits, int index)
        {
            if (index >= mChildUnits.Count) return;
            mChildUnits.RemoveAt(index);
        }

        public static void RemoveChildUnit<T>(IUnit owner) where T : IUnit
        {
            owner.RemoveChildUnit<T>();
        }
        public static void RemoveChildUnit<T>(IUnit owner,ref List<IUnit> childs) where T : IUnit
        {
            int count = childs.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var child = childs[i];
                if (child is T res)
                {
                    childs.RemoveAt(i);
                    SetParent(child, null);
                }
            }
        }

        public static int ChildIndexOf(IUnit parent, IUnit child)
        {
            return parent.ChildIndexOf(child);
        }

        public static int ChildIndexOf(ref List<IUnit> childs, IUnit child)
        {
            return childs.IndexOf(child);
        }

   
     

        public static T AddChildUnit<T>(IUnit owner) where T : IUnit
        {
            T child = (T) System.Activator.CreateInstance(typeof(T));
            return AddChildUnit<T>(owner, child);
        }
        public static T AddChildUnit<T>(IUnit parent, T child) where T : IUnit
        {
            return parent.AddChildUnit(child);
        }

        public static T AddChildUnit<T>(IUnit parent, T child, ref List<IUnit> childs) where T : IUnit
        {
            if (parent.HasChild(child)) return child;
            childs.Add(child);
            if (child.UnitPriority != 0)
            {
                childs.Sort(UnitSortCmp);
            }

            child.SetParent(parent);
            return child;
        }

        public static bool HasChild(ref List<IUnit> childs, IUnit child)
        {
            return childs.Contains(child);
        }

        public static bool TryGetUnit<T>(IUnit owner,out T find) where T : IUnit
        {
            return owner.TryGetUnit(out find);
        }
        public static bool TryGetUnit<T>(ref List<IUnit> childs, out T unit) where T : IUnit
        {
            int count = childs.Count;
            unit = default;
            for (int i = 0; i < count; i++)
            {
                if (childs[i] is T res)
                {
                    unit = res;
                    return true;
                }
            }

            return false;
        }

        public static T GetUnit<T>(IUnit unit) where T : IUnit
        {
            TryGetUnit(unit,out T find);
            return find;
        }

        public static bool TryGetUnits<T>(IUnit owner, out T[] findUnits) where T : IUnit
        {
            return owner.TryGetUnits<T>(out findUnits);
        }
        public static bool TryGetUnits<T>(ref List<IUnit> units, out T[] findUnits) where T : IUnit
        {
            List<T> result = new List<T>();
            int count = units.Count;
            for (int i = 0; i < count; i++)
            {
                if (units[i] is T res)
                {
                    result.Add(res);
                }
            }
            findUnits = result.ToArray();
            return true;
        }

        public static void DisposeManagedResources(ref List<IUnit> mChildUnits, IUnit owner)
        {
            DisposeChilds(mChildUnits);
            SetParent(owner, null);
        }

        private static void DisposeChilds(List<IUnit> childs)
        {
            int count = childs.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                childs[i].Dispose();
            }
        }

        public static void DisposeUnManagedResources(ref List<IUnit> childs, ref UnitEnabledEventArgs unitEnabledEventArgs)
        {
            childs.Clear();
            childs = null;
            unitEnabledEventArgs = null;
        }


       
    }
}