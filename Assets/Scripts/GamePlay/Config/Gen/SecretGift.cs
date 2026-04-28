/************************************************
 * Config class : Activity-SecretGift
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class SecretGift
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 支付key
        /// </summary>
        public string BuyKey { get; set; }

        /// <summary>
        /// DiscountKey
        /// </summary>
        public string DiscountKey { get; set; }

        /// <summary>
        /// 对应分组(photo_other的信息)
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 开启等级
        /// </summary>
        public int lockLevel { get; set; }

        /// <summary>
        /// HasGuide
        /// </summary>
        public bool HasGuide { get; set; }

        /// <summary>
        /// OpenLevel
        /// </summary>
        public int OpenLevel { get; set; }

        /// <summary>
        /// ShowDiscountTimes
        /// </summary>
        public int ShowDiscountTimes { get; set; }
    }
}
