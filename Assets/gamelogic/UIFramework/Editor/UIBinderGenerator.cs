using System.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;
using UGFramework.Util;

using Object = UnityEngine.Object;

namespace UGFramework.UI
{
    [CustomEditor(typeof(UIBinder))]
    public class UIBinderGenerator : UnityEditor.Editor
    {
        private UIBinder binder;
        private SerializedProperty bindingCS;
        private SerializedProperty varsArr;

        private void OnEnable()
        {
            this.binder = this.target as UIBinder;
            this.bindingCS = this.serializedObject.FindProperty("cs");
            this.varsArr = this.serializedObject.FindProperty("varsArr");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            // NOTE: 快捷指令
            EditorGUILayout.BeginHorizontal();
            bool updateCode = GUILayout.Button("更新代码");
            bool jumpCode = GUILayout.Button("跳转脚本");
            if (updateCode) this.UpdateViewCode();
            if (jumpCode) AssetDatabase.OpenAsset(this.bindingCS.objectReferenceValue);
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();
            EditorGUILayout.LabelField("----------Vars----------");
            // NOTE: 游戏组件列表
            // 获取当期已经绑定的对象列表
            for (int i = 0; i < this.varsArr.arraySize; ++i)
            {
                SerializedProperty fieldName = this.varsArr.GetArrayElementAtIndex(i).FindPropertyRelative("fieldName");
                SerializedProperty gameObject = this.varsArr.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                SerializedProperty component = this.varsArr.GetArrayElementAtIndex(i).FindPropertyRelative("component");
                GameObject go = gameObject.objectReferenceValue as GameObject;
                Component selectedComp = component.objectReferenceValue as Component;

                EditorGUILayout.BeginHorizontal();
                // 引用对象
                bool focus = GUILayout.Button(new GUIContent($"{i}.{go.name.Trim('#')}"), GUILayout.Width(100));
                if (focus) EditorGUIUtility.PingObject(go);
                // 字段名字
                string newFieldName = EditorGUILayout.TextField(fieldName.stringValue, GUILayout.MaxWidth(100));
                if (newFieldName != fieldName.stringValue)
                {
                    fieldName.stringValue = newFieldName;
                    EditorUtility.SetDirty(this.target);
                    this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    this.serializedObject.UpdateIfRequiredOrScript();
                }
                // 组件选项
                List<string> componentOptions = new List<string>();
                int selectedIndex = 0;
                Component[] comps = go.GetComponents<Component>();
                for (int j = 0; j < comps.Length; ++j)
                {
                    Component comp = comps[j];
                    if (comp == selectedComp) selectedIndex = j;
                    string compName = comp.GetType().ToString().GetLastFieldName('.');
                    componentOptions.Add($"{j}.{compName}");
                }
                int optionIndex = EditorGUILayout.Popup(selectedIndex, componentOptions.ToArray());
                if (optionIndex != selectedIndex)
                {
                    this.varsArr.GetArrayElementAtIndex(i).FindPropertyRelative("component")
                        .objectReferenceValue = comps[optionIndex];
                    EditorUtility.SetDirty(this.target);
                    this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    this.serializedObject.UpdateIfRequiredOrScript();
                }

                // 删除引用
                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    this.varsArr.DeleteArrayElementAtIndex(i);
                    go.name = go.name.Trim('#');

                    EditorUtility.SetDirty(this.target);
                    this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    this.serializedObject.UpdateIfRequiredOrScript();
                }

                EditorGUILayout.EndHorizontal();
            }

            // NOTE: 拖拽获取组件
            Rect rect = EditorGUILayout.GetControlRect(true, 60);
            if (rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (Event.current.type == EventType.DragExited)
                {
                    Object[] objs = DragAndDrop.objectReferences;
                    for (int i = 0; i < objs.Length; ++i)
                    {
                        GameObject go = objs[i] as GameObject;
                        if (!go) return;
                        string goName = go.name.Trim('#');
                        this.varsArr.InsertArrayElementAtIndex(this.varsArr.arraySize);
                        SerializedProperty element = this.varsArr.GetArrayElementAtIndex(this.varsArr.arraySize - 1);
                        element.FindPropertyRelative("fieldName").stringValue = goName;
                        element.FindPropertyRelative("gameObject").objectReferenceValue = go;
                        element.FindPropertyRelative("component").objectReferenceValue = go.transform;
                        go.name = '#' + go.name.Trim('#');
                        EditorUtility.SetDirty(this.target);
                        this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        this.serializedObject.UpdateIfRequiredOrScript();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 更新view代码
        /// </summary>
        private void UpdateViewCode()
        {
            string assetPath = null;
            // Project中的Prefab是Asset不是Instance
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this.target))
            {
                // 预制体资源就是自身
                assetPath = AssetDatabase.GetAssetPath(this.target);
            }

            // Scene中的Prefab Instance是Instance不是Asset
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this.target))
            {
                // 获取预制体资源
                var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(this.target);
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(prefabAsset);
            }

            // PrefabMode中的GameObject既不是Instance也不是Asset
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(this.binder.gameObject);
            if (prefabStage != null)
            {
                // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
                assetPath = prefabStage.assetPath;
            }

