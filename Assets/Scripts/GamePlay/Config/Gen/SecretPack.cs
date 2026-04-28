/************************************************
 * Config class : Activity-SecretPack
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class SecretPack
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 存储名字
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public List<string> showNames { get; set; }

        /// <summary>
        /// 对应分组(photo_other的信息)
        /// </summary>
        public List<string> Group { get; set; }

        /// <summary>
        /// 支付key
        /// </summary>
        public string BuyKey { get; set; }
    }
}