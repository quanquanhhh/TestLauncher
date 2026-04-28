using Cysharp.Threading.Tasks;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Activity;
using GamePlay.Activity.Pass;
using GamePlay.Component;
using GamePlay.Game;
using GamePlay.Storage;
using GamePlay.UIMain.Shop;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("MainUI",WindowLayer.System)]
    public class MainUI : UIWindow
    {
        [UIBinder("MyBackground")] private RectTransform MyBackground;
        [UIBinder("Btn", "Vip")] private Button vipEntrance;
        [UIBinder("Level")] private TextMeshProUGUI level;

        [UIBinder("FlyItem")] private Transform flyRoot;
        [UIBinder("Coin")] private Transform Coin;
        [UIBinder("Diamond")] private Transform Diamond;
        [UIBinder("Setting")] private GameObject setting;
        
        [UIBinder("Btn","Play")] private Button playBtn;
        [UIBinder("Btn","Shop")] private Button shopBtn;
        [UIBinder("Btn","Select")] private Button DebugBtn;
        [UIBinder("Btn","Collect")] private Button CollectBtn;

        [UIBinder("Vip")] public GameObject vip;
        [UIBinder("PermanentVIP")] public GameObject PermanentVIP;
        [UIBinder("Btn","PermanentVIP")] public Button permanentVipEntrance;


        [UIBinder("LuckyWheel")] public GameObject luckywheel; 
        [UIBinder("RemoveAds")] public GameObject removeAds; 
        [UIBinder("DailyChanllenge")] public GameObject daily;
        [UIBinder("SecretGift")] public GameObject SecretGift;
        [UIBinder("LimitGiftPack")] public GameObject limitgift;
        [UIBinder("ExclusiveBenefits")] public GameObject Pass;
        [UIBinder("BeautyEncounters")] public GameObject Beauty;
        [UIBinder("DailyGift")] public GameObject Sign;

        [UIBinder("Shop")] private Transform shopTrans;
        [UIBinder("Collect")] private Transform collect;

        [UIBinder("Top")] private RectTransform topTrans;
        [UIBinder("LeftIcon")] private RectTransform leftTrans;
        [UIBinder("RightIcon")] private RectTransform rightTrans;
        [UIBinder("Bottom")] private RectTransform bottomTrans;
        [UIBinder("Play")] public RectTransform playTrans;
        [UIBinder("firstA","default")] public GameObject defaultImgA;
        [UIBinder("firstB","default")] public GameObject defaultImgB;

        [UIBinder("Collect")] public static RectTransform CollectTrans;
        private float topY, leftx, rightx, bottomy, playy;
        UguiMediaSource  background;
 
        private BaseInfo baseinfo = StorageManager.Instance.GetStorage<BaseInfo>();
        public override void OnCreate()
        {
            base.OnCreate();
            vipEntrance.onClick.AddListener(OpenBuyVipPop);

            int order = GetSorttingDepth();
            topTrans.anchoredPosition -= new Vector2(0,  ViewUtility.AdjustTopHeight);
            leftTrans.anchoredPosition -= new Vector2(0, ViewUtility.AdjustTopHeight);
            rightTrans.anchoredPosition -= new Vector2(0, ViewUtility.AdjustTopHeight);
            
            
            
            
            AddWidget<CoinWidget>(Coin.gameObject, true,order);
            AddWidget<DiamondWidget>(Diamond.gameObject, true ,order);
            AddWidget<SettingWidget>(setting.gameObject, true ,order);
            AddWidget<LuckyWheelWidget>(luckywheel);
            AddWidget<DailyChallenge>(daily);
            AddWidget<BeautyEncounters>(Beauty);
            AddWidget<PassMainWidget>(Pass);
            AddWidget<SignEntrance>(Sign);

            AddWidget<SecretGiftWidget>(SecretGift);
            
            
            AddWidget<LimitedEntrance>(limitgift); 
             
            var isLiftVip = baseinfo.Buff.IsPermanent;
            AddWidget<Vip>(vip,!isLiftVip);
            AddWidget<Vip>(PermanentVIP,!isLiftVip); 
            var isAds = baseinfo.Buff.RemoveAds;
            AddWidget<RemoveADS>(removeAds,!isAds);
            
            MyBackground.sizeDelta = new Vector2(ViewUtility.UISize.x, ViewUtility.UISize.y);
            background = MyBackground.TryGetOrAddComponent<UguiMediaSource>();
            
            FlyUtilty.FlyRoot = flyRoot;
            playBtn.onClick.AddListener(ToPlay);
            shopBtn.onClick.AddListener(ToShop);
            DebugBtn.onClick.AddListener(TestFunction);
            CollectBtn.onClick.AddListener(OpenAlbums);
            permanentVipEntrance.onClick.AddListener(() => { UIModule.Instance.ShowAsync<PermanentVIP>();});
            var current = baseinfo.Level; 
            
            string name = !string.IsNullOrEmpty(baseinfo.SelectBG) ? baseinfo.SelectBG : baseinfo.CurrentBg;
            ChangeBg(name);

            topY = topTrans.anchoredPosition.y;
            bottomy = bottomTrans.anchoredPosition.y;
            leftx = leftTrans.anchoredPosition.x;
            rightx =  rightTrans.anchoredPosition.x;
            playy = playTrans.anchoredPosition.y; 
            SubScribeEvent<LevelFinished>(OnLevelFinished);
            SubScribeEvent<ChangeBackGround>(OnChangeBackGround);
            SubScribeEvent<UIMainIconMoveHide>(OnUIMainIconMoveHide);
            SubScribeEvent<UIMainIconMoveShow>(OnUIMainIconMoveShow);
            SubScribeEvent<ChangeUserType>(OnChangeUserType);
            SubScribeEvent<LanguageChange>(OnLanguageChange);
            
            Event.Instance.SendEvent(new ChangeUserType());
            
            Event.Instance.SendEvent(new GameUIFinished());
            AudioModule.Instance.PlayBgm("Bgm");
            
            level.text = GUtility.GetLocalizedString("MainUI_Level") + (current + 1);
            
        }

        private void OnLanguageChange(LanguageChange obj)
        {
            
            level.text = GUtility.GetLocalizedString("MainUI_Level") + (baseinfo.Level + 1);
        }

        private void OnChangeUserType(ChangeUserType obj)
        {
            shopTrans.Find("iconA").gameObject.SetActive(UserUtility.UserType == "A");
            shopTrans.Find("iconB").gameObject.SetActive(UserUtility.UserType == "B");
            
            collect.Find("iconA").gameObject.SetActive(UserUtility.UserType == "A");
            collect.Find("iconB").gameObject.SetActive(UserUtility.UserType == "B");
        }


        private void OnUIMainIconMoveShow(UIMainIconMoveShow obj)
        {
            topTrans.DOAnchorPosY(topY, 0.5f);
            bottomTrans.DOAnchorPosY(bottomy, 0.5f);
            leftTrans.DOAnchorPosX(leftx, 0.5f);
            rightTrans.DOAnchorPosX(rightx, 0.5f);
            playTrans.DOAnchorPosY(playy, 0.5f); 
        }

        private void OnUIMainIconMoveHide(UIMainIconMoveHide obj)
        {
            topTrans.DOAnchorPosY(0, 0.5f);
            bottomTrans.DOAnchorPosY(0, 0.5f);
            leftTrans.DOAnchorPosX(0, 0.5f);
            rightTrans.DOAnchorPosX(0, 0.5f);
            playTrans.DOAnchorPosY(0, 0.5f); 

        }  

        private void OnChangeBackGround(ChangeBackGround obj)
        {
            string name = !string.IsNullOrEmpty(baseinfo.SelectBG) ? baseinfo.SelectBG : baseinfo.CurrentBg;
            ChangeBg(name);
        }

        public override void OnHide()
        {
            base.OnHide();
            
            gameObject.SetActive(false);
        }

        public override void OnShow()
        {
            base.OnShow();
            gameObject.SetActive(true);
        }

        private void ToShop()
        {
            AudioModule.Instance.ClickAudio();
            UIModule.Instance.ShowAsync<UIShop>();
        }

        private void OnLevelFinished(LevelFinished obj)
        {
            if (!obj.isWin)
            {
                return;
            }
            
            
            level.text = GUtility.GetLocalizedString("MainUI_Level") + (baseinfo.Level + 1);
            if (!string.IsNullOrEmpty(baseinfo.CurrentLevel.OtherInfo))
            {
                DealLevelInfo(baseinfo.CurrentLevel.OtherInfo);
            }
            Event.Instance.SendEvent(new CheckDownloadByLevel()); //下载
            
            Event.Instance.SendEvent(new UpdateActivityIcon()); //活动icon 
            
            var bg = baseinfo.SelectBG;
            if (!string.IsNullOrEmpty(bg))
            {
                return;
            }
            var photo = baseinfo.CurrentLevel.PhotoName;
            if (!string.IsNullOrEmpty(photo))
            {
                baseinfo.CurrentBg = photo;
                ChangeBg(photo);
            }
            background.gameObject.SetActive(true); 
            
            
        } 

        private void DealLevelInfo(string info)
        {
            if (info.Contains("Daily"))
            {
                var str = info.Replace("Daily", "");
                StorageManager.Instance.GetStorage<DailyInfo>().Daily[str] = true;
            }
        }

        private void ChangeBg(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            { 
                defaultImgA.SetActive(UserUtility.UserType == "A");
                defaultImgB.SetActive(UserUtility.UserType == "B");
                background.gameObject.SetActive(false);
                return;
            }
            defaultImgA.SetActive(false);
            defaultImgB.SetActive(false);
            bool isvideo = photo.ToLower().Contains("mp4");
            photo = GUtility.GetPhotoName(photo);
            MyBackground.sizeDelta = new Vector2(ViewUtility.UISize.x, ViewUtility.UISize.y);
            background.SetSource(photo, isvideo);
            background.gameObject.SetActive(true);
            
        }
 
        private void OpenAlbums()
        {
            AudioModule.Instance.ClickAudio();
            UIModule.Instance.ShowAsync<AlbumsView>();
        }

        private void TestFunction()
        {
            // StorageManager.Instance.GetStorage<BaseInfo>().Level++;
            UIModule.Instance.ShowAsync<PassView>();
            // Event.Instance.SendEvent(new AddItem((int)ItemType.Diamond, 100));
        }

        private void ToPlay()
        { 
            
            AudioModule.Instance.ClickAudio();
            if (baseinfo.CurrentLevel.Level > baseinfo.Level)
            { 
                GameFsm.Instance.ToState<GameStatePlay>(); 
            }
            else
            {
                UIModule.Instance.ShowAsync<ChoosePhoto>();
            }
        }

        private void OpenBuyVipPop()
        {
            UIModule.Instance.ShowAsync<BuyVipPop>();
        }
    }
}