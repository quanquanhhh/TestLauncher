using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.AudioModule;
using Foundation.FSM;
using Foundation.Notification;
using Foundation.Storage;
using GameConfig;
using GamePlay.Component;
using GamePlay.Game;
using GamePlay.Storage;
using GamePlay.UIMain;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using XGame.Scripts.IAP;

namespace GamePlay.LauncherFsm
{
    public class LauncherGame : LauncherBase
    {
        protected internal override async void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);   
            InitGameFsm(); 
            fsm.Owner.accumulateProgress  = 80f ;

        }


        private void EnterGameDeal()
        {
            var baseInfo = StorageManager.Instance.GetStorage<BaseInfo>();
            var statis = StorageManager.Instance.GetStorage<StatisticsInfo>();
            
            if (statis.FirstInstall)
            {
                TBAMgr.Instance.SendInstallEvent();
                statis.FirstInstall = false;
            }

            TBAMgr.Instance.SendSessionEvent();
            if (!baseInfo.Setting.ContainsKey("music"))
            {
                baseInfo.Setting.Add("music", true);
            }

            if (!baseInfo.Setting.ContainsKey("audio"))
            {
                baseInfo.Setting.Add("audio", true);
            }
            
            if (!baseInfo.Setting.ContainsKey("shake"))
            {
                baseInfo.Setting.Add("shake", true);
            }
            if (!baseInfo.Setting.ContainsKey("autosize"))
            {
                baseInfo.Setting.Add("autosize", false);
            }

            if (!baseInfo.Currency.ContainsKey((int)ItemType.RemoveProp))
            {
                var count = GameConfigSys.item.Find(x => x.id == (int)ItemType.RemoveProp).initCount;
                baseInfo.Currency[(int)ItemType.RemoveProp] = count;
            }
            if (!baseInfo.Currency.ContainsKey((int)ItemType.UndoProp))
            {
                var count = GameConfigSys.item.Find(x => x.id == (int)ItemType.UndoProp).initCount;
                baseInfo.Currency[(int)ItemType.UndoProp] = count;
            }
            if (!baseInfo.Currency.ContainsKey((int)ItemType.RandomProp))
            {
                var count = GameConfigSys.item.Find(x => x.id == (int)ItemType.RandomProp).initCount;
                baseInfo.Currency[(int)ItemType.RandomProp] = count;
            }
            if (string.IsNullOrEmpty(baseInfo.LocaleCode))
            {
                baseInfo.LocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            }
            else
            {
                var locale = LocalizationSettings.AvailableLocales.GetLocale(baseInfo.LocaleCode);
                LocalizationSettings.SelectedLocale = locale;
            }

            if (baseInfo.Buff.VipExpire > 0)
            {
                //monthly vip
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                // 还没到期
                if (baseInfo.Buff.VipExpire <= now)
                {
                    // monthly vip still valid
                    baseInfo.Buff.VipExpire = 0;
                    baseInfo.Buff.IsVip = false;
                    if (!baseInfo.Buff.PermanentRemoveAds)
                    {
                        baseInfo.Buff.RemoveAds = false;
                    }
                }
                else
                {
                    var left = baseInfo.Buff.VipExpire - now;
                    UniTaskMgr.Instance.WaitForSecond(left, () =>
                    {
                        baseInfo.Buff.VipExpire = 0;
                        baseInfo.Buff.IsVip = false;
                        if (!baseInfo.Buff.PermanentRemoveAds)
                        {
                            baseInfo.Buff.RemoveAds = false;
                        }
                        Foundation.Event.Instance.SendEvent(new VIPStateChange());
                    }).Forget();
                }
                
            }
 
            bool audio=baseInfo.Setting["audio"];
            bool music=baseInfo.Setting["music"];
            AudioModule.Instance.SetPlayAudioOn(audio);
            AudioModule.Instance.SoundOpen(music);
        }

        private void InitGameFsm()
        {
            EnterGameDeal();
            ItemUtility.Instance.OnInit();
            TileManager.Instance.InitData();
            GameFsm.Instance.OnInit();
            PushNotification.Instance.AddNotifications();
            FirebaseNotification.Instance.OnInit();
            GameFsm.Instance.ToState<GameStateLobby>();
        } 
    }
}