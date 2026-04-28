// using System.Collections.Generic;
// using DG.Tweening;
// using Foundation;
// using Foundation.Pool;
// using GamePlay.Game.Data;
// using UnityEngine;
//
// namespace GamePlay.Game
// {
//     public class SecondArea : UIWidget
//     {
//         List<Transform> AllTrash;
//         List<List<GirlTile>> AllTiles = new List<List<GirlTile>>();
//         public override async void OnCreate()
//         {
//             base.OnCreate();
//             
//         }
//         private void FillAllTrash()
//         {
//             if(AllTrash == null)
//             {
//                 AllTrash = new List<Transform>();
//                 var Container = rectTransform.Find("Container");
//                 for(int i = 1;i <= 6;i++)
//                 {
//                     var item = Container.Find($"ImgTile ({i})");
//                     AllTrash.Add(item);
//                 }
//             }
//             for (; (AllTrash.Count - AllTiles.Count) > 0;)
//             {
//                 AllTiles.Add(new List<GirlTile>());
//             }
//         }
//         public bool RemoveTile(GirlTile tile)
//         {
//             
//             int nTrash = tile.GetTileData().TrashId;
//             int nInTrash = tile.GetTileData().InTrashId;
//             if (nTrash < 0 || nTrash >= AllTiles.Count) return false;
//             if (nInTrash < 0 || nInTrash >= AllTiles[nTrash].Count) return false;
//             AllTiles[nTrash].RemoveAt(nInTrash);
//             
//             TileManager.Instance.LevelData.SecondTileList.Remove(tile.GetTileData());
//             return true;
//         }
//   
//
//         public void BackToArea(GirlTile tile)
//         {
//             
//             int nTrash = tile.GetTileData().TrashId;
//             TileManager.Instance.LevelData.SecondTileList.Add(tile.GetTileData());
//             AllTiles[nTrash].Add(tile);
//             tile.GetTileData().InTrashId = AllTiles[nTrash].Count - 1;
//             Transform target = AllTrash[nTrash];
//             rectTransform.SetAsLastSibling();
//             tile.rectTransform.SetParent(target);
//             tile.SetStatus(  true);
//             tile.rectTransform.DOScale(1, 0.25f);
//             tile.rectTransform.DOLocalMove(Vector2.zero, 0.25f).onComplete = delegate () {
//                 tile.SetStatus(  false);
//             };
//         }
//         public void RecycleAllTiles()
//         {
//             foreach (var tiles in AllTiles)
//             {
//                 foreach (var tile in tiles)
//                 {
//                     TileManager.Instance.PushTileToPool(tile); 
//                 }
//             }
//             AllTiles.Clear();
//         }
//     }
// }