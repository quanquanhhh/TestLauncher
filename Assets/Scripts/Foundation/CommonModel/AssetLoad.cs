using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace Foundation
{
    public class AssetLoad : SingletonComponent<AssetLoad>
    {
        private Dictionary<string, SpriteAtlas> _atlasMap = new Dictionary<string, SpriteAtlas>();

        public T LoadAssetSync<T>(string location) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            AssetHandle handle = ResourceModule.Instance.LoadSync<T>(location);
            if (handle == null)
            {
                return null;
            }

            return handle.AssetObject as T;
        }

        public GameObject LoadGameObjectSync(string name, Transform parent = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var obj = LoadAssetSync<GameObject>(name);
            if (obj == null)
            {
                return null;
            }

            return GameObject.Instantiate(obj, parent);
        }

        public async UniTask<GameObject> LoadGameobjectAsync(string assetName, Transform parent = null)
        {
            return await ResourceModule.Instance.InstantiateAsync(assetName, parent);
        }

        public async UniTask<T> LoadAsset<T>(string assetName) where T : Object
        {
            return await ResourceModule.Instance.LoadAssetAsync<T>(assetName);
        }       

        public Sprite LoadSprite(string assetName, string atlasName = "CommonAtlas")
        {
            if (!_atlasMap.ContainsKey(atlasName))
            {
                var atlas = LoadAssetSync<SpriteAtlas>(atlasName);
                if (atlas == null)
                {
                    Debug.LogError($"[AssetLoad] Atlas not found : {atlasName}");
                    return null;
                }

                _atlasMap.Add(atlasName, atlas);
            }

            return _atlasMap[atlasName].GetSprite(assetName);
        }

        public void PreLoadAtlas(string atlasName)
        {
            if (_atlasMap.ContainsKey(atlasName))
            {
                return;
            }
            
            var atlas = LoadAssetSync<SpriteAtlas>(atlasName);
            if (atlas == null)
            {
                Debug.LogError($"[AssetLoad] Atlas not found : {atlasName}");
                return  ;
            }
            _atlasMap.Add(atlasName, atlas);
        }

        public UniTask<long> GetDownloadSizeByTagAsync(string tag)
        {
            return ResourceModule.Instance.GetDownloadSizeByTagAsync(tag);
        }

        public UniTask<bool> DownloadByTagAsync(string tag, int downloadingMaxNum = 6, int failedTryAgain = 2)
        {
            return ResourceModule.Instance.DownloadByTagAsync(tag, downloadingMaxNum, failedTryAgain);
        }
    }
}
