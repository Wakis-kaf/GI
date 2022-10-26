using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnitFramework.Runtime.Archives
{
    public class UnitArchiveBehaviourWriterHandler : UnitBehaviourHandler<IUnit>
    {
        // public override void OnAssignUnitRegister(IUnit assignUnit)
        // {
        //     base.OnAssignUnitRegister(assignUnit);
        //  
        //     if(ReflectionTool.IsImplementedGenericInterface(typeof(IUnitArchiveWriter<>),
        //         assignUnit.GetType(),out Type genericInterfaceType))
        //     {
        //         // 如果实现了接口
        //        
        //         // 获取实现的泛型类
        //         if (ReflectionTool.TryGetGenericTypesDefinition(genericInterfaceType, out Type[] types))
        //         {
        //             foreach (var genericArgType in types)
        //             {
        //                 Log.Info($"{genericInterfaceType} {genericArgType} {assignUnit.GetType()}");
        //                 if (!ReferenceEquals(Game.Archive, null))
        //                 {
        //                     // TODO : 注册存档添加事件
        //                     //dynamic item = assignUnit;
        //                     dynamic item = Convert.ChangeType(assignUnit, genericInterfaceType);
        //                     Game.Archive.RegisterLoad(item.OnArchiveLoad);
        //                     //MethodInfo method = Game.Archive.GetType().GetMethod("RegisterLoad").MakeGenericMethod(new Type[] { genericArgType});
        //
        //                     //method?.Invoke(Game.Archive, assignUnit.OnArchiveLoad);
        //                     /*Game.Archive.RegisterLoad(item.OnArchiveLoad);
        //                     Game.Archive.RegisterSave(item.OnArchieSave);*/
        //                 }
        //             }
        //         }
        //     }
        //     
        // }

        public override void OnAssignUnitDeRegister(IUnit assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
        }
    }
}