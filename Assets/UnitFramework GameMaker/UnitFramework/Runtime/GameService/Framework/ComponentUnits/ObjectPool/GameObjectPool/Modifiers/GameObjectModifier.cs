
using  UnityEngine;
namespace UnitFramework.Runtime
{
    public class GameObjectModifier : ObjectModifier
    {
        public override string modifierName { get=>"GameObjectModifier";
            set { }
        }
        public override T OnCreate<T>(T item, IModifierData data) where  T: class
        {
            
            if (data is GameObjectGenerateData adata && !ReferenceEquals(adata.prefab,null))
            {
                // 创建一个新的go
                var go = GameObject.Instantiate(adata.prefab, adata.position, adata.rotation, adata.parent);
                go.name = adata.prefab.name;
                return go as T;
            }
            
            return base.OnCreate(item, data);
        }

        public override T OnDestroy<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                GameObject.Destroy(gameObject);
                return null;
            }
            return base.OnDestroy(item, data);
        }

        public override T OnGet<T>(T item, IModifierData data)
        {
            if (item is GameObject gameObject)
            {
                gameObject.SetActive(true);

                if (data is GameObjectGenerateData adata)
                {
                    gameObject.transform.position = adata.position;
                    gameObject.transform.rotation = adata.rotation;
                    gameObject.transform.parent = adata.parent;
                }
            }
            return base.OnGet(item, data);
        }

        public override T OnPut<T>(T item, IModifierData data)
        {
           
            if (item is GameObject gameObject)
            {
              
                gameObject.SetActive(false);
            }
            return base.OnPut(item, data);
        }
    }

}