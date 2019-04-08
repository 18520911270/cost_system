using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class EditBillModel
    {
        /// <summary>
        /// 单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillsType { get; set; }
        /// <summary>
        /// 品牌列表
        /// </summary>
        public List<string> Brand { get; set; }
        /// <summary>
        /// 成本中心
        /// </summary>
        public string CostCenter { get; set; }
        /// <summary>
        /// 特殊属性
        /// </summary>
        public SpecialAttribute SpecialAttribute { get; set; }
        /// <summary>
        /// 货币信息
        /// </summary>
        public Currency Currency { get; set; }
        /// <summary>
        /// 费用项
        /// </summary>
        public List<FeeBillItemModel> Items { get; set; }
        /// <summary>
        /// 打印项
        /// </summary>
        public List<FeeBillItemModel> BillsItems { get; set; }

        /// <summary>
        /// 发票缺失
        /// </summary>
        public int MissBill { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<PhotoModel> Photos { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        public string DPID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DPName { get; set; }
        /// <summary>
        /// 门店code
        /// </summary>
        public string ShopCode { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        public string E_CompanyCode { get; set; }


        public string E_Company { get; set; }
    }

    public class AddPhotoModel
    {
        public List<PhotoModel> Photos { get; set; }

        public string BillNo { get; set; }

        public string Type { get; set; }
    }
}