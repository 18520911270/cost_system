using Marisfrolg.Business;
using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using System.Text;
using System.Data;
using System.Collections;

namespace Marisfrolg.Fee.Controllers
{
    public class SAPController : SecurityController
    {
        //
        // GET: /SAP/

        public ActionResult Index()
        {
            return View();
        }



        public string GetDepartment()
        {
            var model = DbContext.DEPARTMENT.Where(c => c.AVAILABLE == "1").Select(x => new SAPDepartment
             {
                 NAME = x.NAME,
                 ID = x.ID
             }).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        public string GetSAPInfoList(string BillType, string CreateTime1, string CreateTime2, string OverTime1, string OverTime2, string SapProofType, string SapTime1, string SapTime2, string SpecialProperty)
        {
            DateTime time1 = new DateTime(1991, 1, 1);
            DateTime time2 = new DateTime(2099, 1, 1);
            DateTime time3 = new DateTime(1991, 1, 1);
            DateTime time4 = new DateTime(2099, 1, 1);
            List<SAPShowData> DataList = new List<SAPShowData>();
            if (!string.IsNullOrEmpty(CreateTime1))
            {
                time1 = Convert.ToDateTime(CreateTime1);
            }
            if (!string.IsNullOrEmpty(CreateTime2))
            {
                time2 = Convert.ToDateTime(CreateTime2);
            }
            if (!string.IsNullOrEmpty(OverTime1))
            {
                time3 = Convert.ToDateTime(OverTime1);
            }
            if (!string.IsNullOrEmpty(OverTime2))
            {
                time4 = Convert.ToDateTime(OverTime2);
            }
            //为空默认为所有
            if (string.IsNullOrEmpty(BillType))
            {
                BillType = "1,2,3,4";
            }


            List<string> CompanyList = new List<string>();
            string Permission = "1000,2000,1300,2100,4000,1330,1030,1020";
            //权限控制
            if (!string.IsNullOrEmpty(Permission))
            {
                CompanyList = Permission.Split(',').ToList();
            }
            else
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();

                CompanyList = GetPersonPermissionV1(employee.EmployeeNo);
            }


            if (BillType.Contains("1"))
            {
                var model = DbContext.FEE_FEEBILL.Where(c => c.CREATETIME >= time1 & c.CREATETIME <= time2 & c.APPROVALTIME >= time3 & c.APPROVALTIME <= time4 && CompanyList.Contains(c.COMPANYCODE)).Select(c => new SAPShowData
                  {
                      BillNo = c.BILLNO,
                      Brand = c.BRANDCODE,
                      CostCenter = c.COST_ACCOUNT,
                      BillType = "FeeBill",
                      Department = c.BOOT_DP_NAME,
                      DepartmentID = c.BOOT_DP_ID,
                      CreateTime = c.CREATETIME,
                      Owner = c.WORKNUMBER,
                      TotalMoney = c.TOTALMONEY,
                      Remark = c.REMARK,
                      TransactionDate = c.TRANSACTIONDATE,
                      ApprovalTime = c.APPROVALTIME,
                      SAPProof = c.SAPPROOF,
                      SapUploadTime = c.SAPUPLOADTIME,
                      SapCreator = c.SAPCREATOR,
                      CompanyCode = c.COMPANYCODE,
                      Funds = c.IS_TEAMMONEY,
                      Agent = c.IS_AGENTMONEY,
                      BankDebt = c.IS_BANK_MINUS,
                      MarketDebt = c.IS_MARKET_MINUS,
                      Cash = c.IS_CASH_MINUS
                  }).ToList();
                DataList.AddRange(model);
            }
            if (BillType.Contains("2"))
            {
                var model = DbContext.FEE_NOTICEBILL.Where(c => c.CREATETIME >= time1 & c.CREATETIME <= time2 & c.APPROVALTIME >= time3 & c.APPROVALTIME <= time4 && CompanyList.Contains(c.COMPANYCODE)).Select(c => new SAPShowData
                {
                    BillNo = c.BILLNO,
                    Brand = c.BRANDCODE,
                    CostCenter = c.COST_ACCOUNT,
                    BillType = "NoticeBill",
                    Department = c.BOOT_DP_NAME,
                    DepartmentID = c.BOOT_DP_ID,
                    CreateTime = c.CREATETIME,
                    Owner = c.WORKNUMBER,
                    TotalMoney = c.TOTALMONEY,
                    Remark = c.REMARK,
                    TransactionDate = c.TRANSACTIONDATE,
                    ApprovalTime = c.APPROVALTIME,
                    SAPProof = c.SAPPROOF,
                    SapUploadTime = c.SAPUPLOADTIME,
                    SapCreator = c.SAPCREATOR,
                    CompanyCode = c.COMPANYCODE,
                    Funds = c.IS_TEAMMONEY,
                    Agent = c.IS_AGENTMONEY,
                    BankDebt = c.IS_BANK_MINUS,
                    MarketDebt = c.IS_MARKET_MINUS,
                    Cash = c.IS_CASH_MINUS
                }).ToList();
                DataList.AddRange(model);
            }
            if (BillType.Contains("3"))
            {
                var model = DbContext.FEE_BORROWBILL.Where(c => c.CREATETIME >= time1 & c.CREATETIME <= time2 & c.APPROVALTIME >= time3 & c.APPROVALTIME <= time4 && CompanyList.Contains(c.COMPANYCODE)).Select(c => new SAPShowData
                {
                    BillNo = c.BILLNO,
                    Brand = c.BRANDCODE,
                    CostCenter = c.COST_ACCOUNT,
                    BillType = "BorrowBill",
                    Department = c.BOOT_DP_NAME,
                    DepartmentID = c.BOOT_DP_ID,
                    CreateTime = c.CREATETIME,
                    Owner = c.WORKNUMBER,
                    TotalMoney = c.TOTALMONEY,
                    Remark = c.REMARK,
                    TransactionDate = c.TRANSACTIONDATE,
                    ApprovalTime = c.APPROVALTIME,
                    SAPProof = c.SAPPROOF,
                    SapUploadTime = c.SAPUPLOADTIME,
                    SapCreator = c.SAPCREATOR,
                    CompanyCode = c.COMPANYCODE,
                    Funds = c.IS_TEAMMONEY,
                    Agent = c.IS_AGENTMONEY,
                    BankDebt = c.IS_BANK_MINUS,
                    MarketDebt = c.IS_MARKET_MINUS,
                    Cash = c.IS_CASH_MINUS
                }).ToList();
                DataList.AddRange(model);
            }
            if (BillType.Contains("4"))
            {
                var model = DbContext.FEE_FEEREFUNDBILL.Where(c => c.CREATETIME >= time1 & c.CREATETIME <= time2 & c.APPROVALTIME >= time3 & c.APPROVALTIME <= time4 && CompanyList.Contains(c.COMPANYCODE)).Select(c => new SAPShowData
                {
                    BillNo = c.BILLNO,
                    Brand = c.BRANDCODE,
                    CostCenter = c.COST_ACCOUNT,
                    BillType = "RefundBill",
                    Department = c.BOOT_DP_NAME,
                    DepartmentID = c.BOOT_DP_ID,
                    CreateTime = c.CREATETIME,
                    Owner = c.WORKNUMBER,
                    TotalMoney = c.TOTALMONEY,
                    Remark = c.REMARK,
                    TransactionDate = c.TRANSACTIONDATE,
                    ApprovalTime = c.APPROVALTIME,
                    SAPProof = c.SAPPROOF,
                    SapUploadTime = c.SAPUPLOADTIME,
                    SapCreator = c.SAPCREATOR,
                    CompanyCode = c.COMPANYCODE,
                    Funds = c.IS_TEAMMONEY,
                    Agent = c.IS_AGENTMONEY,
                    BankDebt = c.IS_BANK_MINUS,
                    MarketDebt = c.IS_MARKET_MINUS,
                    Cash = c.IS_CASH_MINUS
                }).ToList();


                //var model1 = DbContext.FEE_CASHREFUNDBILL.Where(c => c.CREATETIME >= time1 & c.CREATETIME <= time2 & c.APPROVALTIME >= time3 & c.APPROVALTIME <= time4).Select(c => new SAPShowData
                //{
                //    BillNo = c.BILLNO,
                //    CostCenter = c.COST_ACCOUNT,
                //    BillType = "RefundBill",
                //    Department = c.BOOT_DP_NAME,
                //    DepartmentID = c.BOOT_DP_ID,
                //    CreateTime = c.CREATETIME,
                //    Owner = c.WORKNUMBER,
                //    TotalMoney = c.REALMONEY,
                //    Remark = c.REMARK,
                //    TransactionDate = c.TRANSACTIONDATE,
                //    ApprovalTime = c.APPROVALTIME,
                //    SAPProof = c.SAPPROOF,
                //    SapUploadTime = c.SAPUPLOADTIME,
                //    SapCreator = c.SAPCREATOR
                //}).ToList();

                DataList.AddRange(model);
                //DataList.AddRange(model1);
            }
            //if (!string.IsNullOrEmpty(Department))
            //{
            //    var list = Department.Split(',').ToList();
            //    list.Remove("");
            //    DataList = DataList.Where(c => list.Contains(c.DepartmentID.ToString())).ToList();
            //}
            //if (!string.IsNullOrEmpty(Remark))
            //{
            //    DataList = DataList.Where(c => c.Remark.Contains(Remark)).ToList();
            //}
            //if (!string.IsNullOrEmpty(BillNo))
            //{
            //    BillNo = BillNo.ToUpper().Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            //    var list = BillNo.Split(',').ToList();
            //    list.Remove("");
            //    DataList = DataList.Where(c => list.Contains(c.BillNo)).ToList();
            //}

            //待上传
            if (SapProofType == "1")
            {
                DataList = DataList.Where(c => c.SAPProof == null || c.SAPProof == "").ToList();
            }
            //已上传
            if (SapProofType == "2")
            {
                DataList = DataList.Where(c => c.SAPProof != null && c.SAPProof != "").ToList();
            }
            //我上传的凭证
            if (SapProofType == "3")
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                DataList = DataList.Where(c => c.SapCreator == employee.EmployeeNo && c.SAPProof != null).ToList();
            }

            if (!string.IsNullOrEmpty(SapTime1))
            {
                DateTime time = Convert.ToDateTime(SapTime1);
                DataList = DataList.Where(c => c.SapUploadTime >= time).ToList();
            }

            if (!string.IsNullOrEmpty(SapTime2))
            {
                DateTime time = Convert.ToDateTime(SapTime2);
                DataList = DataList.Where(c => c.SapUploadTime <= time).ToList();
            }

