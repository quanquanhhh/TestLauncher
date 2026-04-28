using Foundation;
using Foundation.Storage;
using GamePlay.Storage;
using TMPro;
using UnityEngine.UI;

namespace GamePlay.DebugPanel
{
    public class DebugCell : UIWidget
    {
        public ActivityInfo activityInfo = StorageManager.Instance.GetStorage<ActivityInfo>();
        [UIBinder("")] private Button btn;
        [UIBinder("text")] private TextMeshProUGUI des;

        public override void OnCreate()
        {
            base.OnCreate();
            des.text = GetDes();
            btn.onClick.AddListener(DebugFun);
        }

        public virtual void DebugFun()
        {
            
        }

        public virtual string GetDes()
        {
            return "";
        }
    }
}