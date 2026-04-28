using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Pool;
using Foundation.Storage;
using GameConfig;
using GamePlay.Component;
using GamePlay.Game.Data;
using GamePlay.Storage;
using GamePlay.UIMain;
using GamePlay.UIMain.Shop;
using TMPro;
using TripleMatch;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.Game
{ 
    [Window("PanelGame", WindowLayer.Game)]
    public class GameMain : UIWindow
    {
        [UIBinder("FirstArea")] private GameObject first;
        [UIBinder("RemoveArea")] private Transform removeArea;
        [UIBinder("ThirdArea")] private GameObject third;
        [UIBinder("Setting")] private Button settingBtn;
        [UIBinder("BtnRemove")] private Button BtnRemove;
        [UIBinder("BtnUndo")] private Button BtnUndo;
        [UIBinder("BtnRefresh")] private Button BtnRefresh;
        [UIBinder("Home")] private Button home;
        [UIBinder("Bottom")] private GameObject bottom;
        [UIBinder("TextLevel")] private TextMeshProUGUI TextLevel; 

        [UIBinder("photo_pic")] private Transform levelPic;

        [UIBinder("DebugWin")] private Button debugWin;
        [UIBinder("blur")] private GameObject blur;
        [UIBinder("VideoTag")] private GameObject VideoTag;
        [UIBinder("Top")] private RectTransform topTrans;
        [UIBinder("BaseNode")] private RectTransform BaseNodeTrans;
        [UIBinder("Content")] private RectTransform _content;
        public FirstArea FirstArea;
        // public SecondArea SecondArea;
        public ThirdArea thirdArea;
         
        private Transform goldImg;
        private Transform DiamondImg;
        private List<GirlTile> AllTiles = new List<GirlTile>();

        private PropWidget removeprop;
        private PropWidget undoprop;
        private PropWidget refreshprop;
        private string currentPhotoName;
        private CanvasGroup _canvasGroup;
        public override async void OnCreate()
        {
            base.OnCreate();

            if (!StorageManager.Instance.GetStorage<BaseInfo>().FinishedGuide.Contains("Level"))
            {
                StorageManager.Instance.GetStorage<BaseInfo>().FinishedGuide.Add("Level");
                UIModule.Instance.ShowAsync<GameGuide>();
            }
            if (ViewUtility.UISize.x - ViewUtility.AdjustTopHeight < 1030)
            {
                _content.localScale = Vector3.one * ((ViewUtility.UISize.x - ViewUtility.AdjustTopHeight) / 1030);
            }

            var contenttop = (_content.rect.height - 1920 + ViewUtility.AdjustTopHeight) / 2;
            _content.offsetMax -= new Vector2(0,contenttop);
            
            topTrans.anchoredPosition -= new Vector2(0, ViewUtility.AdjustTopHeight);
            // BaseNodeTrans.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            _canvasGroup = gameObject.TryGetOrAddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            if (userDatas.Length > 0)
            {
                currentPhotoName = (string)userDatas[0];
            }
            else
            {
                currentPhotoName = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName;
            } 
            
            FirstArea = AddWidget<FirstArea>(first); 
            thirdArea = AddWidget<ThirdArea>(third, true, this);
            
            home.onClick.AddListener(ToHome); 
            BtnRemove.onClick.AddListener(OnRemoveProp);
            BtnUndo.onClick.AddListener(OnUndoProp);
            BtnRefresh.onClick.AddListener(OnRefreshProp); 
            settingBtn.onClick.AddListener(OpenSetting);
            
            SubScribeEvent<UpdateThirdAreaEvent>(OnUpdateThirdAreaEvent);
            SubScribeEvent<ClickTileEvent>(OnClickTileEvent);
            SubScribeEvent<UpdateToolText>(OnUpdateToolText); 
            
            UpdateProps();  
            blur.gameObject.SetActive(false);
            VideoTag.gameObject.SetActive(false);
            await SetSecondGame(currentPhotoName); 
            CreateGame();

            
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            debugWin.gameObject.SetActive(true);
            debugWin.onClick.AddListener(LevelWin);
#else
            debugWin.gameObject.SetActive(false);

#endif

            if (!StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds)
            {
                AdMgr.Instance.ShowBanner();
            }
            UIModule.Instance.Get<UIWait>().BreakLoop();
        }

        public override void Close()
        {
            if (!StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds)
            {
                AdMgr.Instance.HideBanner();
            }
            base.Close();
        }

        public void ShowMain()
        {
            
            _canvasGroup.DOFade(1f, 0.5f);
        }

        private void LevelWin()
        {
            TileManager.Instance.LevelWin();
        }

        public async UniTask SetSecondGame(string photoName)
        {
            var media = levelPic.TryGetOrAddComponent<UguiMediaSource>();
            media.Init();
            currentPhotoName = photoName;
            bool isvideo =  currentPhotoName.ToLower().Contains("mp4");
            if (isvideo)
            {
                blur.gameObject.SetActive(true);
                VideoTag.gameObject.SetActive(true);
                
            }
            currentPhotoName = GUtility.GetPhotoName(currentPhotoName);
            
            CheckFromBeauty(); 
            await media.SetSource( currentPhotoName, isvideo, true);
        }

        private void CheckFromBeauty()
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>(); 
            
            var photoitem = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[info.CurrentLevel.PhotoName];
            if (photoitem.from == (int)PhotoType.BeautyDraft)
            {
                var key = GameConfigSys.GetBeautyKey(info.CurrentLevel.PhotoName);
                var config = GameConfigSys.GetBeautyInfo();
                var name = config.Find(x => x.id.ToString() == key);
                var beauty = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.BeautyItemPhotos[key];
                TextLevel.text = name.name + " " + (beauty.finishedCount + 1) +"/" +4;
            }
            else
            {
                TextLevel.text = "Level "+  StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level.ToString();
            }
        }

        private void ToHome()
        {
            AudioModule.Instance.ClickAudio();
            GameFsm.Instance.ToState<GameStateLobby>();
        }

        private void OpenSetting()
        {

            AudioModule.Instance.ClickAudio();
            UIModule.Instance.ShowAsync<UISetting>("game");
        }
 
        public async void Replay()
        {
            FirstArea.RecycleAllTiles();
            thirdArea.RecycleAllTiles();
            
            FirstArea.Replay(AllTiles);
            AllTiles.Clear();
            currentPhotoName = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName;
            await SetSecondGame(currentPhotoName); 
            UIModule.Instance.Get<UIWait>()?.BreakLoop();
        }
 
        private void OnUpdateToolText(UpdateToolText obj)
        { 
        }

        private void OnClickTileEvent(ClickTileEvent obj)
        {
            GirlTile tile = obj.girlTile as GirlTile;
            bool success = thirdArea.AddTile(tile, true);
            if (success)
            {
                CheckTileState(tile);
            }
        }

        public void CheckTileState(GirlTile tile)
        {
            
            tile.SetBtnEnable(false);
            FirstArea.RemoveTile(tile);
        }

        private void OnUpdateThirdAreaEvent(UpdateThirdAreaEvent obj)
        {
            thirdArea.UpdateTileNodes();
        }

        private void CreateGame()
        {
            FirstArea.InitArea(); 
        }
 

        private void UpdateProps()
        {
            removeprop = AddWidget<PropWidget>(BtnRemove.gameObject, true, ItemType.RemoveProp);
            undoprop = AddWidget<PropWidget>(BtnUndo.gameObject, true, ItemType.UndoProp);
            refreshprop = AddWidget<PropWidget>(BtnRefresh.gameObject, true, ItemType.RandomProp);
        }

        private void OnRefreshProp()
        {
            if ( !TileManager.Instance.TouchEnable) return;
            if (refreshprop.IsEnough())
            {
                bool trigger = FirstArea.RefreshAllTiles();
                if (trigger)
                {
                    refreshprop.SubCount();
                    refreshprop.UpdateCount();
                }
            }
            else
            {
                UIModule.Instance.ShowAsync<UIShop>();
            }
        }

        private void OnUndoProp()
        { 
            if ( !TileManager.Instance.TouchEnable) return;
            if (undoprop.IsEnough())
            {
                bool trigger = thirdArea.FromAreaBack();
                
                if (trigger)
                {
                    undoprop.SubCount();
                    undoprop.UpdateCount();
                } 
            }
            else
            {
                UIModule.Instance.ShowAsync<UIShop>();
            }
        }

        public void OnRemoveProp()
        {
            if ( !TileManager.Instance.TouchEnable) return;
            if (removeprop.IsEnough())
            {
                bool trigger = thirdArea.RemoveOneKindTile();
                if (trigger)
                {
                    removeprop.SubCount();
                    removeprop.UpdateCount();
                }
            }
            else
            {
                UIModule.Instance.ShowAsync<UIShop>();
            }
        }

        public void OnRemovePropFromAd()
        {
            bool trigger = thirdArea.RemoveOneKindTile();
        }
        
        public void PushTileToPool(GirlTile item)
        {
            if (item == null) return; 
            item.Release();
            AllTiles.Add(item);
            item.rectTransform.SetParent(first.transform);
            item.gameObject.SetActive(false);
        }
        public void BackToFirst(GirlTile tile)
        {
            FirstArea.BackToArea(tile);
        }

        public void RemoveIcon(int topIcon, int count, List<GirlTile> thirdTile)
        {
            var firstTile = FirstArea.FindAndRemoveIcon(topIcon, count);
            var remove = new List<GirlTile>();
            remove.AddRange(thirdTile);
            remove.AddRange(firstTile);
            remove.Sort((a, b) => a.rectTransform.position.x.CompareTo(b.rectTransform.position.x));
            thirdArea.RemoveMatch(remove, removeArea);
        }

        public void HideBlur()
        {
            blur.GetComponent<RectTransform>().DOAnchorPosY(1800, 0.5f);
        }
    }
}