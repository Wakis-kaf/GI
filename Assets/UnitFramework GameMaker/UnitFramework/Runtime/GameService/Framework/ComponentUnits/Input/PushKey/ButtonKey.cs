using UnityEngine;

namespace UnitFramework.Runtime
{
    public class ButtonKey : PushKey
    {
        public override bool IsDown { get=>Input.GetButtonDown(mBtnKey); }
        public override bool IsUp { get=>Input.GetButtonUp(mBtnKey); }
        public override bool IsPushing { get=>Input.GetButton(mBtnKey); }
        private string mBtnKey;
        public  ButtonKey(string buttonName)
        {
            mBtnKey = buttonName;
        }
    }
}