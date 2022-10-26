﻿using System.Collections.Generic;
using System.Reflection;

namespace UnitFramework.Runtime
{
    public static class InvocationFactory
    {
        private static Queue<Invocation> m_FreeInvocations = new Queue<Invocation>();
        public static Invocation Create(object instance,MethodInfo baseMethod)
        {
            if (m_FreeInvocations.Count == 0)
            {
                return new Invocation(instance,baseMethod);
            }

            Invocation dequeue =m_FreeInvocations.Dequeue();
            dequeue.BindInstance(instance);
            dequeue.BingBaseMethod(baseMethod);
            return dequeue;
        }
        
    }
}