﻿﻿namespace UnitFramework.Runtime
{
    public interface IInterceptor
    {
        object Intercept(Invocation invocation);
    }
}