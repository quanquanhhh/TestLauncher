// using System.Collections.Generic;
// using DG.Tweening;
// using Foundation;
// using GameConfig;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using Event = Foundation.Event;
//
// namespace GamePlay.Game.Popup
// {
//     [Window("GameWinPop", WindowLayer.Popup)]
//     public class GameWinPopup : UIWindow
//     {
//         [UIBinder("TextAd")] private TextMeshProUGUI TextAd;
//         [UIBinder("TextReward")] private TextMeshProUGUI currentDiamondText;
//         [UIBinder("ImgMulti")] private Transform multi;
//         [UIBinder("ImgArrow")] private RectTransform arrow;
//         [UIBinder("Continue")] private Button claimBtn;
//         private List<int> itemMul;
//         private Sequence seq;
//         private int currentIndex = 0;
//         private int currentDiamond = 0;
//         public override void OnCreate()
//         {
//             base.OnCreate();
//             var muls = GameConfigSys.baseGame.GameWin;
//             for (int i = 0; i < 5; i++)
//             {
//                 multi.Find("TextMulti" + i).GetComponent<TextMeshProUGUI>().text = 
//                     "x"+muls[i].ToString();
//             }
//
//             itemMul = muls;
//             currentDiamond = TileManager.Instance._levelInfo.WinDiamond;
//             claimBtn.onClick.AddListener(ClaimReward);
//         }
//
//         private void ClaimReward()
//         {
//             Event.Instance.SendEvent(new AddItem((int)ItemType.Diamond,currentDiamond));
//             GameFsm.Instance.ToState<GameStateLobby>();
//             Close();
//         }
//
//         public override void OnShow()
//         {
//             base.OnShow();
//              
//             seq = DOTween.Sequence();
//             arrow.anchoredPosition = new Vector2(-370, -82);
//             seq.Append(arrow.DOAnchorPosX(364, 1.5f));
//             seq.Append(arrow.DOAnchorPosX(-370, 1.5f));
//             seq.SetLoops(-1);
//             
//         }
//         private void UpdateMultiIndex()
//         {
//             float fX = arrow.anchoredPosition.x;
//             int nIndex = 0;
//             if(fX < -280)   nIndex = 0;
//             else if(fX < -94) nIndex = 1;
//             else if(fX < 88) nIndex = 2;
//             else if(fX < 270) nIndex = 3;
//             else nIndex = 4;
//
//             if(nIndex != currentIndex)
//             {
//                 currentIndex = nIndex;
//                 UpdateRewardNums();
//             }
//         }
//         private void UpdateRewardNums()
//         {
//             int nMulti = itemMul[currentIndex];
//             TextAd.text = "x" + nMulti; 
//             currentDiamondText.text = "+" + currentDiamond * nMulti;;
//         }
//     }
// }