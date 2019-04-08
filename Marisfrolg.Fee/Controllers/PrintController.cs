using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Marisfrolg.Fee.Models;
using Marisfrolg.Fee.BLL;

namespace Marisfrolg.Fee.Controllers
{
    public class PrintController : SecurityController
    {
        //
        // GET: /Print/

        public ActionResult PrintList()
        {
            return View();
        }

        /// <summary>
        /// 获取打印列表
        /// </summary>
        /// <param name="Type">单据类型</param>
        /// <param name="Time">时间</param>
        /// <param name="Page">页码数</param>
        /// <returns></returns>
        public string GetPrintList(int Type, int Time, int Page = 1)
        {
            try
            {
                InfoList list = new InfoList();
                list.pageSize = 10;
                int totalNumber = 0;
                object ModelList = null;
                //时间区间控制
                DateTime startTime = new DateTime(1999, 1, 1);
                DateTime endTime = new DateTime(2999, 1, 1);
                //创建日期
                switch (Time)
                {
                    //全部
                    case 1:
                        break;
                    //当天
                    case 2:
                        startTime = DateTime.Now.Date;
                        break;
                    //本周
                    case 3:
                        startTime = DateTime.Now.Date.AddDays(1 - Convert.ToInt32(DateTime.Now.Date.DayOfWeek.ToString("d")));
                        endTime = DateTime.Now.Date.AddDays(1 - Convert.ToInt32(DateTime.Now.Date.DayOfWeek.ToString("d"))).AddDays(7);
                        break;
                    //本月
                    case 4:
                        startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
                        break;
                    //上月
                    case 5:
                        startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        break;
                    default:
                        break;
                }
                //根据不同的单据类型调用不同的方法取值
                switch (Type)
                {
                    //费用报销单
                    case 1:
                        List<Models.FeeBillModel> FeeBill = new Marisfrolg.Fee.BLL.FeeBill().GetBillForPrint(startTime, endTime, Page, list.pageSize, out totalNumber);
                        List<Models.FeeBillModelRef> FeeBillList = new List<Models.FeeBillModelRef>();
                        foreach (var item in FeeBill)
                        {
                            Models.FeeBillModelRef FeeModel = new Models.FeeBillModelRef();
                            FeeModel = item.MapTo<Models.FeeBillModel, Models.FeeBillModelRef>();
                            FeeModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            FeeBillList.Add(FeeModel);
                        }
                        ModelList = FeeBillList;
                        break;
                    //付款通知书 
                    case 2:
                        List<Models.NoticeBillModel> NoticeBill = new Marisfrolg.Fee.BLL.NoticeBill().GetBillForPrint(startTime, endTime, Page, list.pageSize, out totalNumber);
                        List<Models.NoticeBillModelRef> NoticeBillList = new List<Models.NoticeBillModelRef>();
                        foreach (var item in NoticeBill)
                        {
                            Models.NoticeBillModelRef NoticeModel = new Models.NoticeBillModelRef();
                            NoticeModel = item.MapTo<Models.NoticeBillModel, Models.NoticeBillModelRef>();
                            NoticeModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            NoticeBillList.Add(NoticeModel);
                        }
                        ModelList = NoticeBillList;
                        break;
                    //借款单（所有的借款记录，不论是否还清）
                    case 3:
                        List<Models.BorrowBillModel> BorrowBill = new Marisfrolg.Fee.BLL.BorrowBill().GetBillForPrint(startTime, endTime, Page, list.pageSize, out totalNumber);
                        List<Models.BorrowBillModelRef> BorrowBillList = new List<Models.BorrowBillModelRef>();
                        foreach (var item in BorrowBill)
                        {
                            Models.BorrowBillModelRef BorrowModel = new Models.BorrowBillModelRef();
                            BorrowModel = item.MapTo<Models.BorrowBillModel, Models.BorrowBillModelRef>();
                            BorrowModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            BorrowBillList.Add(BorrowModel);
                        }
                        ModelList = BorrowBillList;
                        break;
                    //还款单(加载还款记录)
                    case 4:
                        List<Models.RefundBillModel> RefundBill = new Marisfrolg.Fee.BLL.RefundBill().GetBillForPrint(startTime, endTime, Page, list.pageSize, out totalNumber);
                        List<Models.RefundBillModelRef> RefundBillList = new List<Models.RefundBillModelRef>();
                        foreach (var item in RefundBill)
                        {
                            Models.RefundBillModelRef RefundModel = new Models.RefundBillModelRef();
                            RefundModel = item.MapTo<Models.RefundBillModel, Models.RefundBillModelRef>();
                            RefundModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            RefundModel.TotalMoney = RefundModel.RealRefundMoney;
                            RefundBillList.Add(RefundModel);
                        }
                        ModelList = RefundBillList;
                        break;
                    default:
                        break;
                }

                double value = (double)totalNumber / list.pageSize;
                list.totalPages = Math.Ceiling(value);
                list.currentPage = Page;
                list.info = ModelList;
                return Public.JsonSerializeHelper.SerializeToJson(list);
            }
            catch (Exception ex)
            {
                Logger.Write("获取打印列表数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }


        //[HttpGet]
        //[LoginAuthorize]
        //public string BillDetails(string NO)
        //{
        //    string vipData=null;
        //    //PrintInfo vipData = new PrintInfo();
        //    //vipData = DbContext.EMPLOYEE.Where(c => c.NO == NO).Select(x => new PrintInfo
        //    //{
        //    //    Code = x.NO,
        //    //    Name = x.NAME,
        //    //    Mobile = x.CHANGER,
        //    //    BillsType = x.UCSTAR_DEPTID,
        //    //    lastPayTime = x.CREATEDATE,
        //    //    SumPrice = x.CREATOR
        //    //}).FirstOrDefault();
        //    if (vipData == null)
        //        return "未查到会员记录";

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("<table>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>卡号：</td><td>&nbsp;" + vipData.Code + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>名称：</td><td>&nbsp;" + vipData.Name + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>电话：</td><td>&nbsp;" + vipData.Mobile + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>办卡日期：</td><td>&nbsp;" + vipData.BillsType + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>生日：</td><td>&nbsp;" + vipData.lastPayTime + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td style='width:75px;text-align:right;'>开卡人：</td><td>&nbsp;" + vipData.SumPrice + ")</td>");
        //    sb.Append("<tr>");

        //    sb.Append("<tr>");
        //    sb.Append("<td  style='width:75px;text-align:right;' valign='top'>地址：</td><td style='word-wrap:break-word;'>&nbsp;" + vipData.lastPayTime + "</td>");
        //    sb.Append("<tr>");

        //    sb.Append("</table>");

        //    return GetPower("卡号(" + vipData.Code + ")", sb.ToString());
        //}

        public string GetPower(string title, string context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div>");
            sb.Append("<div style='padding: 0px 5px'><h5>" + title + "</h5></div>");
            sb.Append("<div style='border-top: #cccccc 1px solid; overflow: hidden; height: 1p'></div>");
            sb.Append("<div style='padding: 8px 8px;overflow:auto; height:265px;'>");
            sb.Append(context);
            sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        ///  根据单号获取实体对象
        /// </summary>
        /// <param name="BillNo">单号</param>
        /// <param name="type">单据类型</param>
        /// <returns></returns>
        public string GetDataFromBillNo(string BillNo, int Type)
        {
            try
            {
                switch (Type)
                {
                    //费用报销单
                    case 1:
                        Models.FeeBillModel FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetBillModel(BillNo);
                        Models.FeeBillModelRef FeeRefModel = new Models.FeeBillModelRef();
                        FeeRefModel = FeeModel.MapTo<Models.FeeBillModel, Models.FeeBillModelRef>();
                        FeeRefModel.StringTime = FeeRefModel.CreateTime.ToString("yyyy-MM-dd");
                        FeeRefModel.PageName = "FeeBill";
                        return FeeRefModel == null ? "" : Public.JsonSerializeHelper.SerializeToJson(FeeRefModel);
                    //付款通知书
                    case 2:
                        Models.NoticeBillModel NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetBillModel(BillNo);
                        Models.NoticeBillModelRef NoticeRefModel = new Models.NoticeBillModelRef();
                        NoticeRefModel = NoticeModel.MapTo<Models.NoticeBillModel, Models.NoticeBillModelRef>();
                        NoticeRefModel.StringTime = NoticeModel.CreateTime.ToString("yyyy-MM-dd");
                        NoticeRefModel.PageName = "Notice";
                        return NoticeRefModel == null ? "" : Public.JsonSerializeHelper.SerializeToJson(NoticeRefModel);
                    //借款单
                    case 3:
                        Models.BorrowBillModel BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetBillModel(BillNo);
                        Models.BorrowBillModelRef BorrowRefModel = new Models.BorrowBillModelRef();
                        BorrowRefModel = BorrowModel.MapTo<Models.BorrowBillModel, Models.BorrowBillModelRef>();
                        BorrowRefModel.StringTime = BorrowModel.CreateTime.ToString("yyyy-MM-dd");
                        BorrowRefModel.PageName = "BorrowBill";
                        return BorrowRefModel == null ? "" : Public.JsonSerializeHelper.SerializeToJson(BorrowRefModel);
                    //还款单
                    case 4:
                        Models.RefundBillModel RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetBillModel(BillNo);
                        Models.RefundBillModelRef RefundRefModel = new Models.RefundBillModelRef();
                        RefundRefModel = RefundModel.MapTo<Models.RefundBillModel, Models.RefundBillModelRef>();
                        RefundRefModel.StringTime = RefundModel.CreateTime.ToString("yyyy-MM-dd");
                        RefundRefModel.PageName = "RefundBill";
                        RefundRefModel.TotalMoney = RefundModel.RealRefundMoney;
                        return RefundRefModel == null ? "" : Public.JsonSerializeHelper.SerializeToJson(RefundRefModel);
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.Write("获取列表数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }


        /// <summary>
        /// 绘出流程
        /// </summary>
        /// <param name="BillNo"></param>
        /// <param name="Type"></param>
        /// <param name="ShowType"></param>
        /// <param name="WorkFlowId"></param>
        /// <returns></returns>
        public string GetWorkFlowList(string BillNo, int Type, string ShowType, string WorkFlowId)
        {
            string error = string.Empty;
            try
            {
                //原始数据
                var FlowList = new Marisfrolg.Fee.BLL.FeeBill().GetWorkFlowListPlus(BillNo, Type);

                ////驳回数据
                List<FlowInstance> RejectList = new List<FlowInstance>();

                //驳回流程
                if (!string.IsNullOrEmpty(WorkFlowId))
                {
                    var Index = FlowList.FindIndex(c => c.NodeState == "1");
                    if (Index > 0)
                    {
                        for (int i = 0; i < Index; i++)
                        {
                            RejectList.Add(FlowList[i]);
                        }
                    }
                    //只让总经办和出纳有权限驳回
                    RejectList = RejectList.Where(c => c.Description.Contains("财务") || c.Description.Contains("总经办")).ToList();
                }

                if (ShowType.ToUpper() == "SMALL")
                {
                    FlowList = DesignProcess(FlowList);
                }

                //数据加工
                var c1 = FlowList.Where(c => c.Description == "审批岗" || c.Description == "审批岗1" || c.Description == "审批岗2").ToList();
                if (c1.Count > 0)
                {
                    var name = GetPostName(BillNo, Type);
                    foreach (var item in c1)
                    {
                        item.Description = item.Description.Replace("审批岗", name);
                    }
                }

                string TempNo = "";
                foreach (var item in FlowList)
                {
                    for (int i = 0; i < item.PersonName.Count; i++)
                    {
                        TempNo = item.PersonName[i];
                        item.PersonName[i] = AjaxGetName(TempNo);
                    }
                }
                error = "0";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, data = FlowList, RejectList = RejectList });
            }
            catch (Exception)
            {
                error = "1";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error });
        }



        private string GetPostName(string BillNo, int Type)
        {
            string FlowType = string.Empty;
            switch (Type)
            {
                case 1:
                    var f1 = new FeeBill().GetBillModel(BillNo);
                    FlowType = f1.BillsType;
                    break;
                case 2:
                    var f2 = new NoticeBill().GetBillModel(BillNo);
                    FlowType = f2.BillsType;
                    break;
                case 3:
                    var f3 = new BorrowBill().GetBillModel(BillNo);
                    FlowType = f3.BillsType;
                    break;
                case 4:
                    var f4 = new RefundBill().GetBillModel(BillNo);
                    FlowType = f4.BillsType;
                    break;
                default:
                    break;
            }
            var name = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == FlowType).Select(x => x.NAME).FirstOrDefault();
            return name;
        }

        /// <summary>
        /// 市场发展中心的个性化显示
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        private List<FlowInstance> DesignProcess(List<FlowInstance> Model)
        {
            //外层来组合结果集
            var FirstModel = Model.Where(c => c.Description.Contains("岗1") || c.Description.Contains("岗2")).ToList();
            if (FirstModel.Count > 0)
            {
                string Remark = string.Empty;
                List<int> KeyWord = new List<int>();
                List<string> StringTime = new List<string>();
                var SP = Model.Where(c => c.Description.Contains("岗") && !c.Description.Contains("品牌")).FirstOrDefault();

                var Person = SP.PersonName;

                //从结果集中删除
                foreach (var item in FirstModel)
                {
                    Model.Remove(item);
                    if (!string.IsNullOrEmpty(item.ActiveID))
                    {
                        Remark = item.Remark;
                        KeyWord = item.KeyWord;
                        StringTime = item.StringTime;
                    }
                }

                if (SP != null)
                {
                    SP.Description = "市场发展中心";
                    var t1 = FirstModel.Where(c => c.KeyWord.Contains(0)).FirstOrDefault();
                    var t2 = FirstModel.Where(c => c.KeyWord.Count == 0).FirstOrDefault();//存在未审批的情况
                    var t3 = SP.KeyWord.Contains(0);  //判断是否为第一步拒绝

                    if (t1 != null || t2 == null || t3)
                    {
                        SP.PersonName = Person;
                        SP.Remark = KeyWord.Count == 0 ? SP.Remark : Remark;
                        SP.KeyWord = KeyWord.Count == 0 ? SP.KeyWord : KeyWord;
                        SP.StringTime = StringTime.Count == 0 ? SP.StringTime : StringTime;
                    }
                    else
                    {
                        SP.PersonName = new List<string>();
                        SP.Remark = "";
                        SP.KeyWord = new List<int>();
                        SP.StringTime = new List<string>();
                    }
                }
            }
            return Model;
        }


        /// <summary>
        /// 删除单据
        /// </summary>
        /// <param name="BillNo">单号</param>
        /// <param name="Type">类型</param>
        /// <returns></returns>
        public string DelectMyBill(string BillNo, int Type)
        {
            string result = "";
            switch (Type)
            {
                case 1:
                    result = new FeeBill().DelectFeeBill(BillNo);
                    break;
                case 2:
                    result = new NoticeBill().DelectNotcieBill(BillNo);
                    break;
                case 3:
                    result = new BorrowBill().DelectBorrowBill(BillNo);
                    break;
                case 4:
                    result = new RefundBill().DelectRefundBill(BillNo);
                    break;
                default:
                    break;
            }
            return result;
        }


        /// <summary>
        /// 获取费用项
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <returns></returns>
        public string GetSmallSort(int IsHeadOffice)
        {
            int num = 0;
            if (IsHeadOffice == 1)
            {
                num = 2;
            }
            else
            {
                num = 0;
            }

            var model = DbContext.FEE_ACCOUNT.Where(c => c.HIDE != num).Select(x => new LoginUserIdentity()
            {
                NAME = x.NAME,
                CODE = x.ID
            }).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        public string GetBillTypeFrmBillNo(string BillNo)
        {
            var model = new FeeBill().GetBillModel(BillNo);
            if (model != null)
            {
                return "FeeBill";
            }
            var model1 = new NoticeBill().GetBillModel(BillNo);
            if (model != null)
            {
                return "NoticeBill";
            }
            var model2 = new BorrowBill().GetBillModel(BillNo);
            if (model2 != null)
            {
                return "BorrowBill";
            }
            var model3 = new RefundBill().GetBillModel(BillNo);
            if (model3 != null)
            {
                return "RefundBill";
            }
            return "";
        }
    }
}
