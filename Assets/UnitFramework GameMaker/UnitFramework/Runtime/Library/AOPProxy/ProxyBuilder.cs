﻿//#define ILDebug 
//#define FieldChunkDisable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnitFramework.Utils;
using UnityEngine;
// using Label = System.Reflection.Emit.Label;
// using Object = System.Object;


namespace UnitFramework.Runtime
{
    /// <summary>
    /// 代理构建类
    /// </summary>
    public static class ProxyBuilder
    {
        /// <summary>
        /// 代理方法在代理中的字段数据
        /// </summary>
        struct MethodFieldData
        {
            public FieldBuilder proxyMethodField;
            public FieldBuilder interceptorsField;
            public FieldBuilder invocationChunkField;
            public FieldBuilder paramTypesField;
            public FieldBuilder baseMethodDelegateField;
        }

        /// <summary>
        /// 程序集名称
        /// </summary>
        private static AssemblyName aseemblyName = new AssemblyName("DynamicAssembly");

        /// <summary>
        /// 在内存中保存好存放代理类的动态程序集
        /// </summary>
        private static AssemblyBuilder assyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(aseemblyName, AssemblyBuilderAccess.Run);

        /// <summary>
        /// 在内存中保存好存放代理类的托管模块
        /// </summary>
        private static ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(aseemblyName.Name);

        private static Dictionary<MethodInfo, MethodFieldData> m_Method2FieldDatas
            = new Dictionary<MethodInfo, MethodFieldData>();

        private static Dictionary<MethodInfo, Type[]> m_Method2ParamsType
            = new Dictionary<MethodInfo, Type[]>();

        private static Dictionary<ConstructorBuilder, Type[]> m_CB2ParamTypes
            = new Dictionary<ConstructorBuilder, Type[]>();

        //private static List<Type> m_ProxyType;
        private static string BaseMethodCallerPrefix = "_Proxy_";

        // /// <summary>
        // /// 代理器初始化
        // /// </summary>
        // public static void Init()
        // {
        //    
        // }
        //
        // /// <summary>
        // /// 代理关闭
        // /// </summary>
        // public static void Close()
        // {
        //     m_ProxyType.Clear();
        //     m_ProxyType = null;
        // }

        /// <summary>
        /// 创建代理类实例
        /// 如果目标是代理对象就创建代理,否则就创建传入时类型的的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object CreateProxyInstance<T>()
        {
            Type proxyType;
            Type originType = typeof(T);
            TryBuildProxyType(originType, out proxyType);
            return ReflectionTool.CreateInstance(proxyType);
        }

        /// <summary>
        /// 创建代理类实例
        /// </summary>
        /// <param name="objs">构造传参</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object CreateProxyInstance<T>(params object[] objs)
        {
            Type proxyType;
            Type originType = typeof(T);
            TryBuildProxyType(originType, out proxyType);
            return ReflectionTool.CreateInstance(proxyType, objs);
        }


