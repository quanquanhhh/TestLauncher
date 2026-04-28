using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Activity;
using GamePlay.Storage;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class BeautyEncounters : UIWidget
    {
        [UIBinder("Btn")] private Button btn;
        public override void OnCreate()
        {
            base.OnCreate();
            
            btn.onClick.AddListener(() =>
            {
                UIModule.Instance.ShowAsync<BeautyDraft>();
                
                var dic = new Dictionary<string, object>();
                dic.Add("pos", "BeautyEntrance");
                TBAMgr.Instance.SendLogEvent("guide", dic);      
                UIModule.Instance.Close<UIGuide>();
            });
            
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
            
        }
 
        public override void ChangeActive()
        {
            base.ChangeActive();
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.BeautyDraft) ||
                DownloadUtility.Instance.activityTags[PhotoType.BeautyDraft].Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.BeautyDraft])
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