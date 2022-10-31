﻿using LitJson.Extensions;
 using UnityEngine;

 namespace UnitFramework.Utils.GridSystem
{
    [System.Serializable]
    public class GridMap : ScriptableObject
    {
        //[JsonIgnore]
        public GridCell[] Cells
        {
            get=>m_Cells;
            private set { }
        }
        [SerializeField]
        public GridCell[] m_Cells = new GridCell[0]{};
        public Vector3 gridSize;
        public Vector3 gridShape;
        public Vector3 gridGap;
        public CellSwizzle cellSwizzle;

        public GridMap()
        {
            
        }
        
        public GridMap(CellSwizzle cellSwizzle, Vector3 size, Vector3Int shape, Vector3 gap)
        {
            Init(cellSwizzle,size, shape, gap);
          
        }

        public void SetCells(GridCell[] cells)
        {
            this.m_Cells = cells;
        }
        public static GridMap CreateMap(CellSwizzle cellSwizzle, Vector3 size, Vector3Int shape, Vector3 gap)
        {
            GridMap gridMap = ScriptableObject.CreateInstance<GridMap>();
            gridMap.Init (cellSwizzle,size,shape,gap);

            return gridMap;
        }

        private void Init(CellSwizzle cellSwizzle,Vector3 size, Vector3Int shape, Vector3 gap)
        {
            this.gridSize = size;
            this.gridShape = shape;
            this.gridGap = gap;
            this.cellSwizzle = cellSwizzle;
        }
    }
}