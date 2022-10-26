namespace UnitFramework.Runtime
{
    public class MonoController : MonoUnit,IController
    {
        public virtual string ControllerName { get=>UnitName; }
        public virtual void ControllerUpdate()
        {
            
        }
    }
}