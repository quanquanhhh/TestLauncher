using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using Foundation.Storage;
using GamePlay.Component;
using GamePlay.Storage;
using GamePlay.UIMain;
using GamePlay.Utility;
using UnityEngine;

namespace GamePlay
{
    public class GameStateLobby : GameFsmBase
    {
        protected internal async override void OnEnter(IFsm<GameFsm> fsm)
        {
            base.OnEnter(fsm);
            // await DownloadPhoto();
            if (UIModule.Instance.Get<MainUI>() == null)
            {
                await UIModule.Instance.ShowAsync<MainUI>();
            }
            else
            {
                await UniTaskMgr.Instance.WaitForSecond(0.5f);//icon 收回0.5s
            }

            if (UIModule.Instance.Get<ProtectScreen>() == null)
            {
                UIModule.Instance.ShowAsync<ProtectScreen>().Forget();
            }

            if (UIModule.Instance.Get<Tips>() == null)
            {
                await UIModule.Instance.ShowAsync<Tips>();
            }

#if UNITY_EDITOR || UNITY_DEBUG || DEVELOPMENT_BUILD 
            
            if (UIModule.Instance.Get<DebugPanel.DebugPanel>() == null)
            {
                await UIModule.Instance.ShowAsync<DebugPanel.DebugPanel>();
            }
#endif
            
            LobbySequence.Instance.OnStartLobbySequence();
        }
        private async UniTask DownloadPhoto()
        {
            var photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentBg;
            if (string.IsNullOrEmpty(photo))
            {
                return;
            }
            photo = GUtility.GetPhotoName(photo);
            byte[] bytes = await XResDownloadQueue.TryGetXRes(photo);
        }

        
    }
}