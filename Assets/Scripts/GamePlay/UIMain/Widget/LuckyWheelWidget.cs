using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class LuckyWheelWidget : UIWidget
    {
        [UIBinder("Btn")] private Button entrance;
        public override void OnCreate()
        {
            base.OnCreate();
            entrance.onClick.AddListener(OpenLuckyWheel);
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
        }
        public override void ChangeActive()
        {
            base.ChangeActive();
            
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.LuckyWheel) ||
                DownloadUtility.Instance.activityTags[PhotoType.LuckyWheel].Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.LuckyWheel])
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
        private void OpenLuckyWheel()
        {
            var dic = new Dictionary<string, object>();
            dic.Add("pos", "LuckyWheelEntrance");
            TBAMgr.Instance.SendLogEvent("guide", dic);    
            UIModule.Instance.Close<UIGuide>();
            UIModule.Instance.ShowAsync<LuckyWheelView>();
        }
    }
}