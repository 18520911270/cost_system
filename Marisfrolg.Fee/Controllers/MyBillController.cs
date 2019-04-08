using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Marisfrolg.Public;
using Marisfrolg.Fee.BLL;
using System.Data;
using Aspose.Cells;
using System.IO;
using Marisfrolg.Fee.Models;

namespace Marisfrolg.Fee.Controllers
{
    public class MyBillController : SecurityController
    {
        //
        // GET: /MyBill/

        public ActionResult MyBill()
        {
            return View();
        }


        public string ExportExcel(DataTable dt, string TableName)
        {
            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格

            int Colnum = dt.Columns.Count;//表格列数
            int Rownum = dt.Rows.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(dt.Columns[i].ColumnName);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(dt.Rows[i][k].ToString());
                }
                cells.SetRowHeight(1 + i, 24);
            }

            string NewlocalPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "ExcelDownLoad");
            if (!System.IO.Directory.Exists(NewlocalPath))
            {
                System.IO.Directory.CreateDirectory(NewlocalPath);
            }
            string filePathName = TableName + '-' + DateTime.Now.ToString("yyMMddhhmmss") + ".xls";
            workbook.Save(Path.Combine(NewlocalPath, filePathName));
            return filePathName;
        }


        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="Type">单据类型</param>
        /// <param name="Time">时间</param>
        /// <param name="WFType">单据状态</param>
        /// <param name="CreateTimeParameters1">开始时间</param>
        /// <param name="CreateTimeParameters2">结束时间</param>
        /// <returns></returns>
        public string GetMyBillList(int Type, int BillBelong, string CreateTimeParameters1 = "", string CreateTimeParameters2 = "", string DepartmentCode = "", string CheckStatus = "", string PrintStatus = "", string IsExport = "", string FeeSortList = "", string OverTimeParameters1 = "", string OverTimeParameters2 = "", string ShopCodeList = "", string BillStatus = "")
        {

            List<string> DepartmentList = new List<string>();
            if (!string.IsNullOrEmpty(DepartmentCode))
            {
                DepartmentList = DepartmentCode.Split(',').ToList();
                DepartmentList.Remove("");
            }
            try
            {
                List<Models.FeeBillModelRef> ModelList = new List<Models.FeeBillModelRef>();
                //时间区间控制
                DateTime startTime = new DateTime(1999, 1, 1);
                DateTime endTime = new DateTime(2999, 1, 1);
                if (!string.IsNullOrEmpty(CreateTimeParameters1))
                {
                    startTime = Convert.ToDateTime(CreateTimeParameters1);
                }
                if (!string.IsNullOrEmpty(CreateTimeParameters2))
                {
                    endTime = Convert.ToDateTime(CreateTimeParameters2);
                    endTime = endTime.AddDays(1).AddSeconds(-1);
                }

                //根据不同的单据类型调用不同的方法取值
                switch (Type)
                {
                    //全部
                    case 0:
                        List<Models.FeeBillModel> list1 = new Marisfrolg.Fee.BLL.FeeBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        foreach (var item in list1)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "FeeBill", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin };
                            ModelList.Add(temp);
                        }
                        List<Models.NoticeBillModel> list2 = new Marisfrolg.Fee.BLL.NoticeBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        foreach (var item in list2)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "NoticeBill", Remark = item.Remark, CreateTime = item.CreateTime, ProviderName = item.ProviderInfo.ProviderName, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, MissBill = item.MissBill };
                            ModelList.Add(temp);
                        }
                        List<Models.BorrowBillModel> list3 = new Marisfrolg.Fee.BLL.BorrowBill().GetMyBillList(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        foreach (var item in list3)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "BorrowBill", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin };
                            ModelList.Add(temp);
                        }
                        List<Models.RefundBillModel> list4 = new Marisfrolg.Fee.BLL.RefundBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        foreach (var item in list4)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.RealRefundMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "RefundBill", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin };
                            ModelList.Add(temp);
                        }
                        ModelList = ModelList.OrderByDescending(c => c.CreateTime).ToList();
                        break;
                    //费用报销单
                    case 1:
                        List<Models.FeeBillModel> FeeBill = new Marisfrolg.Fee.BLL.FeeBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        List<Models.FeeBillModelRef> FeeBillList = new List<Models.FeeBillModelRef>();
                        foreach (var item in FeeBill)
                        {
                            Models.FeeBillModelRef FeeModel = new Models.FeeBillModelRef();
                            FeeModel = item.MapTo<Models.FeeBillModel, Models.FeeBillModelRef>();
                            FeeModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            FeeModel.PageName = "FeeBill";
                            FeeBillList.Add(FeeModel);
                        }
                        ModelList = FeeBillList.OrderByDescending(c => c.CreateTime).ToList();
                        break;
                    //付款通知书 
                    case 2:
                        List<Models.NoticeBillModel> NoticeBill = new Marisfrolg.Fee.BLL.NoticeBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        List<Models.FeeBillModelRef> NoticeBillList = new List<Models.FeeBillModelRef>();
                        foreach (var item in NoticeBill)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "NoticeBill", Remark = item.Remark, CreateTime = item.CreateTime, ProviderName = item.ProviderInfo.ProviderName, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, MissBill = item.MissBill };
                            NoticeBillList.Add(temp);
                        }
                        ModelList = NoticeBillList.OrderByDescending(c => c.CreateTime).ToList();
                        break;
                    //借款单（所有的借款记录，不论是否还清）
                    case 3:
                        List<Models.BorrowBillModel> BorrowBill = new Marisfrolg.Fee.BLL.BorrowBill().GetMyBillList(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        List<Models.FeeBillModelRef> BorrowBillList = new List<Models.FeeBillModelRef>();
                        foreach (var item in BorrowBill)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "BorrowBill", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin };
                            BorrowBillList.Add(temp);
                        }
                        ModelList = BorrowBillList.OrderByDescending(c => c.CreateTime).ToList();
                        break;
                    //还款单(加载还款记录)
                    case 4:
                        List<Models.RefundBillModel> RefundBill = new Marisfrolg.Fee.BLL.RefundBill().GetBillForPrint(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        List<Models.FeeBillModelRef> RefundBillList = new List<Models.FeeBillModelRef>();
                        foreach (var item in RefundBill)
                        {
                            Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice }, Owner = item.Owner, TotalMoney = item.RealRefundMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "RefundBill", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin };
                            RefundBillList.Add(temp);
                        }
                        ModelList = RefundBillList.OrderByDescending(c => c.CreateTime).ToList();
                        break;
                    //借款单（返回审核通过的借款单）
                    case 5:
                        List<Models.BorrowBillModel> NewBorrowBill = new Marisfrolg.Fee.BLL.BorrowBill().GetMyBillList(startTime, endTime, BillBelong, DepartmentList, CheckStatus, PrintStatus);
                        NewBorrowBill = NewBorrowBill.Where(c => c.ApprovalStatus == 2).ToList();
                        List<Models.BorrowBillModelRef> NewBorrowBillList = new List<Models.BorrowBillModelRef>();
                        foreach (var item in NewBorrowBill)
                        {
                            Models.BorrowBillModelRef BorrowModel = new Models.BorrowBillModelRef();
                            BorrowModel = item.MapTo<Models.BorrowBillModel, Models.BorrowBillModelRef>();
                            BorrowModel.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                            NewBorrowBillList.Add(BorrowModel);
                        }
                        return Public.JsonSerializeHelper.SerializeToJson(NewBorrowBillList.OrderByDescending(c => c.CreateTime));
                    default:
                        break;
                }

                //费用小项的筛选
                if (!string.IsNullOrEmpty(FeeSortList))
                {
                    List<string> FeeList = FeeSortList.Split(',').ToList();
                    FeeList.Remove("");
                    List<decimal> IntList = new List<decimal>();
                    foreach (var item in FeeList)
                    {
                        IntList.Add(Convert.ToDecimal(item));
                    }
                    FeeList = DbContext.FEE_ACCOUNT.Where(c => IntList.Contains(c.ID)).Select(x => x.NAME).ToList();

                    if (ModelList.Count > 0)
                    {
                        List<Models.FeeBillModelRef> DeleteModel = new List<Models.FeeBillModelRef>();

                        foreach (var item in ModelList)
                        {
                            var IsDelete = true;
                            if (item.BillNo.Contains("HK") || item.Items == null || item.Items.Count == 0)
                            {
                                DeleteModel.Add(item);
                            }
                            else
                            {
                                foreach (var Titem in item.Items)
                                {
                                    if (FeeList.Contains(Titem.name))
                                    {
                                        IsDelete = false;
                                        break;
                                    }
                                }
                                if (IsDelete)
                                {
                                    DeleteModel.Add(item);
                                }
                            }
                        }
                        if (DeleteModel.Count > 0)
                        {
                            foreach (var item in DeleteModel)
                            {
                                ModelList.Remove(item);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(OverTimeParameters1))
                {
                    DateTime time = Convert.ToDateTime(OverTimeParameters1);
                    List<Models.FeeBillModelRef> DeleteModel = new List<Models.FeeBillModelRef>();

                    if (ModelList.Count > 0)
                    {
                        foreach (var item in ModelList)
                        {
                            DateTime apptime = Convert.ToDateTime(item.ApprovalTime);

                            if (time > apptime)
                            {
                                DeleteModel.Add(item);
                            }
                        }
                        if (DeleteModel.Count > 0)
                        {
                            foreach (var item in DeleteModel)
                            {
                                ModelList.Remove(item);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(OverTimeParameters2))
                {
                    DateTime time = Convert.ToDateTime(OverTimeParameters2);
                    time = time.AddDays(1).AddSeconds(-1);
                    List<Models.FeeBillModelRef> DeleteModel = new List<Models.FeeBillModelRef>();

                    if (ModelList.Count > 0)
                    {
                        foreach (var item in ModelList)
                        {
                            DateTime apptime = Convert.ToDateTime(item.ApprovalTime);

                            if (time < apptime)
                            {
                                DeleteModel.Add(item);
                            }
                        }
                        if (DeleteModel.Count > 0)
                        {
                            foreach (var item in DeleteModel)
                            {
                                ModelList.Remove(item);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ShopCodeList))
                {
                    List<string> Newlist = ShopCodeList.Split(',').ToList();
                    Newlist.Remove("");
                    ModelList = ModelList.Where(c => Newlist.Contains(c.PersonInfo.ShopCode)).ToList();
                }


                if (!string.IsNullOrEmpty(BillStatus))
                {
                    int status = Convert.ToInt32(BillStatus);

                    ModelList = ModelList.Where(c => c.PageName == "NoticeBill" && c.MissBill == status).ToList();
                }

                if (IsExport != "1")
                {
                    return Public.JsonSerializeHelper.SerializeToJson(ModelList);
                }
                else
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("单号");
                    dt.Columns.Add("单据类型");
                    dt.Columns.Add("发生品牌");
                    dt.Columns.Add("业务人");
                    dt.Columns.Add("供应商");
                    dt.Columns.Add("发生金额");
                    dt.Columns.Add("创建日期");
                    dt.Columns.Add("办结日期");
                    dt.Columns.Add("审核状态");
                    dt.Columns.Add("备注");

                    if (ModelList.Count > 0)
                    {
                        string ApprovalStatus = "";
                        string BillType = "";
                        foreach (var item in ModelList)
                        {
                            DataRow row = dt.NewRow();
                            string Brand = "";
                            row["单号"] = item.BillNo;
                            if (item.PageName == "FeeBill")
                            {
                                BillType = "费用报销单";
                            }
                            else if (item.PageName == "NoticeBill")
                            {
                                BillType = "付款通知书";
                                row["供应商"] = item.ProviderName;
                            }
                            else if (item.PageName == "BorrowBill")
                            {
                                BillType = "借款单";
                            }
                            else if (item.PageName == "RefundBill")
                            {
                                BillType = "还款单";
                            }
                            row["单据类型"] = BillType;
                            if (item.PersonInfo.Brand == null || item.PersonInfo.Brand.Count == 0)
                            {
                                Brand = "无记账品牌";
                            }
                            else
                            {
                                foreach (var item1 in item.PersonInfo.Brand)
                                {
                                    Brand += item1 + ",";
                                }
                                Brand = Brand.Remove(Brand.Length - 1);
                            }
                            row["发生品牌"] = Brand;
                            row["业务人"] = item.Owner;
                            row["发生金额"] = item.TotalMoney;
                            row["创建日期"] = item.CreateTime;
                            row["办结日期"] = item.ApprovalTime == null ? "" : item.ApprovalTime;
                            if (item.ApprovalStatus == 2)
                            {
                                ApprovalStatus = "通过";
                            }
                            else if (item.ApprovalStatus == 3)
                            {
                                ApprovalStatus = "拒绝";
                            }
                            else
                            {
                                ApprovalStatus = item.ApprovalPost + "审核中";
                            }
                            row["审核状态"] = ApprovalStatus;
                            row["备注"] = item.Remark;
                            dt.Rows.Add(row);
                        }
                    }
                    string Name = ExportExcel(dt, "我的单据");
                    return Name;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("获取打印列表数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }


        public string GetShopDataList(string Department)
        {
            if (!string.IsNullOrEmpty(Department))
            {
                List<string> code = Department.Split(',').ToList();
                code.Remove("");
                List<decimal> IDList = new List<decimal>();
                foreach (var item in code)
                {
                    IDList.Add(Convert.ToDecimal(item));
                }

                try
                {
                    List<ObjectList> list = new List<ObjectList>();
                    foreach (var item1 in IDList)
                    {
                        var model = DbContext.SHOP.Where(c => c.DEPARTMENTID == item1 && c.AVAILABLE == "1").Select(x => new ObjectList
                        {
                            CODE = x.CODE,
                            NAME = x.NAME
                        }).ToList();
                        list.AddRange(model);
                    }
                    //找到我拥有的片区店柜权限
                    List<string> newList = new List<string>();
                    var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                    string str = DbContext.UIVALUE.Where(c => c.VALUETYPE == 4 && c.EMPLOYEENO == employee.EmployeeNo).Select(x => x.VALUE).FirstOrDefault();
                    newList = str == null ? newList : str.Trim().Split(',').ToList();
                    if (!string.IsNullOrEmpty(employee.ShopCode))
                    {
                        newList.Add(employee.ShopCode);
                    }
                    list = list.Where(c => newList.Contains(c.CODE)).ToList();

                    if (list != null && list.Count > 0)
                    {
                        return Public.JsonSerializeHelper.SerializeToJson(list);
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    Logger.Write("获取门店列表数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
                return "";
            }
            return "";
        }


        public string ExistBillNo(string BillNo, string No)
        {
            string Result = "";
            BillNo = BillNo.Trim().ToUpper();
            var FeeModel = new FeeBill().GetBillModel(BillNo, No);
            if (FeeModel != null)
            {
                Result = "FeeBill";
                return Result;
            }
            var NoticeModel = new NoticeBill().GetBillModel(BillNo, No);
            if (NoticeModel != null)
            {
                Result = "NoticeBill";
                return Result;
            }
            var BorrowModel = new BorrowBill().GetBillModel(BillNo, No);
            if (BorrowModel != null)
            {
                Result = "BorrowBill";
                return Result;
            }
            var RefundModel = new RefundBill().GetBillModel(BillNo, No);
            if (RefundModel != null)
            {
                Result = "RefundBill";
                return Result;
            }
            return Result;
        }



        public string AddPrintedCount(string BillNo, string BillType)
        {
            string Status = "";
            switch (BillType)
            {
                case "1":
                    Status = new FeeBill().AddPrintedNum(BillNo);
                    break;
                case "2":
                    Status = new NoticeBill().AddPrintedNum(BillNo);
                    break;
                case "3":
                    Status = new BorrowBill().AddPrintedNum(BillNo);
                    break;
                case "4":
                    Status = new RefundBill().AddPrintedNum(BillNo);
                    break;
                default:
                    break;
            }
            return "";
        }


        public string MoveRecycleBin(string BillNo, string Type, string Status)
        {
            string result = "";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("RecycleBin", Status);
            switch (Type.ToUpper())
            {
                case "FEEBILL":
                    result = new FeeBill().PublicEditMethod(BillNo, dic);
                    break;
                case "NOTICEBILL":
                    result = new NoticeBill().PublicEditMethod(BillNo, dic);
                    break;
                case "BORROWBILL":
                    result = new BorrowBill().PublicEditMethod(BillNo, dic);
                    break;
                case "REFUNDBILL":
                    result = new RefundBill().PublicEditMethod(BillNo, dic);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 添加附件
        /// </summary>
        /// <param name="Model">附件列表</param>
        /// <returns></returns>
        public string AddPhoto(AddPhotoModel Model)
        {
            string result = "";

            Dictionary<string, List<PhotoModel>> dic = new Dictionary<string, List<PhotoModel>>();
            dic.Add("Photos", Model.Photos);

            switch (Model.Type.ToUpper())
            {
                case "1":
                    result = new FeeBill().PublicEditMethod<List<PhotoModel>>(Model.BillNo, dic);
                    break;
                case "2":
                    result = new NoticeBill().PublicEditMethod<List<PhotoModel>>(Model.BillNo, dic);
                    break;
                case "3":
                    result = new BorrowBill().PublicEditMethod<List<PhotoModel>>(Model.BillNo, dic);
                    break;
                case "4":
                    result = new RefundBill().PublicEditMethod<List<PhotoModel>>(Model.BillNo, dic);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
