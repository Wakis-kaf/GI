using UnityEditor;
using UnityEngine;

namespace UnitFramework.Editor
{
    public class LogEditor : EditorWindow
    {
        private static string LogTraceConditionName = "ENABLE_LOG_TRACE";

        [MenuItem("EKafFramework/Log/OpenLogTrace")]
        static void OpenLogTrace()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            string condition = LogTraceConditionName;
            Debug.Log(macro);
            if (!macro.Contains(condition))
            {
                if (!macro.EndsWith(";"))
                {
                    macro += ";";
                }

                macro += condition;
            }

            Debug.Log(macro);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,
                macro); //将信息保存到宏信息里. 参数1:保存到哪个平台  参数2:要保存的内容.
        }

        [MenuItem("EKafFramework/Log/CloseLogTrace")]
        static void CloseLogTrace()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            Debug.Log(macro);
            string condition = LogTraceConditionName;
            if (macro.Contains(condition))
            {
                if (macro.Contains(condition + ";"))
                {
                    macro = macro.Replace(condition + ";", "");
                }
                else
                {
                    macro = macro.Replace(condition, "");
                }
            }

            Debug.Log(macro);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,
                macro); //将信息保存到宏信息里. 参数1:保存到哪个平台  参数2:要保存的内容.
        }
    }
}