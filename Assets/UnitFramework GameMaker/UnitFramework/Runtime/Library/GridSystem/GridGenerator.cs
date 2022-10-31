﻿using UnityEngine;

namespace UnitFramework.Utils.GridSystem
{
    public interface IGridGenerator
    {
        public Grid Owner { get;  }
        public  GridMap GenerateGridMap();
        public void VisibleGridMap(GridMap gridMap);
        public abstract void DrawGridOnScene(GridMap gridMap);
    }
    [System.Serializable]
    public abstract class GridGenerator : IGridGenerator
    {
        private Grid m_Owner;
        public Grid Owner { get=>m_Owner; }
      
        public abstract GridMap GenerateGridMap();
        public abstract void VisibleGridMap(GridMap gridMap);
        public abstract void DrawGridOnScene(GridMap gridMap);
        public GridGenerator(Grid owner)
        {
            this.m_Owner = owner;
        }


       
    }
}