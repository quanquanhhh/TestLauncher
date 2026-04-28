using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlay.UIMain.Widget
{
    public class CurrencyWidgetDic
    {
        
        public static List<CoinWidget> coinList = new List<CoinWidget>(); 
        public static List<DiamondWidget> diamondList = new List<DiamondWidget>();
        
        public static void AddCoinWidget(CoinWidget cw)
        {
            if (cw != null && cw.gameObject != null)
            {
                coinList.Add(cw);
            }
        }
        public static void DelCoinWidget(CoinWidget cw)
        {
            if (cw != null && coinList.Contains(cw))
            {
                coinList.Remove(cw);
            }
        }
        public static CoinWidget GetCurCoinWidget()
        {
            if (coinList == null || coinList.Count == 0)
            {
                return null;
            }
            for (int i = coinList.Count - 1; i >= 0 ; i--)
            {
                if (coinList[i].gameObject.activeInHierarchy)
                {
                    return coinList[i];
                }
            }
            return coinList.Last();
        }
        public static void AddDiamondWidget(DiamondWidget cw)
        {
            if (cw != null && cw.gameObject != null)
            {
                diamondList.Add(cw);
            }
        }
        public static void DelDiamondWidget(DiamondWidget cw)
        {
            if (cw != null && diamondList.Contains(cw))
            {
                diamondList.Remove(cw);
            }
        }
        public static DiamondWidget GetCurDiamondWidget()
        {
            if (diamondList == null || diamondList.Count == 0)
            {
                return null;
            }

            List<int> empty = new();
            for (int i = diamondList.Count - 1; i >= 0 ; i--)
            {
                if (diamondList[i].gameObject.activeInHierarchy)
                {
                    return diamondList[i];
                }
                else if (diamondList[i] == null || diamondList[i].gameObject == null)
                {
                    empty.Add(i);
                }
            }

            if (empty.Count>0)
            {
                for (int i = 0; i < empty.Count; i++)
                {
                    diamondList.RemoveAt(empty[i]);
                
                }
            }
            return diamondList.Last();
        }
    }
}