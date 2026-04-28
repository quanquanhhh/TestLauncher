using System;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.Activity
{
    [Window("Sign", WindowLayer.Popup)]
    public class SignView : UIWindow
    {
        [UIBinder("Middle")] private Transform middle;
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Claim")] private Button claimBtn;

        [UIBinder("iconA")] private GameObject iconA;
        [UIBinder("iconB")] private GameObject iconB;
        private SignInfo _signInfo = StorageManager.Instance.GetStorage<ActivityInfo>().SignInfo;
        
        bool isToday = false;
        public override void OnCreate()
        {
            base.OnCreate();
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
            closeBtn.onClick.AddListener(() =>
            {
                AudioModule.Instance.ClickAudio();
                Close();
            });
            claimBtn.onClick.AddListener(ClaimFun);
             
            if (_signInfo.LastSignTime != 0)
            { 
                isToday =
                    DateTimeOffset.FromUnixTimeSeconds(_signInfo.LastSignTime).LocalDateTime.Date
                          == DateTime.Now.Date;
            }
            if (!isToday && _signInfo.ClaimCount == 7)
            {
                _signInfo.ClaimCount = 0;
            }
            RefreshCell();
            
            SubScribeEvent<ChangeUserType>(OnChangeUserType);
        }

        private void OnChangeUserType(ChangeUserType obj)
        {
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
        }
        public override void Close()
        {
            base.Close();
            LobbySequence.Instance.FinishTask("DailyGiftGuide");
        }

        private void ClaimFun()
        {
            AudioModule.Instance.ClickAudio();
            if (isToday)
            {
                Event.Instance.SendEvent(new ShowTips("today has claimed."));
                return;
            }
            var rewardId = GameConfigSys.sign.SignRewardType[_signInfo.ClaimCount];
            var rewardCnt = GameConfigSys.sign.SignRewardNum[_signInfo.ClaimCount];
            if (rewardId == (int)ItemType.Photo)
            {
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.Sign, rewardCnt));
            }
            else
            {
                Event.Instance.SendEvent(new AddItem(rewardId, rewardCnt));
            }

            isToday = true;
            _signInfo.ClaimCount++;
            middle.Find(_signInfo.ClaimCount +"/gift").DOScale( Vector3.zero,0.2f);
            middle.Find(_signInfo.ClaimCount + "/check").gameObject.SetActive(true);
            middle.Find(_signInfo.ClaimCount + "/check").DOScale(Vector3.one, 0.2f);
            
            _signInfo.LastSignTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void RefreshCell()
        {
            int today = isToday ? _signInfo.ClaimCount : (_signInfo.ClaimCount + 1);
            for (int i = 1; i < 8; i++)
            {
                var cell = middle.Find(i.ToString());
                cell.Find("normal/text").GetComponent<TextMeshProUGUI>().text = "Day "+ i.ToString();
                cell.Find("now/text").GetComponent<TextMeshProUGUI>().text = "Day "+ i.ToString();
                cell.Find("normal").gameObject.SetActive(i != today);
                cell.Find("now").gameObject.SetActive(i == today);
                bool cliam = i < today || (isToday && i == today);
                cell.Find("check").gameObject.SetActive(cliam);
                cell.Find("gift").gameObject.SetActive(!cliam);
                
            }
        }
    }
}