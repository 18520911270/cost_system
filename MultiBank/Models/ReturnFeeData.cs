using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.Models
{
    /// <summary>
    /// 详细的返回数据
    /// </summary>
    public class ReturnFeeData
    {
        public string PREPAIDBANKNUMBER { get; set; }
        public string BILLNO { get; set; }
        public string BILLTYPE { get; set; }
        public string DEPARTMENTNAME { get; set; }
        public string SHOPNAME { get; set; }
        public string CITY { get; set; }
        public string PAYCOMPANYCODE { get; set; }
        public string COMPANYCODE { get; set; }
        public decimal BILLMONEY { get; set; }
        public decimal LOANMONEY { get; set; }
        public decimal AMOUNTMONEY { get; set; }
        public decimal PAYSTATUS { get; set; }
        public string ACCOUNTUSERNAME { get; set; }
        public string ACCOUNTSUBBRANCHBANK { get; set; }
        public string ACCOUNTNO { get; set; }
        public decimal OPPPRIVATEFLAG { get; set; }
        public string CURRENCY { get; set; }
        public DateTime REALPAYTIME { get; set; }
        public DateTime SUBMITPAYMENTTIME { get; set; }
        public string CREATOR { get; set; }
        public decimal DEALSTATE { get; set; }
        public DateTime SETTINGTIME { get; set; }
    }


    /// <summary>
    /// 预付表
    /// </summary>
    public class PrePayData
    {
        public string PREPAIDBANKNUMBER { get; set; }
        public string PAYCOMPANYCODE { get; set; }
        public string BILLTYPE { get; set; }
        public string CITY { get; set; }
        public string ACCOUNTUSERNAME { get; set; }
        public decimal OPPPRIVATEFLAG { get; set; }
        public decimal TotalMoney { get; set; }
        /// <summary>
        /// 交易类型
        /// </summary>
        public string TradeType { get; set; }

        /// <summary>
        /// 办结日期
        /// </summary>
        public DateTime CompleteTime { get; set; }

        public List<ReturnFeeData> DetailedData { get; set; }
    }
}