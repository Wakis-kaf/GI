
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnitFramework.Utils;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Editor
{


    public class DeleteAssetBundle
    {
        [MenuItem("UnitFramework/AssetBundleTools/Delete All Asset Bundles")]
        public static void DelAssetBundle()
        {
            AssetBundleBuilder.CreateABResourceDirectory();
            // 删除AB包输出目录
            string strNeedDeleteDir = string.Empty;

            strNeedDeleteDir = Utility.Path.GetABOutPath();
            if (string.IsNullOrEmpty(strNeedDeleteDir) == false)
            {
                if (Directory.Exists(strNeedDeleteDir))
                {
                    // 注意 ：这里参数 true 表示可以删除非空目录
                    Directory.Delete(strNeedDeleteDir, true);

                    // 同时删除 meta 文件（去除不必要的警告）
                    File.Delete(strNeedDeleteDir + ".meta");

                    // 刷新目录
                    AssetDatabase.Refresh();
                }
            
            }

        }
    }//class_End
}
