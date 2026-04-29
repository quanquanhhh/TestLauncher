using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using Foundation.Notification;
using GameConfig;
using GamePlay.Game;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace GamePlay.LauncherFsm
{
    public class LauncherBootstrap : LauncherBase
    {
        private IFsm<LauncherFsm> _fsm;
        private ResourceDownloaderOperation _downloader;
        private readonly List<string> _mustTags = new();
        private bool _useOfflineResource;

        protected internal override void OnInit(IFsm<LauncherFsm> fsm)
        {
            stepDeltaProgress = 8f;
            base.OnInit(fsm);
        }

        protected internal override async void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            _fsm = fsm;

            fsm.Owner.MarkLaunchBegin();
            var sdkTask = InitSdkAsync(); // T1
            var yooTask = InitYooAssetAsync(); // T2

            await sdkTask;
            await LoadConfigAsync(); // T3, must wait T1

            await yooTask;

            if (!_useOfflineResource && !fsm.Owner.IsLoadingTimeout())
            {
                await DownloadMissingBundlesWithDeadline();
            }
            else
            {
                StartWarmupDownloadIfPossible();
            }

            fsm.Owner.accumulateProgress = Mathf.Max(fsm.Owner.accumulateProgress, 85f);
            ChangeState<LauncherGame>(fsm);
        }

        private async UniTask InitSdkAsync()
        {
            ConfigManager.GetUseConfig();
            PushNotification.Instance.CheckRequestAndroidPush();
            GameCommon.InitSDKsByPlatform();
            await UniTask.Yield();
            _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 20f);
        }

        private async UniTask InitYooAssetAsync()
        {
            ResolveResourceVersionFromLocal();

            YooAssets.Initialize();
            var package = YooAssets.TryGetPackage(GlobalSetting.PackageName) ?? YooAssets.CreatePackage(GlobalSetting.PackageName);

            _useOfflineResource = ShouldUseOfflineResource();
            var initOperation = CreateInitializeOperation(package, _useOfflineResource);
            await initOperation.Task;

            ResourceModule.Instance.Initialize(GlobalSetting.PackageName);
            _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 40f);

            if (_useOfflineResource || _fsm.Owner.IsLoadingTimeout())
            {
                Debug.Log("[LauncherBootstrap] Use local/offline YooAsset pipeline.");
                return;
            }

            await UpdateManifestWithFallback(package);
            CollectMustDownloadTags();
            _downloader = CreateDownloader(package);
            _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 55f);
        }

        private bool ShouldUseOfflineResource()
        {
            if (GlobalSetting.RuntimeConfig.PlayMode == EPlayMode.EditorSimulateMode ||
                GlobalSetting.RuntimeConfig.PlayMode == EPlayMode.OfflinePlayMode)
            {
                return true;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return true;
            }

            if (CheckUserOffline())
            {
                return true;
            }

            if (_fsm.Owner.IsLoadingTimeout())
            {
                return true;
            }

            return false;
        }

        private bool CheckUserOffline()
        {
            if (GameViewComponent._loadingLuncher == null)
            {
                return false;
            }

            return !GameViewComponent._loadingLuncher.LoadS3;
        }

        private InitializationOperation CreateInitializeOperation(ResourcePackage package, bool forceOffline)
        {
            var runtimeConfig = GlobalSetting.RuntimeConfig;
            if (runtimeConfig.PlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(runtimeConfig.PackageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var editorParams = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
                return package.InitializeAsync(editorParams);
            }

            if (forceOffline || runtimeConfig.PlayMode == EPlayMode.OfflinePlayMode)
            {
                var offlineParams = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters()
                };
                return package.InitializeAsync(offlineParams);
            }

            var defaultHostServer = runtimeConfig.GetMainHostServerURL();
            var fallbackHostServer = runtimeConfig.GetFallbackHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var hostParams = new HostPlayModeParameters
            {
                BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new SecureAesCtrDecryption()),
                CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, new SecureAesCtrDecryption())
            };
            return package.InitializeAsync(hostParams);
        }

        private async UniTask UpdateManifestWithFallback(ResourcePackage package)
        {
            bool canWebManifest = await ProbeRemoteManifestHost();
            if (!canWebManifest || _fsm.Owner.IsLoadingTimeout())
            {
                Debug.Log("[LauncherBootstrap] Manifest probe failed or timeout, fallback local manifest.");
                await UpdateLocalManifest(package);
                return;
            }

            var clearManifest = package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
            await clearManifest.Task;

            int manifestTimeout = Mathf.Max(1, Mathf.CeilToInt(_fsm.Owner.GetRemainLoadingMs() / 1000f));
            var remoteManifest = package.UpdatePackageManifestAsync(GlobalSetting.ResourceVersion, manifestTimeout);
            await remoteManifest.Task;
            if (remoteManifest.Status == EOperationStatus.Succeed)
            {
                var packageVersion = package.GetPackageVersion();
                if (!string.IsNullOrEmpty(packageVersion))
                {
                    PlayerPrefs.SetString("ResourceVersionKey", packageVersion);
                }
                return;
            }

            Debug.LogWarning("[LauncherBootstrap] Remote manifest failed, use local manifest fallback.");
            await UpdateLocalManifest(package);
        }

        private async UniTask UpdateLocalManifest(ResourcePackage package)
        {
            string localVersion = PlayerPrefs.GetString("ResourceVersionKey", GlobalSetting.ResourceVersion);
            var localManifest = package.UpdatePackageManifestAsync(localVersion, 3);
            await localManifest.Task;
            if (localManifest.Status == EOperationStatus.Failed)
            {
                Debug.LogWarning("[LauncherBootstrap] Local manifest update failed:" + localManifest.Error);
            }
        }

        private void CollectMustDownloadTags()
        {
            _mustTags.Clear();
            var info = Foundation.Storage.StorageManager.Instance.GetStorage<GamePlay.Storage.BaseInfo>();
            TileManager.Instance.CheckSelectPic();

            foreach (var str in info.CurrentLevel.LevelShowSelectPic)
            {
                AddTag(GUtility.GetPhotoTag(str));
            }

            foreach (var str in info.CurrentLevel.LevelRefreshShowPic)
            {
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                AddTag(GUtility.GetPhotoTag(str));
            }

            AddTag("BuildIn");
        }

        private void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || _mustTags.Contains(tag))
            {
                return;
            }
            _mustTags.Add(tag);
        }

        private ResourceDownloaderOperation CreateDownloader(ResourcePackage package)
        {
            if (_mustTags.Count == 0)
            {
                return package.CreateResourceDownloader(3, 1);
            }
            return package.CreateResourceDownloader(_mustTags.ToArray(), 3, 1);
        }

        private async UniTask DownloadMissingBundlesWithDeadline()
        {
            if (_downloader == null || _downloader.TotalDownloadCount == 0)
            {
                _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 75f);
                DownloadUtility.Instance.OnCheckLauncherGameAssets(_mustTags);
                return;
            }

            _downloader.DownloadUpdateCallback = OnDownloadProgress;
            _downloader.DownloadErrorCallback = (error) => Debug.LogError("[LauncherBootstrap] Download error:" + error.ErrorInfo);
            _downloader.BeginDownload();

            int remainMs = _fsm.Owner.GetRemainLoadingMs();
            if (remainMs <= 0)
            {
                StartWarmupDownloadIfPossible();
                return;
            }

            var timeoutTask = UniTask.Delay(remainMs);
            int winner = await UniTask.WhenAny(_downloader.Task.AsUniTask(), timeoutTask);
            if (winner == 0 && _downloader.Status == EOperationStatus.Succeed)
            {
                DownloadUtility.Instance.OnCheckLauncherGameAssets(_mustTags);
                _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 80f);
                return;
            }

            Debug.LogWarning("[LauncherBootstrap] Download deadline reached, continue in background for next launch.");
            StartWarmupDownloadIfPossible();
        }

        private void StartWarmupDownloadIfPossible()
        {
            if (_downloader == null || _downloader.TotalDownloadCount == 0)
            {
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            if (!_downloader.IsDone)
            {
                return;
            }

            _downloader.DownloadErrorCallback = (error) => Debug.LogWarning("[LauncherBootstrap] Warmup download error:" + error.ErrorInfo);
            _downloader.BeginDownload();
        }

        private void OnDownloadProgress(DownloadUpdateData data)
        {
            float baseProgress = 55f;
            float stageProgress = Mathf.Clamp01(data.Progress) * 25f;
            _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, baseProgress + stageProgress);
        }

        private async UniTask LoadConfigAsync()
        {
            bool useWebConfig = !_useOfflineResource && !_fsm.Owner.IsLoadingTimeout() && await CheckWebConfig();

            string configContent = string.Empty;
            if (useWebConfig)
            {
                configContent = await LoadConfigFromWeb();
            }

            if (string.IsNullOrEmpty(configContent))
            {
                var textAsset = Resources.Load<TextAsset>("Config/gameconfigs");
                if (textAsset != null)
                {
                    configContent = textAsset.text;
                }
            }

            if (!string.IsNullOrEmpty(configContent))
            {
                string json = ConfigCrypto.DecryptResourceText(configContent, "running123123");
                ConfigPackage data = JsonConvert.DeserializeObject<ConfigPackage>(json);
                GameConfigSys.InitConfig(data);
                GameConfigSys.PreDealPhoto();
                GameConfigSys.PreLoadActivittyOpenLevel();
                GameConfigSys.PreLoadActivityConfig();
                GameCommon.InitIAP();
            }

            _fsm.Owner.accumulateProgress = Mathf.Max(_fsm.Owner.accumulateProgress, 70f);
        }

        private async UniTask<bool> CheckWebConfig()
        {
            string url = $"{GlobalSetting.HostUrl}{GlobalSetting.ResourceVersion}/Config/gameconfigs.txt";
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Head(url);
                request.timeout = Mathf.Max(1, Mathf.CeilToInt(_fsm.Owner.GetRemainLoadingMs() / 1000f));
                await request.SendWebRequest();
                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
            finally
            {
                request?.Dispose();
            }
        }

        private async UniTask<string> LoadConfigFromWeb()
        {
            string url = $"{GlobalSetting.HostUrl}{GlobalSetting.ResourceVersion}/Config/gameconfigs.txt";
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);
                request.timeout = Mathf.Max(1, Mathf.CeilToInt(_fsm.Owner.GetRemainLoadingMs() / 1000f));
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LauncherBootstrap] LoadConfigFromWeb failed:" + ex.Message);
            }
            finally
            {
                request?.Dispose();
            }

            return string.Empty;
        }

        private async UniTask<bool> ProbeRemoteManifestHost()
        {
            string checkUrl = $"{GlobalSetting.HostUrl}{GlobalSetting.ResourceVersion}/";
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Head(checkUrl);
                request.timeout = Mathf.Max(1, Mathf.CeilToInt(_fsm.Owner.GetRemainLoadingMs() / 1000f));
                await request.SendWebRequest();
                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
            finally
            {
                request?.Dispose();
            }
        }

        private void ResolveResourceVersionFromLocal()
        {
            string localResVersion = PlayerPrefs.GetString("LocalVersion", GlobalSetting.ResourceVersion);
            try
            {
                int lastVersion = int.Parse(localResVersion.Replace("v", ""));
                int packageVersion = int.Parse(GlobalSetting.ResourceVersion.Replace("v", ""));
                if (lastVersion > packageVersion)
                {
                    GlobalSetting.ResourceVersion = localResVersion;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LauncherBootstrap] Parse local version failed:" + e.Message);
            }
        }
    }
}
