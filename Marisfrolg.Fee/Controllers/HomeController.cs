using Marisfrolg.Business;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Marisfrolg.Fee.Models;
using Marisfrolg.Fee.BLL;

namespace Marisfrolg.Fee.Controllers
{

    public class HomeController : BaseController
    {
        [LoginAuthorize]
        public ActionResult Index()
        {
            Logger.Write("进入首页");
            return View();
        }

        public ActionResult Demo()
        {
            return View();
        }

        public string GetPersonInfomation()
        {
            try
            {
                EnterpriceService.MfWeiXinEmpServiceSoapClient wsc = new EnterpriceService.MfWeiXinEmpServiceSoapClient();

                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                string result = wsc.GetuseridInfo(Identify, employee.EmployeeNo);


                return result;
            }
            catch (Exception ex)
            {
                Logger.Write("微信方法报错：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "{\"errcode\":60111,\"errmsg\":\"userid not found\"}";
        }


        /// <summary>
        /// 获取公司名称
        /// </summary>
        /// <returns>返回具体公司名称</returns>
        public string GetCompanyName(string Info = "", string CompanyCode = "")
        {
            ObjectList obj = new ObjectList();
            try
            {
                if (string.IsNullOrEmpty(Info) && string.IsNullOrEmpty(CompanyCode))
                {
                    var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                    var result = DbContext.COMPANY.Where(c => c.CODE == employee.CompanyCode).Select(x => new ObjectList
                    {
                        CODE = x.CODE,
                        NAME = x.NAME
                    }).FirstOrDefault();

                    obj = result;
                }
                else if (!string.IsNullOrEmpty(Info))
                {

                    decimal dec = Convert.ToDecimal(Info);
                    var department = DbContext.DEPARTMENT.Where(c => c.ID == dec).FirstOrDefault();
                    var result = DbContext.COMPANY.Where(c => c.CODE == department.COMPANYCODE).Select(x => new ObjectList
                    {
                        CODE = x.CODE,
                        NAME = x.NAME
                    }).FirstOrDefault();

                    obj = result;
                }
                else if (!string.IsNullOrEmpty(CompanyCode))
                {
                    var result = DbContext.COMPANY.Where(c => c.CODE == CompanyCode).Select(x => new ObjectList
                    {
                        CODE = x.CODE,
                        NAME = x.NAME
                    }).FirstOrDefault();
                    obj = result;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("获取公司名称失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return obj == null ? "" : Public.JsonSerializeHelper.SerializeToJson(obj);
        }


        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public string GetBrandList()
        {
            try
            {
                List<ObjectList> brand = new List<ObjectList>()
                {
                    new ObjectList() { CODE = "MA", NAME = "Marisfrolg" },
                    new ObjectList() { CODE = "SU", NAME = "Masfer.Su" },
                    new ObjectList() { CODE = "ZH", NAME = "CHONGYUNZHU" },
                    new ObjectList() { CODE = "MD", NAME = "MDC" },
                    new ObjectList() { CODE = "DA", NAME = "Men's" },
                    new ObjectList() { CODE = "KA", NAME = "Krizia" },
                    new ObjectList() { CODE = "AM", NAME = "AUM" },
                };
                return brand == null ? "" : Public.JsonSerializeHelper.SerializeToJson(brand);

            }
            catch (Exception ex)
            {
                Logger.Write("获取品牌列表失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取默认品牌
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <param name="CompanyCode"></param>
        /// <param name="ShopCode"></param>
        /// <returns></returns>
        public string GetDefaultBrand(int IsHeadOffice, string CompanyCode = "", string ShopCode = "")
        {
            List<string> list = new List<string>();   //费用公摊
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            if (!string.IsNullOrEmpty(CompanyCode))
            {
                employee.CompanyCode = CompanyCode;
            }
            //总部
            if (IsHeadOffice == 1)
            {
                //为1000公司
                if (employee.CompanyCode == "1300")
                {
                    list.Add("AM");
                }
                //为1300公司
                else if (employee.CompanyCode == "1000")
                {
                    list.Add("MA");
                    list.Add("AM");
                    list.Add("SU");
                    list.Add("ZH");
                    list.Add("DA");
                    list.Add("KA");
                    list.Add("MD");
                }
                //明佳豪
                else if (employee.CompanyCode == "4000")
                {
                    list.Add("MA");
                    list.Add("AM");
                    list.Add("SU");
                    list.Add("ZH");
                    list.Add("DA");
                    list.Add("KA");
                    list.Add("MD");
                }
                else if (employee.CompanyCode == "2000")
                {
                    list.Add("SU");
                }
                else if (employee.CompanyCode == "2100")
                {
                    list.Add("KA");
                    list.Add("ZH");
                }
                else if (employee.CompanyCode == "2200")
                {
                    list.Add("MT");
                }
            }
            //片区
            else
            {
                //办事处
                if (string.IsNullOrEmpty(ShopCode))
                {
                    //为1000公司
                    if (employee.CompanyCode == "1000")
                    {
                        list.Add("MA");
                        list.Add("ZH");
                        list.Add("DA");
                        list.Add("MD");
                    }
                    //为1300公司
                    else if (employee.CompanyCode == "1300")
                    {
                        list.Add("AM");
                    }
                    else if (employee.CompanyCode == "2000")
                    {
                        list.Add("SU");
                    }
                    else if (employee.CompanyCode == "2100")
                    {
                        list.Add("KA");
                    }
                    else
                    {
                        list.Add("MA");
                        list.Add("SU");
                        list.Add("ZH");
                        list.Add("DA");
                        list.Add("KA");
                        list.Add("MD");
                        list.Add("AM");
                    }
                }
                //门店
                else
                {
                    string sql = "select BRAND_CODE from SHOP_BRAND_LOCATION  where  shop_code='" + ShopCode + "'";
                    list = DbContext.Database.SqlQuery<string>(sql).Distinct().ToList();
                    list.Remove("");
                }
            }
            return Public.JsonSerializeHelper.SerializeToJson(list); ;
        }

        /// <summary>
        /// 获取片区数据
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <returns></returns>
        public string GetMyArea(int IsHeadOffice, string EmployeeNo, bool IsShowClub = false)
        {
            try
            {
                List<ObjectList> ObjectModel = new List<ObjectList>();
                //1.0确定登录人是否具有多身份
                //var model = DbContext.EMPLOYEE_MUTI_DEPARTMENT.Where(c => c.EMPLOYEENO == EmployeeNo).Select(x => x.UCSTAR_ID).FirstOrDefault();

                string sql = "select UCSTAR_DEPTID from employee where no='" + EmployeeNo + "' and UCSTAR_DEPTID like '%,%'";
                string model = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();

                List<LoginUserIdentity> dataList = new List<LoginUserIdentity>();

                if (!string.IsNullOrEmpty(model))
                {
                    var tempModel = model.Trim().Split(',');
                    foreach (var item in tempModel)
                    {
                        LoginUserIdentity obj = new LoginUserIdentity();
                        var son = DbContext.DEPARTMENT.Where(c => c.UCSTAR_ID == item).FirstOrDefault();
                        if (son != null)
                        {
                            var pid = GetRootDepartment(son).ID;
                            var data = DbContext.DEPARTMENT.Where(c => c.ID == pid).Select(x => new LoginUserIdentity
                            {
                                CODE = x.ID,
                                NAME = x.NAME
                            }).FirstOrDefault();
                            dataList.Add(data);
                        }
                    }
                    dataList = dataList.GroupBy(c => c.CODE).Select(c => new LoginUserIdentity() { CODE = c.Key, NAME = c.FirstOrDefault().NAME }).ToList();
                }
                var AreaModelCount = dataList.Where(c => c.NAME.Contains("直营片区")).ToList();
                var realModel = dataList.Where(c => c.NAME.Contains("直营片区") == false).ToList();
                //是片区或者包含片区身份
                if (IsHeadOffice == 0 || AreaModelCount.Count > 0)
                {
                    var list = GetAreaList(EmployeeNo, IsShowClub);
                    foreach (var item in realModel)
                    {
                        ObjectModel.Add(new ObjectList() { CODE = item.CODE.ToString(), NAME = item.NAME });
                    }
                    ObjectModel.AddRange(list);
                }
                else
                {
                    //两个不同根部门的多身份总部用户
                    if (realModel.Count > 1)
                    {
                        foreach (var item in realModel)
                        {
                            ObjectModel.Add(new ObjectList() { CODE = item.CODE.ToString(), NAME = item.NAME });
                        }
                    }
                }
                return ObjectModel.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(ObjectModel);
            }
            catch (Exception ex)
            {
                Logger.Write("获取片区数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取当前登录用户片区权限数据
        /// </summary>
        /// <returns></returns>
        private List<ObjectList> GetAreaList(string EmployeeNo, bool IsShowClub)
        {
            var employee = (from a in DbContext.EMPLOYEE
                            join b in DbContext.SHOP on a.SHOPCODE equals b.CODE
                            into temp
                            from b in temp.DefaultIfEmpty()
                            join c in DbContext.DEPARTMENT on a.DEPID equals c.ID
                            where a.NO == EmployeeNo && !b.NAME.Contains("时尚体验中心")
                            select new
                            {
                                IsUcStar = a.IS_UCSTAR,
                                EmployeeNo = a.NO,
                                CompanyCode = c.COMPANYCODE,
                                DepartmentID = a.DEPID,
                                ShopCode = a.SHOPCODE,
                                ShopName = (b == null ? "" : b.NAME),
                                ShopType = (b == null ? "" : b.SHOP_PROPERTY),
                                Oversea = (b == null ? 0 : b.OVERSEA),
                                EmployeeName = a.NAME,
                                DepartmentName = (c == null ? "" : c.NAME),
                                COST_ACCOUNT = (c == null ? "" : c.COST_ACCOUNT)
                            }).FirstOrDefault();

            string str = DbContext.UIVALUE.Where(c => c.VALUETYPE == 3 && c.EMPLOYEENO == EmployeeNo).Select(x => x.VALUE).FirstOrDefault();
            List<ObjectList> areaList = new List<ObjectList>();
            if (!string.IsNullOrEmpty(str))
            {
                List<string> list = str.Split(',').ToList();
                list.Remove("");
                string name = "";
                decimal dec = 0;
                foreach (var item in list)
                {
                    dec = Convert.ToDecimal(item);
                    name = DbContext.DEPARTMENT.Where(c => c.ID == dec).Select(x => x.NAME).FirstOrDefault().Replace(" ", "");
                    if (!name.Contains("时尚体验中心") || IsShowClub)
                    {
                        areaList.Add(new ObjectList() { CODE = item, NAME = name });
                    }
                }
            }
            else
            {
                var SingleDepartment = (from a in DbContext.SHOP
                                        join b in DbContext.DEPARTMENT on a.DEPARTMENTID equals b.ID
                                        where string.IsNullOrEmpty(employee.ShopCode) ? false : a.CODE == employee.ShopCode
                                        select b.ID).ToList();
                foreach (var item in SingleDepartment)
                {
                    var temp = DbContext.DEPARTMENT.Where(c => c.ID == item).Select(x => x.NAME).FirstOrDefault().Replace(" ", "");
                    areaList.Add(new ObjectList() { CODE = item.ToString(), NAME = temp });
                }
            }
            return areaList;
        }

        /// <summary>
        /// 获取门店数据
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public string GetShopList(string employeeNo, decimal departmentId)
        {
            var employee = (from a in DbContext.EMPLOYEE
                            join b in DbContext.SHOP on a.SHOPCODE equals b.CODE
                            into temp
                            from b in temp.DefaultIfEmpty()
                            join c in DbContext.DEPARTMENT on a.DEPID equals c.ID
                            where a.NO == employeeNo
                            select new
                            {
                                IsUcStar = a.IS_UCSTAR,
                                EmployeeNo = a.NO,
                                CompanyCode = c.COMPANYCODE,
                                DepartmentID = a.DEPID,
                                ShopCode = a.SHOPCODE,
                                ShopName = (b == null ? "" : b.NAME),
                                ShopType = (b == null ? "" : b.SHOP_PROPERTY),
                                Oversea = (b == null ? 0 : b.OVERSEA),
                                EmployeeName = a.NAME,
                                DepartmentName = (c == null ? "" : c.NAME),
                                COST_ACCOUNT = (c == null ? "" : c.COST_ACCOUNT)
                            }).FirstOrDefault();

            try
            {
                //获取到该片区下所有店柜
                List<ObjectList> list = DbContext.SHOP.Where(c => c.DEPARTMENTID == departmentId && c.AVAILABLE == "1").Select(x => new ObjectList
                {
                    CODE = x.CODE,
                    NAME = x.NAME
                }).ToList();
                //找到我拥有的片区店柜权限
                List<string> newList = new List<string>();
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


        /// <summary>
        /// 成本中心
        /// </summary>
        /// <param name="departmentId">片区id</param>
        /// <param name="ShopId">门店id</param>
        /// <returns></returns>
        public string GetCostCenter(string departmentId, string ShopId, int IsHeadOffice)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();

            string sql = "select COSTERCENTER FROM  FEE_COSTERCENTER_EXTEND where (employeeno='" + employee.EmployeeNo + "'  and departmentid='" + departmentId + "' and type='P') or (departmentid='" + departmentId + "' and type='D')";
            string Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();


            List<string> List = new List<string>();
            List<CosterCenterExtend> NewList = new List<CosterCenterExtend>();

            if (string.IsNullOrEmpty(Database))
            {
                try
                {
                    decimal dec = 0;
                    if (string.IsNullOrEmpty(ShopId))
                    {
                        dec = Convert.ToDecimal(departmentId);
                        var department = DbContext.DEPARTMENT.Where(c => c.ID == dec).FirstOrDefault();//总部
                        var custom = DbContext.FEE_SHOP_COSTCENTER.Where(c => c.SHOPCODE == department.CODE).Select(x => x.COSTCODE).ToList();//片区

                        if (custom == null || custom.Count == 0)
                        {
                            List.Add(department.COST_ACCOUNT);
                        }
                        else
                        {
                            List.AddRange(custom);
                        }
                    }
                    else
                    {
                        string NewSql = "SELECT NEWCOSTERCENTER FROM FEE_SHOPCOSCENTER_EXTEND where OLDSHOPCODE='" + ShopId + "'";
                        string NewDatabase = DbContext.Database.SqlQuery<string>(NewSql).FirstOrDefault();
                        if (!string.IsNullOrEmpty(NewDatabase))
                        {
                            List.Add(NewDatabase);
                        }
                        else
                        {
                            var custom = DbContext.FEE_SHOP_COSTCENTER.Where(c => c.SHOPCODE == ShopId).FirstOrDefault();
                            List.Add(custom.COSTCODE);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write("获取成本中心数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                List = Database.Split(',').ToList();
                List.Remove("");
            }
            if (List.Count > 0)
            {
                foreach (var item in List)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    CosterCenterExtend obj = new CosterCenterExtend();
                    obj.CosterCenter = item;
                    if (IsHeadOffice == 1)
                    {
                        if (List.Contains("80030080"))
                        {
                            string newsql = "select BUKRS as CODE,names as NAME from FEE_SAPDATA where KOSTL like '%" + item + "%'";
                            var newvalue = DbContext.Database.SqlQuery<ObjectList>(newsql).FirstOrDefault();
                            if (newvalue != null)
                            {
                                obj.CompanyCode = newvalue.CODE;
                                obj.Description = item + "-" + newvalue.NAME;
                                if (newvalue.CODE == "2100")
                                {
                                    obj.Description += "KA";
                                }
                            }
                        }
                        else
                        {
                            var result = GetBrandFromCosterCenter(item);
                            obj.CosterCenter = item;
                            if (result.CODE == "2100")
                            {
                                obj.Description = item + "-" + result.NAME + "&ZCY";
                            }
                            else if (result.CODE == "2200")
                            {
                                obj.Description = item + "-MT";
                            }
                            else
                            {
                                obj.Description = item + "-" + result.NAME;
                            }
                            obj.CompanyCode = result.CODE;
                        }

                    }
                    else
                    {
                        if (List.Count > 1)
                        {
                            var result = GetBrandFromCosterCenterNew(item);
                            string laststring = item.Remove(0, item.Length - 1);
                            if (result.NAME == "MF" && laststring == "8")
                            {
                                obj.Description = item + "-办事处";
                                obj.CosterCenter = item;
                            }
                            else
                            {
                                obj.Description = item + "-" + result.NAME;
                                obj.CosterCenter = item;
                            }
                        }
                        else
                        {
                            obj.Description = item;
                            obj.CosterCenter = item;
                        }
                    }
                    NewList.Add(obj);
                }
            }
            return NewList.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(NewList);
        }


        /// <summary>
        /// 获取进项税信息
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <param name="CompanyCode"></param>
        /// <param name="ShopCode"></param>
        /// <returns></returns>
        public string GetTaxInfo(int IsHeadOffice, string CompanyCode, string ShopCode)
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
                return Public.JsonSerializeHelper.SerializeToJson(obj);
            }
            catch (Exception ex)
            {
                Logger.Write("获取进项税数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取未办结单据
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<FeeBillModelRef> GetNotFinishBill(string EmployeeNo)
        {
            List<FeeBillModelRef> Temp = new List<FeeBillModelRef>();
            var FeeModel = new FeeBill().GetNotFinishBill(EmployeeNo);
            if (FeeModel != null && FeeModel.Count > 0)
            {
                foreach (var item in FeeModel)
                {
                    var model = new FeeBillModelRef() { BillNo = item.BillNo, TotalMoney = item.TotalMoney, Owner = item.Owner };
                    model.PageName = "费用报销单";
                    Temp.Add(model);
                }
            }
            var NoticeModel = new NoticeBill().GetNotFinishBill(EmployeeNo);
            if (NoticeModel != null && NoticeModel.Count > 0)
            {
                foreach (var item in NoticeModel)
                {
                    var model = new FeeBillModelRef() { BillNo = item.BillNo, TotalMoney = item.TotalMoney, Owner = item.Owner };
                    model.PageName = "付款通知书";
                    Temp.Add(model);
                }
            }
            var BorrowModel = new BorrowBill().GetNotFinishBill(EmployeeNo);
            if (BorrowModel != null && BorrowModel.Count > 0)
            {
                foreach (var item in BorrowModel)
                {
                    var model = new FeeBillModelRef() { BillNo = item.BillNo, TotalMoney = item.TotalMoney, Owner = item.Owner };
                    model.PageName = "借款单";
                    Temp.Add(model);
                }
            }
            var RefundModel = new RefundBill().GetNotFinishBill(EmployeeNo);
            if (RefundModel != null && RefundModel.Count > 0)
            {
                foreach (var item in RefundModel)
                {
                    var model = new FeeBillModelRef() { BillNo = item.BillNo, TotalMoney = item.RealRefundMoney, Owner = item.Owner };
                    model.PageName = "还款单";
                    Temp.Add(model);
                }
            }
            return Temp;
        }

        /// <summary>
        /// 获取我填写的单据信息
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public string GetMyFilledBillCount(string Type)
        {
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                switch (Type)
                {
                    //未办理的单据
                    case "未办结的单据":
                        var NoDoModel = GetNotFinishBill(employee.EmployeeNo);
                        return NoDoModel.Count.ToString();
                    //发票缺失
                    case "发票缺失":
                        var NoticeModel = new NoticeBill().GetMissBill(employee.EmployeeNo);
                        return NoticeModel.Count.ToString();
                    //借款未还
                    case "借款未还":
                        var BorrowModel = new BorrowBill().GetBillForPrint(employee.EmployeeNo);
                        return BorrowModel.Count == 0 ? "0" : BorrowModel.Sum(c => c.SurplusMoney).ToString();
                    //押金
                    case "押金":
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

                WriteLog.WebGuiInLog("获取未办结单据列表数据失败" + ex.ToString(), "首页控制器GetMyFilledBillCount", "");
            }
            return "";
        }

        /// <summary>
        /// 获取我填写的单据信息
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public string GetMyFilledBill(string Type)
        {
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                switch (Type)
                {
                    //未办理的单据
                    case "未办结的单据":
                        var NoDoModel = GetNotFinishBill(employee.EmployeeNo);
                        return NoDoModel.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(NoDoModel);
                    //发票缺失
                    case "发票缺失":
                        var NoticeModel = new NoticeBill().GetMissBill(employee.EmployeeNo);
                        return NoticeModel.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(NoticeModel);
                    //借款未还
                    case "借款未还":
                        var BorrowModel = new BorrowBill().GetBillForPrint(employee.EmployeeNo);
                        return BorrowModel.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(BorrowModel);
                    //押金
                    case "押金":
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("获取未办结单据列表数据失败" + ex.ToString(), "首页控制器GetMyFilledBill", "");
            }
            return "";
        }

        /// <summary>
        /// 获取我审批的任务
        /// </summary>
        /// <param name="Type">任务类型</param>
        /// <returns></returns>
        public string GetMyProcess(string Type)
        {
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
                        //未审批的费用报销单
                        case "费用报销单":
                            var FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.FeeBillModelRef> RefList = new List<Models.FeeBillModelRef>();
                            foreach (var item in FeeModel)
                            {
                                Marisfrolg.Fee.Models.FeeBillModelRef RefModel = new Models.FeeBillModelRef();
                                RefModel = item.MapTo<FeeBillModel, FeeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefList.Add(RefModel);
                            }
                            return RefList.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(RefList);
                        //未审批的付款通知书  
                        case "付款通知书":
                            var NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.NoticeBillModelRef> NoticeRefList = new List<Models.NoticeBillModelRef>();
                            foreach (var item in NoticeModel)
                            {
                                Marisfrolg.Fee.Models.NoticeBillModelRef RefModel = new Models.NoticeBillModelRef();
                                RefModel = item.MapTo<NoticeBillModel, NoticeBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                NoticeRefList.Add(RefModel);
                            }
                            return NoticeRefList.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(NoticeRefList);
                        //未审批的借款单  
                        case "借款单":
                            var BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.BorrowBillModelRef> BorrowRefList = new List<Models.BorrowBillModelRef>();
                            foreach (var item in BorrowModel)
                            {
                                Marisfrolg.Fee.Models.BorrowBillModelRef RefModel = new Models.BorrowBillModelRef();
                                RefModel = item.MapTo<BorrowBillModel, BorrowBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                BorrowRefList.Add(RefModel);
                            }
                            return BorrowRefList.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(BorrowRefList);
                        //未审批的借款单  
                        case "还款单":
                            var RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            List<Marisfrolg.Fee.Models.RefundBillModelRef> RefundRefList = new List<Models.RefundBillModelRef>();
                            foreach (var item in RefundModel)
                            {
                                Marisfrolg.Fee.Models.RefundBillModelRef RefModel = new Models.RefundBillModelRef();
                                RefModel = item.MapTo<RefundBillModel, RefundBillModelRef>();
                                MongoDB.Bson.ObjectId id = MongoDB.Bson.ObjectId.Parse(RefModel.WorkFlowID);
                                RefModel.AssignmentID = dic.Where(c => c.Value._id == id).Select(c => c.Key).FirstOrDefault();
                                RefModel.TotalMoney = RefModel.RealRefundMoney;
                                RefundRefList.Add(RefModel);
                            }
                            return RefundRefList.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(RefundRefList);
                        default:
                            break;
                    }

                }
                return "";
            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("获取我审批的任务" + ex.ToString(), "首页控制器GetMyProcess", "");
            }
            return "";
        }

        /// <summary>
        /// 获取我审批的任务
        /// </summary>
        /// <param name="Type">任务类型</param>
        /// <returns></returns>
        public string GetMyProcessCount(string Type)
        {
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
                        //未审批的费用报销单
                        case "费用报销单":
                            var FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            return FeeModel.Count.ToString();
                        //未审批的付款通知书  
                        case "付款通知书":
                            var NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            return NoticeModel.Count.ToString();
                        //未审批的借款单  
                        case "借款单":
                            var BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            return BorrowModel.Count.ToString();
                        //未审批的借款单  
                        case "还款单":
                            var RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(employee.EmployeeNo, WorkFlowList);
                            return RefundModel.Count.ToString();
                        default:
                            break;
                    }

                }
                return "";
            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("获取我审批的任务" + ex.ToString(), "首页控制器GetMyProcessCount", "");
            }
            return "";
        }

        /// <summary>
        /// 提交审批（通过/不通过）
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string SubmitWorkFlowList(string AssignmentID, string ActionName, string Remark = "", string ApprovalPost = "", string ActiveID = "")
        {
            try
            {
                var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
                //if ((ApprovalPost.Contains("总经办") || ApprovalPost.Contains("出纳")) && ActionName == "0")
                //{
                //    ActionName = "2";
                //}
                try
                {
                    WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Action", ActionName);
                    dic.Add("Remark", Remark);
                    if (!string.IsNullOrEmpty(ActiveID))
                    {
                        dic.Add("NextActivityID", ActiveID);
                    }
                    proxy.ProcessWorkFlow("", AssignmentID, dic, employee.EmployeeNo);
                    return "Yes";
                }
                catch (Exception ex)
                {
                    return "";
                }

            }
            catch (Exception ex)
            {
                Logger.Write("提交审批失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }


        public string ToggleUserIdentity(decimal RootDepartmentCode, decimal SelectDepartmentCode, string EmployeeNo)
        {

            string sql = "SELECT TYPE FROM FEE_PERSON_EXTEND where TYPE='Cookie' and  value like '%" + EmployeeNo + "%' ";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            if (string.IsNullOrEmpty(Database))
            {
                return "";
            }

            //初始根部门
            var dp1 = DbContext.DEPARTMENT.Where(c => c.ID == RootDepartmentCode).FirstOrDefault();
            var root1 = GetRootDepartment(dp1);
            //选择的次级部门
            var dp2 = DbContext.DEPARTMENT.Where(c => c.ID == SelectDepartmentCode).FirstOrDefault();
            var root = GetRootDepartment(dp2);
            //选择的根部门
            if (root1.IsHeadOffice == root.IsHeadOffice)
            {
                return "";
            }
            else
            {
                //跟新cookie
                var query = (from a in DbContext.EMPLOYEE
                             join b in DbContext.SHOP on a.SHOPCODE equals b.CODE
                             into temp
                             from b in temp.DefaultIfEmpty()
                             join c in DbContext.DEPARTMENT on a.DEPID equals c.ID
                             where a.NO == EmployeeNo
                             select new
                             {
                                 IsUcStar = a.IS_UCSTAR,
                                 EmployeeNo = a.NO,
                                 CompanyCode = c.COMPANYCODE,
                                 DepartmentID = a.DEPID,
                                 ShopCode = a.SHOPCODE,
                                 ShopName = (b == null ? "" : b.NAME),
                                 ShopType = (b == null ? "" : b.SHOP_PROPERTY),
                                 Oversea = (b == null ? 0 : b.OVERSEA),
                                 EmployeeName = a.NAME,
                                 Password = a.PASSWORD,
                                 DepartmentName = (c == null ? "" : c.NAME),
                                 COST_ACCOUNT = (c == null ? "" : c.COST_ACCOUNT)
                             }).FirstOrDefault();

                var CompanyName = DbContext.COMPANY.Where(c => c.CODE == root.COMPANYCODE).Select(x => x.NAME).FirstOrDefault();

                string employeeInfo = "{CompanyCode:'" + root.COMPANYCODE
                                       + "',CompanyName:'" + HttpUtility.UrlEncode(CompanyName)
                                       + "',DepartmentID:'" + query.DepartmentID
                                       + "',ShopCode:'" + (string.IsNullOrEmpty(query.ShopCode) ? "" : query.ShopCode)
                                       + "',ShopName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.ShopName) ? "" : query.ShopName), System.Text.Encoding.UTF8)
                                       + "',ShopType:'" + (string.IsNullOrEmpty(query.ShopType) ? "" : query.ShopType)
                                       + "',EmployeeNo:'" + query.EmployeeNo
                                       + "',e:'" + Marisfrolg.Public.Common.Encryption(query.EmployeeNo)
                                       + "',PassWord:'" + query.Password
                                       + "',Oversea:'" + query.Oversea
                                       + "',IsUcStar:'" + query.IsUcStar.ToString()
                                       + "',EmployeeName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.EmployeeName) ? "" : query.EmployeeName), System.Text.Encoding.UTF8)
                                       + "',DepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(root.NAME) ? "" : root.NAME), System.Text.Encoding.UTF8)
                                       + "',RootDepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(root.NAME) ? "" : root.NAME), System.Text.Encoding.UTF8)
                                       + "',RootDepartmentID:'" + root.ID
                                       + "',BankCardNo:'" + HttpUtility.UrlEncode("********", System.Text.Encoding.UTF8)
                                       + "',COST_ACCOUNT:'" + (string.IsNullOrEmpty(root.COST_ACCOUNT) ? "" : root.COST_ACCOUNT)
                                       + "',IsHeadOffice:'" + (root.IsHeadOffice ? "1" : "0") //是否总部人员，1表示是
                                       + "',WebSocketConnection:'" + System.Web.Configuration.WebConfigurationManager.AppSettings["WebSocketConnection"]
                                       + "',UPCODE:'" + Common.Encryption(EmployeeNo)
                                       + "'}";
                //跟新Cookie
                Marisfrolg.Public.CookieHelper.RemoveEmployeeInfo();
                Marisfrolg.Public.CookieHelper.SetEmplyeeInfo(employeeInfo);
                Logger.Write("跟新COOKIE成功:方法为ToggleUserIdentity,时间是" + DateTime.Now);
                return Public.JsonSerializeHelper.SerializeToJson(employeeInfo);
            }
        }
    }
}
