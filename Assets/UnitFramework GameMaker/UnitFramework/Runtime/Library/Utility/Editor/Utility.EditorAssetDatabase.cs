
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Utils
{

    public static partial class EditorUtility
    {
        public static class EditorAssetDatabase
        {
            public static string GetCurrentAssetDirectory()
            {
                Type projectWindowUtilType = typeof(ProjectWindowUtil);
                MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath",
                    BindingFlags.Static | BindingFlags.NonPublic);
                object obj = getActiveFolderPath.Invoke(null, new object[0]);
                string pathToCurrentFolder = obj.ToString();

                return pathToCurrentFolder;
            }

            public static string GetClickedDirFullPath()
            {
                string clickedAssetGuid = Selection.assetGUIDs[0];
                string clickedPath = AssetDatabase.GUIDToAssetPath(clickedAssetGuid);
                //string clickedPathFull  = System.IO.Path.Combine(Directory.GetCurrentDirectory(), clickedPath);
                string clickedPathFull = clickedPath;

                FileAttributes attr = File.GetAttributes(clickedPathFull);
                return attr.HasFlag(FileAttributes.Directory)
                    ? clickedPathFull
                    : System.IO.Path.GetDirectoryName(clickedPathFull);
            }
        }
    }

}

