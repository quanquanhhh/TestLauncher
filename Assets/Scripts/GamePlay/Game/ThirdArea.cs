using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Game.Popup;
using GamePlay.Storage;
using Spine.Unity;
using UnityEngine;
using Event = Foundation.Event;

namespace GamePlay.Game
{
    public class ThirdArea : UIWidget
    {
        [UIBinder("Container")] private Transform container;
        [UIBinder("MatchArea")] private Transform MatchArea;
        [UIBinder("EffectArea")] private Transform EffectArea;
        [UIBinder("SpineEliminate")] private SkeletonGraphic SpineEliminate;
        
        
        List<SkeletonGraphic> EliminatList = new List<SkeletonGraphic>();
        
        List<GirlTile> allTiles = new List<GirlTile>();
        List<Transform> AllTileNode = new ();

        private GameMain _gameMainMain;
        
        private int count = 0;
        private bool moving = false;

        private int removingCount = 0;
        public override void OnCreate()
        {
            base.OnCreate();
            _gameMainMain = (GameMain)userDatas[0];
            SpineEliminate.gameObject.SetActive(false);
            EliminatList.Add(SpineEliminate);
            if (AllTileNode.Count == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    var item = container.Find("ImgTile" + i);
                    AllTileNode.Add(item);
                    allTiles.Add(null);
                }
            } 
            
        }
 

        private bool IsFull()
        {
            var all =  allTiles.FindAll(x=>x!= null);
            count = all.Count;
            return (all.Count ) == 8;
        }
        private void ChangeIndexFromAtoB(int nAIndex, int nBIndex)
        {
            var node = AllTileNode[nAIndex];
            AllTileNode.RemoveAt(nAIndex);
            AllTileNode.Insert(nBIndex, node);
            
            var tile = allTiles[nAIndex];
            allTiles.RemoveAt(nAIndex);
            allTiles.Insert(nBIndex, tile);
            float fX = GetXByIndex(nBIndex);
            node.transform.localPosition = new Vector2(fX, 0);
        }         
        private float GetXByIndex(int nIndex)
        {
            float fX = -427 + nIndex * 123;
            return fX;
        }
        public bool AddTile(GirlTile tile, bool anim)
        {  
            if (IsFull()) 
                return false;
            int icon = tile.GetTileData().Icon;
            int index = allTiles.FindLastIndex(x => x!= null && x.GetTileData().Icon == icon);
            if (index < 0)
            {
                index = count - removingCount;
            }
            else
            {
                index++;
            }

            Debug.Log("AddTile : " + tile.index + " " + tile.GetTileData().Icon + " |" + count + "|" + index +"|" + removingCount);
            ChangeIndexFromAtoB(7, index);
            allTiles[index] = tile;
            var targetNode = AllTileNode[index];
            targetNode.SetAsLastSibling();
            tile.rectTransform.SetParent(targetNode);

            var isTripple = CheckMatch(icon);
            bool isFail = isTripple.Count < 3 && removingCount == 0 && IsFull();
            if (isTripple.Count == 3)
            {
                removingCount += isTripple.Count;
            }
            if (anim)
            {
                tile.SetStatus(true);
                tile.PlayClickEffect();
                float scl = tile.rectTransform.localScale.x * 1.3f;
                float time = 0.07f;
                Sequence seq = DOTween.Sequence();
                seq.Insert(0f,tile.rectTransform.DOScale(scl,time));
                seq.Insert(time,tile.rectTransform.DOScale(1, 0.15f));
                seq.Insert(time,tile.rectTransform.DOLocalMove(Vector2.zero, 0.15f));
                seq.onComplete = delegate () {
                    tile.SetStatus(false);
                    if (isTripple.Count == 3)
                    {
                        PlayMatch(isTripple);
                    }
                    else if (isFail)
                    {
                        TileManager.Instance.gameover = true;
                        UIModule.Instance.ShowAsync<GameFailPopup>();
                    }
                };
                rectTransform.SetAsLastSibling();
            }
            else
            {
                tile.rectTransform.localPosition = Vector2.zero;
                tile.rectTransform.localScale = Vector3.one;
            }

            bool hasSame = TileManager.Instance.LevelData.ThirdTileList.Contains(tile.GetTileData());
            if (!hasSame)
            {
                TileManager.Instance.LevelData.ThirdTileList.Insert(index, tile.GetTileData());
                TileManager.Instance.LevelData.OrderList.Add(tile.GetTileData().Sibling);
            }

            return true;
        }


        private List<GirlTile> CheckMatch(int icon)
        {
            var sames = allTiles.FindAll(x =>x != null && x.GetTileData().Icon == icon );
            return sames;
        }