            if (string.IsNullOrEmpty(assetPath))
                throw new NoNullAllowedException("请先创建预制体！！！");

            var prefabPath = assetPath.Substring(Path.Combine("Assets", UIConfig.prefabRoot).PathFormat().Length).TrimSuffix(".prefab");
            var fileName = prefabPath.GetLastFieldName().FetchAlpAndDigAndLine();
            var csPath = Path.Combine(Application.dataPath, UIConfig.csharpRoot, prefabPath.GetDirectory(), fileName + ".cs").PathFormat();
            if (null == this.bindingCS.objectReferenceValue)
            {
                UGFramework.Util.FileUtil.DepCreateFile(csPath);
                // NOTE:  必须刷新，否则csAsset找不到
                AssetDatabase.Refresh();
                Debug.Log($"创建C#脚本，在{csPath}");
                var csAssetPath = "Assets" + csPath.Substring(Application.dataPath.PathFormat().Length);
                var csAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csAssetPath);
                this.bindingCS.objectReferenceValue = csAsset;
                AutoGenBaseView();
                EditorUtility.SetDirty(this.target);
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.UpdateIfRequiredOrScript();
            }

            AutoUpdateCodeOfBaseView(prefabPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 是否能够匹配CS类名
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string IsMatchCSClassName(string text)
        {
            string classNameMatch = @"class\s+([a-z A-Z]+)\s+:\s+BaseView";
            Match result = Regex.Match(text, classNameMatch);
            if (!result.Success) return null;
            return result.Groups[1].Value;
        }

        /// <summary>
        /// 生成view模板
        /// </summary>
        private void AutoGenBaseView()
        {
            TextAsset cs = this.bindingCS.objectReferenceValue as TextAsset;
            var className = cs.name.Trim('#').PascalFormat();
            // 生成代码模板
            var codeTemplate = @$"using UGFramework.UI;
            using UnityEngine;using UnityEngine.UI;
            namespace GameLogic.UI{{
                public class UI{className}: BaseView
                {{
                    // ++
                    // --
                }}
            }}";
            var csPath = Path.Combine(Application.dataPath.TrimSuffix("/Assets"), AssetDatabase.GetAssetPath(cs)).PathFormat();
            File.WriteAllText(csPath, codeTemplate);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成预制体路径、组件获取代码
        /// </summary>
        /// <param name="prefabPath"></param>
        private void AutoUpdateCodeOfBaseView(string prefabPath)
        {
            TextAsset cs = this.bindingCS.objectReferenceValue as TextAsset;
            string codeTxt = cs.text;
            // NOTE: 没有找到类名，重新生成
            if (string.IsNullOrEmpty(IsMatchCSClassName(codeTxt)))
                AutoGenBaseView();
            string[] lines = codeTxt.Split('\n');
            StringBuilder sb = new StringBuilder();
            bool flag = true;
            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Contains("// --"))
                {
                    flag = true;
                }
                if (!flag) continue;
                sb.AppendLine(lines[i]);
                if (lines[i].Contains("// ++"))
                {
                    flag = false;
                    var bindPathRegion = @$"public override string BindPath()
                    {{
                        return ""{prefabPath}"";
                    }}";
                    sb.AppendLine(bindPathRegion);

                    StringBuilder fieldsSB = new StringBuilder();
                    StringBuilder varsSB = new StringBuilder();
                    varsSB.AppendLine($"protected override void BindVars() {{");
                    for (int j = 0; j < this.varsArr.arraySize; ++j)
                    {
                        SerializedProperty fieldNameProperty = this.varsArr.GetArrayElementAtIndex(j).FindPropertyRelative("fieldName");
                        SerializedProperty componentProperty = this.varsArr.GetArrayElementAtIndex(j).FindPropertyRelative("component");
                        Component component = componentProperty.objectReferenceValue as Component;
                        string fieldType = component.GetType().ToString().GetLastFieldName('.');
                        string fieldName = fieldNameProperty.stringValue;

                        UIBinder uiBinder = component as UIBinder;
                        if (uiBinder != null)
                        {
                            TextAsset bindingCS = uiBinder.cs;
                            string className = IsMatchCSClassName(bindingCS.text);
                            if (string.IsNullOrEmpty(className)) throw new NoNullAllowedException("类名匹配失败！！！");
                            var fieldLine = $"private {className} {fieldName};";
                            var fetchLine = $"this.{fieldName} = this.GetBinder<{className}>({j});";
                            fieldsSB.AppendLine(fieldLine);
                            varsSB.AppendLine(fetchLine);
                        }
                        else
                        {
                            fieldsSB.AppendLine($"private {fieldType} {fieldName};");
                            varsSB.AppendLine($"this.{fieldName} = this.GetVar<{fieldType}>({j});");
                        }
                    }
                    varsSB.AppendLine($"}}");
                    sb.Append(fieldsSB);
                    sb.Append(varsSB);
                }
            }
            var csPath = Path.Combine(Application.dataPath.TrimSuffix("/Assets"), AssetDatabase.GetAssetPath(cs)).PathFormat();
            File.WriteAllText(csPath, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}