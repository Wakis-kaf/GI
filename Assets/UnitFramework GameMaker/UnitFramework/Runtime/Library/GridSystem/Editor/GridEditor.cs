using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using UnitFramework.Collections;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Utils.GridSystem.Editor
{
    [CustomEditor(typeof(Grid))]
    public class GridEditor : OdinEditor
    {
        private Grid m_OwenrGrid;
        private GridMap m_GridMap;
        private SerializedProperty m_CurrentPreviewGridMapSP;
        private SerializedProperty m_GridVetexsSP;
        private List<Vector3[]> m_GridCellVertex  = new List<Vector3[]>(); 
        protected override void OnEnable()
        {
            base.OnEnable();

            // 获取当前的序列化对象（target：当前检视面板中显示的对象）
            m_OwenrGrid = target as Grid;
            m_CurrentPreviewGridMapSP = serializedObject.FindProperty("currentPreviewGridMap");
            m_GridVetexsSP = serializedObject.FindProperty("m_GridVertexs");
            OnGridRefresh();
        }
        
        private void OnSceneGUI()
        {
            //ShowGrid();
        }
       

        private void OnGridRefresh()
        {
            ShowGrid();
        }
        private void ShowGrid()
        {
            m_OwenrGrid.ShowGrid();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();
            base.DrawDefaultInspector();
           

            if (GUILayout.Button("生成网格"))
            {
                m_OwenrGrid.GeneratorGrid();
                OnGridRefresh();
            }
            if (GUILayout.Button("保存地图"))
            {
                m_OwenrGrid.SaveGrid();
                SaveGridMap();
                OnGridRefresh();
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_OwenrGrid.ResetGenerator();
                OnGridRefresh();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SaveGridMap()
        {
            string currentPath = EditorUtility.EditorAssetDatabase.GetCurrentAssetDirectory();
            string fileName = EditorUtility.EditorAssetDatabase.GetNotRepeateAssetName(currentPath, "New GridMap Data");
            string fullPath = Path.Combine(currentPath, fileName+".asset");
            Debug.Log($"FullPath{fullPath}");
            GridMap map = m_OwenrGrid.GetCurrentPreview();
            
            if (map == null)
            {
                Debug.LogError("Null error map");
                return;
            }

            AssetDatabase.CreateAsset(map, fullPath);
            AssetDatabase.Refresh();

        }
    }
}