        /// <summary>
        /// 尝试构建某个类的代理类
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="proxyType"></param>
        /// <returns></returns>
        public static bool TryBuildProxyType(Type origin, out Type proxyType)
        {
            proxyType = origin;
            // 该类不可代理
            if (!IsProxyAble(origin)) return false;
            // 创建 TypeBuilder
            CreateTypeBuilder(origin, out TypeBuilder proxyTypeBuilder);
            // 创建和父类一样的构造函数
            ConstructorBuilder[] cbs = CreateConstructorBuilders(proxyTypeBuilder, origin);
            // 对方法进行代理 
            try
            {
                BuildProxyMethods(origin, proxyTypeBuilder, cbs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError($"Build proxy {origin} method  Error! {e.Message}");
                //throw;
                return false;
            }

            //创建代理类类型
            proxyType = proxyTypeBuilder.CreateType();
            // 保存proxyType
            //SaveProxyType(origin,proxyType);
            return true;
        }


        // /// <summary>
        // /// 是否是已经构建过的代理类型
        // /// </summary>
        // /// <param name="type"></param>
        // /// <returns></returns>
        // public static bool IsProxiedType(Type type)
        // {
        //     return m_ProxyType.Contains(type);
        // }

        /// <summary>
        /// 是否是代理类型对象
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsProxiedType(object instance)
        {
            return IsProxiedType(instance.GetType());
        }

        /// <summary>
        /// 判断类型是否可以被代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static bool IsProxyAble(Type type)
        {
            if (!Attribute.IsDefined(type, typeof(ProxyableAttribute)))
            {
                Log.DebugInfo(
                    $"Type: {type} cant be proxied,target type does not have {typeof(ProxyableAttribute)} attributes attached !");
                //Debug.Log($"Type: {type} cant be proxied,target type does not have {typeof(ProxyableAttribute)} attributes attached !");

                return false;
            }

            if (!type.IsClass)
            {
                throw new Exception($"Type: {type} cant be proxied,target type is not a class ! ");
            }

            if (type.IsSealed)
            {
                throw new Exception($"Type: {type} cant be proxied,target type is a sealed  class ! ");
            }

            return true;
        }

        /// <summary>
        /// 判断代理是否有拦截者实现
        /// </summary>
        /// <param name="interceptorType"></param>
        /// <param name="proxyableType"></param>
        /// <param name="proxyableMethodTypeAttr"></param>
        /// <returns></returns>
        private static bool IsProxyInterceptor(Type interceptorType, Type proxyableType, string proxyMethodKey)
        {
            var invocationAttrs =
                CustomAttributeExtensions.GetCustomAttributes<ProxyInterceptorAttribute>(interceptorType);
            int count = invocationAttrs.Count();
            for (int i = 0; i < count; i++)
            {
                var invocationAttr = invocationAttrs.ElementAt(i);
                if (ReferenceEquals(invocationAttr, null)) return false;
                if (invocationAttr.proxyableClassType == proxyableType &&
                    invocationAttr.proxyMethodKey == proxyMethodKey)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 构建TypeBuilder
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="proxyTypeBuilder"></param>
        /// <exception cref="Exception"></exception>
        private static void CreateTypeBuilder(Type baseType, out TypeBuilder proxyTypeBuilder)
        {
            // 判断是否是接口
            if (baseType.IsInterface)
            {
                throw new Exception("cannot create a proxy class for the interface");
            }

            // 创建代理类型 
            Type TypeOfParent = baseType;
            Type[] TypeOfInterfaces = new Type[0];
            // 创建一个新的新的类型
            TypeBuilder typeBuilder =
                modBuilder.DefineType($"{baseType.Name}_Proxy_{Guid.NewGuid().ToString("N")}",
                    TypeAttributes.Class |
                    TypeAttributes.Public |
                    TypeAttributes.BeforeFieldInit,
                    TypeOfParent,
                    TypeOfInterfaces);
            proxyTypeBuilder = typeBuilder;
        }

        /// <summary>
        /// 构建和父类相同的构造函数
        /// 在每个构造函数中对代理所需要的字段进行初始化
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        private static ConstructorBuilder[] CreateConstructorBuilders(TypeBuilder typeBuilder, Type origin)
        {
            ConstructorInfo[] constructorInfos = origin.GetConstructors();
            ConstructorBuilder[] cbs = new ConstructorBuilder[constructorInfos.Length];
            for (int i = 0; i < constructorInfos.Length; i++)
            {
                ConstructorInfo constructorInfo = constructorInfos[i];
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                Type[] paramsType = new Type[parameterInfos.Length];
                for (int j = 0; j < paramsType.Length; j++)
                {
                    // 获取类型
                    paramsType[i] = parameterInfos[i].ParameterType;
                }

                ConstructorBuilder cb = typeBuilder.DefineConstructor(MethodAttributes.Public,
                    CallingConventions.Any, paramsType);
                // save param types
                m_CB2ParamTypes.Add(cb, paramsType);
                cbs[i] = cb;
            }

            return cbs;
        }

        /// <summary>
        /// 创建代理方法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="typeBuilder"></param>
        /// <param name="cbs"></param>
        /// <returns></returns>
        private static bool BuildProxyMethods(Type origin, TypeBuilder typeBuilder, ConstructorBuilder[] cbs)
        {
            // 获取所有可以代理方法
            GetProxyableMethods(origin,
                out List<MethodInfo> methodInfos,
                out Dictionary<MethodInfo, Type[]> proxyMethodInterceptors);
            if (methodInfos.Count == 0) return false;
            // 对获取到的方法进行重写,生成代理方法
            BuildProxyMethods(typeBuilder, cbs, methodInfos, proxyMethodInterceptors);
            ClearProxyMethodsBuildInfo(methodInfos, cbs);
            return true;
        }

        private static void ClearProxyMethodsBuildInfo(List<MethodInfo> methodInfos, ConstructorBuilder[] cbs)
        {
            for (int i = 0; i < methodInfos.Count; i++)
            {
                var method = methodInfos[i];
                m_Method2FieldDatas.Remove(method);
                m_Method2ParamsType.Remove(method);
            }

            for (int i = 0; i < cbs.Length; i++)
            {
                m_CB2ParamTypes.Remove(cbs[i]);
            }
        }

        /// <summary>
        /// 实现对方法的代理
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="cbs">被代理类的所有构造函数构建器</param>
        /// <param name="methodInfos">需要被代理的所有类的信息</param>
        /// <param name="proxyMethodInterceptors">每个类对应的需要实现的拦截器</param>
        private static void BuildProxyMethods(
            TypeBuilder typeBuilder,
            ConstructorBuilder[] cbs,
            List<MethodInfo> methodInfos,
            Dictionary<MethodInfo, Type[]> proxyMethodInterceptors
        )
        {
            // 遍历每个方法
            for (int i = 0; i < methodInfos.Count; i++)
            {
                var targetMethod = methodInfos[i];

                // TODO: Build Method Proxy
                // 构建代理方法
                BuildProxyMethod(typeBuilder, methodInfos[i], cbs, proxyMethodInterceptors[targetMethod]);
                var paramType = m_Method2ParamsType[targetMethod];

                //TODO: Init Method field
                // 对代理方法需要的字段进行初始化
                InitMethodField(targetMethod, cbs, proxyMethodInterceptors[targetMethod], paramType);
            }

            // TODO : 调用基类方法，调用构造函数
            CallBaseConstructors(typeBuilder, cbs);
        }

        private static void BuildProxyMethod(
            TypeBuilder typeBuilder,
            MethodInfo baseMethod,
            ConstructorBuilder[] cbs,
            Type[] interceptorTypes)
        {
            //var baseMethod = methodInfos[i];
            Type[] paramType;
            Type returnType;
            ParameterInfo[] paramInfo;
            // 获取事件委托类型
            Type delegateType = GetDelegateType(
                baseMethod,
                out paramType,
                out returnType,
                out paramInfo);
            // 保存该方法的参数类型
            m_Method2ParamsType.Add(baseMethod, paramType);
            // 重载目标方法
            // 在全局变量中定义当前方法所需要的字段
            DefineMethodField(
                typeBuilder,
                cbs,
                baseMethod,
                interceptorTypes,
                paramType
            );

            // 创建覆盖被拦截方法的方法方法
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                baseMethod.Name,
                GetOverrideMethodAttrs(baseMethod),
                CallingConventions.Any,
                returnType,
                paramType);

            // 创建可以调用父类被拦截方法的目标代理方法
            MethodBuilder baseMethodBuilder = typeBuilder.DefineMethod(
                BaseMethodCallerPrefix + baseMethod.Name,
                MethodAttributes.Private
                | MethodAttributes.HideBySig,
                CallingConventions.HasThis,
                returnType,
                paramType
            );


            // 定义参数
            for (var j = 0; j < paramInfo.Length; j++)
            {
                ParameterBuilder paramBuilder =
                    methodBuilder.DefineParameter(j + 1, paramInfo[j].Attributes,
                        paramInfo[j].Name);

                ParameterBuilder proxyParamBuilder =
                    baseMethodBuilder.DefineParameter(j + 1,
                        paramInfo[j].Attributes, paramInfo[j].Name);

                if (paramInfo[j].HasDefaultValue)
                {
                    paramBuilder.SetConstant(paramInfo[j].DefaultValue);

                    proxyParamBuilder.SetConstant(paramInfo[j].DefaultValue);
                }
            }


            BuildBaseCallerMethodIL(
                baseMethod,
                baseMethodBuilder,
                delegateType,
                paramType,
                returnType);

            // 构建IL
            BuildProxyMethodIL(
                typeBuilder.BaseType,
                baseMethod,
                methodBuilder,
                delegateType,
                paramType,
                returnType,
                interceptorTypes);


            typeBuilder.DefineMethodOverride(methodBuilder, baseMethod);
        }

        private static void BuildBaseCallerMethodIL(
            MethodInfo baseMethod,
            MethodBuilder baseMethodBuilder,
            Type delegateType,
            Type[] paramType,
            Type returnType)
        {
            ILGenerator il = baseMethodBuilder.GetILGenerator();
            bool isVoidReturn = returnType == typeof(void);
            if (!isVoidReturn)
            {
                il.DeclareLocal(returnType);
            }

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            // call message 
            for (int i = 1; i <= paramType.Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            // call base method
            il.Emit(OpCodes.Call, baseMethod);
            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc_0);
                Label endLabel = il.DefineLabel();
                il.Emit(OpCodes.Br_S, endLabel);
                il.MarkLabel(endLabel);
                il.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 创建 Il
        /// </summary>
        /// <param name="targetMethod">方法对象</param>
        /// <param name="methodBuilder"></param>
        /// <param name="delegateType">方法委托类型</param>
        /// <param name="paramType">方法参数</param>
        /// <param name="returnType">方法返回值</param>
        /// <param name="methodInfos">方法列表</param>
        /// <param name="interceptorTypes">方法对应的拦截者类型</param>
        private static void BuildProxyMethodIL(
            Type origin,
            MethodInfo targetMethod,
            MethodBuilder methodBuilder,
            Type delegateType,
            Type[] paramType, Type returnType,
            Type[] interceptorTypes)
        {
            ILGenerator il = methodBuilder.GetILGenerator();
            // 定义参数变量

            #region 函数内局部变量

            LocalBuilder parameter = il.DeclareLocal(typeof(object[])); // parameter
            LocalBuilder interceptor = il.DeclareLocal(typeof(IInterceptor)); // interceptor
            LocalBuilder invocation = il.DeclareLocal(typeof(Invocation)); // invocation
            LocalBuilder res = il.DeclareLocal(typeof(object)); // res
            LocalBuilder interceptors = il.DeclareLocal(typeof(Type[])); // interceptors
            LocalBuilder baseMethodProxy = il.DeclareLocal(typeof(System.Reflection.MethodInfo)); // baseMethodProxy
            LocalBuilder delegateInstance = il.DeclareLocal(typeof(System.Object)); // delegateInstance
            LocalBuilder baseMethodDelType = il.DeclareLocal(delegateType); // DelegateMethod
            LocalBuilder delegateMethod = il.DeclareLocal(typeof(Delegate)); // delegateMethod

            LocalBuilder isBaseMethodDelNull = il.DeclareLocal(typeof(bool));
            //LocalBuilder isInterceptorsNull = il.DeclareLocal(typeof(bool));
            //LocalBuilder isInvocationNotNull = il.DeclareLocal(typeof(bool));
            LocalBuilder index = il.DeclareLocal(typeof(Int32));
            LocalBuilder item = il.DeclareLocal(typeof(Func<Invocation, object>));
            LocalBuilder isLoopOver = il.DeclareLocal(typeof(bool));
            LocalBuilder isInterceptorNull = il.DeclareLocal(typeof(bool));
            LocalBuilder re = null; // 拦截方法返回结果
            if (returnType != typeof(void)) // 如果拦截的方法返回值不为void 创建一个返回值的临时变量
            {
                re = il.DeclareLocal(returnType);
            }

            #endregion

            #region for loop mark label

            Label lbIsTrue = il.DefineLabel();
            Label lbCondition = il.DefineLabel();
            Label interceptorNullBrLabel = il.DefineLabel();
            Label isBaseMethodDelNullLabel = il.DefineLabel();

            #endregion

            #region IL Body

            il.Emit(OpCodes.Nop);

            //  TODO: object[] Parameter = new object[2];

            #region TODO: object[] Parameter = new object[2];

            il.Emit(OpCodes.Ldc_I4, paramType.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, 0);

            #endregion

            //  TODO:Parameter[0] = a1; TODO:Parameter[1] = a2;

            #region TODO:Parameter[0] = a1; TODO:Parameter[1] = a2;

            // 创建传入参数
            for (int j = 0; j < paramType.Length; j++)
            {
                //将索引 0 处的局部变量加载到计算堆栈上。
                il.Emit(OpCodes.Ldloc_0, 0);
                il.Emit(OpCodes.Ldc_I4, j);
                // 将自变量（由指定索引值引用）加载到堆栈上。
                il.Emit(OpCodes.Ldarg, j + 1);
                if (paramType[j].IsValueType)
                {
                    // 将值类转换为对象引用（O 类型）。
                    il.Emit(OpCodes.Box, paramType[j]);
                }

                // 用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素。
                il.Emit(OpCodes.Stelem_Ref);
            }

            #endregion

            //  TODO:IInterceptor interceptor = null;

            #region TODO:IInterceptor interceptor = null;

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, 1);

            #endregion

            //  TODO:Invocation invocation = null;

            #region TODO:Invocation invocation = null;

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, 2);

            #endregion

            //  TODO: object res = null;

            #region TODO: object res = null;

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, 3);

            #endregion

            // TODO  if (interceptors == null)

            // #region if (interceptors == null)
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].interceptorsField);
            // il.Emit(OpCodes.Ldnull);
            // il.Emit(OpCodes.Ceq);
            // il.Emit(OpCodes.Stloc_S,isInterceptorsNull);
            // il.Emit(OpCodes.Ldloc_S,isInterceptorsNull);
            // il.Emit(OpCodes.Brfalse,interceptorsNotNullBrLabel);
            //
            // il.Emit(OpCodes.Nop);
            // #region branch content
            //
            // int interceptorsLength = interceptorTypes.Length;
            // il.Emit(OpCodes.Ldarg,0);
            // il.Emit(OpCodes.Ldc_I4, interceptorsLength);
            // il.Emit(OpCodes.Newarr, typeof(IInterceptor));
            //
            // // 初始化    
            // for (int j = 0; j < interceptorsLength; j++)
            // {
            //     il.Emit(OpCodes.Dup);
            //     il.Emit(OpCodes.Ldc_I4, j);
            //     il.Emit(OpCodes.Ldtoken, interceptorTypes[j]);
            //     il.Emit(OpCodes.Call, typeof(InterceptorFactory).GetMethod("Create",
            //         new[] { typeof(Type) }));
            //     il.Emit(OpCodes.Stelem_Ref);
            // }
            //
            //
            // il.Emit(OpCodes.Stfld, m_Method2FieldDatas[targetMethod].interceptorsField);
            //
            // il.Emit(OpCodes.Ldstr, "if (interceptors == null) After");
            // il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
            //     GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
            //         new Type[] { typeof(string) }, null));
            // #endregion
            // il.Emit(OpCodes.Nop);
            // il.MarkLabel(interceptorsNotNullBrLabel);
            // #endregion
            //
            // // TODO: interceptors = new Type2[...]
            //

