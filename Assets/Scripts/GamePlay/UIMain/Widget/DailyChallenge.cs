using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class DailyChallenge : UIWidget
    {
        [UIBinder("Btn")] private Button open;
        public override void OnCreate()
        {
            base.OnCreate();
            open.onClick.AddListener(OpenView);
            
            
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
        }

        private void OpenView()
        {
            var dic = new Dictionary<string, object>();
            dic.Add("pos", "DailyEntrance");
            TBAMgr.Instance.SendLogEvent("guide", dic);     
            UIModule.Instance.Close<UIGuide>();
            UIModule.Instance.ShowAsync<DailyChallengeView>();
        }
        
        public override void ChangeActive()
        {
            base.ChangeActive();
            
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.DailyChallenge) ||
                DownloadUtility.Instance.activityTags[PhotoType.DailyChallenge].Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.DailyChallenge])
            {
                gameObject.SetActive(true);
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
    }
}