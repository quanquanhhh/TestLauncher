using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.DebugPanel
{
    [Window("DebugUI", WindowLayer.Popup)]
    public class DebugPanel : UIWindow
    {
        [UIBinder("debugPanel")] private GameObject panel;
        [UIBinder("entrance")] private Button entrance;
        [UIBinder("model")] private Transform model;
        [UIBinder("CloseBtn")] private Button hideBtn;
        public override void OnCreate()
        {
            base.OnCreate();
            hideBtn.onClick.AddListener(ClosePanel);
            entrance.onClick.AddListener(OpenPanel);
            model.gameObject.SetActive(false);
            CreateCell();
        }

        private void ClosePanel()
        {
            panel.SetActive(false);
        }

        private void CreateObj<T>() where T : DebugCell, new()
        {
            
            var obj = GameObject.Instantiate(model, model.parent);
            AddWidget<T>(obj.gameObject, true);
        }

        private void CreateCell()
        {
            CreateObj<ClearPassActivityInfo>();
            CreateObj<ClearSignActivityInfo>();
            CreateObj<ClearPhotoInfo>();
            CreateObj<ClearSecretActivityInfo>();
            CreateObj<ClearDailyChallengeInfo>();
            CreateObj<Activity_LuckyWheel>();
            CreateObj<ChangeUserTypeFun>();
                
            
        }

        private void OpenPanel()
        {
            panel.SetActive(true);
        }
 
    }
}