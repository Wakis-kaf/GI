using System;

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnitFramework.Runtime;
using UnitFramework.Utils;

namespace UnitFramework.Editor
{
    /// <summary>
    /// 自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(ProcedureComponent.FSMProcessConfig))]
    [DrawWithUnity]
    public class FSMProcessConfigDrawer : PropertyDrawer
    {
        private  float _heightOneLine = 20f;
        private const float Space = 5;
        private SerializedProperty _availableProcedureTypeName;
        private SerializedProperty _entranceProcedureTypeName;
        private SerializedProperty _currentProcedureTypeName;
        private Rect _header;
        private string[] _procedureTypeNames = new string[0];
        
        private List<string> _availableProcedureTypeNames = new List<string>();
        private List<string> _currentProcedureTypeNames = new List<string>();
        
        
        private float _currentY;
        private int _entranceProcedureIndex = 0;
        private string _entranceProcedureName = null;
        Dictionary<string,bool> _toggles = new Dictionary<string, bool>(10);
        private bool _foldOut;

        /// <summary>
        /// 绘制属性
        /// </summary>
        /// <param name="position">应该使用窗口的哪个区域来绘制属性</param>
        /// <param name="property">属性本身</param>
        /// <param name="label">应该为属性设置的标签</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);
            // 获取属性
            _availableProcedureTypeName = property.FindPropertyRelative("_availableProcedureTypeNames");
            _entranceProcedureTypeName = property.FindPropertyRelative("_entranceProcedureTypeName");
            _currentProcedureTypeName = property.FindPropertyRelative("_currentProcedureTypeNames");

            // 读取数据
            // 读取序列化对象中的数据
            ReadProcedureTypeNames();
            // 通过反射获取Type
            RefreshTypeNames();
            InitToggle();
            
            
            // 获取初始位置
            Rect singlePos = position; 
            singlePos.height = _heightOneLine;
           // 构建折叠头
           _foldOut = EditorGUI.BeginFoldoutHeaderGroup(singlePos, _foldOut, "FSM Config");
           singlePos.y += _heightOneLine;
        
           // 展开
           if (_foldOut)
           {
               UIConstructor(singlePos,property,label);
           }
           // 关闭折叠组
           EditorGUI.EndFoldoutHeaderGroup();
           
           // 更新数据
           WriteProcedureTypeNames();
           _entranceProcedureTypeName.stringValue = _entranceProcedureName;
           
           EditorGUI.EndChangeCheck();
           EditorGUI.EndProperty();
           property.serializedObject.ApplyModifiedProperties();
        }

        private void ReadProcedureTypeNames()
        {
            _availableProcedureTypeNames.Clear();
            _currentProcedureTypeNames.Clear();
            for (int i = 0; i < _availableProcedureTypeName.arraySize; i++)
            {
                // 添加数据
                string name = _availableProcedureTypeName.GetArrayElementAtIndex(i).stringValue;
                _availableProcedureTypeNames.Add(name);
                
            } 
            for (int i = 0; i < _currentProcedureTypeName.arraySize; i++)
            {
                // 添加数据
                string name = _currentProcedureTypeName.GetArrayElementAtIndex(i).stringValue;
                _currentProcedureTypeNames.Add(name);
                
            }
            
           
            
        }
        /// <summary>
        /// 刷新类型名字
        /// </summary>
        private  void RefreshTypeNames()
        {
            // 获取所有的类型名字
            _procedureTypeNames = ReflectionTool.GetDerivedClassTypeName<Procedure>().ToArray();
            if(_procedureTypeNames.Length> _entranceProcedureIndex)
                _entranceProcedureName = _procedureTypeNames[_entranceProcedureIndex];
            // _currentProcedureTypeNames.Clear();
            if(!string.IsNullOrEmpty(_entranceProcedureTypeName.stringValue) )
                _entranceProcedureName = _entranceProcedureTypeName.stringValue;
            for (int i = 0; i < _procedureTypeNames.Length; i++)
            {
                string name = _procedureTypeNames[i];
                if (!_availableProcedureTypeNames.Contains(name))
                {
                    _availableProcedureTypeNames.Add(name);
                    _currentProcedureTypeNames.Add(name);
                }

                if (_entranceProcedureName == name)
                {
                    _entranceProcedureIndex = i;
                }
            }
           
            
        }


        private void InitToggle()
        {
            _toggles.Clear();
            foreach (var name in _availableProcedureTypeNames)
            {
                if (_currentProcedureTypeNames.Contains(name))
                {
                    _toggles.Add(name,true);
                }
                else
                {
                    _toggles.Add(name,false);
                }
                
            }
        }

