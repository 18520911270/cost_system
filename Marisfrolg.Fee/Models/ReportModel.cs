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
    /// 报表呈现到界面动态结构
    /// </summary>
    public class ReportModel
    {
        public List<string> Columns { get; set; }
        public List<object[]> Rows { get; set; }
    }


    public class FinanceReportData
    {
        public string BillNO { get; set; }
        public string DepartmentName { get; set; }
        public string FeeName { get; set; }
        public string CreateTime { get; set; }
        public string OperationTime { get; set; }
        public string TotalMoney { get; set; }
        public string Brand { get; set; }
    }

    public class NewAccountInfo
    {
        public decimal No { get; set; }
        public string Name { get; set; }
        public string ReasonCode { get; set; }
        public string ApprovalType { get; set; }
        public string Permission { get; set; }
    }

    /// <summary>
    /// 汇总报表
    /// </summary>
    public class ReportCollect
    {
        public string BillNo { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
        public string Shop { get; set; }
        public string ShopCode { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<FeeBillItemModel> Items { get; set; }
        public List<string> Brand { get; set; }
        public DateTime ApprovalTime { get; set; }
        public string BillType { get; set; }
        public string CostCenter { get; set; }
        public string Owner { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 付款通知书的收款人
        /// </summary>
        public string ProviderName { get; set; }
        public decimal TotalMoney { get; set; }
        /// <summary>
        /// 特殊属性
        /// </summary>
        public SpecialAttribute SpecialProperty { get; set; }
        /// <summary>
        /// 回收发票
        /// </summary>
        public int MissBill { get; set; }

        public int IsHeadOffice { get; set; }

        /// <summary>
        /// 冲借款金额
        /// </summary>
        public decimal EntryMoney { get; set; }

        public int ApprovalStatus { get; set; }

        public string ApprovalPost { get; set; }

        /// <summary>
        /// 收款信息
        /// </summary>
        public CollectionInfo gather { get; set; }

        /// <summary>
        /// 付款公司代码
        /// </summary>
        public string PaymentCompanyCode { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 剩余金额
        /// </summary>
        public decimal SurplusMoney { get; set; }

        /// <summary>
        /// 是否超出还款
        /// </summary>
        public int OutDebt { get; set; }

        /// <summary>
        /// 借款单号
        /// </summary>
        public string BorrowBillNo { get; set; }
    }

    public class NewPERMISSION
    {
        public decimal ID { get; set; }
        public string NAME { get; set; }
        public string CONTROLLER { get; set; }
        public string DESCRIPTION { get; set; }
        public decimal PID { get; set; }
        public string ACTION { get; set; }
        public string APPTYPE { get; set; }
        public string ISMENU { get; set; }
        public int SORT { get; set; }
    }


    public class Person
    {
        public string No { get; set; }
        public string Name { get; set; }
    }


    public class FindPerson
    {
        public string No { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 是否微信通知
        /// </summary>
        public string IsNotice { get; set; }
        /// <summary>
        /// 预留字段
        /// </summary>
        public string standby { get; set; }
    }

    public class TempTime
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public DateTime Time { get; set; }
    }

    public class SAPDepartment
    {
        public string NAME { get; set; }
        public Decimal ID { get; set; }
    }

    public class AccountInfo
    {
        public string No { get; set; }
        public string Name { get; set; }
        public string ReasonCode { get; set; }
        public string ApprovalType { get; set; }
        public string Permission { get; set; }
        /// <summary>
        /// 1考核，0不考核
        /// </summary>
        public short? Market { get; set; }

        public decimal? SortId { get; set; }
    }

    public class ObjectList
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
    }

    public class LoginUserIdentity
    {
        public decimal CODE { get; set; }
        public string NAME { get; set; }
    }

    public class CosterCenterExtend
    {
        public string CosterCenter { get; set; }
        public string Description { get; set; }
        public string CompanyCode { get; set; }
    }

    public class InfoList
    {
        public int currentPage { get; set; }
        public double totalPages { get; set; }
        public int pageSize { get; set; }
        /// <summary>
        /// 任意参数源
        /// </summary>
        public object info { get; set; }

    }

    public class DepartmentList
    {
        public string Department { get; set; }
        public string Shop { get; set; }
        public string ShopCode { get; set; }

        public List<List<FeeBillItemModel>> items { get; set; }
    }


    public class RetrunBillCount
    {
        public string BillType { get; set; }
        public string BillCount { get; set; }
    }


    public class SpecialData
    {
        public string VALUE { get; set; }
        public string FYTYPE { get; set; }
        public string FEEINFO { get; set; }
    }

    public class SpecialArray
    {
        public string VALUE { get; set; }
        public string PARAMETERONE { get; set; }
        public string PARAMETERTWO { get; set; }
        public string SIDDEPARTMENTCODE { get; set; }
    }

    public class SAPShowData
    {
        public string BillNo { get; set; }
        public string Department { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Brand { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public string BillType { get; set; }
        public string CostCenter { get; set; }
        public string Owner { get; set; }
        public string Remark { get; set; }
        public decimal? TotalMoney { get; set; }
        public int? DepartmentID { get; set; }
        /// <summary>
        /// SAP凭证
        /// </summary>
        public string SAPProof { get; set; }

        public DateTime? SapUploadTime { get; set; }

        public string SapCreator { get; set; }


        public string CompanyCode { get; set; }

        /// <summary>
        /// 活动经费
        /// </summary>
        public short? Funds { get; set; }
        /// <summary>
        /// 商场账扣
        /// </summary>
        public short? MarketDebt { get; set; }
        /// <summary>
        /// 银行账扣
        /// </summary>
        public short? BankDebt { get; set; }
        /// <summary>
        /// 代理商费用
        /// </summary>
        public short? Agent { get; set; }
        /// <summary>
        /// 押金
        /// </summary>
        public short? Cash { get; set; }
    }


    public class DepartCosterCenter
    {
        public decimal DEPARTMENTID { get; set; }
        public string NAME { get; set; }
        public string COMPANYCODE { get; set; }
        public string COSTERCENTER { get; set; }
    }

    public class PublicClass
    {
        public string c1 { get; set; }
        public string c2 { get; set; }
        public string c3 { get; set; }
        public string c4 { get; set; }
        public string c5 { get; set; }
        public string c6 { get; set; }
        public string c7 { get; set; }
        public string c8 { get; set; }
        public string c9 { get; set; }
        public string c10 { get; set; }
        public string c11 { get; set; }
        public string c12 { get; set; }
        public string c13 { get; set; }
    }


    public class ExternalModel
    {
        public string BusinessDate { get; set; }
        public string CreateTime { get; set; }
        public string ShopCode { get; set; }
        public string BillNo { get; set; }
        public decimal Money { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string DepartmentID { get; set; }
    }

    public class MongoModel
    {
        public DateTime BusinessDate { get; set; }
        public string BillNo { get; set; }
        public List<FeeBillItemModel> Items { get; set; }
        public string ShopCode { get; set; }
        public string DepartmentCode { get; set; }
        public DateTime CreateTime { get; set; }
    }

    //汇款明细表
    public class RemittanceDetails
    {
        public string BillNo { get; set; }
        public string Department { get; set; }
        public string Shop { get; set; }
        public string PayCode { get; set; }
        public string BillType { get; set; }
        public string City { get; set; }
        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal BillMoney { get; set; }
        /// <summary>
        /// 冲借款金额
        /// </summary>
        public decimal RepaymentMoney { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        public decimal PaymentMoney { get; set; }

        /// <summary>
        /// 收款人信息
        /// </summary>
        public CollectionInfo Gather { get; set; }

    }

    /// <summary>
    /// 出纳汇款
    /// </summary>
    public class FinanceExcle
    {
        public string TableName { get; set; }
        /// <summary>
        /// 汇总sheet1
        /// </summary>
        public DataTable sheet1 { get; set; }
        /// <summary>
        /// 明细sheet2
        /// </summary>
        public DataTable sheet2 { get; set; }
    }


    /// <summary>
    /// 费用系统接口对接多银行数据
    /// </summary>
    public class FeeSystemData
    {
        /// <summary>
        /// 单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 门店编码
        /// </summary>
        public string ShopCode { get; set; }
        /// <summary>
        /// 所处城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 支付公司代码
        /// </summary>
        public string PayCompanyCode { get; set; }
        /// <summary>
        /// 所处公司代码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal BillMoney { get; set; }
        /// <summary>
        /// 冲借款金额
        /// </summary>
        public decimal LoanMoney { get; set; }
        /// <summary>
        /// 财务付款总金额
        /// </summary>
        public decimal AmountMoney { get; set; }
        /// <summary>
        /// 办结时间
        /// </summary>
        public string SettingTime { get; set; }
        /// <summary>
        /// 对公标识（2对公，1对私默认对公）
        /// </summary>
        public int OppprivateFlag { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 收款人账号
        /// </summary>
        public string AccountNo { get; set; }
        /// <summary>
        /// 收款人姓名
        /// </summary>
        public string AccountUserName { get; set; }
        /// <summary>
        /// 收款支行
        /// </summary>
        public string AccountSubbranchBank { get; set; }

        /// <summary>
        /// 支付状态  1,未支付，2，已支付，3，支付失败，4，支付中
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 删除状态    0未删除，1删除
        /// </summary>
        public int IsDel { get; set; }
    }
}