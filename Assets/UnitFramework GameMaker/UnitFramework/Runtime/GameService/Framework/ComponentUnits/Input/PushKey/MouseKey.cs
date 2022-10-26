using UnityEngine;

namespace UnitFramework.Runtime
{
    public class MouseKey : PushKey
    {
        private int mBindKey;
        public override bool IsDown{get => Input.GetMouseButtonDown(mBindKey); }
        public override bool IsUp{get => Input.GetMouseButtonUp(mBindKey); }
        public override bool IsPushing { get=>Input.GetMouseButton(mBindKey); }
        public MouseKey(int key){
            this.mBindKey = key;
        }
        public void ResetKey(int key){
            this.mBindKey = key;
        }
    }
}