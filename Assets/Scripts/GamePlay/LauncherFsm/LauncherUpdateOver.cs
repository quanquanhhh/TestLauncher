using Foundation;
using Foundation.FSM;
using UnityEngine;
using YooAsset;

namespace GamePlay.LauncherFsm
{
    public class LauncherUpdateOver : LauncherBase
    {
        private IFsm<LauncherFsm> _fsm;
        
        protected internal override void OnEnter(IFsm<LauncherFsm> fsm)
        {
            stepDeltaProgress += 5; 
            base.OnEnter(fsm);
            Debug.Log(" [LauncherLogic] = LauncherUpdateOver");
            _fsm = fsm;
            var package = YooAssets.GetPackage(GlobalSetting.PackageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += OnCompleted;
        }

        private async void OnCompleted(AsyncOperationBase obj)
        {
            ChangeState<LauncherGame>(_fsm);
        }
    }
}