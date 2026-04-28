using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("LuckyWheel", WindowLayer.Popup)]
    public class LuckyWheelView : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Btn","SpinBtn")] private Button spinBtn;
        [UIBinder("Btn", "BuyBtn")] private Button buyBtn;
        [UIBinder("Btn", "VipBtn")] private Button vipBtn;

        [UIBinder("count", "SpinBtn")] private TextMeshProUGUI spintimes;
        [UIBinder("price")] private TextMeshProUGUI price;
        [UIBinder("Cells")] private Transform cells;
        [UIBinder("adicon")] private GameObject adIcon;
        [UIBinder("Refresh")] private GameObject refreshTrans;
        [UIBinder("SpinBtn")] private GameObject spinTrans;
        [UIBinder("timer")] private TextMeshProUGUI timer;

        [UIBinder("Text", "Progress")] private TextMeshProUGUI bigwinprogress;
        [UIBinder("fill", "Mask")] private Image bigwinprogressFill;
        private LuckyWheelInfo info;
        private int current = 0;
        public override void OnCreate()
        {
            base.OnCreate();
            info = StorageManager.Instance.GetStorage<ActivityInfo>().LuckyWheelInfo;
            closeBtn.onClick.AddListener(() =>
            {
                
                AudioModule.Instance.ClickAudio();
                Close();
            });
            spinBtn.onClick.AddListener(SpinBtn);
            DealStorage();
            CreateCell();

            price.text = GameConfigSys.Wheel.Cost.ToString();
            buyBtn.onClick.AddListener(BuyDraw);
            vipBtn.onClick.AddListener(FreeDraw);

            ChangeBtnState();

        }

        public void ChangeBtnState()
        {
            
            bool vip = StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent;
            bool isToday =
                DateTimeOffset.FromUnixTimeSeconds(info.VipLastTime).LocalDateTime.Date
                == DateTime.Now.Date;
            
            vipBtn.transform.parent.gameObject.SetActive(vip && !isToday);
            buyBtn.transform.parent.gameObject.SetActive(!vip || isToday);
        }
        private async void FreeDraw()
        {
            AudioModule.Instance.ClickAudio();
            info.VipLastTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Event.Instance.SendEvent(new SubItem((int)ItemType.Diamond, GameConfigSys.Wheel.Cost));
            SetBtnEnable(false);
            ChangeBtnState();
            await StartMulSpin(GameConfigSys.Wheel.BuyCount); 
            // SetBtnEnable(true);
        }


        public override void Close()
        {
            base.Close();
            LobbySequence.Instance.FinishTask("LuckyWheelGuide");
        }

        private async void BuyDraw()
        {
            AudioModule.Instance.ClickAudio();
            if (GUtility.IsEnoughItem(ItemType.Diamond, GameConfigSys.Wheel.Cost))
            {
                Event.Instance.SendEvent(new SubItem((int)ItemType.Diamond, GameConfigSys.Wheel.Cost));
                SetBtnEnable(false);
                info.RollingTimes += 5;
                UpdateBigwin();
                await StartMulSpin(GameConfigSys.Wheel.BuyCount); 
                // SetBtnEnable(true);
            } 
        }

        private int maxTimes = 0;
        private int openDay = 0;
        private void DealStorage()
        {
            long timestamp = info.LastRollTimestamp;

            DateTime localTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            openDay = localTime.Day;
            bool isToday = localTime.Date == DateTime.Now.Date;
            maxTimes = GameConfigSys.Wheel.MaxCount;
            if (!isToday)
            {
                info.LeftTimes = GameConfigSys.Wheel.MaxCount; 
            }
            spintimes.text = info.LeftTimes + "/" + maxTimes;
            SetAdIcon();
            UpdateBigwin();

        }

        private void UpdateBigwin()
        {
            int show = Math.Min(info.RollingTimes, GameConfigSys.Wheel.BigwinCount);
            bigwinprogress.text = $"{show}/{GameConfigSys.Wheel.BigwinCount}";
            bigwinprogressFill.fillAmount = (float)show / GameConfigSys.Wheel.BigwinCount;
        }

        private void SetAdIcon()
        {
            bool isfree = maxTimes - info.LeftTimes < GameConfigSys.Wheel.FreeCount;
            adIcon.SetActive(!isfree);
            refreshTrans.SetActive(info.LeftTimes == 0);
            spinTrans.SetActive(info.LeftTimes != 0);
            if (info.LeftTimes == 0)
            {
                EnableUpdate();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            DateTime now = DateTime.Now;
            DateTime nextDay = now.Date.AddDays(1); // 明天 00:00:00
            if (openDay != now.Day)
            {
                DealStorage();
                DisableUpdate();
            }
            else
            {
                
                long remainSeconds = (long)(nextDay - now).TotalSeconds;
                if (remainSeconds <= 0)
                {
                    remainSeconds = 0;
                    DealStorage();
                    DisableUpdate();
                }
                timer.text = GUtility.GetRemainTimeText(remainSeconds);
            }
        }

        private  async void SpinBtn()
        {
            //时间
            if (info.LeftTimes == 0)
            {
                return;
            }
            AudioModule.Instance.ClickAudio();
            if (maxTimes - info.LeftTimes >= GameConfigSys.Wheel.MaxCount)
            {
                //需要看广告
                AdMgr.Instance.PlayRV(async () =>
                {
                    
                    await StartSpin();
                },"LuckyWheel"); 
            }
            else
            {
                await StartSpin();
                
            }
             
        }

        private async UniTask StartMulSpin(int count)
        {
            AudioModule.Instance.PlayOneShotSfx("wheel_spin"); 
            int index = 0;
            int moveTimes = 10;
            int m = (int)Math.Ceiling(12.0 / count) ;
            var result = GUtility.GetWeightIndex(GameConfigSys.Wheel.Weight, count);
            while (moveTimes > 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    var cell = cells.Find("cell" + i);
                    if (i % m == index)
                    {
                        cell.Find("select").gameObject.SetActive(true);
                    }
                    else
                    {
                        cell.Find("select").gameObject.SetActive(false);
                    }
                }
                index++;
                index = index % m;
                moveTimes--;
                await UniTaskMgr.Instance.WaitForSecond(0.15f);
            }
            
            AudioModule.Instance.PlayOneShotSfx("wheel_stop"); 
            for (int i = 0; i < 12; i++)
            {
                var cell = cells.Find("cell" + i);
                if (result.Contains(i))
                {
                    cell.Find("select").gameObject.SetActive(true);
                }
                else
                {
                    cell.Find("select").gameObject.SetActive(false);
                } 
            }

            await UniTaskMgr.Instance.WaitForSecond(0.5f);
            DealReward(result);
        }

        private void DealReward(List<int> resultIndex)
        {
            
            int photos = 0;
            for (int i = 0; i < resultIndex.Count; i++)
            {
                var id = GameConfigSys.Wheel.RewardIds[resultIndex[i]];
                var amount = GameConfigSys.Wheel.RewardCount[resultIndex[i]];
                
                if (id == (int)ItemType.Photo)
                {
                    photos++;
                }
                else
                {
                    Event.Instance.SendEvent(new AddItem(id, amount));
                }
            }

            if (photos > 0)
            {
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.LuckyWheel, photos, endAction: DealBigWin));
            }
            else
            {
                DealBigWin();
            }
        }
        
        private void DealBigWin()
        {
            if (info.RollingTimes >= GameConfigSys.Wheel.BigwinCount)
            {
                int photos = 0;
                for (int i = 0; i < GameConfigSys.Wheel.BigwinRewards.Count; i++)
                {
                    var item = GameConfigSys.Wheel.BigwinRewards[i];
                    var count = GameConfigSys.Wheel.BigwinRewardsAmount[i];
                    if (item == (int)ItemType.Photo)
                    {
                        photos += count;
                    }
                    else
                    {
                        Event.Instance.SendEvent(new AddItem(item, count));
                    }
                }

                info.RollingTimes = 0;
                UpdateBigwin();
                if (photos > 0)
                {
                    Event.Instance.SendEvent(new AddPhoto((int)PhotoType.LuckyWheel, photos, endAction: ()=>{SetBtnEnable(true); }));
                }
                else
                {
                    SetBtnEnable(true);
                }
                
            }
            else
            {
                SetBtnEnable(true);
            }
            
        }
        private async UniTask StartSpin( )
        {
            if (info.LeftTimes == 0)
            {
                Event.Instance.SendEvent(new ShowTips("Not Times."));
                return;
            }

            AudioModule.Instance.PlayOneShotSfx("wheel_spin");
            SetBtnEnable(false);
            info.LeftTimes--;
            info.RollingTimes++;
            SetAdIcon();
            UpdateBigwin();
            spintimes.text = info.LeftTimes + "/" + maxTimes;
            info.LastRollTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            int movecount = 0;
            int last = -1  ;
            var result = GameConfigSys.GetLuckyWheelResult();
            int first = current;
            while (movecount <= 18 || current != result)
            {
                int index = (first + movecount) % 12;
                var cell = cells.Find("cell" + index);
                cell.Find("select").gameObject.SetActive(true);
                if (last >= 0)
                {
                    cells.Find("cell" + last +"/select").gameObject.SetActive(false);
                }
                movecount++;
                current = index;
                last = index;
                await UniTaskMgr.Instance.WaitForSecond(0.08f);
            }

            
            AudioModule.Instance.PlayOneShotSfx("wheel_stop");
            var l = new List<int>(){result};
            DealReward(l);
            // int rw = GameConfigSys.Wheel.RewardIds[result];
            // int amount = GameConfigSys.Wheel.RewardCount[result];
            // if (rw == (int)ItemType.Photo)
            // {
            //     Event.Instance.SendEvent(new AddPhoto((int)PhotoType.LuckyWheel, 1));
            // }
            // else
            // {
            //     Event.Instance.SendEvent(new AddItem(rw, amount));
            // }
            // SetBtnEnable(true);
        }

        private void SetBtnEnable(bool isEnable)
        {

            spinBtn.interactable = isEnable;
            buyBtn.interactable = isEnable;
            closeBtn.interactable = isEnable;

        }

        private void CreateCell()
        {
            int index = 0;
            for (int i = 0; i < 12; i++)
            {
                var cell = cells.Find("cell" + i);
                var iconimg = cell.Find("Icon").GetComponent<Image>();
                cell.Find("select").gameObject.SetActive(false);
                var amount = iconimg.transform.Find("amount").GetComponent<TextMeshProUGUI>();
                var photo = cell.Find("media");
                if (GameConfigSys.Wheel.RewardIds[i] == (int)ItemType.Coin||
                    GameConfigSys.Wheel.RewardIds[i] == (int)ItemType.Diamond)
                {
                    var item = GameConfigSys.GetItemConfig(GameConfigSys.Wheel.RewardIds[i]);
                    var icon = GUtility.GetItemIcon((ItemType)GameConfigSys.Wheel.RewardIds[i]);
                    iconimg.sprite = icon;
                    amount.text ="x"+ GameConfigSys.Wheel.RewardCount[i].ToString();
                }
                
                photo.gameObject.SetActive(GameConfigSys.Wheel.RewardIds[i] == (int)ItemType.Photo);
                iconimg.gameObject.SetActive(GameConfigSys.Wheel.RewardIds[i] != (int)ItemType.Photo);
            }
        }
    }
}