using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foundation
{
    public interface IEvent
    {
    } 
     
    public struct AdjustBack : IEvent
    {
        public int result; 
        public AdjustBack(int result )
        {
            this.result = result; 
        }
    }
    public struct ShowSystemTips : IEvent
    {
        public string text;
        public bool isDebug;
        public ShowSystemTips(string text, bool isDebug = false)
        {
            this.text = text;
            this.isDebug = isDebug;
        }
    }
    public struct RVPlayFinished : IEvent
    {
        public Action action; 
        public RVPlayFinished(Action a )
        {
            action = a; 
        }
    }
    
    public struct IVPlayFinished : IEvent
    {
        public Action action; 
        public IVPlayFinished(Action a )
        {
            action = a; 
        }
    }

    public struct AddPhoto : IEvent
    {
        public int photofrom;
        public int count;
        public  List<string>  photoname;
        public Action endAction;
        public bool showpop;
        public int photoStatus;
        
        public AddPhoto(int photofrom, int count, List<string> photoname = null, Action endAction = null, bool showpop = true, int photoStatus = 1)
        {
            this.photofrom = photofrom;
            this.count = count;
            this.photoname = photoname;
            this.endAction = endAction;
            this.showpop = showpop;
            this.photoStatus = photoStatus;
        }
    }
    public struct AddItem : IEvent
    {
        public int itemType; 
        public int itemCount;
        public Vector3 startPos; 
        public AddItem( int itemType, int itemCount, Vector3 startPos=default(Vector3))
        {
            this.itemType = itemType;
            this.itemCount = itemCount;
            this.startPos = startPos; 
        }
    }
    public struct SubItem : IEvent
    {
        public int itemType; 
        public int itemCount;
        public SubItem( int itemType, int itemCount )
        {
            this.itemType = itemType;
            this.itemCount = itemCount;
        }
    }

    public struct ItemCountChangeShow : IEvent
    {
        public int itemType;
        public Vector3 flystartPos;
        public bool isFly;
        public ItemCountChangeShow( int itemType, bool isFly = false,  Vector3 flystartPos = default(Vector3) )
        {
            this.itemType = itemType; 
            this.isFly = isFly;
            this.flystartPos = flystartPos;
        }
    }

    public struct SelectDailyChallenge : IEvent
    {
        public int index;
        public SelectDailyChallenge(int index = 0)
        {
            this.index = index;
        }
    }
    
    public struct UpdateThirdAreaEvent : IEvent { }

    public struct ClickTileEvent : IEvent
    {
        public UIWidget girlTile;

        public ClickTileEvent(UIWidget girlTile)
        {
            this.girlTile = girlTile;
        }
    }
    public struct UpdateToolText : IEvent { }
    public struct ReplayGame : IEvent { }
    public struct PhotoInShopCountChange : IEvent { }

    public struct ReviveGame : IEvent
    {
        public bool isAd;

        public ReviveGame(bool isAd = false)
        {
            this.isAd = isAd;
        }
    }

    public struct LevelFinished : IEvent
    {
        public bool isWin;

        public LevelFinished(bool isWin)
        {
            this.isWin =  isWin;
        }
    }

    // public struct TriggerRemoveProp : IEvent
    // {
    //     public bool hasCosume;
    //
    //     public TriggerRemoveProp(bool hasCosume = true)
    //     {
    //         this.hasCosume = hasCosume;
    //     }
    // }

    public struct ChangeTopUIOrder : IEvent
    {
        public bool isTop;
        public bool hasCoinWidget;
        public bool hasDiamondWidget;
        public bool hasSettingWidget;
        public ChangeTopUIOrder(bool isTop, bool hasCoinWidget = true, bool hasDiamondWidget = true, bool hasSettingWidget = true)
        {
            this.isTop = isTop;
            this.hasCoinWidget = hasCoinWidget;
            this.hasDiamondWidget =  hasDiamondWidget;
            this.hasSettingWidget = hasSettingWidget;
        }
    } 

    public struct ShowTips : IEvent
    {
        public string text;
        public bool isdebug; 
        public ShowTips(string  text, bool isdebug = false)
        {
            this.text = text;
            this.isdebug = isdebug;
        }
    }

    public struct VIPStateChange : IEvent { }
    public struct BuyLimitedTimeGift : IEvent { }
    public struct AdsStateChange : IEvent { }
    public struct UpdateBeautyDraftOrder : IEvent { }
    public struct BuySecret : IEvent { }
    public struct ChangeBackGround : IEvent { }
    public struct UIMainIconMoveHide : IEvent { }
    public struct UIMainIconMoveShow : IEvent { }
    public struct CheckDownloadByLevel : IEvent { }
    public struct UpdateActivityIcon : IEvent { }
    public struct UpdatePropCount : IEvent { }
    public struct ChangeUserType : IEvent { }
    public struct GameUIFinished : IEvent { }
    public struct FocusLeft : IEvent { }
    public struct FocusEnter : IEvent { }
    public struct LanguageChange : IEvent { }

    public struct RestoreBuff : IEvent
    {
        public string productid;
        public long expire;

        public RestoreBuff(string id, long expire)
        {
            productid = id;
            this.expire = expire;
        }
    }
     
}