        /// <summary>
        /// 构建UI
        /// </summary>
        private void UIConstructor(Rect position, SerializedProperty property, GUIContent label)
        {
            GetRectPosition(position, out Rect title,out Rect boxContent,out Rect footer);
            // 创建标题头
            var textStyle = new GUIStyle()
            {
                fontSize  = 13,
                fontStyle =  FontStyle.Bold,
            };
            textStyle.normal.textColor = Color.white;
            EditorGUI.LabelField(title,
                "Available Procedures",textStyle);
            // 创建中间的内容边框
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextArea(boxContent,"");
            EditorGUI.EndDisabledGroup();
            // 创建Names
            TypeNamesItemConstructor(boxContent);
            // 构建程序入口
            footer.width *= 0.75f;
            EditorGUI.LabelField(footer,"Entrance Procedure");
            footer.x += footer.width / 3;
            int selectedIndex = EditorGUI.Popup(footer, _entranceProcedureIndex, _procedureTypeNames);
            if (selectedIndex != _entranceProcedureIndex)
            {
                _entranceProcedureIndex = selectedIndex;
                _entranceProcedureName = _procedureTypeNames[selectedIndex];
            }
        }
        private void WriteProcedureTypeNames()
        {
            _currentProcedureTypeName.ClearArray();
            if(_entranceProcedureName == null) return;
            
            if(_toggles.ContainsKey(_entranceProcedureName) && !_toggles[_entranceProcedureName])
            {
                _toggles[_entranceProcedureName] = true;
                _currentProcedureTypeNames.Add(_entranceProcedureName);
            }
            
            
            for (int i = 0; i < _currentProcedureTypeNames.Count; i++)
            {
                _currentProcedureTypeName.InsertArrayElementAtIndex(i);
                _currentProcedureTypeName.GetArrayElementAtIndex(i).stringValue = _currentProcedureTypeNames[i];
            }
            _availableProcedureTypeName.ClearArray();
            for (int i = 0; i < _availableProcedureTypeNames.Count; i++)
            {
                _availableProcedureTypeName.InsertArrayElementAtIndex(i);
                _availableProcedureTypeName.GetArrayElementAtIndex(i).stringValue = _availableProcedureTypeNames[i];
            }
       
            //if(_availableProcedureTypeNames)
        }



        private void GetRectPosition(Rect position,out Rect title,out Rect boxContent,out Rect foot)
        {
            position.y += Space;
            Rect singleRect = position;
            
            title = singleRect;
            
            singleRect.y += _heightOneLine + Space;
            boxContent = singleRect;
            boxContent.height = _procedureTypeNames.Length * (_heightOneLine + Space);

            foot = boxContent;
            
            foot.y = foot.y + foot.height + Space*2;
            foot.height = _heightOneLine;

        }

     
    

   
        private Rect GetNextItemPosition(Rect boxContent, int index,out Rect toggleLeftPosition)
        {
            Rect itemPos = new Rect(boxContent.x, boxContent.y+ index * (_heightOneLine + 5), boxContent.width, _heightOneLine + 5);
            toggleLeftPosition = itemPos;
            toggleLeftPosition.x += 15;
            
            return itemPos;
        }
        private void TypeNamesItemConstructor(Rect boxContent)
        {
            for (int i = 0; i < _procedureTypeNames.Length; i++)
            {
                string name = _procedureTypeNames[i];
                Rect position = GetNextItemPosition(boxContent,i,out Rect toggleLeftPosition);
                _toggles[name] = EditorGUI.ToggleLeft(toggleLeftPosition,name,_toggles[name]);
                if (_toggles[name] && !_currentProcedureTypeNames.Contains(name))
                {
                    _currentProcedureTypeNames.Add(name);
                }
                else if(!_toggles[name] && _currentProcedureTypeNames.Contains(name))
                {
                    _currentProcedureTypeNames.Remove(name);
                }
            }
            

            //WriteProcedureTypeNames();

        }

      
        /// <summary>
        /// 绘制属性的高度
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // while (property.NextVisible(true))
            // {
            //     Debug.Log("GetPropertyHeight："+property.name);
            // }
            // 4行
            
            _heightOneLine = base.GetPropertyHeight(property, label);
            if (_foldOut)
            {
                return _heightOneLine * (_procedureTypeNames.Length+5);    
            }
            return _heightOneLine;

        }
    }
}