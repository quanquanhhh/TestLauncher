using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Foundation
{
    public class ResourceModule : SingletonComponent<ResourceModule>
    {
        private ResourcePackage _package;

        public void Initialize(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("[ResourceModule] Package name is null or empty.");
                return;
            }

            _package = YooAssets.TryGetPackage(packageName);
            if (_package == null)
            {
                Debug.LogError($"[ResourceModule] Package not found : {packageName}");
                return;
            }

            Debug.Log($"[ResourceModule] Initialized with package : {packageName}");
        }

        public AssetHandle LoadAsync<T>(string location) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Debug.LogError("[ResourceModule] LoadAsync location is null or empty.");
                return null;
            }

            var package = GetPackage();
            if (package == null)
            {
                return null;
            }

            return package.LoadAssetAsync<T>(location);
        }

        public AssetHandle LoadSync<T>(string location) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Debug.LogError("[ResourceModule] LoadSync location is null or empty.");
                return null;
            }

            var package = GetPackage();
            if (package == null)
            {
                return null;
            }

            return package.LoadAssetSync<T>(location);
        }

        public async UniTask<T> LoadAssetAsync<T>(string location) where T : Object
        {
            var handle = LoadAsync<T>(location);
            if (handle == null)
            {
                return null;
            }

            await handle.Task;
            return handle.AssetObject as T;
        }

        public async UniTask<GameObject> InstantiateAsync(string location, Transform parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(location);
            if (prefab == null)
            {
                Debug.LogError($"[ResourceModule] InstantiateAsync failed. Prefab not found : {location}");
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }

        public ResourceDownloaderOperation CreateDownloaderAll()
        {
            var package = GetPackage();
            var downloader = package.CreateResourceDownloader(10,3);
            return downloader;
        }
        public ResourceDownloaderOperation CreateDownloader(string tag)
        {
            var package = GetPackage();
            var downloader = package.CreateResourceDownloader(tag, 3, 3);
            return downloader;
        }

        public ResourceDownloaderOperation CreateDownloader(List<string> tags)
        {
            string[] t =  new string[tags.Count];
            t = tags.ToArray();
            var package = GetPackage();
            var downloader = package.CreateResourceDownloader(t, 3, 1);
            return downloader;
            
        }
        public async UniTask<long> GetDownloadSizeByTagAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogError("[ResourceModule] GetDownloadSizeByTagAsync tag is null or empty.");
                return 0;
            }

            var package = GetPackage();
            if (package == null)
            {
                return 0;
            }

            
            var operation = package.CreateResourceDownloader(tag, 3, 3);
            await operation.Task;
            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceModule] GetDownloadSizeByTagAsync failed. Tag:{tag}, Error:{operation.Error}");
                return 0;
            }

            return operation.TotalDownloadBytes;
        }

        public async UniTask<bool> DownloadByTagAsync(string tag, int downloadingMaxNum = 6, int failedTryAgain = 2)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogError("[ResourceModule] DownloadByTagAsync tag is null or empty.");
                return false;
            }

            var package = GetPackage();
            if (package == null)
            {
                return false;
            }

            var downloader = package.CreateResourceDownloader(tag, downloadingMaxNum, failedTryAgain);
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log($"[ResourceModule] DownloadByTagAsync no pending bundles. Tag:{tag}");
                return true;
            }

            Debug.Log($"[ResourceModule] Download start. Tag:{tag}, Count:{downloader.TotalDownloadCount}, Bytes:{downloader.TotalDownloadBytes}");
            downloader.BeginDownload();
            await downloader.Task;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[ResourceModule] Download failed. Tag:{tag}, Error:{downloader.Error}");
                return false;
            }

            Debug.Log($"[ResourceModule] Download finished. Tag:{tag}");
            return true;
        }

        public ResourcePackage GetPackage()
        {
            if (_package != null)
            {
                return _package;
            }

            Debug.LogError("[ResourceModule] Module is not initialized. Call Initialize(packageName) after YooAsset package initialization.");
            return null;
        }
    }
}
