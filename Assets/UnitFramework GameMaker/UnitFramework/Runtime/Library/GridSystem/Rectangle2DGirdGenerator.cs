using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
#if UNITY_EDITOR
using UnityEditor;

#endif
namespace UnitFramework.Utils.GridSystem
{
    public enum CellSwizzle
    {
        Z,
        Y,
        X
    }

    [System.Serializable]
    public class Rectangle2DGirdGenerator : GridGenerator
    {
        public Vector2 cellSize = new Vector2(1, 1);
        public Vector2 cellGap = Vector2.zero; // 网格间距
        public Vector2Int gridShape = Vector2Int.one;

        public Rectangle2DGirdGenerator(Grid owner) : base(owner)
        {
        }

        public override GridMap GenerateGridMap()
        {
            // 以当前位置为中心点开始生成
            Vector3 centerPosition = Owner.transform.position;
            // upxis Y
            // var correctGridShape = new Vector3Int(gridShape.x, 0, gridShape.y);
            // var correctCellGap = new Vector3(cellGap.x, 0, cellGap.y);
            // var correctCellSize = new Vector3(cellSize.x, 0, cellSize.y);
            GetSizeAndShapeAndGap(out var correctCellSize, out var correctGridShape, out var correctCellGap);
            GridMap gridMap = GridMap.CreateMap(Owner.cellSwizzle,correctCellSize, correctGridShape, correctCellGap);
            gridMap.SetCells(GenerateGridCell(gridMap, centerPosition));
            return gridMap;
        }

        public override void VisibleGridMap(GridMap gridMap)
        {
            //可视化网格图
        }

#if UNITY_EDITOR
        public override void DrawGridOnScene(GridMap gridMap)
        {
            for (int i = 0; i < gridMap.Cells.Length; i++)
            {
                var grid = gridMap.Cells[i];
                Handles.DrawWireCube(grid.position, gridMap.gridSize);
            }
        }
#endif

        private GridCell[] GenerateGridCell(GridMap ownerMap, Vector3 centerPosition)
        {
            var positions = CalculatePositions(centerPosition,out Vector3Int[] indexs);
            List<GridCell> cells = new List<GridCell>();
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                cells.Add(GenerateCell(ownerMap, pos,indexs[i]));
            }

            return cells.ToArray();
        }


        void GetSizeAndShapeAndGap(out Vector3 correctCellSize, out Vector3Int correctGridShape,
            out Vector3 correctCellGap)
        {
            switch (Owner.cellSwizzle)
            {
                case CellSwizzle.X:
                    correctGridShape = new Vector3Int(0, gridShape.y, gridShape.x);
                    correctCellGap = new Vector3(0, cellGap.y, cellGap.x);
                    correctCellSize = new Vector3(0, cellSize.y, cellSize.x);
                    break;
                case CellSwizzle.Z:
                    correctGridShape = new Vector3Int(gridShape.x, gridShape.y, 0);
                    correctCellGap = new Vector3(cellGap.x, cellGap.y, 0);
                    correctCellSize = new Vector3(cellSize.x, cellSize.y, 0);
                    break;
                default:
                case CellSwizzle.Y:
                    correctGridShape = new Vector3Int(gridShape.x, 0, gridShape.y);
                    correctCellGap = new Vector3(cellGap.x, 0, cellGap.y);
                    correctCellSize = new Vector3(cellSize.x, 0, cellSize.y);
                    break;
            }
        }

