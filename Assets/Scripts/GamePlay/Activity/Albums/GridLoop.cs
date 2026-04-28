using System.Collections.Generic;
using Foundation;
using Foundation.GridViewLoop;
using GamePlay.Storage;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    public class GridLoop : UIWidget
    {
        [UIBinder("")] private GridView mLoopGridView;

        private List<PhotoItem> itemData;
        public int mTotalDataCount = 100;//total item count in the GridView  
        int mCurrentSelectIndex = -1;

        private bool gridInit = false;
        private AlbumsPanel _panel;
        public override void OnCreate()
        {
            base.OnCreate();
            itemData = (List<PhotoItem>)userDatas[0];
            _panel = (AlbumsPanel)userDatas[1];
            mTotalDataCount = itemData.Count;
            mLoopGridView.AdjustContainSize = true;
            
            float sx = ViewUtility.GetEnoughXScale(); 
            // mLoopGridView.ItemSize *= sx;
            // mLoopGridView.ItemPadding *= sx;
            mLoopGridView.ItemSclae = sx;
            mLoopGridView.InitGridView(mTotalDataCount, OnGetItemByRowColumn);
        }

        public void SetCount(List<PhotoItem> count)
        {
            if (count.Count <= mCurrentSelectIndex)
            {
                mCurrentSelectIndex = -1;
            }

            itemData = count;
            mTotalDataCount = count.Count;
            mLoopGridView.SetListItemCount(count.Count, false);
            mLoopGridView.RefreshAllShownItem();
 
            
        } 
        
        LoopGridViewItem OnGetItemByRowColumn(GridView gridView, int index, int row, int column)
        {
            if (index < 0)
            {
                return null;
            } 
            LoopGridViewItem item = gridView.NewListViewItem("Item");
            var proxy = item.TryGetOrAddComponent<MonoCustomDataProxy>();
            var pitem = proxy.GetCustomData<AlbumsItem>();
            if (pitem == null)
            {
                pitem = new AlbumsItem(item.gameObject,_panel);
                proxy.SetCustomData(pitem);
            }

            pitem.UpdateContent(index,itemData[index]);
            return item;
        }
 
 
    }
}