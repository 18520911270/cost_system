using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class ReportTableData
    {
        /// <summary>
        /// 报表类型
        /// </summary>
        public string ReportType { get; set; }
        /// <summary>
        /// 部门集合
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 费用大类
        /// </summary>
        public string AccountType { get; set; }
        /// <summary>
        /// 费用小类
        /// </summary>
        public string AccountInfo { get; set; }
        /// <summary>
        /// 创建开始时间
        /// </summary>
        public string CreateBeginTime { get; set; }
        /// <summary>
        /// 创建结束时间
        /// </summary>
        public string CreateEndTime { get; set; }
        /// <summary>
        /// 办结开始时间
        /// </summary>
        public string OverBeginTime { get; set; }
        /// <summary>
        /// 办结结束时间
        /// </summary>
        public string OverEndTime { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 权限等级
        /// </summary>
        public string PermissonGrade { get; set; }
        /// <summary>
        /// 片区门店权限
        /// </summary>
        public string AreaPermissonList { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string BillStatus { get; set; }
        /// <summary>
        /// 公司代码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 片区和门店编码
        /// </summary>
        public string AreaCodeAndShopCode { get; set; }
        /// <summary>
        /// 是否导出
        /// </summary>
        public string IsExport { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 收款人
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 最小金额
        /// </summary>
        public decimal MinMoney { get; set; }
        /// <summary>
        /// 最大金额
        /// </summary>
        public decimal MaxMoney { get; set; }
        /// <summary>
        /// 特殊属性
        /// </summary>
        public string SpecialProperty { get; set; }
    }
}