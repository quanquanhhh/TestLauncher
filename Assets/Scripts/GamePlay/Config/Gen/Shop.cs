/************************************************
 * Config class : Shop-Shop
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Shop
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 购买的物品
        /// </summary>
        public List<int> buyItem { get; set; }

        /// <summary>
        /// 购买物品数量
        /// </summary>
        public List<int> buyCount { get; set; }

        /// <summary>
        /// 购买类型0广告1金币2钻石3内购
        /// </summary>
        public int buyType { get; set; }

        /// <summary>
        /// 价格，内购的可以不写
        /// </summary>
        public int price { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        public List<string> otherInfo { get; set; }

        /// <summary>
        /// 是否展示
        /// </summary>
        public bool isShow { get; set; }

        /// <summary>
        /// 展示类型（方块2，长条1）
        /// </summary>
        public int ShowType { get; set; }

        /// <summary>
        /// 支付拉起的key
        /// </summary>
        public string buyKey { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        public string group { get; set; }
    }
}
