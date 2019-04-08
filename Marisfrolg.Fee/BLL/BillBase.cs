using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Marisfrolg.Fee.BLL
{
    public enum BillType
    {
        费用草稿单 = 0,
        费用报销单 = 1,
        付款通知书 = 2,
        借款单 = 3,
        还款单 = 4,
        费用报销还款单 = 5,
    }

    public class BillBase
    {
        //单据编码格式： 起头字母+短日期+流水号四位 比如 FY201605270001
        public readonly static Dictionary<int, string> BillNoHead = new Dictionary<int, string>()
        {
            {0,"CG"},//除了单据起头字母，其他部分为数字
            {1,"FB"},
            {2,"FT"},
            {3,"JS"},
            {4,"HK"},
            {5,"FB"},
        };


        private BillType _BillType;
        public BillBase(BillType pBillType)
        {
            _BillType = pBillType;
        }

        public BillType BillType
        {
            get
            {
                return this._BillType;
            }
        }

        public virtual string GetMaxBillNo()
        {
            return "";
        }

        public string GenerateMaxBillNo()
        {
            string oldMax = GetMaxBillNo();

            if (oldMax == null || !oldMax.StartsWith(BillNoHead[(int)this.BillType]))
            {
                return string.Format("{0}{1}{2}", BillNoHead[(int)this.BillType], DateTime.Now.ToString("yyyyMMdd").Substring(2, 6), "0001");
            }

            string newMax = string.Format("{0}{1}", BillNoHead[(int)this.BillType], Int64.Parse(oldMax.Replace(BillNoHead[(int)this.BillType], "")) + 1);

            return newMax;
        }

    }
}