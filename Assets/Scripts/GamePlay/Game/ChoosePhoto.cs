using System;
using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Game
{
    [Window("GameChoosePhoto", WindowLayer.Popup)]
    public class ChoosePhoto : UIWindow
    { 
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Normal")] private Transform  normal;
        [UIBinder("Ad")] private Transform  ad;
        [UIBinder("High")] private Transform  high;

        [UIBinder("Btn", "Normal")] private Button normalBtn;
        [UIBinder("Btn", "Ad")]     private Button  adBtn;
        [UIBinder("Btn", "High")]   private Button  highBtn;
        
        [UIBinder("refresh", "Normal")] private Button normalRefresh;
        [UIBinder("refresh", "Ad")]     private Button  adRefresh;
        [UIBinder("refresh", "High")]   private Button  highRefresh;


        [UIBinder("AdPlay")] private Button adPlay;
        [UIBinder("CoinPlay")] private Button CoinPlay;
        [UIBinder("Play")] private Button Play;
        [UIBinder("Vip")] private Button vipPlay;
        [UIBinder("vipspine")] private SkeletonGraphic vipSpine;
        [UIBinder("text","Vip")] private TextMeshProUGUI vipText;
        
        private string normalName = "";
        private string secondeName = "";
        private string thirdName = "";
        private string currentName;
        private int cost;
        public override void OnCreate()
        {
            base.OnCreate();
            
            var dic = new Dictionary<string, object>();
            dic.Add("pos", "playEntrance");
            TBAMgr.Instance.SendLogEvent("guide", dic);       
            UIModule.Instance.Close<UIGuide>();
            closeBtn.onClick.AddListener(CloseFun);

            SetFirstPic();
            SetSecondPic();
            SetThirdPic();
            
            bool photocount = !string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[0]) ;
            normalRefresh.gameObject.SetActive(photocount);
            normalRefresh.onClick.AddListener(() =>
            {
                normalRefresh.gameObject.SetActive(false);
                RefreshPhoto("first");
            });
            
            
            bool photocount2 = !string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[1]) ; 
            adRefresh.gameObject.SetActive(photocount2);
            adRefresh.onClick.AddListener(() =>
            {
                adRefresh.gameObject.SetActive(false);
                RefreshPhoto("second");
            });
            
            
            bool photocount3 = !string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[2]) ; 
            highRefresh.gameObject.SetActive(photocount3);
            highRefresh.onClick.AddListener(() =>
            {
                highRefresh.gameObject.SetActive(false);
                RefreshPhoto("third");
            });
            
             normalBtn.onClick.AddListener(()=>{Select(normalName);});
             adBtn.onClick.AddListener(()=>{Select(secondeName);});
             highBtn.onClick.AddListener(()=>{Select(thirdName);});
             
             adPlay.onClick.AddListener(OnAdPlayGame);
             Play.onClick.AddListener(OnPlayGame);
             CoinPlay.onClick.AddListener(OnCostPlayGame);
             vipPlay.onClick.AddListener(OnVipPlay);
             Select(normalName);
             ChangeVipSpine();
             SubScribeEvent<VIPStateChange>(OnChangeVip);
        }
        
        private void CloseFun()
        {
            Close();
            if (GameFsm.Instance.InGame())
            {
                GameFsm.Instance.ToState<GameStateLobby>();
            }
        }

        private void OnVipPlay()
        {
            AudioModule.Instance.ClickAudio();
            if (StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent)
            {
                PlayGame(currentName);
            }
            else
            {
                UIModule.Instance.ShowAsync<BuyVipPop>(1);
            }
        }

        private void OnChangeVip(VIPStateChange obj)
        {
            ChangeVipSpine();
        }
        

        public void ChangeVipSpine()
        {
            if (StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent)
            {
                // vipSpine.initialSkinName =
                //     "vip" + (StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent ? 1 : 2);
                // vipSpine.Initialize(true);
                vipText.text = "Play";
            }
            else
            {
                vipText.text = "Get";
            }
        }
        private void OnCostPlayGame()
        {
            AudioModule.Instance.ClickAudio();
            if (GUtility.IsEnoughItem(ItemType.Coin, cost))
            {
                OnPlayGame();
            } 
        }

        private void OnPlayGame()
        {
            AudioModule.Instance.ClickAudio();
            PlayGame(currentName);
        }

        private void OnAdPlayGame()
        {
            AudioModule.Instance.ClickAudio();
            AdMgr.Instance.PlayRV(() =>
            {
                PlayGame(currentName);
            },"levelchoose");
        }


        private void Select(string name)
        {
            currentName = name;
            normal.Find("select").gameObject.SetActive(name == normalName);
            ad.Find("select").gameObject.SetActive(name == secondeName);
            high.Find("select").gameObject.SetActive(name == thirdName);
            
            ShowButton(name);
        }
        private void PlayGame(string pic)
        {
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName = pic;
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level = StorageManager.Instance.GetStorage<BaseInfo>().Level + 1;
            GameFsm.Instance.ToState<GameStatePlay>();

            string text = pic == normalName ? "free" : pic == secondeName ? "rv" : pic == thirdName ? "vip" : "free";
            
            var dic = new Dictionary<string, object>();
            dic["chooseType"] = text;
            dic["choosePhoto"] = pic;
            TBAMgr.Instance.SendLogEvent("stage", dic);
            
            Close();
            
            TileManager.Instance.RefreshLevelPic();
        }

        private void RefreshPhoto(string photoType)
        {
            int index = 0;
            switch (photoType)
            {
                case "first":
                    index = 0;
                    StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[normalName].State = (int)PhotoState.Remove;
                    SetFirstPic(true);
                    break;
                case "second":
                    index = 1;
                    StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[secondeName].State = (int)PhotoState.Remove;
                    SetSecondPic(true);
                    break;
                case "third":
                    index = 2;
                    StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[thirdName].State = (int)PhotoState.Remove;
                    SetThirdPic(true);
                    break;
            }
            AudioModule.Instance.ClickAudio();
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[index] = StorageManager
                .Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[index];
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[index] = "";
        }

        private void ShowButton(string name)
        {
            Play.gameObject.SetActive(name == normalName);
            adPlay.gameObject.SetActive(name != normalName);
            CoinPlay.gameObject.SetActive(name == secondeName);
            vipPlay.gameObject.SetActive(name == thirdName);
            
            if (name == thirdName)
            {
                cost = GameConfigSys.baseGame.LevelChooseHighCost;
                CoinPlay.transform.Find("text").GetComponent<TextMeshProUGUI>().text = cost.ToString();
            }
            else if (name == secondeName)
            {
                cost = GameConfigSys.baseGame.LevelChooseCost;
                CoinPlay.transform.Find("text").GetComponent<TextMeshProUGUI>().text = cost.ToString();
            }
        }

        private void ShowSelectIcon(string name)
        {
            normal.Find("select").gameObject.SetActive(name == "normal");
            ad.Find("select").gameObject.SetActive(name == "ad");
            high.Find("select").gameObject.SetActive(name == "high");
        }

        private async void SetFirstPic(bool isRefresh = false)
        {
            if (string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[0]))
            {
                return;
            }
            var photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[0];
            if (isRefresh)
            {
                photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[0];
            }
            normal.Find("Mask").gameObject.SetActive(string.IsNullOrEmpty(photo));
            if (string.IsNullOrEmpty(photo))
            {
                return;
            }
            normalName = photo;
            var str = GUtility.GetPhotoName(normalName);
            // str = "Free150";
            string tagName = GameConfigSys.GetPhotoAtlasName(str);
           
            var texture =  AssetLoad.Instance.LoadSprite(str,tagName);
            if (normal.Find("msk/image") == null)
            {
                Debug.LogError(" image is null");
            }
            else
            {
                Debug.Log(" image has!!");
            }

            if (texture == null)
            {
                Debug.LogError(" texture is null"+str);
            }
            else
            {
                
                Debug.Log(" texture has!!");
            }
            normal.Find("msk/image").GetComponent<Image>().sprite = texture;
            ApplyAspect(texture, normal.Find("msk/image").GetComponent<RectTransform>());
            AddStoragePhoto(photo, PhotoType.Level1);
            
            
        }
        private async void SetSecondPic(bool isRefresh = false)
        {
            if (string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[1]))
            {
                return;
            }
            var photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[1]; 
            if (isRefresh)
            {
                photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[1];
            }
            ad.Find("Mask").gameObject.SetActive(photo == null);
            if (photo == null)
            {
                return;
            }

            secondeName = photo;
            var str = GUtility.GetPhotoName(secondeName); 
            string tagName = GameConfigSys.GetPhotoAtlasName(str); 
            var texture =  AssetLoad.Instance.LoadSprite(str,tagName);
            ad.Find("msk/image").GetComponent<Image>().sprite = texture; 
            
            ApplyAspect(texture, ad.Find("msk/image").GetComponent<RectTransform>());
            AddStoragePhoto(photo, PhotoType.Level2);
            
            
        }     
        private async void SetThirdPic(bool isRefresh = false)
        { 
            if (string.IsNullOrEmpty(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[2]))
            {
                return;
            }
            var photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelShowSelectPic[2]; 
            if (isRefresh)
            {
                photo = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LevelRefreshShowPic[2];
            }
            high.Find("Mask").gameObject.SetActive(photo == null);

            if (photo == null)
            {
                return;
            }
            thirdName = photo;
            var str = GUtility.GetPhotoName(thirdName);  
            
            
            string tagName = GameConfigSys.GetPhotoAtlasName(str); 
            var texture =  AssetLoad.Instance.LoadSprite(str,tagName);
            high.Find("msk/image").GetComponent<Image>().sprite = texture; 
             
            ApplyAspect(texture, high.Find("msk/image").GetComponent<RectTransform>());
            AddStoragePhoto(photo, PhotoType.Level1, true);
            
            
        }        
        private void ApplyAspect( Sprite texture, RectTransform rt)
        { 
            float width = rt.rect.width;
            float height = rt.rect.height;
            float ratio = (float) width/texture.rect.width;
            float ratio2 = (float) height /texture.rect.height;
            float m = Math.Max(ratio, ratio2); 
            rt.sizeDelta = new Vector2(texture.rect.width * m , texture.rect.height * m);
        }
 

        private void AddStoragePhoto(string name, PhotoType from, bool highLevel = false)
        {
            GUtility.CheckPhotoState(name, (int)from, highLevel); 
        }
    }
}