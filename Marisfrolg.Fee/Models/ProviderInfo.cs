using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    /// <summary>
    /// 供应商
    /// </summary>
    public class TempData
    {
        public string label { get; set; }
        public string value { get; set; }
    }

    /// <summary>
    /// 供应商银行信息
    /// </summary>
    public class ProviderInfoShow
    {
        public string BankName { get; set; }
        public string BankNo { get; set; }
        public string IBAN { get; set; }
        public string Swift { get; set; }
    }
}