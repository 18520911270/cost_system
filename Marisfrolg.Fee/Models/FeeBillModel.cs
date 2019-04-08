using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class FeeBillItemModel
    {
        public string rowid { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public decimal money { get; set; }
        public string reason_code { get; set; }
        public string taxcode { get; set; }
        public decimal taxmoney { get; set; }
        /// <summary>
        /// 发票号
        /// </summary>
        public string InvoiceNum { get; set; }
        /// <summary>
        /// 资产号
        /// </summary>
        public string AssetsNum { get; set; }
        /// <summary>
        /// 合同号
        /// </summary>
        public string ContractNum { get; set; }

        /// <summary>
        /// 发文号
        /// </summary>
        public string DispatchNum { get; set; }

        /// <summary>
        /// 转入银行
        /// </summary>
        public string shiftBank { get; set; }

        /// <summary>
        /// 转出银行
        /// </summary>
        public string TurnoutBank { get; set; }

        /// <summary>
        /// 是否考核
        /// </summary>
        public int IsMarket { get; set; }
    }

    public class PhotoModel
    {
        public string filename { get; set; }
        //显示
        public string url { get; set; }
        //原始url
        public string OriginalUrl { get; set; }
    }
    public class PersonInfo
    {
        public int IsHeadOffice { get; set; }
        public string Company { get; set; }
        public string CompanyCode { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
        public string Shop { get; set; }
        public string ShopCode { get; set; }
        public string CostCenter { get; set; }
        public List<string> Brand { get; set; }
    }

    public class SpecialAttribute
    {
        /// <summary>
        /// 活动经费
        /// </summary>
        public int Funds { get; set; }
        /// <summary>
        /// 商场账扣
        /// </summary>
        public int MarketDebt { get; set; }
        /// <summary>
        /// 银行账扣
        /// </summary>
        public int BankDebt { get; set; }
        /// <summary>
        /// 代理商费用
        /// </summary>
        public int Agent { get; set; }
        /// <summary>
        /// 考核
        /// </summary>
        public int Check { get; set; }
        /// <summary>
        /// 押金
        /// </summary>
        public int Cash { get; set; }
    }

    /// <summary>
    /// 收款信息
    /// </summary>
    public class CollectionInfo
    {
        /// <summary>
        /// 开卡城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string Bank { get; set; }
        /// <summary>
        /// 开卡卡号
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 开卡姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 支行
        /// </summary>
        public string SubbranchBank { get; set; }

        /// <summary>
        /// 支行CODE
        /// </summary>
        public string SubbranchBankCode { get; set; }

    }
    /// <summary>
    /// 提交到mongdb的类
    /// </summary>
    [Serializable]
    public class FeeBillModel
    {
        /*
            Creator: employeeInfo.EmployeeNo,
            DepartmentID: employeeInfo.DepartmentID,
            DepartmentName: employeeInfo.DepartmentName,
            TransactionDate: $("#busDate").val(),
            Remark: $(".weui_textarea").val(),
            Items: []
        */
        //单据自动生成唯一码
        public ObjectId Id { get; set; }
        //单号
        public string BillNo { get; set; }
        //创建人
        public string Creator { get; set; }
        //报销人工号
        public string WorkNumber { get; set; }
        //业务发生者
        public string Owner { get; set; }
        //创建日期
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        //业务发生者所属部门
        public string DepartmentID { get; set; }
        //业务发生者成本中心
        public string COST_ACCOUNT { get; set; }
        //业务发生者部门名称
        public string DepartmentName { get; set; }
        //业务发生日期
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionDate { get; set; }
        //事由
        public string Remark { get; set; }
        //流程状态
        public int Status { get; set; } //0表示创建，1表示审批完成
        //总金额
        public decimal TotalMoney { get; set; }
        //单据明细项目
        public List<FeeBillItemModel> Items { get; set; }
        //单据照片证据
        public List<PhotoModel> Photos { get; set; }
        //审批系统实例ID
        public string WorkFlowID { get; set; }

        //个人信息
        public PersonInfo PersonInfo { get; set; }
        //特殊属性
        public SpecialAttribute SpecialAttribute { get; set; }

        /// <summary>
        /// 发票明细
        /// </summary>
        public List<FeeBillItemModel> BillsItems { get; set; }
        /// <summary>
        /// 收款信息
        /// </summary>
        public CollectionInfo CollectionInfo { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillsType { get; set; }

        /// <summary>
        /// 审批状态
        /// </summary>
        public int ApprovalStatus { get; set; }

        /// <summary>
        /// 货币类型
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// 审批岗
        /// </summary>
        public string ApprovalPost { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public string ApprovalTime { get; set; }

        /// <summary>
        /// 结算日期
        /// </summary>
        public CountTime CountTime { get; set; }

        /// <summary>
        /// 已经打印的次数
        /// </summary>
        public int PrintedCount { get; set; }


        /// <summary>
        /// 复制次数
        /// </summary>
        public int CopyCount { get; set; }

        /// <summary>
        /// 搁置人员
        /// </summary>
        public string ShelveNo { get; set; }

        /// <summary>
        /// 回收站
        /// </summary>
        public int RecycleBin { get; set; }

        /// <summary>
        /// 付款公司代码
        /// </summary>
        public string PaymentCompanyCode { get; set; }

        /// <summary>
        /// 流程实体
        /// </summary>
        public List<PostDescription> PostString { get; set; }

        /// <summary>
        /// 店柜Logo
        /// </summary>
        public string ShopLogo { get; set; }
    }
    /// <summary>
    /// 供外部调用的类
    /// </summary>
    public class FeeBillModelRef : FeeBillModel
    {
        //额外属性
        //审批系统任务ID
        public string AssignmentID { get; set; }
        public string PageName { get; set; }
        public string StringTime { get; set; }
        public string ModelString { get; set; }
        /// <summary>
        /// 删除的草稿箱No
        /// </summary>
        public string DeleteDraftNo { get; set; }

        /// <summary>
        /// 单据提交类型
        /// </summary>
        public CommitType CommitType { get; set; }

        /// <summary>
        /// 还款类型
        /// </summary>
        public string RefundType { get; set; }

        /// <summary>
        /// 待还总额
        /// </summary>
        public decimal SurplusMoney { get; set; }

        /// <summary>
        /// 借款单号
        /// </summary>
        public string BorrowBillNo { get; set; }

        public DateTime ExamineTime { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ProviderName { get; set; }


        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ProviderCode { get; set; }

        /// <summary>
        /// 是否复制(0不复制，1为复制)
        /// </summary>
        public int IsCopy { get; set; }


        /// <summary>
        /// 是否还款超过欠款
        /// </summary>
        public int OutDebt { get; set; }

        /// <summary>
        /// 缺失发票
        /// </summary>
        public int MissBill { get; set; }


        public DateTime AuditTime { get; set; }

        public double CostTime { get; set; }


        /// <summary>
        /// 是否加急
        /// </summary>
        public int IsUrgent { get; set; }

        /// <summary>
        /// 最后付款日
        /// </summary>
        public DateTime LastPayDate { get; set; }
    }

    /// <summary>
    /// 提交的单据类型
    /// </summary>
    public enum CommitType
    {
        费用报销单 = 1,
        付款通知书 = 2,
        借款单 = 3,
        还款单 = 4
    }

    /// <summary>
    /// 结算日期
    /// </summary>
    public class CountTime
    {
        /// <summary>
        /// 结算开始日期
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CountStartTime { get; set; }
        /// <summary>
        /// 结算结束日期
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CountEndTime { get; set; }
    }


    /// <summary>
    /// 岗位
    /// </summary>
    public class PostDescription
    {
        public string JobNumber { get; set; }
        public string Post { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Time { get; set; }
    }


    /// <summary>
    /// 费用报表基础数据类
    /// </summary>
    public class FEE_Report
    {
        public FEE_Report()
        {

        }

        public FEE_Report(string Brand)
        {
            this.Brand = Brand;
        }



        public string Brand { get; set; }

        public List<FeeBillItemModel> Items
        {
            get
            {
                if (items == null)
                {
                    items = new List<FeeBillItemModel>();
                }

                return items;
            }

            set
            {
                items = value;
            }
        }

        private List<FeeBillItemModel> items;

    }

    /// <summary>
    /// 费用报表视图展示类
    /// </summary>
    public class FEE_Report_Show
    {
        public string Brand { get; set; }

        public string FeeName { get; set; }

        /// <summary>
        /// 本期指标
        /// </summary>
        public string Current_Index { get; set; }

        /// <summary>
        /// 本期实际
        /// </summary>
        public string Current_Actual { get; set; }

        /// <summary>
        /// 超指标
        /// </summary>
        public string Super_Index { get; set; }

        /// <summary>
        /// 考核费用/万
        /// </summary>
        public decimal Assess_Cost { get; set; }

        /// <summary>
        /// 考核业绩
        /// </summary>
        public decimal Assess_Achieve { get; set; }

        /// <summary>
        /// 品牌直接考核费用
        /// </summary>
        public decimal Brand_Assess_Cost { get; set; }

        /// <summary>
        /// 分摊费用
        /// </summary>
        public decimal Share_Cost { get; set; }

    }

    /// <summary>
    /// 临时数据表
    /// </summary>
    public class FEE_TempTable
    {

        public decimal MF { get; set; }
        public decimal SU { get; set; }
        public decimal AUM { get; set; }

        public decimal DEPARTMENT_ID { get; set; }

        /// <summary>
        /// 薪资
        /// </summary>
        public decimal SALARY { get; set; }

        /// <summary>
        /// 公司代码
        /// </summary>
        public string BRAND { get; set; }
    }

    /// <summary>
    /// 正式数据表
    /// </summary>
    public class FEE_OracleTable
    {
        public decimal MF { get; set; }
        public decimal SU { get; set; }
        public decimal AUM { get; set; }

        public List<string> Depart_ID
        {
            get
            {
                if (depart_ID == null)
                {
                    depart_ID = new List<string>();
                }
                return depart_ID;
            }

            set
            {
                depart_ID = value;
            }
        }

        private List<string> depart_ID;

        public decimal GetBrandNorm(string Brand)
        {
            decimal value = 0;
            switch (Brand)
            {
                case "MA":
                    value = this.MF;
                    break;
                case "SU":
                    value = this.SU;
                    break;
                case "AUM":
                    value = this.AUM;
                    break;
                default:
                    break;
            }
            return value;
        }
    }


    public class FEE_FixedData
    {

        public List<string> Brand
        {
            get
            {
                return new List<string>() { "MA", "SU", "AUM" };
            }
        }

        public List<string> FeeName
        {
            get
            {
                return new List<string>() { "合计", "人力成本", "快递费", "季末退货费", "日常费用", "差旅费", "办事处费用", "营销费用", "货车费用" };
            }
        }

        public List<string> GetFeeItems(string FeeName)
        {
            List<string> Items = new List<string>();
            FEE_FixedData NewData = new FEE_FixedData();
            switch (FeeName)
            {
                case "合计":
                    foreach (var item in NewData.FeeName)
                    {
                        if (item != "合计")
                        {
                            Items.AddRange(GetItems(item));
                        }
                    }
                    break;
                default:
                    Items = GetItems(FeeName);
                    break;
            }

            return Items;
        }

        private List<string> GetItems(string FeeName)
        {
            List<string> Items;
            switch (FeeName)
            {
                case "人力成本":
                    Items = new List<string>() { "福利费", "招聘费", "补贴", "劳动关系费", "月薪工资" };
                    break;
                case "快递费":
                    Items = new List<string>() { "快递费" };
                    break;
                case "季末退货费":
                    Items = new List<string>() { "季末退货费" };
                    break;
                case "日常费用":
                    Items = new List<string>() { "商场招待费", "商场培训费", "大理石养护费", "电话通讯费", "电话费", "网络使用费", "网络费", "调拨费", "办公费", "低值易耗品及摊销", "饮水费", "清洁费", "修衣改衣费", "日常维修费", "日常维修及材料费", "培训场地费", "销售物资" };
                    break;
                case "差旅费":
                    Items = new List<string>() { "市内交通费", "差旅费-长途交通费", "差旅费-住宿费" };
                    break;
                case "办事处费用":
                    Items = new List<string>() { "房屋租赁费", "预付房屋租赁费", "办事处装修费", "物业管理费", "预付物业管理费", "水电费", "办事处押金", "办事处租赁押金", "办事处房租租赁税" };
                    break;
                case "营销费用":
                    Items = new List<string>() { "营销及VIP回馈费用", "赠品", "内销差额" };
                    break;
                case "货车费用":
                    Items = new List<string>() { "货车加油费", "货车停车费", "货车维修费", "货车年检费", "货车保险费" };
                    break;
                default:
                    Items = new List<string>();
                    break;
            }

            return Items;
        }

    }

    public class SalesModel
    {
        public string BRAND { get; set; }

        public decimal MONEY { get; set; }
    }
}