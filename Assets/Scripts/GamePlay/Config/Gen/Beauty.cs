/************************************************
 * Config class : Activity-Beauty
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Beauty
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 购买方式 0 iap 1  金币 2 钻石 3 广告
        /// </summary>
        public int buyType { get; set; }

        /// <summary>
        /// iap拉起的支付key
        /// </summary>
        public string buykey { get; set; }

        /// <summary>
        /// 金币/钻石支付的价格
        /// </summary>
        public int price { get; set; }

        /// <summary>
        /// 开启等级
        /// </summary>
        public int OpenLevel { get; set; }

        /// <summary>
        /// 跳转弹窗条件
        /// </summary>
        public int Condition { get; set; }

        /// <summary>
        /// 满足条件后，连续几关谈一次跳转弹窗
        /// </summary>
        public int ShowTipsIntervalLevel { get; set; }

        /// <summary>
        /// 连续几次不点击跳转，则不在提示
        /// </summary>
        public int NoShowTipsCount { get; set; }

        /// <summary>
        /// 是否强引导
        /// </summary>
        public bool HasGuide { get; set; }
    }
}
