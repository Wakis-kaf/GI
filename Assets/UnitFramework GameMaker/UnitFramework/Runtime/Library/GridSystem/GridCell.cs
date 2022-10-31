using LitJson.Extensions;
using UnitFramework.Runtime;
using UnityEngine;

namespace UnitFramework.Utils.GridSystem
{
    [System.Serializable]
    public class GridCell
    {
        [JsonSerializer, SerializeField] private string m_Tag;
        public string Tag => m_Tag;

        [SerializeField] private GirdCellType m_CellType;
        [SerializeField] private GridDimension m_Dimension;
        [SerializeField] private CellSwizzle m_CellSwizzle;
        [SerializeField] public Vector3 m_Position;
        [SerializeField] private int m_RowIndex;
        [SerializeField] private int m_ColumnIndex;
        [SerializeField] private int m_DeepIndex;
        public int rowIndex => m_RowIndex;
        public int columnIndex => m_ColumnIndex;
        public int deepIndex => m_DeepIndex;
        public GridMap ownerMap { get; private set; }

        public GirdCellType cellType
        {
            get => m_CellType;
        }

        public GridDimension dimension
        {
            get => m_Dimension;
        } 
        public CellSwizzle cellSwizzle
        {
            get => m_CellSwizzle;
        }

        public Vector3 position
        {
            get => m_Position;
        }

        public IGridGenerator owenrGenerator { get; private set; }

        public GridCell()
        {
        }

        public GridCell(GridMap gridMap, Vector3 pos, IGridGenerator gridGenerator, int row, int column, int deep)
        {
            this.m_Position = pos;
            this.ownerMap = gridMap;
            this.owenrGenerator = gridGenerator;
            m_RowIndex = row;
            m_ColumnIndex = column;
            m_DeepIndex = deep;
            m_Dimension = gridGenerator.Owner.girdDimension;
            m_CellType = gridGenerator.Owner.girdCellType;
            m_CellSwizzle = gridGenerator.Owner.cellSwizzle;
        }

        public virtual Vector3[] GetVertexPositions()
        {
            throw new System.NotImplementedException();
        }

        public void SetTag(string tag)
        {
            m_Tag = tag;
        }
    }
}