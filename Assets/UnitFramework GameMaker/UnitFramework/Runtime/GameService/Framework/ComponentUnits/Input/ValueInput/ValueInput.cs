using UnityEngine;

namespace UnitFramework.Runtime
{
    public  abstract class ValueInput
    {
        public abstract float Value{get;}
    }
    public class ScrollInputX: ValueInput{
        public override float Value{get => Input.mouseScrollDelta.x; }
    }
    public class ScrollInputY: ValueInput{
        public override float Value{get => Input.mouseScrollDelta.y; }
    }
    public class AxisInput: ValueInput{
        private string mBindAxisName;
        public override float Value{get => Input.GetAxis(mBindAxisName); }
        public AxisInput(string name){
            this.mBindAxisName = name;
        }
        public void ResetName(string name){
            this.mBindAxisName = name;
        }
    } 
}