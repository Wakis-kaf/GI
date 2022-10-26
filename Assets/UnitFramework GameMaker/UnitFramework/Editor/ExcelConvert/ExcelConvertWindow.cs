using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnitFramework.Runtime;
using UnitFramework.Utils;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Windows;
using EditorUtility = UnitFramework.Utils.EditorUtility;

//using EditorUtility = UnitFramework.Utils.EditorUtility;

namespace UnitFramework.Editor
{
    public class ExcelConvertWindow : EditorWindow
    {
        private static string convertFilterName = "ConvertTargetTypesFilter";
        private UnityEngine.Object m_ExcelObj;
        private UnityEngine.Object m_LastExcelObj;
        private string m_ExcelObjErrorMsg;
        private string m_ConvertObjErrorMsg;
        private string m_ConvertObjInfoMsg;
        private string m_ConvertPath;
        private string m_ConvertName = "New Excel Data";
        private bool m_IsCurrentActivePath = true;
        private bool m_IsPickMode = true;
        private string m_ConvertTargetType;
        private int m_ConvertTargetTypeIndex;
        private string[] m_ConvertTargetTypes = new string[0];
        [SerializeField] private List<string> m_ConvertTargetTypesFilter = new List<string>();

        //private int m_LastFilterHashCode;
        private SerializedObject m_SerializedObject;

        //序列化属性
        private SerializedProperty m_AssetLstProperty;
        private Type[] m_AssemblyTypes = new Type[0];

        [MenuItem("UnitFramework/Window/Excel Asset Convert Window")]
        private static void OpenWindow()
        {
            //获得一个已存在的MyNodeEditor窗口，若没有则创建一个新的：
            ExcelConvertWindow window = GetWindow<ExcelConvertWindow>("ExcelConvertWindow");
            window.Show();
        }

        private void OnFocus()
        {
            //Reload();
        }

        private void OnProjectChange()
        {
            Load();
        }

        private void OnEnable()
        {
            Load();
        }

        private void OnDisable()
        {
            Save();
        }

        private void Load()
        {
            //使用当前类初始化
            m_SerializedObject = new SerializedObject(this);
            //获取当前类中可序列话的属性
            m_AssetLstProperty = m_SerializedObject.FindProperty("m_ConvertTargetTypesFilter");

            if (PlayerPrefs.HasKey(convertFilterName))
            {
                string content = PlayerPrefs.GetString(convertFilterName);
                if (string.IsNullOrEmpty(content))
                {
                    m_ConvertTargetTypesFilter.Clear();
                }
                else
                {
                    m_ConvertTargetTypesFilter = content.Split(';').ToList();
                }
            }
            else
            {
                m_ConvertTargetTypesFilter.Clear();
            }

            m_AssemblyTypes = Utility.Assembly.GetTypes().Where(t =>
            {
                return t.IsClass && t.IsSubclassOf(typeof(ScriptableObject)) &&
                       typeof(IExcelDataReceiver).IsAssignableFrom(t);
            }).ToArray();
            LoadConvertTypes();
        }

        private void LoadConvertTypes()
        {
            List<string> typeNames = new List<string>();
            for (int i = 0; i < m_AssemblyTypes.Length; i++)
            {
                string name = m_AssemblyTypes[i].ToString();
                bool isAdd = true;
                if (!m_IsPickMode)
                {
                    for (int j = 0; j < m_ConvertTargetTypesFilter.Count; j++)
                    {
                        if (name.ToLower().Contains(m_ConvertTargetTypesFilter[j].ToLower()))
                        {
                            isAdd = false;
                            break;
                        }
                    }
                }
                else
                {
                    isAdd = false;
                    for (int j = 0; j < m_ConvertTargetTypesFilter.Count; j++)
                    {
                        if (name.ToLower().Contains(m_ConvertTargetTypesFilter[j].ToLower()))
                        {
                            isAdd = true;
                            break;
                        }
                    }
                }

                if (isAdd)
                    typeNames.Add(name);
            }

            m_ConvertTargetTypes = typeNames.ToArray();
        }

        private void Save()
        {
            string content = string.Join(";", m_ConvertTargetTypesFilter);
            Debug.Log(content);
            PlayerPrefs.SetString(convertFilterName, content);
        }


