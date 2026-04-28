using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using Foundation.Notification;
using Foundation.Storage;
using GameConfig;
using GamePlay.Game;
using GamePlay.Storage;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace GamePlay.LauncherFsm
{
    public class LauncherCheckConfig :  LauncherBase
    {
        private IFsm<LauncherFsm> _fsm;

        protected internal override void OnInit(IFsm<LauncherFsm> fsm)
        {
            base.OnInit(fsm);
            
            stepDeltaProgress = 10f;
        }

        protected internal override async void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            _fsm = fsm;
            PushNotification.Instance.CheckRequestAndroidPush();
            var task1 = CheckGameConfig();
            var task2 = CheckManifest();
            
            await UniTask.WhenAll(task1, task2);
            
            ChangeState<LauncherDownload>(_fsm);
        }

        #region CheckGameConfig

        
        private async UniTask CheckGameConfig()
        { 
            await LoadConfig();
            CheckPreDealConfig();
            CheckDownLoadTag();
            GameCommon.InitIAP();
        }
        private void CheckPreDealConfig()
        {
            GameConfigSys.PreDealPhoto();
            GameConfigSys.PreLoadActivittyOpenLevel();
            GameConfigSys.PreLoadActivityConfig();
        }

        private async void CheckDownLoadTag()
        {

            List<string> tags = new();
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            TileManager.Instance.CheckSelectPic();
            foreach (var str in info.CurrentLevel.LevelShowSelectPic)
            {
                var tag = GUtility.GetPhotoTag(str);
                if (tags.Contains(tag))
                {
                    continue;
                }
                tags.Add(tag);
            } 
            foreach (var str in info.CurrentLevel.LevelRefreshShowPic)
            {
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                var tag = GUtility.GetPhotoTag(str);

                if (tags.Contains(tag))
                {
                    continue;
                }
                tags.Add(tag);
            }
            _fsm.Owner.AddCheckTag("must", tags);
        }

        private async UniTask LoadConfig()
        {
         
            var done =  await LoadGamePlayJsonData();
            if (string.IsNullOrEmpty(done))
            {
                var textAsset = Resources.Load<TextAsset>("Config/gameconfigs"); 
                if (textAsset == null)
                {
                    return;
                }

                done = textAsset.text;
            }
            string json = ConfigCrypto.DecryptResourceText(done,"running123123");
            ConfigPackage data = JsonConvert.DeserializeObject<ConfigPackage>(json);

            GameConfigSys.InitConfig(data); 
        }
        

        private async UniTask<string> LoadGamePlayJsonData()
        {
            if (!GameViewComponent._loadingLuncher.LoadS3)
            {
                return string.Empty;
            }
            string result = "";
            
            string url = $"{GlobalSetting.HostUrl}{GlobalSetting.ResourceVersion}/Config/gameconfigs.txt";
            Debug.Log(url);
            UnityWebRequest request = null;

            try
            {
                request = UnityWebRequest.Get(url);
                request.SetRequestHeader("User-Agent", "Mozilla/5.0");
                request.timeout = (int)(1);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    result = request.downloadHandler.text;
                }
                else
                { 
                    Debug.LogWarning($"Web load failed: {request.result}, code: {request.responseCode}");
                }
            }
            catch (Exception ex)
            {  
                Debug.LogWarning($"Exception: {ex.Message}");
            }

            return result;
        }
        

        #endregion


        #region CheckManifest

        private async UniTask CheckManifest()
        {
            var package = YooAssets.GetPackage(GlobalSetting.PackageName);
            string checkVersion = GlobalSetting.ResourceVersion;
            if (GlobalSetting.RuntimeConfig.PlayMode == EPlayMode.EditorSimulateMode ||
                Application.internetReachability == NetworkReachability.NotReachable)
            {
                var operation = package.RequestPackageVersionAsync();
                await operation.Task;
                checkVersion = operation.PackageVersion;
            }
            else
            {
                var a= package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
                await a.Task;
            }

            var checkManifest = package.UpdatePackageManifestAsync(checkVersion,60);
            await checkManifest.Task;
            if (checkManifest.Status ==  EOperationStatus.Failed)
            { 
                Debug.LogError("UpdateManifest:Error->:" + checkManifest.Error);
            }
            if (checkManifest.Status == EOperationStatus.Succeed)
            {
                var packageVersion = YooAssets.GetPackage(GlobalSetting.PackageName).GetPackageVersion();
                if (!string.IsNullOrEmpty(packageVersion))
                {
                    PlayerPrefs.SetString("ResourceVersionKey", packageVersion);
                    Debug.Log("Store Local Res Version:" + packageVersion);
                }
            }
        }

        #endregion
        
    }
}