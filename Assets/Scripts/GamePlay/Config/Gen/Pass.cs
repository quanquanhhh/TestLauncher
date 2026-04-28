/************************************************
 * Config class : Activity-Pass
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Pass
    {
        /// <summary>
        /// id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 免费
        /// </summary>
        public bool isFree { get; set; }

        /// <summary>
        /// 支付key
        /// </summary>
        public string BuyKey { get; set; }

        /// <summary>
        /// OpenLevel
        /// </summary>
        public int OpenLevel { get; set; }

        /// <summary>
        /// HasGuide
        /// </summary>
        public bool HasGuide { get; set; }
    }
}
