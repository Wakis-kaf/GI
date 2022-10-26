

using System;
using System.Data;
using System.IO;
using Excel;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class ExcelLoader :  SingletonComponentUnit<ExcelLoader>
    {
        public override string ComponentUnitName { get=>"ExcelLoader"; }
        public string excelFileSaveStreamingPath = "/ExcelFiles/";

        // public DataRowCollection ReadExcel(string filePath, string fileName,out  int columnNum, out int rowNum)
        // {
        //     DataRowCollection collect = ReadExcel(filePath,   columnNum,   rowNum);
        // }
        /// <summary>
        /// 读取表格文件
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <returns></returns>
        public static DataSet ReadExcel(string filePath)
        {
            // try
            // {
                if(!File.Exists(filePath))
                    throw  new  Exception($"{filePath} not exist!");
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    Debug.Log(stream == null || stream == Stream.Null);
                    IExcelDataReader excelReader;
                    if(filePath.EndsWith(".xls"))
                        excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    DataSet result = excelReader.AsDataSet();
                    return result;
                }
              
            //}
            // catch (Exception e)
            // {
            //     Debug.LogErrorFormat("Read Excel Error! {0}",e.Message);
            //     return null;
            // }
        }
        /// <summary>
        /// 从Excel中获取数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="row">行数</param>
        /// <param name="column">列数</param>
        /// <param name="data">输出数据</param>
        /// <param name="tableIndex">查找表下标</param>
        /// <returns></returns>
        public static bool GetData(string filePath,int row,int column,out object data,int tableIndex = 0)
        {
            data = default;
            DataSet dataSet = ReadExcel(filePath);
            if (dataSet == null)
            {
                return false;
            }

            var table = dataSet.Tables;
            if (table.Count >= tableIndex)
            {
                Debug.LogErrorFormat("Read excel data table index error! {0} current excel count {1}",tableIndex,table.Count);
                return false;
            }

            data = dataSet.Tables[tableIndex].Rows[row][column];
            return true;
             
        }

        /// <summary>
        /// 从Excel中获取每一行带数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="row"></param>
        /// <param name="datas">输出数据</param>
        /// <param name="tableIndex">表下标</param>
        /// <returns></returns>
        public static bool GetRows(string filePath, int row, out object[] datas, int tableIndex = 0)
        {
            datas = default;
            DataSet dataSet = ReadExcel(filePath);
            if (dataSet == null)
            {
                return false;
            }

            var table = dataSet.Tables;
            if (table.Count >= tableIndex)
            {
                Debug.LogErrorFormat("Read excel data table index error! {0} current excel count {1}",tableIndex,table.Count);
                return false;
            }

            datas = dataSet.Tables[tableIndex].Rows[row].ItemArray;
            return true;
        }
       
        //public static T 
        
    }
    
}