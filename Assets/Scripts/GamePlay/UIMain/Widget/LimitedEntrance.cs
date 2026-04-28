using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Activity;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    
    public class LimitedEntrance : UIWidget
    {
        [UIBinder("Btn")] private Button openBtn;
        [UIBinder("iconA")] private GameObject iconA;
        [UIBinder("iconB")] private GameObject iconB;
        
        [UIBinder("timer")] private TextMeshProUGUI _timer;
        
        
        private CancellationTokenSource cts; 
        private bool hasPhotos = true;
        private bool isShow = false;
        private int showTimer = 0;
        private LimitPackInfo storage = StorageManager.Instance.GetStorage<ActivityInfo>().LimitPackInfo;
        public override void OnCreate()
        {
            base.OnCreate();
            CheckDataInfo();
            
            showTimer = GameConfigSys.LimitGiftTimer();
            openBtn.onClick.AddListener(() =>
            {
                UIModule.Instance.ShowAsync<LimitedGift>();
                
                var dic = new Dictionary<string, object>();
                dic.Add("pos", "LimitedGiftEntrance");
                TBAMgr.Instance.SendLogEvent("guide", dic);        
                UIModule.Instance.Close<UIGuide>();
            });
            SubScribeEvent<BuyLimitedTimeGift>(OnBuyLimitedTime);
            
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
            
            SubScribeEvent<ChangeUserType>(OnChangeUserType);
        }

        private void CheckDataInfo()
        {
            var info = GameConfigSys.GetNoShowPhotos(PhotoType.LimitedGift);
            storage.LeftPhotos = info.Count;
            if (info.Count == 0 && UserUtility.UserType == "B")
            {
                var dic = new Dictionary<string, object>();
                dic["from"] = "limitedGift";
                TBAMgr.Instance.SendLogEvent("nopic", dic);
            }
        }

        private void OnChangeUserType(ChangeUserType obj)
        {
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
        }

        public override void ChangeActive()
        {
            base.ChangeActive();
            if (cts != null)
            {
                return;
            }
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.LimitedGift) ||
                DownloadUtility.Instance.activityTags[PhotoType.LimitedGift].Count == 0 ||
                storage.LeftPhotos == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.LimitedGift])
            {
                CheckOpenActivity(); 
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        
        private void OnUpdateActivityIcon(UpdateActivityIcon obj)
        {
            ChangeActive();
        }
        
        private void OnBuyLimitedTime(BuyLimitedTimeGift obj)
        {
            isShow = false; 
            gameObject.SetActive(false);
            DisableUpdate();
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        public void CheckOpenActivity()
        {
            if (!hasPhotos || isShow)
            {
                return;
            }
            var isopen = StorageManager.Instance.GetStorage<BaseInfo>().Level >= GameConfigSys.LimitGiftOpenLevel();
           if (!isopen)
           {
               gameObject.SetActive(false);
               return;
           }
           bool empty = storage.LeftPhotos <= 0; 
           if (empty)
           {
               gameObject.SetActive(false);
               return;
           }

           var ps = GameConfigSys.GetNoShowPhotos(PhotoType.LimitedGift);
           if (ps.Count == 0)
           {
               gameObject.SetActive(false);
               hasPhotos = false;
               return;
           }

           gameObject.SetActive(true);
           CheckTimer();
           isShow = true;
           EnableUpdate();
        }

        private void CheckTimer()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            storage.LastOpenTime = timestamp;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (isShow)
            {
                long endTime = storage.LastOpenTime + showTimer;   // 结束时间戳
                long nowTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long remainSeconds = endTime - nowTime;
                string str = GUtility.GetRemainTimeText(remainSeconds);
                _timer.text = str;
                if (remainSeconds == 0)
                {
                    isShow = false;
                    cts = new CancellationTokenSource();
                    UniTaskMgr.Instance.WaitForSecond(GameConfigSys.LimitGiftInterval(), CheckOpenActivity,cts).Forget();
                    gameObject.SetActive(false);
                    DisableUpdate();
                }
            }
        }
    }
}