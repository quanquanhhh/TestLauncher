using UnityEngine;
using YooAsset;

namespace GamePlay
{
    [System.Serializable]
    public class YooAssetRuntimeConfig
    {
        public string PackageName = "Tile";
        public EPlayMode PlayMode = EPlayMode.HostPlayMode; 
        public string GetMainHostServerURL()
        {
            return BuildServerUrl(GlobalSetting.HostUrl);
        }

        public string GetFallbackHostServerURL()
        {
            if (string.IsNullOrEmpty(GlobalSetting.HostUrl))
            {
                return BuildServerUrl(GlobalSetting.HostUrl);
            }
            return BuildServerUrl(GlobalSetting.HostUrl);
        }

        private string BuildServerUrl(string serverRoot)
        {
            return $"{serverRoot}{GlobalSetting.ResourceVersion}/Bundle/"+ GlobalSetting.Group;
        }
    }

    public static class GlobalSetting
    {
        
        public static string ResourceVersion = "v1";
        public static string Group = "A";

        public static string HostUrl = "https://tile.desiregirls.net/";
        public static string PhotoUrl = "https://tile.desiregirls.net/APhoto";
        // public static string MainHostServer = "https://tile.desiregirls.net/Test";
        // public static string FallbackHostServer = "https://tile.desiregirls.net/Test";
        
        private static YooAssetRuntimeConfig _runtimeConfig = new YooAssetRuntimeConfig();

        public static YooAssetRuntimeConfig RuntimeConfig => _runtimeConfig;
        public static string PackageName => _runtimeConfig.PackageName;

        // public static string ResourceVersion
        // {
        //     get => GlobalSetting.ResourceVersion;
        //     set => GlobalSetting.ResourceVersion = value;
        // }

        public static void Configure(YooAssetRuntimeConfig runtimeConfig)
        {
            if (runtimeConfig == null)
            {
                Debug.LogError("[YooAsset] Runtime config is null. Keep previous config.");
                return;
            }
            _runtimeConfig = runtimeConfig;
            Debug.Log($"[YooAsset] Config updated. Package:{_runtimeConfig.PackageName}, PlayMode:{_runtimeConfig.PlayMode}, MainHost:{_runtimeConfig.GetMainHostServerURL()}, FallbackHost:{_runtimeConfig.GetFallbackHostServerURL()}");
        } 
    }

    public static class GameViewComponent
    {
        public static LoadingLuncher _loadingLuncher;
    }
}
