using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class SettingWidget : UIWidget
    {
        [UIBinder("")] private Canvas canvas;
        [UIBinder("Btn")] private Button openView;
        private int oldOrder;

        public override void OnCreate()
        {
            base.OnCreate();
            oldOrder = (int)userDatas[0];
            
            canvas.sortingOrder = oldOrder + 1;
            openView.onClick.AddListener(OpenSettingView);
            SubScribeEvent<ChangeTopUIOrder>(OnChangeTopUIOrder);
            
        }

        private void OpenSettingView()
        {
            UIModule.Instance.ShowAsync<UISetting>("home");
        }

        private void OnChangeTopUIOrder(ChangeTopUIOrder obj)
        {
            if (obj.hasSettingWidget && obj.isTop)
            {
                canvas.sortingOrder = GUtility.TopUISorting;
            }
            else if (!obj.isTop)
            {
                canvas.sortingOrder = oldOrder;
            }
        }
    }
}