/************************************************
 * Config class : BaseGame-VIP
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class VIP
    {
        /// <summary>
        /// 描述(和多语言对应，目前只有1-6)
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// 永久vip权益
        /// </summary>
        public bool lifevip { get; set; }

        /// <summary>
        /// 限时vip权益
        /// </summary>
        public bool normalvip { get; set; }

        /// <summary>
        /// Vip一天内跳关
        /// </summary>
        public int skipLevel { get; set; }

        /// <summary>
        /// 免费使用道具
        /// </summary>
        public int VipFreePropCount { get; set; }

        /// <summary>
        /// 购买的key
        /// </summary>
        public string buykey { get; set; }
    }
}
