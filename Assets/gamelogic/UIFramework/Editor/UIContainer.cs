using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using GFramework.Util;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace GFramework.EditorExtern
{
    [CustomEditor(typeof(UI.UIContainer))]
    public class UIContainer : Editor
    {
        private UI.UIContainer container;
        private SerializedProperty bindingViewPath;
        private SerializedProperty bindingViewType;
        private SerializedProperty bindingViewModelType;
        private SerializedProperty varsArr;
        private readonly string prefixPath = "Assets/Resources/Prefabs/UI/";

        private void OnEnable()
        {
            this.container = this.target as UI.UIContainer;
            this.bindingViewPath = this.serializedObject.FindProperty("bindingViewPath");
            this.bindingViewType = this.serializedObject.FindProperty("bindingViewType");
            this.bindingViewModelType = this.serializedObject.FindProperty("bindingViewModelType");
            this.varsArr = this.serializedObject.FindProperty("varsArr");
        }

        private void UpdateTargetView()
        {
            string path = EditorUtility.OpenFilePanel("选择目标View.cs文件", Application.dataPath, "cs");
            this.bindingViewPath.stringValue = path;
            string text = File.ReadAllText(path);
            string matchModel = @"class\s+([a-z A-Z]+)\s+:\s+BaseView<([a-z A-Z]+)>";
            Match match = Regex.Match(text, matchModel);
            if (!match.Success) throw new Exception("匹配类名失败！！！");
            string viewName = match.Groups[1].Value;
            string modelName = match.Groups[2].Value;
            this.bindingViewType.stringValue = viewName;
            this.bindingViewModelType.stringValue = modelName;
            EditorUtility.SetDirty(this.target);
            this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            this.serializedObject.UpdateIfRequiredOrScript();
        }

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
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(this.container.gameObject);
            if (prefabStage != null)
            {
                // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
                assetPath = prefabStage.prefabAssetPath;
            }

            if (string.IsNullOrEmpty(assetPath))
            {
                throw new Exception("请先创建预制体！！！");
            }

            string prefabPath = assetPath.Substring(prefixPath.Length).TrimEnd(".prefab");
            string[] lines = File.ReadAllLines(this.container.bindingViewPath);
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
                    sb.AppendLine($"    public override string BindPath()");
                    sb.AppendLine($"    {{");
                    sb.AppendLine($"        return \"{prefabPath}\";");
                    sb.AppendLine($"    }}");

                    StringBuilder fieldsSB = new StringBuilder();
                    StringBuilder varsSB = new StringBuilder();
                    varsSB.AppendLine($"    protected override void BindVars() {{");
                    for (int j = 0; j < this.varsArr.arraySize; ++j)
                    {
                        SerializedProperty fieldNameProperty = this.varsArr.GetArrayElementAtIndex(j).FindPropertyRelative("fieldName");
                        SerializedProperty componentProperty = this.varsArr.GetArrayElementAtIndex(j).FindPropertyRelative("component");
                        Component component = componentProperty.objectReferenceValue as Component;
                        string fieldType = component.GetType().ToString().GetLastFieldNameWithoutSuffix('.');
                        string fieldName = fieldNameProperty.stringValue;

                        UI.UIContainer uiContainer = component as UI.UIContainer;
                        if (uiContainer != null)
                        {
                            fieldsSB.AppendLine($"    private {uiContainer.bindingViewType} {fieldName};");
                            varsSB.AppendLine($"        this.{fieldName} = this.GetContainer<{uiContainer.bindingViewType}>({j});");
                        }
                        else
                        {
                            fieldsSB.AppendLine($"    private {fieldType} {fieldName};");
                            varsSB.AppendLine($"        this.{fieldName} = this.GetVar<{fieldType}>({j});");
                        }
                    }
                    varsSB.AppendLine($"    }}");
                    sb.Append(fieldsSB);
                    sb.Append(varsSB);
                }
            }
            File.WriteAllText(this.container.bindingViewPath, sb.ToString());
            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            bool updateView = GUILayout.Button("Update View");
            bool updateCode = GUILayout.Button("Update Code");
            EditorGUILayout.EndHorizontal();

            if (updateView)
            {
                this.UpdateTargetView();
            }

            if (updateCode)
            {
                if (string.IsNullOrEmpty(this.container.bindingViewPath) || !File.Exists(this.container.bindingViewPath)) this.UpdateTargetView();
                this.UpdateViewCode();
            }
            string targetViewPath = string.IsNullOrEmpty(this.container.bindingViewPath) ? "未绑定目标view!!!" : this.container.bindingViewPath.Substring(Application.dataPath.Length);
            EditorGUILayout.LabelField(targetViewPath);
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("----------Vars----------");
            EditorGUILayout.BeginVertical();
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
                if (focus)
                {
                    EditorGUIUtility.PingObject(go);
                }
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
                    string compName = comp.GetType().ToString().GetLastFieldNameWithoutSuffix('.');
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


            Rect rect = EditorGUILayout.GetControlRect();
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
    }
}