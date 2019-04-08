using MultiBank.BLL;
using MultiBank.Extention;
using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultiBank.Controllers
{
    public class PayFeeController : WebController
    {

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取费用数据
        /// </summary>
        /// <param name="Time">时间</param>
        /// <param name="Type">类型</param>
        /// <param name="PayCode">付款公司代码</param>
        /// <param name="City">城市</param>
        /// <param name="BillType">单据类型</param>
        /// <param name="PayCompanyCode">付款公司代码（）</param>
        /// <param name="TradeType">对公对私标识</param>
        /// <returns></returns>
        public ActionResult GetIndexData(DateTime Time, string Type, string PayCode = "1", string City = "", string BillType = "", string PayCompanyCode = "", string TradeType = "")
        {
            IGetFeeData _IGetFeeData = new GetFeeData();

            List<ReturnFeeData> Model;

            Model = _IGetFeeData.GetFeeShowData(Time, PayCompanyCode, TradeType);

            Model = Model.Where(c => c.AMOUNTMONEY > 0).ToList();  //只显示大于0的数据

            DataTable dt = new DataTable();

            switch (Type.ToLower())
            {
                case "prepay":

                    List<PrePayData> PrePay = new List<PrePayData>();

                    #region  付款状态属于未推送的才能付款
                    var PreModel = Model.Where(c => c.PAYSTATUS == 0 && string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK) && c.BILLTYPE != "付款通知书").ToList();

                    if (!string.IsNullOrEmpty(PayCode) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(BillType))
                    {
                        var PreModel2 = PreModel.GroupBy(c => new { c.PAYCOMPANYCODE, c.ACCOUNTUSERNAME }).ToList();

                        foreach (var item in PreModel2)
                        {
                            PrePayData pay = new PrePayData();
                            pay.TradeType = "103";
                            pay.PREPAIDBANKNUMBER = MultiBank.Extention.IdHelper.CreateGuid();
                            pay.PAYCOMPANYCODE = item.Key.PAYCOMPANYCODE;
                            pay.OPPPRIVATEFLAG = 1;
                            pay.ACCOUNTUSERNAME = item.Key.ACCOUNTUSERNAME;
                            pay.TotalMoney = item.Sum(c => c.AMOUNTMONEY);

                            pay.DetailedData = item.ToList();
                            pay.CompleteTime = item.Min(c => c.SETTINGTIME);
                            PrePay.Add(pay);
                        }
                    }
                    else if (!string.IsNullOrEmpty(PayCode) && !string.IsNullOrEmpty(City) && string.IsNullOrEmpty(BillType))
                    {
                        var PreModel2 = PreModel.GroupBy(c => new { c.PAYCOMPANYCODE, c.ACCOUNTUSERNAME, c.CITY }).ToList();

                        foreach (var item in PreModel2)
                        {
                            PrePayData pay = new PrePayData();
                            pay.TradeType = "103";
                            pay.PREPAIDBANKNUMBER = MultiBank.Extention.IdHelper.CreateGuid();
                            pay.PAYCOMPANYCODE = item.Key.PAYCOMPANYCODE;
                            pay.OPPPRIVATEFLAG = 1;
                            pay.ACCOUNTUSERNAME = item.Key.ACCOUNTUSERNAME;
                            pay.TotalMoney = item.Sum(c => c.AMOUNTMONEY);
                            pay.CITY = item.Key.CITY;

                            pay.DetailedData = item.ToList();
                            pay.CompleteTime = item.Min(c => c.SETTINGTIME);
                            PrePay.Add(pay);
                        }
                    }
                    else if (!string.IsNullOrEmpty(PayCode) && string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(BillType))
                    {
                        var PreModel2 = PreModel.GroupBy(c => new { c.PAYCOMPANYCODE, c.ACCOUNTUSERNAME, c.BILLTYPE }).ToList();

                        foreach (var item in PreModel2)
                        {
                            PrePayData pay = new PrePayData();
                            pay.TradeType = "103";
                            pay.PREPAIDBANKNUMBER = MultiBank.Extention.IdHelper.CreateGuid();
                            pay.PAYCOMPANYCODE = item.Key.PAYCOMPANYCODE;
                            pay.OPPPRIVATEFLAG = 1;
                            pay.ACCOUNTUSERNAME = item.Key.ACCOUNTUSERNAME;
                            pay.TotalMoney = item.Sum(c => c.AMOUNTMONEY);
                            pay.BILLTYPE = item.Key.BILLTYPE;

                            pay.DetailedData = item.ToList();
                            pay.CompleteTime = item.Min(c => c.SETTINGTIME);
                            PrePay.Add(pay);
                        }
                    }
                    else
                    {
                        var PreModel2 = PreModel.GroupBy(c => new { c.PAYCOMPANYCODE, c.ACCOUNTUSERNAME, c.BILLTYPE, c.CITY }).ToList();

                        foreach (var item in PreModel2)
                        {
                            PrePayData pay = new PrePayData();
                            pay.TradeType = "103";
                            pay.PREPAIDBANKNUMBER = MultiBank.Extention.IdHelper.CreateGuid();
                            pay.PAYCOMPANYCODE = item.Key.PAYCOMPANYCODE;
                            pay.OPPPRIVATEFLAG = 1;
                            pay.ACCOUNTUSERNAME = item.Key.ACCOUNTUSERNAME;
                            pay.TotalMoney = item.Sum(c => c.AMOUNTMONEY);
                            pay.CITY = item.Key.CITY;
                            pay.BILLTYPE = item.Key.BILLTYPE;

                            pay.DetailedData = item.ToList();
                            pay.CompleteTime = item.Min(c => c.SETTINGTIME);
                            PrePay.Add(pay);
                        }
                    }

                    //这里处理付款通知书
                    var PreModel3 = Model.Where(c => c.PAYSTATUS == 0 && string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK) && c.BILLTYPE == "付款通知书").ToList();

                    foreach (var item in PreModel3)
                    {
                        PrePayData pay = new PrePayData();

                        var Istrue = _IGetFeeData.IsBelongInternalTransfer(item.BILLNO);
                        if (Istrue)
                        {
                            //获取付款公司名称
                            var PayName = GetCompanyName(item.PAYCOMPANYCODE);
                            //需付款公司代码一致且付款名称相同才是组织间
                            if (item.COMPANYCODE == item.PAYCOMPANYCODE && PayName == item.ACCOUNTUSERNAME)
                            {
                                pay.TradeType = "1008";
                            }
                            else
                            {
                                pay.TradeType = "104";
                            }
                        }
                        else
                        {
                            pay.TradeType = "103";
                        }

                        pay.PREPAIDBANKNUMBER = MultiBank.Extention.IdHelper.CreateGuid();
                        pay.PAYCOMPANYCODE = item.PAYCOMPANYCODE;
                        pay.OPPPRIVATEFLAG = item.OPPPRIVATEFLAG;
                        pay.ACCOUNTUSERNAME = item.ACCOUNTUSERNAME;
                        pay.TotalMoney = item.AMOUNTMONEY;
                        pay.CITY = item.CITY;
                        pay.BILLTYPE = item.BILLTYPE;
                        pay.CompleteTime = item.SETTINGTIME;
                        pay.DetailedData = new List<ReturnFeeData>() { item };

                        PrePay.Add(pay);
                    }

                    #endregion

                    //删选掉不需要付款的单据
                    PrePay = PrePay.Where(c => c.TotalMoney > 0).ToList();

                    //按照办结日期进行降序排列
                    PrePay = PrePay.OrderByDescending(c => c.CompleteTime).ToList();

                    this.KeepCache(this.CurrentSession.UserName, PrePay);

                    dt = ConvertToDataTable(PrePay, null, "prepay");

                    this.KeepCache(this.CurrentSession.UserName + "V1", dt);

                    break;

                case "original":
                    dt = ConvertToDataTable(null, Model, "original");

                    this.KeepCache(this.CurrentSession.UserName, dt);
                    break;

                case "invalid":

                    var invalid = Model.Where(c => string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK)).ToList();
                    dt = ConvertToDataTable(null, invalid, "original");

                    this.KeepCache(this.CurrentSession.UserName, dt);
                    break;

                case "notpush":

                    var notPush = Model.Where(c => c.PAYSTATUS == 0 && string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK)).ToList();

                    dt = ConvertToDataTable(null, notPush, "notpush");

                    this.KeepCache(this.CurrentSession.UserName, dt);
                    break;

                case "nopay":

                    var noPay = Model.Where(c => c.PAYSTATUS != 0 && c.PAYSTATUS != 2 && !string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK) && c.DEALSTATE == 2).ToList();

                    dt = ConvertToDataTable(null, noPay, "nopay");

                    this.KeepCache(this.CurrentSession.UserName, dt);
                    break;

                case "successpay":

                    var successPay = Model.Where(c => c.PAYSTATUS == 2 && !string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK) && c.DEALSTATE == 2).ToList();

                    dt = ConvertToDataTable(null, successPay, "successpay");

                    this.KeepCache(this.CurrentSession.UserName, dt);
                    break;
                case "fail":

                    var fail = Model.Where(c => c.PAYSTATUS != 0 && !string.IsNullOrEmpty(c.PREPAIDBANKNUMBER) && !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK) && c.DEALSTATE == 3).ToList();

                    dt = ConvertToDataTable(null, fail, "fail");

                    this.KeepCache(this.CurrentSession.UserName, dt);

                    break;
                default:
                    break;
            }
            return this.SuccessData(dt);
        }


        public ActionResult CollectionHabit(string PayCompanyCode, string City, string BillType, string TradeType)
        {

            if (string.IsNullOrEmpty(PayCompanyCode) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(BillType) && string.IsNullOrEmpty(TradeType))
            {
                return this.FailedMsg("无可保存值");
            }

            UserHabit Habit = new UserHabit();
            Habit.PayCompanyCode = PayCompanyCode;
            Habit.City = City;
            Habit.BillType = BillType;
            Habit.TradeType = TradeType;

            IGetFeeData _IGetFeeData = new GetFeeData();
            int result = _IGetFeeData.EidtUserHabit(this.CurrentSession.UserName, JsonHelper.Serialize(Habit));

            if (result > 0)
            {
                return this.SuccessMsg("收藏成功");
            }
            else
            {
                return this.FailedMsg("收藏失败");
            }
        }



        public DataTable ConvertToDataTable(List<PrePayData> PrePay, List<ReturnFeeData> RetrunData, string Type)
        {
            DataTable dt = new DataTable();

            if (PrePay != null && PrePay.Count > 0)
            {
                dt.Columns.Add("全选");
                dt.Columns.Add("预付编号");
                dt.Columns.Add("付款公司代码");

                bool City = !string.IsNullOrEmpty(PrePay.FirstOrDefault().CITY);
                bool BillType = !string.IsNullOrEmpty(PrePay.FirstOrDefault().BILLTYPE);

                if (City)
                {
                    dt.Columns.Add("所处城市");
                }
                if (BillType)
                {
                    dt.Columns.Add("单据类型");
                }
                dt.Columns.Add("收款人");
                dt.Columns.Add("付款总额");


                foreach (var item in PrePay)
                {
                    DataRow row = dt.NewRow();
                    row["预付编号"] = item.PREPAIDBANKNUMBER;
                    row["付款公司代码"] = item.PAYCOMPANYCODE;
                    if (City)
                    {
                        row["所处城市"] = item.CITY;
                    }
                    if (BillType)
                    {
                        row["单据类型"] = item.BILLTYPE;
                    }
                    row["收款人"] = item.ACCOUNTUSERNAME;
                    row["付款总额"] = item.TotalMoney;
                    dt.Rows.Add(row);
                }

            }
            else if (RetrunData != null && RetrunData.Count > 0)
            {

                if (Type == "fail")
                {
                    dt.Columns.Add("全选");
                    Type = "nopay";
                }

                bool original = Type == "original";
                bool notpush = Type == "notpush";
                bool nopay = Type == "nopay";
                bool successpay = Type == "successpay";

                if (nopay || successpay)
                {
                    dt.Columns.Add("付款编号");
                }
                dt.Columns.Add("单号");
                dt.Columns.Add("单据类型");
                dt.Columns.Add("公司代码");
                dt.Columns.Add("付款公司代码");
                dt.Columns.Add("所处城市");
                if (original || notpush)
                {
                    dt.Columns.Add("单据金额");
                }
                dt.Columns.Add("财务付款金额");
                dt.Columns.Add("收款人");
                if (original || notpush)
                {
                    dt.Columns.Add("联行号");
                }
                dt.Columns.Add("支付状态");

                if (nopay || successpay)
                {
                    dt.Columns.Add("提交时间");
                    dt.Columns.Add("提交人");
                }
                if (successpay)
                {
                    dt.Columns.Add("成功付款时间");
                }

                foreach (var item in RetrunData)
                {
                    DataRow row = dt.NewRow();
                    if (nopay || successpay)
                    {
                        row["付款编号"] = item.PREPAIDBANKNUMBER;
                    }
                    row["单号"] = item.BILLNO;
                    row["单据类型"] = item.BILLTYPE;
                    row["公司代码"] = item.COMPANYCODE;
                    row["付款公司代码"] = item.PAYCOMPANYCODE;
                    row["所处城市"] = item.CITY;
                    row["财务付款金额"] = item.AMOUNTMONEY;
                    if (original || notpush)
                    {
                        row["单据金额"] = item.BILLMONEY;
                    }
                    row["收款人"] = item.ACCOUNTUSERNAME;
                    if (original || notpush)
                    {
                        row["联行号"] = item.ACCOUNTSUBBRANCHBANK;
                    }
                    row["支付状态"] = GetPayStatus(item.ACCOUNTSUBBRANCHBANK, item.PAYSTATUS);
                    if (nopay || successpay)
                    {
                        row["提交时间"] = item.SUBMITPAYMENTTIME.ToString("yyyy-MM-dd HH:mm");
                        row["提交人"] = item.CREATOR;
                    }
                    if (successpay)
                    {
                        row["成功付款时间"] = item.REALPAYTIME.ToString("yyyy-MM-dd HH:mm");
                    }
                    dt.Rows.Add(row);
                }
            }

            return dt;
        }


        public string GetPayStatus(string BankCode, decimal PayStatus)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(BankCode))
            {
                result = "无效数据";
            }
            else
            {
                int _PayStatus = Convert.ToInt32(PayStatus);
                switch (_PayStatus)
                {
                    case 0:
                        result = "未推送";
                        break;
                    case 1:
                        result = "未支付";
                        break;
                    case 2:
                        result = "已支付";
                        break;
                    case 3:
                        result = "支付失败";
                        break;
                    case 4:
                        result = "支付中";
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public ActionResult SubmitFeeData(string PrepaidBankNum)
        {
            if (string.IsNullOrEmpty(PrepaidBankNum))
            {
                return this.FailedMsg("数据错误");
            }

            var list = PrepaidBankNum.Split(',').ToList();
            list.Remove("");
            var PreData = this.GetCache<List<PrePayData>>(this.CurrentSession.UserName);
            PreData = PreData.Where(c => list.Contains(c.PREPAIDBANKNUMBER)).ToList();

            //生成插入银行SQL，并且事物化提交
            IGetFeeData _IGetFeeData = new GetFeeData();
            bool result = _IGetFeeData.BitchInserIntoBankSystem(PreData, this.CurrentSession.UserName);

            if (result)
            {
                //跟新支付状态，创建人，预付编号
                bool UpdateStatus = _IGetFeeData.BitchUpdateStatus(PreData, this.CurrentSession.UserName);
                if (UpdateStatus)
                {
                    return this.SuccessData("提交成功");
                }
                else
                {
                    return this.FailedMsg("服务器繁忙");
                }
            }
            else
            {
                return this.FailedMsg("服务器繁忙");
            }
        }


        public ActionResult DownLoad(string tableName)
        {
            DataTable dt;

            if (tableName == "待付清单")
            {
                dt = this.GetCache<DataTable>(CurrentSession.UserName + "V1");
            }
            else
            {
                dt = this.GetCache<DataTable>(CurrentSession.UserName);
            }

            var name = DownLoadFile.ExportExcel(dt, tableName);

            return Content(name);
        }


        public ActionResult GetUserHabit()
        {
            IGetFeeData _IGetFeeData = new GetFeeData();
            var Habit = _IGetFeeData.LookUserHabit(this.CurrentSession.UserName);
            return this.SuccessData(Habit);
        }

        public ActionResult ShowPreNumDetailInfo(string PreNum)
        {
            var list = this.GetCache<List<PrePayData>>(this.CurrentSession.UserName);
            var model = list.Where(c => c.PREPAIDBANKNUMBER == PreNum).FirstOrDefault();

            string msg = string.Empty;

            foreach (var item in model.DetailedData)
            {
                msg += string.Format("{0}实付金额：{1},", item.BILLNO, item.AMOUNTMONEY);
            }

            msg = msg.Remove(msg.Length - 1);

            return this.SuccessMsg(msg);
        }


        /// <summary>
        /// 调用存储过程同步数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SynchData()
        {
            IGetFeeData _IGetFeeData = new GetFeeData();
            bool IsTrue = _IGetFeeData.SynchData();
            if (IsTrue)
            {
                return this.SuccessMsg("同步成功");
            }
            else
            {
                return this.FailedMsg("同步失败");
            }
        }


        public ActionResult RevokeOperation(string PrepaidBankNum)
        {
            if (string.IsNullOrEmpty(PrepaidBankNum))
            {
                return this.FailedMsg("数据错误");
            }

            var list = PrepaidBankNum.Split(',').Distinct().ToList();
            list.Remove("");

            IGetFeeData _IGetFeeData = new GetFeeData();
            bool IsTrue = _IGetFeeData.BitchUpdateStatus(list);
            if (IsTrue)
            {
                return this.SuccessMsg("回退成功");
            }
            else
            {
                return this.FailedMsg("回退失败");
            }
        }


        string GetCompanyName(string Code)
        {
            string ReturnName = string.Empty;
            switch (Code)
            {
                case "1000":
                    ReturnName = "深圳玛丝菲尔时装股份有限公司";
                    break;
                case "2000":
                    ReturnName = "深圳玛丝菲尔素时装有限公司";
                    break;
                case "1300":
                    ReturnName = "深圳玛丝菲尔噢姆服饰有限公司";
                    break;
                case "4000":
                    ReturnName = "惠州市玛丝菲尔时装制造有限公司";
                    break;
                case "2100":
                    ReturnName = "克芮绮亚时装（中国）有限公司";
                    break;
                case "2300":
                    ReturnName = "深圳玛丝菲尔设计研发中心有限公司";
                    break;
                case "2200":
                    ReturnName = "MDC";
                    break;
                default:
                    break;
            }
            return ReturnName;
        }
    }
}
