﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnitFramework.Runtime;
using JetBrains.Annotations;
using UnityEngine;

namespace UnitFramework.Utils
{
    public static class ReflectionTool
    {
        public static Type[] GetSubClassOf<T>(Type attrType = null)
        {
            return GetSubClassOf(typeof(T), attrType);
        }

        public static bool IsSubClassOfRawGeneric([NotNull] this Type type, [NotNull] Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            while (type != null && type != typeof(object))
            {
                bool isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        public static Type[] GetSubClassOf(Type type, Type attrType = null)
        {
            Assembly assembly = type.Assembly;
            var assemblyAllTypes = assembly.GetTypes();
            var names = new List<Type>();
            for (int i = 0; i < assemblyAllTypes.Length; i++)
            {
                //Debug.Log($" ab type{assemblyAllTypes[i]} targetType{type}");
                Type item = assemblyAllTypes[i];
                if (item.IsSubclassOf(type))
                {
                    if (attrType != null && !Attribute.IsDefined(item, attrType))
                        continue;
                    names.Add(item);
                }
            }

            return names.ToArray();
        }

        public static Type[] GetSubClassOfRawGeneric(Type type, bool ignoreGenericType = false)
        {
            Assembly assembly = type.Assembly;
            var assemblyAllTypes = assembly.GetTypes();
            var names = new List<Type>();
            for (int i = 0; i < assemblyAllTypes.Length; i++)
            {
                //Debug.Log($" ab type{assemblyAllTypes[i]} targetType{type}");
                if (assemblyAllTypes[i].IsSubClassOfRawGeneric(type))
                {
                    if (ignoreGenericType && assemblyAllTypes[i].IsGenericType) continue;
                    names.Add(assemblyAllTypes[i]);
                }
            }

            return names.ToArray();
        }


        public static Type[] GetAssignedClassOf(Type interfaceType, Type attributeType = null)
        {
            if (!interfaceType.IsInterface) return null;
            Assembly assembly = interfaceType.Assembly;
            var assemblyAllTypes = assembly.GetTypes();
            var res = new List<Type>();
            for (int i = 0; i < assemblyAllTypes.Length; i++)
            {
                //Debug.Log($" ab type{assemblyAllTypes[i]} targetType{type}");
                Type item = assemblyAllTypes[i];
                if (!item.IsClass) continue;
                if (interfaceType.IsAssignableFrom(item))
                {
                    if (attributeType != null && !Attribute.IsDefined(item, attributeType))
                        continue;
                    res.Add(item);
                }
            }

            return res.ToArray();
        }

        public static bool IsImplementedGenericInterface(Type interfaceType, Type targetType,
            out Type genericInterfaceType)
        {
            var interfaces = targetType.GetInterfaces();
            genericInterfaceType = null;
            bool hasImplemented = false;
            if (interfaces.Length > 0)
            {
                foreach (var inter in interfaces)
                {
                    if (!inter.IsGenericType) continue;

                    var t = inter.GetGenericTypeDefinition();

                    if (t == interfaceType)
                    {
                        genericInterfaceType = inter;
                        hasImplemented = true;
                        break;
                    }
                }
            }

            return hasImplemented;
        }

        public static Type[] GetAssignedClassOf<T>()
        {
            var interfaceType = typeof(T);
            return GetAssignedClassOf(interfaceType);
        }

        public static Type[] GetAssignedClassOf<T>(Type attributeType)
        {
            var interfaceType = typeof(T);
            return GetAssignedClassOf(interfaceType, attributeType);
        }

        /// <summary>
        /// 获取当前程序集中某个类的派生类类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetDerivedClassType<T>(bool containSelf = false) where T : class
        {
            // 获取所有的类型
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var baseType = typeof(T);
            List<Type> derivedClassType = new List<Type>();
            foreach (var type in types)
            {
                // 判断 type 是否继承于基类 baseType
                if (type.IsSubclassOf(baseType) && (!containSelf && type != baseType))
                {
                    derivedClassType.Add(type);
                }
            }

            return derivedClassType;
        }

        /// <summary>
        /// 获取当前程序集中某个类的派生类类型名字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetDerivedClassTypeName<T>(bool containSelf = false) where T : class
        {
            // 获取所有的类型
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var baseType = typeof(T);
            List<string> derivedClassType = new List<string>();
            foreach (var type in types)
            {
                // 判断 type 是否继承于基类 baseType
                if (type.IsSubclassOf(baseType) && (!containSelf && type != baseType))
                {
                    derivedClassType.Add(type.ToString());
                }
            }

            return derivedClassType;
        }

        /// <summary>
        /// 创建当前程序集中某个类的派生类实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> CreateDerivedClassInstance<T>(bool containSelf = false) where T : class
        {
            List<Type> types = GetDerivedClassType<T>(containSelf);
            List<T> instances = new List<T>();
            foreach (var type in types)
            {
                T instance = CreateInstance(type) as T;
                instances.Add(instance);
            }

            return instances;
        }


        public static T CreateInstance<T>()
        {
            return System.Activator.CreateInstance<T>();
        }

        public static T CreateInstance<T>(params object[] objects)
        {
            return (T) System.Activator.CreateInstance(typeof(T), objects);
        }

        public static T CreateInstance<T>(Type type)
        {
            return (T) System.Activator.CreateInstance(type);
        }

        public static T CreateInstance<T>(Type type, params object[] objects)
        {
            return (T) System.Activator.CreateInstance(type, objects);
        }

        public static object CreateInstance(Type type, params object[] objects)
        {
            return System.Activator.CreateInstance(type, objects);
        }

        public static bool TryGetGenericTypesDefinition(Type type, out Type[] types)
        {
            types = null;
            if (!type.IsGenericType) return false; // 判断是否是泛型
            types = type.GetGenericArguments();
            return true;
        }
    }
}