        public void RemoveMatch(List<GirlTile> tiles, Transform parent)
        {
            int x = 110; 

            Sequence seq = DOTween.Sequence();
            GUtility.Vibrate();
            for (int index = 0; index < tiles.Count; index++)
            {
                int temp = index;
                var tile = tiles[index];
                tile.SetStatus(true);
                tile.SetTileShown(true);
                float scl = tile.rectTransform.localScale.x * 1.3f;
                float time = 0.07f;
                float posx = x * (index - 1);
                Transform oldParent = tile.rectTransform.parent;
                tile.rectTransform.parent = parent;
                DealTilesData(oldParent, tile); 
                seq.Insert(0f,tile.rectTransform.DOScale(scl,time));
                seq.Insert(time,tile.rectTransform.DOScale(1, 0.25f));
                seq.Insert(time,tile.rectTransform.DOAnchorPos(new Vector2(posx,0), 0.25f));
                seq.onComplete += delegate ()
                {
                    tile.rectTransform.parent = oldParent;
                    PlayMatchEffect(tile, temp);
                    tile.SetStatus(false);
                    
                };
                _gameMainMain.CheckTileState(tile);
            }
            
            CheckWin(seq);
            UpdateTileNodes();
        }

        private void CheckWin(Sequence sequence)
        { 
            bool bWin = TileManager.Instance.IsLevelWin();
            if (bWin)
            {
                sequence.onComplete += delegate ()
                {
                    TileManager.Instance.LevelWin(); 
                };
            }
            else
            {
                UpdateTileNodes();
            }
        }

        private void DealTilesData(Transform parent, GirlTile tile)
        {
            int index = AllTileNode.IndexOf(parent);

            if (index >=0 && allTiles[index] != null)
            {
                allTiles[index] = null;
                ChangeIndexFromAtoB(index, 7);
                TileManager.Instance.LevelData.ThirdTileList.Remove(tile.GetTileData());
                TileManager.Instance.LevelData.OrderList.Remove(tile.GetTileData().Sibling);
            }
        }
        private void PlayMatchEffect(GirlTile tile, int mid,Sequence eliminateSeq = null )
        {
            AudioModule.Instance.PlayOneShotSfx("disappear"); 
            var cardNode = tile.rectTransform.parent; 
            tile.rectTransform.SetParent(MatchArea);
            DealTilesData(cardNode, tile);
            if (eliminateSeq == null)
            {
                eliminateSeq = DOTween.Sequence(); 
            }

            var offsetPos = (mid - 1) * 30 + tile.rectTransform.anchoredPosition.x;
            var endpos = -(mid - 1) * 133 + offsetPos;
            eliminateSeq.Insert(0f, tile.rectTransform.DOScale(1.15f, 0.08f));
            
            eliminateSeq.Insert(0.06f, tile.rectTransform.DOAnchorPosX(endpos, 0.13f).SetEase(Ease.InBack)); 
            eliminateSeq.InsertCallback(0.1f, delegate()
            {
                if (mid == 1)
                {
                    ShowTileEffect(tile);
                }
            });
            eliminateSeq.InsertCallback(0.15f, delegate () {
                if (mid == 1)
                {
                    MatchReward(tile);
                }
            });
            eliminateSeq.Insert(0.18f, tile.rectTransform.DOScale(0f, 0.15f));
            eliminateSeq.InsertCallback(0.33f, delegate ()
            {
                TileManager.Instance.PushTileToPool(tile); 
            });
        }
        private bool PlayMatch(List<GirlTile> tiles)
        { 
            Sequence eliminateSeq = DOTween.Sequence();
            removingCount -= tiles.Count;
            GUtility.Vibrate();
            for (int i = 0; i < tiles.Count; i++)
            {
                PlayMatchEffect(tiles[i], i, eliminateSeq);
            }

            CheckWin(eliminateSeq);
            // bool bWin = TileManager.Instance.IsLevelWin();
            // if (bWin)
            // {
            //     eliminateSeq.onComplete = delegate ()
            //     {
            //         TileManager.Instance.LevelWin(); 
            //     };
            // }
            // else
            // {
            //     UpdateTileNodes();
            // }
            return true;
        }
        private void MatchReward(GirlTile tile)
        {
            // int num = 1;
            // int level = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level;
            // switch (level)
            // {
            //     case 1: num = 5; break;
            //     case 2: num = 10; break;
            //     default: num = 40; break;
            // }
            //  
            // Event.Instance.SendEvent(new AddItem((int)ItemType.Coin,num , tile.rectTransform.position));
        }
        
        private void ShowTileEffect(GirlTile tile)
        {
            var item = GetEliminateSpine();
            item.transform.localPosition = tile.rectTransform.localPosition;
            item.PlayAsync("animation", false, () =>
            {
                EliminatList.Add(item);
                item.gameObject.SetActive(false);
            }); 
        } 
        private SkeletonGraphic GetEliminateSpine()
        {
            if(EliminatList.Count == 0)
            {
                var item = GameObject.Instantiate(SpineEliminate,EffectArea);
                EliminatList.Add(item);
            }
            var target = EliminatList[0];
            EliminatList.RemoveAt(0);
            target.gameObject.SetActive(true);
            return target;
        }
        public async void UpdateTileNodes()
        {
            if (moving)
            {
                return;
            }
            while (true)
            {

                bool bMove = false;
                for (int i = 0; i < 8; i++)
                {
                    var node = AllTileNode[i];
                    float fCurX = node.transform.localPosition.x;
                    float fTargetX = GetXByIndex(i);
                    float fDelta = fTargetX - fCurX;
                    float fAbs = Mathf.Abs(fDelta);
                    float fDir = fDelta / fAbs;
                    if (fDelta < 0.5f && fDelta > -0.5f) continue;
                    bMove = true;
                    float fEndX = fCurX;
                    if (fAbs > 200)
                    {
                        fEndX = fCurX + fDir * 50;
                    }
                    else if (fAbs > 30)
                    {
                        fEndX = fCurX + fDir * 30;
                    }
                    else
                    {
                        fEndX = fCurX + fDelta;
                    }
                    node.transform.localPosition = new Vector2(fEndX, 0);
                }
                
                if (!bMove)
                { 
                    moving = false;
                    break;
                }
                else
                {
                    moving = true;
                }
                await UniTaskMgr.Instance.Yield();
            }
        }

 

