using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Foundation;
using GamePlay.Component;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using Event = Foundation.Event;

namespace GamePlay
{
    public class LoadingLuncher : MonoBehaviour
    {
 
        public bool LocalRes = true;
        public bool LoadS3 = true;
        public bool RunAccountCheck = true;
        private int FrameRate = 60;
        public static float MAXCHECKTIME = 0;
        
        
        public static EPlayMode RunPlayMode = EPlayMode.EditorSimulateMode;
        
        private LauncherFsm.LauncherFsm launcherFsm;
        private void Start()
        {
            
#if UNITY_ANDROID 
            MonitorManager.Instance.CheckLimited();
#endif
            
            StartAsync().Forget();
            StartLauncher(); 
        }



        private void StartLauncher()
        {
            
            launcherFsm = new LauncherFsm.LauncherFsm();
            launcherFsm.Initialize();
            launcherFsm.Start();
            
        }

        private EPlayMode ResolvePlayMode()
        {
#if UNITY_EDITOR
            return RunPlayMode;
#else
            return EPlayMode.HostPlayMode;
#endif
        }

        private void Update()
        {
            if (launcherFsm != null)
            {
                launcherFsm.Update();
            }
        }

        private async UniTask StartAsync()
        { 
            RunPlayMode = ResolvePlayMode();
            GlobalSetting.Configure(new YooAssetRuntimeConfig
            {
                PackageName =  GlobalSetting.PackageName, 
                PlayMode = RunPlayMode
            });
            Application.targetFrameRate = FrameRate;
            Application.runInBackground = true;
            
            GameCommon.GameName = GlobalSetting.PackageName;
            
            
            if (!RunAccountCheck && !LoadS3)
            {
                MAXCHECKTIME = 1;
            }
            
            await UniTask.NextFrame();
            
             
        }
  
  
  
        
    }
}