        private Vector3[] CalculatePositions(Vector3 centerPosition,out Vector3Int[] indexs)
        {
            int rows = gridShape.y;
            int columns = gridShape.x;
            // Vector3Int correctGridShape = new Vector3Int(gridShape.x,0,gridShape.y);
            // Vector3 correctCellGap = new Vector3(cellGap.x,0,cellGap.y);
            // Vector3 correctCellSize = new Vector3(cellSize.x,0,cellSize.y);
            List<Vector3Int> indexList = new List<Vector3Int>();
            GetSizeAndShapeAndGap(out var correctCellSize, out var correctGridShape, out var correctCellGap);
            Vector3 startPosition =
                CalculateStartPosition(centerPosition, correctGridShape, correctCellGap, correctCellSize);
            Debug.Log(
                $"Start pos {startPosition}  correctGridShape{correctGridShape} correctCellGap{correctCellGap} correctCellSize{correctCellSize}");
            Debug.Log($"rows {rows}  columns{columns} ");
            int deep = 0;
            List<Vector3> positions = new List<Vector3>();
            Vector3 gap = correctCellGap;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    gap = correctCellGap;
                    gap = CorrectFirstCellGap(row, column, deep, gap);
                    // 开始生成Cell
                    Vector3 pos = CalculatePosition(startPosition, row, column, deep, gap, correctCellSize);
                    positions.Add(pos);
                    indexList.Add(new Vector3Int(row,column,deep));
                }
            }
            indexs = indexList.ToArray();
            return positions.ToArray();
        }

        private Vector3 CorrectFirstCellGap(int row, int column, int deep, Vector3 gap)
        {
            switch (Owner.cellSwizzle)
            {
                case CellSwizzle.X:
                    if (row == 0) gap.y = 0;
                    if (column == 0) gap.z = 0;
                    if (deep == 0) gap.x = 0;
                    break;
                case CellSwizzle.Z:
                    if (row == 0) gap.y = 0;
                    if (column == 0) gap.x = 0;
                    if (deep == 0) gap.z = 0;
                    break;
                default:
                case CellSwizzle.Y:
                    if (row == 0) gap.z = 0;
                    if (column == 0) gap.x = 0;
                    if (deep == 0) gap.y = 0;
                    break;
            }

            return gap;
        }

        private Vector3 CalculatePosition(Vector3 startPosition, int row, int column, int deep, Vector3 cellGap,
            Vector3 cellSize)
        {
            Vector3 pos = startPosition;
            int correctColumn = column;
            int correctDeep = deep;
            int correctRow = row;
            // CorrectColumnAndDeepAndRow(row, column, deep, out int correctRow, out int correctColumn,
            //     out int correctDeep);
            switch (Owner.cellSwizzle)
            {
                case CellSwizzle.X:
                    pos.x += correctDeep * cellSize.x + correctDeep * cellGap.x;
                    pos.y += correctRow * cellSize.y + correctRow * cellGap.y;
                    pos.z += correctColumn * cellSize.z + correctColumn * cellGap.z;

                    break;
                case CellSwizzle.Z:
                    pos.x += correctColumn * cellSize.x + correctColumn * cellGap.x;
                    pos.y += correctRow * cellSize.y + correctRow * cellGap.y;
                    pos.z += correctDeep * cellSize.z + correctDeep * cellGap.z;

                    break;
                default:
                case CellSwizzle.Y:
                    pos.x += correctColumn * cellSize.x + correctColumn * cellGap.x;
                    pos.y += correctDeep * cellSize.y + correctDeep * cellGap.y;
                    pos.z += correctRow * cellSize.z + correctRow * cellGap.z;
                    break;
            }


            return pos;
        }

        // private void CorrectColumnAndDeepAndRow(int row, int column, int deep, out int correctRow,
        //     out int correctColumn, out int correctDeep)
        // {
        //     correctRow = row;
        //     correctColumn = column;
        //     correctDeep = deep;
        // }
        private GridCell GenerateCell(GridMap gridMap, Vector3 startPosition, int row, int column, int deep,
            Vector3 cellGap, Vector3 cellSize)
        {
            Vector3 pos = CalculatePosition(startPosition, row, column, deep, cellGap, cellSize);
            return new RectangleCell(gridMap, pos, this,row, column, deep);
        }

        private GridCell GenerateCell(GridMap gridMap, Vector3 cellPosition,Vector3Int index)
        {
            return new RectangleCell(gridMap, cellPosition, this,index.x,index.y,index.z);
        }


        // private void GetSizeAndShapeAndGap(out Vector3 correctCellSize, out Vector3Int correctGridShape,
        //     out Vector3 correctCellGap)
        // {
        //   
        // }

        private Vector3 CalculateStartPosition(Vector3 centerPos, Vector3Int gridShape, Vector3 cellGap,
            Vector3 cellSize)
        {
            int row = gridShape.z;
            int columns = gridShape.x;
            int deep = gridShape.y;

            float cellHeight = cellSize.z;
            float cellWidth = cellSize.x;
            float cellDeep = cellSize.y;

            float rowGap = cellGap.z;
            float columnGap = cellGap.x;
            float deepGap = cellGap.y;

            int halfRowCount = (int) (row / 2);


            float rowValue = -CalculateStart(row, cellHeight, rowGap) + centerPos.z;
            float columnValue = -CalculateStart(columns, cellWidth, columnGap) + centerPos.x;
            float deepValue = -CalculateStart(deep, cellDeep, deepGap) + centerPos.y;

            return CorrectPosition(columnValue, deepValue, rowValue);
        }

        private float CalculateStart(int count, float length, float gap)
        {
            int halfCount = (int) (count / 2);
            float start = 0;
            if (count % 2 != 0)
            {
                start = halfCount * length + halfCount * gap;
            }
            else
            {
                start = (length + gap) / 2 + (halfCount - 1) * (length + gap);
            }

            return start;
        }

        private Vector3 CorrectPosition(float columnValue, float deepValue, float rowValue)
        {
            Debug.Log($"{columnValue} {deepValue} {rowValue}");
            return new Vector3(columnValue, deepValue, rowValue);
        }
    }
}