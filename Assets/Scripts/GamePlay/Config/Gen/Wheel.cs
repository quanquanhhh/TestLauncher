/************************************************
 * Config class : Activity-Wheel
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Wheel
    {
        /// <summary>
        /// RewardIds
        /// </summary>
        public List<int> RewardIds { get; set; }

        /// <summary>
        /// RewardCount
        /// </summary>
        public List<int> RewardCount { get; set; }

        /// <summary>
        /// Weight
        /// </summary>
        public List<int> Weight { get; set; }

        /// <summary>
        /// OpenLevel
        /// </summary>
        public int OpenLevel { get; set; }

        /// <summary>
        /// BIgwin需要的次数
        /// </summary>
        public int BigwinCount { get; set; }

        /// <summary>
        /// 大奖奖励id
        /// </summary>
        public List<int> BigwinRewards { get; set; }

        /// <summary>
        /// 大奖奖励数量
        /// </summary>
        public List<int> BigwinRewardsAmount { get; set; }

        /// <summary>
        /// 每天刷新的次数
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 每天的免费次数
        /// </summary>
        public int FreeCount { get; set; }

        /// <summary>
        /// 购买需要的钻石数量
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// 购买给的抽奖次数
        /// </summary>
        public int BuyCount { get; set; }

        /// <summary>
        /// HasGuide
        /// </summary>
        public bool HasGuide { get; set; }

        /// <summary>
        /// 购买key
        /// </summary>
        public string BuyKey { get; set; }
    }
}
