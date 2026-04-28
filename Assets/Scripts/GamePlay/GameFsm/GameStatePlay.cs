using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using Foundation.Storage;
using GamePlay.Component;
using GamePlay.Game;
using GamePlay.Storage;
using GamePlay.UIMain;
using GamePlay.Utility;
using UnityEngine;
using Event = Foundation.Event;

namespace GamePlay
{
    public class GameStatePlay : GameFsmBase
    {
        
        
        protected internal async override void OnEnter(IFsm<GameFsm> fsm)
        {
            base.OnEnter(fsm);
            string name = ""; 
            ItemUtility.Instance.AddVipPropLeftCount();
            Event.Instance.SendEvent(new UIMainIconMoveHide());
            TileManager.Instance.CheckActivityLevel(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName);
            TileManager.Instance.Reset();
            TileManager.Instance.SetTileDataByLevel();
            await CheckPhoto();
            UIModule.Instance.ShowAsync<GameMain>( ); 
        }

        private async UniTask CheckPhoto()
        {
            var photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName;
            bool isvideo =  photo.ToLower().Contains("mp4");
            if (isvideo)
            {
                DownloadPhoto(photo).Forget();
            }
            else
            {
                await DownloadPhoto(photo);
            }
            
        }

        protected internal override async void OnResetState()
        {
            base.OnResetState();
            Event.Instance.SendEvent(new LevelFinished(true));
            ItemUtility.Instance.AddVipPropLeftCount();
            TileManager.Instance.Reset();
            TileManager.Instance.SetTileDataByLevel();
            await CheckPhoto();
            UIModule.Instance.Get<GameMain>().Replay();
        }

        private async UniTask DownloadPhoto(string photo)
        { 
            
            //isvideo 
            photo = GUtility.GetPhotoName(photo);
            UIModule.Instance.ShowAsync<UIWait>();
            
            byte[] bytes = await XResDownloadQueue.TryGetXRes(photo); 
        }

        protected internal override void OnLeave(IFsm<GameFsm> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            UIModule.Instance.Close<GameMain>();
            Event.Instance.SendEvent(new UIMainIconMoveShow());
            bool win = TileManager.Instance.IsWin();
            TileManager.Instance.Reset();
            
            Event.Instance.SendEvent(new LevelFinished(win));
        }
    }
}