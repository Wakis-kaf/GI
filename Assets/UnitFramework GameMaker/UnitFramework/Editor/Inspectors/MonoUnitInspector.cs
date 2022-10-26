using UnitFramework.Runtime;
using UnityEditor;

namespace UnitFramework.Editor
{
    [CustomEditor(typeof(MonoUnit))]
    public class MonoUnitInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // TODO : 序列化目标 UNIT 所依赖的所有扩展
           
            
        }
        
    }
}