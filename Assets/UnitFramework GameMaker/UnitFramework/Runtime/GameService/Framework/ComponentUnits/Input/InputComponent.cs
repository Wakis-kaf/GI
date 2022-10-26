using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace  UnitFramework.Runtime
{
    /// <summary>
    ///  框架输入系统封装
    /// </summary>
    public class InputComponent : SingletonComponentUnit<InputComponent>
    {
        public override string ComponentUnitName { get=>"InputComponent"; }
        
        [SerializeField] private InputData[] inputs;
        [SerializeField] private ControllerData[] controllers;

        private Dictionary<string, InputController> mControllersMap;
        private ControllerBase mSourceInput;

        public InputController this[string controllerName]
        {
            get
            {
                if (string.IsNullOrEmpty(controllerName) || !mControllersMap.ContainsKey(controllerName))
                    return null;
                return mControllersMap[controllerName];
            }
        }
        public InputComponent(){
            mSourceInput = new ControllerBase();
            mControllersMap =new Dictionary<string, InputController>();
        }

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            
            //  初始化输入
            if (!ReferenceEquals(inputs, null) && inputs.Length > 0)
            {
                // 创建input
                for (int i = 0; i < inputs.Length; i++)
                {
                    var input = inputs[i];
                    switch (input.inputType)
                    {
                        case InputType.Keyboard:
                            mSourceInput.Register(input.inputName,new KeyboardKey(input.key));
                            break;
                        case InputType.MouseButton:
                            mSourceInput.Register(input.inputName,new MouseKey(input.mouseKey));
                            break;
                        case InputType.Button:
                            mSourceInput.Register(input.inputName,new ButtonKey(input.btnName));
                            break;
                        case InputType.ScrollXValue:
                            mSourceInput.Register(input.inputName,new ScrollInputX());
                            break;
                        case InputType.ScrollYValue:
                            mSourceInput.Register(input.inputName,new ScrollInputY());
                            break;
                        case InputType.AxisValue:
                            mSourceInput.Register(input.inputName,new AxisInput(input.axisName));
                            break;
                        case InputType.MousePosition:
                            mSourceInput.Register(input.inputName,new MousePosition());
                            break;
                        
                    }
                }
            }
          
            if (!ReferenceEquals(controllers, null) && controllers.Length > 0)
            {
                
                // 创建 Controllers
                for (int i = 0; i < controllers.Length; i++)
                {
                    var ctrData = controllers[i];
                    string name = ValidateControllerName(ctrData.controllerName) ;
                    InputController controller = new InputController(new ControllerBlocker(mSourceInput),ctrData);
                    mControllersMap.Add(name,controller);
                }
            }
        }

        private string ValidateControllerName(string name)
        {
            return name;
        }

        [System.Serializable]
        public class InputData
        {
            public string inputName;
            public InputType inputType;
            [ShowIf("inputType",InputType.Keyboard)]
            public KeyCode key;
            [ShowIf("inputType",InputType.Button)]
            public string btnName;
            [ShowIf("inputType",InputType.AxisValue)]
            public string axisName;
            [ShowIf("inputType",InputType.MouseButton)]
            public int mouseKey;
        }

        [System.Serializable]
        public  class ControllerData
        {
            public string controllerName;
            public bool awakeEnable = true;
        }

        public enum InputType
        {
            Keyboard,
            MouseButton,
            Button,
            ScrollXValue,
            ScrollYValue,
            AxisValue,
            MousePosition,
            
            
        }
    }
    
    

}