        public bool RemoveOneKindTile()
        { 
            var all =   allTiles.FindAll(x=>x!= null);
            if (all.Count == 0)
            {
                return false;
            }
            Dictionary<int,  List<GirlTile>> icon_index = new();
            int hasTopCount = -1;
            int topIcon = -1; 
            for (int i = 0; i < all.Count; i++)
            {
                int icon = all[i].GetTileData().Icon;
                if (!icon_index.ContainsKey(icon))
                {
                    icon_index.Add(icon, new List<GirlTile>());
                }
                icon_index[icon].Add(all[i]);
                if (icon_index[icon].Count > hasTopCount)
                {
                    hasTopCount = icon_index[icon].Count;
                    topIcon = icon;
                }
            }
            
            _gameMainMain.RemoveIcon(topIcon,hasTopCount, icon_index[topIcon]);
            return true;
        }

        // public bool RemoveAllTileToTrash()
        // {
        //     
        //     
        //     
        //     TileManager.Instance.gameover = false;
        //     var all =  allTiles.FindAll(x=>x!= null);
        //     int count = all.Count;
        //     if (count == 0) return false;
        //     if (count > 6) count = 6;
        //     for (int i = 0; i < count; i++)
        //     {
        //         int sibling = int.MaxValue;
        //         GirlTile tile = null;
        //         foreach (var item in allTiles)
        //         {
        //             if(item == null) continue;
        //             int index = item.rectTransform.parent.GetSiblingIndex();
        //             if (index < sibling)
        //             {
        //                 sibling = index;
        //                 tile = item;
        //             }
        //         }
        //
        //         if (tile == null) continue;
        //         int nodeindex = AllTileNode.FindIndex(a => a == tile.rectTransform.parent);
        //         allTiles[nodeindex] = null;
        //         TileManager.Instance.LevelData.ThirdTileList.Remove(tile.GetTileData());
        //         TileManager.Instance.LevelData.OrderList.Remove(tile.GetTileData().Sibling);
        //         tile.rectTransform.parent = MatchArea;
        //         tile.SetBtnEnable(true);
        //         ChangeIndexFromAtoB(nodeindex, 7);
        //         _gameMainMain.SecondAreaAddTile(tile);
        //     }
        //
        //     return true;
        //
        // }

        public bool FromAreaBack()
        {
            var all =   allTiles.FindAll(x=>x!= null);
            
            int nCount = all.Count;
            if (nCount == 0) return false;

            int nSibing = -1;
            GirlTile tile = null;
            foreach (var item in allTiles)
            {
                if (item == null) continue;
                int nIndex = item.rectTransform.parent.GetSiblingIndex();
                if (nIndex > nSibing)
                {
                    nSibing = nIndex;
                    tile = item;
                }
            }
            if (tile == null)
                return false;
            int nIndexA = AllTileNode.FindIndex(a => a == tile.rectTransform.parent);
            allTiles[nIndexA] = null;
            TileManager.Instance.LevelData.ThirdTileList.Remove(tile.GetTileData());
            TileManager.Instance.LevelData.OrderList.Remove(tile.GetTileData().Sibling);
            tile.rectTransform.SetParent(MatchArea);
            tile.SetBtnEnable(true);
            ChangeIndexFromAtoB(nIndexA,7);
            _gameMainMain.BackToFirst(tile);
            // int nTrash = tile.GetTileData().TrashId;
            // int nInTrash = tile.GetTileData().InTrashId;
            // if(nTrash >= 0 && nInTrash >= 0)
            // {
                // _gameMainMain.BackToSecond(tile);
                // PanelGame.secondArea.BackToSecond(tile);
            // }
            // else
            // {
                // PanelGame.firstArea.BackToFirst(tile);
            // }
            return true;
        }
        
        public void RecycleAllTiles()
        {
            foreach (var tile in allTiles)
            {
                TileManager.Instance.PushTileToPool(tile);  
            }

            for (int i = 0; i < allTiles.Count; i++)
            {
                if (allTiles[i] != null)
                {
                    TileManager.Instance.PushTileToPool(allTiles[i]);
                    allTiles[i] = null;
                }
            } 
        }
    }
}