using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Activity.Pass;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class PassMainWidget : UIWidget
    {
        [UIBinder("Btn")] private Button btn;
        [UIBinder("iconA")] private GameObject iconA;
        [UIBinder("iconB")] private GameObject iconB;
        public override void OnCreate()
        {
            base.OnCreate();
            
            btn.onClick.AddListener(() =>
            {
                UIModule.Instance.ShowAsync<PassView>();
                
                var dic = new Dictionary<string, object>();
                dic.Add("pos", "PassEntrance");
                TBAMgr.Instance.SendLogEvent("guide", dic);   
                UIModule.Instance.Close<UIGuide>();
            });
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
            SubScribeEvent<ChangeUserType>(OnChangeUserType);
        }
        private void OnChangeUserType(ChangeUserType obj)
        {
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
        }
        public override void ChangeActive()
        {
            base.ChangeActive();
            
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.Pass) ||
                DownloadUtility.Instance.activityTags[PhotoType.Pass].Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.Pass])
            {
                GameConfigSys.DealPassImage(); 
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