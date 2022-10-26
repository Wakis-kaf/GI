namespace UnitFramework.Runtime
{
    public abstract class PushKey
    {
        public abstract bool IsDown{get;}
        public abstract bool IsUp{get;}
        public abstract bool IsPushing{get;}
    }
}