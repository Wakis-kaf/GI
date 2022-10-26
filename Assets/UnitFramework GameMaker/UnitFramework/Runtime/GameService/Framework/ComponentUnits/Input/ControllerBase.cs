using System.Collections.Generic;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class ControllerBase
    {
        /* 将所有的映射都写到字典中，以方便改键 */
        private Dictionary<string, PushKey> mPushKeyMaps;
        private Dictionary<string, ValueInput> mValueInputMaps;
        private Dictionary<string, MousePosition> mMousePositionMaps;
        public ControllerBase(){
            mPushKeyMaps = new Dictionary<string, PushKey>();
            mValueInputMaps = new Dictionary<string, ValueInput>();
            mMousePositionMaps = new Dictionary<string, MousePosition>();
        }
        public bool IsKeyDown(string key){
            return mPushKeyMaps[key].IsDown;
        }
        public bool IsKeyUp(string key){
            return mPushKeyMaps[key].IsUp;
        }
        public bool IsKeyPushing(string key){
            return mPushKeyMaps[key].IsPushing;
        }
        public float Value(string key){
            return mValueInputMaps[key].Value;
        }
        public Vector2 Pos(string key){
            return mMousePositionMaps[key].Pos;
        }
        public void Register(string key, PushKey pushKey){
            if(mPushKeyMaps.ContainsKey(key)){
                mPushKeyMaps[key] = pushKey;
            }else{
                mPushKeyMaps.Add(key, pushKey);
            }
        }
        public void Register(string key, ValueInput valueInput){
            if(mValueInputMaps.ContainsKey(key)){
                mValueInputMaps[key] = valueInput;
            }else{
                mValueInputMaps.Add(key, valueInput);
            }
        }
        public void Register(string key, MousePosition pos){
            if(mMousePositionMaps.ContainsKey(key)){
                mMousePositionMaps[key] = pos;
            }else{
                mMousePositionMaps.Add(key, pos);
            }
        }
    }
}