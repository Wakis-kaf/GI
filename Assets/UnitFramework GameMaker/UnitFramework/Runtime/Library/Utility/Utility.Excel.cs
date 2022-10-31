﻿﻿using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace UnitFramework.Utils.Excel
{
    public static partial class Utility
    {
        public static class Excel
        {
            public static void ConvertTo(DataRow dataRow, object obj)
            {
                // 获取 obj 中所有含有 ExcelDataSetColumnData 特性的字段
                FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance
                                                                 | BindingFlags.Public
                                                                 | BindingFlags.Static
                                                                 | BindingFlags.NonPublic).Where(info =>
                {
                    return Attribute.IsDefined(info, typeof(ExcelDataSetColumnData));
                }).ToArray();
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    var fieldInfo = fieldInfos[i];
                    var columnData = fieldInfo.GetCustomAttribute<ExcelDataSetColumnData>();
                    int index = columnData.columnIndex == -1 ? i : columnData.columnIndex;
                    if(index>= dataRow.ItemArray.Length) continue;
                    try
                    {
                        var data = Convert.ChangeType(dataRow[index], fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Convert from {fieldInfo.FieldType} to {dataRow[index].GetType()} error, res {e.Message} !");
                    }
                   
                   
                }
            }

       
        }
   
        
    }
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    public class  ExcelDataSetColumnData : Attribute
    {
        public int columnIndex { get; private set; } = -1;

        public ExcelDataSetColumnData()
        {
                
        }
        public ExcelDataSetColumnData(int columnIndex)
        {
            this.columnIndex = columnIndex;
        }
    }
}