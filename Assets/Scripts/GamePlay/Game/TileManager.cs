using System;
using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Component;
using GamePlay.Game.Data;
using GamePlay.Game.Popup;
using GamePlay.Storage;
using GamePlay.Utility;
using TripleMatch;
using UnityEditor;
using Event = Foundation.Event;
using Random = UnityEngine.Random;

namespace GamePlay.Game
{
    public class LevelInfo
    {
        public int Difficult;
        public int WinDiamond;
        
    }
    public class TileManager : SingletonScript<TileManager>
    {
        public static int TileWidth { get { return 133; } }
        public static int TileHeight { get { return 187; } }
        public static int TileIconNums { get { return 20; } }
        
        private bool initLayer = false;
        private MatchLevels easyLayer, normalLayer, hardLayer;

        public MatchLevelData TileDatas;
        public GirlLevelData LevelData;
        public LevelInfo _levelInfo;
        public bool gameover = false;
        public bool TouchEnable = true; 
        public bool GameWin = false; 
        
        
        public async void InitData()
        {
            initLayer = true;
            easyLayer = await AssetLoad.Instance.LoadAsset<MatchLevels>("EasyLayers") ;
            normalLayer = await AssetLoad.Instance.LoadAsset<MatchLevels>("NormalLayers") ;
            hardLayer = await AssetLoad.Instance.LoadAsset<MatchLevels>("HardLayers") ;
            Event.Instance.Subscribe<ReplayGame>(OnReplayGame);
            Event.Instance.Subscribe<ReviveGame>(OnReviveGame);
        }

        private void OnReviveGame(ReviveGame obj)
        {
            gameover = false;
            if (obj.isAd)
            {
                UIModule.Instance.Get<GameMain>().OnRemovePropFromAd();
            }
            else
            {
                UIModule.Instance.Get<GameMain>().OnRemoveProp();   
                
            }
        }

        private void OnReplayGame(ReplayGame obj)
        {
            gameover = false;
            LevelData = new GirlLevelData(TileDatas);
            InitTileIcon();//icon
            UIModule.Instance.Get<GameMain>().Replay();
        }

        public bool IsWin()
        {
            return GameWin;
        }

        public void Reset()
        {
            TileDatas = null;
            LevelData = null;
            _levelInfo = null;
            gameover = false;
            TouchEnable = true;
            GameWin = false;
        }
        public void SetTileDataByLevel()
        { 
            int level = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level;
            var levelInfo = GameConfigSys.GetDifficultByLevel(level);
            _levelInfo = new LevelInfo();
            _levelInfo.Difficult = levelInfo.difficult;
            if (levelInfo.rewardType == (int)(ItemType.Diamond))
            {
                _levelInfo.WinDiamond = levelInfo.reward;
            }

            TileDatas = GetMatchLevelData(_levelInfo.Difficult);
            LevelData = new GirlLevelData(TileDatas);
            InitTileIcon();//icon
        }

        private MatchLevelData GetMatchLevelData(int infoDifficult)
        {
            List<int> layers = GameConfigSys.GetDifficultConfig(infoDifficult);
            List<MatchLevelData> layerList = new();
            
            foreach (var layer in layers)
            {
                MatchLevels check = new();
                switch (layer)
                {
                    case 1: check = easyLayer; break;
                    case 2: check = normalLayer; break;
                    case 3: check = hardLayer; break;
                }

                int rnd = Random.Range(0, check.LevelList.Count);
                layerList.Add(check.LevelList[rnd]);
            }

            var target = new MatchLevelData();
            foreach (var layer in layerList)
            {
                foreach (var tile in layer.TileList)
                {
                    var data = new MatchTileData();
                    data.Position = tile.Position;
                    target.TileList.Add(data);
                }
            }
            return target;
        }
        

         public void InitTileIcon()
         {
             int count = LevelData.FirstTileList.Count;
             var allType =  new List<int>();
             int currentIcon = -1;
             for (int cellIndex = 0; cellIndex < count; cellIndex++)
             {
                 if (allType.Count == 0)
                 {
                     for (int i = 1; i < TileIconNums; i++)
                     {
                         allType.Add(i);
                     }
                 }
                 if (currentIcon == -1 || cellIndex % 3 == 0)
                 {
                     int index = Random.Range(0, allType.Count);
                     currentIcon = allType[index];
                 }
                 var item =  LevelData.FirstTileList[cellIndex];
                 item.Icon = currentIcon;
             }
              
         }
                 
        public string GetTileName(int ntype)
        {
            return "ui_mj_" + ntype;
        }

        public bool IsLevelWin()
        {
            return 
            LevelData.FirstTileList.Count == 0 &&
            LevelData.SecondTileList.Count == 0 &&
            LevelData.ThirdTileList.Count == 0;
        }

        public void PushTileToPool(GirlTile item)
        {
            UIModule.Instance.Get<GameMain>().PushTileToPool(item);
        }

        public void SetTouchEnable(bool enable)
        {
            TouchEnable = enable;
        }

