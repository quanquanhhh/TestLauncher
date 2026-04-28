using Foundation;
using GameConfig;
using UnityEngine.UI;

namespace GamePlay.Game.Popup
{
    [Window("GetPhotoFromLevel",WindowLayer.Popup)]
    public class GetPhotoFromLevel : GetPhoto
    {
        [UIBinder("GoNext")] private Button goNextLevel;
        public override void OnCreate()
        {
            base.OnCreate();
            
            goNextLevel.onClick.AddListener(GoNextLevel);
        }

        private async void GoNextLevel()
        {
            var config = GameConfigSys.GetPhotoByName(photoname);
            if (config.sourceFrom == (int)PhotoType.Level1 ||
                config.sourceFrom == (int)PhotoType.Level2 ||
                config.sourceFrom == (int)PhotoType.Level3)
            {
                await UIModule.Instance.ShowAsync<ChoosePhoto>();
            }
            else
            {
                GameFsm.Instance.ToState<GameStateLobby>();
            }
            Close();
        }
    }
}