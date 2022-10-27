using System.Dynamic;

namespace UnitFramework.Runtime
{
    public interface IPoolElement
    {
        void OnPrewarm();
        void OnCreate();
        void OnGet();
        void OnPut();
        void OnDestroy();
    }
}