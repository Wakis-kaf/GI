using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitFramework.Runtime
{
    public class UIComponent : SingletonComponentUnit<UIComponent>
    {
        public override string ComponentUnitName { get=>"UI Component"; } 

        public ViewManager viewManagerInstance {
            get;
            private set;
        }

        public override void OnUnitAwake()
        {
            base.OnUnitAwake();
            viewManagerInstance =  gameObject.AddComponent<ViewManager>();
            viewManagerInstance.dontDestroyTransform = viewManagerInstance.transform;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            viewManagerInstance.Refresh();  
        }

        protected override void DisposeUnManagedRes()
        {
            base.DisposeUnManagedRes();
            viewManagerInstance = null;
        }

       
    }
 
}
