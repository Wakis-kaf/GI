using UnitFramework.Utils;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public partial class GameService : SingletonMonoUnit<GameService>, IUnitAwake
    {
        public override int UnitPriority => 1000;

        public void OnUnitAwake()
        {
            ComponentUnitInit();
        }

        private void ComponentUnitInit()
        {
            // 获取所有的服务组件
            ComponentUnit[] componentUnits = GetAllComponentUnit();
            foreach (var componentUnit in componentUnits)
            {
                AddChildUnit(componentUnit);
            }
        }

        private ComponentUnit[] GetAllComponentUnit()
        {
            return Utility.UnityTransform.GetComponentsInChild<ComponentUnit>(transform);
        }

        public static T GetUnitFromFrame<T>() where T : IUnit
        {
            return Instance.GetUnit<T>();
        }
    }

    public partial class GameService
    {
        public static AudioComponent Audio => Instance.GetUnit<AudioComponent>();
        public static InputComponent Input => Instance.GetUnit<InputComponent>();
        public static ObjectPoolComponent ObjectPool => Instance.GetUnit<ObjectPoolComponent>();

        public static ArchiveSystem Archive => Instance.GetUnit<ArchiveSystem>();
        public static AssetsLoadMgr Asset => Instance.GetUnit<AssetsLoadMgr>();
        public static FileComponent File => Instance.GetUnit<FileComponent>();
        public static SettingComponent Setting => Instance.GetUnit<SettingComponent>();
        public static ProcedureComponent Procedure => Instance.GetUnit<ProcedureComponent>();
        public static DebuggerComponent DebuggerComponent => Instance.GetUnit<DebuggerComponent>();
    }
}