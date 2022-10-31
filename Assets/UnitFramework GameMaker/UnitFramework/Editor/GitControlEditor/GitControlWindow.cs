using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace  UnitFramework.Editor
{
    public class GitControlWindow : EditorWindow
    {
        [MenuItem("UnitFramework/Window/Git Version Control Window")]
        static void OpenGitControlWindow()
        {
            GitControlWindow controlWindow = EditorWindow.GetWindow<GitControlWindow>("Git Version Control ");
            controlWindow.InitWindow();
        }

        private void InitWindow()
        {
        
        }
    }

}