        private void OnGUI()
        {
            //更新
            m_SerializedObject.Update();
            m_ExcelObj = EditorGUILayout.ObjectField("Excel 文件", m_ExcelObj, typeof(UnityEngine.Object), false);
            if (m_ExcelObj != null && m_LastExcelObj != m_ExcelObj)
            {
                int instanceId = m_ExcelObj.GetInstanceID();
                // 查找
                string assetPath = AssetDatabase.GetAssetPath(instanceId);
                Debug.Log(assetPath);
                if (!assetPath.EndsWith(".xls"))
                {
                    m_ExcelObj = null;
                    m_ExcelObjErrorMsg = "请选择一个Excel(.xls) 文件";
                }
                else
                {
                    m_ExcelObjErrorMsg = string.Empty;
                }
            }

            // 显示提示文字
            if (!string.IsNullOrEmpty(m_ExcelObjErrorMsg))
            {
                GUIStyle style = EditorStyles.helpBox;
                style.normal.textColor = Color.red;
                GUILayout.Box(m_ExcelObjErrorMsg, style);
            }

            m_ConvertName = EditorGUILayout.TextField("转换文件名", m_ConvertName);
            GUILayout.BeginHorizontal();

            if (m_IsCurrentActivePath)
            {
                EditorGUI.BeginDisabledGroup(m_IsCurrentActivePath);
                m_ConvertPath = EditorUtility.EditorAssetDatabase.GetCurrentAssetDirectory();
                m_ConvertPath = EditorGUILayout.TextField("转换路径", m_ConvertPath);

                EditorGUI.EndDisabledGroup();
            }
            else
            {
                // 获取当前Project 的目录

                m_ConvertPath = EditorGUILayout.TextField("转换路径", m_ConvertPath);
            }

            m_IsCurrentActivePath = GUILayout.Toggle(m_IsCurrentActivePath, "锁定当前Project路径 ", GUILayout.Width(150f));
            //m_IsCurrentActivePath = EditorGUILayout.Toggle("是否是当前路径", m_IsCurrentActivePath);
            GUILayout.EndHorizontal();
            //开始检查是否有修改
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_AssetLstProperty, new GUIContent("转换类型筛选"), true);


            //结束检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                //提交修改
                m_SerializedObject.ApplyModifiedProperties();
                LoadConvertTypes();
            }

            GUILayout.BeginHorizontal();
            m_ConvertTargetTypeIndex =
                EditorGUILayout.Popup("Convert Target Type", m_ConvertTargetTypeIndex, m_ConvertTargetTypes);

            EditorGUI.BeginChangeCheck();
            m_IsPickMode = GUILayout.Toggle(m_IsPickMode, "匹配模式", GUILayout.Width(150f));
            //结束检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                LoadConvertTypes();
            }

            GUILayout.EndHorizontal();

            // 显示转换按钮
            if (GUILayout.Button("Start Convert"))
            {
                StartConvert();
            }

            if (!string.IsNullOrEmpty(m_ConvertObjErrorMsg))
            {
                GUIStyle style = GUI.skin.label;

                style.normal.textColor = Color.red;
                GUILayout.Label(m_ConvertObjErrorMsg, style);
            }

            GUILayout.Label(m_ConvertObjInfoMsg);

            m_LastExcelObj = m_ExcelObj;
        }

        private void StartConvert()
        {
            m_ConvertObjErrorMsg = string.Empty;
            if (m_ExcelObj == null)
            {
                m_ConvertObjErrorMsg = "convert target is null";
                return;
            }

            if (m_ConvertTargetTypes.Length == 0 || m_ConvertTargetTypeIndex >= (m_ConvertTargetTypes.Length))
            {
                m_ConvertObjErrorMsg = "convert target type name invalidate";
                return;
            }

            string dataSOType = m_ConvertTargetTypes[m_ConvertTargetTypeIndex];
            Type type = Utility.Assembly.GetType(dataSOType);
            if (type == null)
            {
                m_ConvertObjErrorMsg = $"convert target type : {dataSOType} invalidate";
                return;
            }

            try
            {
                // 读取excel
                string assetPath = AssetDatabase.GetAssetPath(m_ExcelObj.GetInstanceID());
                string dataPath = Application.dataPath;
                dataPath = dataPath.Substring(0, dataPath.IndexOf("/Assets"));
                string filePath = dataPath + "/" + assetPath;
                Debug.Log(filePath);
                //AssetDatabase.LoadAssetAtPath(assetPath,typeof(importAsset));
                var data = ExcelLoader.ReadExcel(filePath);
                //  创建文件
                ScriptableObject so = ScriptableObject.CreateInstance(type);
                IExcelDataReceiver dataReceiver = so as IExcelDataReceiver;
                dataReceiver.InitWith(data);

                // 获取完整文件路径
                string path = $"{m_ConvertPath}/{m_ConvertName}.asset";
                string fullTargetPath = $"{dataPath}/{path}";
                Debug.Log(fullTargetPath);
                // 删除文件
                if (File.Exists(fullTargetPath))
                {
                    File.Delete(fullTargetPath);
                    AssetDatabase.Refresh();
                }

                // 创建新的 ScriptObject 
                AssetDatabase.CreateAsset(so, path);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                m_ConvertObjErrorMsg = e.Message;
            }
        }
    }
}