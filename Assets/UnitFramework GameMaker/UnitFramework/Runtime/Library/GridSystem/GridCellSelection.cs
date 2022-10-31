using System;
using TMPro;
using UnityEngine;

namespace UnitFramework.Utils.GridSystem
{
    [ExecuteInEditMode]
    public class GridCellSelection : MonoBehaviour
    {
        public string tag;
        private GridCell m_Cell;
        private TextMeshPro m_Tmp;
        private RectTransform m_TmpRT;

        private void Awake()
        {
          
            Transform txtRoot = new  GameObject("Txt").transform;
            txtRoot.SetParent(transform);
            m_Tmp = txtRoot.gameObject.AddComponent<TextMeshPro>();
            m_Tmp.alignment = TextAlignmentOptions.Center;
            
            m_Tmp.color = Color.black;
            m_Tmp.fontSize = 2;
            m_TmpRT = m_Tmp.GetComponent<RectTransform>();
            m_TmpRT.sizeDelta = new Vector2(1,1);
            
        }

        private Vector3 GetSwizzleDir()
        {
            switch (m_Cell.ownerMap.cellSwizzle)
            {
                default:
                case CellSwizzle.Y: return Vector3.down;
                case CellSwizzle.Z: return Vector3.forward;
                case CellSwizzle.X: return Vector3.left;
            }
        }private Vector3 GetRotAngles()
        {
            switch (m_Cell.ownerMap.cellSwizzle)
            {
                default:
                case CellSwizzle.Y: return new Vector3(90,0,0);
                case CellSwizzle.Z: return Vector3.zero;
                case CellSwizzle.X:  return new Vector3(0,-90,0);
            }
        }
        

        public void BindCell(GridCell cell)
        {
            m_Cell = cell;
            gameObject.name = GetName("");
            m_TmpRT.anchoredPosition3D = GetSwizzleDir() * -1; ;
            m_TmpRT.localEulerAngles = GetRotAngles(); 
        }

        private string GetName(string tag = "")
        {
            return $"[{tag}] [{m_Cell?.position}]  [({m_Cell?.rowIndex},{m_Cell?.columnIndex},{m_Cell?.deepIndex})]";
        }

        public void OnInspectorGUI()
        {
            m_Cell?.SetTag(tag);
            gameObject.name = GetName(tag);
            m_Tmp.text = tag;
        }
    }
}