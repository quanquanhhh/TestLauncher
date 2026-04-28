using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.Activity
{
    [Window("LimitedGift", WindowLayer.Popup)]
    public class LimitedGift : UIWindow
    {
        [UIBinder("model")] private Transform model;
        [UIBinder("price")] private TextMeshProUGUI price;
        [UIBinder("Timer")] private TextMeshProUGUI timerText;
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("BuyBtn")] private Button buyBtn;
        private List<string> names = new();
        private List<string> showPhotos = new();
        LimitPackInfo storage = StorageManager.Instance.GetStorage<ActivityInfo>().LimitPackInfo;
        private string buykey;
        private int timer = 0;
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(() =>
            {
                
                AudioModule.Instance.ClickAudio();
                Close();
            });
            buykey = GameConfigSys.LimitGiftBuyKey();
            timer = GameConfigSys.LimitGiftTimer();
            price.text = IAPManager.Instance.GetLocalizedPrice(buykey);
            buyBtn.onClick.AddListener(BuyLimite);
            CreateCell();
            
            EnableUpdate();
            
        }

        private void BuyLimite()
        {
            AudioModule.Instance.ClickAudio();

            string getKey = GameConfigSys.LimitGiftBuyKey();
            GameIAP.Purchase(getKey, () =>
            {
                storage.LeftPhotos = names.Count - showPhotos.Count;
                Event.Instance.SendEvent(new BuyLimitedTimeGift());
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.LimitedGift, showPhotos.Count, showPhotos));
                Close();
                
            },"limitedgift");   
        }

        public override void Close()
        {
            base.Close();
            LobbySequence.Instance.FinishTask("LimitGiftPackGuide");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            long endTime = storage.LastOpenTime + timer;   // 结束时间戳
            long nowTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remainSeconds = endTime - nowTime;
            string str = GUtility.GetRemainTimeText(remainSeconds);
            timerText.text = str;
            if (remainSeconds == 0)
            {
                Close();
                DisableUpdate();
            }
        }

        private async void CreateCell()
        {
            names = GameConfigSys.GetNoShowPhotos(PhotoType.LimitedGift);
            foreach (var name in DownloadUtility.Instance.activityTags[PhotoType.LimitedGift])
            {
                Debug.Log("[Limited] has bundle  "+name);
            }
            
            for (int index = 0; index < 6; index++)
            {
                var obj = GameObject.Instantiate(model, model.parent);
                var img = obj.Find("mask/image").GetComponent<Image>();
                var video = obj.Find("videoTag");
                bool isvideo = names[index].Contains(".mp4");
               video.gameObject.SetActive(isvideo);
                var name = GUtility.GetPhotoName(names[index]);
         
                string atlas =  GameConfigSys.GetPhotoAtlasName(name);
                var sp = AssetLoad.Instance.LoadSprite(name, atlas);
                img.sprite = sp;
                showPhotos.Add(names[index]);
                GUtility.ApplyAspect(sp, img.transform.GetComponent<RectTransform>(), img.GetComponent<RectTransform>());
            }
            model.gameObject.SetActive(false);
        }
    }
}