using System;
using UnityEditor;

namespace UnitFramework.Utils.GridSystem.Editor
{
    [CustomEditor(typeof(GridCellSelection))]
    public class GridCellSelectionEditor : UnityEditor.Editor
    {
        private GridCellSelection gridCell;
        private void OnEnable()
        {
            this.gridCell = target as GridCellSelection;;
        }

        public override void OnInspectorGUI()
        {

            EditorGUI.BeginChangeCheck();
            base.DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                gridCell.OnInspectorGUI();
            }


        }
    }
}