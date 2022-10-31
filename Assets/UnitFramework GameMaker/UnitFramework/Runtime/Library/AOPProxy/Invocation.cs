﻿﻿using System;
using System.Reflection;
using UnityEngine;

namespace UnitFramework.Runtime
{
    /// <summary>
    /// 调用者
    /// </summary>
    public class Invocation
    {
        public Invocation(object instance,MethodInfo targetMethodInfo)
        {
         
            BindInstance(instance);
            BingBaseMethod(targetMethodInfo);
        }

        public void BindInstance(object instance)
        {
            this.instance = instance;
        }

        public void BingBaseMethod(MethodInfo targetMethodInfo)
        {
            this.targetMethodInfo = targetMethodInfo;
        }
        public object[] Parameter { get;  set; }
        public Delegate DelegateMethod { get; set; }
        public Type MethodInvocationTarget { get;  set; }
        public Type TargetType { get; set; }
        public object instance { get;private set; }
        public MethodInfo targetMethodInfo { get;private set; }
        private Type nextCallerType = typeof(Func<Invocation, object>);
        public object Proceed()
        {
            Debug.Log($"方法调用{DelegateMethod?.GetType()} 目标类型{targetMethodInfo}");
            
            //return this.DelegateMethod.DynamicInvoke(Parameter);
            //this.DelegateMethod.DynamicInvoke(Parameter);
#if !ILDebug

            if (DelegateMethod.GetType() == nextCallerType)
            {
                return this.DelegateMethod.DynamicInvoke(Parameter);
            }
            else
            {
                return targetMethodInfo?.Invoke(instance,Parameter);
            }
#else
    return this.DelegateMethod.DynamicInvoke(Parameter);
#endif
           
        }
    }
}