        public void LevelWin()
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            var photoitem = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[info.CurrentLevel.PhotoName];
            DealTBA(photoitem,info.CurrentLevel.Level);
            if (photoitem.from == (int)PhotoType.BeautyDraft)
            {
                DealBeautyWin(info.CurrentLevel.PhotoName);
                
            }
            else
            {
                info.Level = info.CurrentLevel.Level;
                photoitem.State = (int)PhotoState.Unlock;
                photoitem.index = info.Level;
                photoitem.time = DateTime.Now.Year+"." + DateTime.Now.Month + "."+ DateTime.Now.Day ;
                GameWin = true;
                Dictionary<int, int> rewardList = GetReward();
                if (rewardList != null && rewardList.Count > 0)
                {
                    UIModule.Instance.ShowAsync<GameWinDialog>(rewardList);
                }
                else
                {
                    Action a = () => { GameFsm.Instance.ToState<GameStateLobby>();}; 
                    UIModule.Instance.ShowAsync<GetPhotoFromLevel>(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName, a,"level");
                }
            }
        }

        private void DealTBA(PhotoItem photoitem, int currentLevelLevel)
        {
            switch (photoitem.from)
            {
                case (int)PhotoType.Level1:
                case (int)PhotoType.Level2:
                case (int)PhotoType.Level3:
                    var dic = new Dictionary<string, object>();
                    dic["level"] = currentLevelLevel;
                    TBAMgr.Instance.SendLogEvent("levelwin", dic);
                    break;
                    
            }
        }

        private Dictionary<int, int> GetReward()
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            var photoitem = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[info.CurrentLevel.PhotoName];
            if (photoitem.from == (int)PhotoType.Level1 ||
                photoitem.from == (int)PhotoType.Level2 ||
                photoitem.from == (int)PhotoType.Level3)
            {
                var coin  = GameConfigSys.baseGame.GameWinCoin;
                return new Dictionary<int, int>() { [(int)ItemType.Coin] = coin };
            }
            else if (photoitem.from == (int)PhotoType.DailyChallenge)
            {
                Dictionary<int, int> res = new();
                foreach (var daily in GameConfigSys.daily)
                {
                    if (daily.itemid != (int)ItemType.Photo)
                    {
                        res.Add(daily.itemid, daily.itemAmount);
                    }
                }

                return res;
            }

            return null;
        }
        private async void DealBeautyWin(string name)
        {
            var key = GameConfigSys.GetBeautyKey(name);
            var beauty = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.BeautyItemPhotos[key];
            beauty.finishedCount++;
            if (beauty.finishedCount == 4)
            {//完成
                Action a = () => { GameFsm.Instance.ToState<GameStateLobby>();};
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.BeautyDraft, 4, beauty.photoNames, a));
                UIModule.Instance.Get<GameMain>().HideBlur();
                foreach (var photoname in beauty.photoNames)
                {
                    GUtility.CheckPhotoState(photoname, (int)PhotoType.BeautyDraft, false, PhotoState.Unlock);
                }
            }
            else
            { 
                Reset(); 
                SetTileDataByLevel();
                string next = beauty.photoNames[beauty.finishedCount];
                string downloadName = GUtility.GetPhotoName(next);
                TileDatas = GetMatchLevelData(_levelInfo.Difficult);
                LevelData = new GirlLevelData(TileDatas);
                // byte[] bytes = await XResDownloadQueue.TryGetXRes(downloadName); 
                Event.Instance.SendEvent(new ReplayGame());
                UIModule.Instance.Get<GameMain>().SetSecondGame(next);
            }
        }

        public void CheckActivityLevel(string name)
        { 
            var photoitem = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[name];
            if (photoitem.from == (int)PhotoType.BeautyDraft)
            {
                var key = GameConfigSys.GetBeautyKey(name);
                var beauty = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.BeautyItemPhotos[key];
                beauty.finishedCount = 0;
                
            }
        }

        public List<string> GetLevelPicTags()
        {
            List<string> res = new();
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            foreach (var item in info.CurrentLevel.LevelShowSelectPic)
            {
                var tag = GUtility.GetPhotoTag(item);
                if (!res.Contains(tag))
                {
                    res.Add(tag);
                }
            } 
            foreach (var item in info.CurrentLevel.LevelRefreshShowPic)
            {
                var tag = GUtility.GetPhotoTag(item);
                if (!res.Contains(tag))
                {
                    res.Add(tag);
                }
            } 
            return res;
            
        }

        public void CheckSelectPic()
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            if (info.CurrentLevel.LevelShowSelectPic.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    var str = GameConfigSys.GetNextLevelPhoto(i);
                    string name = "";
                    if (str != null)
                    {
                        name = str.name;
                        GUtility.CheckPhotoState(str.name, str.sourceFrom);
                    }
                    info.CurrentLevel.LevelShowSelectPic.Add(name);
                }
            }

            if (info.CurrentLevel.LevelRefreshShowPic.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    var str = GameConfigSys.GetNextLevelPhoto(i);
                    string name = "";
                    if (str != null)
                    {
                        name = str.name;
                        GUtility.CheckPhotoState(str.name, str.sourceFrom);
                    }
                    info.CurrentLevel.LevelRefreshShowPic.Add(name);
                }
            }
            
        }

        public void RefreshLevelPic()
        { 
            var deal = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel;
            deal.LevelRefreshShowPic.Clear();
            deal.LevelShowSelectPic.Clear();
            CheckSelectPic();
            var res = GetLevelPicTags();
            DownloadUtility.Instance.DownloadTags(res, PhotoType.Level1);
        }

        public void LevelFail()
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            var photoitem = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[info.CurrentLevel.PhotoName];
            if (photoitem.from == (int)PhotoType.BeautyDraft)
            {
                var key = GameConfigSys.GetBeautyKey(info.CurrentLevel.PhotoName);
                var beauty = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.BeautyItemPhotos[key];
                beauty.finishedCount = 0;
                
            }
        }
    }
}