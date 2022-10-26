using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Editor
{
    public class ResourcesBuild
    {
        [MenuItem("UnitFramework/Resources/BuildResourcesExportConfig")]
        static void BuildResourcesExportConfig()
        {
            string path  = Application.dataPath + "/Resources/";
            if (!Directory.Exists(path))
            {
                Debug.LogWarningFormat("path{0} not exists " ,path);
                return;
            }
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            string txt = "";
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;

                string name = file.Replace(path, "");
                name = name.Substring(0, name.LastIndexOf("."));
                name = name.Replace("\\", "/");
                txt += name + "\n";
            }

            path = path + "FileList.bytes";
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllText(path, txt);
            AssetDatabase.Refresh();
        }
    }
}