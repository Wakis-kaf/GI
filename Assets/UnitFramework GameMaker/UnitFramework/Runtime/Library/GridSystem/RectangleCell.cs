using UnityEngine;

namespace UnitFramework.Utils.GridSystem
{
    public class RectangleCell : GridCell
    {
        private Vector3[] m_VertexPositions = new Vector3[0];
        
        public RectangleCell(GridMap gridMap, Vector3 pos, IGridGenerator gridGenerator,int row, int column, int deep) : base(gridMap, pos,
            gridGenerator,row,column,deep)
        {
            InitVertexPositions();
        }

        private void InitVertexPositions()
        {
            Vector3 center = position;
            float x = center.x;
            float y = center.y;
            float z = center.z;
            float sizeX = ownerMap.gridSize.x / 2;
            float sizeY = ownerMap.gridSize.y / 2;
            float sizeZ = ownerMap.gridSize.z / 2;
            m_VertexPositions = new Vector3[]
            {
                // new Vector3(x - sizeX, y + sizeY, z + sizeZ),
                // new Vector3(x - sizeX, y - sizeY, z + sizeZ),
                // new Vector3(x + sizeX, y + sizeY, z + sizeZ),
                // new Vector3(x + sizeX, y - sizeY, z + sizeZ),
                // new Vector3(x - sizeX, y + sizeY, z - sizeZ),
                // new Vector3(x - sizeX, y - sizeY, z - sizeZ),
                // new Vector3(x + sizeX, y + sizeY, z - sizeZ),
                // new Vector3(x + sizeX, y - sizeY, z - sizeZ), 
                //
                new Vector3(x - sizeX, y , z + sizeZ),
                new Vector3(x + sizeX, y, z + sizeZ),
               
                new Vector3(x + sizeX, y , z - sizeZ),
                new Vector3(x - sizeX, y, z - sizeZ),
         
            };
        }

        public override Vector3[] GetVertexPositions()
        {
            return m_VertexPositions;
        }
    }
}