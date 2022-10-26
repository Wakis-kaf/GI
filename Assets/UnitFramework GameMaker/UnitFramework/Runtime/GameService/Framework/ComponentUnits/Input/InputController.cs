using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class InputController : IInputController
    {

        private bool mEnable = true; 
        public bool Enable { get=>mEnable; }
        private ControllerBlocker mBlocker;
        
        public InputController(ControllerBlocker blocker,InputComponent.ControllerData ctrData)
        {
            mBlocker = blocker;
            InitControllerData(ctrData);
        }

        public bool IsKeyDown(string name)
        {
            return mBlocker.IsKeyDown(name);
        }
        public bool IsKeyUp(string name)
        {
            return mBlocker.IsKeyUp(name);
        }
        public bool IsKeyPushing(string name)
        {
            return mBlocker.IsKeyPushing(name);
        }

        public float Value(string name)
        {
            return mBlocker.Value(name);
        }

        public Vector2 Pos(string name)
        {
            return mBlocker.Pos(name);
        }
        private void InitControllerData(InputComponent.ControllerData ctrData )
        {
            SetEnable(ctrData.awakeEnable);
        }
        
        public void SetEnable(bool enable)
        {
            mEnable = enable;
            mBlocker.setEnabled(enable);
            
        }
    }
}