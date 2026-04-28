/************************************************
 * Config class : IAP-IAP
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class IAP
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// productId
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// price
        /// </summary>
        public int price { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// AdjustKey
        /// </summary>
        public string AdjustKey { get; set; }

        /// <summary>
        /// 0=可以多次购买的产品 1=一次性 2=有有限有效期的产品
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// des
        /// </summary>
        public string des { get; set; }
    }
}