using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    /// <summary>
    /// 提交到数据库的类
    /// </summary>
    public class NoticeBillModel
    {
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
        public NoticeAttribute SpecialAttribute { get; set; }

        /// <summary>
        /// 发票明细
        /// </summary>
        public List<FeeBillItemModel> BillsItems { get; set; }

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
        /// 缺失发票
        /// </summary>
        public int MissBill { get; set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        public ProviderInfo ProviderInfo { get; set; }

        /// <summary>
        /// 审批岗
        /// </summary>
        public string ApprovalPost { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public string ApprovalTime { get; set; }


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
        /// 回收发票人
        /// </summary>
        public string RecycleNo { get; set; }

        /// <summary>
        /// 是否加急
        /// </summary>
        public int IsUrgent { get; set; }

        /// <summary>
        /// 最后付款日
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastPayDate { get; set; }


        /// <summary>
        /// 店柜Logo
        /// </summary>
        public string ShopLogo { get; set; }
    }

    /// <summary>
    /// 付款通知书的特殊属性
    /// </summary>
    public class NoticeAttribute
    {
        /// <summary>
        /// 活动经费
        /// </summary>
        public int Funds { get; set; }

        /// <summary>
        /// 代理商费用
        /// </summary>
        public int Agent { get; set; }

        public int Check { get; set; }
    }

    /// <summary>
    /// 供应商信息
    /// </summary>
    public class ProviderInfo
    {
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// 开户行账号
        /// </summary>
        public string BankNo { get; set; }
        /// <summary>
        /// 银行代码(Swift)
        /// </summary>
        public string BankCode { get; set; }
        /// <summary>
        /// IBAN码
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string ProviderCode { get; set; }

        /// <summary>
        /// 公司编码
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 输入的内容
        /// </summary>
        public string InputName { get; set; }


        /// <summary>
        /// 支行编码
        /// </summary>
        public string SubbranchBankCode { get; set; }

    }

    /// <summary>
    /// 供外部调用类
    /// </summary>
    public class NoticeBillModelRef : NoticeBillModel
    {
        //额外属性
        public string StringTime { get; set; }
        public string PageName { get; set; }
        //审批系统任务ID
        public string AssignmentID { get; set; }
        //审批时间
        public DateTime ExamineTime { get; set; }

        public DateTime AuditTime { get; set; }

        public double CostTime { get; set; }
    }

    public class Currency
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}