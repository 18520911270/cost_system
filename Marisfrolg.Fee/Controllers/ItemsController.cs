using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using System.IO;
using Aspose.Cells;
using WorkFlowModel;

namespace Marisfrolg.Fee.Controllers
{
    public class ItemsController : SecurityController 
    {
        //
        // GET: /Items/

        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 获取行项目扩展信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetNamesLevel(string name)
        {
            string sql = "SELECT  VALUE as CODE,PARAMETERONE as NAME, VALUE FROM FEE_PERSON_EXTEND where TYPE='Items' and VALUE like '%" + name + "%'";
            var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();
            if (Database == null)
            {
                return "";
            }
            var list = Database.CODE.Split(',').ToList();
            list.Remove("");

            foreach (var item in list)
            {
                if (item == name)
                {
                    return Database.NAME;
                }
            }
            return "";
        }


        public string GetBankData()
        {
            var model = DbContext.FEE_BANK.Select(x => x.ACCOUNTNAME).ToList();

            List<string> str = new List<string>();

            foreach (var item in model)
            {
                str.Add(item.Replace("银行存款-", ""));
            }
            str = str.Distinct().ToList();

            return Public.JsonSerializeHelper.SerializeToJson(str);
        }


        public string CheckBankData(string b1, string b2)
        {
            string error = string.Empty;
            string msg = string.Empty;

            string newB1 = "银行存款-" + b1;
            string newB2 = "银行存款-" + b2;

            var m1 = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME == b1 || c.ACCOUNTNAME == newB1).FirstOrDefault();
            if (m1 == null)
            {
                error = "1";
                msg = "请下拉选择转出银行！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            var m2 = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME == b2 || c.ACCOUNTNAME == newB2).FirstOrDefault();
            if (m2 == null)
            {
                error = "1";
                msg = "请下拉选择转入银行！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            error = "0";
            msg = "成功！";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        public string GetErrorModel()
        {
            string error = string.Empty;
            string msg = string.Empty;
            ErrorBill Error = new ErrorBill();
            List<FeeBillModelRef> modelList = new List<FeeBillModelRef>();
            List<string> DelStr = new List<string>();
            List<string> StrList = new List<string>();
            List<ErrorQueue> IErrorList = new List<ErrorQueue>();

            var file = HttpContext.Request.Files;
            if (file.Count > 0 && file[0].ContentLength > 0)
            {
                //读取文件
                Workbook workbook = new Workbook(file[0].InputStream);
                Cells cells = workbook.Worksheets[0].Cells;
                for (int i = 1; i < cells.MaxDataRow + 1; i++)  //第一行标题
                {
                    StrList.Add(cells[i, 0].StringValue.Trim());
                }
            }
            var List = Error.GetAllData();

            string url = "http://192.168.2.14//WorkFlowServer/WorkFlowServer" + "/GetErrorQueue";

            string postDataStr = string.Format("Type={0}", "Fee");

            string jsonList = this.HttpGet(url, postDataStr);

            IErrorList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ErrorQueue>>(jsonList);

            foreach (var item in List)
            {
                var temp = ConvertData(item.WorkFlowID);
                if (temp == null || temp.Status == 1 || temp.PersonInfo == null)
                {
                    DelStr.Add(item.WorkFlowID);
                }
                else
                {
                    modelList.Add(temp);
                }
            }
            foreach (var item in StrList)
            {
                var temp = ConvertData(item);
                if (temp == null || temp.Status == 1 || temp.PersonInfo == null)
                {

                }
                else
                {
                    modelList.Add(temp);
                }
            }

            foreach (var item in IErrorList)
            {
                var temp = ConvertData(item.WorkFlowId);
                if (temp == null || temp.Status == 1 || temp.PersonInfo == null)
                {

                }
                else
                {
                    modelList.Add(temp);
                }
            }

            foreach (var item in DelStr)
            {
                Error.DeleteErrorData(item);
            }

            error = "0";
            msg = "查询成功";

            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg, data = modelList });
        }


        public string DelErrorModel(string wkID)
        {
            var IsTrue = new ErrorBill().DeleteErrorData(wkID);
            return IsTrue == true ? "Success" : "Fail";
        }


