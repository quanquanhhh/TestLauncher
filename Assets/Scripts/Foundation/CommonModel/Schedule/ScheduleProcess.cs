using UnityEngine;

namespace Foundation
{
    public class ScheduleProcess: MonoBehaviour
    {
        public static bool LoadingFinished = false;
        public float PauseThreshold;
        public float _pauseStartTime = 0;
        public int cd;
        public float lastPlayTime = 0;
        
 
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (AdMgr.Instance.isPlayAd || !LoadingFinished)
            {
                return;
            }

            if (pauseStatus)
            {
                Event.Instance.SendEvent(new FocusLeft());
            }
            else
            {
                Event.Instance.SendEvent(new FocusEnter());
            }
        
            // if (UIModule.Instance.Get<NotificationDialog>() != null)
            // {
            //     UIModule.Instance.Get<NotificationDialog>().ComeBack();
            //     return;
            // }
            // if (PauseThreshold <= 0)
            // {
            //     PauseThreshold = ConfigInfo.Insert.JbibhBkciHgki[0];
            // }
            //
            // if (cd <= 0)
            // {
            //     cd = ConfigInfo.Insert.JbibhBkciCh[0]; 
            // }
            // if (pauseStatus)
            // {
            //     _pauseStartTime = Time.realtimeSinceStartup; 
            //     return;
            // }
            // else
            // {
            //     float pauseDuration = Time.realtimeSinceStartup - _pauseStartTime;
            //     float overCD = Time.realtimeSinceStartup - lastPlayTime;
            //     if (pauseDuration >= PauseThreshold && overCD >= cd)
            //     {
            //         AdMgr.Instance.PlayIVFromAppComeBack(() =>
            //         {
            //             lastPlayTime = Time.realtimeSinceStartup;
            //         });
            //     }
            // }
            
        }
    }
}