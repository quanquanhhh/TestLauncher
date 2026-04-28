using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Foundation.Pool
{
    public class PoolManager : SingletonScript<PoolManager>
    {
        public GameObject GameObjPool;
 
        public void Init()
        {
            
            GameObjPool = new GameObject();
            GameObjPool.name = "GameObjPool";
            GameObjPool.transform.parent = RootManager.MgrRoot.transform;
            GameObjPool.gameObject.SetActive(false);
        }
        protected Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>();

        public void BackToPool(string name, GameObject obj)
        {
            if (GameObjPool ==  null)
            {
                Init();
            }
            if (pools.ContainsKey(name))
            {
                pools[name].Add(obj);
            }
            else
            {
                pools.Add(name, new List<GameObject>());
                pools[name].Add(obj);
            }
            obj.transform.SetParent(GameObjPool.transform);
        }

        public GameObject GetPoolObj(string name)
        {
            if (GameObjPool ==  null)
            {
                Init();
            }

            if (pools.ContainsKey(name) && pools[name].Count > 0)
            {
                var obj = pools[name][^1];
                pools[name].RemoveAt(pools[name].Count - 1);
                return obj;
            }
            else
            {
                var obj =  AssetLoad.Instance.LoadGameObjectSync(name);
                return obj;
            }
        }
    }
}