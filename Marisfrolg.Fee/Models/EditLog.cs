using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class EditLog
    {
        public ObjectId Id { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int NumberID { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public EditBillModel OriginalData { get; set; }
        /// <summary>
        /// 修改后的数据
        /// </summary>
        public EditBillModel ModifiedData { get; set; }
    }
}