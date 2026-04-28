using System;
using Foundation;
using Foundation.AudioModule;
using GamePlay.UIMain.Shop;
using UnityEngine.UI;

namespace GamePlay.Game.Popup
{
    [Window("NoEnoughProp", WindowLayer.Popup)]
    public class NoEnoughProp : UIWindow
    {

        [UIBinder("OpenShop")] private Button openshop;
        [UIBinder("AdClaim")] private Button AdClaim;
        Action adaction;
        public override void OnCreate()
        {
            base.OnCreate();
            adaction = (Action)userDatas[0];
            openshop.onClick.AddListener(OpenShopFun);
            AdClaim.onClick.AddListener(ClaimByAd);
        }

        private void ClaimByAd()
        {
            adaction?.Invoke();
            AudioModule.Instance.ClickAudio();
            Close();
        }

        private void OpenShopFun()
        {
            AudioModule.Instance.ClickAudio();
            UIModule.Instance.ShowAsync<UIShop>();
            Close();
        }
    }
}