        private FeeBillModelRef ConvertData(string wkID)
        {
            FeeBillModelRef model = new FeeBillModelRef();

            var FeeModel = new FeeBill().GetModelFromWorkFlowID(wkID);
            if (FeeModel != null)
            {
                model = FeeModel.MapTo<FeeBillModel, FeeBillModelRef>();
                model.PageName = "FeeBill";
                return model;
            }
            var NotcieModel = new NoticeBill().GetModelFromWorkFlowID(wkID);
            if (NotcieModel != null)
            {
                model.BillNo = NotcieModel.BillNo;
                model.PersonInfo = NotcieModel.PersonInfo;
                model.TotalMoney = NotcieModel.TotalMoney;
                model.CreateTime = NotcieModel.CreateTime;
                model.TransactionDate = NotcieModel.TransactionDate;
                model.ApprovalTime = NotcieModel.ApprovalTime;
                model.ApprovalStatus = NotcieModel.ApprovalStatus;
                model.ApprovalPost = NotcieModel.ApprovalPost;
                model.Remark = NotcieModel.Remark;
                model.ProviderName = NotcieModel.ProviderInfo.ProviderName;
                model.Owner = NotcieModel.Owner;
                model.Creator = NotcieModel.Creator;
                model.SpecialAttribute = new SpecialAttribute();
                model.SpecialAttribute.Funds = NotcieModel.SpecialAttribute.Funds;
                model.SpecialAttribute.Agent = NotcieModel.SpecialAttribute.Agent;
                model.SpecialAttribute.Check = NotcieModel.SpecialAttribute.Check;
                model.Items = NotcieModel.Items;
                model.Currency = NotcieModel.Currency;
                model.WorkNumber = NotcieModel.WorkNumber;
                model.ProviderCode = NotcieModel.ProviderInfo.ProviderCode;
                model.Owner = NotcieModel.Owner;
                model.MissBill = NotcieModel.MissBill;
                model.WorkFlowID = NotcieModel.WorkFlowID;
                model.CountTime = new CountTime();
                model.Status = NotcieModel.Status;
                model.PageName = "NoticeBill";
                return model;
            }
            var BorrowModel = new BorrowBill().GetModelFromWorkFlowID(wkID);
            if (BorrowModel != null)
            {
                model.BillNo = BorrowModel.BillNo;
                model.PageName = "BorrowBill";
                model.PersonInfo = BorrowModel.PersonInfo;
                model.TotalMoney = BorrowModel.TotalMoney;
                model.CreateTime = BorrowModel.CreateTime;
                model.TransactionDate = BorrowModel.TransactionDate;
                model.ApprovalTime = BorrowModel.ApprovalTime;
                model.ApprovalStatus = BorrowModel.ApprovalStatus;
                model.ApprovalPost = BorrowModel.ApprovalPost;
                model.Remark = BorrowModel.Remark;
                model.Owner = BorrowModel.Owner;
                model.Creator = BorrowModel.Creator;
                model.SpecialAttribute = BorrowModel.SpecialAttribute;
                model.Items = BorrowModel.Items;
                model.Currency = BorrowModel.Currency;
                model.WorkNumber = BorrowModel.WorkNumber;
                model.Owner = BorrowModel.Owner;
                model.WorkFlowID = BorrowModel.WorkFlowID;
                model.CollectionInfo = BorrowModel.CollectionInfo;
                model.CountTime = new CountTime();
                model.Status = BorrowModel.Status;
                return model;
            }
            var RefundModel = new RefundBill().GetModelFromWorkFlowID(wkID);
            if (RefundModel != null)
            {
                model = RefundModel.MapTo<RefundBillModel, FeeBillModelRef>();
                model.PageName = "RefundBill";
                return model;
            }
            return model;
        }


        public string DownLoadFeeAccount()
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            MemoryCachingClient M = new MemoryCachingClient();
            List<PublicClass> dt = M.GetData(employee.EmployeeNo) as List<PublicClass>;


            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格


            List<string> Title = new List<string>() { "费用大类", "费用项", "审批岗", "是否可见", "是否考核", "科目号", "排序号", "审核人员" };

