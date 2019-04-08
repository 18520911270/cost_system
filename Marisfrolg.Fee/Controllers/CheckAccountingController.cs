using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkFlowEngine;
using Marisfrolg.Fee.Extention;

namespace Marisfrolg.Fee.Controllers
{




    public class CheckAccountingController : SecurityController
    {


        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取我审批过的单据
        /// </summary>
        /// <param name="Type">单据类型</param>
        /// <param name="Time">时间</param>
        /// <param name="EmployeeNo">工号</param>
        /// <returns></returns>
        public object GetApprovalData(int Type, int Time, string EmployeeNo, string departmentID, string StartTime, string EndTime)
        {
            try
            {
                var TempTime1 = DateTime.Now.Date;
                var TempTime2 = DateTime.Now.Date;
                if (Time == 6)
                {
                    TempTime1 = StartTime == "" ? new DateTime(1999, 1, 1) : Convert.ToDateTime(StartTime);
                    TempTime2 = EndTime == "" ? new DateTime(2999, 1, 1) : Convert.ToDateTime(EndTime);
                }
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                List<WorkFlowInstance> dic = new List<WorkFlowInstance>();
                string objectID = proxy.GetWorkFlowListByUserId(EmployeeNo, TempTime1, TempTime2, ref dic);

                if (!string.IsNullOrEmpty(objectID) && dic.Count > 0)
                {
                    List<TempTime> Mylist = new List<TempTime>();
                    foreach (var item in dic)
                    {
                        TempTime Tmp = new TempTime();
                        Tmp.Id = item._id;
                        if (item.Assignments == null)
                        {
                            item.Assignments = new List<Assignment>();
                        }
                        var model = item.Assignments.Where(c => (c.Keyword == 12002 || c.Keyword == 12003 || c.Keyword == 12005) && c.UserCode == EmployeeNo).ToList();
                        if (model != null && model.Count > 0)
                        {
                            Tmp.Time = model.Select(c => c.updatetime).LastOrDefault();
                            Mylist.Add(Tmp);
                        }
                    }
                    List<string> WorkFlowList = new List<string>();
                    foreach (var item in Mylist)
                    {
                        WorkFlowList.Add(item.Id.ToString());
                    }

                    switch (Type)
                    {
                        //获取所有单据（不分类型）
                        case 0:
                            List<FeeBillModelRef> AllModel = new List<FeeBillModelRef>();
                            var Temp1 = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            var Temp2 = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            var Temp3 = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            var Temp4 = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            foreach (var item1 in Temp1)
                            {
                                FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item1.BillNo, PageName = "费用报销单", Creator = item1.Creator, Owner = item1.Owner, TotalMoney = item1.TotalMoney, StringTime = item1.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item1.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item1.PersonInfo.DepartmentCode, Department = item1.PersonInfo.Department } };
                                var Id = MongoDB.Bson.ObjectId.Parse(item1.WorkFlowID);
                                TempModel.ExamineTime = Mylist.Where(c => c.Id == Id).Select(x => x.Time).FirstOrDefault();
                                AllModel.Add(TempModel);
                            }
                            foreach (var item2 in Temp2)
                            {
                                FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item2.BillNo, PageName = "付款通知书", Creator = item2.Creator, Owner = item2.Owner, TotalMoney = item2.TotalMoney, StringTime = item2.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item2.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item2.PersonInfo.DepartmentCode, Department = item2.PersonInfo.Department } };
                                var Id = MongoDB.Bson.ObjectId.Parse(item2.WorkFlowID);
                                TempModel.ExamineTime = Mylist.Where(c => c.Id == Id).Select(x => x.Time).FirstOrDefault();
                                AllModel.Add(TempModel);
                            }
                            foreach (var item3 in Temp3)
                            {
                                FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item3.BillNo, PageName = "借款单", Creator = item3.Creator, Owner = item3.Owner, TotalMoney = item3.TotalMoney, StringTime = item3.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item3.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item3.PersonInfo.DepartmentCode, Department = item3.PersonInfo.Department } };
                                var Id = MongoDB.Bson.ObjectId.Parse(item3.WorkFlowID);
                                TempModel.ExamineTime = Mylist.Where(c => c.Id == Id).Select(x => x.Time).FirstOrDefault();
                                AllModel.Add(TempModel);
                            }
                            foreach (var item4 in Temp4)
                            {
                                FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item4.BillNo, PageName = item4.RefundType.ToUpper() == "CASH" ? "现金还款" : "费用单还款", Creator = item4.Creator, Owner = item4.Owner, TotalMoney = item4.RealRefundMoney, StringTime = item4.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item4.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item4.PersonInfo.DepartmentCode, Department = item4.PersonInfo.Department } };
                                var Id = MongoDB.Bson.ObjectId.Parse(item4.WorkFlowID);
                                TempModel.ExamineTime = Mylist.Where(c => c.Id == Id).Select(x => x.Time).FirstOrDefault();
                                AllModel.Add(TempModel);
                            }
                            if (departmentID == "0")
                            {

                                AllModel = AllModel.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2).ToList();
                            }
                            else
                            {
                                AllModel = AllModel.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2 && c.PersonInfo.DepartmentCode == departmentID).ToList();
                            }

                            return AllModel.OrderByDescending(c => c.ExamineTime).ToList();
                        //未审批的费用报销单
                        case 1:
                            var FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.FeeBillModelRef> RefList = new List<Models.FeeBillModelRef>();
                            foreach (var item in FeeModel)
                            {
                                Marisfrolg.Fee.Models.FeeBillModelRef RefModel = new Models.FeeBillModelRef();
                                RefModel = item.MapTo<FeeBillModel, FeeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "费用报销单";
                                RefModel.ExamineTime = Mylist.Where(c => c.Id == id).Select(x => x.Time).FirstOrDefault();
                                RefList.Add(RefModel);
                            }
                            if (departmentID == "0")
                            {
                                RefList = RefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2).ToList();
                            }
                            else
                            {
                                RefList = RefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2 && c.PersonInfo.DepartmentCode == departmentID).ToList();
                            }
                            return RefList.OrderByDescending(c => c.ExamineTime).ToList();
                        //未审批的付款通知书  
                        case 2:
                            var NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.NoticeBillModelRef> NoticeRefList = new List<Models.NoticeBillModelRef>();
                            foreach (var item in NoticeModel)
                            {
                                Marisfrolg.Fee.Models.NoticeBillModelRef RefModel = new Models.NoticeBillModelRef();
                                RefModel = item.MapTo<NoticeBillModel, NoticeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "付款通知书";
                                RefModel.ExamineTime = Mylist.Where(c => c.Id == id).Select(x => x.Time).FirstOrDefault();
                                NoticeRefList.Add(RefModel);
                            }
                            if (departmentID == "0")
                            {
                                NoticeRefList = NoticeRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2).ToList();
                            }
                            else
                            {
                                NoticeRefList = NoticeRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2 && c.PersonInfo.DepartmentCode == departmentID).ToList();
                            }
                            return NoticeRefList.OrderByDescending(c => c.ExamineTime).ToList();
                        //未审批的借款单  
                        case 3:
                            var BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.BorrowBillModelRef> BorrowRefList = new List<Models.BorrowBillModelRef>();
                            foreach (var item in BorrowModel)
                            {
                                Marisfrolg.Fee.Models.BorrowBillModelRef RefModel = new Models.BorrowBillModelRef();
                                RefModel = item.MapTo<BorrowBillModel, BorrowBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "借款单";
                                RefModel.ExamineTime = Mylist.Where(c => c.Id == id).Select(x => x.Time).FirstOrDefault();
                                BorrowRefList.Add(RefModel);
                            }
                            if (departmentID == "0")
                            {
                                BorrowRefList = BorrowRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2).ToList();
                            }
                            else
                            {
                                BorrowRefList = BorrowRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2 && c.PersonInfo.DepartmentCode == departmentID).ToList();
                            }
                            return BorrowRefList.OrderByDescending(c => c.ExamineTime).ToList();
                        //未审批的借款单  
                        case 4:
                            var RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.RefundBillModelRef> RefundRefList = new List<Models.RefundBillModelRef>();
                            foreach (var item in RefundModel)
                            {
                                Marisfrolg.Fee.Models.RefundBillModelRef RefModel = new Models.RefundBillModelRef();
                                RefModel = item.MapTo<RefundBillModel, RefundBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.TotalMoney = RefModel.RealRefundMoney;
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = RefModel.RefundType.ToUpper() == "CASH" ? "现金还款" : "费用单还款";
                                RefModel.ExamineTime = Mylist.Where(c => c.Id == id).Select(x => x.Time).FirstOrDefault();
                                RefundRefList.Add(RefModel);
                            }
                            if (departmentID == "0")
                            {
                                RefundRefList = RefundRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2).ToList();
                            }
                            else
                            {
                                RefundRefList = RefundRefList.Where(c => c.ExamineTime.Date >= TempTime1 && c.ExamineTime.Date <= TempTime2 && c.PersonInfo.DepartmentCode == departmentID).ToList();
                            }
                            return RefundRefList.OrderBy(c => c.ExamineTime).ToList();
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("获取我审批过的单据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return null;
        }

        public ReportModel TransformData(object data, int Type)
        {
            ReportModel result = new ReportModel();
            result.Rows = new List<object[]>();
            result.Columns = new List<string>() { "所在片区", "单号", "单据类型", "制单人", "业务人", "发生金额", "创建日期", "审批日期" };
            switch (Type)
            {
                //全部单据
                case 0:
                    var temp0 = data as List<FeeBillModelRef>;
                    foreach (var item in temp0)
                    {
                        item.Creator = AjaxGetName(item.Creator);
                        object[] obj = new object[8] { item.PersonInfo.Department, item.BillNo, item.PageName, item.Creator, item.Owner, item.TotalMoney, item.StringTime, item.ExamineTime.ToString("yyyy-MM-dd HH:mm") };
                        result.Rows.Add(obj);
                    }
                    break;
                case 1:
                    var temp1 = data as List<FeeBillModelRef>;
                    foreach (var item in temp1)
                    {
                        item.Creator = AjaxGetName(item.Creator);
                        object[] obj = new object[8] { item.PersonInfo.Department, item.BillNo, item.PageName, item.Creator, item.Owner, item.TotalMoney, item.StringTime, item.ExamineTime.ToString("yyyy-MM-dd HH:mm") };
                        result.Rows.Add(obj);
                    }
                    break;
                case 2:
                    var temp2 = data as List<NoticeBillModelRef>;
                    foreach (var item in temp2)
                    {
                        item.Creator = AjaxGetName(item.Creator);
                        object[] obj = new object[8] { item.PersonInfo.Department, item.BillNo, item.PageName, item.Creator, item.Owner, item.TotalMoney, item.StringTime, item.ExamineTime.ToString("yyyy-MM-dd HH:mm") };
                        result.Rows.Add(obj);
                    }
                    break;
                case 3:
                    var temp3 = data as List<BorrowBillModelRef>;
                    foreach (var item in temp3)
                    {
                        item.Creator = AjaxGetName(item.Creator);
                        object[] obj = new object[8] { item.PersonInfo.Department, item.BillNo, item.PageName, item.Creator, item.Owner, item.TotalMoney, item.StringTime, item.ExamineTime.ToString("yyyy-MM-dd HH:mm") };
                        result.Rows.Add(obj);
                    }
                    break;
                case 4:
                    var temp4 = data as List<RefundBillModelRef>;
                    foreach (var item in temp4)
                    {
                        item.Creator = AjaxGetName(item.Creator);
                        object[] obj = new object[8] { item.PersonInfo.Department, item.BillNo, item.PageName, item.Creator, item.Owner, item.RealRefundMoney, item.StringTime, item.ExamineTime.ToString("yyyy-MM-dd HH:mm") };
                        result.Rows.Add(obj);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        public string GetReportData(int billType, int Time, int departmentID, string operation, string StartTime = "", string EndTime = "")
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();

            //我搁置的单
            if (operation == "1")
            {

                List<FeeBillModelRef> AllModel = new List<FeeBillModelRef>();
                var Temp1 = new FeeBill().ReturnShelveNo(employee.EmployeeName);
                var Temp2 = new NoticeBill().ReturnShelveNo(employee.EmployeeNo);
                var Temp3 = new BorrowBill().ReturnShelveNo(employee.EmployeeNo);
                var Temp4 = new RefundBill().ReturnShelveNo(employee.EmployeeNo);

                foreach (var item1 in Temp1)
                {
                    FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item1.BillNo, PageName = "费用报销单", Creator = item1.Creator, Owner = item1.Owner, TotalMoney = item1.TotalMoney, StringTime = item1.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item1.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item1.PersonInfo.DepartmentCode, Department = item1.PersonInfo.Department } };
                    AllModel.Add(TempModel);
                }
                foreach (var item2 in Temp2)
                {
                    FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item2.BillNo, PageName = "付款通知书", Creator = item2.Creator, Owner = item2.Owner, TotalMoney = item2.TotalMoney, StringTime = item2.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item2.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item2.PersonInfo.DepartmentCode, Department = item2.PersonInfo.Department } };
                    AllModel.Add(TempModel);
                }
                foreach (var item3 in Temp3)
                {
                    FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item3.BillNo, PageName = "借款单", Creator = item3.Creator, Owner = item3.Owner, TotalMoney = item3.TotalMoney, StringTime = item3.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item3.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item3.PersonInfo.DepartmentCode, Department = item3.PersonInfo.Department } };
                    AllModel.Add(TempModel);
                }
                foreach (var item4 in Temp4)
                {
                    FeeBillModelRef TempModel = new FeeBillModelRef() { BillNo = item4.BillNo, PageName = item4.RefundType.ToUpper() == "CASH" ? "现金还款" : "费用单还款", Creator = item4.Creator, Owner = item4.Owner, TotalMoney = item4.RealRefundMoney, StringTime = item4.CreateTime.ToString("yyyy-MM-dd"), CreateTime = item4.CreateTime, PersonInfo = new PersonInfo() { DepartmentCode = item4.PersonInfo.DepartmentCode, Department = item4.PersonInfo.Department } };
                    AllModel.Add(TempModel);
                }

                switch (billType)
                {
                    case 0:
                        break;
                    case 1:
                        AllModel.RemoveAll(c => c.PageName.Contains("付款") || c.PageName.Contains("借款") || c.PageName.Contains("还款"));
                        break;
                    case 2:
                        AllModel.RemoveAll(c => c.PageName.Contains("费用报销") || c.PageName.Contains("借款") || c.PageName.Contains("还款"));
                        break;
                    case 3:
                        AllModel.RemoveAll(c => c.PageName.Contains("付款") || c.PageName.Contains("费用报销") || c.PageName.Contains("还款"));
                        break;
                    case 4:
                        AllModel.RemoveAll(c => c.PageName.Contains("付款") || c.PageName.Contains("借款") || c.PageName.Contains("费用报销"));
                        break;
                    default:
                        break;
                }
                if (AllModel.Count > 0)
                {
                    var temp = TransformData(AllModel, 0);

                    MemoryCachingClient M = new MemoryCachingClient();
                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, temp);

                    return JsonSerializeHelper.SerializeToJson(temp);
                }
                return "{}";
            }

            object data = GetApprovalData(billType, Time, employee.EmployeeNo, departmentID.ToString(), StartTime, EndTime);

            if (data != null)
            {
                var temp = TransformData(data, billType);

                MemoryCachingClient M = new MemoryCachingClient();
                M.Remove(employee.EmployeeNo);
                M.Add(employee.EmployeeNo, temp);

                return JsonSerializeHelper.SerializeToJson(temp);
            }
            return "{}";
            //ReportHelper helper = new ReportHelper();
            //DataTable dt = helper.GetDataTable(@"select ID,NO,NAME,SHOPCODE,CREATOR,CHANGER,AVAILABLE,LEAVE,PASSWORD,DEPID from Employee where (DEPID=" + departmentID + " or 0=" + departmentID + ") and length(NO)>4 and ROWNUM<100");
            //foreach (DataColumn item in dt.Columns)
            //{
            //    if (item.ColumnName == "NAME")
            //    {
            //        item.ColumnName = "姓名";
            //    }
            //    if (item.ColumnName == "NO")
            //    {
            //        item.ColumnName = "工号";
            //    }
            //}
            //var temp = ReportHelper.ConvertDataTable(dt);
            //return JsonSerializeHelper.SerializeToJson(temp);
        }

        /// <summary>
        /// 获取供应商信息
        /// </summary>
        /// <param name="InputWord"></param>
        /// <returns></returns>
        public string GetDistributionCenter()
        {
            try
            {
                List<TempData> td = new List<TempData>();
                var temp = DbContext.DEPARTMENT.Where(c => c.IS_UCSTAR == 0 && c.ZONECODE.Trim().Length > 0).ToList();
                var temp1 = DbContext.DEPARTMENT.Where(c => c.IS_UCSTAR == 2 && c.PID == 1 && c.COMPANYCODE == "1000").ToList();
                var temp2 = DbContext.DEPARTMENT.Where(c => c.IS_UCSTAR == 2 && c.PID == 4860 && c.COMPANYCODE == "1300").ToList();
                var temp3 = DbContext.DEPARTMENT.Where(c => c.IS_UCSTAR == 2 && c.PID == 1 && c.COMPANYCODE == "4000").ToList();
                temp.ForEach((department) =>
                {

                    td.Add(new TempData()
                    {
                        label = department.COMPANYCODE + "-" + department.NAME,
                        value = department.ID.ToString()
                    });
                });
                temp1.ForEach((department) =>
                {

                    td.Add(new TempData()
                    {
                        label = department.COMPANYCODE + "-" + department.NAME,
                        value = department.ID.ToString()
                    });
                });
                temp2.ForEach((department) =>
                {

                    td.Add(new TempData()
                    {
                        label = department.COMPANYCODE + "-" + department.NAME,
                        value = department.ID.ToString()
                    });
                });
                temp3.ForEach((department) =>
                {

                    td.Add(new TempData()
                    {
                        label = department.COMPANYCODE + "-" + department.NAME,
                        value = department.ID.ToString()
                    });
                });
                return td == null ? "" : Public.JsonSerializeHelper.SerializeToJson(td);
            }
            catch (Exception ex)
            {
                Logger.Write("获取片区信息GetDistributionCenter：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }
    }
}
