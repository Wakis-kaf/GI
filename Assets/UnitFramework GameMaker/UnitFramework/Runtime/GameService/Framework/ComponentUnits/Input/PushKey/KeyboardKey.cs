using UnityEngine;

namespace UnitFramework.Runtime
{
    public class KeyboardKey : PushKey
    {
        private KeyCode mBindKey;
        public override bool IsDown => Input.GetKeyDown(mBindKey);
        public override bool IsUp => Input.GetKeyUp(mBindKey);
        public override bool IsPushing => Input.GetKey(mBindKey);
        public KeyboardKey(KeyCode k){
            this.mBindKey = k;
        }
        public void ResetKey(KeyCode k){
            this.mBindKey = k;
        }
    }
}