            if (!string.IsNullOrEmpty(SpecialProperty))
            {
                List<string> list = SpecialProperty.Split(',').ToList();
                list.Remove("");
                int hd = 2; int hdcount = 0;
                int dl = 2; int dlcount = 0;
                int sc = 2; int sccount = 0;
                int yh = 2; int yhcount = 0;
                int yj = 2; int yjcount = 0;
                int fp = 2; int fpcount = 0;
                foreach (var item in list)
                {
                    if (item.Contains("hd"))
                    {
                        hdcount++;
                        if (hdcount <= 1)
                        {
                            hd = Convert.ToInt32(item.Replace("hd", ""));
                        }
                        else
                        {
                            hd = 2;
                        }
                    }
                    else if (item.Contains("dl"))
                    {
                        dlcount++;
                        if (dlcount <= 1)
                        {
                            dl = Convert.ToInt32(item.Replace("dl", ""));
                        }
                        else
                        {
                            dl = 2;
                        }

                    }
                    else if (item.Contains("sc"))
                    {
                        sccount++;
                        if (sccount <= 1)
                        {
                            sc = Convert.ToInt32(item.Replace("sc", ""));
                        }
                        else
                        {
                            sc = 2;
                        }
                    }
                    else if (item.Contains("yh"))
                    {
                        yhcount++;
                        if (yhcount <= 1)
                        {
                            yh = Convert.ToInt32(item.Replace("yh", ""));
                        }
                        else
                        {
                            yh = 2;
                        }

                    }
                    else if (item.Contains("yj"))
                    {
                        yjcount++;
                        if (yjcount <= 1)
                        {
                            yj = Convert.ToInt32(item.Replace("yj", ""));
                        }
                        else
                        {
                            yj = 2;
                        }
                    }
                    else if (item.Contains("fp"))
                    {
                        fpcount++;
                        if (fpcount <= 1)
                        {
                            fp = Convert.ToInt32(item.Replace("fp", ""));
                        }
                        else
                        {
                            fp = 2;
                        }
                    }
                }
                if (hd != 2)
                {
                    DataList = DataList.Where(c => c.Funds == hd).ToList();
                }
                if (dl != 2)
                {
                    DataList = DataList.Where(c => c.Agent == dl).ToList();
                }
                if (sc != 2)
                {
                    DataList = DataList.Where(c => c.MarketDebt == sc).ToList();
                }
                if (yh != 2)
                {
                    DataList = DataList.Where(c => c.BankDebt == yh).ToList();
                }
                if (yj != 2)
                {
                    DataList = DataList.Where(c => c.Cash == yj).ToList();
                }
            }

            if (DataList.Count > 0)
            {
                DataList = DataList.OrderByDescending(c => c.ApprovalTime).ToList();
            }

            return Public.JsonSerializeHelper.SerializeToJson(DataList);
        }



        public string GetPersonPermission(string no)
        {

            List<string> CompanyList = new List<string>();
            string str = "'%" + no + "%'";
            string sql = "select brand from fee_person_extend where type='system' and departmentname='财务会计' and brand is not null  and value like " + str + "";
            var Database = DbContext.Database.SqlQuery<string>(sql).ToList();

            if (Database.Count > 0)
            {
                foreach (var item in Database)
                {
                    if (item == "MF" || item == "集团" || item == "MDC")
                    {
                        CompanyList.Add("1000");
                        CompanyList.Add("1020");
                        CompanyList.Add("1030");
                    }
                    else if (item == "ZCY" || item == "KA")
                    {
                        CompanyList.Add("2100");
                    }
                    else if (item == "SU")
                    {
                        CompanyList.Add("2000");
                        CompanyList.Add("1030");
                    }
                    else if (item == "AM")
                    {
                        CompanyList.Add("1300");
                        CompanyList.Add("1330");
                    }
                    else if (item == "明佳豪")
                    {
                        CompanyList.Add("4000");
                    }
                }
                CompanyList = CompanyList.Distinct().ToList();
            }

            return Public.JsonSerializeHelper.SerializeToJson(CompanyList);
        }



        public List<string> GetPersonPermissionV1(string no)
        {
            List<string> CompanyList = new List<string>();
            string str = "'%" + no + "%'";
            string sql = "select brand from fee_person_extend where type='system' and departmentname='财务会计' and brand is not null  and value like " + str + "";
            var Database = DbContext.Database.SqlQuery<string>(sql).ToList();

            if (Database.Count > 0)
            {
                foreach (var item in Database)
                {
                    if (item == "MF" || item == "集团" || item == "MDC")
                    {
                        CompanyList.Add("1000");
                        CompanyList.Add("1020");
                        CompanyList.Add("1030");
                    }
                    else if (item == "ZCY" || item == "KA")
                    {
                        CompanyList.Add("2100");
                    }
                    else if (item == "SU")
                    {
                        CompanyList.Add("2000");
                        CompanyList.Add("1030");
                    }
                    else if (item == "AM")
                    {
                        CompanyList.Add("1300");
                        CompanyList.Add("1330");
                    }
                    else if (item == "明佳豪")
                    {
                        CompanyList.Add("4000");
                    }
                }
                CompanyList = CompanyList.Distinct().ToList();
            }
            return CompanyList;
        }



        public string FillRemarkIntoISAP(string remark, string itemName, decimal money, string InvoiceNum)
        {
            itemName = itemName.Split('-')[0];
            string str = String.Empty;
            if (money < 0 && itemName.Contains("银行"))
            {
                str = String.Format("{0}报销总金额{1}", remark, money * -1);
            }
            else
            {
                str = String.Format("{0}{1}金额{2}", remark, itemName, money);
            }
            if (!string.IsNullOrEmpty(InvoiceNum))
            {
                str += "-" + InvoiceNum;
            }

            return str;
        }



