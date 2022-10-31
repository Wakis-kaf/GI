﻿using System;
 using System.Collections.Generic;
 using LitJson;
 using Sirenix.OdinInspector;
using Sirenix.Serialization;
 using UnitFramework.Collections;
 using UnityEditor;
 using UnityEngine;
using UnitFramework.Utils;
namespace UnitFramework.Utils.GridSystem
{
    public enum GridDimension
    {
        TwoD,
        ThreeD
    }
    public enum GirdCellType
    {
        Rectangle, 
        Hexagon,
        Isometric
    }
    
    public  class Grid : MonoBehaviour
    {
        [SerializeField]
        public GridMap currentPreviewGridMap;

        [SerializeField] 
        private List<SerializeVector3[]> m_GridVertexs;
        private GridGenerator m_GridGenerator;
        public GridDimension girdDimension;
        public GirdCellType girdCellType;
        public CellSwizzle cellSwizzle ;
        public IGridGenerator GridGenerator=>m_GridGenerator;
        
        
        [ShowIf("@this.girdDimension == GridDimension.TwoD && this.girdCellType == GirdCellType.Rectangle")]
        public Rectangle2DGirdGenerator rectangle2DGirdGenerator;
        Grid()
        {
            rectangle2DGirdGenerator = new Rectangle2DGirdGenerator(this);
            ResetGenerator();
        }
        public void ResetGenerator()
        {
            ResetGenerator(girdDimension, girdCellType);
        }

        private void ResetGenerator(GridDimension dimension,GirdCellType cellType )
        {
            if (dimension == GridDimension.TwoD && cellType == GirdCellType.Rectangle)
            {
                m_GridGenerator = rectangle2DGirdGenerator;
            }
           
        }

        // 生成网格
        public  GridMap GeneratorGrid()
        {
            Utility.UnityTransform.DestroyChildsImmediate(transform);
            // 生成地图
            GridMap map = m_GridGenerator?.GenerateGridMap();
            GridMapSelection gridMapSelection = new GridMapSelection(this,map);
            currentPreviewGridMap = map;
            return map;
        }
        
        // public GridMap GridGenerate()
        // {
        //     GridMap map = m_GridGenerator?.GenerateGridMap();
        //     m_GridGenerator?.VisibleGridMap(map);
        //     currentPreviewGridMap = map;
        //     m_GridVertexs = new List<SerializeVector3[]>();
        //     for (int i = 0; i < map.Cells.Length; i++)
        //     {
        //         m_GridVertexs.Add(map.Cells[i].GetVertexPositions().ConvertToSerializeVector3Array() );   
        //     }
        //     return map;
        // }
#if  UNITY_EDITOR
        public void ShowGrid()
        {
            //m_GridGenerator.DrawGridOnScene(currentPreviewGridMap);
        }
#endif
        /// <summary>
        /// 保存网格
        /// </summary>
        public void SaveGrid()
        {
#if UNITY_EDITOR    
            if(currentPreviewGridMap == null) return;
            // 保存为本地JSON文件x`
            
            //Debug.Log(jsonContent);
            // 创建文件
#endif
        }

        public GridMap GetCurrentPreview()
        {
            
            return currentPreviewGridMap;
        }
    }
}