            int Colnum = Title.Count;//表格列数
            int Rownum = dt.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(Title[i]);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                cells[1 + i, 0].PutValue(dt[i].c1);
                cells[1 + i, 1].PutValue(dt[i].c2);
                cells[1 + i, 2].PutValue(dt[i].c3);
                cells[1 + i, 3].PutValue(dt[i].c4);
                cells[1 + i, 4].PutValue(dt[i].c5);
                cells[1 + i, 5].PutValue(dt[i].c6);
                cells[1 + i, 6].PutValue(dt[i].c7);
                cells[1 + i, 7].PutValue(dt[i].c8);
            }

            string NewlocalPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "ExcelDownLoad");
            if (!System.IO.Directory.Exists(NewlocalPath))
            {
                System.IO.Directory.CreateDirectory(NewlocalPath);
            }
            string filePathName = "费用科目明细" + '-' + DateTime.Now.ToString("yyMMddhhmmss") + ".xls";
            workbook.Save(Path.Combine(NewlocalPath, filePathName));
            return filePathName;
        }

        public string GetFeeItems()
        {
            string sql = "select c.name as c1,c.code as c10, a.name as c2,b.name as c3, CASE when a.hide=1 then '都可见' when a.hide=0 then '总部可见片区不可见'  else '片区可见总部不可见' end as c4,CASE  when a.IS_MARKET=1 then '考核' else '不考核' end as c5,a.ACCOUNT as c6,a.sort||'' as c7,a.id||'' as c9 from fee_account a left join FEE_ACCOUNT_DICTIONARY b on a.WORKFLOW_CODE=b.code and b.brand='MF' left join fee_account_type c on a.account_type=c.code  order by a.account_type";

            var Database = DbContext.Database.SqlQuery<PublicClass>(sql).ToList();

            var list = Database.GroupBy(c => c.c3).ToList();

            foreach (var item in list)
            {
                var str = GetPostStaff(item.Key);
                Database.Where(c => c.c3 == item.Key).ToList().ForEach(c => c.c8 = str);
            }

            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            MemoryCachingClient M = new MemoryCachingClient();
            M.Remove(employee.EmployeeNo);
            M.Add(employee.EmployeeNo, Database);

            return Public.JsonSerializeHelper.SerializeToJson(Database);
        }

        private string GetPostStaff(string post)
        {
            string str = string.Empty;

            string sql = "select x.NAME from  EMPLOYEE x join  EMPLOYEE_ROLE a on x.NO=a.EMPLOYEENO join ROLE b on a.ROLEID=b.ID join ROLE_PERMISSION c on b.ID=c.ROLEID join PERMISSION d on c.PERMISSIONID=d.ID where d.APPTYPE ='WORKFLOW' AND  d.NAME ='" + post + "' union select x.NAME from EMPLOYEE x join EMPLOYEE_PERMISSION a on x.NO = a.EMPLOYEENO join PERMISSION b on a.PERMISSIONID = b.ID where b.APPTYPE = 'WORKFLOW' AND x.LEAVE=0 AND x.available=1 AND b.NAME ='" + post + "'";

            var Database = DbContext.Database.SqlQuery<string>(sql).ToList();

            foreach (var item in Database)
            {
                str += item.Split('-')[0] + ",";
            }

            if (str.Length > 0)
            {
                str = str.Remove(str.Length - 1);
            }

            return str;
        }

        private string ReturnPayCode(string payCode, string CostCenter, int IsHeadOffice)
        {
            if (string.IsNullOrEmpty(payCode))
            {
                string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + CostCenter + "%'";
                var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(pay))
                {
                    var obj = PublicGetCosterCenter(IsHeadOffice, CostCenter);
                    return GetCompanyCode(obj.NAME);
                }
                else
                {
                    return pay;
                }
            }
            else
            {
                return payCode;
            }
        }


        public string GetEditPayCodeList(string BillNo)
        {
            BillNo = BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = BillNo.Split(',').ToList();
            list.Remove("");

            var FB = new FeeBill();
            var FT = new NoticeBill();
            var JS = new BorrowBill();
            var RB = new RefundBill();

            List<PublicClass> ReturnModel = new List<PublicClass>();

            foreach (var item in list)
            {
                PublicClass model = new PublicClass();
                if (item.Contains("FB"))
                {
                    var isFee = FB.GetBillModel(item);
                    if (isFee != null)
                    {
                        model.c1 = isFee.BillNo;
                        model.c2 = "费用报销单";
                        model.c3 = AjaxGetName(isFee.Creator);
                        model.c4 = isFee.Owner;
                        model.c5 = isFee.CreateTime.ToString("yyyy-mm-dd");
                        model.c6 = isFee.ApprovalTime;
                        model.c7 = ReturnPayCode(isFee.PaymentCompanyCode, isFee.PersonInfo.CostCenter, isFee.PersonInfo.IsHeadOffice);
                        model.c8 = isFee.Remark;
                    }
                    else
                    {
                        var IsRB = RB.GetBillModel(BillNo);
                        if (IsRB != null)
                        {
                            model.c1 = IsRB.BillNo;
                            model.c2 = "费用还款单";
                            model.c3 = AjaxGetName(IsRB.Creator);
                            model.c4 = IsRB.Owner;
                            model.c5 = IsRB.CreateTime.ToString("yyyy-mm-dd");
                            model.c6 = IsRB.ApprovalTime;
                            model.c7 = ReturnPayCode(IsRB.PaymentCompanyCode, IsRB.PersonInfo.CostCenter, IsRB.PersonInfo.IsHeadOffice);
                            model.c8 = IsRB.Remark;
                        }
                    }
                }
                else if (item.Contains("FT"))
                {
                    var isFT = FT.GetBillModel(BillNo);
                    if (isFT != null)
                    {
                        model.c1 = isFT.BillNo;
                        model.c2 = "付款通知书";
                        model.c3 = AjaxGetName(isFT.Creator);
                        model.c4 = isFT.Owner;
                        model.c5 = isFT.CreateTime.ToString("yyyy-mm-dd");
                        model.c6 = isFT.ApprovalTime;
                        model.c8 = isFT.Remark;
                    }
                }
                else if (item.Contains("JS"))
                {
                    var isJS = JS.GetBillModel(BillNo);
                    if (isJS != null)
                    {
                        model.c1 = isJS.BillNo;
                        model.c2 = "借款单";
                        model.c3 = AjaxGetName(isJS.Creator);
                        model.c4 = isJS.Owner;
                        model.c5 = isJS.CreateTime.ToString("yyyy-mm-dd");
                        model.c6 = isJS.ApprovalTime;
                        model.c7 = ReturnPayCode(isJS.PaymentCompanyCode, isJS.PersonInfo.CostCenter, isJS.PersonInfo.IsHeadOffice);
                        model.c8 = isJS.Remark;
                    }
                }
                if (!string.IsNullOrEmpty(model.c1))
                {
                    ReturnModel.Add(model);
                }
            }
            return Public.JsonSerializeHelper.SerializeToJson(ReturnModel);
        }

        public string UpdatePayCode(string BillNo, string code)
        {
            string error = string.Empty;
            string msg = string.Empty;

            if (string.IsNullOrEmpty(BillNo))
            {
                error = "1";
                msg = "单号为空";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            BillNo = BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = BillNo.Split(',').ToList();
            list.Remove("");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("PaymentCompanyCode", code);

            var FB = new FeeBill();
            var FT = new NoticeBill();
            var JS = new BorrowBill();
            var RB = new RefundBill();

            try
            {
                foreach (var item in list)
                {
                    if (item.Contains("FB"))
                    {
                        var isFee = FB.GetBillModel(item);
                        if (isFee != null)
                        {
                            FB.PublicEditMethod(item, dic);

                            string sql = string.Format(" Update FEE_FEEBILL set PAYCOMPANYCODE='{0}' where BIllno='{1}' ", code, item);
                            DbContext.Database.ExecuteSqlCommand(sql);

                            DbContext.SaveChanges();
                        }
                        else
                        {
                            RB.PublicEditMethod(item, dic);

                            string sql = string.Format(" Update FEE_FEEREFUNDBILL set PAYCOMPANYCODE='{0}' where BIllno='{1}' ", code, item);
                            DbContext.Database.ExecuteSqlCommand(sql);

                            DbContext.SaveChanges();
                        }
                    }
                    else if (item.Contains("FT"))
                    {
                        FT.PublicEditMethod(item, dic);

                        string sql = string.Format(" Update FEE_NOTICEBILL set PAYCOMPANYCODE='{0}' where BIllno='{1}' ", code, item);
                        DbContext.Database.ExecuteSqlCommand(sql);

                        DbContext.SaveChanges();
                    }
                    else if (item.Contains("JS"))
                    {
                        JS.PublicEditMethod(item, dic);

                        string sql = string.Format(" Update FEE_BORROWBILL set PAYCOMPANYCODE='{0}' where BIllno='{1}' ", code, item);
                        DbContext.Database.ExecuteSqlCommand(sql);

                        DbContext.SaveChanges();
                    }
                    else
                    {
                        RB.PublicEditMethod(item, dic);

                        string sql = string.Format(" Update FEE_FEEREFUNDBILL set PAYCOMPANYCODE='{0}' where BIllno='{1}' ", code, item);
                        DbContext.Database.ExecuteSqlCommand(sql);

                        DbContext.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                error = "1";
                msg = "系统错误";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            error = "0";
            msg = "更新成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }



        public string EditColletion(string BillList, string username, string bankname, string code, string number)
        {
            string error = string.Empty;
            string msg = string.Empty;

            if (string.IsNullOrEmpty(BillList) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(bankname) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(number))
            {
                error = "1";
                msg = "信息缺失";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            BillList = BillList.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = BillList.Split(',').ToList();

            var FB = new FeeBill();
            var JS = new BorrowBill();
            var RB = new RefundBill();


            Dictionary<string, CollectionInfo> dic = new Dictionary<string, CollectionInfo>();
            CollectionInfo coll = new CollectionInfo();
            coll.Name = username;
            coll.SubbranchBank = bankname;
            coll.SubbranchBankCode = number;
            coll.CardCode = code;
            dic.Add("CollectionInfo", coll);


            foreach (var item in list)
            {
                if (item.Contains("FB"))
                {
                    var model = FB.GetBillModel(item);
                    if (model != null)
                    {
                        //审批通过了
                        if (model.ApprovalStatus == 2)
                        {
                            //不予许通过
                            msg += "单号" + item + "未做处理;";
                        }
                        else
                        {
                            //进行修改
                            FB.PublicEditMethod<CollectionInfo>(item, dic);
                        }
                    }
                    else
                    {
                        var model2 = RB.GetBillModel(item);
                        if (model2.ApprovalStatus == 2)
                        {
                            msg += "单号" + item + "未做处理;";
                        }
                        else
                        {
                            RB.PublicEditMethod<CollectionInfo>(item, dic);
                        }
                    }
                }
                else if (item.Contains("JS"))
                {
                    var Model3 = JS.GetBillModel(item);
                    if (Model3.ApprovalStatus == 2)
                    {
                        msg += "单号" + item + "未做处理;";
                    }
                    else
                    {
                        JS.PublicEditMethod<CollectionInfo>(item, dic);
                    }
                }
            }

            error = "0";
            if (string.IsNullOrEmpty(msg))
            {
                msg = "修改成功";
            }

            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }

        public string EditPrividerInfo(string billno, string name, string bankname, string bankcode, string swift, string iban, string num, string providercode, string companycode)
        {
            string error = string.Empty;
            string msg = string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(bankname) || string.IsNullOrEmpty(bankcode))
            {
                error = "1";
                msg = "供应商信息缺失";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            if (string.IsNullOrEmpty(num))
            {
                var grade = GetSubjectCode(name);
                //只有外汇可以没有联行号
                if (grade != 3)
                {
                    error = "1";
                    msg = "支行数据错误";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
            }


            billno = billno.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = billno.Split(',').ToList();

            var FT = new NoticeBill();

            ProviderInfo provider = new ProviderInfo();
            provider.ProviderName = name;
            provider.BankName = bankname;
            provider.BankNo = bankcode;
            provider.CompanyCode = companycode;
            provider.IBAN = iban;
            provider.BankCode = swift;
            provider.ProviderCode = providercode;
            provider.SubbranchBankCode = num;

            Dictionary<string, ProviderInfo> dic = new Dictionary<string, ProviderInfo>();
            dic.Add("ProviderInfo", provider);

            foreach (var item in list)
            {
                if (item.Contains("FT"))
                {
                    //通过的单不处理
                    var model = FT.GetBillModel(item);
                    if (model != null & model.ApprovalStatus != 2)
                    {
                        FT.PublicEditMethod<ProviderInfo>(item, dic);
                    }
                    else
                    {
                        msg += "单号" + item + "未做处理;";
                    }
                }
                //不处理
            }

            error = "0";
            if (string.IsNullOrEmpty(msg))
            {
                msg = "修改成功";
            }

            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }


        /// <summary>
        /// 判断是否为外汇
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public int GetSubjectCode(string providerName)
        {
            //1为个人，2为公司，3为外汇
            int grade = 2;
            if (string.IsNullOrEmpty(providerName))
            {
                grade = 1;
            }
            else
            {
                if (providerName.Length <= 4)
                {
                    grade = 1;
                }
                else if (providerName.Length > 4)
                {
                    string str = providerName.Remove(1);
                    string pattern = @"^[a-zA-Z]*$";
                    bool Istrue = System.Text.RegularExpressions.Regex.IsMatch(str, pattern);
                    if (Istrue)
                    {
                        grade = 3;
                    }
                    else
                    {
                        grade = 2;
                    }
                }
            }
            return grade;
        }


        public ActionResult GetAllBillNo(string No)
        {
            List<Models.FeeBillModelRef> ModelList = new List<Models.FeeBillModelRef>();

            List<Models.FeeBillModel> list1 = new Marisfrolg.Fee.BLL.FeeBill().GetBillModelByNo(No);
            foreach (var item in list1)
            {
                Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "费用报销单", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, Status = item.Status };
                ModelList.Add(temp);
            }
            List<Models.NoticeBillModel> list2 = new Marisfrolg.Fee.BLL.NoticeBill().GetBillModelByNo(No);
            foreach (var item in list2)
            {
                Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "付款通知书", Remark = item.Remark, CreateTime = item.CreateTime, ProviderName = item.ProviderInfo.ProviderName, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, Status = item.Status, MissBill = item.MissBill };
                ModelList.Add(temp);
            }
            List<Models.BorrowBillModel> list3 = new Marisfrolg.Fee.BLL.BorrowBill().GetBillModelByNo(No);
            foreach (var item in list3)
            {
                Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.TotalMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "借款单", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, Status = item.Status, SurplusMoney = item.SurplusMoney };
                ModelList.Add(temp);
            }
            List<Models.RefundBillModel> list4 = new Marisfrolg.Fee.BLL.RefundBill().GetBillModelByNo(No);
            foreach (var item in list4)
            {
                Models.FeeBillModelRef temp = new Models.FeeBillModelRef() { BillNo = item.BillNo, PersonInfo = new Models.PersonInfo { Brand = item.PersonInfo.Brand, IsHeadOffice = item.PersonInfo.IsHeadOffice, ShopCode = item.PersonInfo.ShopCode }, Owner = item.Owner, TotalMoney = item.RealRefundMoney, StringTime = item.CreateTime.ToString("yyyy-MM-dd"), ApprovalTime = item.ApprovalTime, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, PageName = "还款单", Remark = item.Remark, CreateTime = item.CreateTime, PrintedCount = item.PrintedCount, Items = item.Items, CopyCount = item.CopyCount, RecycleBin = item.RecycleBin, Status = item.Status };
                ModelList.Add(temp);
            }
            ModelList = ModelList.OrderByDescending(c => c.CreateTime).ToList();

            return Json(ModelList, JsonRequestBehavior.AllowGet);
        }


        public string EditFeeAccount(string Id, string FeeName, string IsShow, string IsMarket, string SortId, string OldName, int BigClass)
        {
            //string sql = string.Format("update Fee_account set NAME='{0}',HIDE={1},IS_MARKET={2},Sort={3},OLDNAME='{4}' where ID={5}",
            //    FeeName, IsShow, IsMarket, SortId, OldName, Id);

            string sql = string.Format("update Fee_account set NAME='{0}',HIDE={1},IS_MARKET={2},Sort={3},ACCOUNT_TYPE={5} where ID={4}",
               FeeName, IsShow, IsMarket, SortId, Id, BigClass);
            int result = DbContext.Database.ExecuteSqlCommand(sql);
            if (result > 0)
            {
                return "Success";
            }
            return "Fail";
        }


        public ActionResult TransferAuditor(string BillNos, string No)
        {
            List<string> list = BillNos.ToUpper().Replace(",", "").Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H").Split(',').ToList();
            list.Remove("");
            var em = DbContext.EMPLOYEE.Where(c => c.NO == No && c.LEAVE == "0" && c.AVAILABLE == "1").FirstOrDefault();

            if (em == null)
            {
                return Json(new { Msg = "工号无效", Status = "error" });
            }

            string str = string.Empty;
            foreach (var item in list)
            {
                string Wk_Id = string.Empty;
                string obj_ID = new FeeBill().GetTransferData(item, out Wk_Id);

                string url = "http://192.168.2.14/WorkFlowServer/WorkFlowServer/upApproval";

                string postDataStr = string.Format("WorkFlowId={0}&AssignmentIds={1}&userids={2}", Wk_Id, obj_ID, No);

                string jsonList = this.HttpGet(url, postDataStr);

                if (jsonList == "0")
                {
                    str += "单号" + item + ",";
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                str += "未转移成功";
            }

            return Json(new { Msg = str, Status = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult QueryPassword(string No)
        {
            var em = DbContext.EMPLOYEE.Where(c => c.NO == No && c.LEAVE == "0" && c.AVAILABLE == "1").FirstOrDefault();

            if (em == null)
            {
                return Content("");
            }

            var password = Marisfrolg.Public.Common.Decryption(em.PASSWORD);

            return Content(password);
        }

        public ActionResult EditProviderName(string BillNos, string Name)
        {
            var list = BillNos.ToUpper().Replace(",", "").Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H").Split(',').ToList();
            list.Remove("");

            string str = string.Empty;
            foreach (var item in list)
            {
                var Notice_BLL = new NoticeBill();
                if (item.Contains("FT"))
                {
                    var Model = Notice_BLL.GetBillModel(item);
                    Model.ProviderInfo.ProviderName = Name.Replace(" ", "");
                    Dictionary<string, ProviderInfo> dic = new Dictionary<string, ProviderInfo>();
                    dic.Add("ProviderInfo", Model.ProviderInfo);

                    var result = Notice_BLL.PublicEditMethod<ProviderInfo>(item, dic);
                    if (string.IsNullOrEmpty(result))
                    {
                        str += "单号" + item + "，";
                    }
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                str += "未修改成功";
            }

            return Json(new { Msg = str, Status = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetChargeNo(int DparId, string No)
        {
            var dp = DbContext.DEPARTMENT.Where(c => c.ID == DparId).FirstOrDefault();

            if (dp == null)
            {
                return Json(new { Msg = "部门ID无效", Status = "error" });
            }

            var em = DbContext.EMPLOYEE.Where(c => c.NO == No && c.LEAVE == "0" && c.AVAILABLE == "1").FirstOrDefault();
            if (em == null)
            {
                return Json(new { Msg = "工号无效", Status = "error" });
            }

            string sql = string.Format(@"MERGE INTO FEE_PERSON_EXTEND T1  
USING (SELECT count(*) ct from FEE_PERSON_EXTEND where TYPE='extradata' and DEPARTMENTNAME='中心负责人' and DEPARTMENTCODE='{0}' ) T2  
ON (T2.ct<>0)  
WHEN MATCHED THEN  
update set T1.value='{1}' where T1.TYPE='extradata' and  T1.DEPARTMENTNAME='中心负责人' and  T1.DEPARTMENTCODE='{0}'
WHEN NOT MATCHED THEN   
insert(TYPE,DEPARTMENTNAME,value,DEPARTMENTCODE) VALUES('extradata','中心负责人','{1}','{0}')                
                ",
                DparId,
                No
                );

            int result = Public.DBHelper.ExecuteNonQuery(sql);

            return Json(new { Msg = "", Status = "success" }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetSAPData()
        {
            string sql = "SELECT KOSTL, BUKRS, NAMES, PROFIT FROM FEE_SAPDATA ";

            var resut = DbContext.Database.SqlQuery<SAPDATA>(sql).ToList();

            return Json(resut, JsonRequestBehavior.AllowGet);
        }


        public ActionResult OperateShopStatus(string key)
        {

            var model = DbContext.SHOP.Where(c => c.NAME.Contains(key) || c.CODE.Contains(key)).ToList();

            List<ShopTempData> NewModel = new List<ShopTempData>();

            if (model.Count > 0)
            {
                foreach (var item in model)
                {
                    ShopTempData temp = new ShopTempData();
                    temp.CODE = item.CODE;
                    temp.NAME = item.NAME;
                    temp.CREATEDATE = item.CREATEDATE.ToString("yyyy-MM-dd");
                    temp.AVAILABLE = item.AVAILABLE;
                    NewModel.Add(temp);
                }
            }

            return Json(NewModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateShopStatus(string status, string code)
        {

            string sql = string.Format("update shop set AVAILABLE='{0}' where code='{1}'", status, code);

            int result = DbContext.Database.ExecuteSqlCommand(sql);

            if (result > 0)
            {
                return Json(new { Msg = "", Status = "success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Msg = "", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAllBigClass()
        {
            var model = DbContext.FEE_ACCOUNT_TYPE.ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchPerson(string BillNo, string position)
        {
            BillNo = BillNo.ToUpper();
            var WorkFlowID = GetWorkFlowID(BillNo);

            if (!string.IsNullOrEmpty(WorkFlowID))
            {
                //远程调用方法
                string url = "http://192.168.2.14/Marisfrolg.Fee/WorkFlow/GetUsersByWorkFlowID";

                string postDataStr = string.Format("WorkFlowId={0}&type=4&value={1}", WorkFlowID, position);

                string jsonList = this.HttpGet(url, postDataStr);

                return Content(jsonList);
            }
            else
            {
                return Content("单号有误");
            }
        }

        public ActionResult RepairBill(string BillNo)
        {
            string WorkFlowIDS = string.Empty;

            if (!string.IsNullOrEmpty(BillNo))
            {
                var list = BillNo.ToUpper().Replace(",", "").Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H").Split(',').ToList();
                list.Remove("");
                foreach (var item in list)
                {
                    var WorkFlowID = GetWorkFlowID(item);
                    if (!string.IsNullOrEmpty(WorkFlowID))
                    {
                        WorkFlowIDS += WorkFlowID + ",";
                    }
                }
            }
 
            if (!string.IsNullOrEmpty(WorkFlowIDS))
            {
                WorkFlowIDS = WorkFlowIDS.TrimEnd(',');

                string url = "http://192.168.2.14/WorkFlowServer/WorkFlowServer/repairBill";

                string postDataStr = string.Format("WorkFlowID={0}", WorkFlowIDS);

                string jsonList = this.HttpGet(url, postDataStr);

                if (jsonList == "1")
                {
                    return Json(new { Status = "success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "error" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = "error" }, JsonRequestBehavior.AllowGet);
        }



        private string GetWorkFlowID(string BillNo)
        {
            BillNo = BillNo.Trim(' ');
            string WorkFlowID = string.Empty;
            if (BillNo.Contains("FB"))
            {
                var model = new FeeBill().GetBillModel(BillNo);
                if (model != null)
                {
                    WorkFlowID = model.WorkFlowID;
                }
                else
                {
                    var model1 = new RefundBill().GetBillModel(BillNo);
                    WorkFlowID = model1.WorkFlowID;
                }
            }
            else if (BillNo.Contains("FT"))
            {
                var model = new NoticeBill().GetBillModel(BillNo);
                WorkFlowID = model.WorkFlowID;
            }
            else if (BillNo.Contains("JS"))
            {
                var model = new BorrowBill().GetBillModel(BillNo);
                WorkFlowID = model.WorkFlowID;

            }
            else
            {
                var model = new RefundBill().GetBillModel(BillNo);
                WorkFlowID = model.WorkFlowID;
            }
            return WorkFlowID;
        }
    }

    public class SAPDATA
    {
        public string KOSTL { get; set; }
        public string BUKRS { get; set; }
        public string NAMES { get; set; }
        public string PROFIT { get; set; }
    }

    public class ShopTempData
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string CREATEDATE { get; set; }
        public string AVAILABLE { get; set; }
    }
}