        //检测成本中心,利润中心,原因代码的逻辑
        string TestData(Sp1 Model)
        {
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(Model.ReasonCode))
            {
                sql = "SELECT REASON_CODE FROM FEE_ACCOUNT WHERE REASON_CODE='" + Model.ReasonCode + "'";
                var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(result))
                {
                    return "原因代码不存在";
                }
            }

            if (!string.IsNullOrEmpty(Model.CosterCenter))
            {
                sql = "SELECT KOSTL FROM FEE_SAPDATA WHERE KOSTL like '%" + Model.CosterCenter + "'";
                var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(result))
                {
                    return "成本中心不存在";
                }
            }

            if (!string.IsNullOrEmpty(Model.ProfitCenter))
            {
                sql = "SELECT PROFIT FROM FEE_SAPDATA WHERE PROFIT like '%" + Model.ProfitCenter + "'";
                var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(result))
                {
                    return "利润中心不存在";
                }
            }

            return "";
        }



        public string CallSapMethod(SubmitSapModel temp)
        {

            //数据验证
            if (temp.Items != null && temp.Items.Count > 0)
            {

                for (int i = 0; i < temp.Items.Count; i++)
                {
                    string errorMsg = TestData(temp.Items[i]);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        errorMsg = "第" + (i + 1) + "行" + errorMsg;
                        return Public.JsonSerializeHelper.SerializeToJson(new { error = true, errorMsg = errorMsg });
                    }
                }
            }

            SapUpload upload = new SapUpload();

            //结构体赋值
            DOCUMENTHEADER doc = new DOCUMENTHEADER();
            doc.USERNAME = temp.UserName;
            doc.HEADER_TXT = temp.Head_txt;
            doc.COMP_CODE = temp.CompanyCode;
            doc.DOC_DATE = temp.TransactionDate;
            doc.PSTNG_DATE = temp.ApprovalTime;
            doc.OBJ_TYPE = "BKPFF";
            doc.BUS_ACT = "RFBU RMWE";
            doc.REF_DOC_NO = temp.Reference;
            doc.TRANS_DATE = temp.ApprovalTime;

            List<ACCOUNTPAYABLE> A_table = new List<ACCOUNTPAYABLE>();
            List<ACCOUNTRECEIVABLE> B_table = new List<ACCOUNTRECEIVABLE>();
            List<ACCOUNTGL> C_table = new List<ACCOUNTGL>();
            List<CURRENCYAMOUNT> D_table = new List<CURRENCYAMOUNT>();
            List<EXTENSION2> E_table = new List<EXTENSION2>();

            int count = 1;

            foreach (var item in temp.Items)
            {
                if (item.SubjectType == "K")
                {
                    ACCOUNTPAYABLE Ktable = new ACCOUNTPAYABLE();
                    Ktable.ITEMNO_ACC = count;
                    Ktable.VENDOR_NO = ConvertAccountCode(item.Code);
                    Ktable.UMSKZ = item.BSCHL_Sign;
                    if (string.IsNullOrEmpty(temp.Remark))
                    {
                        Ktable.ITEM_TEXT = item.SpareRemark + (string.IsNullOrEmpty(item.InvoiceNum) ? "" : ("-" + item.InvoiceNum));

                    }
                    else
                    {
                        Ktable.ITEM_TEXT = FillRemarkIntoISAP(temp.Remark, item.SapName, item.Money, item.InvoiceNum);
                    }
                    Ktable.ACCT_TYPE = item.SubjectType;
                    if (!string.IsNullOrEmpty(item.CosterCenter))
                    {
                        Ktable.COSTCENTER = ConvertAccountCode(item.CosterCenter);
                    }
                    if (!string.IsNullOrEmpty(item.ProfitCenter))
                    {
                        Ktable.PROFIT_CTR = ConvertAccountCode(item.ProfitCenter);
                    }
                    if (!string.IsNullOrEmpty(item.PO_NUMBER))
                    {
                        Ktable.PO_NUMBER = item.PO_NUMBER;
                        var t1 = DbContext.FEE_ACCOUNT.Where(c => c.NAME == item.HideName || c.OLDNAME == item.HideName).FirstOrDefault();
                        if (t1 != null)
                        {
                            Ktable.GL_ACCOUNT = ConvertAccountCode(t1.ACCOUNT);

                        }
                    }
                    Ktable.ALLOC_NMBR = item.ALLOC_NMBR;
                    A_table.Add(Ktable);
                }
                else if (item.SubjectType == "D")
                {
                    ACCOUNTRECEIVABLE DTable = new ACCOUNTRECEIVABLE();
                    DTable.ITEMNO_ACC = count;
                    DTable.CUSTOMER = ConvertAccountCode(item.Code);
                    if (string.IsNullOrEmpty(temp.Remark))
                    {
                        DTable.ITEM_TEXT = item.SpareRemark + (string.IsNullOrEmpty(item.InvoiceNum) ? "" : ("-" + item.InvoiceNum));
                    }
                    else
                    {
                        DTable.ITEM_TEXT = FillRemarkIntoISAP(temp.Remark, item.SapName, item.Money, item.InvoiceNum);
                    }

                    DTable.ACCT_TYPE = item.SubjectType;
                    if (!string.IsNullOrEmpty(item.CosterCenter))
                    {
                        DTable.COSTCENTER = ConvertAccountCode(item.CosterCenter);
                    }
                    if (!string.IsNullOrEmpty(item.ProfitCenter))
                    {
                        DTable.PROFIT_CTR = ConvertAccountCode(item.ProfitCenter);
                    }
                    DTable.ALLOC_NMBR = item.ALLOC_NMBR;
                    B_table.Add(DTable);
                }
                //总账类
                else
                {
                    ACCOUNTGL STable = new ACCOUNTGL();
                    STable.ITEMNO_ACC = count;
                    STable.GL_ACCOUNT = ConvertAccountCode(item.Code);
                    if (string.IsNullOrEmpty(temp.Remark))
                    {
                        STable.ITEM_TEXT = item.SpareRemark + (string.IsNullOrEmpty(item.InvoiceNum) ? "" : ("-" + item.InvoiceNum));
                    }
                    else
                    {
                        STable.ITEM_TEXT = FillRemarkIntoISAP(temp.Remark, item.SapName, item.Money, item.InvoiceNum);
                    }

                    STable.ACCT_TYPE = item.SubjectType;
                    if (!string.IsNullOrEmpty(item.CosterCenter))
                    {
                        STable.COSTCENTER = ConvertAccountCode(item.CosterCenter);
                    }
                    if (!string.IsNullOrEmpty(item.ProfitCenter))
                    {
                        STable.PROFIT_CTR = ConvertAccountCode(item.ProfitCenter);
                    }
                    if (!string.IsNullOrEmpty(item.AssetsNum))
                    {
                        STable.ASSET_NO = item.AssetsNum;
                    }
                    STable.ALLOC_NMBR = item.ALLOC_NMBR;
                    C_table.Add(STable);
                }

                //币种
                CURRENCYAMOUNT cur = new CURRENCYAMOUNT();
                cur.ITEMNO_ACC = count;
                cur.CURRENCY = item.CoinType;
                cur.AMT_DOCCUR = item.Money;
                if (!string.IsNullOrEmpty(item.ExchangeRate))
                {
                    cur.EXCH_RATE = Convert.ToDecimal(item.ExchangeRate);
                }
                D_table.Add(cur);

                //结构体
                ZFI_EXTEN ZFI_Table = new ZFI_EXTEN();
                ZFI_Table.ITEMNO_ACC = count;
                if (!string.IsNullOrEmpty(item.ChargeCode))
                {
                    ZFI_Table.RSTGR = item.ReasonCode;
                }
                ZFI_Table.BSCHL = item.ChargeCode;
                if (!string.IsNullOrEmpty(item.BSCHL_Sign))
                {
                    ZFI_Table.UMSKZ = item.BSCHL_Sign;
                }
                if (!string.IsNullOrEmpty(item.PO_NUMBER))
                {
                    ZFI_Table.PO_NUMBER = item.PO_NUMBER;
                    ZFI_Table.PO_ITEM = count * 10;
                }
                if (!string.IsNullOrEmpty(item.AssetsNum))
                {
                    ZFI_Table.ANBWA = "100";
                    ZFI_Table.ANLN1 = item.AssetsNum;
                    ZFI_Table.ANLN2 = "0000";
                    ZFI_Table.BSCHL = "70";
                }

                //行项目
                EXTENSION2 EXTable = ConvertToExtension(ZFI_Table);
                E_table.Add(EXTable);
                count++;
            }


            //判断凭证类型，三种以上为sa，两者不存在sa则为sa，存在sa则为另外一种，只有一种类型则为改类型
            var proofList = temp.Items.GroupBy(c => c.ProofType).Select(x => x.Key).ToList();
            if (proofList.Count >= 3)
            {
                doc.DOC_TYPE = "SA";
            }
            else if (proofList.Count == 2)
            {
                if (proofList.Contains("SA"))
                {
                    doc.DOC_TYPE = proofList[0] == "SA" ? proofList[1] : proofList[0];
                }
                else
                {
                    doc.DOC_TYPE = "SA";
                }
            }
            else
            {
                doc.DOC_TYPE = proofList[0];
            }

            upload.结构体DOC = doc;
            upload.科目K = A_table;
            upload.科目D = B_table;
            upload.一般性总账 = C_table;
            upload.币种 = D_table;
            upload.行项目 = E_table;

            //预付原料
            var cig = upload.科目K.Where(c => !string.IsNullOrEmpty(c.PO_NUMBER)).FirstOrDefault();

            //return Public.JsonSerializeHelper.SerializeToJson(new { TYPE = "S", MESSAGE = "错误" });

            SapRetrun result = new SapRetrun();

            if (cig == null)
            {
                result = SapHandler.CreateAccounting(upload, "RFC");
            }
            else
            {
                result = SapHandler.CreateAccounting(upload, "RFG");
            }

            if (result.TYPE == "S")
            {
                temp.BillNo = temp.BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
                var list = temp.BillNo.Split(',').ToList();
                list.Remove("");

                ArrayList DataList = new ArrayList();

                foreach (var item in list)
                {
                    var str = UpdateVOUCHER(item, temp.PageName, temp.UserName, result.VOUCHER);
                    DataList.Add(str);
                }

                //批量执行事务
                new ReportHelper().ExecuteSqlTran(DataList);

            }

            return Public.JsonSerializeHelper.SerializeToJson(result);
        }



        /// <summary>
        /// 跟新凭证号
        /// </summary>
        public string UpdateVOUCHER(string billNo, string pageName, string creator, string VOUCHER)
        {
            string str = string.Empty;

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            switch (pageName.ToUpper())
            {
                case "FEEBILL":

                    str = string.Format("update FEE_FEEBILL set SAPCREATOR='{0}',SAPPROOF='{1}',SAPUPLOADTIME= to_date('{2}','YYYY-MM-DD HH24:MI:SS') where BILLNO='{3}'", creator, VOUCHER, time, billNo);

                    break;
                case "NOTICEBILL":

                    str = string.Format("update FEE_NOTICEBILL set SAPCREATOR='{0}',SAPPROOF='{1}',SAPUPLOADTIME= to_date('{2}','YYYY-MM-DD HH24:MI:SS') where BILLNO='{3}'", creator, VOUCHER, time, billNo);

                    break;
                case "BORROWBILL":

                    str = string.Format("update FEE_BORROWBILL set SAPCREATOR='{0}',SAPPROOF='{1}',SAPUPLOADTIME= to_date('{2}','YYYY-MM-DD HH24:MI:SS') where BILLNO='{3}'", creator, VOUCHER, time, billNo);

                    break;
                case "REFUNDBILL":

                    if (billNo.Contains("HK"))
                    {
                        str = string.Format("update FEE_CASHREFUNDBILL set SAPCREATOR='{0}',SAPPROOF='{1}',SAPUPLOADTIME= to_date('{2}','YYYY-MM-DD HH24:MI:SS') where BILLNO='{3}'", creator, VOUCHER, time, billNo);
                    }
                    else
                    {
                        str = string.Format("update FEE_FEEREFUNDBILL set SAPCREATOR='{0}',SAPPROOF='{1}',SAPUPLOADTIME= to_date('{2}','YYYY-MM-DD HH24:MI:SS') where BILLNO='{3}'", creator, VOUCHER, time, billNo);
                    }

                    break;
                default:
                    break;
            }
            return str;
        }



        /// <summary>
        /// 是否为费用类科目
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsFeeSubject(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }
            string str = code.Remove(1);
            if (str == "6")
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// 统计总金额
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public decimal SumLoanMoney(List<int> list, List<CURRENCYAMOUNT> cur)
        {
            return cur.Where(x => list.Contains(x.ITEMNO_ACC)).Sum(c => c.AMT_DOCCUR);
        }


        /// <summary>
        /// 获取利润中心
        /// </summary>
        /// <param name="shopCode"></param>
        /// <returns></returns>
        public string GetProfitInfo(string shopCode)
        {
            string sql = "select VALUE from FEE_SAPINFO where APPNAME='profit' and SHOPCODE='" + shopCode + "'";
            var profit = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            return ConvertAccountCode(profit);
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


        public EXTENSION2 ConvertToExtension(ZFI_EXTEN model)
        {
            EXTENSION2 temp = new EXTENSION2();
            temp.STRUCTURE = "ZFI_EXTEN";
            temp.VALUEPART1 = ConvertToZFI_EXTEN(model);
            return temp;
        }


        /// <summary>
        /// 转换成ZFI_EXTEN结构体的字符串
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string ConvertToZFI_EXTEN(ZFI_EXTEN model)
        {
            string str = string.Empty;
            str += ConvertToString(model.ITEMNO_ACC.ToString(), 10);
            str += ConvertToString(model.RSTGR, 3);
            str += ConvertToString(model.BSCHL, 2);
            str += ConvertToString(model.UMSKZ, 1);
            str += ConvertToString(model.ANBWA, 3);
            str += ConvertToString(model.ANLN1, 12);
            str += ConvertToString(model.ANLN2, 4);
            str += ConvertToString(model.XNEGP, 1);
            str += ConvertToString(model.PO_NUMBER, 10);
            str += ConvertToString(model.PO_ITEM.ToString(), 5);
            return str;
        }


        public string ConvertToString(string originalValue, int valueLength)
        {
            int originalLength = 0;
            if (!string.IsNullOrEmpty(originalValue))
            {
                originalLength = originalValue.Length;
            }
            string addValue = string.Empty;
            for (int i = 0; i < valueLength - originalLength; i++)
            {
                if (string.IsNullOrEmpty(originalValue))
                {
                    addValue += " ";
                }
                else
                {
                    addValue += "0";
                }

            }
            originalValue = (addValue + originalValue);
            return originalValue;
        }


        public string GetProofType()
        {
            string sql = "select VALUE as NAME from fee_sapinfo where  appname='proof'";
            var obj = DbContext.Database.SqlQuery<string>(sql).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(obj);
        }


        /// <summary>
        /// (贷方)根据银行科目号和公司代码获取利润中心
        /// </summary>
        /// <param name="subjectNo"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public string GetProfitWithSubject(string subjectNo, string companyCode)
        {
            string sql = "select VALUE as CODE from fee_sapinfo where  appname='bank' and DESCRIPTION='" + subjectNo + "' and  companyCode='" + companyCode + "' ";
            var obj = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            if (obj == null)
            {
                string newSql = "select VALUE as CODE from fee_sapinfo where  appname='Defaultprofit' and  companyCode='" + companyCode + "' ";
                var newObj = DbContext.Database.SqlQuery<string>(newSql).FirstOrDefault();
                return newObj;
            }
            return obj;
        }



        public string GetTaxInfoProfit(string companycode)
        {
            string str = String.Empty;
            switch (companycode)
            {
                case "1000":
                    str = "200100";
                    break;
                case "1300":
                    str = "700100";
                    break;
                case "2000":
                    str = "100100";
                    break;
                case "2100":
                    str = "400100";
                    break;
                case "4000":
                    str = "280000";
                    break;
                default:
                    break;
            }
            return str;
        }


        /// <summary>
        /// 借方获取利润中心
        /// </summary>
        /// <returns></returns>
        public string GetProfitWithBorrowOwner(int IsHeadOffice, string costerCenter, string departmentCode, string shopCode)
        {

            if (string.IsNullOrEmpty(shopCode))
            {
                var temp = Convert.ToDecimal(departmentCode);
                shopCode = DbContext.DEPARTMENT.Where(c => c.ID == temp).Select(x => x.CODE).FirstOrDefault();
            }

            //总部根据成本中心获取
            if (IsHeadOffice == 1)
            {
                string sql = "select PROFIT from FEE_SAPDATA where KOSTL like '%" + costerCenter + "%'";
                var value = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                return value;
            }
            //片区根据四位编码获取
            else
            {
                string sql = "select VALUE from FEE_SAPINFO where SHOPCODE='" + shopCode + "' and APPNAME='profit'  ";
                var value = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                return value;
            }
        }


        public string GetSubjectType(string keyWord)
        {
            if (string.IsNullOrEmpty(keyWord) || keyWord.ToUpper() == "S")
            {
                return "S";
            }
            else if (keyWord.ToUpper() == "D")
            {
                return "D";
            }
            else if (keyWord.ToUpper() == "K")
            {
                return "K";
            }
            return "S";
        }


        /// <summary>
        /// 获取所有科目名称
        /// </summary>
        /// <returns></returns>
        public string GetAllSubject(string query)
        {
            var name1 = DbContext.FEE_ACCOUNT.Where(c => c.NAME != null).Select(x => x.SAPNAME).ToList();  //科目名称
            var name2 = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME != null & c.COMPANYCODE == query).Select(x => x.ACCOUNTNAME).ToList();  //银行
            name1.AddRange(name2);
            name1.Add("进项税(非专卖店)");
            name1.Add("进项税(专卖店)");
            name1 = name1.Distinct().ToList();
            return Public.JsonSerializeHelper.SerializeToJson(name1);
        }


        public string ReturnHeadTextV1(string ceator, DateTime time, int isDebt)
        {
            if (isDebt > 0)
            {
                return String.Format("{0}-{1}年{2}月办结", ceator, time.Year, time.Month);
            }
            else
            {
                return String.Format("{0}-{1}年{2}月{3}日办结", ceator, time.Year, time.Month, time.Day);
            }
        }


        public string ReturnHeadText(string ceator, string billNo)
        {
            return String.Format("{0}-{1}", ceator, billNo);
        }


        public string ReturnRemark(string departmentName, string shopName, DateTime time, string Remark, DateTime time2, string BorrowNo)
        {

            string str = String.Empty;

            if (!string.IsNullOrEmpty(BorrowNo))
            {
                str += BorrowNo;
            }

            if (time2 != new DateTime())
            {
                time = time2;
            }

            if (string.IsNullOrEmpty(Remark))
            {
                if (string.IsNullOrEmpty(shopName))
                {
                    str += String.Format("{0}{1}年{2}月", departmentName, time.Year, time.Month);
                }
                else
                {
                    str += String.Format("{0}{1}年{2}月", shopName, time.Year, time.Month);
                }
            }
            else
            {
                if (Remark.Length > 25)
                {
                    str += Remark.Remove(25);
                }
                else
                {
                    str += Remark;
                }
            }

            return str;
        }


        /// <summary>
        /// 获取配置--预付原料,内部转账,固定资产
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<string> GetSystemConfiguration(string name)
        {
            List<string> str = new List<string>();
            string sql = "SELECT VALUE FROM FEE_PERSON_EXTEND where TYPE='Items' and PARAMETERONE='" + name + "'";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            if (!string.IsNullOrEmpty(Database))
            {
                str = Database.Split(',').ToList();
                str.Remove("");
            }
            return str;
        }

        private string GetSystemConfigurationStr(string name)
        {
            List<string> str = new List<string>();
            string sql = "SELECT VALUE FROM FEE_PERSON_EXTEND where TYPE='Items' and PARAMETERONE='" + name + "'";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();

            return Database;
        }



        /// <summary>
        /// 填充SAP数据
        /// </summary>
        /// <returns></returns>
        public string FillSapData(string BillNo, string BillType, string companyCode)
        {
            string msg = string.Empty;
            string error = string.Empty;
            var model = ConvertToPublicModel(BillNo, BillType);
            SubmitSapModel sapModel = new SubmitSapModel();

            List<string> ErrorList = GetSystemConfiguration("预付原料");
            List<string> ErrorList1 = GetSystemConfiguration("固定资产");
            List<string> ErrorList2 = GetSystemConfiguration("内部转账");

            var errorModel = model.Items.Where(c => ErrorList.Contains(c.name) && string.IsNullOrEmpty(c.ContractNum)).FirstOrDefault();
            if (errorModel != null)
            {
                error = "1";
                msg = "预付采购生产合同号不能为空！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            var errorModel1 = model.Items.Where(c => ErrorList1.Contains(c.name) && string.IsNullOrEmpty(c.AssetsNum)).FirstOrDefault();
            if (errorModel1 != null)
            {
                error = "1";
                msg = "固定资产资产号不能为空！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            bool IsError = model.Items.Where(c => ErrorList2.Contains(c.name)).FirstOrDefault() != null && model.Items.Count > 1;
            if (IsError)
            {
                error = "1";
                msg = "内部转账行项目只允许单行！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            var errorModel2 = model.Items.Where(c => ErrorList2.Contains(c.name) && (string.IsNullOrEmpty(c.shiftBank) || string.IsNullOrEmpty(c.TurnoutBank))).FirstOrDefault();
            if (errorModel2 != null)
            {
                error = "1";
                msg = "内部转账转入和转出行不能为空！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            else
            {
                var errorModel3 = model.Items.Where(c => ErrorList2.Contains(c.name)).FirstOrDefault();
                if (errorModel3 != null)
                {
                    var zryh = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME.Contains(errorModel3.shiftBank)).FirstOrDefault();
                    var zcyh = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME.Contains(errorModel3.TurnoutBank)).FirstOrDefault();
                    if (zryh == null)
                    {
                        error = "1";
                        msg = "内部转账转入行有误！";
                        return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                    }
                    if (zcyh == null)
                    {
                        error = "1";
                        msg = "内部转账转出行有误！";
                        return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                    }
                }
            }

            sapModel = PublicMethod(model, companyCode, false);

            error = "0";
            msg = "成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg, data = sapModel, IsHeadOffice = model.PersonInfo.IsHeadOffice });
        }




        private bool JudgeIsBelongItems(string ItemsName, string name)
        {
            var itmes = GetSystemConfigurationStr(ItemsName);
            if (!itmes.Contains(name))
            {
                return false;
            }
            var list = itmes.Split(',').ToList();
            list.Remove("");
            foreach (var item in list)
            {
                if (item == name)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 借贷双方的方法（是否是批量生成）
        /// </summary>
        /// <param name="model"></param>
        /// <param name="companyCode"></param>
        /// <param name="IsBatch">是否批量</param>
        /// <returns></returns>
        private SubmitSapModel PublicMethod(FeeBillModelRef model, string companyCode, bool IsBatch)
        {
            SubmitSapModel sapModel = new SubmitSapModel();
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();

            sapModel.CompanyCode = model.PersonInfo.CompanyCode;
            if (!string.IsNullOrEmpty(companyCode))
            {
                sapModel.CompanyCode = companyCode;
            }
            if (model.SpecialAttribute.MarketDebt == 1)
            {
                sapModel.Remark = ReturnRemark(model.PersonInfo.Department, model.PersonInfo.Shop, model.TransactionDate, "", model.CountTime.CountEndTime, model.BorrowBillNo);
            }
            else if (model.PageName.ToUpper() == "NOTICEBILL")
            {
                sapModel.Remark = ReturnRemark(model.PersonInfo.Department, model.PersonInfo.Shop, model.TransactionDate, model.Remark, model.CountTime.CountEndTime, model.BorrowBillNo);
            }
            else
            {
                sapModel.Remark = ReturnRemark(model.PersonInfo.Department, model.PersonInfo.Shop, model.TransactionDate, "", model.CountTime.CountEndTime, model.BorrowBillNo);
            }
            sapModel.TransactionDate = Convert.ToDateTime(model.ApprovalTime);
            sapModel.ApprovalTime = new DateTime(sapModel.TransactionDate.Year, sapModel.TransactionDate.Month, 1).AddMonths(1).AddDays(-1);
            sapModel.Head_txt = ReturnHeadText(employee.EmployeeNo, model.BillNo);
            sapModel.BillNo = model.BillNo;
            sapModel.PageName = model.PageName;

            var NBZZ = GetSystemConfiguration("内部转账");

            List<Sp1> items = new List<Sp1>();

            //内部转账
            var NbzzModel = model.Items.Where(c => NBZZ.Contains(c.name)).FirstOrDefault();
            if (NbzzModel != null)
            {
                var s1 = model.Items[0];
                var zryh = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME.Contains(s1.shiftBank)).FirstOrDefault();
                var zcyh = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME.Contains(s1.TurnoutBank)).FirstOrDefault();
                if (zryh != null && zcyh != null)
                {
                    Sp1 sp_Temp = new Sp1();
                    var temp = DbContext.FEE_ACCOUNT.Where(c => c.NAME == s1.name || c.OLDNAME == s1.name).FirstOrDefault();

                    //公司间内部转账，借银行贷银行
                    if (zryh.COMPANYCODE == zcyh.COMPANYCODE)
                    {
                        sp_Temp.OriginalName = zryh.ACCOUNTNAME;
                        sp_Temp.SapName = zryh.ACCOUNTNAME;
                        sp_Temp.Code = zryh.ACCOUNTCODE;
                        sp_Temp.ChargeCode = "107";
                        sp_Temp.ProofType = "SA";
                        sp_Temp.SubjectType = "S";
                        sp_Temp.ProfitCenter = GetProfitWithSubject(zryh.ACCOUNTCODE, sapModel.CompanyCode);
                        sp_Temp.CoinType = model.Currency.Code;
                        sp_Temp.ReasonCode = "101";
                        sp_Temp.Money = s1.money;
                        items.Add(sp_Temp);
                    }
                    //跨公司转账，借往来，贷银行
                    else
                    {
                        sp_Temp.OriginalName = zryh.ACCOUNTNAME;
                        sp_Temp.SapName = zryh.ACCOUNTNAME;
                        sp_Temp.Code = "V" + zryh.COMPANYCODE;
                        sp_Temp.ChargeCode = "";
                        sp_Temp.ProofType = "KZ";
                        sp_Temp.SubjectType = "K";
                        sp_Temp.ProfitCenter = "";
                        sp_Temp.CoinType = model.Currency.Code;
                        sp_Temp.Money = s1.money;
                        items.Add(sp_Temp);
                    }

                    //贷方
                    Sp1 sp_Temp1 = new Sp1();
                    sp_Temp1.OriginalName = zcyh.ACCOUNTNAME;
                    sp_Temp1.SapName = zcyh.ACCOUNTNAME;
                    sp_Temp1.Code = zcyh.ACCOUNTCODE;
                    sp_Temp1.ChargeCode = "50";
                    sp_Temp1.ProofType = "SA";
                    sp_Temp1.SubjectType = "S";
                    sp_Temp1.ProfitCenter = GetProfitWithSubject(zryh.ACCOUNTCODE, sapModel.CompanyCode);
                    sp_Temp1.Money = s1.money * -1;
                    sp_Temp1.CoinType = model.Currency.Code;
                    items.Add(sp_Temp1);
                    if (zryh.COMPANYCODE == zcyh.COMPANYCODE)
                    {
                        sp_Temp1.ReasonCode = "107";
                    }
                    else
                    {
                        sp_Temp1.ReasonCode = "207";
                    }

                    sapModel.Items = items;
                    //跟新分配号
                    sapModel.Items.ForEach(c => c.ALLOC_NMBR = model.BillNo);
                }
                return sapModel;
            }

            if (model.PageName.ToUpper() == "BORROWBILL")
            {
                Sp1 sp_Temp = new Sp1();
                var newName = model.CollectionInfo.Name;
                sp_Temp.OriginalName = newName;
                sp_Temp.SapName = newName;
                sp_Temp.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == newName && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                sp_Temp.ChargeCode = "25";
                sp_Temp.ProofType = "KZ";
                sp_Temp.SubjectType = "K";
                sp_Temp.ProfitCenter = "";
                sp_Temp.CoinType = model.Currency.Code;
                sp_Temp.Money = model.Items.Sum(c => c.money + c.taxmoney);
                //AUM借款单
                if (sapModel.CompanyCode == "1300")
                {
                    var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(newName) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();

                    sp_Temp.OriginalName = temp1 == null ? "" : temp1.NAME;
                    sp_Temp.SapName = temp1 == null ? "" : temp1.NAME;
                    sp_Temp.Code = temp1 == null ? "" : temp1.CODE;
                }

                items.Add(sp_Temp);
            }
            else
            {
                //借
                foreach (var item in model.Items)
                {
                    var temp = DbContext.FEE_ACCOUNT.Where(c => c.NAME == item.name || c.OLDNAME == item.name).FirstOrDefault();
                    bool IsTrue = IsFeeSubject(temp.ACCOUNT);
                    Sp1 sp_Temp = new Sp1();
                    //金额作为一行
                    if (item.money > 0)
                    {
                        sp_Temp.OriginalName = temp.NAME;
                        sp_Temp.SapName = temp.SAPNAME;
                        sp_Temp.Money = item.money;
                        if (IsTrue)
                        {
                            sp_Temp.CosterCenter = model.PersonInfo.CostCenter;
                        }
                        sp_Temp.ProfitCenter = GetProfitWithBorrowOwner(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter, model.PersonInfo.DepartmentCode, model.PersonInfo.ShopCode);
                        sp_Temp.SubjectType = GetSubjectType(temp.ACCOUNT_WORD);
                        sp_Temp.CoinType = model.Currency.Code;
                        sp_Temp.ChargeCode = temp.ACCOUNTCODE_BORROW;
                        sp_Temp.BSCHL_Sign = temp.ACCOUNT_SIGN;
                        sp_Temp.ProofType = temp.PROOF_TYPE;

                        //还款单凭证类型为AB
                        if (model.PageName.ToUpper() == "REFUNDBILL")
                        {
                            sp_Temp.ProofType = "AB";
                        }

                        if (sp_Temp.SubjectType == "K")
                        {
                            //判断是否为付款通知书
                            if (!string.IsNullOrEmpty(model.ProviderCode))
                            {
                                sp_Temp.Code = model.ProviderCode;
                                sp_Temp.OriginalName = model.ProviderName;
                                sp_Temp.SapName = model.ProviderName;

                                //预付原料需要合同号,且是总账类型
                                if (JudgeIsBelongItems("预付原料", item.name))
                                {
                                    sp_Temp.PO_NUMBER = item.ContractNum;
                                    sp_Temp.HideName = item.name;
                                }
                            }
                            else
                            {
                                var newName = model.CollectionInfo.Name;
                                sp_Temp.OriginalName = newName;
                                sp_Temp.SapName = newName;
                                sp_Temp.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == newName && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                            }

                            if (item.name == "店柜备用金")
                            {
                                var newName = model.CollectionInfo.Name;
                                if (sapModel.CompanyCode == "1300")
                                {
                                    var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(newName) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();

                                    sp_Temp.OriginalName = temp1 == null ? "" : temp1.NAME;
                                    sp_Temp.SapName = temp1 == null ? "" : temp1.NAME;
                                    sp_Temp.Code = temp1 == null ? "" : temp1.CODE;
                                }
                                else
                                {
                                    string name = model.PersonInfo.Department;
                                    string newName1 = string.Empty;
                                    if (name.Contains("片区"))
                                    {
                                        newName1 = System.Text.RegularExpressions.Regex.Split(name, "片区", System.Text.RegularExpressions.RegexOptions.IgnoreCase)[0];
                                    }
                                    else if (name.Contains("会所"))
                                    {
                                        int end = name.IndexOf("所");
                                        newName1 = name.Remove(end + 1);
                                    }
                                    newName = newName1 + "-" + newName;
                                    var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == newName && c.GROUP_TYPE == "Z003").FirstOrDefault();
                                    sp_Temp.OriginalName = temp1 == null ? "" : temp1.NAME;
                                    sp_Temp.SapName = temp1 == null ? "" : temp1.NAME;
                                    sp_Temp.Code = temp1 == null ? "" : temp1.CODE;
                                }
                            }
                            else if (item.name == "办事处押金")
                            {
                                string name = model.PersonInfo.Department;
                                string newName = name.Replace("片区", ",").Replace("会所", ",").Split(',')[0];
                                newName = newName + "办事处";
                                var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Contains(newName) && c.GROUP_TYPE == "Z005").FirstOrDefault();
                                sp_Temp.OriginalName = temp1 == null ? "" : temp1.NAME;
                                sp_Temp.SapName = temp1 == null ? "" : temp1.NAME;
                                sp_Temp.Code = temp1 == null ? "" : temp1.CODE;
                            }
                            else if (item.name != "员工备用金" && string.IsNullOrEmpty(model.ProviderCode))
                            {
                                //sp_Temp.OriginalName = "";
                                //sp_Temp.SapName = "";
                                //sp_Temp.Code = "";
                            }
                        }
                        else if (sp_Temp.SubjectType == "D")
                        {
                            sp_Temp.Code = DbContext.FEE_SHOP_CUSTOMER.Where(c => c.SHOPCODE == model.PersonInfo.ShopCode && c.SHOPCODE != null).Select(x => x.CUSTOMERNO).FirstOrDefault();
                        }
                        else
                        {
                            sp_Temp.Code = ConvertAccountCode(temp.ACCOUNT);
                        }
                        //固定资产
                        if (JudgeIsBelongItems("固定资产", item.name))
                        {
                            sp_Temp.AssetsNum = item.AssetsNum;
                        }
                    }
                    else
                    {
                        //赔偿金额，作为贷方
                        if (item.name.Contains("赔款"))
                        {
                            sp_Temp.OriginalName = temp.NAME;
                            sp_Temp.SapName = temp.SAPNAME;
                            sp_Temp.Money = item.money;
                            if (IsTrue)
                            {
                                sp_Temp.CosterCenter = model.PersonInfo.CostCenter;
                            }
                            sp_Temp.ProfitCenter = GetProfitWithBorrowOwner(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter, model.PersonInfo.DepartmentCode, model.PersonInfo.ShopCode);
                            sp_Temp.SubjectType = GetSubjectType(temp.ACCOUNT_WORD);
                            sp_Temp.CoinType = model.Currency.Code;
                            sp_Temp.ChargeCode = temp.ACCOUNTCODE_LOAN;
                            sp_Temp.BSCHL_Sign = temp.ACCOUNT_SIGN;
                            sp_Temp.ProofType = temp.PROOF_TYPE;
                            sp_Temp.Code = temp.ACCOUNT;
                            sp_Temp.SpareRemark = item.DispatchNum + "-" + item.name + "总金额" + (item.money + item.taxmoney);
                        }
                    }
                    //如果税额不为零，另外起一行
                    if (item.taxmoney > 0)
                    {
                        var str = temp.ACCOUNT.Remove(2);
                        string strList = "1601010000,1601050000,1601040000,1601030000,1601020000,1605010000,1605010000,1605010000, 1701010000,1701010000";
                        //不是所有的税额都双行显示
                        if (strList.Contains(temp.ACCOUNT) || str == "66")
                        {
                            Sp1 sp_Temp1 = new Sp1();
                            sp_Temp1.Money = item.taxmoney;
                            sp_Temp1.ProfitCenter = GetTaxInfoProfit(sapModel.CompanyCode);  //进项税获取利润中心
                            sp_Temp1.SapName = "进项税(非专卖店)";
                            sp_Temp1.Code = "2221010200";
                            sp_Temp1.SubjectType = GetSubjectType(temp.ACCOUNT_WORD);
                            sp_Temp1.CoinType = model.Currency.Code;
                            sp_Temp1.ChargeCode = "40";
                            sp_Temp1.BSCHL_Sign = temp.ACCOUNT_SIGN;
                            sp_Temp1.ProofType = temp.PROOF_TYPE;
                            sp_Temp1.InvoiceNum = item.InvoiceNum;
                            items.Add(sp_Temp);
                            items.Add(sp_Temp1);
                        }
                        else
                        {
                            sp_Temp.Money = item.money + item.taxmoney;
                            items.Add(sp_Temp);
                        }
                    }
                    else
                    {
                        items.Add(sp_Temp);
                    }
                }
            }

            //还款单贷方
            if (model.PageName.ToUpper() == "REFUNDBILL")
            {
                var BModel = new BorrowBill().GetBillModel(model.BorrowBillNo);

                decimal totalMoney = BModel.Items.Sum(c => c.money + c.taxmoney);

                bool IsLast = IsLastRefundBill(model.BorrowBillNo, model.ApprovalTime, ref totalMoney);

                var obj1 = GetSubjectCode(sapModel.CompanyCode, model.ProviderName);
                var bank = DbContext.FEE_BANK.Where(c => c.ACCOUNTCODE == obj1.CODE).FirstOrDefault();

                if (IsLast)
                {
                    //借款小于实际用款（费用报销大于借款）--2贷1借
                    if (totalMoney < 0)
                    {
                        //贷其他应收
                        Sp1 sp_temp1 = new Sp1();
                        sp_temp1.ChargeCode = "37";
                        sp_temp1.Money = -1 * (model.Items.Sum(c => c.money + c.taxmoney) + totalMoney);
                        sp_temp1.SapName = "其他应收款";
                        sp_temp1.ProofType = "AB";
                        sp_temp1.CoinType = BModel.Currency.Code;
                        sp_temp1.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                        sp_temp1.SubjectType = "K";
                        if (sapModel.CompanyCode == "1300")
                        {
                            var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                            sp_temp1.OriginalName = temp1 == null ? "" : temp1.NAME;
                            sp_temp1.Code = temp1 == null ? "" : temp1.CODE;
                        }
                        else
                        {
                            sp_temp1.OriginalName = BModel.CollectionInfo.Name;
                            sp_temp1.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                        }
                        items.Add(sp_temp1);

                        var firstName = model.Items[0].name;
                        var temp = DbContext.FEE_ACCOUNT.Where(c => c.NAME == firstName || c.OLDNAME == firstName).FirstOrDefault();
                        //贷银行
                        Sp1 sp_temp2 = new Sp1();
                        sp_temp2.ChargeCode = "50";
                        sp_temp2.Money = totalMoney;
                        sp_temp2.SapName = bank.ACCOUNTNAME;
                        sp_temp2.ProofType = "AB";
                        sp_temp2.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                        sp_temp2.Code = bank.ACCOUNTCODE;
                        sp_temp2.CoinType = BModel.Currency.Code;
                        sp_temp2.SubjectType = "S";
                        sp_temp2.ReasonCode = temp.REASON_CODE;
                        items.Add(sp_temp2);
                    }
                    else
                    {
                        //未清完借款  贷方（2借1贷）
                        if (totalMoney > 0)
                        {
                            //借
                            Sp1 sp_temp1 = new Sp1();
                            sp_temp1.Money = totalMoney;
                            sp_temp1.ChargeCode = "24";
                            sp_temp1.ProofType = "AB";
                            sp_temp1.CoinType = BModel.Currency.Code;
                            sp_temp1.SubjectType = "K";
                            sp_temp1.SapName = "其他应收-员工借款";
                            sp_temp1.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                            if (sapModel.CompanyCode == "1300")
                            {
                                var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                                sp_temp1.OriginalName = temp1 == null ? "" : temp1.NAME;
                                sp_temp1.Code = temp1 == null ? "" : temp1.CODE;
                            }
                            else
                            {
                                sp_temp1.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                                sp_temp1.OriginalName = BModel.CollectionInfo.Name;
                            }
                            items.Add(sp_temp1);

                            //贷
                            Sp1 sp_temp2 = new Sp1();
                            sp_temp2.ChargeCode = "37";
                            sp_temp2.SapName = "其他应收款";
                            sp_temp2.ProofType = "AB";
                            sp_temp2.CoinType = BModel.Currency.Code;
                            sp_temp2.Money = -1 * (model.Items.Sum(c => c.money + c.taxmoney) + totalMoney);
                            sp_temp2.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                            sp_temp2.SubjectType = "K";
                            if (sapModel.CompanyCode == "1300")
                            {
                                var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                                sp_temp2.OriginalName = temp1 == null ? "" : temp1.NAME;
                                sp_temp2.Code = temp1 == null ? "" : temp1.CODE;
                            }
                            else
                            {
                                sp_temp2.OriginalName = BModel.CollectionInfo.Name;
                                sp_temp2.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                            }
                            items.Add(sp_temp2);
                        }
                        //刚好清空借款
                        else
                        {
                            Sp1 sp_temp1 = new Sp1();
                            sp_temp1.ChargeCode = "37";
                            sp_temp1.SapName = "其他应收款";
                            sp_temp1.ProofType = "KZ";
                            sp_temp1.CoinType = BModel.Currency.Code;
                            sp_temp1.Money = -1 * model.Items.Sum(c => c.money + c.taxmoney);
                            sp_temp1.SubjectType = "K";
                            sp_temp1.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                            if (sapModel.CompanyCode == "1300")
                            {
                                var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                                sp_temp1.OriginalName = temp1 == null ? "" : temp1.NAME;
                                sp_temp1.Code = temp1 == null ? "" : temp1.CODE;
                            }
                            else
                            {
                                sp_temp1.OriginalName = BModel.CollectionInfo.Name;
                                sp_temp1.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                            }
                            items.Add(sp_temp1);
                        }
                    }
                }
                else
                {
                    //借
                    Sp1 sp_temp1 = new Sp1();
                    sp_temp1.Money = totalMoney;
                    sp_temp1.ChargeCode = "24";
                    sp_temp1.ProofType = "AB";
                    sp_temp1.CoinType = BModel.Currency.Code;
                    sp_temp1.SubjectType = "K";
                    sp_temp1.SapName = "其他应收-员工借款";
                    sp_temp1.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                    if (sapModel.CompanyCode == "1300")
                    {
                        var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                        sp_temp1.OriginalName = temp1 == null ? "" : temp1.NAME;
                        sp_temp1.Code = temp1 == null ? "" : temp1.CODE;
                    }
                    else
                    {
                        sp_temp1.OriginalName = BModel.CollectionInfo.Name;
                        sp_temp1.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                    }
                    items.Add(sp_temp1);


                    //贷
                    Sp1 sp_temp2 = new Sp1();
                    sp_temp2.ChargeCode = "37";
                    sp_temp2.SapName = "其他应收款";
                    sp_temp2.ProofType = "AB";
                    sp_temp2.CoinType = BModel.Currency.Code;
                    sp_temp2.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                    sp_temp2.Money = -1 * (model.Items.Sum(c => c.money + c.taxmoney) + totalMoney);
                    sp_temp2.SubjectType = "K";

                    if (sapModel.CompanyCode == "1300")
                    {
                        var temp1 = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim().Contains(BModel.CollectionInfo.Name) && c.GROUP_TYPE == "Z003" && c.NAME.Trim().ToUpper().Contains("AUM")).FirstOrDefault();
                        sp_temp2.OriginalName = temp1 == null ? "" : temp1.NAME;
                        sp_temp2.Code = temp1 == null ? "" : temp1.CODE;
                    }
                    else
                    {
                        sp_temp2.OriginalName = BModel.CollectionInfo.Name;
                        sp_temp2.Code = DbContext.FEE_PROVIDER.Where(c => c.NAME.Trim() == BModel.CollectionInfo.Name && c.GROUP_TYPE == "Z003").Select(x => x.CODE).FirstOrDefault();
                    }
                    items.Add(sp_temp2);
                }
            }
            //适用于非还款单贷方
            else
            {
                var obj1 = GetSubjectCode(sapModel.CompanyCode, model.ProviderName);

                //是商场账扣
                if (model.PageName.ToUpper() == "FEEBILL" && model.SpecialAttribute.MarketDebt == 1)
                {
                    Sp1 sp_temp1 = new Sp1();
                    sp_temp1.Code = DbContext.FEE_SHOP_CUSTOMER.Where(c => c.SHOPCODE == model.PersonInfo.ShopCode && c.SHOPCODE != null).Select(x => x.CUSTOMERNO).FirstOrDefault();
                    sp_temp1.OriginalName = model.PersonInfo.Shop;
                    sp_temp1.Money = -1 * model.Items.Sum(c => c.money + c.taxmoney);
                    sp_temp1.SapName = "应收账款";
                    sp_temp1.CoinType = model.Currency.Code;
                    sp_temp1.ProfitCenter = GetProfitWithBorrowOwner(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter, model.PersonInfo.DepartmentCode, model.PersonInfo.ShopCode);
                    sp_temp1.SubjectType = "D";
                    sp_temp1.ProofType = "DR";
                    sp_temp1.ChargeCode = "18";
                    if (sapModel.CompanyCode == "2100")
                    {
                        sp_temp1.ChargeCode = "15";
                    }
                    items.Add(sp_temp1);
                }
                ////银行账扣
                //else if (model.PageName.ToUpper() == "FEEBILL" && model.SpecialAttribute.BankDebt == 1)
                //{


                //}
                //押金账扣
                else if (model.PageName.ToUpper() == "FEEBILL" && model.SpecialAttribute.Cash == 1)
                {

                }
                else
                {
                    //贷银行数据
                    var firstName = model.Items[0].name;
                    var temp = DbContext.FEE_ACCOUNT.Where(c => c.NAME == firstName || c.OLDNAME == firstName).FirstOrDefault();
                    var bank = DbContext.FEE_BANK.Where(c => c.ACCOUNTCODE == obj1.CODE).FirstOrDefault();
                    Sp1 sp_temp1 = new Sp1();
                    sp_temp1.SapName = bank.ACCOUNTNAME;
                    sp_temp1.Money = -1 * model.Items.Sum(c => c.money + c.taxmoney);
                    sp_temp1.ProfitCenter = GetProfitWithSubject(bank.ACCOUNTCODE, sapModel.CompanyCode);
                    sp_temp1.ReasonCode = temp.REASON_CODE;
                    sp_temp1.ChargeCode = "50";
                    sp_temp1.Code = bank.ACCOUNTCODE;
                    sp_temp1.CoinType = model.Currency.Code;
                    sp_temp1.SubjectType = "S";
                    sp_temp1.ProofType = temp.PROOF_TYPE;
                    //sp_temp1.BSCHL_Sign = temp.ACCOUNT_SIGN;
                    items.Add(sp_temp1);

                }
            }
            //如果是批量
            if (IsBatch)
            {
                foreach (var item in items)
                {
                    string str = string.Empty;
                    if (!string.IsNullOrEmpty(item.SapName) && item.SapName.Contains("银行存款") && item.Money < 0)
                    {
                        str = String.Format("{0}报销总金额{1}", sapModel.Remark, item.Money * -1);
                    }
                    else if (item.OriginalName=="快递丢货赔款")
                    {
                        str = item.SpareRemark;
                    }
                    else
                    {
                        str = String.Format("{0}{1}金额{2}", sapModel.Remark, item.SapName, item.Money);
                    }
                    item.SpareRemark = str;
                }
            }
            sapModel.Items = items;
            //跟新分配号
            sapModel.Items.ForEach(c => c.ALLOC_NMBR = model.BillNo);
            return sapModel;
        }


        /// <summary>
        /// 判断是否为最后一个还款单
        /// </summary>
        /// <param name="billNo"></param>
        /// <param name="approvalTime"></param>
        /// <returns></returns>
        public bool IsLastRefundBill(string billNo, string approvalTime, ref decimal totalMoney)
        {
            bool IsTrue = true;
            DateTime lastTime = Convert.ToDateTime(approvalTime);
            var Recored = new RefundBill().GetRefundRecode(billNo);
            Recored = Recored.Where(c => c.ApprovalStatus == 2 && c.ApprovalTime != null).ToList();

            foreach (var item in Recored)
            {
                var temp = Convert.ToDateTime(item.ApprovalTime);

                if (lastTime >= temp)
                {
                    if (item.Items == null)
                    {
                        totalMoney -= item.RealRefundMoney;
                    }
                    else { totalMoney -= item.Items.Sum(c => c.money + c.taxmoney); }
                }
                else
                {
                    if (IsTrue)
                    {
                        IsTrue = false;
                    }
                }
            }

            return IsTrue;
        }


        public ObjectList GetSubjectCode(string companyCode, string providerName)
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

            string sql = "select VALUE as CODE,CURRENCY as NAME from FEE_SAPINFO where APPNAME='DefaultBank'  and COMPANYCODE='" + companyCode + "' and GRADE='" + grade + "'";
            var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();

            return Database;
        }


        public SAPShowData GetDataFromOracle(string billNo)
        {
            var model1 = DbContext.FEE_FEEBILL.Where(c => c.BILLNO == billNo).Select(c => new SAPShowData
            {
                BillNo = c.BILLNO,
                Brand = c.BRANDCODE,
                CostCenter = c.COST_ACCOUNT,
                BillType = "FeeBill",
                Department = c.BOOT_DP_NAME,
                DepartmentID = c.BOOT_DP_ID,
                CreateTime = c.CREATETIME,
                Owner = c.WORKNUMBER,
                TotalMoney = c.TOTALMONEY,
                Remark = c.REMARK,
                TransactionDate = c.TRANSACTIONDATE,
                ApprovalTime = c.APPROVALTIME,
                SAPProof = c.SAPPROOF,
                SapUploadTime = c.SAPUPLOADTIME,
                SapCreator = c.SAPCREATOR,
                CompanyCode = c.COMPANYCODE,
                Funds = c.IS_TEAMMONEY,
                Agent = c.IS_AGENTMONEY,
                BankDebt = c.IS_BANK_MINUS,
                MarketDebt = c.IS_MARKET_MINUS,
                Cash = c.IS_CASH_MINUS
            }).FirstOrDefault();
            if (model1 != null)
            {
                return model1;
            }

            var model2 = DbContext.FEE_NOTICEBILL.Where(c => c.BILLNO == billNo).Select(c => new SAPShowData
            {
                BillNo = c.BILLNO,
                Brand = c.BRANDCODE,
                CostCenter = c.COST_ACCOUNT,
                BillType = "NoticeBill",
                Department = c.BOOT_DP_NAME,
                DepartmentID = c.BOOT_DP_ID,
                CreateTime = c.CREATETIME,
                Owner = c.WORKNUMBER,
                TotalMoney = c.TOTALMONEY,
                Remark = c.REMARK,
                TransactionDate = c.TRANSACTIONDATE,
                ApprovalTime = c.APPROVALTIME,
                SAPProof = c.SAPPROOF,
                SapUploadTime = c.SAPUPLOADTIME,
                SapCreator = c.SAPCREATOR,
                CompanyCode = c.COMPANYCODE,
                Funds = c.IS_TEAMMONEY,
                Agent = c.IS_AGENTMONEY,
                BankDebt = c.IS_BANK_MINUS,
                MarketDebt = c.IS_MARKET_MINUS,
                Cash = c.IS_CASH_MINUS
            }).FirstOrDefault();
            if (model2 != null)
            {
                return model2;
            }
            var model3 = DbContext.FEE_BORROWBILL.Where(c => c.BILLNO == billNo).Select(c => new SAPShowData
            {
                BillNo = c.BILLNO,
                Brand = c.BRANDCODE,
                CostCenter = c.COST_ACCOUNT,
                BillType = "BorrowBill",
                Department = c.BOOT_DP_NAME,
                DepartmentID = c.BOOT_DP_ID,
                CreateTime = c.CREATETIME,
                Owner = c.WORKNUMBER,
                TotalMoney = c.TOTALMONEY,
                Remark = c.REMARK,
                TransactionDate = c.TRANSACTIONDATE,
                ApprovalTime = c.APPROVALTIME,
                SAPProof = c.SAPPROOF,
                SapUploadTime = c.SAPUPLOADTIME,
                SapCreator = c.SAPCREATOR,
                CompanyCode = c.COMPANYCODE,
                Funds = c.IS_TEAMMONEY,
                Agent = c.IS_AGENTMONEY,
                BankDebt = c.IS_BANK_MINUS,
                MarketDebt = c.IS_MARKET_MINUS,
                Cash = c.IS_CASH_MINUS
            }).FirstOrDefault();
            if (model3 != null)
            {
                return model3;
            }
            var model4 = DbContext.FEE_FEEREFUNDBILL.Where(c => c.BILLNO == billNo).Select(c => new SAPShowData
            {
                BillNo = c.BILLNO,
                Brand = c.BRANDCODE,
                CostCenter = c.COST_ACCOUNT,
                BillType = "RefundBill",
                Department = c.BOOT_DP_NAME,
                DepartmentID = c.BOOT_DP_ID,
                CreateTime = c.CREATETIME,
                Owner = c.WORKNUMBER,
                TotalMoney = c.TOTALMONEY,
                Remark = c.REMARK,
                TransactionDate = c.TRANSACTIONDATE,
                ApprovalTime = c.APPROVALTIME,
                SAPProof = c.SAPPROOF,
                SapUploadTime = c.SAPUPLOADTIME,
                SapCreator = c.SAPCREATOR,
                CompanyCode = c.COMPANYCODE,
                Funds = c.IS_TEAMMONEY,
                Agent = c.IS_AGENTMONEY,
                BankDebt = c.IS_BANK_MINUS,
                MarketDebt = c.IS_MARKET_MINUS,
                Cash = c.IS_CASH_MINUS
            }).FirstOrDefault();
            if (model4 != null)
            {
                return model4;
            }
            return null;
        }


        public FeeBillModelRef ConvertToPublicModelV1(string BillNo)
        {
            FeeBillModelRef model = new FeeBillModelRef();

            var model1 = new FeeBill().GetBillModel(BillNo);
            if (model1 != null)
            {
                model = model1.MapTo<FeeBillModel, FeeBillModelRef>();
                model.PageName = "FeeBill";
                return model;
            }
            var model2 = new NoticeBill().GetBillModel(BillNo);
            if (model2 != null)
            {
                model.BillNo = model2.BillNo;
                model.PageName = "NoticeBill";
                model.PersonInfo = model2.PersonInfo;
                model.TotalMoney = model2.TotalMoney;
                model.CreateTime = model2.CreateTime;
                model.TransactionDate = model2.TransactionDate;
                model.ApprovalTime = model2.ApprovalTime;
                model.ApprovalStatus = model2.ApprovalStatus;
                model.ApprovalPost = model2.ApprovalPost;
                model.Remark = model2.Remark;
                model.ProviderName = model2.ProviderInfo.ProviderName;
                model.Owner = model2.Owner;
                model.Creator = model2.Creator;
                model.SpecialAttribute = new SpecialAttribute();
                model.SpecialAttribute.Funds = model2.SpecialAttribute.Funds;
                model.SpecialAttribute.Agent = model2.SpecialAttribute.Agent;
                model.SpecialAttribute.Check = model2.SpecialAttribute.Check;
                model.Items = model2.Items;
                model.Currency = model2.Currency;
                model.WorkNumber = model2.WorkNumber;
                model.ProviderCode = model2.ProviderInfo.ProviderCode;
                model.Owner = model2.Owner;
                model.MissBill = model2.MissBill;
                model.CountTime = new CountTime();
                return model;
            }
            var model3 = new BorrowBill().GetBillModel(BillNo);
            if (model3 != null)
            {
                model.BillNo = model3.BillNo;
                model.PageName = "BorrowBill";
                model.PersonInfo = model3.PersonInfo;
                model.TotalMoney = model3.TotalMoney;
                model.CreateTime = model3.CreateTime;
                model.TransactionDate = model3.TransactionDate;
                model.ApprovalTime = model3.ApprovalTime;
                model.ApprovalStatus = model3.ApprovalStatus;
                model.ApprovalPost = model3.ApprovalPost;
                model.Remark = model3.Remark;
                model.Owner = model3.Owner;
                model.Creator = model3.Creator;
                model.SpecialAttribute = model3.SpecialAttribute;
                model.Items = model3.Items;
                model.Currency = model3.Currency;
                model.WorkNumber = model3.WorkNumber;
                model.Owner = model3.Owner;
                model.CollectionInfo = model3.CollectionInfo;
                model.CountTime = new CountTime();
                return model;
            }
            var model4 = new RefundBill().GetBillModel(BillNo);
            if (model4 != null)
            {
                model = model4.MapTo<RefundBillModel, FeeBillModelRef>();
                model.PageName = "RefundBill";
                return model;
            }
            return null;
        }


        public FeeBillModelRef ConvertToPublicModel(string BillNo, string BillType)
        {
            FeeBillModelRef model = new FeeBillModelRef();
            switch (BillType.ToUpper())
            {
                case "FEEBILL":
                    var feeModel = new FeeBill().GetBillModel(BillNo);
                    if (feeModel != null)
                    {
                        model = feeModel.MapTo<FeeBillModel, FeeBillModelRef>();
                        model.PageName = "FeeBill";
                    }
                    break;
                case "NOTICEBILL":
                    var noticeModel = new NoticeBill().GetBillModel(BillNo);
                    if (noticeModel != null)
                    {
                        model.BillNo = noticeModel.BillNo;
                        model.PageName = "NoticeBill";
                        model.PersonInfo = noticeModel.PersonInfo;
                        model.TotalMoney = noticeModel.TotalMoney;
                        model.CreateTime = noticeModel.CreateTime;
                        model.TransactionDate = noticeModel.TransactionDate;
                        model.ApprovalTime = noticeModel.ApprovalTime;
                        model.ApprovalStatus = noticeModel.ApprovalStatus;
                        model.ApprovalPost = noticeModel.ApprovalPost;
                        model.Remark = noticeModel.Remark;
                        model.ProviderName = noticeModel.ProviderInfo.ProviderName;
                        model.Owner = noticeModel.Owner;
                        model.Creator = noticeModel.Creator;
                        model.SpecialAttribute = new SpecialAttribute();
                        model.SpecialAttribute.Funds = noticeModel.SpecialAttribute.Funds;
                        model.SpecialAttribute.Agent = noticeModel.SpecialAttribute.Agent;
                        model.SpecialAttribute.Check = noticeModel.SpecialAttribute.Check;
                        model.Items = noticeModel.Items;
                        model.Currency = noticeModel.Currency;
                        model.WorkNumber = noticeModel.WorkNumber;
                        model.ProviderCode = noticeModel.ProviderInfo.ProviderCode;
                        model.Owner = noticeModel.Owner;
                        model.MissBill = noticeModel.MissBill;
                        model.CountTime = new CountTime();
                    }
                    break;
                case "BORROWBILL":
                    var borrowModel = new BorrowBill().GetBillModel(BillNo);
                    if (borrowModel != null)
                    {
                        model.BillNo = borrowModel.BillNo;
                        model.PageName = "BorrowBill";
                        model.PersonInfo = borrowModel.PersonInfo;
                        model.TotalMoney = borrowModel.TotalMoney;
                        model.CreateTime = borrowModel.CreateTime;
                        model.TransactionDate = borrowModel.TransactionDate;
                        model.ApprovalTime = borrowModel.ApprovalTime;
                        model.ApprovalStatus = borrowModel.ApprovalStatus;
                        model.ApprovalPost = borrowModel.ApprovalPost;
                        model.Remark = borrowModel.Remark;
                        model.Owner = borrowModel.Owner;
                        model.Creator = borrowModel.Creator;
                        model.SpecialAttribute = borrowModel.SpecialAttribute;
                        model.Items = borrowModel.Items;
                        model.Currency = borrowModel.Currency;
                        model.WorkNumber = borrowModel.WorkNumber;
                        model.Owner = borrowModel.Owner;
                        model.CollectionInfo = borrowModel.CollectionInfo;
                        model.CountTime = new CountTime();
                    }
                    break;
                case "REFUNDBILL":
                    var refundModel = new RefundBill().GetBillModel(BillNo);
                    if (refundModel != null)
                    {
                        model = refundModel.MapTo<RefundBillModel, FeeBillModelRef>();
                        model.PageName = "RefundBill";
                    }
                    break;
                default:
                    break;
            }

            return model;
        }



        public ObjectList GetTaxInfo(int IsHeadOffice, string CompanyCode, string ShopCode)
        {
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                ObjectList obj = new ObjectList();
                if (IsHeadOffice == 1)
                {
                    switch (employee.CompanyCode)
                    {
                        case "1000":
                            obj.CODE = "2221010200";
                            break;
                        case "1300":
                            obj.CODE = "2221010200";
                            break;
                        default:
                            obj.CODE = "2221010200";
                            break;
                    }
                    obj.NAME = "进项税(非专卖店)";  //总部的都是非专卖店
                }
                else
                {
                    bool IsStore = false;  //是否为专卖店
                    if (!string.IsNullOrEmpty(ShopCode))
                    {
                        var result = DbContext.SHOP.Where(c => c.CODE == ShopCode).FirstOrDefault();
                        if (result != null)
                        {
                            IsStore = result.SHOP_PROPERTY == "02" ? true : false;
                        }
                    }
                    if (IsStore)  //如果为专卖店
                    {
                        switch (CompanyCode)
                        {
                            case "1000":
                                obj.CODE = "2221010201";
                                break;
                            case "1300":
                                obj.CODE = "2221010201";
                                break;
                            default:
                                obj.CODE = "2221010201";
                                break;
                        }
                    }
                    else
                    {
                        switch (CompanyCode)
                        {
                            case "1000":
                                obj.CODE = "2221010200";
                                break;
                            case "1300":
                                obj.CODE = "2221010200";
                                break;
                            default:
                                obj.CODE = "2221010200";
                                break;
                        }
                    }
                    obj.NAME = string.Format("进项税{0}", IsStore.ToString().ToLower() == "true" ? "(专卖店)" : "(非专卖店)");
                }
                return obj;
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("获取进项税数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return null;
        }


        /// <summary>
        /// 更改
        /// </summary>
        /// <param name="name"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public string EditSapName(string name, string companyCode)
        {
            SapStatus sp = new SapStatus();
            var value1 = DbContext.FEE_ACCOUNT.Where(c => c.SAPNAME == name).Select(x => x.ACCOUNT).FirstOrDefault();
            if (!string.IsNullOrEmpty(value1))
            {
                sp.Status = 1;
                sp.Code = value1;
                return Public.JsonSerializeHelper.SerializeToJson(sp);
            }
            var value2 = DbContext.FEE_BANK.Where(c => c.ACCOUNTNAME == name && c.COMPANYCODE == companyCode).Select(x => x.ACCOUNTCODE).FirstOrDefault();  //银行
            if (!string.IsNullOrEmpty(value2))
            {
                sp.Status = 1;
                sp.Code = value2;
                sp.Profit = GetProfitWithSubject(sp.Code, companyCode);
            }
            if (name == "进项税(非专卖店)")
            {
                sp.Status = 1;
                sp.Code = "2221010200";
                return Public.JsonSerializeHelper.SerializeToJson(sp);
            }
            if (name == "进项税(专卖店)")
            {
                sp.Status = 1;
                sp.Code = "2221010201";
                return Public.JsonSerializeHelper.SerializeToJson(sp);
            }
            return Public.JsonSerializeHelper.SerializeToJson(sp);
        }


        public string BatchFillData(string BillNo, string companyCode)
        {
            string msg = string.Empty;
            string error = string.Empty;
            string str = BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
            var list = str.Split(',').ToList();
            list.Remove("");

            List<FeeBillModelRef> refList = new List<FeeBillModelRef>();

            List<string> ErrorList = GetSystemConfiguration("预付原料");
            List<string> ErrorList1 = GetSystemConfiguration("固定资产");
            List<string> ErrorList2 = GetSystemConfiguration("内部转账");

            foreach (var item in list)
            {
                var test = GetDataFromOracle(item);
                if (test == null)
                {
                    error = "1";
                    msg = item + "不存在";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
                if (!string.IsNullOrEmpty(test.SAPProof))
                {
                    error = "1";
                    msg = item + "已提交凭证";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }

                var temp = ConvertToPublicModelV1(item);
                refList.Add(temp);
            }
            var reBill = refList.GroupBy(c => c.BillNo).Where(x => x.Count() > 1).FirstOrDefault();
            if (reBill != null)
            {
                error = "1";
                msg = reBill.Key + "单号重复";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            var type = refList.GroupBy(c => c.PageName).ToList();
            if (type.Count != 1)
            {
                error = "1";
                msg = "不同类型单据不能混合提交！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }
            //还款单必须相同部门相同店
            if (type[0].Key.ToUpper() == "REFUNDBILL")
            {
                var DP = refList.GroupBy(c => c.PersonInfo.DepartmentCode).ToList();
                if (DP.Count != 1)
                {
                    error = "1";
                    msg = "还款单需处于同一部门！";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
                var MD = refList.GroupBy(c => c.PersonInfo.ShopCode).ToList();
                if (MD.Count != 1)
                {
                    error = "1";
                    msg = "还款单需处于同一门店！";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
            }
            var cc = refList.GroupBy(c => c.SpecialAttribute.MarketDebt).Count() + refList.GroupBy(c => c.SpecialAttribute.BankDebt).Count() + refList.GroupBy(c => c.SpecialAttribute.Cash).Count();
            if (cc > 3)
            {
                error = "1";
                msg = "账扣类费用不能混合提交！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            //判断账扣类费用不能混合一起提交
            int count = refList.Where(c => c.SpecialAttribute.MarketDebt == 1).ToList().Count >= 1 ? 1 : 0 + refList.Where(c => c.SpecialAttribute.BankDebt == 1).ToList().Count >= 1 ? 1 : 0 + refList.Where(c => c.SpecialAttribute.Cash == 1).ToList().Count >= 1 ? 1 : 0;
            if (count > 1)
            {
                error = "1";
                msg = "账扣类费用不能混合提交！";
                return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
            }

            //判读办结时间
            var orzTime = Convert.ToDateTime(refList[0].ApprovalTime);
            foreach (var item in refList)
            {
                var time = Convert.ToDateTime(item.ApprovalTime);
                bool IsOk = true;
                //非帐扣类
                if (count == 0)
                {
                    IsOk = orzTime.Year.Equals(time.Year) && orzTime.Month.Equals(time.Month) && orzTime.Day.Equals(time.Day);
                }
                else
                {
                    IsOk = orzTime.Year.Equals(time.Year) && orzTime.Month.Equals(time.Month);
                }

                if (IsOk == false)
                {
                    error = "1";
                    msg = "单号" + item.BillNo + "办结时间不同";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }

                var errorModel = item.Items.Where(c => ErrorList.Contains(c.name) && string.IsNullOrEmpty(c.ContractNum)).FirstOrDefault();
                if (errorModel != null)
                {
                    error = "1";
                    msg = "单号" + item.BillNo + "合同号为空！";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }

                var errorModel1 = item.Items.Where(c => ErrorList1.Contains(c.name) && string.IsNullOrEmpty(c.AssetsNum)).FirstOrDefault();
                if (errorModel1 != null)
                {
                    error = "1";
                    msg = "单号" + item.BillNo + "资产号为空！";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }

                bool IsError = item.Items.Where(c => ErrorList2.Contains(c.name)).FirstOrDefault() != null;
                if (IsError)
                {
                    error = "1";
                    msg = "内部转账请在查询单据处提交！";
                    return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
                }
            }


            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            //得到了多个实体对象
            List<SubmitSapModel> SubmitList = new List<SubmitSapModel>();

            foreach (var item in refList)
            {
                var model = PublicMethod(item, companyCode, true);
                SubmitList.Add(model);
            }

            SubmitSapModel sapModel = new SubmitSapModel();
            List<Sp1> sp1 = new List<Sp1>();
            foreach (var item in SubmitList)
            {
                sp1.AddRange(item.Items);
            }
            //统一贷方银行类科目
            var totalItems = sp1.Where(c => c.SapName.Contains("银行存款") && c.Money < 0).FirstOrDefault();
            if (totalItems != null)
            {
                var totalMoney = sp1.Where(c => c.SapName.Contains("银行存款") && c.Money < 0).Sum(x => x.Money);
                totalItems.ALLOC_NMBR = "";
                totalItems.Money = totalMoney;

                //refList.Max(c => c.Items.Max(x => (x.money + x.taxmoney)));
                decimal maxMoney = 0;
                string text = "";
                foreach (var item in refList)
                {
                    var temp = item.Items.OrderByDescending(c => (c.money + c.taxmoney)).FirstOrDefault();
                    if ((temp.money + temp.taxmoney) > maxMoney)
                    {
                        maxMoney = temp.money + temp.taxmoney;
                        text = temp.name + "等";
                    }
                }


                if (SubmitList[0].PageName.ToUpper() == "BORROWBILL")
                {
                    totalItems.SpareRemark = String.Format("{0}年{1}月{2}日借款总金额{3}，{4}", SubmitList[0].TransactionDate.Year, SubmitList[0].TransactionDate.Month, SubmitList[0].TransactionDate.Day, totalMoney * -1, text);
                }
                else
                {
                    totalItems.SpareRemark = String.Format("{0}年{1}月{2}日报销总金额{3}，{4}", SubmitList[0].TransactionDate.Year, SubmitList[0].TransactionDate.Month, SubmitList[0].TransactionDate.Day, totalMoney * -1, text);
                }

                sp1.RemoveAll(c => c.SapName.Contains("银行存款") && c.Money < 0);
                sp1.Add(totalItems);
            }

            sapModel.BillNo = BillNo;
            sapModel.CompanyCode = SubmitList[0].CompanyCode;
            sapModel.ApprovalTime = SubmitList[0].ApprovalTime;
            sapModel.TransactionDate = SubmitList[0].TransactionDate;
            sapModel.PageName = SubmitList[0].PageName;
            sapModel.Items = sp1;
            sapModel.Head_txt = ReturnHeadTextV1(employee.EmployeeNo, SubmitList[0].TransactionDate, count);

            //对借贷方数据进行整合
            error = "0";
            msg = "成功";
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg, data = sapModel, IsHeadOffice = refList[0].PersonInfo.IsHeadOffice });
        }
    }
}
