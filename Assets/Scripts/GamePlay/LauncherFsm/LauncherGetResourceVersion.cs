using System;
using Foundation;
using Foundation.FSM;
using UnityEngine;

namespace GamePlay.LauncherFsm
{
    public class LauncherGetResourceVersion : LauncherBase
    {
        private IFsm<LauncherFsm> _fsm;

        protected internal override void OnInit(IFsm<LauncherFsm> fsm)
        {
            stepDeltaProgress = 5f;
            base.OnInit(fsm);
        }

        protected internal override void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            _fsm = fsm;
            GetVersionInfo();
        }

        private void GetVersionInfo()
        {
            string localResVersion = PlayerPrefs.GetString("LocalVersion", GlobalSetting.ResourceVersion);
            try
            {
                int lastVersion = int.Parse(localResVersion.Replace("v", ""));
                int pacakgeVersion = int.Parse(GlobalSetting.ResourceVersion.Replace("v", ""));
                if (lastVersion > pacakgeVersion)
                {
                    GlobalSetting.ResourceVersion = localResVersion;
                }
                
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            ChangeState<LauncherInitializeAsset>(_fsm);
 
            
        }
    }
}