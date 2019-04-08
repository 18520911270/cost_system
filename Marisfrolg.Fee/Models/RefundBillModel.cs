using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    /// <summary>
    /// 数据库提交类
    /// </summary>
    public class RefundBillModel
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
        /// 还款类型(费用还款或者现金还款)
        /// </summary>
        public string RefundType { get; set; }

        /// <summary>
        /// 借款单号
        /// </summary>
        public string BorrowBillNo { get; set; }

        /// <summary>
        /// 冲销账号（用于总部现金冲账的账号）
        /// </summary>
        public string OffsetBillNo { get; set; }

        /// <summary>
        /// 实际还款金额
        /// </summary>
        public decimal RealRefundMoney { get; set; }

        /// <summary>
        /// 0员工还给总部;1总部清账给员工
        /// </summary>
        public int Flag { get; set; }

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
        /// 是否还款超过欠款
        /// </summary>
        public int OutDebt { get; set; }
        /// <summary>
        /// 实际上还款额度
        /// </summary>
        public decimal DebtMoney { get; set; }

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
    /// 外部调用类
    /// </summary>
    public class RefundBillModelRef : RefundBillModel
    {
        //额外属性
        //审批系统任务ID
        public string AssignmentID { get; set; }
        public string PageName { get; set; }
        public string StringTime { get; set; }
        public string ModelString { get; set; }

        /// <summary>
        /// 单据提交类型
        /// </summary>
        public CommitType CommitType { get; set; }

        /// <summary>
        /// 借款总金额
        /// </summary>
        public decimal TotalMoney { get; set; }
        /// <summary>
        /// 剩余偿还金额
        /// </summary>
        public decimal SurplusMoney { get; set; }

        public DateTime ExamineTime { get; set; }

        public DateTime AuditTime { get; set; }

        public double CostTime { get; set; }
    }
}