using System;
using System.Collections.Generic;
using Foundation;
using Foundation.FSM;
using Foundation.ScreenSecurity;
using Foundation.Storage;
using GamePlay.Component;
using GamePlay.Storage;
using UnityEngine;

namespace GamePlay.LauncherFsm
{
    public class LauncherSetUpView : LauncherBase
    {
        protected internal override void OnInit(IFsm<LauncherFsm> fsm)
        {
            stepDeltaProgress = 2f;
            base.OnInit(fsm);
        }

        protected internal override void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            CheckStorage();
            InitAPPConfigAndSDK();
            ViewUtility.SetUpViewSize();
            ScreenSecurity.EnableAndroidSecureFlag();
            ShowLoading(fsm); 
        }

 
        private void CheckStorage()
        {
            List<StorageBase> storagebase =  new List<StorageBase>();
            storagebase.Add(new BaseInfo());
            storagebase.Add(new PhotoInfo());
            storagebase.Add(new DailyInfo());
            storagebase.Add(new ActivityInfo());
            storagebase.Add(new StatisticsInfo());
            StorageManager.Instance.Init(storagebase);
        }
        private void InitAPPConfigAndSDK()
        {
            ConfigManager.GetUseConfig();
            
            GameCommon.InitSDKsByPlatform();  
        }

        private void ShowLoading(IFsm<LauncherFsm> fsm)
        {
            var load = Resources.Load<GameObject>("Loading/Loading");
            var uiCanvas = GameObject.Find("Root/UIRoot/UICanvas");
            if (uiCanvas != null)
            {
                var loadView = GameObject.Instantiate(load, uiCanvas.transform);
                loadView.GetComponent<Canvas>().sortingLayerName = "Loading";
                loadView.name = "Loading";
                fsm.Owner.LoadingView = loadView;
                
                
                var PhotoArea = loadView.transform.Find("PhotoArea");
            
                var flow = PhotoArea.TryGetOrAddComponent<UICoverFlow>();

                var items = new List<RectTransform>();
                for (int i = 0; i < 7; i++)
                {
                    var item = PhotoArea.Find("img" + i);
                    items.Add(item.GetComponent<RectTransform>());
                }
            
                flow.SetContent(items);
                flow.SetLoop(true);
                flow.SetAutoScroll(true);
                PhotoArea.gameObject.SetActive(true);
            }

            ChangeState<LauncherGetResourceVersion>(fsm);
        }
    }
}