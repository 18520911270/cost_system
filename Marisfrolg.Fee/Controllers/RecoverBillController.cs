using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using System.Data;
using Aspose.Cells;
using System.Text;
using Marisfrolg.Business;
using System.Data.Entity.Validation;
using Marisfrolg.Public;
using System.IO;

namespace Marisfrolg.Fee.Controllers
{
    public class RecoverBillController : SecurityController
    {
        //
        // GET: /RecoverBill/

        public ActionResult Index()
        {
            return View();
        }


        List<string> GetListString(string str)
        {
            var s1 = str.Split(',').ToList();
            s1.Remove("");
            return s1;
        }

        List<int> GetListInt(List<string> str)
        {
            List<int> OutInt = new List<int>();

            foreach (var item in str)
            {
                OutInt.Add(Convert.ToInt32(item));
            }
            return OutInt;
        }

        /// <summary>
        /// 获取回收单据列表
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        public string GetMissBill(string Type, string CompanyCode, string IsBelong, string CodeList, string BillNo, string TimeValue1, string TimeValue2)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            DateTime startTime = new DateTime(1999, 1, 1);
            DateTime endTime = new DateTime(2999, 1, 1);
            if (!string.IsNullOrEmpty(TimeValue1))
            {
                startTime = Convert.ToDateTime(TimeValue1);
            }
            if (!string.IsNullOrEmpty(TimeValue2))
            {
                endTime = Convert.ToDateTime(TimeValue2);
            }
            List<NoticeBillModel> model = new List<NoticeBillModel>();

            if (!string.IsNullOrEmpty(BillNo))
            {
                BillNo = BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
                var list = BillNo.Split(',').ToList();
                list.Remove("");
                NoticeBill NB = new NoticeBill();

                foreach (var item in list)
                {
                    var temp = NB.GetBillModel(item);
                    if (temp != null)
                    {
                        model.Add(temp);
                    }
                }
            }
            else
            {
                var t1 = GetListInt(GetListString(Type));
                var t2 = GetListInt(GetListString(IsBelong));
                model = new NoticeBill().GetMissBill(t1, CompanyCode, t2);
            }

            if (!string.IsNullOrEmpty(CodeList))
            {
                model = model.Where(c => CodeList.Contains(c.PersonInfo.DepartmentCode)).ToList();
            }
            model = model.Where(c => c.CreateTime >= startTime && c.CreateTime <= endTime).ToList();
            model = model.OrderByDescending(c => c.CreateTime).ToList();
            List<NoticeBillModelRef> ModelRef = new List<NoticeBillModelRef>();
            if (model != null && model.Count > 0)
            {
                foreach (var item in model)
                {
                    NoticeBillModelRef temp = new NoticeBillModelRef();
                    temp = item.MapTo<NoticeBillModel, NoticeBillModelRef>();
                    temp.StringTime = item.CreateTime.ToString("yyyy-MM-dd");
                    ModelRef.Add(temp);
                }

                MemoryCachingClient M = new MemoryCachingClient();
                M.Remove(employee.EmployeeNo);
                M.Add(employee.EmployeeNo, ModelRef);
            }

            return ModelRef.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(ModelRef.OrderByDescending(c => c.StringTime));
        }



        public string DownLoadFile()
        {
            string error = string.Empty;
            string msg = string.Empty;

            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            MemoryCachingClient M = new MemoryCachingClient();
            List<NoticeBillModelRef> dt = M.GetData(employee.EmployeeNo) as List<NoticeBillModelRef>;

            if (dt == null || dt.Count <= 0)
            {
                error = "1";
                msg = "无相应数据";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            List<string> Columns = new List<string>() { "单号", "业务人", "发生金额", "所在部门", "创建日期", "发票是否追回" };

            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格

            int Colnum = Columns.Count;//表格列数
            int Rownum = dt.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(Columns[i]);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                cells[1 + i, 0].PutValue(dt[i].BillNo);
                cells[1 + i, 1].PutValue(dt[i].WorkNumber);
                cells[1 + i, 2].PutValue(dt[i].TotalMoney);
                cells[1 + i, 3].PutValue(dt[i].PersonInfo.Department);
                cells[1 + i, 4].PutValue(dt[i].CreateTime);
                cells[1 + i, 5].PutValue(dt[i].MissBill == 0 ? "否" : "是");
            }

            string NewlocalPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "ExcelDownLoad");
            if (!System.IO.Directory.Exists(NewlocalPath))
            {
                System.IO.Directory.CreateDirectory(NewlocalPath);
            }
            string filePathName = "回收发票" + '-' + DateTime.Now.ToString("yyMMddhhmmss") + ".xls";
            workbook.Save(Path.Combine(NewlocalPath, filePathName));

