using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class CollectionInformation
    {
        public ObjectId Id { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreatorID { get; set; }

        /// <summary>
        /// 开卡城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 开卡卡号
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// 开卡姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 开卡银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 支行
        /// </summary>
        public string SubbranchBank { get; set; }
    }
}