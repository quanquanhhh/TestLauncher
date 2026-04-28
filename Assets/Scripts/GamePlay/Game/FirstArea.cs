 
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Foundation;
using Foundation.Pool;
using Foundation.Storage;
using GamePlay.Storage;
using UnityEngine;

namespace GamePlay.Game
{
    public class FirstArea : UIWidget
    {
        private List<GirlTile> allTiles = new();
        private float selfWidth, selfHeight, selfX,selfY;
        public override void OnCreate()
        {
            base.OnCreate();
            selfWidth = rectTransform.rect.size.x;
            selfHeight = rectTransform.rect.size.y;
            selfX = rectTransform.anchoredPosition.x;
            selfY = rectTransform.anchoredPosition.y;
        }


        private void CheckAroundTiles(GirlTile tile)
        {
            foreach (var item in allTiles)
            {
                if (item == tile) continue;
                bool bShown = item.IsTileShown();
                if (!bShown) continue;
                bool bAround = tile.IsAround(item);
                if (bAround)
                {
                    Debug.Log($" cover {item.index} by { tile.index}" );
                    item.SetTileShown(false);
                }
            }
        }
        private void ScaleSelf()
        {
            if (!StorageManager.Instance.GetStorage<BaseInfo>().Setting["autosize"])
            {
                return;
            }
            float fMinX = float.MaxValue, fMinY = float.MaxValue, fMaxX = float.MinValue, fMaxY = float.MinValue;
            for(int i = 0;i < allTiles.Count;i++)
            {
                var item = allTiles[i];
                var data = item.GetTileData();
                if (data == null)
                {
                    continue;
                }
                float fX = data.X, fY = data.Y;
                if(fX > fMaxX) fMaxX = fX;
                if(fY > fMaxY) fMaxY = fY;
                if(fX < fMinX) fMinX = fX;
                if(fY < fMinY) fMinY = fY;
            }

            float fWidth = fMaxX - fMinX + TileManager.TileWidth;
            float fHeight = fMaxY - fMinY + TileManager.TileHeight;
            float fScaleX = selfWidth / fWidth;
            float fScaleY = selfHeight / fHeight;
            float fScale = fScaleX > fScaleY ? fScaleY : fScaleX;
            if(fScale > 2) fScale = 2;
            rectTransform.DOScale(fScale,0.2f);
            
            float fDeltaX = (fMaxX + fMinX) / 2;
            float fDeltaY = (fMaxY + fMinY) / 2;
            rectTransform.DOLocalMove(new Vector2(selfX - fDeltaX * fScale, selfY - fDeltaY * fScale),0.2f);
        } 

        public bool RemoveTile(GirlTile tile)
        {
            if (!allTiles.Contains(tile))
            {
                return false;
            }
            allTiles.Remove(tile);
            TileManager.Instance.LevelData.FirstTileList.Remove(tile.GetTileData());
            ScaleSelf();

            List<GirlTile> coveredTiles = allTiles.FindAll(x => !x.IsTileShown() && tile.IsAround(x));
            foreach (var target in coveredTiles)
            {
                bool toshow = true;
                foreach (var item in allTiles)
                {
                    if (item == target) continue;
                    bool cover = target.IsAround(item);
                    if (cover)
                    {
                        bool up = item.GetTileData().Sibling > target.GetTileData().Sibling;
                        if (up)
                        {
                            toshow = false;
                            break;
                        }
                    }
                }
                
                if (toshow)
                {
                    target.SetTileShown(true);
                }
            }

            return true;
        }

