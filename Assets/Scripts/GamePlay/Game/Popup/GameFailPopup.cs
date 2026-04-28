using System;
using Foundation;
using Foundation.AudioModule;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.Game.Popup
{
    [Window("GameFailPop", WindowLayer.Popup)]
    public class GameFailPopup : UIWindow
    {
        [UIBinder("BtnClose")] private Button closeBtn;
        [UIBinder("BtnRestart")] private Button resestBtn;
        [UIBinder("BtnContinue")] private Button adBtn;

        [UIBinder("Top")] private RectTransform top;

        public override void OnCreate()
        {
            base.OnCreate();
            top.anchoredPosition -= new Vector2(0, ViewUtility.AdjustTopHeight);
            closeBtn.onClick.AddListener(OverGame);
            resestBtn.onClick.AddListener(ResetGame);
            adBtn.onClick.AddListener(ContinueGame);
        }

        private void OverGame()
        {
            AudioModule.Instance.ClickAudio();
            TileManager.Instance.LevelFail();
            GameFsm.Instance.ToState<GameStateLobby>();
            Close();
        }

        private void ContinueGame()
        {
            AudioModule.Instance.ClickAudio();
            AdMgr.Instance.PlayRV(() =>
            {
                Event.Instance.SendEvent(new ReviveGame(true)); 
                Close();
            },"GameFail");
        }

        private void ResetGame()
        {
            AudioModule.Instance.ClickAudio();
            
                 
            if (GUtility.IsEnoughItem(ItemType.RemoveProp, 1))
            {
                Event.Instance.SendEvent(new ReviveGame());
                Close();
            }
            else
            {
                Action action = () =>
                {
                    ContinueGame();
                };
                UIModule.Instance.ShowAsync<NoEnoughProp>(action);
            }
        }
    }
}