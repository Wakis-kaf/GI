using System;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class ControllerBlocker
    {
        private ControllerBase sourceInput;
        private Func<string, float> __fetch_value;
        private Func<string, bool> __fetch_keydown;
        private Func<string, bool> __fetch_keyup;
        private Func<string, bool> __fetch_keypushing;
        private Func<string, Vector2> __fetch_pos;
    
        public ControllerBlocker(ControllerBase src){
            this.sourceInput = src;
        }
        private float __disabled_value(string key){
            return 0;
        }
        private bool __disabled_pushkey(string key){
            return false;
        }
        private Vector2 __disabled_pos(string key){
            return Vector2.zero;
        }
    
        /* 对外的API */
        public void setEnabled(bool value){
            if(value){
                __fetch_value = sourceInput.Value;
                __fetch_keydown = sourceInput.IsKeyDown;
                __fetch_keyup = sourceInput.IsKeyUp;
                __fetch_keypushing = sourceInput.IsKeyPushing;
                __fetch_pos = sourceInput.Pos;
            }else{
                __fetch_value = __disabled_value;
                __fetch_keydown = __disabled_pushkey;
                __fetch_keyup = __disabled_pushkey;
                __fetch_keypushing = __disabled_pushkey;
                __fetch_pos = __disabled_pos;
            }
        }
        public bool IsKeyDown(string key){
            return __fetch_keydown(key);
        }
        public bool IsKeyUp(string key){
            return __fetch_keyup(key);
        }
        public bool IsKeyPushing(string key){
            return __fetch_keypushing(key);
        }
        public float Value(string key){
            return __fetch_value(key);
        }
        public Vector2 Pos(string key){
            return __fetch_pos(key);
        }
    }
}