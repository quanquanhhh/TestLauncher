using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;
using Event = Foundation.Event;

namespace GamePlay
{
    public class DownloadUtility : SingletonComponent<DownloadUtility>
    {
        public Dictionary<PhotoType, List<string>> activityTags = new Dictionary<PhotoType, List<string>>();
        public Dictionary<string, bool> downloadState =  new Dictionary<string, bool>(); 
        public override void OnInit()
        {
            base.OnInit();
            Event.Instance.Subscribe<CheckDownloadByLevel>(OnCheckDownloadByLevel);
        }

        public void OnCheckLauncherGameAssets(List<string> tags) //for level
        {
            foreach (var tag in tags)
            {
                
                downloadState[tag] = true; 
                PreloadAtlasIfExists(tag);
            }
        }

        public void OnCheckVipDownload()
        {
            var tags=   GameConfigSys.GetActivityBundleTag(PhotoType.VIP); 
            DownloadTags(tags, PhotoType.VIP);
        }
        public void OnCheckActivityDownLoad()
        {
            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            foreach (var activityPhoto in GameConfigSys.activityPhotos)
            {
                if (GameConfigSys.activityOpenLevel[activityPhoto.Key] - 2 > level)
                {
                    continue;
                }

                List<string> tags = new List<string>();
                var a=   GameConfigSys.GetActivityBundleTag(activityPhoto.Key);
                foreach (var tag in a)
                {
                    if (tags.Contains(tag))
                    {
                        continue;
                    }
                    tags.Add(tag);
                }
                
                DownloadTags(tags, activityPhoto.Key);
            }
        }
        private void OnCheckDownloadByLevel(CheckDownloadByLevel obj)
        {
            OnCheckActivityDownLoad();
        }

        public async void DownloadTags(List<string> tags, PhotoType photoType)
        {
            if (tags == null || tags.Count == 0)
            {
                return;
            }

            if (!activityTags.ContainsKey(photoType))
            {
                activityTags.Add(photoType, new List<string>());
            }
            foreach (var tag in tags)
            {
                downloadState[tag] = false;
                var downloader = ResourceModule.Instance.CreateDownloader(tag);
                if (downloader.TotalDownloadCount != 0)
                {
                    var download = new DownloadItem();
                    download.BeginDownload(downloader, () =>
                    {
                        downloadState[tag] = true;
                        activityTags[photoType].Add(tag);
                        PreloadAtlasIfExists(tag).Forget();
                    }).Forget(); 
                }
                else
                {
                    downloadState[tag] = true; 
                    activityTags[photoType].Add(tag);
                    await PreloadAtlasIfExists(tag);
                }
            } 
        }
         
       
        public async UniTask PreloadAtlasIfExists(string atlasLocation)
        {
            var package =
                ResourceModule.Instance.GetPackage();
            if (package == null)
                return; 

            if (string.IsNullOrEmpty(atlasLocation))
                return;
            atlasLocation += "_asset";
            // 不存在就什么都不做
            if (!package.CheckLocationValid(atlasLocation))
                return;

            
            var handle = package.LoadAssetAsync<SpriteAtlas>(atlasLocation);
            // yield return handle;
            await handle.Task;
            if (handle.Status == EOperationStatus.Succeed)
            {
                SpriteAtlas atlas = handle.GetAssetObject<SpriteAtlas>();
                if (atlas != null)
                {
                    // _atlasHandles[atlasLocation] = handle;
                    AssetLoad.Instance.PreLoadAtlas(atlasLocation);
                    Debug.Log($"[AtlasPreload] preload success : {atlasLocation}");
                }
                else
                {
                    handle.Release();
                    Debug.LogWarning($"[AtlasPreload] loaded but atlas is null : {atlasLocation}");
                }
            }
            else
            {
                handle.Release();
                Debug.LogWarning($"[AtlasPreload] preload failed : {atlasLocation}, error = {handle.LastError}");
            }
        }
 
    }
}