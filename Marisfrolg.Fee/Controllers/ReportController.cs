using Aspose.Cells;
using ICSharpCode.SharpZipLib.Zip;
using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WorkFlowEngine;

namespace Marisfrolg.Fee.Controllers
{
    public class ReportController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        private string ConvertDatas(string str)
        {
            var model = str.Trim().Split(',').ToList();
            string NewStr = "";
            foreach (var item in model)
            {
                NewStr += "'" + item + "',";
            }
            return NewStr.Remove(NewStr.Length - 1);
        }

        #region 费用率报表内容
        [AllowAnonymous]
        public ActionResult FeeReport()
        {
            return View();
        }

        /// <summary>
        /// 获取费用报表主数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="ReportID"></param>
        /// <returns></returns>
        public ActionResult GetFeeReport(DateTime startTime, DateTime endTime, string city)
        {
            List<FEE_Report_Show> ShowData = GetFeeReportData(startTime, endTime, city);

            return Json(ShowData, JsonRequestBehavior.AllowGet);
        }

        private List<FEE_Report_Show> GetFeeReportData(DateTime startTime, DateTime endTime, string city)
        {
            FEE_OracleTable OracleData = new FEE_OracleTable();

            endTime = endTime.AddDays(1).AddSeconds(-1);//增加一天,减去一秒

            List<FEE_TempTable> tempList = new List<FEE_TempTable>();
            //区分是总部还是片区
            if (!string.IsNullOrEmpty(city))
            {
                string sql = string.Format(@"
                select a.MF,a.SU,a.AUM,b.department_id,
                case when f.COMPANYCODE='1000' then 'MA'  when f.COMPANYCODE='2000' then 'SU'  when f.COMPANYCODE='1300' then 'AUM' else f.COMPANYCODE end BRAND,
                sum((case when e.CELL_250 is null then 0 else to_number(e.CELL_250) end)-(case when e.CELL_180 is null then 0 else to_number(e.CELL_180) end)-(case when e.CELL_407 is null then 0 else to_number(e.CELL_407) end)) as SALARY from 
                FEE_REPORT a left join
                Department_Report b  on a.city=b.name left join 
                department f on b.department_id=f.id left join 
                shop c on  b.department_id=c.DEPARTMENTID left join
                SALARY_FLOW d on c.code=d.SHOPCODE left join
                SALARY_FLOW_DETAIL e on  d.id=e.pid  and d.CREATETIME BETWEEN to_date('{1}','yyyymmdd') and  to_date('{2}','yyyymmdd')
                where a.city='{0}' 
                group by a.MF,a.SU,a.AUM,b.department_id,f.COMPANYCODE
              ", city, startTime.ToString("yyyyMMdd"), endTime.ToString("yyyyMMdd"));

                tempList = DbContext.Database.SqlQuery<FEE_TempTable>(sql).ToList();

                OracleData.MF = tempList[0].MF;
                OracleData.SU = tempList[0].SU;
                OracleData.AUM = tempList[0].AUM;

                foreach (var item in tempList)
                {
                    OracleData.Depart_ID.Add(item.DEPARTMENT_ID.ToString());
                }
            }

            List<string> companyCode = new List<string>() { "1000", "2000", "1300" };

            //1.0费用数据  
            var data1 = new FeeBill().GetFeeReportData(startTime, endTime, OracleData.Depart_ID, companyCode);
            var data2 = new NoticeBill().GetFeeReportData(startTime, endTime, OracleData.Depart_ID, companyCode);
            var data3 = new RefundBill().GetFeeReportData(startTime, endTime, OracleData.Depart_ID, companyCode);
            data1.AddRange(data2); data1.AddRange(data3);

            List<FEE_Report> BrandData = new List<FEE_Report>();
            FEE_Report MA_Data = new FEE_Report("MA");
            FEE_Report SU_Data = new FEE_Report("SU");
            FEE_Report AUM_Data = new FEE_Report("AUM");
            FEE_Report NoBrand_Data = new FEE_Report("");

            foreach (var item in data1.GroupBy(c => c.Brand))
            {
                var tempData = item.Select(c => c.Items).ToList();
                if (string.IsNullOrEmpty(item.Key))
                {
                    foreach (var item1 in tempData)
                    {
                        NoBrand_Data.Items.AddRange(item1);
                    }
                }
                else if (item.Key.Equals("MA"))
                {
                    foreach (var item1 in tempData)
                    {
                        MA_Data.Items.AddRange(item1);
                    }
                }
                else if (item.Key.Equals("SU"))
                {
                    foreach (var item1 in tempData)
                    {
                        SU_Data.Items.AddRange(item1);
                    }
                }
                else
                {
                    foreach (var item1 in tempData)
                    {
                        AUM_Data.Items.AddRange(item1);
                    }
                }
            }
            BrandData.Add(MA_Data);
            BrandData.Add(SU_Data);
            BrandData.Add(AUM_Data);
            BrandData.Add(NoBrand_Data);

            //2.0销售数据
            string SalesSql = string.Format(@"select SHOPLOGO as Brand,sum(C_AMOUNT) as MONEY
               FROM {0}
               where WADAT_IST >= '{1}'
               and WADAT_IST <= '{2}'
               AND  ZONECODE NOT IN('R0', 'R9', 'R10')
               and NAME_1 like '%{3}%' and SHOPLOGO in('MA', 'SU', 'AM')
               group by SHOPLOGO
            ",
            "\"_SYS_BIC\".\"MF_SALES.DAILYSALE/SALES_D_BRAND_FY\"",
            startTime.ToString("yyyy-MM-dd HH:mm:ss"),
            endTime.ToString("yyyy-MM-dd HH:mm:ss"),
            city
            );

            DataTable dt = Marisfrolg.Public.HanaSQLHelper.GetDataTable(SalesSql);

            List<SalesModel> SalesData = new List<SalesModel>();
            foreach (DataRow item in dt.Rows)
            {
                SalesModel temp = new SalesModel();
                temp.BRAND = item["BRAND"].ToString();
                temp.BRAND = temp.BRAND == "AM" ? "AUM" : temp.BRAND;
                temp.MONEY = item["MONEY"] == DBNull.Value ? 0 : Convert.ToDecimal(item["MONEY"]);
                SalesData.Add(temp);
            }

            //数据整合
            List<FEE_Report_Show> ShowData = new List<FEE_Report_Show>();
            //模板数据
            FEE_FixedData FixedData = new FEE_FixedData();

            var NoBrandData = BrandData.Where(c => string.IsNullOrEmpty(c.Brand)).FirstOrDefault();

            foreach (var item in FixedData.Brand)
            {
                var BrandSales = SalesData.Where(c => c.BRAND == item).Sum(c => c.MONEY);
                BrandSales = Math.Round((BrandSales / 10000), 2);
                foreach (var item1 in FixedData.FeeName)
                {
                    FEE_Report_Show temp = new FEE_Report_Show();
                    temp.Brand = item;
                    temp.FeeName = item1;
                    var fixData = FixedData.GetFeeItems(item1);
                    if (item1 == "合计")
                    {
                        temp.Current_Index = Math.Round(OracleData.GetBrandNorm(item) * 100, 2) + "%";
                        var brand_data = BrandData.Where(c => c.Brand == item).FirstOrDefault();
                        temp.Brand_Assess_Cost = brand_data.Items.Where(c => fixData.Contains(c.name)).Sum(c => c.money + c.taxmoney);
                        temp.Brand_Assess_Cost += tempList.Where(c => c.BRAND == item).Sum(c => c.SALARY);
                        temp.Brand_Assess_Cost = Math.Round((temp.Brand_Assess_Cost / 10000), 2);
                        temp.Assess_Achieve = BrandSales;
                        temp.Share_Cost = NoBrandData.Items.Sum(c => c.money + c.taxmoney) * (SalesData.Where(c => c.BRAND == item).Sum(c => c.MONEY) / SalesData.Sum(c => c.MONEY));
                        temp.Share_Cost = Math.Round((temp.Share_Cost / 10000), 2);
                        temp.Assess_Cost = temp.Brand_Assess_Cost + temp.Share_Cost;
                        var ratio = Math.Round(100 * temp.Assess_Cost / BrandSales, 2);
                        temp.Current_Actual = ratio + "%";
                        temp.Super_Index = (ratio - Math.Round(OracleData.GetBrandNorm(item) * 100, 2)) + "%";

                    }
                    else if (item1 == "人力成本")
                    {
                        var brand_data = BrandData.Where(c => c.Brand == item).FirstOrDefault();
                        temp.Brand_Assess_Cost = brand_data.Items.Where(c => fixData.Contains(c.name)).Sum(c => c.money + c.taxmoney);
                        temp.Brand_Assess_Cost += tempList.Where(c => c.BRAND == item).Sum(c => c.SALARY);
                        temp.Brand_Assess_Cost = Math.Round((temp.Brand_Assess_Cost / 10000), 2);
                        temp.Share_Cost = NoBrandData.Items.Where(c => fixData.Contains(c.name)).Sum(c => c.money + c.taxmoney) * (SalesData.Where(c => c.BRAND == item).Sum(c => c.MONEY) / SalesData.Sum(c => c.MONEY));
                        temp.Share_Cost = Math.Round((temp.Share_Cost / 10000), 2);
                        temp.Assess_Cost = temp.Brand_Assess_Cost + temp.Share_Cost;
                        temp.Current_Actual = Math.Round(100 * temp.Assess_Cost / BrandSales, 2) + "%";
                    }
                    else
                    {
                        var brand_data = BrandData.Where(c => c.Brand == item).FirstOrDefault();
                        temp.Brand_Assess_Cost = brand_data.Items.Where(c => fixData.Contains(c.name)).Sum(c => c.money + c.taxmoney);
                        temp.Brand_Assess_Cost = Math.Round((temp.Brand_Assess_Cost / 10000), 2);
                        temp.Share_Cost = NoBrandData.Items.Where(c => fixData.Contains(c.name)).Sum(c => c.money + c.taxmoney) * (SalesData.Where(c => c.BRAND == item).Sum(c => c.MONEY) / SalesData.Sum(c => c.MONEY));
                        temp.Share_Cost = Math.Round((temp.Share_Cost / 10000), 2);
                        temp.Assess_Cost = temp.Brand_Assess_Cost + temp.Share_Cost;
                        temp.Current_Actual = Math.Round(100 * temp.Assess_Cost / BrandSales, 2) + "%";
                    }
                    ShowData.Add(temp);
                }
            }

            return ShowData;
        }

        string DownExcel(List<FEE_Report_Show> dt, string TableName)
        {
            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格

            var Colnums = new List<string>() { "品牌", "本期指标", "本期实际", "超指标", "考核费用/万", "考核业绩/万", "品牌直接考核费用/万", "已分摊到品牌的考核费用/万" };

            int Rownum = dt.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnums.Count; i++)
            {
                cells[0, i].PutValue(Colnums[i]);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                var newvalue = dt[i].FeeName == "合计" ? (dt[i].Brand + "（" + dt[i].FeeName + "）") : dt[i].FeeName;
                cells[1 + i, 0].PutValue(newvalue);
                cells[1 + i, 1].PutValue(dt[i].Current_Index);
                cells[1 + i, 2].PutValue(dt[i].Current_Actual);
                cells[1 + i, 3].PutValue(dt[i].Super_Index);
                cells[1 + i, 4].PutValue(dt[i].Assess_Cost);
                cells[1 + i, 5].PutValue(dt[i].Assess_Achieve);
                cells[1 + i, 6].PutValue(dt[i].Brand_Assess_Cost);
                cells[1 + i, 7].PutValue(dt[i].Share_Cost);
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

        public ActionResult ExportFeeReportExcel(DateTime startTime, DateTime endTime, string city)
        {
            List<FEE_Report_Show> ShowData = GetFeeReportData(startTime, endTime, city);
            var name = DownExcel(ShowData, "品牌费用率报表");
            return Content(name);
        }

        #endregion

        /// <summary>
        /// 获取个人片区权限
        /// </summary>
        /// <param name="employeeNo"></param>
        /// <returns></returns>
        public ActionResult GetSelfArea(string employeeNo)
        {
            //确定管理员权限
            string managerSql = "select count(*) from FEE_REPORT where TYPENAME='Manager' and power like '%" + employeeNo + "%'";
            var value = DbContext.Database.SqlQuery<int>(managerSql).FirstOrDefault();
            string sql = string.Empty;
            if (value == 0)
            {
                sql = string.Format(@"
                  select city from FEE_REPORT where power like '%{0}%' and  TYPENAME='Area'
            ", employeeNo);
            }
            else
            {
                sql = "select city  from FEE_REPORT where TYPENAME!='Manager'";
            }
            var tempList = DbContext.Database.SqlQuery<string>(sql).ToList();
            return Json(tempList, JsonRequestBehavior.AllowGet);
        }

        public string ExportExcel(ReportModel dt, string TableName)
        {
            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格

            int Colnum = dt.Columns.Count;//表格列数
            int Rownum = dt.Rows.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(dt.Columns[i]);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(dt.Rows[i][k]);
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




        public string ConvertToApprovalText(int ApprovalStatus, string ApprovalPost)
        {
            if (ApprovalStatus == 2)
            {
                return "通过";
            }
            if (ApprovalStatus == 3)
            {
                return "拒绝";
            }
            if (ApprovalStatus == 5)
            {
                return "搁置中,理由为：" + ApprovalPost;
            }
            if (!string.IsNullOrEmpty(ApprovalPost))
            {
                return ApprovalPost + "审核中";
            }
            return "";
        }


        public string GetEmployees(ReportTableData OriginData)
        {
            ReportHelper helper = new ReportHelper();
            StringBuilder sb = new StringBuilder();
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            MemoryCachingClient M = new MemoryCachingClient();
            //费用大类KPI考核
            if (OriginData.ReportType == "1")
            {
                sb.Append(" select DEPARTMENT_MERGE.name ,count(no)  from department left join  DEPARTMENT_MERGE on department.MERGE_ID=DEPARTMENT_MERGE.ID");
                sb.Append(" left join employee on department.id=employee.DEPID  ");
                sb.Append(" where  department.MERGE_ID in (" + OriginData.Area + ") and employee.AVAILABLE=1 and employee.LEAVE=0 ");
                if (OriginData.CreateBeginTime != "1")
                {
                    sb.Append(OriginData.CreateBeginTime == "1" ? " and to_CHAR(MFDEV.EMPLOYEE.CREATEDATE,'MM')<7" : " and to_CHAR(MFDEV.EMPLOYEE.CREATEDATE,'MM')>6");
                }
                sb.Append(" GROUP by DEPARTMENT_MERGE.name ");
                DataTable dt = helper.GetDataTable(sb.ToString());
                return JsonSerializeHelper.SerializeToJson(dt);
            }
            else if (OriginData.ReportType == "2")
            {

            }
            //财务数据导出数据查找
            else if (OriginData.ReportType == "3")
            {
                List<string> AccountInfoList3;
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList3);
                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("单号");
                    dt.Columns.Add("部门");
                    dt.Columns.Add("店柜");
                    dt.Columns.Add("店柜编码");
                    dt.Columns.Add("品牌");
                    dt.Columns.Add("单据类型");
                    dt.Columns.Add("成本中心");
                    dt.Columns.Add("科目号");
                    dt.Columns.Add("费用小类");
                    dt.Columns.Add("金额");
                    dt.Columns.Add("办结时间");
                    dt.Columns.Add("业务人");
                    //填充内容
                    foreach (var item in RefList)
                    {
                        if (item.Items != null && item.Items.Count > 0)
                        {
                            foreach (var item1 in item.Items)
                            {
                                if (AccountInfoList3.Count > 0)
                                {
                                    if (AccountInfoList3.Contains(item1.name))
                                    {
                                        DataRow row = dt.NewRow();
                                        row["单号"] = item.BillNo;
                                        row["部门"] = item.Department;
                                        row["店柜"] = item.Shop == null ? "" : item.Shop;
                                        row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                                        string Brand = "";
                                        if (item.Brand == null || item.Brand.Count == 0)
                                        {
                                            Brand = "无记账品牌";
                                        }
                                        else
                                        {
                                            foreach (var item2 in item.Brand)
                                            {
                                                Brand += item2 + ",";
                                            }
                                            Brand = Brand.Remove(Brand.Length - 1);
                                        }
                                        row["品牌"] = Brand;
                                        row["单据类型"] = item.BillType;
                                        row["成本中心"] = item.CostCenter;
                                        row["科目号"] = item1.taxcode == null ? "" : item1.taxcode;
                                        row["费用小类"] = item1.name;
                                        row["金额"] = item1.money + item1.taxmoney;
                                        row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                                        row["业务人"] = item.Owner;
                                        dt.Rows.Add(row);
                                    }
                                }
                                else
                                {
                                    DataRow row = dt.NewRow();
                                    row["单号"] = item.BillNo;
                                    row["部门"] = item.Department;
                                    row["店柜"] = item.Shop == null ? "" : item.Shop;
                                    row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                                    string Brand = "";
                                    if (item.Brand == null || item.Brand.Count == 0)
                                    {
                                        Brand = "无记账品牌";
                                    }
                                    else
                                    {
                                        foreach (var item2 in item.Brand)
                                        {
                                            Brand += item2 + ",";
                                        }
                                        Brand = Brand.Remove(Brand.Length - 1);
                                    }
                                    row["品牌"] = Brand;
                                    row["单据类型"] = item.BillType;
                                    row["成本中心"] = item.CostCenter;
                                    row["科目号"] = item1.code == null ? "" : item1.code;
                                    row["费用小类"] = item1.name;
                                    row["金额"] = item1.money + item1.taxmoney;
                                    row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                                    row["业务人"] = item.Owner;
                                    dt.Rows.Add(row);
                                }
                            }
                        }
                        //针对ITems为空的（现金单）
                        else
                        {
                            DataRow row = dt.NewRow();
                            row["单号"] = item.BillNo;
                            row["部门"] = item.Department;
                            row["店柜"] = item.Shop == null ? "" : item.Shop;
                            row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                            string Brand = "无记账品牌";
                            row["品牌"] = Brand;
                            row["单据类型"] = item.BillType;
                            row["成本中心"] = item.CostCenter;
                            row["科目号"] = "";
                            row["费用小类"] = "";
                            row["金额"] = item.TotalMoney;
                            row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                            row["业务人"] = item.Owner;
                            dt.Rows.Add(row);
                        }
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, dt);

                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }
            //综合明细数据查找
            else if (OriginData.ReportType == "8")
            {
                List<string> AccountInfoList3;
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList3);
                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("单号");
                    dt.Columns.Add("部门");
                    dt.Columns.Add("店柜");
                    dt.Columns.Add("店柜编码");
                    dt.Columns.Add("业务人");
                    dt.Columns.Add("品牌");
                    dt.Columns.Add("单据类型");
                    dt.Columns.Add("费用小类");
                    dt.Columns.Add("费用项是否考核");
                    dt.Columns.Add("金额");
                    dt.Columns.Add("创建时间");
                    dt.Columns.Add("业务时间");
                    dt.Columns.Add("审核状态");
                    dt.Columns.Add("办结时间");
                    dt.Columns.Add("备注");

                    //RefundBill RB = new RefundBill();
                    //BorrowBill BW = new BorrowBill();
                    ////循环处理借款单
                    //foreach (var item in RefList)
                    //{
                    //    var list = RB.GetRefundRecode(item.BillNo);
                    //    if (list.Count > 0)
                    //    {
                    //        list = list.Where(c => c.Status == 0 && c.ApprovalStatus == 2).ToList();
                    //        var sur = item.TotalMoney - list.Sum(c => c.RealRefundMoney);
                    //        sur = sur < 0 ? 0 : sur;
                    //        BW.EditBorrowBill(item.BillNo, sur);
                    //    }
                    //}

                    //RefundBill RB = new RefundBill();
                    //BorrowBill BW = new BorrowBill();
                    //RefList = RefList.Where(c => c.OutDebt == 1).ToList();

                    //foreach (var item in RefList)
                    //{
                    //    var list = RB.GetRefundRecode(item.BorrowBillNo).Where(c => c.Status == 0 && c.ApprovalStatus == 2).ToList();
                    //    var BMoney = BW.GetBillModel(item.BorrowBillNo).TotalMoney;
                    //    var sur = BMoney - list.Sum(c => c.RealRefundMoney);
                    //    if (sur >= 0)
                    //    {
                    //        RB.EditRefundBill(item.BillNo);
                    //    }
                    //}


                    //填充内容
                    foreach (var item in RefList)
                    {
                        if (item.Items != null && item.Items.Count > 0)
                        {
                            foreach (var item1 in item.Items)
                            {
                                if (AccountInfoList3.Count > 0)
                                {
                                    if (AccountInfoList3.Contains(item1.name))
                                    {
                                        DataRow row = dt.NewRow();
                                        row["单号"] = item.BillNo;
                                        row["部门"] = item.Department;
                                        row["店柜"] = item.Shop == null ? "" : item.Shop;
                                        row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                                        string Brand = "";
                                        if (item.Brand == null || item.Brand.Count == 0)
                                        {
                                            Brand = "无记账品牌";
                                        }
                                        else
                                        {
                                            foreach (var item2 in item.Brand)
                                            {
                                                Brand += item2 + ",";
                                            }
                                            Brand = Brand.Remove(Brand.Length - 1);
                                        }
                                        row["品牌"] = Brand;
                                        row["单据类型"] = item.BillType;
                                        row["费用小类"] = item1.name;
                                        row["费用项是否考核"] = DbContext.FEE_ACCOUNT.Where(c => c.NAME == item1.name).Select(c => c.IS_MARKET).FirstOrDefault() == 1 ? "是" : "否";
                                        row["金额"] = item1.money + item1.taxmoney;
                                        row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                                        row["业务人"] = item.Owner;
                                        row["创建时间"] = item.CreateTime.ToString("yy-MM-dd");
                                        row["业务时间"] = item.TransactionDate.ToString("yy-MM-dd");
                                        row["审核状态"] = ConvertToApprovalText(item.ApprovalStatus, item.ApprovalPost);
                                        row["备注"] = item.Remark;
                                        dt.Rows.Add(row);
                                    }
                                }
                                else
                                {
                                    DataRow row = dt.NewRow();
                                    row["单号"] = item.BillNo;
                                    row["部门"] = item.Department;
                                    row["店柜"] = item.Shop == null ? "" : item.Shop;
                                    row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                                    string Brand = "";
                                    if (item.Brand == null || item.Brand.Count == 0)
                                    {
                                        Brand = "无记账品牌";
                                    }
                                    else
                                    {
                                        foreach (var item2 in item.Brand)
                                        {
                                            Brand += item2 + ",";
                                        }
                                        Brand = Brand.Remove(Brand.Length - 1);
                                    }
                                    row["品牌"] = Brand;
                                    row["单据类型"] = item.BillType;
                                    row["费用小类"] = item1.name;
                                    row["费用项是否考核"] = DbContext.FEE_ACCOUNT.Where(c => c.NAME == item1.name).Select(c => c.IS_MARKET).FirstOrDefault() == 1 ? "是" : "否";
                                    row["金额"] = item1.money + item1.taxmoney;
                                    row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                                    row["业务人"] = item.Owner;
                                    row["创建时间"] = item.CreateTime.ToString("yy-MM-dd");
                                    row["业务时间"] = item.TransactionDate.ToString("yy-MM-dd");
                                    row["审核状态"] = ConvertToApprovalText(item.ApprovalStatus, item.ApprovalPost);
                                    row["备注"] = item.Remark;
                                    dt.Rows.Add(row);
                                }
                            }
                        }
                        //针对ITems为空的（现金单）
                        else
                        {
                            DataRow row = dt.NewRow();
                            row["单号"] = item.BillNo;
                            row["部门"] = item.Department;
                            row["店柜"] = item.Shop == null ? "" : item.Shop;
                            row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                            string Brand = "无记账品牌";
                            row["品牌"] = Brand;
                            row["单据类型"] = item.BillType;
                            row["费用小类"] = "";
                            row["金额"] = item.TotalMoney;
                            row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                            row["业务人"] = item.Owner;
                            row["创建时间"] = item.CreateTime.ToString("yy-MM-dd");
                            row["业务时间"] = item.TransactionDate.ToString("yy-MM-dd");
                            row["审核状态"] = ConvertToApprovalText(item.ApprovalStatus, item.ApprovalPost);
                            row["备注"] = item.Remark;
                            dt.Rows.Add(row);
                        }
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, dt);
                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }
            else if (OriginData.ReportType == "4")
            {
                List<string> AccountInfoList4;
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList4);
                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    if (string.IsNullOrEmpty(OriginData.AreaCodeAndShopCode))
                    {
                        var cc = RefList.GroupBy(c => new { c.Department, c.Shop, c.ShopCode }).Select(g => new DepartmentList { Department = g.Key.Department, Shop = g.Key.Shop, ShopCode = g.Key.ShopCode, items = g.Select(e => e.Items).ToList() }).ToList();
                        dt.Columns.Add("部门");
                        dt.Columns.Add("店柜");
                        dt.Columns.Add("店柜编码");
                        if (AccountInfoList4.Count > 0)
                        {
                            foreach (var temp2 in AccountInfoList4)
                            {
                                dt.Columns.Add(temp2);
                            }
                        }
                        foreach (var item in cc)
                        {
                            DataRow row = dt.NewRow();
                            row["部门"] = item.Department;
                            row["店柜"] = item.Shop == null ? "" : item.Shop;
                            row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                            foreach (var temp in AccountInfoList4)
                            {
                                decimal sum = 0;
                                foreach (var temp1 in item.items)
                                {
                                    sum += temp1.Where(c => c.name == temp).Select(x => x.money + x.taxmoney).ToList().Sum();
                                }
                                row[temp] = sum;
                            }
                            dt.Rows.Add(row);
                        }
                    }
                    else
                    {
                        dt.Columns.Add("单号");
                        dt.Columns.Add("报销方");
                        dt.Columns.Add("店柜编码");
                        dt.Columns.Add("创建时间");
                        dt.Columns.Add("品牌");
                        dt.Columns.Add("费用小类");
                        dt.Columns.Add("金额");
                        var Datalist = OriginData.AreaCodeAndShopCode.Split(',').ToList();
                        foreach (var item in RefList)
                        {
                            string Brand = "";
                            DataRow row = dt.NewRow();
                            row["单号"] = item.BillNo;
                            row["报销方"] = item.Department + (item.Shop == null ? "" : ("-" + item.Shop));
                            row["店柜编码"] = item.ShopCode;
                            row["创建时间"] = item.CreateTime.ToString("yyyy-MM-dd");
                            if (item.Brand == null || item.Brand.Count == 0)
                            {
                                Brand = "无记账品牌";
                            }
                            else
                            {
                                foreach (var temp in item.Brand)
                                {
                                    Brand += temp + ",";
                                }
                                Brand = Brand.Remove(Brand.Length - 1);
                            }
                            row["品牌"] = Brand;

                            if (Datalist.Count > 0)
                            {
                                row["费用小类"] = Datalist[0];
                                row["金额"] = item.Items.Where(c => c.name == Datalist[0]).Select(x => x.money + x.taxmoney).FirstOrDefault();
                            }
                            dt.Rows.Add(row);
                        }
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, dt);
                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }
            //单据查询
            else if (OriginData.ReportType == "5")
            {
                if (string.IsNullOrEmpty(OriginData.BillNo))
                {
                    return "";
                }
                else
                {
                    List<FeeBillModelRef> ModelList = new List<FeeBillModelRef>();
                    List<string> str = new List<string>();
                    if (!string.IsNullOrEmpty(OriginData.AreaPermissonList))
                    {
                        List<string> temp = OriginData.AreaPermissonList.ToString().Split(',').ToList();
                        temp.Remove("");
                        str = temp;
                    }

                    OriginData.BillNo = OriginData.BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
                    var list = OriginData.BillNo.Split(',').ToList();
                    list.Remove("");
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var model = FindBillNo(item, OriginData.PermissonGrade, employee.EmployeeNo, str);
                            if (model != null)
                            {
                                ModelList.Add(model);
                            }
                        }
                    }
                    if (ModelList.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("单号");
                        dt.Columns.Add("单据类型");
                        dt.Columns.Add("品牌列表");
                        dt.Columns.Add("业务人");
                        dt.Columns.Add("总金额");
                        dt.Columns.Add("创建时间");
                        dt.Columns.Add("办结时间");
                        dt.Columns.Add("审核状态");
                        dt.Columns.Add("供应商名称");
                        dt.Columns.Add("备注");
                        dt.Columns.Add("凭证号");
                        dt.Columns.Add("操作");

                        foreach (var item in ModelList)
                        {
                            DataRow row = dt.NewRow();
                            row["单号"] = item.BillNo;
                            row["单据类型"] = GetBillValue(item.PageName);
                            string Brand = "";
                            if (item.PersonInfo.Brand == null || item.PersonInfo.Brand.Count == 0)
                            {
                                Brand = "无记账品牌";
                            }
                            else
                            {
                                foreach (var item2 in item.PersonInfo.Brand)
                                {
                                    Brand += item2 + ",";
                                }
                                Brand = Brand.Remove(Brand.Length - 1);
                            }
                            row["品牌列表"] = Brand;
                            row["业务人"] = item.Owner;
                            row["总金额"] = item.TotalMoney;
                            row["创建时间"] = item.CreateTime.ToString("yyyy-MM-dd");
                            row["办结时间"] = item.ApprovalTime;
                            row["审核状态"] = ConvertToApprovalText(item.ApprovalStatus, item.ApprovalPost);
                            row["供应商名称"] = item.ProviderName;
                            row["备注"] = item.Remark;

                            if (OriginData.PermissonGrade == "3")
                            {
                                string sql = string.Empty;
                                switch (item.PageName.ToUpper())
                                {
                                    case "FEEBILL":
                                        sql = "select SAPPROOF from FEE_FEEBILL where BILLNO='" + item.BillNo + "'";
                                        break;
                                    case "NOTICEBILL":
                                        sql = "select SAPPROOF from FEE_NOTICEBILL where BILLNO='" + item.BillNo + "'";
                                        break;
                                    case "BORROWBILL":
                                        sql = "select SAPPROOF from FEE_BORROWBILL where BILLNO='" + item.BillNo + "'";
                                        break;
                                    case "REFUNDBILL":
                                        sql = "select SAPPROOF from FEE_FEEREFUNDBILL where BILLNO='" + item.BillNo + "'";
                                        break;
                                    default:
                                        break;
                                }

                                string SAPPROOF = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                                row["凭证号"] = SAPPROOF;
                            }
                            dt.Rows.Add(row);
                        }

                        M.Remove(employee.EmployeeNo);
                        M.Add(employee.EmployeeNo, dt);

                        return Public.JsonSerializeHelper.SerializeToJson(dt);
                    }
                    else
                    {
                        return "[]";
                    }
                }
            }
            //多条件查询
            else if (OriginData.ReportType == "6")
            {
                List<string> AccountInfoList3;
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList3);
                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("单号");
                    dt.Columns.Add("部门");
                    dt.Columns.Add("店柜");
                    dt.Columns.Add("店柜编码");
                    dt.Columns.Add("品牌");
                    dt.Columns.Add("单据类型");
                    dt.Columns.Add("成本中心");
                    dt.Columns.Add("金额");
                    dt.Columns.Add("办结时间");
                    dt.Columns.Add("业务人");
                    //填充内容
                    foreach (var item in RefList)
                    {
                        DataRow row = dt.NewRow();
                        row["单号"] = item.BillNo;
                        row["部门"] = item.Department;
                        row["店柜"] = item.Shop == null ? "" : item.Shop;
                        row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;
                        string Brand = "";
                        if (item.Brand == null || item.Brand.Count == 0)
                        {
                            Brand = "无记账品牌";
                        }
                        else
                        {
                            foreach (var item2 in item.Brand)
                            {
                                Brand += item2 + ",";
                            }
                            Brand = Brand.Remove(Brand.Length - 1);
                        }
                        row["品牌"] = Brand;
                        row["单据类型"] = item.BillType;
                        row["成本中心"] = item.CostCenter;
                        row["金额"] = item.TotalMoney;
                        row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                        row["业务人"] = item.Owner;
                        dt.Rows.Add(row);
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, dt);

                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }
            //导出所有单据的审批时间
            else if (OriginData.ReportType == "9")
            {
                //不是报表管理员不给权限查询
                if (OriginData.PermissonGrade != "3")
                {
                    return "";
                }
                #region


                //补充维护数据
                var _model1 = new FeeBill().GetCompleteBill("", "", false);
                var _model2 = new NoticeBill().GetCompleteBill("", "", false);
                var _model3 = new BorrowBill().GetCompleteBill("", "", false);
                var _model4 = new RefundBill().GetCompleteBill("", "", false);


                //Dictionary<string, string> dic = new Dictionary<string, string>();
                //dic.Add("PostString", null);

                //FeeBill FB = new FeeBill();
                //NoticeBill FT = new NoticeBill();
                //BorrowBill JS = new BorrowBill();
                //RefundBill RB = new RefundBill();

                //foreach (var item in _model1)
                //{
                //    FB.PublicEditMethod(item.BillNo, dic);
                //}

                //foreach (var item in _model2)
                //{
                //    FT.PublicEditMethod(item.BillNo, dic);
                //}

                //foreach (var item in _model3)
                //{
                //    JS.PublicEditMethod(item.BillNo, dic);
                //}

                //foreach (var item in _model4)
                //{
                //    RB.PublicEditMethod(item.BillNo, dic);
                //}

                //return "";


                #region   数据处理
                //费用单
                if (_model1.Count > 0)
                {
                    string _data1 = string.Empty;
                    List<ObjectList> NoList1 = new List<ObjectList>();

                    for (int i = 0; i < _model1.Count; i++)
                    {
                        if (i < _model1.Count - 1)
                        {
                            if (i % 50 == 0 && i != 0)
                            {
                                _data1 = _data1.Remove(_data1.Length - 1);
                                //查询数据并且处理数据
                                GetWorkFlowData(NoList1, _data1);

                                NoList1 = new List<ObjectList>();
                                _data1 = string.Empty;

                            }
                            _data1 += _model1[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model1[i].BillNo, NAME = _model1[i].WorkFlowID });
                        }

                        else
                        {
                            _data1 += _model1[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model1[i].BillNo, NAME = _model1[i].WorkFlowID });

                            _data1 = _data1.Remove(_data1.Length - 1);

                            GetWorkFlowData(NoList1, _data1);
                        }
                    }
                }


                //付款通知书
                if (_model2.Count > 0)
                {
                    string _data1 = string.Empty;
                    List<ObjectList> NoList1 = new List<ObjectList>();

                    for (int i = 0; i < _model2.Count; i++)
                    {
                        if (i < _model2.Count - 1)
                        {
                            if (i % 50 == 0 && i != 0)
                            {
                                _data1 = _data1.Remove(_data1.Length - 1);
                                //查询数据并且处理数据
                                GetWorkFlowData(NoList1, _data1);

                                NoList1 = new List<ObjectList>();
                                _data1 = string.Empty;

                            }
                            _data1 += _model2[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model2[i].BillNo, NAME = _model2[i].WorkFlowID });
                        }

                        else
                        {
                            _data1 += _model2[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model2[i].BillNo, NAME = _model2[i].WorkFlowID });

                            _data1 = _data1.Remove(_data1.Length - 1);

                            GetWorkFlowData(NoList1, _data1);
                        }
                    }
                }




                //借款单
                if (_model3.Count > 0)
                {
                    string _data1 = string.Empty;
                    List<ObjectList> NoList1 = new List<ObjectList>();

                    for (int i = 0; i < _model3.Count; i++)
                    {
                        if (i < _model3.Count - 1)
                        {
                            if (i % 50 == 0 && i != 0)
                            {
                                _data1 = _data1.Remove(_data1.Length - 1);
                                //查询数据并且处理数据
                                GetWorkFlowData(NoList1, _data1);

                                NoList1 = new List<ObjectList>();
                                _data1 = string.Empty;

                            }
                            _data1 += _model3[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model3[i].BillNo, NAME = _model3[i].WorkFlowID });
                        }

                        else
                        {
                            _data1 += _model3[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model3[i].BillNo, NAME = _model3[i].WorkFlowID });

                            _data1 = _data1.Remove(_data1.Length - 1);

                            GetWorkFlowData(NoList1, _data1);
                        }
                    }
                }


                //还款单
                if (_model4.Count > 0)
                {
                    string _data1 = string.Empty;
                    List<ObjectList> NoList1 = new List<ObjectList>();

                    for (int i = 0; i < _model4.Count; i++)
                    {
                        if (i < _model4.Count - 1)
                        {
                            if (i % 50 == 0 && i != 0)
                            {
                                _data1 = _data1.Remove(_data1.Length - 1);
                                //查询数据并且处理数据
                                GetWorkFlowData(NoList1, _data1);

                                NoList1 = new List<ObjectList>();
                                _data1 = string.Empty;

                            }
                            _data1 += _model4[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model4[i].BillNo, NAME = _model4[i].WorkFlowID });
                        }

                        else
                        {
                            _data1 += _model4[i].WorkFlowID + ",";
                            NoList1.Add(new ObjectList() { CODE = _model4[i].BillNo, NAME = _model4[i].WorkFlowID });

                            _data1 = _data1.Remove(_data1.Length - 1);

                            GetWorkFlowData(NoList1, _data1);
                        }
                    }
                }

                #endregion

                List<PublicClass> ListModel = new List<PublicClass>();

                //展示数据
                var model1 = new FeeBill().GetCompleteBill(OriginData.CreateBeginTime, OriginData.CreateEndTime, true);
                var model2 = new NoticeBill().GetCompleteBill(OriginData.CreateBeginTime, OriginData.CreateEndTime, true);
                var model3 = new BorrowBill().GetCompleteBill(OriginData.CreateBeginTime, OriginData.CreateEndTime, true);
                var model4 = new RefundBill().GetCompleteBill(OriginData.CreateBeginTime, OriginData.CreateEndTime, true);

                foreach (var item in model1)
                {
                    ListModel.Add(GetPostString(item.BillNo, item.Creator, item.CreateTime, item.PostString));
                }

                foreach (var item in model2)
                {
                    ListModel.Add(GetPostString(item.BillNo, item.Creator, item.CreateTime, item.PostString));
                }

                foreach (var item in model3)
                {
                    ListModel.Add(GetPostString(item.BillNo, item.Creator, item.CreateTime, item.PostString));
                }

                foreach (var item in model4)
                {
                    ListModel.Add(GetPostString(item.BillNo, item.Creator, item.CreateTime, item.PostString));
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("单据号");
                dt.Columns.Add("创建人");
                dt.Columns.Add("负责人");
                dt.Columns.Add("品牌审批岗");
                dt.Columns.Add("XX审批岗");
                dt.Columns.Add("XX审批岗1");
                dt.Columns.Add("XX审批岗2");
                dt.Columns.Add("财务会计");
                dt.Columns.Add("总经办");
                dt.Columns.Add("出纳");
                dt.Columns.Add("创建时间");
                dt.Columns.Add("流程走向");
                dt.Columns.Add("总共耗时");


                foreach (var item in ListModel)
                {
                    DataRow row = dt.NewRow();
                    row["单据号"] = item.c1;
                    row["创建人"] = item.c2;
                    row["负责人"] = item.c3;
                    row["品牌审批岗"] = item.c4;
                    row["XX审批岗"] = item.c5;
                    row["XX审批岗1"] = item.c6;
                    row["XX审批岗2"] = item.c7;
                    row["财务会计"] = item.c8;
                    row["总经办"] = item.c9;
                    row["出纳"] = item.c10;
                    row["创建时间"] = item.c11;
                    row["流程走向"] = item.c12;
                    row["总共耗时"] = item.c13;
                    dt.Rows.Add(row);
                }

                M.Remove(employee.EmployeeNo);
                M.Add(employee.EmployeeNo, dt);

                return Public.JsonSerializeHelper.SerializeToJson(dt);

                #endregion
            }
            //借款单查询
            else if (OriginData.ReportType == "10")
            {
                //不是报表管理员不给权限查询
                if (OriginData.PermissonGrade != "3")
                {
                    return "";
                }

                List<string> AccountInfoList3;
                OriginData.BillStatus = "1";
                OriginData.BillType = "3";
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList3);

                //删选出正确的数据
                RefList = RefList.Where(c => c.SurplusMoney > 0).ToList();

                RefundBill RB = new RefundBill();

                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("单号");
                    dt.Columns.Add("部门");
                    dt.Columns.Add("店柜");
                    dt.Columns.Add("品牌");
                    dt.Columns.Add("成本中心");
                    dt.Columns.Add("借款金额");
                    dt.Columns.Add("待还金额");
                    dt.Columns.Add("还款详细单号");
                    //填充内容
                    foreach (var item in RefList)
                    {
                        DataRow row = dt.NewRow();
                        row["单号"] = item.BillNo;
                        row["部门"] = item.Department;
                        row["店柜"] = item.Shop == null ? "" : item.Shop;
                        string Brand = "";
                        if (item.Brand == null || item.Brand.Count == 0)
                        {
                            Brand = "无记账品牌";
                        }
                        else
                        {
                            foreach (var item2 in item.Brand)
                            {
                                Brand += item2 + ",";
                            }
                            Brand = Brand.Remove(Brand.Length - 1);
                        }
                        row["品牌"] = Brand;
                        row["成本中心"] = item.CostCenter;
                        row["借款金额"] = item.TotalMoney;
                        row["待还金额"] = item.SurplusMoney;

                        var RBModel = RB.GetRefundRecode(item.BillNo);
                        if (RBModel != null && RBModel.Count > 0)
                        {
                            var strModel = RBModel.Where(c => c.ApprovalStatus != 3).Select(c => c.BillNo).ToList();
                            string str = string.Empty;
                            foreach (var item10 in strModel)
                            {
                                str += item10 + ",";
                            }
                            row["还款详细单号"] = str;
                        }
                        else
                        {
                            row["还款详细单号"] = "无还款记录";
                        }

                        dt.Rows.Add(row);
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, dt);

                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }
            //汇款明细查询
            else if (OriginData.ReportType == "7")
            {
                List<string> AccountInfoList3;
                OriginData.BillStatus = "1";
                OriginData.BillType = "1,3,4";
                OriginData.SpecialProperty = "sc0,yh0,yj0,";
                List<ReportCollect> RefList = ReportDataTestV1(OriginData, out AccountInfoList3);
                if (RefList != null && RefList.Count > 0)
                {
                    DataTable dt = new DataTable();
                    List<RemittanceDetails> Details = new List<RemittanceDetails>();

                    dt.Columns.Add("单号");
                    dt.Columns.Add("部门");
                    dt.Columns.Add("店柜");
                    dt.Columns.Add("店柜编码");
                    dt.Columns.Add("所处城市");
                    dt.Columns.Add("付款公司代码");
                    dt.Columns.Add("成本中心对应品牌");
                    dt.Columns.Add("单据类型");
                    dt.Columns.Add("成本中心");
                    dt.Columns.Add("单据金额");
                    dt.Columns.Add("冲借款金额");
                    dt.Columns.Add("财务汇款金额");
                    dt.Columns.Add("办结时间");
                    dt.Columns.Add("业务人");
                    //填充内容
                    foreach (var item in RefList)
                    {
                        RemittanceDetails det = new RemittanceDetails();
                        DataRow row = dt.NewRow();
                        row["单号"] = item.BillNo;
                        row["部门"] = item.Department;
                        row["店柜"] = item.Shop == null ? "" : item.Shop;
                        row["店柜编码"] = item.ShopCode == null ? "" : item.ShopCode;

                        var obj = PublicGetCosterCenter(item.IsHeadOffice, item.CostCenter);

                        row["成本中心对应品牌"] = obj.NAME;
                        row["单据类型"] = item.BillType;
                        row["成本中心"] = item.CostCenter;
                        row["单据金额"] = item.TotalMoney;
                        row["冲借款金额"] = item.EntryMoney;
                        row["财务汇款金额"] = item.TotalMoney - item.EntryMoney;
                        row["办结时间"] = item.ApprovalTime == new DateTime() ? "无" : item.ApprovalTime.ToString("yy-MM-dd");
                        row["业务人"] = item.Owner;
                        if (item.IsHeadOffice == 1)
                        {
                            row["所处城市"] = "总部";
                        }
                        else
                        {
                            if (item.Department.Contains("会所"))
                            {
                                row["所处城市"] = item.Department;
                            }
                            else
                            {
                                string sql = string.Empty;
                                if (string.IsNullOrEmpty(item.ShopCode))
                                {
                                    sql = "select b.shortname from department a left join shop b on a.code=b.code where a.id='" + item.DepartmentCode + "'";
                                }
                                else
                                {
                                    sql = "select  shortname from shop where code='" + item.ShopCode + "'";
                                }
                                row["所处城市"] = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                            }
                        }
                        if (string.IsNullOrEmpty(item.PaymentCompanyCode))
                        {
                            string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + item.CostCenter + "%'";
                            var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                            if (string.IsNullOrEmpty(pay))
                            {
                                row["付款公司代码"] = GetCompanyCode(obj.NAME);
                            }
                            else
                            {
                                row["付款公司代码"] = pay;
                            }
                        }
                        else
                        {
                            row["付款公司代码"] = item.PaymentCompanyCode;
                        }

                        //出纳付款清单表
                        det.BillNo = item.BillNo;
                        det.Department = item.Department;
                        det.Shop = item.Shop;
                        det.PayCode = row["付款公司代码"].ToString();
                        det.BillType = item.BillType;
                        det.City = row["所处城市"].ToString();
                        det.BillMoney = item.TotalMoney;
                        det.RepaymentMoney = item.EntryMoney;
                        det.PaymentMoney = item.TotalMoney - item.EntryMoney;
                        det.Gather = item.gather;
                        dt.Rows.Add(row);
                        Details.Add(det);
                    }

                    M.Remove(employee.EmployeeNo);
                    M.Add(employee.EmployeeNo, Details);

                    return Public.JsonSerializeHelper.SerializeToJson(dt);
                }
                return "";
            }

            DataTable dataList = helper.GetSQLDataTable(sb.ToString());
            return JsonSerializeHelper.SerializeToJson(dataList);
        }


        PublicClass GetPostString(string BillNo, string Creator, DateTime CteateTime, List<PostDescription> Description)
        {
            PublicClass ReturnModel = new PublicClass();
            ReturnModel.c1 = BillNo;
            ReturnModel.c2 = Creator;
            string AllText = string.Empty;

            foreach (var item in Description)
            {
                string text = item.Post;

                string inText = item.Post + "（" + item.JobNumber + "（" + DbContext.EMPLOYEE.Where(c => c.NO == item.JobNumber).Select(x => x.NAME).FirstOrDefault() + "），" + item.Time.ToString("yyyy-MM-dd HH:mm") + "）";

                AllText += inText + "->";

                if (text.Contains("负责人"))
                {
                    ReturnModel.c3 = inText;
                }
                else if (text.Contains("品牌"))
                {
                    ReturnModel.c4 = inText;
                }
                else if (text.Contains("岗1"))
                {
                    ReturnModel.c5 = inText;
                }
                else if (text.Contains("岗2"))
                {
                    ReturnModel.c6 = inText;
                }
                else if (text.Contains("岗"))
                {
                    ReturnModel.c7 = inText;
                }
                else if (text == "财务会计")
                {
                    ReturnModel.c8 = inText;
                }
                else if (text == "总经办")
                {
                    ReturnModel.c9 = inText;
                }
                else if (text == "出纳")
                {
                    ReturnModel.c10 = inText;
                }
            }

            ReturnModel.c11 = CteateTime.ToString("yyyy-MM-dd HH:mm");

            AllText = AllText.Remove(AllText.Length - 2);

            ReturnModel.c12 = AllText;

            ReturnModel.c13 = Math.Round((Description.LastOrDefault().Time - CteateTime).TotalDays, 2).ToString();

            return ReturnModel;
        }


        void GetWorkFlowData(List<ObjectList> ListStr, string Str)
        {
            //得到数据
            FeeBill FB = new FeeBill();
            NoticeBill FT = new NoticeBill();
            BorrowBill JS = new BorrowBill();
            RefundBill RB = new RefundBill();

            List<List<PostDescription>> ListModel = new List<List<PostDescription>>();

            string url = "http://192.168.2.14//WorkFlowServer/WorkFlowServer" + "/GetWorkFlowListByIDs";
            string postDataStr = String.Format("ids={0}", Str);

            string Character = HttpGet(url, postDataStr);
            var ObjModel = JsonConvert.DeserializeObject<List<WorkFlowInstance>>(Character);



            foreach (var item in ListStr)
            {
                var id = MongoDB.Bson.ObjectId.Parse(item.NAME);

                var obj = ObjModel.Where(c => c._id == id).FirstOrDefault();

                if (obj == null)
                {
                    continue;
                }

                var obj1 = FB.Getxuqiu(obj);

                Dictionary<string, List<PostDescription>> dic = new Dictionary<string, List<PostDescription>>();
                dic.Add("PostString", obj1);

                if (item.CODE.Contains("FB"))
                {
                    var IsFee = FB.GetBillModel(item.CODE) != null;
                    if (IsFee)
                    {
                        FB.PublicEditMethod(item.CODE, dic);
                    }
                    else
                    {
                        RB.PublicEditMethod(item.CODE, dic);
                    }
                }
                else if (item.CODE.Contains("FT"))
                {
                    FT.PublicEditMethod(item.CODE, dic);
                }
                else if (item.CODE.Contains("JS"))
                {
                    JS.PublicEditMethod(item.CODE, dic);
                }
                else if (item.CODE.Contains("HK"))
                {
                    RB.PublicEditMethod(item.CODE, dic);
                }
            }
        }


        private string GetPaymentBank(string PayCode)
        {
            string bankName = string.Empty;
            switch (PayCode)
            {
                case "1000":
                    bankName = "招行";
                    break;
                case "1300":
                    bankName = "招行";
                    break;
                case "4000":
                    bankName = "招行";
                    break;
                case "2000":
                    bankName = "建行";
                    break;
                //研发设计中心
                case "2300":
                    bankName = "建行";
                    break;
                case "2100":
                    bankName = "中行";
                    break;
                default:
                    break;
            }
            return bankName;
        }


        public static void CreateZip(string zipFileName, string sourceDirectory, bool recurse = true, string fileFilter = "")
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentNullException("SourceZipDirectory");
            }
            if (string.IsNullOrEmpty(zipFileName))
            {
                throw new ArgumentNullException("TargetZipName");
            }
            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException("SourceDirecotry");
            }
            if (Path.GetExtension(zipFileName).ToUpper() != ".ZIP")
                throw new ArgumentException("TargetZipName  is not zip");
            FastZip fastZip = new FastZip();
            fastZip.CreateZip(zipFileName, sourceDirectory, recurse, fileFilter);
        }


        private FinanceExcle ConvertToModel(string TableName, string PayCode, List<RemittanceDetails> Model, string BillType)
        {
            FinanceExcle t1 = new FinanceExcle();
            t1.TableName = TableName;
            //综合表
            DataTable sheet1 = new DataTable();
            var bankName = GetPaymentBank(PayCode);
            if (bankName == "招行")
            {
                sheet1.Columns.Add("帐号");
                sheet1.Columns.Add("户名");
                sheet1.Columns.Add("金额");
                sheet1.Columns.Add("开户行");
                sheet1.Columns.Add("开户地");
                sheet1.Columns.Add("注释");

                var c2 = Model.Where(x => x.PaymentMoney > 0).GroupBy(c => new { c.Gather.Name, c.City }).ToList();
                foreach (var item1 in c2)
                {
                    DataRow row = sheet1.NewRow();
                    var first = item1.FirstOrDefault();
                    row["帐号"] = first.Gather.CardCode;
                    row["户名"] = first.Gather.Name;
                    row["开户行"] = first.Gather.Bank;
                    row["开户地"] = first.Gather.City;
                    row["金额"] = item1.Sum(c => c.PaymentMoney);
                    row["注释"] = BillType;
                    sheet1.Rows.Add(row);
                }
            }
            else if (bankName == "建行")
            {
                sheet1.Columns.Add("序号");
                sheet1.Columns.Add("付款方客户账号");
                sheet1.Columns.Add("付款方账户名称");
                sheet1.Columns.Add("收款方行别代码（01-本行 02-国内他行）");

                sheet1.Columns.Add("收款方客户账号");
                sheet1.Columns.Add("收款方账户名称");
                sheet1.Columns.Add("收款方开户行名称");
                sheet1.Columns.Add("收款方联行号");
                sheet1.Columns.Add("客户方流水号");
                sheet1.Columns.Add("金额");
                sheet1.Columns.Add("用途");
                sheet1.Columns.Add("备注");

                var c2 = Model.Where(x => x.PaymentMoney > 0).GroupBy(c => new { c.Gather.Name, c.City }).ToList();

                int count = 1;

                foreach (var item1 in c2)
                {
                    DataRow row = sheet1.NewRow();
                    var first = item1.FirstOrDefault();
                    row["序号"] = count;
                    row["付款方客户账号"] = PayCode == "2000" ? "44250100000700000385" : "44250100000700000386";
                    row["付款方账户名称"] = PayCode == "2000" ? "深圳玛丝菲尔素时装有限公司" : "深圳玛丝菲尔设计研发中心有限公司";
                    row["收款方行别代码（01-本行 02-国内他行）"] = "02";
                    row["收款方客户账号"] = first.Gather.CardCode;
                    row["收款方账户名称"] = first.Gather.Name;
                    row["收款方开户行名称"] = first.Gather.Bank;
                    row["金额"] = item1.Sum(c => c.PaymentMoney);
                    row["备注"] = BillType;
                    count++;
                    sheet1.Rows.Add(row);
                }
            }
            else
            {
                sheet1.Columns.Add("付款金额");
                sheet1.Columns.Add("收款人名称");
                sheet1.Columns.Add("收款人账号");
                sheet1.Columns.Add("开户行名称");
                sheet1.Columns.Add("CNAPS行号");
                sheet1.Columns.Add("付款人账号");
                sheet1.Columns.Add("付款人名称");
                sheet1.Columns.Add("付费账号");
                sheet1.Columns.Add("指定付款日期");
                sheet1.Columns.Add("用途");
                sheet1.Columns.Add("优先级");
                sheet1.Columns.Add("收款人EMAIL");
                sheet1.Columns.Add("收款人开户行");


                var c2 = Model.Where(x => x.PaymentMoney > 0).GroupBy(c => new { c.Gather.Name, c.City }).ToList();
                foreach (var item1 in c2)
                {
                    DataRow row = sheet1.NewRow();
                    var first = item1.FirstOrDefault();
                    row["付款人账号"] = "770567791300";
                    row["付款人名称"] = "克芮绮亚时装(中国)有限公司";
                    row["付费账号"] = "770567791300";
                    row["优先级"] = "普通";
                    row["收款人账号"] = first.Gather.CardCode;
                    row["收款人名称"] = first.Gather.Name;
                    row["付款金额"] = item1.Sum(c => c.PaymentMoney);
                    row["开户行名称"] = first.Gather.Bank;
                    row["用途"] = BillType;
                    sheet1.Rows.Add(row);
                }
            }
            //明细表
            DataTable sheet2 = new DataTable();

            sheet2.Columns.Add("单号");
            sheet2.Columns.Add("部门");
            sheet2.Columns.Add("店柜");
            sheet2.Columns.Add("所在城市");
            sheet2.Columns.Add("付款公司代码");
            sheet2.Columns.Add("单据类型");
            sheet2.Columns.Add("单据金额");
            sheet2.Columns.Add("冲借款金额");
            sheet2.Columns.Add("财务汇款金额");
            sheet2.Columns.Add("收款人");

            foreach (var item2 in Model)
            {
                DataRow row = sheet2.NewRow();
                row["单号"] = item2.BillNo;
                row["部门"] = item2.Department;
                row["店柜"] = item2.Shop;
                row["所在城市"] = item2.City;
                row["付款公司代码"] = item2.PayCode;
                row["单据类型"] = item2.BillType;
                row["单据金额"] = item2.BillMoney;
                row["冲借款金额"] = item2.RepaymentMoney;
                row["财务汇款金额"] = item2.PaymentMoney;
                row["收款人"] = item2.Gather.Name;
                sheet2.Rows.Add(row);
            }

            t1.sheet1 = sheet1;
            t1.sheet2 = sheet2;
            return t1;
        }


        public string DownloadFile(string flag, string parameterList = "")
        {
            string error = string.Empty;
            string msg = string.Empty;
            string data = string.Empty;

            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            MemoryCachingClient M = new MemoryCachingClient();
            try
            {
                if (flag == "审批历史")
                {
                    ReportModel dt = M.GetData(employee.EmployeeNo) as ReportModel;
                    data = ExportExcel(dt, "审批历史");
                    error = "0";
                    msg = "导出成功";
                }
                else if (flag != "汇款明细查询")
                {
                    DataTable dt = M.GetData(employee.EmployeeNo) as DataTable;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        data = ExportExcel(dt, flag);
                        error = "0";
                        msg = "导出成功";
                    }
                    else
                    {
                        error = "1";
                        msg = "数据异常";
                    }
                }
                else
                {
                    List<RemittanceDetails> Details = M.GetData(employee.EmployeeNo) as List<RemittanceDetails>;
                    List<FinanceExcle> Model = new List<FinanceExcle>();

                    //得到数据
                    if (parameterList == "B")
                    {
                        var c1 = Details.Where(c => c.Gather != null).GroupBy(c => new { c.PayCode }).ToList();
                        //添加文件
                        foreach (var item in c1)
                        {
                            string TableName = item.Key.PayCode + "-汇款合计金额" + item.Sum(c => c.PaymentMoney);
                            var temp = item.ToList();
                            var t = ConvertToModel(TableName, item.Key.PayCode, temp, "费用");
                            Model.Add(t);
                        }
                    }
                    else if (parameterList == "B,C")
                    {
                        var c1 = Details.Where(c => c.Gather != null).GroupBy(c => new { c.PayCode, c.City }).ToList();
                        //添加文件
                        foreach (var item in c1)
                        {
                            string TableName = item.Key.PayCode + "-" + item.Key.City + "-汇款合计金额" + item.Sum(c => c.PaymentMoney);
                            var temp = item.ToList();
                            var t = ConvertToModel(TableName, item.Key.PayCode, temp, "费用");
                            Model.Add(t);
                        }
                    }
                    else if (parameterList == "B,T")
                    {
                        var c1 = Details.Where(c => c.Gather != null).GroupBy(c => new { c.PayCode, c.BillType }).ToList();
                        //添加文件
                        foreach (var item in c1)
                        {
                            string TableName = item.Key.PayCode + "-" + item.Key.BillType + "-汇款合计金额" + item.Sum(c => c.PaymentMoney);
                            var temp = item.ToList();
                            var t = ConvertToModel(TableName, item.Key.PayCode, temp, item.Key.BillType);
                            Model.Add(t);
                        }
                    }
                    else
                    {
                        var c1 = Details.Where(c => c.Gather != null).GroupBy(c => new { c.PayCode, c.City, c.BillType }).ToList();
                        //添加文件
                        foreach (var item in c1)
                        {
                            string TableName = item.Key.PayCode + "-" + item.Key.City + "-" + item.Key.BillType + "-汇款合计金额" + item.Sum(c => c.PaymentMoney);
                            var temp = item.ToList();
                            var t = ConvertToModel(TableName, item.Key.PayCode, temp, item.Key.BillType);
                            Model.Add(t);
                        }
                    }

                    //得到excle
                    if (Model.Count > 0)
                    {
                        string lo = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "Details");
                        if (!System.IO.Directory.Exists(lo))
                        {
                            System.IO.Directory.CreateDirectory(lo);
                        }
                        //生成文件夹
                        string fileName = "汇款明细查询" + '-' + DateTime.Now.ToString("yyMMddhhmmss");
                        string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "Details", fileName);
                        if (!System.IO.Directory.Exists(localPath))
                        {
                            System.IO.Directory.CreateDirectory(localPath);
                        }

                        //往指定文件夹里面添加excle
                        foreach (var item in Model)
                        {
                            Workbook workbook = new Workbook(); //工作簿                   
                            workbook.Worksheets.Add("明细表");
                            //保存sheel1
                            Worksheet sheet = workbook.Worksheets[0]; //工作表1
                            Cells cells = sheet.Cells;//单元格
                            workbook.Worksheets.Add();
                            int Colnum = item.sheet1.Columns.Count;//表格列数
                            int Rownum = item.sheet1.Rows.Count;//表格行数

                            //生成行2 列名行
                            for (int i = 0; i < Colnum; i++)
                            {
                                cells[0, i].PutValue(item.sheet1.Columns[i].ColumnName);
                                cells.SetRowHeight(1, 25);
                            }

                            //生成数据行
                            for (int i = 0; i < Rownum; i++)
                            {
                                for (int k = 0; k < Colnum; k++)
                                {
                                    cells[1 + i, k].PutValue(item.sheet1.Rows[i][k].ToString());
                                }
                                cells.SetRowHeight(1 + i, 24);
                            }

                            //保存sheel2
                            Worksheet sheet1 = workbook.Worksheets["明细表"]; //工作表1

                            Cells cells1 = sheet1.Cells;//单元格

                            int Colnum1 = item.sheet2.Columns.Count;//表格列数
                            int Rownum1 = item.sheet2.Rows.Count;//表格行数

                            //生成行2 列名行
                            for (int i = 0; i < Colnum1; i++)
                            {
                                cells1[0, i].PutValue(item.sheet2.Columns[i].ColumnName);
                                cells1.SetRowHeight(1, 25);
                            }

                            //生成数据行
                            for (int i = 0; i < Rownum1; i++)
                            {
                                for (int k = 0; k < Colnum1; k++)
                                {
                                    cells1[1 + i, k].PutValue(item.sheet2.Rows[i][k].ToString());
                                }
                                cells1.SetRowHeight(1 + i, 24);
                            }

                            string filePathName = item.TableName + ".xls";
                            workbook.Save(Path.Combine(localPath, filePathName));
                        }
                        //压缩文件夹，生成压缩包
                        string zipName = fileName + ".zip";
                        string PathName = localPath + ".zip";
                        CreateZip(PathName, localPath);

                        data = zipName;
                        error = "0";
                        msg = "导出成功";
                    }
                }
            }
            catch (Exception)
            {
                error = "1";
                msg = "数据异常";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg, data = data });
        }


        public string GetBillValue(string Value)
        {
            string str = String.Empty;
            if (string.IsNullOrEmpty(Value))
            {
                return str;
            }
            switch (Value.ToUpper())
            {
                case "FEEBILL":
                    str = "费用报销单";
                    break;
                case "NOTICEBILL":
                    str = "付款通知书";
                    break;
                case "BORROWBILL":
                    str = "借款单";
                    break;
                case "REFUNDBILL":
                    str = "还款单";
                    break;
                default:
                    break;
            }
            return str;
        }


        /// <summary>
        /// 数据验证V1
        /// </summary>
        /// <param name="OriginData">数据源</param>
        /// <param name="AccountInfoList3">费用小项</param>
        /// <returns></returns>
        private List<ReportCollect> ReportDataTestV1(ReportTableData OriginData, out List<string> AccountInfoList3)
        {
            int Department3 = 3;  //默认值
            List<string> AreaList3 = new List<string>();
            List<string> AccountTypeList3 = new List<string>();
            AccountInfoList3 = new List<string>();
            List<int> BillTypesList3 = new List<int>();
            List<string> MyCompanycode3 = new List<string>();
            List<ReportCollect> RefList = new List<ReportCollect>();
            if (OriginData.Area.Contains("XXX1"))
            {
                Department3 = 2;  //所有总部和片区
            }
            else if (OriginData.Area.Contains("XXX2"))
            {
                Department3 = 1;  //所有总部
            }
            else if (OriginData.Area.Contains("XXX3"))
            {
                Department3 = 0;  //所有片区
            }
            else
            {
                AreaList3 = OriginData.Area.Split(',').ToList();
                AreaList3.Remove("");
            }
            if (!string.IsNullOrEmpty(OriginData.AccountType))
            {
                AccountTypeList3 = OriginData.AccountType.Split(',').ToList();
                AccountTypeList3.Remove("");
            }
            if (!string.IsNullOrEmpty(OriginData.AccountInfo))
            {
                AccountInfoList3 = OriginData.AccountInfo.Split(',').ToList();
                AccountInfoList3.Remove("");

                List<decimal> InfoList = new List<decimal>();
                foreach (var item in AccountInfoList3)
                {
                    InfoList.Add(Convert.ToDecimal(item));
                }
                AccountInfoList3 = DbContext.FEE_ACCOUNT.Where(c => InfoList.Contains(c.ID)).Select(x => x.NAME).ToList();
                AccountInfoList3.AddRange(DbContext.FEE_ACCOUNT.Where(c => InfoList.Contains(c.ID)).Select(x => x.OLDNAME).ToList());
                AccountInfoList3 = AccountInfoList3.Distinct().ToList();
                AccountInfoList3.RemoveAll(c => string.IsNullOrEmpty(c));
            }
            if (!string.IsNullOrEmpty(OriginData.BillStatus))
            {
                //审核中
                if (OriginData.BillStatus.Contains("0"))
                {
                    BillTypesList3.Add(0);
                    BillTypesList3.Add(1);
                    BillTypesList3.Add(4);
                }
                //通过
                if (OriginData.BillStatus.Contains("1"))
                {
                    BillTypesList3.Add(2);
                }
                //拒绝
                if (OriginData.BillStatus.Contains("2"))
                {
                    BillTypesList3.Add(3);
                }
            }
            //创建日期
            DateTime StartTime3 = new DateTime(1999, 1, 1);
            if (!string.IsNullOrEmpty(OriginData.CreateBeginTime))
            {
                StartTime3 = Convert.ToDateTime(OriginData.CreateBeginTime);
            }
            DateTime OverTime3 = new DateTime(2999, 1, 1);
            if (!string.IsNullOrEmpty(OriginData.CreateEndTime))
            {
                OverTime3 = Convert.ToDateTime(OriginData.CreateEndTime);
                OverTime3 = OverTime3.AddDays(1).AddSeconds(-1);
            }


            if (!string.IsNullOrEmpty(OriginData.CompanyCode))
            {
                MyCompanycode3 = OriginData.CompanyCode.Split(',').ToList();
                MyCompanycode3.Remove("");
            }


            RefList = new List<ReportCollect>();
            if (OriginData.BillType.Contains("1"))
            {
                var FeeModel = new FeeBill().CountSmallSortNum(Department3, AreaList3, AccountInfoList3, BillTypesList3, StartTime3, OverTime3, "", OriginData.AreaCodeAndShopCode, MyCompanycode3);
                if (FeeModel.Count > 0)
                {
                    foreach (var item in FeeModel)
                    {
                        ReportCollect Ref = new ReportCollect() { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, DepartmentCode = item.PersonInfo.DepartmentCode, Shop = item.PersonInfo.Shop, CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Items = item.Items, ShopCode = item.PersonInfo.ShopCode, BillNo = item.BillNo, ApprovalTime = Convert.ToDateTime(item.ApprovalTime), BillType = "费用报销单", CostCenter = item.COST_ACCOUNT, Owner = item.Owner, Remark = item.Remark, TotalMoney = item.TotalMoney, SpecialProperty = item.SpecialAttribute, IsHeadOffice = item.PersonInfo.IsHeadOffice, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, gather = item.CollectionInfo, PaymentCompanyCode = item.PaymentCompanyCode, Currency = item.Currency.Code };
                        Ref.ProviderName = item.CollectionInfo == null ? "" : item.CollectionInfo.Name;//开卡人
                        RefList.Add(Ref);
                    }
                }
            }

            if (OriginData.BillType.Contains("2"))
            {
                var NoticeModel = new NoticeBill().CountSmallSortNum(Department3, AreaList3, AccountInfoList3, BillTypesList3, StartTime3, OverTime3, "", OriginData.AreaCodeAndShopCode, MyCompanycode3);
                if (NoticeModel.Count > 0)
                {
                    foreach (var item in NoticeModel)
                    {

                        ReportCollect Ref = new ReportCollect() { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, DepartmentCode = item.PersonInfo.DepartmentCode, Shop = item.PersonInfo.Shop, CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Items = item.Items, ShopCode = item.PersonInfo.ShopCode, BillNo = item.BillNo, ApprovalTime = Convert.ToDateTime(item.ApprovalTime), BillType = "付款通知书", CostCenter = item.COST_ACCOUNT, Owner = item.Owner, Remark = item.Remark, TotalMoney = item.TotalMoney, SpecialProperty = new SpecialAttribute() { Funds = item.SpecialAttribute.Funds, Agent = item.SpecialAttribute.Agent, Check = item.SpecialAttribute.Check }, ProviderName = item.ProviderInfo.ProviderName, MissBill = item.MissBill, IsHeadOffice = item.PersonInfo.IsHeadOffice, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, gather = new CollectionInfo() { Name = item.ProviderInfo.ProviderName, CardCode = item.ProviderInfo.BankNo, SubbranchBank = item.ProviderInfo.BankName }, Currency = item.Currency.Code };
                        RefList.Add(Ref);
                    }
                }
            }
            if (OriginData.BillType.Contains("3"))
            {
                var NoticeModel = new BorrowBill().CountSmallSortNum(Department3, AreaList3, AccountInfoList3, BillTypesList3, StartTime3, OverTime3, "", OriginData.AreaCodeAndShopCode, MyCompanycode3);
                if (NoticeModel.Count > 0)
                {
                    foreach (var item in NoticeModel)
                    {

                        ReportCollect Ref = new ReportCollect() { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, DepartmentCode = item.PersonInfo.DepartmentCode, Shop = item.PersonInfo.Shop, CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Items = item.Items, ShopCode = item.PersonInfo.ShopCode, BillNo = item.BillNo, ApprovalTime = Convert.ToDateTime(item.ApprovalTime), BillType = "借款单", CostCenter = item.COST_ACCOUNT, Owner = item.Owner, Remark = item.Remark, TotalMoney = item.TotalMoney, SpecialProperty = item.SpecialAttribute, IsHeadOffice = item.PersonInfo.IsHeadOffice, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, gather = item.CollectionInfo, PaymentCompanyCode = item.PaymentCompanyCode, Currency = item.Currency.Code, SurplusMoney = item.SurplusMoney };
                        Ref.ProviderName = item.CollectionInfo == null ? "" : item.CollectionInfo.Name;//开卡人
                        RefList.Add(Ref);
                    }
                }
            }
            if (OriginData.BillType.Contains("4"))
            {
                var RefundModel = new RefundBill().CountSmallSortNum(Department3, AreaList3, AccountInfoList3, BillTypesList3, StartTime3, OverTime3, "", OriginData.AreaCodeAndShopCode, MyCompanycode3, OriginData.ReportType);
                if (RefundModel.Count > 0)
                {
                    foreach (var item in RefundModel)
                    {

                        ReportCollect Ref = new ReportCollect() { Brand = item.PersonInfo.Brand, Department = item.PersonInfo.Department, DepartmentCode = item.PersonInfo.DepartmentCode, Shop = item.PersonInfo.Shop, CreateTime = item.CreateTime, TransactionDate = item.TransactionDate, Items = item.Items, ShopCode = item.PersonInfo.ShopCode, BillNo = item.BillNo, ApprovalTime = Convert.ToDateTime(item.ApprovalTime), BillType = "还款单", CostCenter = item.COST_ACCOUNT, Owner = item.Owner, Remark = item.Remark, TotalMoney = item.RealRefundMoney, SpecialProperty = item.SpecialAttribute, IsHeadOffice = item.PersonInfo.IsHeadOffice, ApprovalPost = item.ApprovalPost, ApprovalStatus = item.ApprovalStatus, gather = item.CollectionInfo, PaymentCompanyCode = item.PaymentCompanyCode, Currency = item.Currency.Code, OutDebt = item.OutDebt, BorrowBillNo = item.BorrowBillNo };
                        Ref.ProviderName = item.CollectionInfo == null ? "" : item.CollectionInfo.Name;//开卡人
                        if (item.DebtMoney == 0)
                        {
                            Ref.EntryMoney = Ref.TotalMoney;
                        }
                        else
                        {
                            Ref.EntryMoney = item.DebtMoney;
                        }
                        RefList.Add(Ref);
                    }
                }
            }
            RefList = RefList.OrderByDescending(c => c.CreateTime).ToList();
            //办结日期的筛选
            if (!string.IsNullOrEmpty(OriginData.OverBeginTime))
            {
                DateTime Temptime = Convert.ToDateTime(OriginData.OverBeginTime);
                RefList = RefList.Where(c => c.ApprovalTime > Temptime).ToList();
            }
            if (!string.IsNullOrEmpty(OriginData.OverEndTime))
            {
                DateTime Temptime = Convert.ToDateTime(OriginData.OverEndTime);
                Temptime = Temptime.AddDays(1).AddSeconds(-1);
                RefList = RefList.Where(c => c.ApprovalTime < Temptime).ToList();
            }
            //备注的筛选
            if (!string.IsNullOrEmpty(OriginData.Remark))
            {
                RefList = RefList.Where(c => c.Remark != null).ToList();
                RefList = RefList.Where(c => c.Remark.Contains(OriginData.Remark)).ToList();
            }
            if (!string.IsNullOrEmpty(OriginData.ProviderName))
            {
                RefList = RefList.Where(c => c.ProviderName != null).ToList();
                RefList = RefList.Where(c => c.ProviderName.Contains(OriginData.ProviderName)).ToList();
            }
            if (!string.IsNullOrEmpty(OriginData.SpecialProperty))
            {
                List<string> list = OriginData.SpecialProperty.Split(',').ToList();
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
                RefList = RefList.Where(c => c.SpecialProperty != null).ToList();
                if (hd != 2)
                {
                    RefList = RefList.Where(c => c.SpecialProperty.Funds == hd).ToList();
                }
                if (dl != 2)
                {
                    RefList = RefList.Where(c => c.SpecialProperty.Agent == dl).ToList();
                }
                if (sc != 2)
                {
                    RefList = RefList.Where(c => c.SpecialProperty.MarketDebt == sc).ToList();
                }
                if (yh != 2)
                {
                    RefList = RefList.Where(c => c.SpecialProperty.BankDebt == yh).ToList();
                }
                if (yj != 2)
                {
                    RefList = RefList.Where(c => c.SpecialProperty.Cash == yj).ToList();
                }
                if (fp != 2)
                {
                    RefList = RefList.Where(c => c.MissBill == fp).ToList();
                }
            }

            if (OriginData.MinMoney != 0)
            {
                RefList = RefList.Where(c => c.TotalMoney >= OriginData.MinMoney).ToList();
            }
            if (OriginData.MaxMoney != 0)
            {
                RefList = RefList.Where(c => c.TotalMoney <= OriginData.MaxMoney).ToList();
            }
            if (!string.IsNullOrEmpty(OriginData.BillNo))
            {
                OriginData.BillNo = OriginData.BillNo.ToUpper().Replace("，", "").Replace(" ", "").Replace("/", "").Replace("F", ",F").Replace("J", ",J").Replace("H", ",H");
                var list = OriginData.BillNo.Split(',').ToList();
                list.Remove("");
                RefList = RefList.Where(c => list.Contains(c.BillNo)).ToList();
            }

            // RefList = RefList.Where(c => string.IsNullOrEmpty(c.ApprovalPost)).OrderBy(c=>c.CostCenter).ToList();//错误数据
            return RefList;
        }

        public string GetALLAreaList()
        {
            var model = DbContext.DEPARTMENT.Select(x => new LoginUserIdentity
            {
                CODE = x.ID,
                NAME = x.NAME
            }).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        public FeeBillModelRef FindBillNo(string BillNo, string Permisson, string No, List<string> str)
        {
            FeeBillModelRef Model = new FeeBillModelRef();
            FeeBillModel FeeModel = new FeeBillModel();
            RefundBillModel RefundModel = new RefundBillModel();
            NoticeBillModel NoticeModel = new NoticeBillModel();
            BorrowBillModel BorrowModel = new BorrowBillModel();
            //费用单和还款单
            if (BillNo.Contains("FB"))
            {
                FeeBill FEE = new FeeBill();
                RefundBill Refund = new RefundBill();

                switch (Permisson)
                {
                    case "0":
                        FeeModel = FEE.GetBillModel(BillNo, No);
                        RefundModel = Refund.GetBillModel(BillNo, No);
                        break;
                    case "1":
                        FeeModel = FEE.GetBillModel(BillNo, str);
                        RefundModel = Refund.GetBillModel(BillNo, str);
                        break;
                    case "2":
                        FeeModel = FEE.GetBillModelPlus(BillNo, 0);
                        RefundModel = Refund.GetBillModelPlus(BillNo, 0);
                        break;
                    case "3":
                        FeeModel = FEE.GetBillModel(BillNo);
                        RefundModel = Refund.GetBillModel(BillNo);
                        break;
                    default:
                        break;
                }
            }
            else if (BillNo.Contains("FT"))
            {
                NoticeBill Notice = new NoticeBill();
                switch (Permisson)
                {
                    case "0":
                        NoticeModel = Notice.GetBillModel(BillNo, No);
                        break;
                    case "1":
                        NoticeModel = Notice.GetBillModel(BillNo, str);
                        break;
                    case "2":
                        NoticeModel = Notice.GetBillModelPlus(BillNo, 0);
                        break;
                    case "3":
                        NoticeModel = Notice.GetBillModel(BillNo);
                        break;
                    default:
                        break;
                }
            }
            else if (BillNo.ToUpper().Contains("JS"))
            {
                BorrowBill Borrow = new BorrowBill();
                switch (Permisson)
                {
                    case "0":
                        BorrowModel = Borrow.GetBillModel(BillNo, No);
                        break;
                    case "1":
                        BorrowModel = Borrow.GetBillModel(BillNo, str);
                        break;
                    case "2":
                        BorrowModel = Borrow.GetBillModelPlus(BillNo, 0);
                        break;
                    case "3":
                        BorrowModel = Borrow.GetBillModel(BillNo);
                        break;
                    default:
                        break;
                }
            }
            else if (BillNo.Contains("HK"))
            {
                RefundBill Refund = new RefundBill();
                switch (Permisson)
                {
                    case "0":
                        RefundModel = Refund.GetBillModel(BillNo, No);
                        break;
                    case "1":
                        RefundModel = Refund.GetBillModel(BillNo, str);
                        break;
                    case "2":
                        RefundModel = Refund.GetBillModelPlus(BillNo, 0);
                        break;
                    case "3":
                        RefundModel = Refund.GetBillModel(BillNo);
                        break;
                    default:
                        break;
                }
            }
            if (FeeModel != null && !string.IsNullOrEmpty(FeeModel.BillNo))
            {
                Model.BillNo = FeeModel.BillNo;
                Model.PageName = "FeeBill";
                Model.PersonInfo = new PersonInfo();
                Model.PersonInfo.Brand = FeeModel.PersonInfo.Brand;
                Model.TotalMoney = FeeModel.TotalMoney;
                Model.CreateTime = FeeModel.CreateTime;
                Model.TransactionDate = FeeModel.TransactionDate;
                Model.ApprovalTime = FeeModel.ApprovalTime;
                Model.ApprovalStatus = FeeModel.ApprovalStatus;
                Model.ApprovalPost = FeeModel.ApprovalPost;
                Model.Remark = FeeModel.Remark;
                Model.Owner = FeeModel.Owner;
                Model.Creator = FeeModel.Creator;
            }
            else if (NoticeModel != null && !string.IsNullOrEmpty(NoticeModel.BillNo))
            {

                Model.BillNo = NoticeModel.BillNo;
                Model.PageName = "NoticeBill";
                Model.PersonInfo = new PersonInfo();
                Model.PersonInfo.Brand = NoticeModel.PersonInfo.Brand;
                Model.TotalMoney = NoticeModel.TotalMoney;
                Model.CreateTime = NoticeModel.CreateTime;
                Model.TransactionDate = NoticeModel.TransactionDate;
                Model.ApprovalTime = NoticeModel.ApprovalTime;
                Model.ApprovalStatus = NoticeModel.ApprovalStatus;
                Model.ApprovalPost = NoticeModel.ApprovalPost;
                Model.Remark = NoticeModel.Remark;
                Model.ProviderName = NoticeModel.ProviderInfo.ProviderName;
                Model.Owner = NoticeModel.Owner;
                Model.Creator = NoticeModel.Creator;
            }
            else if (BorrowModel != null && !string.IsNullOrEmpty(BorrowModel.BillNo))
            {

                Model.BillNo = BorrowModel.BillNo;
                Model.PageName = "BorrowBill";
                Model.PersonInfo = new PersonInfo();
                Model.PersonInfo.Brand = BorrowModel.PersonInfo.Brand;
                Model.TotalMoney = BorrowModel.TotalMoney;
                Model.CreateTime = BorrowModel.CreateTime;
                Model.TransactionDate = BorrowModel.TransactionDate;
                Model.ApprovalTime = BorrowModel.ApprovalTime;
                Model.ApprovalStatus = BorrowModel.ApprovalStatus;
                Model.ApprovalPost = BorrowModel.ApprovalPost;
                Model.Remark = BorrowModel.Remark;
                Model.Owner = BorrowModel.Owner;
                Model.Creator = BorrowModel.Creator;
                Model.SurplusMoney = BorrowModel.SurplusMoney;
            }
            else if (RefundModel != null && !string.IsNullOrEmpty(RefundModel.BillNo))
            {

                Model.BillNo = RefundModel.BillNo;
                Model.PageName = "RefundBill";
                Model.PersonInfo = new PersonInfo();
                Model.PersonInfo.Brand = RefundModel.PersonInfo.Brand;
                Model.TotalMoney = RefundModel.RealRefundMoney;
                Model.CreateTime = RefundModel.CreateTime;
                Model.TransactionDate = RefundModel.TransactionDate;
                Model.ApprovalTime = RefundModel.ApprovalTime;
                Model.ApprovalStatus = RefundModel.ApprovalStatus;
                Model.ApprovalPost = RefundModel.ApprovalPost;
                Model.Remark = RefundModel.Remark;
                Model.Owner = RefundModel.Owner;
                Model.Creator = RefundModel.Creator;
            }
            else
            {
                return null;
            }
            return Model;
        }

        /// <summary>
        /// 获取KPI考核片区列表
        /// </summary>
        /// <returns></returns>
        public string GetKpiAreaList()
        {
            var list = GetAreaList();
            if (!string.IsNullOrEmpty(list))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct(DEPARTMENT_MERGE.id) as CODE,DEPARTMENT_MERGE.NAME  from DEPARTMENT_MERGE left join DEPARTMENT on  DEPARTMENT_MERGE.id=DEPARTMENT.MERGE_ID where DEPARTMENT.id in (" + list + ") ");

                var data = DbContext.Database.SqlQuery<LoginUserIdentity>(sb.ToString()).ToList();
                return Public.JsonSerializeHelper.SerializeToJson(data);
            }
            return "";
        }
        /// <summary>
        /// 获取当前登录用户片区权限数据
        /// </summary>
        /// <returns></returns>
        private string GetAreaList()
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            string str = DbContext.UIVALUE.Where(c => c.VALUETYPE == 3 && c.EMPLOYEENO == employee.EmployeeNo).Select(x => x.VALUE).FirstOrDefault();
            if (!string.IsNullOrEmpty(str))
            {
                return str.Trim();
            }
            else
            {
                var SingleDepartment = (from a in DbContext.SHOP
                                        join b in DbContext.DEPARTMENT on a.DEPARTMENTID equals b.ID
                                        where string.IsNullOrEmpty(employee.ShopCode) ? false : a.CODE == employee.ShopCode
                                        select b.ID).ToList();
                for (int i = 0; i < SingleDepartment.Count; i++)
                {
                    str += SingleDepartment[i] + ",";
                }
                str = str.Remove(str.Length - 1);
                return str;
            }
        }


        public string GetAccountInfoList(string IsHeadOffice, string Code)
        {
            List<NewAccountInfo> Data = new List<NewAccountInfo>();
            if (string.IsNullOrEmpty(Code))
            {
                return "";
            }
            var tp = Code.Trim().Split(',').ToList();
            decimal decCode = 0;
            foreach (var item in tp)
            {
                decCode = Convert.ToDecimal(item);
                if (IsHeadOffice == "1")
                {
                    var list = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT_TYPE == decCode).Select(x => new NewAccountInfo
                    {
                        No = x.ID,
                        Name = x.NAME,
                        ReasonCode = x.REASON_CODE
                    }).ToList();
                    Data.AddRange(list);
                }
                else
                {
                    var list = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT_TYPE == decCode && c.HIDE != 0).Select(x => new NewAccountInfo
                    {
                        No = x.ID,
                        Name = x.NAME,
                        ReasonCode = x.REASON_CODE,
                        ApprovalType = x.WORKFLOW_CODE,
                        Permission = x.PERMISSION
                    }).OrderBy(c => c.ApprovalType).ToList();
                    Data.AddRange(list);
                }
            }
            return Data.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(Data);
        }

        /// <summary>
        /// 获取费用大类或者小类名称列表
        /// </summary>
        /// <param name="StringList"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public string GetBigOrSmallSortName(String StringList, string Type)
        {
            List<string> List = new List<string>();
            List = StringList.Split(',').ToList();
            List.Remove("");
            if (Type.ToUpper() == "BIG")
            {
                List = DbContext.FEE_ACCOUNT_TYPE.Where(c => List.Contains(c.CODE)).Select(u => u.NAME).ToList();
            }
            else if (Type.ToUpper() == "SMALL")
            {
                List<decimal> DecimalList = new List<decimal>();
                foreach (var item in List)
                {
                    DecimalList.Add(Convert.ToDecimal(item));
                }
                List = DbContext.FEE_ACCOUNT.Where(c => DecimalList.Contains(c.ID)).Select(u => u.NAME).ToList();
            }
            return Public.JsonSerializeHelper.SerializeToJson(List);
        }


        public string GetCostManage(string IsHeadOffice)
        {
            List<Person> Data = null;


            try
            {
                List<short?> list = new List<short?>();

                if (IsHeadOffice == "1")
                {
                    list = DbContext.FEE_ACCOUNT.Select(x => x.ACCOUNT_TYPE).Distinct().ToList();
                }
                else
                {
                    list = DbContext.FEE_ACCOUNT.Where(c => c.HIDE != 0).Select(x => x.ACCOUNT_TYPE).Distinct().ToList();
                }

                List<string> StrList = new List<string>();
                foreach (var item in list)
                {
                    StrList.Add(item.ToString());
                }
                Data = DbContext.FEE_ACCOUNT_TYPE.Where(c => StrList.Contains(c.CODE)).OrderBy(x => x.SORT).Select(x => new Person
                {
                    No = x.CODE,
                    Name = x.NAME,
                }).ToList();
                return Data == null ? "" : Public.JsonSerializeHelper.SerializeToJson(Data);
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("获取报销大类数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }
    }
}
