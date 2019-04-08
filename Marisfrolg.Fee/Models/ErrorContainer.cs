using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class ErrorContainer
    {

        public ObjectId Id { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int NumberID { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }
        /// <summary>
        /// 流程id
        /// </summary>
        public string WorkFlowID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Createtime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 处理状态
        /// </summary>
        public int Status { get; set; }
    }
}