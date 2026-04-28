/************************************************
 * Config class : BaseGame-BaseGame
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class BaseGame
    {
        /// <summary>
        /// 好评关卡
        /// </summary>
        public int RateLevel { get; set; }

        /// <summary>
        /// 下载消耗
        /// </summary>
        public int DownloadCostType { get; set; }

        /// <summary>
        /// 下载消耗
        /// </summary>
        public int DownloadCostAmount { get; set; }

        /// <summary>
        /// 选关折扣
        /// </summary>
        public int ChoosePercent { get; set; }

        /// <summary>
        /// 选关刷新关卡数
        /// </summary>
        public int ChooseRefresh { get; set; }

        /// <summary>
        /// Loading进度条时间
        /// </summary>
        public int LoadingTime { get; set; }

        /// <summary>
        /// 通关获得金币奖励
        /// </summary>
        public int GameWinCoin { get; set; }

        /// <summary>
        /// 通关看广告奖励翻倍
        /// </summary>
        public int GameWinMul { get; set; }

        /// <summary>
        /// 签到
        /// </summary>
        public int SignIn { get; set; }

        /// <summary>
        /// 商场礼包
        /// </summary>
        public int MallGift { get; set; }

        /// <summary>
        /// 礼包1
        /// </summary>
        public int GiftPack { get; set; }

        /// <summary>
        /// 选秀
        /// </summary>
        public int SuperiseShowPack { get; set; }

        /// <summary>
        /// VIP
        /// </summary>
        public int VIP { get; set; }

        /// <summary>
        /// 通行证
        /// </summary>
        public int Adventure { get; set; }

        /// <summary>
        /// 秘密合集
        /// </summary>
        public int SecretAlbums { get; set; }

        /// <summary>
        /// 转转盘
        /// </summary>
        public int BonusWheel { get; set; }

        /// <summary>
        /// 选秀
        /// </summary>
        public int BeautyDraft { get; set; }

        /// <summary>
        /// 每日挑战
        /// </summary>
        public int Dailychallenge { get; set; }

        /// <summary>
        /// VIP
        /// </summary>
        public int PermanentVIP { get; set; }

        /// <summary>
        /// 大礼包
        /// </summary>
        public int Draftpackage { get; set; }

        /// <summary>
        /// 通行证2
        /// </summary>
        public int BeautyEncounters { get; set; }

        /// <summary>
        /// 选择第2个关卡图花费的金币
        /// </summary>
        public int LevelChooseCost { get; set; }

        /// <summary>
        /// 选择第3个关卡图花费的金币
        /// </summary>
        public int LevelChooseHighCost { get; set; }
    }
}