            #region TODO: interceptors = new Type2[...]

            int interceptorLength = interceptorTypes.Length;
            il.Emit(OpCodes.Ldc_I4, interceptorLength);
            il.Emit(OpCodes.Newarr, typeof(System.Type));
            for (int j = 0; j < interceptorLength; j++)
            {
                //Debug.Log(interceptorTypes[j]);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, j);

                il.Emit(OpCodes.Ldtoken, interceptorTypes[j]);
                il.Emit(OpCodes.Call, typeof(Type)
                    .GetMethod(
                        "GetTypeFromHandle",
                        new[] {typeof(RuntimeTypeHandle)}));
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Stloc_S, interceptors);

            #endregion


            // TODO : MethodInfo baseMethod = GetType().GetMethod(
            // "AddTwoNumProxy",
            // BindingFlags.Instance | BindingFlags.NonPublic,
            // );


            #region MethodInfo baseMethod = GetType().GetMethod("AddTwoNumProxy",BindingFlags.Instance | BindingFlags.NonPublic);

            il.Emit(OpCodes.Ldarg, 0);
            il.Emit(OpCodes.Call, typeof(System.Object).GetMethod("GetType"));
            il.Emit(OpCodes.Ldstr, BaseMethodCallerPrefix + targetMethod.Name);
            il.Emit(OpCodes.Ldc_I4_S, 36);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldarg, 0);
            il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].paramTypesField);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Callvirt,
                typeof(System.Type).GetMethod(
                    "GetMethod",
                    (BindingFlags) (~0),
                    null,
                    CallingConventions.Any,
                    new Type[]
                    {
                        typeof(string), typeof(BindingFlags), typeof(Binder), typeof(Type[]),
                        typeof(ParameterModifier[])
                    },
                    null)
            );
            il.Emit(OpCodes.Stloc_S, baseMethodProxy);

            #endregion

            // TODO :  object instance = this;

            #region object instance = this;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc_S, delegateInstance);

            #endregion

            // TODO:  delegateType = typeof(Action<int, int>);

            #region delegateType = typeof(Action<int, int>);

            il.Emit(OpCodes.Ldtoken, delegateType);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(
                "GetTypeFromHandle",
                new[] {typeof(RuntimeTypeHandle)}));
            il.Emit(OpCodes.Stloc_S, baseMethodDelType);

            #endregion

            // TODO :  if (baseProxyMethod == null)

            #region if (baseProxyMethod == null)

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].baseMethodDelegateField);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Stloc_S, isBaseMethodDelNull);
            il.Emit(OpCodes.Ldloc_S, isBaseMethodDelNull);
            il.Emit(OpCodes.Brfalse_S, isBaseMethodDelNullLabel);

            il.Emit(OpCodes.Nop);
            // TODO :baseProxyMethod = Delegate.CreateDelegate(
            // typeof(Func<int, int, int>), instance, baseMethod);

            #region baseProxyMethod = Delegate.CreateDelegate(typeof(Func<int, int, int>), instance, baseMethod);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldtoken, delegateType);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(
                "GetTypeFromHandle",
                new[] {typeof(RuntimeTypeHandle)}));
            il.Emit(OpCodes.Ldloc_S, delegateInstance);
            il.Emit(OpCodes.Ldloc_S, baseMethodProxy);
            il.Emit(OpCodes.Call, typeof(Delegate).GetMethod(
                "CreateDelegate",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] {typeof(Type), typeof(object), typeof(MethodInfo)},
                null));
            il.Emit(OpCodes.Stfld, m_Method2FieldDatas[targetMethod].baseMethodDelegateField);

            #endregion

            il.Emit(OpCodes.Nop);
            il.MarkLabel(isBaseMethodDelNullLabel);

            #endregion


            // TODO: Func<int, int, int> DelegateMethod = base.AddTwoNum;

