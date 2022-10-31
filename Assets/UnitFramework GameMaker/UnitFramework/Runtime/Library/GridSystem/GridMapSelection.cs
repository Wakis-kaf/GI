using UnityEngine;

namespace UnitFramework.Utils.GridSystem
{
    public class GridMapSelection
    {
        private Grid m_OwnerGrid;
        private GridMap m_OwnerGridMap;
        private Transform m_Root;
        public GridMapSelection(Grid grid, GridMap map)
        {
            this.m_OwnerGrid = grid;
            this.m_OwnerGridMap = map;
           
            m_Root = new GameObject("Root").transform;
            m_Root.SetParent(grid.transform);
            GenerateEditorMap();
        }

        private void GenerateEditorMap()
        {
            var primitiveType = PrimitiveType.Cube;
            for (int i = 0; i < m_OwnerGridMap.Cells.Length; i++)
            {
                var cell = m_OwnerGridMap.Cells[i];
                // 生成GridCellEditor
                Quaternion rot = GetRot();
                GameObject newCell =GameObject.CreatePrimitive(primitiveType);
                newCell.transform.rotation = rot;
                newCell.transform.position = cell.position;
                newCell.transform.SetParent(m_Root); 
                GridCellSelection gridCellSelection = newCell.AddComponent<GridCellSelection>();
                GameObject.DestroyImmediate(newCell.GetComponent<BoxCollider>()) ;
                Vector3 size = m_OwnerGridMap.gridSize;
                size.x = Mathf.Max(size.x, .001f);
                size.y = Mathf.Max(size.y, .001f);
                size.z = Mathf.Max(size.z, .001f);
                newCell.transform.localScale =size;
                gridCellSelection.BindCell(cell);
            }
        }

        private Quaternion GetRot()
        {
            switch (m_OwnerGrid.cellSwizzle)
            {
                case CellSwizzle.Z:return Quaternion.LookRotation(Vector3.forward);
                case CellSwizzle.X: return Quaternion.LookRotation(Vector3.forward);
                default:
                case CellSwizzle.Y: return Quaternion.LookRotation(Vector3.forward);
                    
            }
        }
    }
}