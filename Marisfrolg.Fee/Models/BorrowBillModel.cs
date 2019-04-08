using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Marisfrolg.Fee.Models
{
    //数据提交类
    public class BorrowBillModel
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

        //个人信息(不需要记账品牌)
        public PersonInfo PersonInfo { get; set; }

        /// <summary>
        /// 发票明细
        /// </summary>
        public List<FeeBillItemModel> BillsItems { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillsType { get; set; }

        /// <summary>
        /// 收款信息
        /// </summary>
        public CollectionInfo CollectionInfo { get; set; }

        /// <summary>
        /// 审批状态
        /// </summary>
        public int ApprovalStatus { get; set; }

        /// <summary>
        /// 待还总额
        /// </summary>
        public decimal SurplusMoney { get; set; }

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

        //特殊属性
        public SpecialAttribute SpecialAttribute { get; set; }

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
    //外部调用类
    public class BorrowBillModelRef : BorrowBillModel
    {
        //额外属性
        public string PageName { get; set; }
        public string StringTime { get; set; }
        //审批系统任务ID
        public string AssignmentID { get; set; }

        public DateTime ExamineTime { get; set; }

        public DateTime AuditTime { get; set; }

        public double CostTime { get; set; }
    }


    public class BorrowPersonInfo
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
}