//             #region TODO: Func<int, int, int> DelegateMethod = base.AddTwoNum;
//
// #if ILDebug
//              il.Emit(OpCodes.Ldarg, 0);
//             
//              
//              // il.Emit(OpCodes.Ldftn,typeBuilder.BaseType.GetMethod(targetMethod.Name,
//              //     BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static));
//              //Delegate targetMethodDel = CreateDelegateFromMethodInfoByDelegate(delegateType,targetMethod);
//           
//              il.Emit(OpCodes.Ldftn, targetMethod);
//              il.Emit(OpCodes.Newobj, delegateType.GetConstructors()[0]);
//              il.Emit(OpCodes.Stloc_S, DelegateMethod);
// #endif
//
//             #endregion

            // TODO:Delegate delegateMethod = DelegateMethod;

            #region TODO:Delegate delegateMethod = DelegateMethod;

            //il.Emit(OpCodes.Ldnull); //delegateMethod = null;
            il.Emit(OpCodes.Ldarg_0); //delegateMethod = null;
            il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].baseMethodDelegateField); //delegateMethod = null;
            il.Emit(OpCodes.Stloc_S, delegateMethod);

            #endregion

            // TODO   if (invocation == null)

            // #region if (invocation == null)
            //
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Ldnull);
            // il.Emit(OpCodes.Ceq);
            // il.Emit(OpCodes.Stloc_S,isInvocationNotNull);
            //
            // il.Emit(OpCodes.Ldloc_S,isInvocationNotNull);
            // // Debug S
            // il.Emit(OpCodes.Box, typeof(bool));
            // il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
            //     GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
            //         new Type[] { typeof(string) }, null)); 
            // il.Emit(OpCodes.Nop);
            //
            // il.Emit(OpCodes.Ldloc_S,isInvocationNotNull);
            // // Debug E
            // il.Emit(OpCodes.Brfalse,isInvocationNotNullLabel);
            // #region content
            //
            // il.Emit(OpCodes.Nop);
            // // TODO   invocation = new Invocation(instance,baseMethod);
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldloc_S,delegateInstance);
            // il.Emit(OpCodes.Ldloc_S,baseMethodProxy);
            // ConstructorInfo ctor = typeof(Invocation).GetConstructor(
            //     BindingFlags.Public | BindingFlags.Instance,
            //     null,
            //     new Type[] {typeof(object), typeof(MethodInfo)}, null);
            // //Debug.Log(ctor == null);
            // il.Emit(OpCodes.Newobj,ctor);
            // il.Emit(OpCodes.Stfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            //
            // #endregion
            // il.Emit(OpCodes.Nop);
            // il.MarkLabel(isInvocationNotNullLabel);
            //
            //
            //
            // /*il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Ldnull);
            // il.Emit(OpCodes.Ceq);
            // il.Emit(OpCodes.Stloc_S,isInvocationNotNull);
            //
            // il.Emit(OpCodes.Ldloc_S,isInvocationNotNull);
            // // Debug S
            // il.Emit(OpCodes.Box, typeof(bool));
            // il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
            //     GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
            //         new Type[] { typeof(string) }, null)); 
            // il.Emit(OpCodes.Nop);*/
            //
            //
            //
            //
            //
            // #endregion
            //
            // TODO:  for (int index = 0; index < interceptors.Length; index++)

            #region interceptors for loop start

            #region loop init

            il.Emit(OpCodes.Ldc_I4, 0);
            il.Emit(OpCodes.Stloc_S, index);

            il.Emit(OpCodes.Br, lbCondition);
            il.MarkLabel(lbIsTrue);
            il.Emit(OpCodes.Nop);

            #endregion

            // for content start

            #region loop body

            // 输出下标Debug

            #region 输出下标Debug

            //
            // il.Emit(OpCodes.Ldloc, index);
            // il.Emit(OpCodes.Box, typeof(System.Int32));
            // il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
            //     GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
            //         new Type[] { typeof(string) }, null));
            // il.Emit(OpCodes.Nop);

            #endregion

            // TODO:        invocation = new Invocation(instance,baseMethod);

            #region TODO:        invocation = new Invocation(instance,baseMethod);

            //
            // // il.Emit(OpCodes.Newobj, typeof(Invocation).
            // //     GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            // //     .First(e => e.GetParameters().Length == 0));
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Stloc, 2);
            //
            il.Emit(OpCodes.Ldloc_S, delegateInstance);
            il.Emit(OpCodes.Ldloc_S, baseMethodProxy);
            ConstructorInfo ctor = typeof(Invocation).GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] {typeof(object), typeof(MethodInfo)}, null);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_S, invocation);

            #endregion


            // TODO: invocation.Parameter = Parameter;

            #region invocation.Parameter = Parameter;

            il.Emit(OpCodes.Ldloc, 2);
            il.Emit(OpCodes.Ldloc, 0);
            il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_Parameter"));
            il.Emit(OpCodes.Nop);

            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Ldloc_0);
            // il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_Parameter"));
            // il.Emit(OpCodes.Nop);

            #endregion

            // TODO:   invocation.DelegateMethod = delegateMethod;

            #region invocation.DelegateMethod = delegateMethod;

            il.Emit(OpCodes.Ldloc, 2);
            il.Emit(OpCodes.Ldloc_S, delegateMethod);
            il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_DelegateMethod"));
            il.Emit(OpCodes.Nop);
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Ldloc_S, delegateMethod);
            // il.Emit(OpCodes.Callvirt, typeof(Invocation).GetMethod("set_DelegateMethod"));
            // il.Emit(OpCodes.Nop);

            #endregion

            // TODO : interceptor = interceptors[index];

            #region interceptor = interceptors[index];

            // 创建拦截类 S
            il.Emit(OpCodes.Ldloc_S, interceptors);
            il.Emit(OpCodes.Ldloc_S, index);
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Call, typeof(InterceptorFactory).GetMethod("Create",
                new[] {typeof(Type)}));
            il.Emit(OpCodes.Stloc, 1);
            // 创建拦截类 E

            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld, m_Method2FieldDatas[targetMethod].interceptorsField);
            // il.Emit(OpCodes.Ldloc_S, index);
            // il.Emit(OpCodes.Ldelem_Ref);
            // il.Emit(OpCodes.Stloc, 1);

            #endregion

            // TODO:  Func<Invocation, object> item = interceptor.Intercept;

            #region Func<Invocation, object> item = interceptor.Intercept;

            il.Emit(OpCodes.Ldloc, 1);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldvirtftn, typeof(IInterceptor)
                .GetMethod("Intercept"));
            il.Emit(OpCodes.Newobj, typeof(Func<Invocation, object>).GetConstructors()[0]);
            il.Emit(OpCodes.Stloc_S, item);

            #endregion

            // TODO:  delegateMethod = item;

            #region TODO:  delegateMethod = item;

            il.Emit(OpCodes.Ldloc_S, item);
            il.Emit(OpCodes.Stloc_S, delegateMethod);

            #endregion

            // TODO: Parameter = new object[1]{invocation};

            #region Parameter = new object[1]{invocation};

            il.Emit(OpCodes.Ldc_I4, 1);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, 0);
            il.Emit(OpCodes.Ldloc, 2);
            il.Emit(OpCodes.Stelem_Ref);
            il.Emit(OpCodes.Stloc, 0);
            // il.Emit(OpCodes.Ldc_I4, 1);
            // il.Emit(OpCodes.Newarr, typeof(object));
            // il.Emit(OpCodes.Dup);
            // il.Emit(OpCodes.Ldc_I4, 0);
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Stelem_Ref);
            // il.Emit(OpCodes.Stloc, 0);

            #endregion

            #endregion

            // for content End
            il.Emit(OpCodes.Nop);

            //  interceptors for end

            #region loop end

            // Loop end
            il.Emit(OpCodes.Ldloc_S, index);
            //il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) }));
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_S, index);

            il.MarkLabel(lbCondition);
            il.Emit(OpCodes.Ldloc_S, index);
            il.Emit(OpCodes.Ldloc_S, interceptors);
            //il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].interceptorsField);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Clt);
            il.Emit(OpCodes.Stloc_S, isLoopOver);
            il.Emit(OpCodes.Ldloc_S, isLoopOver);
            //il.MarkLabel(lbIsTrue);
            il.Emit(OpCodes.Brtrue, lbIsTrue);

            #endregion

            #endregion

            // TODO:   if (interceptor != null)

            #region if (interceptor != null)

            il.Emit(OpCodes.Ldloc_S, 1);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Cgt_Un);
            il.Emit(OpCodes.Stloc_S, isInterceptorNull);

            il.Emit(OpCodes.Ldloc_S, isInterceptorNull);
            il.Emit(OpCodes.Brfalse, interceptorNullBrLabel);

            il.Emit(OpCodes.Nop);

            //  TODO: res = interceptor.Intercept(invocation);

            #region TODO: res = interceptor.Intercept(invocation);

            il.Emit(OpCodes.Ldloc, 1);
            il.Emit(OpCodes.Ldloc, 2);
            il.Emit(OpCodes.Callvirt, typeof(IInterceptor).GetMethod("Intercept"));
            il.Emit(OpCodes.Stloc, 3);
            // il.Emit(OpCodes.Ldloc, interceptor);
            // il.Emit(OpCodes.Ldarg_0);

            // il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            // il.Emit(OpCodes.Ldnull);
            // il.Emit(OpCodes.Ceq);
            // il.Emit(OpCodes.Box, typeof(bool));
            // il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
            //     GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
            //         new Type[] { typeof(string) }, null)); 
            //il.Emit(OpCodes.Nop);


            /*il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld,m_Method2FieldDatas[targetMethod].invocationChunkField);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Stloc_S,isInvocationNotNull);
            
            il.Emit(OpCodes.Ldloc_S,isInvocationNotNull);
            // Debug S
            il.Emit(OpCodes.Box, typeof(bool));
            il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).
                GetMethod("Log", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                    new Type[] { typeof(string) }, null)); 
            il.Emit(OpCodes.Nop);*/

            #endregion

            #endregion

            il.Emit(OpCodes.Nop);
            il.MarkLabel(interceptorNullBrLabel);
            // TODO:Not void  Return 

            #region Not void  Return

            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Ldloc, res);
                il.Emit(OpCodes.Unbox_Any, returnType);
                il.Emit(OpCodes.Stloc_S, re);
                Label endLabel = il.DefineLabel();
                il.Emit(OpCodes.Br, endLabel);
                il.MarkLabel(endLabel);
                il.Emit(OpCodes.Ldloc_S, re);
            }
            // else
            // {
            //     il.Emit(OpCodes.Pop);
            // }

            #endregion

            il.Emit(OpCodes.Ret);

            #endregion
        }

        /// <summary>
        /// 获取所有需要实现代理的方法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="methodInfos"></param>
        /// <param name="proxyMethodInterceptors"></param>
        private static void GetProxyableMethods(
            Type origin,
            out List<MethodInfo> methodInfos,
            out Dictionary<MethodInfo, Type[]> proxyMethodInterceptors
        )
        {
            // TODO: 对可代理的Module中的代理方法寻找对应的拦截器并返回代理器类型
            var methods = origin.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            methodInfos = new List<MethodInfo>();
            proxyMethodInterceptors =
                new Dictionary<MethodInfo, Type[]>();

            // 获取所有可以进行拦截的方法
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                // 判断方法是否是Virtual 并且含有 ProxyableMethod属性
                if (!method.IsVirtual || method.IsStatic || method.IsFinal) continue;
                // 获取所有的允许代理属性
                var methodAttr =
                    method.GetCustomAttribute<ProxyableMethodAttribute>(true);
                //Debug.Log($"当前类型{origin} 当前代理方法{methods[i]} 包含属性{methodAttr}");

                // 判断方法是否挂载了 可重写特性
                if (ReferenceEquals(methodAttr, null)) continue;
                // 在当前程序集中查找 实现类包含proxyAbleAttr 和 customAttr 类型的 调用者
                var proxyAbleAttr = CustomAttributeExtensions.GetCustomAttribute<ProxyableAttribute>(origin, true);
                string methodKey = methodAttr.proxyMethodKey;

                // 获取所有的拦截者
                var interceptors =
                    GetInterceptors(origin, methodKey);
                if (interceptors.Length == 0) continue;

                methodInfos.Add(method);
                proxyMethodInterceptors.Add(method, interceptors);
            }
        }

        /// <summary>
        /// 获取目标方法在当前代理类中的字段名字
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static void GetTargetMethodFieldName(MethodInfo methodInfo,
            out string proxyMethodFieldName,
            out string interceptorTypesFieldName,
            out string invocationChunkFieldName,
            out string paramTypesFieldName,
            out string baseMethodDelgateFieldName)
        {
            string methodHC = GetObjectHashCodeConvert(methodInfo);
            proxyMethodFieldName = String.Concat("_proxyMethodInfo", methodHC);
            interceptorTypesFieldName = String.Concat("_interceptorTypes", methodHC);
            invocationChunkFieldName = String.Concat("_invocationChunk", methodHC);
            paramTypesFieldName = String.Concat("_paramTypes", methodHC);
            baseMethodDelgateFieldName = String.Concat("_baseMethodDelegate", methodHC);
        }

        private static string GetObjectHashCodeConvert(object obj)
        {
            string name = obj.GetHashCode().ToString().Replace("-", "_");
            return name;
        }

        private static MethodAttributes GetOverrideMethodAttrs(MethodInfo baseMethod)
        {
            MethodAttributes methodAttributes = MethodAttributes.Virtual
                                                | MethodAttributes.Final
                                                | MethodAttributes.HideBySig;
            if (baseMethod.IsPublic)
            {
                methodAttributes = MethodAttributes.Public | methodAttributes;
            }
            else
            {
                methodAttributes = MethodAttributes.Private | methodAttributes;
            }

            return methodAttributes;
        }

        /// <summary>
        /// 获取指定类型的拦截者
        /// </summary>
        /// <param name="proxyableType"></param>
        /// <param name="proxyableMethodTypeAttr"></param>
        /// <returns></returns>
        private static Type[] GetInterceptors(Type proxyableType, string methodKey)
        {
            Type[] interceptorTypes = ReflectionTool.GetAssignedClassOf<IInterceptor>();
            List<Type> res = new List<Type>();
            for (int i = 0; i < interceptorTypes.Length; i++)
            {
                var interceptorType = interceptorTypes[i];
                if (interceptorType.IsInterface) continue;
                // Debug.Log($"find interpector{interceptorType}");
                if (IsProxyInterceptor(interceptorType, proxyableType, methodKey))
                {
                    // Debug.Log($"find interpector{interceptorType}" +
                    //           $" proxyableType:{proxyableType} proxyableMethodTypeAttr{proxyableMethodTypeAttr}");
                    res.Add(interceptorType);
                }
            }

            return res.ToArray();
            //Attribute.GetCustomAttributes<InvocationAttribute>(true);
        }

        /// <summary>
        /// 通过MethodInfo获得其参数类型列表，返回类型，和委托类型
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="paramType"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static Type GetDelegateType(MethodInfo targetMethod,
            out Type[] paramType,
            out Type returnType,
            out ParameterInfo[] paramInfo)
        {
            paramInfo = targetMethod.GetParameters(); // 获取参数列表
            //paramType
            paramType = new Type[paramInfo.Length];
            for (int i = 0; i < paramInfo.Length; i++)
            {
                paramType[i] = paramInfo[i].ParameterType; //保存参数类型
            }

            //returnType
            returnType = targetMethod.ReturnType;
            //delegateType
            Type delegateType;
            if (returnType == typeof(void))
            {
                switch (paramType.Length)
                {
                    case 0:
                        delegateType = typeof(Action);
                        break;
                    case 1:
                        delegateType = typeof(Action<>).MakeGenericType(paramType);
                        break;
                    case 2:
                        delegateType = typeof(Action<,>).MakeGenericType(paramType);
                        break;
                    case 3:
                        delegateType = typeof(Action<,,>).MakeGenericType(paramType);
                        break;
                    case 4:
                        delegateType = typeof(Action<,,,>).MakeGenericType(paramType);
                        break;
                    case 5:
                        delegateType = typeof(Action<,,,,>).MakeGenericType(paramType);
                        break;
                    case 6:
                        delegateType = typeof(Action<,,,,,>).MakeGenericType(paramType);
                        break;
                    case 7:
                        delegateType = typeof(Action<,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 8:
                        delegateType = typeof(Action<,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 9:
                        delegateType = typeof(Action<,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 10:
                        delegateType = typeof(Action<,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 11:
                        delegateType = typeof(Action<,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 12:
                        delegateType = typeof(Action<,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 13:
                        delegateType = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 14:
                        delegateType = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    case 15:
                        delegateType = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                    default:
                        delegateType = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(paramType);
                        break;
                }
            }
            else
            {
                Type[] arr = new Type[paramType.Length + 1];
                for (int i = 0; i < paramType.Length; i++)
                {
                    arr[i] = paramType[i];
                }

                arr[paramType.Length] = returnType;
                switch (paramType.Length)
                {
                    case 0:
                        delegateType = typeof(Func<>).MakeGenericType(arr);
                        break;
                    case 1:
                        delegateType = typeof(Func<,>).MakeGenericType(arr);
                        break;
                    case 2:
                        delegateType = typeof(Func<,,>).MakeGenericType(arr);
                        break;
                    case 3:
                        delegateType = typeof(Func<,,,>).MakeGenericType(arr);
                        break;
                    case 4:
                        delegateType = typeof(Func<,,,,>).MakeGenericType(arr);
                        break;
                    case 5:
                        delegateType = typeof(Func<,,,,,>).MakeGenericType(arr);
                        break;
                    case 6:
                        delegateType = typeof(Func<,,,,,,>).MakeGenericType(arr);
                        break;
                    case 7:
                        delegateType = typeof(Func<,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 8:
                        delegateType = typeof(Func<,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 9:
                        delegateType = typeof(Func<,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 10:
                        delegateType = typeof(Func<,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 11:
                        delegateType = typeof(Func<,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 12:
                        delegateType = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 13:
                        delegateType = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 14:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    case 15:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                    default:
                        delegateType = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(arr);
                        break;
                }
            }

            return delegateType;
        }
        // private static Type SaveProxyType(Type origin,Type proxiedType)
        // {
        //     if (m_ProxyType.Contains(proxiedType)) return proxiedType;
        //     m_ProxyType.Add(proxiedType);
        //     ProxyCollections.SaveProxyType(origin, proxiedType);
        //     Debug.LogFormat("保存代理类型{0}", proxiedType);
        //     return proxiedType;
        // }


        /// <summary>
        /// 生成代理方法的字段
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="baseMethodInfo"></param>
        /// <param name="paramsType"></param>
        /// <param name="interceptorTypes"></param>
        private static void DefineMethodField(
            TypeBuilder typeBuilder,
            ConstructorBuilder[] cbs,
            MethodInfo baseMethodInfo,
            Type[] interceptorTypes,
            Type[] paramTypes)
        {
            // 获取当前方法对应的字段名称
            GetTargetMethodFieldName(baseMethodInfo,
                out string proxyMethodFieldName,
                out string interceptorTypesFieldName,
                out string invocationChunkFieldName,
                out string paramTypesFieldName,
                out string baseMethodDelgateFieldName);

            MethodFieldData fieldData = new MethodFieldData();

            // 创建字段一: 当前方法的目标代理方法 MethodInfo
            fieldData.proxyMethodField =
                typeBuilder.DefineField(proxyMethodFieldName, typeof(MethodInfo), FieldAttributes.Private);

            // 创建字段二: 当前拦截目标方法的拦截器列表 type[] interceptorTypes
            fieldData.interceptorsField = typeBuilder.DefineField(interceptorTypesFieldName, typeof(IInterceptor[]),
                FieldAttributes.Private);

            // 创建字段三: 当前方法的Invocation 对象
            fieldData.invocationChunkField =
                typeBuilder.DefineField(invocationChunkFieldName, typeof(Invocation), FieldAttributes.Private);

            // 创建字段三: 当前方法的入参对象类型数组 paramTypes
            fieldData.paramTypesField =
                typeBuilder.DefineField(paramTypesFieldName, typeof(Type[]), FieldAttributes.Private);

            // 创建字段四 : 目标代理方法的事件委托
            fieldData.baseMethodDelegateField =
                typeBuilder.DefineField(baseMethodDelgateFieldName, typeof(Delegate), FieldAttributes.Private);

            // 保存当前方法对应的字段信息
            m_Method2FieldDatas.Add(baseMethodInfo, fieldData);
        }

        /// <summary>
        /// 在每个函数中对当前代理方法需要的字段进行初始化
        /// 在构造函数中对当前函数中的字段进行初始化
        /// </summary>
        /// <param name="cbs"></param>
        /// <param name="baseMethodInfo"></param>
        /// <param name="paramTypes"></param>
        private static void InitMethodField(MethodInfo baseMethodInfo, ConstructorBuilder[] cbs,
            Type[] interceptorTypes, Type[] paramTypes)
        {
            var fieldData = m_Method2FieldDatas[baseMethodInfo];
            // 在每个函数中对当前代理方法需要的字段进行初始化
            // 在构造函数中对当前函数中的字段进行初始化
            for (int i = 0; i < cbs.Length; i++)
            {
                ConstructorBuilder cb = cbs[i];
                ILGenerator il = cb.GetILGenerator();


                // TODO: MethodInfo baseMethod = GetType().GetMethod("AddTwoNumProxy",BindingFlags.Instance | BindingFlags.NonPublic);


                // TODO:   private IInterceptor[] interceptorTypes  =  new Type[3]{...}

                #region private Type[] interceptors  =  null

                il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stfld, fieldData.interceptorsField);
                //

                //

                #endregion

                // TODO : private Delegate baseMethodDelegate = null

                #region private Delegate baseMethodDelegate = null

                il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stfld, fieldData.baseMethodDelegateField);

                #endregion

                // TODO : private Invocation invocation = new Invocation();

                #region private Invocation invocation = new Invocation();

                il.Emit(OpCodes.Ldarg, 0);
                // il.Emit(OpCodes.Newobj, typeof(Invocation).
                //     GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                //     .First(e => e.GetParameters().Length == 0)); 
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stfld, fieldData.invocationChunkField);

                #endregion

                // TODO: private Type[] pramsTypes  =  new Type[3]{...}

                #region private Type[] pramsTypes  =  new Type[3]{...}

                int paramTypesLength = paramTypes.Length;
                il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldc_I4, paramTypesLength);
                il.Emit(OpCodes.Newarr, typeof(System.Type));
                for (int j = 0; j < paramTypesLength; j++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, j);
                    il.Emit(OpCodes.Ldtoken, paramTypes[j]);
                    il.Emit(OpCodes.Call, typeof(Type).GetMethod(
                        "GetTypeFromHandle",
                        new[] {typeof(RuntimeTypeHandle)}));
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Stfld, fieldData.paramTypesField);

                #endregion
            }
        }


        private static void CallBaseConstructors(TypeBuilder typeBuilder, ConstructorBuilder[] cbs)
        {
            // TODO :call base class constructors
            for (int i = 0; i < cbs.Length; i++)
            {
                var constructorParamTypes = m_CB2ParamTypes[cbs[i]];
                // call base constructor
                CallBaseConstructor(cbs[i], typeBuilder.BaseType, constructorParamTypes);
            }
        }

        private static void CallBaseConstructor(ConstructorBuilder cb, Type baseType, Type[] constructorParamTypes)
        {
            ILGenerator il = cb.GetILGenerator();

            #region call base constructor

            il.Emit(OpCodes.Ldarg, 0);
            //Type[] constructorParamTypes = m_CB2ParamTypes[cb];
            //Debug.LogFormat("当前构造函数{0} 需要的参数类型数量为{1}", cb, constructorParamTypes.Length);
            // 传递当前构造函数许哦需要的参数
            for (int j = 1; j <= constructorParamTypes.Length; j++)
            {
                il.Emit(OpCodes.Ldarg, j);
            }

            il.Emit(OpCodes.Call,
                baseType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    constructorParamTypes,
                    null));
            if (constructorParamTypes.Length != 0)
                il.Emit(OpCodes.Nop);
            // TODO : Constructor ret
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);

            #endregion
        }


        /// <summary>
        /// IL Unity Log output
        /// </summary>
        /// <param name="il"></param>
        private static void ILCallLog(ILGenerator il)
        {
            il.Emit(OpCodes.Call, typeof(UnityEngine.Debug).GetMethod("Log", BindingFlags.Public | BindingFlags.Static,
                Type.DefaultBinder,
                new Type[] {typeof(string)}, null));
        }
    }
}
// 定义IL实现功能如下
//         using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Reflection;
//
// namespace InterceptorProgram
// {
//     //下面的il相当于
//     public class parent
//     {
//         public virtual string test(List<string> p1, int p2)
//         {
//             return "123";
//         }
//     }
//
//     public class child : parent
//     {
//         public override string test(List<string> p1, int p2)
//         {
//             object[] Parameter = new object[2];
//             Parameter[0] = p1;
//             Parameter[1] = p2;
//             Func<List<string>, int, string> DelegateMethod = base.test;
//
//             Invocation invocation = new Invocation();
//             invocation.Parameter = Parameter;
//             invocation.DelegateMethod = DelegateMethod;
//             TestInterceptor interceptor = new TestInterceptor();
//             return (string) interceptor.Intercept(invocation);
//         }
//     }
//
//     public class TestInterceptor : IInterceptor
//     {
//         public object Intercept(Invocation invocation)
//         {
//             Console.WriteLine("1号 拦截前");
//             var res = invocation.Proceed();
//             Console.WriteLine("1号 拦截后");
//             return res;
//         }
//     }
//
//     public class Test2Interceptor : IInterceptor
//     {
//         public object Intercept(Invocation invocation)
//         {
//             Console.WriteLine("2号 拦截前");
//             var res = invocation.Proceed();
//             Console.WriteLine("2号 拦截后");
//             return res;
//         }
//     }
//
//     public class Test
//     {
//         public virtual void AddTwoNum(int a1, int a2)
//         {
//             Console.WriteLine($"计算{a1} add {a2} == {a1 + a2}");
//             //return a1 + a2;
//         }
//     }
//
//     public class TestProxy : Test
//     {
//         public override void AddTwoNum(int a1, int a2)
//         {
//             object[] Parameter = new object[2];
//             Parameter[0] = a1;
//             Parameter[1] = a2;
//             IInterceptor interceptor = null;
//             Invocation invocation = null;
//             
//             object res = null;
//             Type[] interceptors = new Type[3]
//             {
//                 typeof(TestInterceptor),
//                 typeof(Test2Interceptor),
//                 typeof(Test2Interceptor),
//             };
//             InterceptorFactory.Create(interceptors[0]);
//             Action<int, int> DelegateMethod = base.AddTwoNum;
//             Delegate delegateMethod = DelegateMethod;
//             //object[] nextParams = new Object[1];
//             for (int index = 0; index < interceptors.Length; index++)
//             {
//                 invocation = new Invocation(); 
//                 invocation.Parameter = Parameter;
//                 invocation.DelegateMethod = delegateMethod;
//                 interceptor =  (IInterceptor) InterceptorFactory.Create(interceptors[index]);
//                 Func<Invocation, object> item = interceptor.Intercept;
//                 delegateMethod = item;
//                 Parameter = new object[]{invocation};
//             }
//             /*
//            */
//             res = interceptor.Intercept(invocation);
//             //return (int)res;
//         }
//
//         private void CreateDelegate(Invocation parentInvocation, Func<object, Invocation> action)
//         {
//             parentInvocation.DelegateMethod = action;
//         }
//         
//     }
// }
//