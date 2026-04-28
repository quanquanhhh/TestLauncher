// using System.Collections.Generic;
// using Foundation;
// using Foundation.GridViewLoop;
// using GameConfig;
// using GamePlay.Storage;
// using GamePlay.UIMain;
//
// namespace GamePlay.Activity
// {
//     public class SecretLoop : UIWidget
//     {
//         
//         [UIBinder("")] private GridView mLoopGridView;
//
//         // private List<PhotoItem> itemData;
//         private List<SecretGift> itemData;
//         public int mTotalDataCount = 100;//total item count in the GridView  
//         int mCurrentSelectIndex = -1;
//
//         private bool gridInit = false;
//         public override void OnCreate()
//         {
//             base.OnCreate();
//             itemData = (List<SecretGift>)userDatas[0];
//             mTotalDataCount = itemData.Count;
//             mLoopGridView.InitGridView(mTotalDataCount, OnGetItemByRowColumn);
//         }
//
//         public void UpdateAll()
//         {
//             mLoopGridView.RefreshAllShownItem(); 
//         } 
//         
//         LoopGridViewItem OnGetItemByRowColumn(GridView gridView, int index, int row, int column)
//         {
//             if (index < 0)
//             {
//                 return null;
//             } 
//             
//             LoopGridViewItem item = gridView.NewListViewItem("Item");
//             var proxy = item.TryGetOrAddComponent<MonoCustomDataProxy>();
//             var pitem = proxy.GetCustomData<SecretViewItem>();
//             if (pitem == null)
//             {
//                 pitem = new SecretViewItem(item.gameObject);
//                 proxy.SetCustomData(pitem);
//             }
//
//             pitem.UpdateContent(index,itemData[index]);
//             return item;
//         }
//     }
// }