        public void BackToArea(GirlTile tile)
        {
            int nIndex = tile.GetTileData().Sibling;
            bool bAdd1 = false, bAdd2 = false;
            var mLevelData = TileManager.Instance.LevelData;
            for (int i = 0; i < mLevelData.FirstTileList.Count; i++)
            {
                var item = mLevelData.FirstTileList[i];
                bool bFit = item.Sibling > nIndex;
                if (bFit)
                {
                    mLevelData.FirstTileList.Insert(i, tile.GetTileData());
                    bAdd1 = true;
                    break;
                }
            }
            if (!bAdd1) mLevelData.FirstTileList.Add(tile.GetTileData());

            // int nSibing = allTiles.Count;
            for (int i = 0; i < allTiles.Count; i++)
            {
                var item = allTiles[i];
                bool bFit = item.GetTileData().Sibling > nIndex;
                if (bFit)
                {
                    allTiles.Insert(i, tile);
                    bAdd2 = true;
                    // nSibing = item.rectTransform.GetSiblingIndex();
                    break;
                }
            }
            if (!bAdd2) allTiles.Add(tile);
            ScaleSelf();
            CheckAroundTiles(tile);
            rectTransform.SetAsLastSibling();
            tile.rectTransform.SetParent(rectTransform);
            tile.SetStatus(true);
            tile.rectTransform.DOScale(1, 0.25f);
            Vector2 pos = new Vector2(tile.GetTileData().X, tile.GetTileData().Y);
            tile.rectTransform.DOLocalMove(pos, 0.25f).onComplete = delegate () {
                tile.SetStatus(false);
            };
        }
        public bool RefreshAllTiles()
        {
            int nCount = allTiles.Count;
            if (nCount == 0) return false; 
            TileManager.Instance.SetTouchEnable(false);
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < nCount; i++)
            {
                int nRandom = Random.Range(0, nCount);
                int nIconA = allTiles[i].GetTileData().Icon;
                int nIconB = allTiles[nRandom].GetTileData().Icon;
                allTiles[i].GetTileData().Icon = nIconB;
                allTiles[nRandom].GetTileData().Icon = nIconA;
                GirlTile tile = allTiles[i];
                sequence.Insert(0f, tile.rectTransform.DORotate(new Vector3(90, 0, 0), 0.15f));
                sequence.InsertCallback(0.15f, delegate () {
                    tile.InitIcon(tile.GetTileData().Icon);
                });
                sequence.Insert(0.15f, tile.rectTransform.DORotate(new Vector3(0, 0, 0), 0.15f));
            }
            sequence.onComplete = delegate () {
                TileManager.Instance.SetTouchEnable(true); 
            }; 
            return true;
            
        }

        public void RecycleAllTiles()
        {
            foreach (var tile in allTiles)
            {
                TileManager.Instance.PushTileToPool(tile);
            }
            allTiles.Clear();
        }

        private void CreateItem()
        {
            
        }
        public async void InitArea()
        {
            Debug.Log(TileManager.Instance.LevelData.FirstTileList.Count);
            int index = 0;
            foreach (var item in TileManager.Instance.LevelData.FirstTileList)
            {
                var obj = PoolManager.Instance.GetPoolObj("TileItem");
                obj.transform.parent = rectTransform;
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = new Vector2(item.X, item.Y);

                var tile = AddWidget<GirlTile>(obj);
                tile.InitTile(item, index);
                index++;
                tile.SetTileShown(true);
                CheckAroundTiles(tile);
                allTiles.Add(tile);
            }
            ScaleSelf(); 
        }
        public void Replay(List<GirlTile> girlTiles)
        {
            Debug.Log(TileManager.Instance.LevelData.FirstTileList.Count);
            int index = 0;
            var checkList = new List<GirlTile>();
            checkList = girlTiles; 
            allTiles.Clear();
            
            for (int i = 0; i < TileManager.Instance.LevelData.FirstTileList.Count; i++)
            {
                GirlTile tile = null;
                if (checkList.Count <= i)
                { 
                    var  obj = PoolManager.Instance.GetPoolObj("TileItem");  
                    tile = AddWidget<GirlTile>(obj);
                }
                else
                {
                    tile = checkList[i];
                }
                var item = TileManager.Instance.LevelData.FirstTileList[i];
                tile.rectTransform.parent = rectTransform;
                tile.rectTransform.localScale = Vector3.one;
                tile.rectTransform.localPosition = new Vector2(item.X, item.Y);
                tile.InitTile(item, i);
                tile.SetTileShown(true);
                CheckAroundTiles(tile);
                allTiles.Add(tile);
                tile.rectTransform.SetSiblingIndex(item.Sibling);
            }
            
            ScaleSelf();
        }

        public List<GirlTile> FindAndRemoveIcon( int topIcon, int count)
        {
            var icons = allTiles.FindAll(x => x.GetTileData() != null && x.GetTileData().Icon == topIcon);
            if (icons.Count == 0)
            {
                Debug.LogError("Check Error FindAndRemoveIcon  icon = " + topIcon);
            }
            var sortedIcons = new List<GirlTile>();
            sortedIcons.AddRange(icons.FindAll(x => x.IsTileShown()));
            sortedIcons.AddRange(icons.FindAll(x => !x.IsTileShown()));

            List<GirlTile> tiles = new();
            for (int i = 0; i < 3-count; i++)
            {
                tiles.Add(sortedIcons[i]); 
            }

            return tiles;
        }
    }
}