using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class SapUpload
    {
        /// <summary>
        /// 接口体
        /// </summary>
        public DOCUMENTHEADER 结构体DOC { get; set; }

        public List<ACCOUNTGL> 一般性总账 { get; set; }

        public List<ACCOUNTRECEIVABLE> 科目D { get; set; }

        public List<ACCOUNTPAYABLE> 科目K { get; set; }

        public List<CURRENCYAMOUNT> 币种 { get; set; }

        public List<EXTENSION2> 行项目 { get; set; }

    }

    /// <summary>
    /// 结构体
    /// </summary>
    public class DOCUMENTHEADER
    {
        /// <summary>
        /// 参考交易
        /// </summary>
        public string OBJ_TYPE { get; set; }

        /// <summary>
        /// 字段参考关键
        /// </summary>
        public string OBJ_KEY { get; set; }

        /// <summary>
        /// 源凭证的逻辑系统
        /// </summary>
        public string OBJ_SYS { get; set; }

        /// <summary>
        /// 业务事务
        /// </summary>
        public string BUS_ACT { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }

        /// <summary>
        /// 凭证抬头文本
        /// </summary>
        public string HEADER_TXT { get; set; }

        /// <summary>
        /// 公司代码
        /// </summary>
        public string COMP_CODE { get; set; }

        /// <summary>
        /// 业务日期
        /// </summary>
        public DateTime DOC_DATE { get; set; }

        /// <summary>
        /// 凭证中的过帐日期
        /// </summary>
        public DateTime PSTNG_DATE { get; set; }

        /// <summary>
        /// 换算日期
        /// </summary>
        public DateTime TRANS_DATE { get; set; }

        /// <summary>
        /// 会计年度
        /// </summary>
        public int FISC_YEAR { get; set; }

        /// <summary>
        /// 会计期间
        /// </summary>
        public int FIS_PERIOD { get; set; }

        /// <summary>
        /// 凭证类型
        /// </summary>
        public string DOC_TYPE { get; set; }

        /// <summary>
        /// 参考凭证编号
        /// </summary>
        public string REF_DOC_NO { get; set; }

        /// <summary>
        /// 会计凭证编号
        /// </summary>
        public string AC_DOC_NO { get; set; }

        /// <summary>
        /// 取消: 对象码 (AWREF_REV and AWORG_REV)
        /// </summary>
        public string OBJ_KEY_R { get; set; }

        /// <summary>
        /// 冲销原因
        /// </summary>
        public string REASON_REV { get; set; }

        /// <summary>
        /// ACC 界面中的组件
        /// </summary>
        public string COMPO_ACC { get; set; }

        /// <summary>
        /// 参考凭证号(对相关性参看长文本)
        /// </summary>
        public string REF_DOC_NO_LONG { get; set; }

        /// <summary>
        /// 会计原则
        /// </summary>
        public string ACC_PRINCIPLE { get; set; }

        /// <summary>
        /// 标识: 反记帐
        /// </summary>
        public string NEG_POSTNG { get; set; }

        /// <summary>
        /// 发票参考: 对象键值 (AWREF_REB 和 AWORG_REB)
        /// </summary>
        public string OBJ_KEY_INV { get; set; }

        /// <summary>
        /// 出具发票类别
        /// </summary>
        public string BILL_CATEGORY { get; set; }

        /// <summary>
        /// 报税日期
        /// </summary>
        public DateTime VATDATE { get; set; }

        /// <summary>
        /// 发票接收日期
        /// </summary>
        public DateTime INVOICE_REC_DATE { get; set; }

        /// <summary>
        /// ECS 环境
        /// </summary>
        public string ECS_ENV { get; set; }
    }




    public class BaseAccount
    {
        /// <summary>
        /// 项目编号
        /// </summary>
        public int ITEMNO_ACC { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string ITEM_TEXT { get; set; }
        /// <summary>
        /// 分配号
        /// </summary>
        public string ALLOC_NMBR { get; set; }
        /// <summary>
        /// 成本中心
        /// </summary>
        public string COSTCENTER { get; set; }
        /// <summary>
        /// 利润中心
        /// </summary>
        public string PROFIT_CTR { get; set; }
        /// <summary>
        /// 科目类型
        /// </summary>
        public string ACCT_TYPE { get; set; }
    }


    /// <summary>
    /// 总账类型科目填写此表即科目类型为S的科目
    /// </summary>
    public class ACCOUNTGL : BaseAccount
    {
        /// <summary>
        /// 科目号
        /// </summary>
        public string GL_ACCOUNT { get; set; }

        /// <summary>
        /// 采购凭证号
        /// </summary>
        public string PO_NUMBER { get; set; }

        /// <summary>
        /// 采购凭证的项目编号
        /// </summary>
        public string ASSET_NO { get; set; }
    }


    /// <summary>
    /// 科目类型为D的科目请填写此表
    /// </summary>
    public class ACCOUNTRECEIVABLE : BaseAccount
    {
        /// <summary>
        /// 客户编号,客户为纯数字，请补前导0至10位
        /// </summary>
        public string CUSTOMER { get; set; }
    }
    /// <summary>
    /// 科目类型为K的科目请填写此表
    /// </summary>
    public class ACCOUNTPAYABLE : BaseAccount
    {
        /// <summary>
        /// 供应商号,为纯数字，请补前导0至10位
        /// </summary>
        public string VENDOR_NO { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string PO_NUMBER { get; set; }

        /// <summary>
        /// 总账标记
        /// </summary>
        public string UMSKZ { get; set; }

        /// <summary>
        /// 总账科目
        /// </summary>
        public string GL_ACCOUNT { get; set; }
    }


    /// <summary>
    /// 币种(必填)
    /// </summary>
    public class CURRENCYAMOUNT
    {

        /// <summary>
        /// 会计凭证行项目编号
        /// </summary>
        public int ITEMNO_ACC { get; set; }

        /// <summary>
        /// 货币类型和评估视图
        /// </summary>
        public string CURR_TYPE { get; set; }

        /// <summary>
        /// 货币码
        /// </summary>
        public string CURRENCY { get; set; }

        /// <summary>
        /// ISO代码货币
        /// </summary>
        public string CURRENCY_ISO { get; set; }

        /// <summary>
        /// 凭证货币金额
        /// </summary>
        public decimal AMT_DOCCUR { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal EXCH_RATE { get; set; }

        /// <summary>
        /// 间接引用的汇率
        /// </summary>
        public decimal EXCH_RATE_V { get; set; }

        /// <summary>
        /// 用凭证货币表示的税收基础金额
        /// </summary>
        public decimal AMT_BASE { get; set; }

        /// <summary>
        /// 可用来计算现金折扣的符合条件金额(以凭证货币形式)
        /// </summary>
        public string DISC_BASE { get; set; }

        /// <summary>
        /// 以货币类型货币计的现金折扣金额
        /// </summary>
        public string DISC_AMT { get; set; }

        /// <summary>
        /// 凭证货币金额
        /// </summary>
        public string TAX_AMT { get; set; }
    }



    /// <summary>
    /// 行项目
    /// </summary>
    public class EXTENSION2
    {

        /// <summary>
        /// BAPI 表扩展的结构名称
        /// </summary>
        public string STRUCTURE { get; set; }

        /// <summary>
        /// BAPI 扩展参数的数据部分
        /// </summary>
        public string VALUEPART1 { get; set; }

        /// <summary>
        /// BAPI 扩展参数的数据部分
        /// </summary>
        public string VALUEPART2 { get; set; }

        /// <summary>
        /// BAPI 扩展参数的数据部分
        /// </summary>
        public string VALUEPART3 { get; set; }

        /// <summary>
        /// BAPI 扩展参数的数据部分
        /// </summary>
        public string VALUEPART4 { get; set; }
    }



    public class ZFI_EXTEN
    {
        /// <summary>
        /// 项目编号
        /// </summary>
        public int ITEMNO_ACC { get; set; }

        /// <summary>
        /// 原因代码
        /// </summary>
        public string RSTGR { get; set; }
        /// <summary>
        /// 记账码
        /// </summary>
        public string BSCHL { get; set; }
        /// <summary>
        /// 特别总账标记
        /// </summary>
        public string UMSKZ { get; set; }
        /// <summary>
        /// 资产业务类型
        /// </summary>
        public string ANBWA { get; set; }
        /// <summary>
        /// 主资产号
        /// </summary>
        public string ANLN1 { get; set; }
        /// <summary>
        /// 资产次级编号
        /// </summary>
        public string ANLN2 { get; set; }
        /// <summary>
        /// 标识: 反记帐 
        /// </summary>
        public string XNEGP { get; set; }

        /// <summary>
        /// 采购凭证
        /// </summary>
        public string PO_NUMBER { get; set; }

        /// <summary>
        /// 采购相关编号
        /// </summary>
        public int PO_ITEM { get; set; }
    }



    /// <summary>
    /// 返回值
    /// </summary>
    public class RETURN
    {

        /// <summary>
        /// 消息类型: S 成功,E 错误,W 警告,I 信息,A 中断
        /// </summary>
        public string TYPE { get; set; }

        /// <summary>
        /// 消息, 消息类
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 消息, 消息编号
        /// </summary>
        public int NUMBER { get; set; }

        /// <summary>
        /// 消息文本
        /// </summary>
        public string MESSAGE { get; set; }

        /// <summary>
        /// 应用程序日志: 日志号
        /// </summary>
        public string LOG_NO { get; set; }

        /// <summary>
        /// 应用日志：内部邮件序列号
        /// </summary>
        public int LOG_MSG_NO { get; set; }

        /// <summary>
        /// 消息,消息变量
        /// </summary>
        public string MESSAGE_V1 { get; set; }

        /// <summary>
        /// 消息,消息变量
        /// </summary>
        public string MESSAGE_V2 { get; set; }

        /// <summary>
        /// 消息,消息变量
        /// </summary>
        public string MESSAGE_V3 { get; set; }

        /// <summary>
        /// 消息,消息变量
        /// </summary>
        public string MESSAGE_V4 { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string PARAMETER { get; set; }

        /// <summary>
        /// 参数中的行
        /// </summary>
        public int ROW { get; set; }

        /// <summary>
        /// 参数中的字段
        /// </summary>
        public string FIELD { get; set; }

        /// <summary>
        /// 引发消息的逻辑系统
        /// </summary>
        public string SYSTEM { get; set; }
    }


    public class SapRetrun
    {
        /// <summary>
        /// 返回凭证  S成功，E失败
        /// </summary>
        public string TYPE { get; set; }
        /// <summary>
        /// 消息提示
        /// </summary>
        public string MESSAGE { get; set; }
        /// <summary>
        /// 凭证号
        /// </summary>
        public string VOUCHER { get; set; }
    }


    /// <summary>
    /// 提交到SAP类
    /// </summary>
    public class SubmitSapModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 抬头文本
        /// </summary>
        public string Head_txt { get; set; }

        /// <summary>
        /// 公司代码
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 业务日期（办结日期）
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// 办结日期（办结日期的最后一天）
        /// </summary>
        public DateTime ApprovalTime { get; set; }

        /// <summary>
        /// 费用项科目
        /// </summary>
        public List<Sp1> Items { get; set; }

        /// <summary>
        /// 事由（写在行项目里）
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 过账期间
        /// </summary>
        public string PostingPeriod { get; set; }

        /// <summary>
        ///参照
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 往来凭证号
        /// </summary>
        public string CurrentDocumentNumber { get; set; }

        /// <summary>
        /// 单号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 页面名称
        /// </summary>
        public string PageName { get; set; }

    }


    public class Sp1
    {
        /// <summary>
        /// 原始名称
        /// </summary>
        public string OriginalName { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string SapName { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 原因代码(贷方才需要)
        /// </summary>
        public string ReasonCode { get; set; }
        /// <summary>
        /// 商户号，供应商号，科目号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string CoinType { get; set; }

        /// <summary>
        /// 成本中心
        /// </summary>
        public string CosterCenter { get; set; }

        /// <summary>
        /// 利润中心
        /// </summary>
        public string ProfitCenter { get; set; }

        /// <summary>
        /// 记账码
        /// </summary>
        public string ChargeCode { get; set; }

        /// <summary>
        /// 总账标记
        /// </summary>
        public string BSCHL_Sign { get; set; }

        /// <summary>
        /// 科目类型
        /// </summary>
        public string SubjectType { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public string ExchangeRate { get; set; }


        /// <summary>
        /// 凭证类型（根据财务规则来默认）
        /// </summary>
        public string ProofType { get; set; }

        /// <summary>
        /// 备用备注
        /// </summary>
        public string SpareRemark { get; set; }

        /// <summary>
        /// 分配号
        /// </summary>
        public string ALLOC_NMBR { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string PO_NUMBER { get; set; }

        /// <summary>
        /// 隐藏的原始名称
        /// </summary>
        public string HideName { get; set; }

        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetsNum { get; set; }

        /// <summary>
        /// 发票号
        /// </summary>
        public string InvoiceNum { get; set; }
    }


    public class SapStatus
    {
        public int Status { get; set; }

        public string Code { get; set; }

        public string Profit { get; set; }
    }
}