            return Public.JsonSerializeHelper.SerializeToJson(new { error = "0", msg = "导出成功", data = filePathName });
        }


        /// <summary>
        /// 回收发票操作
        /// </summary>
        /// <param name="BillNo">单号</param>
        /// <returns></returns>
        public string RecoverBill(string BillNo)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            string Result = new NoticeBill().NoticeRecoverBill(BillNo, employee.EmployeeNo);
            return Result;
        }


        public ActionResult TransferBill()
        {
            return View();
        }


        public ActionResult InvoiceInfo()
        {

            return View();
        }

        public ActionResult TaxInfo()
        {
            return View();
        }


        public string ShiftBill(string Work1, string Work2)
        {
            string Result = "";
            if (string.IsNullOrEmpty(Work1) || string.IsNullOrEmpty(Work2))
            {
                Result = "工号不能为空";
                return Result;
            }
            var employee = DbContext.EMPLOYEE.Where(c => c.NO == Work1).FirstOrDefault();
            if (employee == null)
            {
                Result = "工号不存在";
                return Result;
            }
            var employee2 = DbContext.EMPLOYEE.Where(c => c.NO == Work2).FirstOrDefault();
            if (employee2 == null)
            {
                Result = "工号不存在";
                return Result;
            }
            if (employee.LEAVE == "0")
            {
                Result = "该员工还在职,不能转移单据";
                return Result;
            }
            try
            {
                var count1 = new FeeBill().TransferBill(Work1, Work2, employee2.NAME);
                var count2 = new NoticeBill().TransferBill(Work1, Work2, employee2.NAME);
                var count3 = new BorrowBill().TransferBill(Work1, Work2, employee2.NAME);
                var count4 = new RefundBill().TransferBill(Work1, Work2, employee2.NAME);
                string str = String.Format("费用单转移{0}单,付款通知书转移{1}单,借款单转移{2}单,还款单转移{3}单", count1, count2, count3, count4);
                return str;
            }
            catch (Exception)
            {
                return "Fail";
            }
        }


        public string LoadBillNo(string BillNo)
        {
            var model1 = new FeeBill().GetBillModel(BillNo);
            if (model1 != null)
            {
                return "1";
            }
            var model2 = new NoticeBill().GetBillModel(BillNo);
            if (model2 != null)
            {
                return "2";
            }
            var model3 = new BorrowBill().GetBillModel(BillNo);
            if (model3 != null)
            {
                return "3";
            }
            var model4 = new RefundBill().GetBillModel(BillNo);
            if (model4 != null)
            {
                return "4";
            }
            return "";
        }


        /// <summary>
        /// 获取供应商银行信息
        /// </summary>
        /// <returns></returns>
        public string LoadProviderBankInfo()
        {
            var query = (from a in DbContext.FEE_PROVIDERBANK
                         join b in DbContext.FEE_PROVIDER on a.CODE equals b.CODE
                         select new
                         {
                             PROVIDERNAME = b.NAME,
                             CODE = a.CODE,
                             BANKNAME = a.BANKNAME,
                             BANKNO = a.BANKNO,
                             SWIFT = a.SWIFT,
                             IBAN = a.IBAN,
                             ISUSERDATA = a.ISUSERDATA,
                             ID = a.ID
                         }).ToList();

            return Public.JsonSerializeHelper.SerializeToJson(query);
        }

        /// <summary>
        /// 删除方法
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string DeleteProviderBankInfo(decimal ID)
        {
            var model = DbContext.FEE_PROVIDERBANK.Where(c => c.ID == ID).FirstOrDefault();
            DbContext.FEE_PROVIDERBANK.Attach(model);
            DbContext.FEE_PROVIDERBANK.Remove(model);
            int result = DbContext.SaveChanges();
            if (result > 0)
            {
                return "Success";
            }
            else
            {
                return "Fail";
            }
        }


        public string GetProViderInfo(decimal ID)
        {
            var Model = DbContext.FEE_PROVIDERBANK.Where(c => c.ID == ID).FirstOrDefault();
            return Public.JsonSerializeHelper.SerializeToJson(Model);
        }


        public string SubmitProviderInfo(Marisfrolg.Business.FEE_PROVIDERBANK Model)
        {
            //新增
            if (Model.ID == 0)
            {
                Model.BANKCODE = Model.CODE;
                Model.PROVIDERNAME = "system";
                Model.ISUSERDATA = 1;
                Model.LAND = "CN";
                DbContext.FEE_PROVIDERBANK.Add(Model);
            }
            //编辑
            else
            {
                var oldModel = DbContext.FEE_PROVIDERBANK.Where(c => c.ID == Model.ID).FirstOrDefault();
                oldModel.CODE = Model.CODE;
                oldModel.COMPANYCODE = Model.COMPANYCODE;
                oldModel.BANKNAME = Model.BANKNAME;
                oldModel.BANKNO = Model.BANKNO;
                oldModel.SWIFT = Model.SWIFT;
                oldModel.IBAN = Model.IBAN;
                var entry = DbContext.Entry<Marisfrolg.Business.FEE_PROVIDERBANK>(oldModel);
                entry.State = System.Data.EntityState.Modified;
            }
            int result = DbContext.SaveChanges();
            if (result > 0)
            {
                return "Success";
            }
            else
            {
                return "Fail";
            }
        }


        /// <summary>
        /// 获取个人消息记录
        /// </summary>
        /// <returns></returns>
        public string GetPesonalRecord()
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            var model = DbContext.FEE_IMPORTDATA.Where(c => c.CREATOR == employee.EmployeeNo).GroupBy(x => x.REMARK).Select(p => p.Key).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }



        //检测标题
        public bool ExamineExcleHead(Workbook workbook)
        {
            Cells cells = workbook.Worksheets[0].Cells;
            int num = cells.MaxDisplayRange.ColumnCount;
            if (num != 17)
            {
                return false;
            }

            return "片区".Equals(cells[0, 0].StringValue.Trim()) && "片区编码".Equals(cells[0, 1].StringValue.Trim()) && "门店".Equals(cells[0, 2].StringValue.Trim()) && "门店编码".Equals(cells[0, 3].StringValue.Trim()) && "应发合计".Equals(cells[0, 4].StringValue.Trim()) && "个税".Equals(cells[0, 5].StringValue.Trim()) && "公积金".Equals(cells[0, 6].StringValue.Trim()) && "社保".Equals(cells[0, 7].StringValue.Trim()) && "其它应扣".Equals(cells[0, 8].StringValue.Trim()) && "实发工资".Equals(cells[0, 9].StringValue.Trim()) && "罚款".Equals(cells[0, 10].StringValue.Trim()) && "公司发文赔款、罚款".Equals(cells[0, 11].StringValue.Trim()) && "商场罚款".Equals(cells[0, 12].StringValue.Trim()) && "餐费".Equals(cells[0, 13].StringValue.Trim()) && "爱心基金".Equals(cells[0, 14].StringValue.Trim()) && "扣话费".Equals(cells[0, 15].StringValue.Trim()) && "其他".Equals(cells[0, 16].StringValue.Trim());
        }


        [HttpPost]
        public ActionResult LoadExcelData()
        {
            var file = HttpContext.Request.Files[0];
            var a = HttpContext.Request.Form["listFourSelect"];
            var b = HttpContext.Request.Form["listFourSelect2"];
            var c = HttpContext.Request.Form["Time1"];

            string msg = string.Empty;
            string error = string.Empty;

            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                //读取Excel内容
                Workbook workbook = new Workbook(file.InputStream);

                if (file.ContentLength <= 0)
                {
                    error = "请选择Excel文件！";
                    return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                }

                bool Istrue = ExamineExcleHead(workbook);
                if (Istrue == false)
                {
                    error = "标题行不准确！";
                    return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                }

                Cells cells = workbook.Worksheets[0].Cells;
                for (int i = 1; i < cells.MaxDataRow + 1; i++)
                {
                    FEE_IMPORTDATA IM = new FEE_IMPORTDATA();

                    //IM.DEPARTMENT = cells[i, 0].StringValue.Trim();
                    string str1 = cells[i, 1].StringValue.Trim().ToUpper();
                    var id = DbContext.DEPARTMENT.Where(p => p.CODE == str1).Select(x => x.ID).FirstOrDefault();
                    IM.DEPARTMENTID = Convert.ToInt32(id);
                    if (IM.DEPARTMENTID == 0)
                    {
                        error = "第" + i + "行片区编码数据有误！";
                        return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                    IM.DEPARTMENT = DbContext.DEPARTMENT.Where(p => p.CODE == str1).Select(x => x.NAME).FirstOrDefault();
                    //IM.SHOPNAME = cells[i, 2].StringValue.Trim();
                    IM.SHOPCODE = cells[i, 3].StringValue.Trim().ToUpper();
                    var shopName = DbContext.SHOP.Where(p => p.CODE == IM.SHOPCODE).Select(x => x.NAME).FirstOrDefault();
                    if (!string.IsNullOrEmpty(IM.SHOPCODE) && string.IsNullOrEmpty(shopName))
                    {
                        error = "第" + i + "行门店编码数据有误！";
                        return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                    IM.SHOPNAME = shopName;
                    IM.TRANSACTIONDATE = Convert.ToDateTime(c);
                    IM.CREATETIME = DateTime.Now;
                    IM.TYPE = "CW";
                    IM.CREATOR = employee.EmployeeNo;
                    string str2 = "";
                    if (string.IsNullOrEmpty(IM.SHOPCODE))
                    {
                        str2 = DbContext.FEE_SHOP_COSTCENTER.Where(p => p.SHOPCODE == str1).Select(x => x.COSTCODE).FirstOrDefault();
                    }
                    else
                    {
                        str2 = DbContext.FEE_SHOP_COSTCENTER.Where(p => p.SHOPCODE == IM.SHOPCODE).Select(x => x.COSTCODE).FirstOrDefault();
                    }
                    IM.COST_ACCOUNT = str2;
                    string str5 = IM.TYPE == "CW" ? "财务薪资" : IM.TYPE;
                    IM.REMARK = String.Format("用户{0}已导{1}年{2}月{3}数据，发生品牌为{4}，币种为{5}", IM.CREATOR, Convert.ToDateTime(c).Year, Convert.ToDateTime(c).Month, str5, a, b);
                    var str3 = DbContext.DEPARTMENT.Where(p => p.ID == IM.DEPARTMENTID).Select(x => x.COMPANYCODE).FirstOrDefault();
                    IM.COMPANYCODE = Convert.ToInt32(str3);
                    IM.BRANDCODE = a;
                    IM.CURRENCYCODE = b;
                    IM.TOTALMONEY = GetCorrectData(cells[i, 4].StringValue.Trim());


                    FEE_IMPORTDATA_ITEMS IMitems = new FEE_IMPORTDATA_ITEMS();
                    IMitems.OUGHTPAY = GetCorrectData(cells[i, 4].StringValue.Trim());
                    IMitems.PERSONALTAX = GetCorrectData(cells[i, 5].StringValue.Trim());
                    IMitems.PUBLICFUND = GetCorrectData(cells[i, 6].StringValue.Trim());
                    IMitems.SOCIAL = GetCorrectData(cells[i, 7].StringValue.Trim());
                    IMitems.OTHERDEDUCT = GetCorrectData(cells[i, 8].StringValue.Trim());
                    IMitems.REALSALARY = GetCorrectData(cells[i, 9].StringValue.Trim());
                    IMitems.FINE = GetCorrectData(cells[i, 10].StringValue.Trim());
                    IMitems.COMPANYFINE = GetCorrectData(cells[i, 11].StringValue.Trim());
                    IMitems.MARKETFINE = GetCorrectData(cells[i, 12].StringValue.Trim());
                    IMitems.MEALFEE = GetCorrectData(cells[i, 13].StringValue.Trim());
                    IMitems.BENEFICENTFUND = GetCorrectData(cells[i, 14].StringValue.Trim());
                    IMitems.PHONEFEE = GetCorrectData(cells[i, 15].StringValue.Trim());
                    IMitems.OTHERITEM = GetCorrectData(cells[i, 16].StringValue.Trim());
                    IM.FEE_IMPORTDATA_ITEMS.Add(IMitems);
                    DbContext.FEE_IMPORTDATA.Add(IM);
                    DbContext.Configuration.AutoDetectChangesEnabled = false;
                }
                int result = DbContext.SaveChanges();
                msg = "上传成功！";
            }
            catch (DbEntityValidationException dbEx)
            {
                throw;
            }
            return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
        }

        public decimal GetCorrectData(string obj)
        {
            decimal i = 0;
            if (string.IsNullOrEmpty(obj))
            {
                return 0;
            }
            Decimal.TryParse(obj, out i);
            return i;
        }

        public string OperateBill(string Bill, string IsOver)
        {
            if (string.IsNullOrEmpty(Bill))
            {
                return "";
            }
            Bill = Bill.ToUpper().Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = Bill.Split(',').ToList();
            list.Remove("");
            List<FeeBillModelRef> ModelList = new List<FeeBillModelRef>();
            {
                foreach (var item in list)
                {
                    var model = new ReportController().FindBillNo(item, "3", "", null);
                    if (model != null)
                    {
                        ModelList.Add(model);
                    }
                }
            }
            if (IsOver == "2")
            {
                ModelList = ModelList.Where(c => c.ApprovalStatus == 2).ToList();
            }

            if (ModelList.Count > 0)
            {
                return JsonSerializeHelper.SerializeToJson(ModelList);
            }
            else
            {
                return "[]";
            }
        }

        public string OperateBorrowBill(string Bill)
        {
            if (string.IsNullOrEmpty(Bill))
            {
                return "";
            }
            Bill = Bill.ToUpper().Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = Bill.Split(',').ToList();
            list.Remove("");
            List<FeeBillModelRef> ModelList = new List<FeeBillModelRef>();
            {
                foreach (var item in list)
                {
                    if (!item.Contains("J"))
                    {
                        continue;
                    }
                    var model = new ReportController().FindBillNo(item, "3", "", null);
                    if (model != null)
                    {
                        ModelList.Add(model);
                    }
                }
            }
            ModelList = ModelList.Where(c => c.ApprovalStatus == 2).ToList();
            if (ModelList.Count > 0)
            {
                foreach (var item in ModelList)
                {
                    item.Creator = AjaxGetName(item.Creator);
                }
                return JsonSerializeHelper.SerializeToJson(ModelList);
            }
            else
            {
                return "[]";
            }
        }


        /// <summary>
        /// 关联操作
        /// </summary>
        /// <param name="BorrowNo"></param>
        /// <param name="FeeNo"></param>
        /// <returns></returns>
        public string ReationOperation(string BorrowNo, string FeeNo)
        {
            string Result = "";
            var FeeModel = new FeeBill().GetBillModel(FeeNo);
            if (FeeModel == null)
            {
                Result = "费用单号不存在";
                return Result;
            }
            if (FeeModel.ApprovalStatus == 2)
            {
                Result = "不能关联已通过的费用单";
                return Result;
            }
            var BorrowModel = new BorrowBill().GetBillModel(BorrowNo);
            if (BorrowModel == null)
            {
                Result = "借款单号不存在";
                return Result;
            }
            RefundBillModel model = FeeModel.MapTo<FeeBillModel, RefundBillModel>();
            model.Creator = BorrowModel.Creator;
            model.Owner = BorrowModel.Owner;
            model.WorkNumber = BorrowModel.WorkNumber;
            model.PersonInfo = BorrowModel.PersonInfo;
            model.BorrowBillNo = BorrowModel.BillNo;
            model.COST_ACCOUNT = BorrowModel.COST_ACCOUNT;
            model.RefundType = "FeeBill";
            model.RealRefundMoney = FeeModel.TotalMoney;

            //插入费用还款单
            new RefundBill().InsertOneData(model);

            //删除费用单
            Result = new FeeBill().DelePhysicsNo(FeeModel.BillNo);
            return Result;
        }


        public string GetPersonBorrowBill(string No)
        {
            var Model = new BorrowBill().GetBillForAll(No);

            if (Model.Count > 0)
            {
                return Public.JsonSerializeHelper.SerializeToJson(Model);
            }
            else
            {
                return "[]";
            }
        }

        public string EditApprovalTime(string time, string BillNo, string Type)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ApprovalTime", time);
            string str = String.Empty;
            DateTime APPTime = Convert.ToDateTime(time);
            switch (Type.ToUpper())
            {
                case "FEEBILL":
                    str = new FeeBill().PublicEditMethod(BillNo, dic);
                    var s1 = DbContext.FEE_FEEBILL.Where(c => c.BILLNO == BillNo).FirstOrDefault();
                    s1.APPROVALTIME = APPTime;

                    var entry1 = DbContext.Entry<FEE_FEEBILL>(s1);
                    entry1.State = System.Data.EntityState.Modified;
                    entry1.Property("APPROVALTIME").IsModified = true;

                    break;
                case "NOTICEBILL":
                    str = new NoticeBill().PublicEditMethod(BillNo, dic);

                    var s2 = DbContext.FEE_NOTICEBILL.Where(c => c.BILLNO == BillNo).FirstOrDefault();
                    s2.APPROVALTIME = APPTime;

                    var entry2 = DbContext.Entry<FEE_NOTICEBILL>(s2);
                    entry2.State = System.Data.EntityState.Modified;
                    entry2.Property("APPROVALTIME").IsModified = true;

                    break;
                case "BORROWBILL":
                    str = new BorrowBill().PublicEditMethod(BillNo, dic);

                    var s3 = DbContext.FEE_BORROWBILL.Where(c => c.BILLNO == BillNo).FirstOrDefault();
                    s3.APPROVALTIME = APPTime;

                    var entry3 = DbContext.Entry<FEE_BORROWBILL>(s3);
                    entry3.State = System.Data.EntityState.Modified;
                    entry3.Property("APPROVALTIME").IsModified = true;

                    break;
                case "REFUNDBILL":
                    str = new RefundBill().PublicEditMethod(BillNo, dic);


                    if (BillNo.Contains("HK"))
                    {
                        var s4 = DbContext.FEE_CASHREFUNDBILL.Where(c => c.BILLNO == BillNo).FirstOrDefault();
                        s4.APPROVALTIME = APPTime;

                        var entry4 = DbContext.Entry<FEE_CASHREFUNDBILL>(s4);
                        entry4.State = System.Data.EntityState.Modified;
                        entry4.Property("APPROVALTIME").IsModified = true;

                    }
                    else
                    {
                        var s5 = DbContext.FEE_FEEREFUNDBILL.Where(c => c.BILLNO == BillNo).FirstOrDefault();
                        s5.APPROVALTIME = APPTime;

                        var entry5 = DbContext.Entry<FEE_FEEREFUNDBILL>(s5);
                        entry5.State = System.Data.EntityState.Modified;
                        entry5.Property("APPROVALTIME").IsModified = true;

                    }

                    break;
                default:
                    break;
            }
            DbContext.SaveChanges();
            return str;
        }


        //检测销售标题
        public bool ExamineSaleExcleHead(Workbook workbook)
        {
            Cells cells = workbook.Worksheets[0].Cells;
            int num = cells.MaxDisplayRange.ColumnCount;
            if (num != 6)
            {
                return false;
            }

            return "品牌".Equals(cells[0, 0].StringValue.Trim()) && "报销方".Equals(cells[0, 1].StringValue.Trim()) && "业务日期".Equals(cells[0, 2].StringValue.Trim()) && "费用项".Equals(cells[0, 3].StringValue.Trim()) && "金额".Equals(cells[0, 4].StringValue.Trim()) && "事由".Equals(cells[0, 5].StringValue.Trim());
        }

        //获取
        public string GetNoticeList()
        {
            List<string> obj = new List<string>();

            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "Notice");
            DirectoryInfo dic = new DirectoryInfo(localPath);
            var info = dic.EnumerateFiles();
            foreach (var item in info)
            {
                obj.Add(item.Name);
            }

            return Public.JsonSerializeHelper.SerializeToJson(obj);
        }


        public string DelNotice(string Link)
        {
            string error = string.Empty;
            string msg = string.Empty;

            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "Notice", Link);

            bool IsExists = System.IO.File.Exists(localPath);
            if (IsExists == false)
            {
                error = "1";
                msg = "文件不存在";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            System.IO.File.Delete(localPath);
            error = "0";
            msg = "删除成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }

        [HttpPost]
        public string UpLoadNotice()
        {
            string msg = string.Empty;
            string error = string.Empty;

            try
            {
                var file = HttpContext.Request.Files[0];
                string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "Notice");
                if (!System.IO.Directory.Exists(localPath))
                {
                    System.IO.Directory.CreateDirectory(localPath);
                }
                if (HttpContext.Request.Files.Count == 0)
                {
                    error = "1";
                    msg = "文件为空";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
                //检验原始名称是否被占用
                bool IsExists = System.IO.File.Exists(Path.Combine(localPath, file.FileName));
                if (IsExists)
                {
                    error = "1";
                    msg = "文件名已存在，请更换文件名";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }

                //保存源文件
                file.SaveAs(Path.Combine(localPath, file.FileName));
                error = "0";
                msg = "保存成功";
            }
            catch (Exception)
            {
                error = "1";
                msg = "系统错误";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        [HttpPost]
        public ActionResult UploadSaleExcel()
        {
            var file = HttpContext.Request.Files[0];

            string msg = string.Empty;
            string error = string.Empty;

            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                //读取Excel内容
                Workbook workbook = new Workbook(file.InputStream);

                if (file.ContentLength <= 0)
                {
                    error = "请选择Excel文件！";
                    return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                }

                bool Istrue = ExamineSaleExcleHead(workbook);
                if (Istrue == false)
                {
                    error = "标题行不准确！";
                    return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                }
                Cells cells = workbook.Worksheets[0].Cells;

                List<FeeBillModel> List = new List<FeeBillModel>();
                for (int i = 1; i < cells.MaxDataRow + 1; i++)
                {
                    FeeBillModel Model = new FeeBillModel();

                    Model.Creator = employee.EmployeeNo;
                    Model.WorkNumber = employee.EmployeeNo;
                    Model.Owner = AjaxGetName(employee.EmployeeNo);
                    Model.CreateTime = DateTime.Now;

                    string str1 = cells[i, 1].StringValue.Trim();
                    var department = DbContext.DEPARTMENT.Where(p => p.CODE == str1).FirstOrDefault();
                    if (department == null)
                    {
                        error = "第" + i + "行片区编码数据有误！";
                        return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                    Model.DepartmentID = department.ID.ToString();

                    var COST_ACCOUNT = DbContext.FEE_SHOP_COSTCENTER.Where(p => p.SHOPCODE == str1).Select(x => x.COSTCODE).FirstOrDefault();
                    Model.COST_ACCOUNT = COST_ACCOUNT;
                    Model.DepartmentName = department.NAME;
                    var time = Convert.ToDateTime(cells[i, 2].StringValue.Trim());
                    if (time == new DateTime())
                    {
                        error = "第" + i + "行业务时间有误！";
                        return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                    Model.TransactionDate = time;
                    Model.Remark = cells[i, 5].StringValue.Trim();
                    Model.Status = 0;
                    var money = Convert.ToDecimal(cells[i, 4].StringValue.Trim());
                    Model.TotalMoney = money;

                    var name = cells[i, 3].StringValue.Trim();

                    Model.Items = new List<FeeBillItemModel>() { new FeeBillItemModel() { rowid = "1", money = money, name = name, taxcode = "2221010200", taxmoney = 0 } };
                    Model.Photos = new List<PhotoModel>() { new PhotoModel() { filename = "Images/img_load_error.png", OriginalUrl = "http://210.75.13.196/Marisfrolg.Fee/Images/img_load_error.png", url = "http://210.75.13.196/Marisfrolg.Fee/Images/img_load_error.png" } };

                    var StrBrand = cells[i, 0].StringValue.Trim();
                    if (StrBrand == "MF")
                    {
                        StrBrand = "MA";
                    }

                    Model.PersonInfo = new PersonInfo() { IsHeadOffice = 0, Brand = new List<string>() { StrBrand }, Department = department.NAME, DepartmentCode = department.CODE, Company = department.COMPANY.NAME, CompanyCode = department.COMPANYCODE, CostCenter = COST_ACCOUNT };
                    Model.SpecialAttribute = new SpecialAttribute();
                    Model.BillsItems = Model.Items;
                    Model.ApprovalStatus = 2;
                    Model.Currency = new Currency() { Code = "CNY", Name = "元" };
                    Model.ApprovalTime = DateTime.Now.ToString("yyyy-MM-dd");
                    Model.BillsType = "FY0";
                    List.Add(Model);
                    DbContext.Configuration.AutoDetectChangesEnabled = false;
                }
                if (List.Count > 0)
                {
                    foreach (var item in List)
                    {
                        new FeeBill().CreateFeeBill(item);
                    }
                }

                msg = "上传成功！";
            }
            catch (DbEntityValidationException dbEx)
            {
                throw;
            }
            return Json(new { error = error, msg = msg }, "text/html", JsonRequestBehavior.AllowGet);
        }


        public string GetPersonBankInfo()
        {
            var model = DbContext.FEE_PERSONINFO.ToList();
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        public string DelOrQueryData(string Type, string No)
        {
            if (Type.ToUpper() == "EDIT")
            {
                var model = DbContext.FEE_PERSONINFO.Where(c => c.NO == No).FirstOrDefault();
                return Public.JsonSerializeHelper.SerializeToJson(model);
            }
            else if (Type.ToUpper() == "DEL")
            {
                var model = DbContext.FEE_PERSONINFO.Where(c => c.NO == No).FirstOrDefault();
                DbContext.FEE_PERSONINFO.Remove(model);
                DbContext.SaveChanges();
                return "Success";
            }
            return "";
        }


        public string AddtBankInfo(FEE_PERSONINFO Model)
        {
            string error = string.Empty;
            string msg = string.Empty;

            var p = DbContext.EMPLOYEE.Where(c => c.NO == Model.NO && c.AVAILABLE == "1" && c.LEAVE == "0").FirstOrDefault();
            if (p == null)
            {
                error = "1";
                msg = "该工号不存在";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            var Fp = DbContext.FEE_PERSONINFO.Where(c => c.NO == Model.NO).FirstOrDefault();
            if (Fp != null)
            {
                error = "1";
                msg = "该工号已存在数据";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            var maxId = DbContext.FEE_PERSONINFO.Max(c => c.ID);
            Model.ID = maxId + 1;
            DbContext.FEE_PERSONINFO.Add(Model);
            DbContext.SaveChanges();
            error = "0";
            msg = "添加成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        public string EditBankInfo(FEE_PERSONINFO Model)
        {
            string error = string.Empty;
            string msg = string.Empty;

            var PM = DbContext.FEE_PERSONINFO.Where(c => c.NO == Model.NO).FirstOrDefault();

            PM.NAME = Model.NAME;
            PM.POSITION = Model.POSITION;
            PM.OPENBANK = Model.OPENBANK;
            PM.OPENCITY = Model.OPENCITY;
            PM.BANKACCOUNT = Model.BANKACCOUNT;

            this.DbContext.Entry(PM).State = EntityState.Modified;
            DbContext.SaveChanges();
            error = "0";
            msg = "修改成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        public string AddDepartCosterCenter(string departId, string value)
        {
            string error = string.Empty;
            string msg = string.Empty;

            //判断成本中心是否有效
            bool IsOk = JudgeCosterCenter(value);
            if (IsOk == false)
            {
                error = "1";
                msg = "成本中心字符串有误";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            string str = GetsingleCosterCenter(departId);
            string sql = String.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                value = str + "," + value;
                sql = "update  FEE_COSTERCENTER_EXTEND set COSTERCENTER='" + value + "' where DEPARTMENTID='" + departId + "'";
            }
            else
            {
                sql = "insert into FEE_COSTERCENTER_EXTEND (DEPARTMENTID,COSTERCENTER,TYPE) values('" + departId + "','" + value + "','D') ";
            }

            int id = DbContext.Database.ExecuteSqlCommand(sql);
            if (id > 0)
            {
                error = "0";
                msg = "添加成功";
            }
            else
            {
                error = "1";
                msg = "添加失败";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        public string EditDepartCosterCenter(string departId, string value)
        {
            string error = string.Empty;
            string msg = string.Empty;

            //判断成本中心是否有效
            bool IsOk = JudgeCosterCenter(value);
            if (IsOk == false)
            {
                error = "1";
                msg = "成本中心字符串有误";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }


            var T_value = GetsingleCosterCenter(departId);
            string sql = string.Empty;

            if (string.IsNullOrEmpty(T_value))
            {
                sql = "insert into FEE_COSTERCENTER_EXTEND (DEPARTMENTID,COSTERCENTER,TYPE) values('" + departId + "','" + value + "','D') ";
            }
            else
            {
                sql = "update  FEE_COSTERCENTER_EXTEND set COSTERCENTER='" + value + "' where DEPARTMENTID='" + departId + "'";
            }
            int id = DbContext.Database.ExecuteSqlCommand(sql);
            if (id > 0)
            {
                error = "0";
                msg = "编辑成功";
            }
            else
            {
                error = "1";
                msg = "编辑失败";
            }

            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        /// <summary>
        /// 判断成本中心串是否符合规则
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool JudgeCosterCenter(string value)
        {
            var list = value.Split(',').ToList();
            list.Remove("");

            string str = String.Empty;

            foreach (var item1 in list)
            {
                str += "'" + ConvertAccountCode(item1) + "',";
            }
            str = str.Remove(str.Length - 1);

            string sql2 = "select distinct(KOSTL) as CODE from FEE_SAPDATA where KOSTL in (" + str + ")";
            var Database2 = DbContext.Database.SqlQuery<ObjectList>(sql2).ToList();
            if (list.Count != Database2.Count)
            {
                return false;
            }
            return true;
        }


        public string GetDepartCostercenterExtend()
        {
            string sql = "select a.Id as DEPARTMENTID,a.NAME,a.COMPANYCODE,b.COSTERCENTER from department a left  join FEE_COSTERCENTER_EXTEND b on a.id=b.DEPARTMENTID where a.is_ucstar=2 and a.name not like '%片区%' and a.pid not in       (3775,4067,3656,3680,3840,3702,3726,3842,3914,4445)";
            var Database = DbContext.Database.SqlQuery<DepartCosterCenter>(sql).ToList();

            foreach (var item in Database)
            {
                if (string.IsNullOrEmpty(item.COSTERCENTER))
                {
                    continue;
                }
                var list = item.COSTERCENTER.Split(',').ToList();
                list.Remove("");

                string str = String.Empty;

                foreach (var item1 in list)
                {
                    str += "'" + ConvertAccountCode(item1) + "',";
                }
                str = str.Remove(str.Length - 1);

                string sql2 = "select KOSTL as CODE, names as NAME from FEE_SAPDATA where KOSTL in (" + str + ")";
                var Database2 = DbContext.Database.SqlQuery<ObjectList>(sql2).ToList();

                string strNew = String.Empty;

                foreach (var item1 in Database2)
                {
                    strNew += item1.CODE + "(" + item1.NAME + "),";
                }

                strNew = strNew.Remove(strNew.Length - 1);
                item.COSTERCENTER = strNew;
            }

            return Public.JsonSerializeHelper.SerializeToJson(Database);
        }


        public string GetsingleCosterCenter(string Id)
        {
            string sql = "SELECT COSTERCENTER FROM FEE_COSTERCENTER_EXTEND where DEPARTMENTID='" + Id + "'";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            return Database;
        }


        public string ConvertAccountCode(string CosterCenter)
        {
            if (string.IsNullOrEmpty(CosterCenter))
            {
                return "";
            }
            //判断是否包含字母，如果包含，则返回原值
            string pattern = @"[a-zA-Z]";
            bool Istrue = System.Text.RegularExpressions.Regex.IsMatch(CosterCenter, pattern);
            if (Istrue)
            {
                return CosterCenter;
            }
            string str = CosterCenter.Remove(1);
            int i = 0;
            bool IsNumber = int.TryParse(CosterCenter.Remove(1), out i);
            //整合8位的成本中心
            if (str != "0" && CosterCenter.Length == 8 && IsNumber)
            {
                CosterCenter = "00" + CosterCenter;
            }
            else if (str != "0" && CosterCenter.Length == 6 && IsNumber)
            {
                CosterCenter = "0000" + CosterCenter;
            }
            return CosterCenter;
        }


        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <returns></returns>
        public string GetOracleConfigurationInfo(string role)
        {
            string error = String.Empty;
            string msg = String.Empty;

            string sql = string.Empty;
            switch (role.ToLower())
            {
                case "system":
                    sql = "SELECT ID as c1,DEPARTMENTNAME as c2,brand as c3,value as c4 FROM FEE_PERSON_EXTEND where TYPE='system'";
                    break;
                case "extradata":
                    sql = "SELECT  a.ID as c1,a.DEPARTMENTNAME as c2,B.name as c3,d.name as c4,a.feeinfo as c5,c.name as c6 FROM FEE_PERSON_EXTEND a left join department b on a.DEPARTMENTCODE=b.ID left join employee c on a.value=c.no left join FEE_ACCOUNT_DICTIONARY d on a.fytype=d.code and d.brand='MF' where a.TYPE='extradata'   ORDER by a.DEPARTMENTNAME desc";
                    break;
                case "company":
                    sql = "SELECT ID as c1,value as c2 from FEE_PERSON_EXTEND where TYPE='company' and  DEPARTMENTNAME='BaseController'";
                    break;
                case "items":
                    sql = "SELECT  ID as c1,PARAMETERONE as c2,VALUE as c3 FROM FEE_PERSON_EXTEND where TYPE='Items' ";
                    break;
                case "employeeno":
                    sql = "SELECT  a.ID as c1,a.DEPARTMENTNAME as c2,b.name as c3,a.PARAMETERTWO as c4,a.value as c5,c.name as c6 FROM FEE_PERSON_EXTEND a left join department b on a.departmentcode=b.id left join employee c on a.parameterone=c.no where a.TYPE='EmployeeNo'";
                    break;
                case "sufob":
                    sql = "SELECT a.ID as c1,a.DEPARTMENTNAME as c2,b.name as c3,value as c4,c.name as c5  FROM FEE_PERSON_EXTEND a left join department b on a.DEPARTMENTCODE=b.id left join employee c on a.PARAMETERONE=c.no where a.TYPE='SuFOB'";
                    break;
                case "brandpost":
                    sql = "select ID as c1,BRAND as c2,DEPARTMENTCODE as c3,value as c4 from FEE_PERSON_EXTEND where TYPE='BrandPost'";
                    break;
                case "keepmanager":
                    sql = "select ID as c1,BRAND as c2 from FEE_PERSON_EXTEND where TYPE='KeepManager'";
                    break;
                case "paymentcode":
                    sql = "select ID as c1,COMPANYCODE as c2,value as c3 from FEE_PERSON_EXTEND where type='PaymentCode' ";
                    break;
                default:
                    break;
            }
            var Database = DbContext.Database.SqlQuery<PublicClass>(sql).ToList();
            if (Database.Count > 0)
            {
                var model = TransformData(role, Database);
                error = "0";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg, data = model });
            }
            else
            {
                error = "1";
                msg = "系统错误";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
        }



        public string GetPersonList(string personList)
        {
            string str = string.Empty;

            personList = TransSqlData(personList);

            string sql = "select name from employee where no in(" + personList + ")";

            var NewList = DbContext.Database.SqlQuery<string>(sql).ToList();

            foreach (var item in NewList)
            {
                str += item.Split('-')[0] + ",";
            }
            str = str.Remove(str.Length - 1);
            return str;
        }

        public string GetDepartmentList(string departList)
        {
            string str = string.Empty;

            departList = TransSqlData(departList);

            string sql = "select name from department where ID in(" + departList + ")";

            var NewList = DbContext.Database.SqlQuery<string>(sql).ToList();

            foreach (var item in NewList)
            {
                str += item + ",";
            }
            str = str.Remove(str.Length - 1);
            return str;
        }


        private ReportModel TransformData(string role, List<PublicClass> list)
        {
            ReportModel result = new ReportModel();
            result.Rows = new List<object[]>();

            switch (role.ToLower())
            {
                case "system":
                    result.Columns = new List<string>() { "ID", "岗位", "品牌", "审核人列表", "操作" };

                    foreach (var item1 in list)
                    {
                        var names = GetPersonList(item1.c4);
                        object[] obj = new object[4] { item1.c1, item1.c2, item1.c3, names };
                        result.Rows.Add(obj);
                    }
                    break;
                case "extradata":
                    result.Columns = new List<string>() { "ID", "指定岗位", "部门名称", "审批岗", "费用项", "指定人员", "操作" };

                    foreach (var item1 in list)
                    {
                        object[] obj = new object[6] { item1.c1, item1.c2, item1.c3, item1.c4, item1.c5, item1.c6 };
                        result.Rows.Add(obj);
                    }

                    break;
                case "company":
                    result.Columns = new List<string>() { "ID", "父级部门(审核权限下放至下级部门)", "操作" };
                    foreach (var item1 in list)
                    {
                        var name = GetDepartmentList(item1.c2);
                        object[] obj = new object[2] { item1.c1, name };
                        result.Rows.Add(obj);
                    }
                    break;
                case "items":
                    result.Columns = new List<string>() { "ID", "类型", "对应值", "操作" };
                    foreach (var item1 in list)
                    {
                        object[] obj = new object[3] { item1.c1, item1.c2, item1.c3 };
                        result.Rows.Add(obj);
                    }
                    break;
                case "employeeno":
                    result.Columns = new List<string>() { "ID", "指定岗位", "部门名称", "指定费用项", "指定人员", "指定审核人", "操作" };
                    foreach (var item1 in list)
                    {
                        var names = GetPersonList(item1.c5);
                        object[] obj = new object[6] { item1.c1, item1.c2, item1.c3, item1.c4, names, item1.c6 };
                        result.Rows.Add(obj);
                    }
                    break;
                case "sufob":

                    result.Columns = new List<string>() { "ID", "指定岗位", "部门名称", "指定人员", "指定审核人", "操作" };
                    foreach (var item1 in list)
                    {
                        var names = GetPersonList(item1.c4);
                        object[] obj = new object[5] { item1.c1, item1.c2, item1.c3, names, item1.c5 };
                        result.Rows.Add(obj);
                    }
                    break;
                case "brandpost":
                    result.Columns = new List<string>() { "ID", "品牌", "审核部门", "审核人员", "操作" };
                    foreach (var item1 in list)
                    {
                        var department = GetDepartmentList(item1.c3);
                        var names = GetPersonList(item1.c4);
                        object[] obj = new object[4] { item1.c1, item1.c2, department, names };
                        result.Rows.Add(obj);
                    }
                    break;

                case "keepmanager":
                    result.Columns = new List<string>() { "ID", "品牌", "操作" };
                    foreach (var item1 in list)
                    {
                        object[] obj = new object[2] { item1.c1, item1.c2 };
                        result.Rows.Add(obj);
                    }
                    break;
                case "paymentcode":
                    result.Columns = new List<string>() { "ID", "所属付款公司代码", "成本中心", "操作" };
                    foreach (var item1 in list)
                    {
                        object[] obj = new object[3] { item1.c1, item1.c2, item1.c3 };
                        result.Rows.Add(obj);
                    }
                    break;
                default:
                    break;
            }

            return result;
        }


        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Role"></param>
        /// <returns></returns>
        public string GetConfigEditValue(string ID, string Role)
        {
            string sql = String.Empty;
            switch (Role.ToLower())
            {
                case "extradata":
                    sql = "select value from FEE_PERSON_EXTEND where id=" + ID + "";
                    break;
                case "employeeno":
                    sql = "select  PARAMETERONE from FEE_PERSON_EXTEND where id=" + ID + "";
                    break;
                case "sufob":
                    sql = "select  PARAMETERONE from FEE_PERSON_EXTEND where id=" + ID + "";
                    break;
                default:
                    break;
            }
            var database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            return database;
        }


        public string PublicConfigEditMethod(string ID, string Value, string Role)
        {
            string error = string.Empty;
            string msg = string.Empty;

            bool IsTrue = VerificateData(Value, Role);
            if (IsTrue == false)
            {
                error = "1";
                msg = "数据错误";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            string sql = string.Empty;
            switch (Role.ToLower())
            {
                case "system":
                    sql = "update FEE_PERSON_EXTEND set VALUE=(VALUE||'," + Value + "') where id=" + ID + "";
                    break;
                case "extradata":
                    sql = "update FEE_PERSON_EXTEND set VALUE='" + Value + "' where id=" + ID + "";
                    break;
                case "company":
                    sql = "update FEE_PERSON_EXTEND set VALUE=(VALUE||'," + Value + "') where id=" + ID + "";
                    break;
                case "items":
                    sql = "update FEE_PERSON_EXTEND set VALUE=(VALUE||'," + Value + "') where id=" + ID + "";
                    break;
                case "employeeno":
                    sql = "update FEE_PERSON_EXTEND set PARAMETERONE='" + Value + "' where id=" + ID + "";
                    break;
                case "sufob":
                    sql = "update FEE_PERSON_EXTEND set PARAMETERONE='" + Value + "' where id=" + ID + "";
                    break;
                case "paymentcode":
                    sql = "update FEE_PERSON_EXTEND set VALUE=(VALUE||'," + Value + "') where id=" + ID + "";
                    break;
                default:
                    break;
            }

            int count = DbContext.Database.ExecuteSqlCommand(sql);
            if (count > 0)
            {
                error = "0";
                msg = "保存成功";
            }
            else
            {
                error = "1";
                msg = "保存失败";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        private bool VerificateData(string value, string role)
        {
            bool Istrue = true;
            switch (role.ToLower())
            {
                case "company":
                    var temp = Convert.ToDecimal(value);
                    var model1 = DbContext.DEPARTMENT.Where(c => c.ID == temp && c.AVAILABLE == "1").FirstOrDefault();
                    if (model1 == null)
                    {
                        Istrue = false;
                    }
                    break;
                case "items":
                    var model2 = DbContext.FEE_ACCOUNT.Where(c => c.NAME == value).FirstOrDefault();
                    if (model2 == null)
                    {
                        Istrue = false;
                    }
                    break;
                case "paymentcode":
                    if (value.Length <= 2)
                    {
                        Istrue = false;
                        break;
                    }
                    string str = value.Remove(2);
                    int i = 0;
                    bool IsNumber = int.TryParse(value.Remove(1), out i);
                    //整合8位的成本中心
                    if (str != "00" && value.Length == 8 && IsNumber)
                    {
                        value = "00" + value;
                    }
                    string sql = "select BUKRS as CODE,names as NAME from FEE_SAPDATA where KOSTL='" + value + "'";
                    var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();
                    if (Database == null)
                    {
                        Istrue = false;
                    }
                    break;
                default:
                    var model3 = DbContext.EMPLOYEE.Where(c => c.NO == value && c.LEAVE == "0" && c.AVAILABLE == "1").FirstOrDefault();
                    if (model3 == null)
                    {
                        Istrue = false;
                    }
                    break;
            }
            return Istrue;
        }


        public string GetAreaExpenses()
        {
            string sql = "select a.ID||'' as c1,a.brand as c2,a.name as c3,case when  a.PARTBRAND='0' then '否' else '是' end as c4,a.BRANDLIST as c5,b.name as c6,c.name as c7,a.BRANDPOST as c8  from FEE_ACCOUNT_DICTIONARY a left join employee b on  a.EXTRAMODEONE=b.NO left join employee c on a.EXTRAMODETWO=c.NO";

            var database = DbContext.Database.SqlQuery<PublicClass>(sql).ToList();

            var list = database.Where(c => !string.IsNullOrEmpty(c.c8)).ToList();

            foreach (var item in list)
            {
                var name = GetPersonList(item.c8);
                item.c8 = name;
            }

            return Public.JsonSerializeHelper.SerializeToJson(database);
        }
    }
}
