using Foundation;
using Foundation.FSM;
using UnityEngine;
using YooAsset;

namespace GamePlay.LauncherFsm
{
    public class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
/// <summary>
/// 对应 FsmInitializePackage
/// </summary>
    public class LauncherInitializeAsset : LauncherBase
    {
        protected internal override void OnInit(IFsm<LauncherFsm> fsm)
        {
            stepDeltaProgress = 8f;
            base.OnInit(fsm);
        }

        protected internal override async void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            // BetterStreamingAssets.Initialize();
            YooAssets.Initialize();

            var package = YooAssets.TryGetPackage(GlobalSetting.PackageName) ?? YooAssets.CreatePackage(GlobalSetting.PackageName);
            
            InitializationOperation operation = CreateInitializeOperation(package);
            await operation.Task;
            ResourceModule.Instance.Initialize(GlobalSetting.PackageName); 
            ChangeState<LauncherCheckConfig>(fsm);
        }

        private InitializationOperation CreateInitializeOperation(ResourcePackage package)
        {
            var runtimeConfig = GlobalSetting.RuntimeConfig; 

            if (runtimeConfig.PlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(runtimeConfig.PackageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var editorParams = new EditorSimulateModeParameters();
                editorParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                return package.InitializeAsync(editorParams);
            }
            else if (runtimeConfig.PlayMode == EPlayMode.OfflinePlayMode)
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
                CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices,new SecureAesCtrDecryption())
            };
            return package.InitializeAsync(hostParams);
        }
    }
}
