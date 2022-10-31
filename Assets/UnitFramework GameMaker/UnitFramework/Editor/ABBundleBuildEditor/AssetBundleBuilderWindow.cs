using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnitFramework.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnitFramework.Editor
{
    public class AssetBundleBuilderWindow : EditorWindow
    {
        static AssetBundleBuilderWindow()
        {
            EditorApplication.projectChanged += OnProjectChanged;
        }


        private static string windosABName = "WindowsABAssets";
        private static string androidABName = "AndroidABAssets";
        private static string iosABName = "IPhoneABAssets";
        private static string abInitDirPathsName = "AbInitDirPaths";
        
        public static string windowsABPrefName = "WindowsABName";
        public static string androidABPrefName = "AndroidABName";
        public static string iosABPrefName = "IosABName";

        [SerializeField] private List<string> m_AbInitDirPaths = new List<string>();
        //序列化对象
        protected SerializedObject m_SerializedObject;

        //序列化属性
        protected SerializedProperty m_AssetLstProperty;
        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnProjectChanged()
        {
            Debug.Log("Build");
            ResourcesBuild.BuildResourcesExportConfig();
            EditorResourcesBuild.BuildResourcesExportConfig();
        }
        [MenuItem("UnitFramework/Window/AssetBundle Builder Window")]
        static void OpenWindow()
        {
            AssetBundleBuilderWindow window = EditorWindow.GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder Window");
            window.minSize = new Vector2(600,400);
            window.Show();
           
            
        }
  

        private void OnEnable()
        {
            if (PlayerPrefs.HasKey(windowsABPrefName))
            {
                windosABName = PlayerPrefs.GetString(windowsABPrefName);
            }
            else
            {
                PlayerPrefs.SetString(windowsABPrefName,windosABName);
            }
            
            if (PlayerPrefs.HasKey(androidABPrefName)) 
            {
                androidABName = PlayerPrefs.GetString(androidABPrefName);
            }
            else
            {
                PlayerPrefs.SetString(androidABPrefName,androidABName);
            }
            
            if (PlayerPrefs.HasKey(iosABPrefName))
            {
                iosABName = PlayerPrefs.GetString(iosABPrefName);
            }
            else
            {
                PlayerPrefs.SetString(iosABPrefName,iosABName);
            }
            
            if (PlayerPrefs.HasKey(abInitDirPathsName))
            {
                string content = PlayerPrefs.GetString(abInitDirPathsName);
                if (string.IsNullOrEmpty(content))
                {
                    m_AbInitDirPaths.Clear();
                }
                else
                {
                    m_AbInitDirPaths = content.Split(';').ToList();
                }
               
            }
            else
            {
                m_AbInitDirPaths = AssetBundleBuilder.assetBundleInitDirPath.ToList();
            }
            
          
            //使用当前类初始化
            m_SerializedObject = new SerializedObject(this);
            //获取当前类中可序列话的属性
            m_AssetLstProperty = m_SerializedObject.FindProperty("m_AbInitDirPaths");
        }

        private void OnDisable()
        { 
            Save();
        }

     
        private void Save()
        {
            PlayerPrefs.SetString(windowsABPrefName,windosABName);
            PlayerPrefs.SetString(androidABPrefName,androidABName);
            PlayerPrefs.SetString(iosABPrefName,iosABName);
            PlayerPrefs.SetString(abInitDirPathsName,string.Join(";",m_AbInitDirPaths));
            AssetBundleBuilder.assetBundleInitDirPath = m_AbInitDirPaths.ToArray();
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            //更新
            m_SerializedObject.Update();

            
            GUILayout.Label("打包路径设置", EditorStyles.boldLabel);
            //GUILayout.BeginHorizontal();
            windosABName = EditorGUILayout.TextField ("Windows 平台主包包名", windosABName);
            androidABName = EditorGUILayout.TextField ("Android 平台主包包名", androidABName);
            iosABName = EditorGUILayout.TextField ("Ios 平台主包包名", iosABName);
            //开始检查是否有修改
            EditorGUI.BeginChangeCheck();

            //显示属性
            //第二个参数必须为true，否则无法显示子节点即List内容
            EditorGUILayout.PropertyField(m_AssetLstProperty,new GUIContent("AB包打包环境初始结构"), true);

            //结束检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {//提交修改
                m_SerializedObject.ApplyModifiedProperties();
            }

            //windosABName = GUILayout.TextField("Windows 包名", windosABName);
            //GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            // 绘制按钮区域
            if (GUILayout.Button("Windows平台打包"))
            {
                string path = $"{Utility.Path.GetPlatformStreamingPath()}/{windosABName}";
                AssetBundleBuilder.BuildAssetBundleForWindows(path);
            }
            
            if (GUILayout.Button("Android平台打包"))
            {
                string path = $"{Utility.Path.GetPlatformStreamingPath()}/{androidABName}";
                AssetBundleBuilder.BuildAssetBundleForAndroid(path);
            }
          
            if (GUILayout.Button("Ios平台打包"))
            {
                string path = $"{Utility.Path.GetPlatformStreamingPath()}/{iosABName}";
                AssetBundleBuilder.BuildAssetBundleForIos(path);
            } 
            
            GUILayout.EndHorizontal();
            if (GUILayout.Button("初始化AB包结构"))
            {
                AssetBundleBuilder.CreateABResourceDirectory(m_AbInitDirPaths.ToArray());
            }
            Color color = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("删除已打包资源"))
            {
                DeleteAssetBundle.DelAssetBundle();
            }
            GUI.backgroundColor = color;
            
            GUILayout.Label("打包标签设置", EditorStyles.boldLabel);
            //GUILayout.BeginHorizontal();
            if (GUILayout.Button("创建AB包标签"))
            {
                AutoSetAssetBundleLabel.AutoSetAbLabel();
            }
            if (GUILayout.Button("移除AB包标签"))
            {
                AutoRemoveAssetBundleLabel.RemoveABLabel();
            }
            //GUILayout.EndHorizontal();
          
            Save();
        }
    }
}