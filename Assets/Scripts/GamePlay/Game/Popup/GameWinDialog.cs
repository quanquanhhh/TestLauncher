using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using Spine.Unity;
using TMPro;
using UnityEngine.UI;

namespace GamePlay.Game.Popup
{
    [Window("Congratulations", WindowLayer.Popup)]
    public class GameWinDialog : UIWindow
    {
        [UIBinder("Amount", "Reward")] private TextMeshProUGUI amount;
        [UIBinder("amount", "ExtraClaim")] private TextMeshProUGUI adAmount;
        [UIBinder("ExtraClaim")] private Button adBtn;
        [UIBinder("Claim")] private Button claim;
        [UIBinder("Coin")] private SkeletonGraphic _spine;

        private int itemId;
        private int mul;
        private int adcoinAmount;
        private int coinAmount;
        public override void OnCreate()
        {
            base.OnCreate();
            AudioModule.Instance.PlayOneShotSfx("BoardReward");
            mul = GameConfigSys.baseGame.GameWinMul; 
            CheckReward();
            
            
            adBtn.onClick.AddListener(AdReward);
            claim.onClick.AddListener(ClaimReward);
        }

        private async UniTask CheckReward()
        {
            if (userDatas.Length > 0)
            {
                var rewards = (Dictionary<int,int>)userDatas[0];
                foreach (var reward in rewards)
                {
                    itemId = reward.Key;
                    int cnt = reward.Value;
                    await _spine.ChangeDataAsset("item_icon_" + itemId);
                    _spine.startingLoop = true;
                    coinAmount = cnt;
                }
            }
            else
            {
                itemId = (int)ItemType.Coin;
                coinAmount = GameConfigSys.baseGame.GameWinCoin;
                
            }
            
            adcoinAmount = mul * coinAmount;
            amount.text = "+"+coinAmount.ToString();
            adAmount.text = "+" + (adcoinAmount);
        }

        private void ClaimReward()
        {
             Action a = () => { GameFsm.Instance.ToState<GameStateLobby>();};
             Event.Instance.SendEvent(new AddItem(itemId,coinAmount));
             UIModule.Instance.ShowAsync<GetPhotoFromLevel>(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName, a,"level");
             AudioModule.Instance.ClickAudio();
             Close();
         }

        private void AdReward()
        { 
            AdMgr.Instance.PlayRV(() =>
            {
                Action a = () => { GameFsm.Instance.ToState<GameStateLobby>();};
                Event.Instance.SendEvent(new AddItem(itemId,adcoinAmount)); 
                UIModule.Instance.ShowAsync<GetPhotoFromLevel>(StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName,a,"level");
                AudioModule.Instance.ClickAudio();
                Close();
                
            },"GameWin");
        }
    }
}