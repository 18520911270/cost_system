using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using Marisfrolg.Fee.BLL;
using System.Text;

namespace Marisfrolg.Fee.Controllers
{
    public class FinanceApprovalController : SecurityController
    {
        //
        // GET: /FinanceApproval/

        public ActionResult Index()
        {
            return View();
        }


        public string GetMyProcess(int Type, int Time, string TimeValue1 = "", string TimeValue2 = "")
        {
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
                //自定义
                case 6:
                    startTime = TimeValue1 == "" ? startTime : Convert.ToDateTime(TimeValue1);
                    endTime = TimeValue2 == "" ? endTime : Convert.ToDateTime(TimeValue2);
                    break;
                default:
                    break;
            }
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();

                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                Dictionary<string, WorkFlowEngine.WorkFlowInstance> dic = new Dictionary<string, WorkFlowEngine.WorkFlowInstance>();
                string result = proxy.GetWorkFlowTaskList(employee.EmployeeNo, "", ref dic);
                if (!string.IsNullOrEmpty(result) && dic.Count > 0)
                {
                    List<MongoDB.Bson.ObjectId> Mylist = dic.Select(c => c.Value).Select(c => c._id).ToList();
                    List<string> WorkFlowList = new List<string>();
                    foreach (var item in Mylist)
                    {
                        WorkFlowList.Add(item.ToString());
                    }

                    switch (Type)
                    {

                        case 0:
                            List<FeeBillModelRef> TempData = new List<FeeBillModelRef>();
                            var list1 = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            foreach (var item in list1)
                            {
                                FeeBillModelRef Temp = new FeeBillModelRef() { PersonInfo = new PersonInfo() { Department = item.PersonInfo.Department, Brand = item.PersonInfo.Brand, Shop = item.PersonInfo.Shop }, BillNo = item.BillNo, PageName = "FeeBill", Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, CreateTime = item.CreateTime, Creator = item.Creator, PostString = item.PostString };
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(item.WorkFlowID);
                                Temp.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();

                                if (Temp.PostString != null)
                                {
                                    Temp.AuditTime = Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? Temp.CreateTime : Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    Temp.CostTime = Math.Round((DateTime.Now - Temp.AuditTime).TotalDays, 2);
                                }

                                TempData.Add(Temp);
                            }
                            var list2 = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            foreach (var item in list2)
                            {
                                FeeBillModelRef Temp = new FeeBillModelRef() { PersonInfo = new PersonInfo() { Department = item.PersonInfo.Department, Brand = item.PersonInfo.Brand, Shop = item.PersonInfo.Shop }, BillNo = item.BillNo, PageName = "NoticeBill", Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, CreateTime = item.CreateTime, Creator = item.Creator, PostString = item.PostString };
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(item.WorkFlowID);
                                Temp.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();

                                if (Temp.PostString != null)
                                {
                                    Temp.AuditTime = Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? Temp.CreateTime : Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    Temp.CostTime = Math.Round((DateTime.Now - Temp.AuditTime).TotalDays, 2);
                                }

                                TempData.Add(Temp);
                            }
                            var list3 = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            foreach (var item in list3)
                            {
                                FeeBillModelRef Temp = new FeeBillModelRef() { PersonInfo = new PersonInfo() { Department = item.PersonInfo.Department, Brand = item.PersonInfo.Brand, Shop = item.PersonInfo.Shop }, BillNo = item.BillNo, PageName = "BorrowBill", Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, CreateTime = item.CreateTime, Creator = item.Creator, PostString = item.PostString };
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(item.WorkFlowID);
                                Temp.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();

                                if (Temp.PostString != null)
                                {
                                    Temp.AuditTime = Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? Temp.CreateTime : Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    Temp.CostTime = Math.Round((DateTime.Now - Temp.AuditTime).TotalDays, 2);
                                }

                                TempData.Add(Temp);
                            }
                            var list4 = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            foreach (var item in list4)
                            {
                                FeeBillModelRef Temp = new FeeBillModelRef() { PersonInfo = new PersonInfo() { Department = item.PersonInfo.Department, Brand = item.PersonInfo.Brand, Shop = item.PersonInfo.Shop }, BillNo = item.BillNo, PageName = "RefundBill", Owner = item.Owner, TotalMoney = item.RealRefundMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, CreateTime = item.CreateTime, Creator = item.Creator, PostString = item.PostString };
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(item.WorkFlowID);
                                Temp.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();


                                if (Temp.PostString != null)
                                {
                                    Temp.AuditTime = Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? Temp.CreateTime : Temp.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    Temp.CostTime = Math.Round((DateTime.Now - Temp.AuditTime).TotalDays, 2);
                                }

                                TempData.Add(Temp);
                            }
                            TempData = TempData.Where(c => c.CreateTime.Date >= startTime.Date && c.CreateTime.Date <= endTime.Date).OrderByDescending(c => c.CreateTime).ToList();
                            return Public.JsonSerializeHelper.SerializeToJson(TempData);
                        //未审批的费用报销单
                        case 1:
                            var FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.FeeBillModelRef> RefList = new List<Models.FeeBillModelRef>();
                            foreach (var item in FeeModel)
                            {
                                Marisfrolg.Fee.Models.FeeBillModelRef RefModel = new Models.FeeBillModelRef();
                                RefModel = item.MapTo<FeeBillModel, FeeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "FeeBill";


                                if (RefModel.PostString != null)
                                {
                                    RefModel.AuditTime = RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? RefModel.CreateTime : RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    RefModel.CostTime = Math.Round((DateTime.Now - RefModel.AuditTime).TotalDays, 2);
                                }

                                RefList.Add(RefModel);
                            }
                            RefList = RefList.Where(c => c.CreateTime.Date >= startTime.Date && c.CreateTime.Date <= endTime.Date).OrderByDescending(c => c.CreateTime).ToList();
                            return Public.JsonSerializeHelper.SerializeToJson(RefList);
                        //未审批的付款通知书  
                        case 2:
                            var NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.NoticeBillModelRef> NoticeRefList = new List<Models.NoticeBillModelRef>();
                            foreach (var item in NoticeModel)
                            {
                                Marisfrolg.Fee.Models.NoticeBillModelRef RefModel = new Models.NoticeBillModelRef();
                                RefModel = item.MapTo<NoticeBillModel, NoticeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "NoticeBill";

                                if (RefModel.PostString != null)
                                {
                                    RefModel.AuditTime = RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? RefModel.CreateTime : RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    RefModel.CostTime = Math.Round((DateTime.Now - RefModel.AuditTime).TotalDays, 2);
                                }

                                NoticeRefList.Add(RefModel);
                            }
                            NoticeRefList = NoticeRefList.Where(c => c.CreateTime >= startTime && c.CreateTime < endTime).OrderByDescending(c => c.CreateTime).ToList();
                            return Public.JsonSerializeHelper.SerializeToJson(NoticeRefList);
                        //未审批的借款单  
                        case 3:
                            var BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.BorrowBillModelRef> BorrowRefList = new List<Models.BorrowBillModelRef>();
                            foreach (var item in BorrowModel)
                            {
                                Marisfrolg.Fee.Models.BorrowBillModelRef RefModel = new Models.BorrowBillModelRef();
                                RefModel = item.MapTo<BorrowBillModel, BorrowBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "BorrowBill";

                                if (RefModel.PostString != null)
                                {
                                    RefModel.AuditTime = RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? RefModel.CreateTime : RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    RefModel.CostTime = Math.Round((DateTime.Now - RefModel.AuditTime).TotalDays, 2);
                                }

                                BorrowRefList.Add(RefModel);
                            }
                            BorrowRefList = BorrowRefList.Where(c => c.CreateTime.Date >= startTime.Date && c.CreateTime.Date <= endTime.Date).OrderByDescending(c => c.CreateTime).ToList();
                            return Public.JsonSerializeHelper.SerializeToJson(BorrowRefList);
                        //未审批的借款单  
                        case 4:
                            var RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.RefundBillModelRef> RefundRefList = new List<Models.RefundBillModelRef>();
                            foreach (var item in RefundModel)
                            {
                                Marisfrolg.Fee.Models.RefundBillModelRef RefModel = new Models.RefundBillModelRef();
                                RefModel = item.MapTo<RefundBillModel, RefundBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefModel.TotalMoney = RefModel.RealRefundMoney;
                                RefModel.StringTime = RefModel.CreateTime.ToString("yyyy-MM-dd");
                                RefModel.PageName = "RefundBill";

                                if (RefModel.PostString != null)
                                {
                                    RefModel.AuditTime = RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault() == null ? RefModel.CreateTime : RefModel.PostString.Where(c => !string.IsNullOrEmpty(c.JobNumber)).LastOrDefault().Time;
                                    RefModel.CostTime = Math.Round((DateTime.Now - RefModel.AuditTime).TotalDays, 2);
                                }

                                RefundRefList.Add(RefModel);
                            }
                            RefundRefList = RefundRefList.Where(c => c.CreateTime.Date >= startTime.Date && c.CreateTime.Date <= endTime.Date).OrderByDescending(c => c.CreateTime).ToList();
                            return Public.JsonSerializeHelper.SerializeToJson(RefundRefList);
                        default:
                            break;
                    }

                }
                return "[]";
            }
            catch (Exception ex)
            {
                Logger.Write("获取未办结单据列表数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "[]";
        }



        /// <summary>
        /// 根据成本中心获取公司代码和名称
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <param name="CosterCenter"></param>
        /// <returns></returns>
        public ObjectList GetCompanyInfo(int IsHeadOffice, string CosterCenter)
        {

            ObjectList obj;
            if (IsHeadOffice == 1)
            {
                obj = GetBrandFromCosterCenter(CosterCenter);
            }
            else
            {
                obj = GetBrandFromCosterCenterNew(CosterCenter);
            }
            obj.NAME = DbContext.COMPANY.Where(c => c.CODE == obj.CODE).Select(x => x.NAME).FirstOrDefault();
            return obj;
        }



        /// <summary>
        /// 保存单据修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveBillChange(EditBillModel model)
        {
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                EditBillModel OriginalModel = new EditBillModel();
                EditLog Editform = new EditLog();
                string result = "";
                switch (model.BillsType)
                {
                    //费用单
                    case "1":
                        var list1 = new FeeBill().GetBillModel(model.BillNo);
                        OriginalModel = list1.MapTo<FeeBillModel, EditBillModel>();
                        OriginalModel.Brand = list1.PersonInfo.Brand;
                        OriginalModel.CostCenter = list1.PersonInfo.CostCenter;
                        var obj1 = GetCompanyInfo(list1.PersonInfo.IsHeadOffice, model.CostCenter);
                        model.E_Company = obj1.NAME;
                        model.E_CompanyCode = obj1.CODE;
                        result = new FeeBill().SaveFeeBillChange(model);
                        break;
                    //付款单
                    case "2":
                        var list2 = new NoticeBill().GetBillModel(model.BillNo);
                        OriginalModel.BillNo = list2.BillNo;
                        OriginalModel.BillsType = list2.BillsType;
                        OriginalModel.BillsItems = list2.BillsItems;
                        OriginalModel.Brand = list2.PersonInfo.Brand;
                        OriginalModel.CostCenter = list2.PersonInfo.CostCenter;
                        OriginalModel.Currency = list2.Currency;
                        OriginalModel.Items = list2.Items;
                        OriginalModel.MissBill = list2.MissBill;
                        OriginalModel.SpecialAttribute = new SpecialAttribute() { Agent = list2.SpecialAttribute.Agent, Funds = list2.SpecialAttribute.Funds, Check = list2.SpecialAttribute.Check, BankDebt = 0, MarketDebt = 0, Cash = 0 };

                        OriginalModel.MissBill = list2.MissBill;
                        OriginalModel.Photos = list2.Photos;
                        var obj2 = GetCompanyInfo(list2.PersonInfo.IsHeadOffice, model.CostCenter);
                        model.E_Company = obj2.NAME;
                        model.E_CompanyCode = obj2.CODE;
                        result = new NoticeBill().SaveNoticeBillChange(model);
                        break;
                    //借款单
                    case "3":
                        var list3 = new BorrowBill().GetBillModel(model.BillNo);
                        OriginalModel = list3.MapTo<BorrowBillModel, EditBillModel>();
                        OriginalModel.SpecialAttribute = list3.SpecialAttribute;
                        OriginalModel.Brand = list3.PersonInfo.Brand;
                        OriginalModel.CostCenter = list3.PersonInfo.CostCenter;
                        var obj3 = GetCompanyInfo(list3.PersonInfo.IsHeadOffice, model.CostCenter);
                        model.E_Company = obj3.NAME;
                        model.E_CompanyCode = obj3.CODE;
                        result = new BorrowBill().SaveBorrowBillChange(model);
                        break;
                    //费用还款单
                    case "4":
                        var list4 = new RefundBill().GetBillModel(model.BillNo);
                        OriginalModel = list4.MapTo<RefundBillModel, EditBillModel>();
                        OriginalModel.Brand = list4.PersonInfo.Brand;
                        OriginalModel.CostCenter = list4.PersonInfo.CostCenter;
                        var obj4 = GetCompanyInfo(list4.PersonInfo.IsHeadOffice, model.CostCenter);
                        model.E_Company = obj4.NAME;
                        model.E_CompanyCode = obj4.CODE;
                        result = new RefundBill().SaveRefundBillChange(model);
                        break;
                    default:
                        break;
                }
                Editform.BillNo = model.BillNo;
                Editform.CreateTime = DateTime.Now;
                Editform.Creator = employee.EmployeeNo;
                Editform.OriginalData = OriginalModel;
                Editform.ModifiedData = model;
                new EditLogForm().CreateEditRecord(Editform);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Write("保存单据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "";
            }
        }



        public string GetRelationBill(string itemName, string DepartmentCode, string ShopCode, DateTime CreateTime, string ProviderName, int IsHeadOffice, string BillNo)
        {
            if (string.IsNullOrEmpty(ShopCode) || ShopCode == "null")
            {
                ShopCode = null;
            }

            int FindDadaType = 0;
            if (BillNo.ToUpper().Contains("FT"))
            {
                FindDadaType = 1;
            }
            else if (BillNo.ToUpper().Contains("JS"))
            {
                FindDadaType = 2;
            }

            //取CreateTime时间，然后往前推三个月
            CreateTime = CreateTime.AddMonths(-3);
            List<FeeBillModelRef> DataList = new List<FeeBillModelRef>();

            if (FindDadaType == 0)
            {
                var FeeModel = new FeeBill().GetRelationBillList(itemName, DepartmentCode, ShopCode, CreateTime);
                if (FeeModel.Count > 0)
                {
                    foreach (var item in FeeModel)
                    {
                        try
                        {
                            FeeBillModelRef model = new FeeBillModelRef() { BillNo = item.BillNo, BillsType = "FeeBill", CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Remark = item.Remark, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PersonInfo = new PersonInfo { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, Shop = item.PersonInfo.Shop }, Items = item.Items, TotalMoney = item.TotalMoney, Creator = item.Creator, Owner = item.Owner };
                            DataList.Add(model);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                    }
                }
            }

            if (FindDadaType == 0 || FindDadaType == 1)
            {
                var NoticeModel = new NoticeBill().GetRelationBillList(itemName, DepartmentCode, ShopCode, CreateTime, ProviderName, IsHeadOffice, FindDadaType);
                if (NoticeModel.Count > 0)
                {
                    foreach (var item in NoticeModel)
                    {
                        try
                        {
                            FeeBillModelRef model = new FeeBillModelRef() { BillNo = item.BillNo, BillsType = "NoticeBill", CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Remark = item.Remark, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PersonInfo = new PersonInfo { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, Shop = item.PersonInfo.Shop }, Items = item.Items, TotalMoney = item.TotalMoney, Creator = item.Creator, Owner = item.Owner };
                            DataList.Add(model);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            if (FindDadaType == 0 || FindDadaType == 2)
            {

                var BorrowModel = new BorrowBill().GetRelationBillList(itemName, DepartmentCode, ShopCode, CreateTime);
                if (BorrowModel.Count > 0)
                {
                    foreach (var item in BorrowModel)
                    {
                        try
                        {
                            FeeBillModelRef model = new FeeBillModelRef() { BillNo = item.BillNo, BillsType = "BorrowBill", CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Remark = item.Remark, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PersonInfo = new PersonInfo { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, Shop = item.PersonInfo.Shop }, Items = item.Items, TotalMoney = item.TotalMoney, Creator = item.Creator, Owner = item.Owner };
                            DataList.Add(model);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            if (FindDadaType == 0)
            {
                var RefundModel = new RefundBill().GetRelationBillList(itemName, DepartmentCode, ShopCode, CreateTime);
                if (RefundModel.Count > 0)
                {
                    foreach (var item in RefundModel)
                    {
                        try
                        {
                            FeeBillModelRef model = new FeeBillModelRef() { BillNo = item.BillNo, BillsType = "RefundBill", CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Remark = item.Remark, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PersonInfo = new PersonInfo { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, Shop = item.PersonInfo.Shop }, Items = item.Items, TotalMoney = item.RealRefundMoney, Creator = item.Creator, Owner = item.Owner };
                            DataList.Add(model);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            if (DataList.Count > 0)
            {
                DataList = DataList.OrderByDescending(c => c.CreateTime).ToList();
            }
            return Public.JsonSerializeHelper.SerializeToJson(DataList);
        }


        /// <summary>
        /// 搁置单据
        /// </summary>
        /// <param name="BillNo"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public string ShelveBill(string BillNo, string Type, string Remark)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            string status = String.Empty;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ApprovalStatus", "5");
            dic.Add("ApprovalPost", Remark);
            dic.Add("ShelveNo", employee.EmployeeNo);
            switch (Type.ToUpper())
            {
                case "FEEBILL":
                    status = new FeeBill().PublicEditMethod(BillNo, dic);
                    break;

                case "NOTICEBILL":
                    status = new NoticeBill().PublicEditMethod(BillNo, dic);
                    break;

                case "BORROWBILL":
                    status = new BorrowBill().PublicEditMethod(BillNo, dic);
                    break;

                case "REFUNDBILL":
                    status = new RefundBill().PublicEditMethod(BillNo, dic);
                    break;
                default:
                    break;
            }
            return status;
        }
    }
}
