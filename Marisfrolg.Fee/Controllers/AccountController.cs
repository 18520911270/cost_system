using Marisfrolg.Business;
using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marisfrolg.Fee.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController()
            : base()
        {
        }

        public ActionResult Login()
        {
            return View();
        }


        void TestData()
        {
            var NewDpModel = DbContext.DEPARTMENT.Where(c => c.IS_UCSTAR == 2).ToList();

            foreach (var item in NewDpModel)
            {
                var oldDpModel = DbContext.DEPARTMENT.Where(c => c.NAME == item.NAME && c.IS_UCSTAR == 1 && c.COMPANYCODE == item.COMPANYCODE).FirstOrDefault();

                if (oldDpModel != null)
                {
                    DbContext.Database.ExecuteSqlCommand(" update DEPARTMENT set COST_ACCOUNT='" + oldDpModel.COST_ACCOUNT + "' where id=" + item.ID + " ");
                }
            }

            //处理扩展信息
            string sql = "SELECT DEPARTMENTID as CODE,COSTERCENTER as NAME FROM FEE_COSTERCENTER_EXTEND";
            var ExpendModel = DbContext.Database.SqlQuery<ObjectList>(sql).ToList();

            foreach (var item in ExpendModel)
            {
                var code = Convert.ToDecimal(item.CODE);
                var ex = DbContext.DEPARTMENT.Where(c => c.ID == code).FirstOrDefault();

                var exid = DbContext.DEPARTMENT.Where(c => c.NAME == ex.NAME && c.COMPANYCODE == ex.COMPANYCODE).FirstOrDefault();

                if (exid != null)
                {

                    DbContext.Database.ExecuteSqlCommand(" insert into FEE_COSTERCENTER_EXTEND (DEPARTMENTID,COSTERCENTER,TYPE)        values('" + exid.ID + "','" + item.NAME + "','D')");
                }
            }
        }

        /// <summary>
        /// 登陆验证
        /// </summary>
        /// <param name="EmployeeNo">员工工号</param>
        /// <param name="PassWord">登陆密码</param>
        /// <param name="Code">验证码</param>
        /// <returns></returns>
        [HttpPost]
        public string Login(string employeeNo, string password, string loginType = null)
        {
            string errorMsg = string.Empty;
            string shopList = "[]";
            string permissionList = "[]";
            try
            {
                if (loginType != null)
                {
                    employeeNo = Marisfrolg.Public.Common.Decryption(employeeNo);
                    password = Marisfrolg.Public.Common.Decryption(password);
                }

                EMPLOYEE employee = DbContext.EMPLOYEE.Where(m => m.NO == employeeNo).FirstOrDefault();
                if (employee == null)
                {
                    errorMsg = "工号不存在!";
                }
                else
                {
                    //判断是否UcStar账号
                    bool IS_UCSTAR = false;
                    try
                    {
                        IS_UCSTAR = ((short)employee.IS_UCSTAR == 2);
                    }
                    catch { }


                    if (employee.LEAVE == "1" || employee.AVAILABLE == "0")
                    {
                        errorMsg = "离职或冻结账号!";
                    }
                    else if (IS_UCSTAR)
                    {
                        try
                        {
                            ////UcStar用户
                            //UcStar.UcstarWebserviceService cl = new UcStar.UcstarWebserviceService();
                            //cl.Timeout = 1000;


#if !DEBUG
                           password = Common.Encryption(password);
                            if (employee.PASSWORD != password)
                            {
                                errorMsg = "密码输入错误!";
                            }        
#endif

                        }
                        catch
                        {
                            //UC连接失败则走ODS数据库验证 00897 2016-04-27
                            password = Common.Encryption(password);
                            if (employee.PASSWORD != password)
                            {
                                errorMsg = "密码输入错误!";
                            }
                        }
                    }
                    else
                    {
                        //非UcStar用户
#if !DEBUG
                        password = Common.Encryption(password);
                        if (employee.PASSWORD != password)
                            errorMsg = "密码输入错误!";
                        
#endif

                    }
                }

                if (string.IsNullOrEmpty(errorMsg))
                {
                    var query = (from a in DbContext.EMPLOYEE
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


                    //获取当前员工可操作的店柜
                    //UIVALUE uiValue = DbContext.UIVALUE.Where(m => m.EMPLOYEENO == query.EmployeeNo && m.VALUETYPE == 4).FirstOrDefault();
                    //string uivalue = (uiValue == null || string.IsNullOrEmpty(uiValue.VALUE)) ? "" : uiValue.VALUE;
                    //var Shops = (from m in DbContext.SHOP
                    //             join d in DbContext.DEPARTMENT on m.DEPARTMENTID equals d.ID
                    //             where uivalue.Contains(m.CODE)
                    //             select new
                    //             {
                    //                 shopCode = m.CODE,
                    //                 shopName = m.NAME,
                    //                 currency = m.CURRENCY.CODE,
                    //                 departmentName = d.NAME,
                    //                 departmentId = d.ID,
                    //                 property = m.SHOP_PROPERTY,
                    //                 oversea = m.OVERSEA
                    //             }).Union(
                    //                 from x in DbContext.SHOP
                    //                 join y in DbContext.DEPARTMENT on x.DEPARTMENTID equals y.ID
                    //                 where (string.IsNullOrEmpty(query.ShopCode) ? false : x.CODE == query.ShopCode)
                    //                 select new
                    //                 {
                    //                     shopCode = x.CODE,
                    //                     shopName = x.NAME,
                    //                     currency = x.CURRENCY.CODE,
                    //                     departmentName = y.NAME,
                    //                     departmentId = y.ID,
                    //                     property = x.SHOP_PROPERTY,
                    //                     oversea = x.OVERSEA
                    //                 }
                    //                 ).OrderBy(s => s.shopCode).ToList();

                    //if (Shops != null && Shops.Count > 0)
                    //{
                    //    shopList = JsonSerializeHelper.SerializeToJson(Shops);
                    //}
                    //else
                    //{
                    //    errorMsg = "请设置店柜权限";
                    //    return "{msg:'请设置店柜权限',shopList:" + shopList + "}";
                    //}

                    DEPARTMENT root = GetRootDepartment(employee.DEPARTMENT);
                    var CompanyName = DbContext.COMPANY.Where(c => c.CODE == root.COMPANYCODE).Select(x => x.NAME).FirstOrDefault();

                    string employeeInfo = "{CompanyCode:'" + query.CompanyCode
                                           + "',CompanyName:'" + HttpUtility.UrlEncode(CompanyName)
                                           + "',DepartmentID:'" + query.DepartmentID
                                           + "',ShopCode:'" + (string.IsNullOrEmpty(query.ShopCode) ? "" : query.ShopCode)
                                           + "',ShopName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.ShopName) ? "" : query.ShopName), System.Text.Encoding.UTF8)
                                           + "',ShopType:'" + (string.IsNullOrEmpty(query.ShopType) ? "" : query.ShopType)
                                           + "',EmployeeNo:'" + query.EmployeeNo
                                           + "',e:'" + Marisfrolg.Public.Common.Encryption(query.EmployeeNo)
                                           + "',PassWord:'" + Marisfrolg.Public.Common.Encryption(password)
                                           + "',Oversea:'" + query.Oversea
                                           + "',IsUcStar:'" + query.IsUcStar.ToString()
                                           + "',EmployeeName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.EmployeeName) ? "" : query.EmployeeName), System.Text.Encoding.UTF8)
                                           + "',DepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.DepartmentName) ? "" : query.DepartmentName), System.Text.Encoding.UTF8)
                                           + "',RootDepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(root.NAME) ? "" : root.NAME), System.Text.Encoding.UTF8)
                                           + "',RootDepartmentID:'" + root.ID
                                           + "',BankCardNo:'" + HttpUtility.UrlEncode("********", System.Text.Encoding.UTF8)
                                           + "',COST_ACCOUNT:'" + (string.IsNullOrEmpty(root.COST_ACCOUNT) ? "" : root.COST_ACCOUNT)
                                           + "',IsHeadOffice:'" + (root.IsHeadOffice ? "1" : "0") //是否总部人员，1表示是
                                           + "',WebSocketConnection:'" + System.Web.Configuration.WebConfigurationManager.AppSettings["WebSocketConnection"]
                                           + "',UPCODE:'" + Common.Encryption(employeeNo)
                                           + "'}";

                    Marisfrolg.Public.CookieHelper.RemoveEmployeeInfo();
                    Marisfrolg.Public.CookieHelper.SetEmplyeeInfo(employeeInfo);

                    //操作日志
                    Marisfrolg.Public.Common.OperationLog(Common.GetClientComputerName(), employee.NO, employee.NO, this.GetType().ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name, "Employee", "员工登陆", "", "");
                    DbContext.SaveChanges();
                    errorMsg = "success";
                }
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.Write("登陆失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",登陆账号:" + employeeNo);
                errorMsg = "数据库连接失败!";
            }
            catch (Exception ex)
            {
                Logger.Write("登陆失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",登陆账号:" + employeeNo);
                errorMsg = "系统错误!";
            }

            //permissionList = JsonSerializeHelper.SerializeToJson(GetPermission(employeeNo));

            return "{msg:'" + errorMsg + "',shopList:" + shopList + ",permissionList:" + permissionList + "}";
        }

        //注销
        public ActionResult LogOut()
        {
            Marisfrolg.Public.CookieHelper.RemoveEmployeeInfo();
            return Redirect("~/Account/Login");
        }

        public string GetPermissionJson(string EmployeeNo)
        {
            return JsonSerializeHelper.SerializeToJson(GetPermission(EmployeeNo));

        }

        //获取权限
        private List<NewPERMISSION> GetPermission(string EmployeeNo)
        {
            EmployeeInfo employeeInfo = Common.GetEmployeeInfo();
            List<NewPERMISSION> Permissions = new List<NewPERMISSION>();
            try
            {

                string sql = "SELECT d.* FROM EMPLOYEE_ROLE a LEFT  join  ROLE b on a.ROLEID=b.ID left  join  ROLE_PERMISSION c on  b.ID = c.ROLEID left join PERMISSION  d on c.PERMISSIONID=d.ID  where a.EMPLOYEENO='" + EmployeeNo + "' and  d.APPTYPE = 'WORKFLOW' union   select b.* from  EMPLOYEE_PERMISSION a  left join PERMISSION b on a.PERMISSIONID=b.ID where a.EMPLOYEENO ='" + EmployeeNo + "' and b.APPTYPE = 'WORKFLOW'";

                List<NewPERMISSION> permisson = DbContext.Database.SqlQuery<NewPERMISSION>(sql).ToList().Distinct().ToList();
                return permisson;

                #region 之间的linq莫名的报错了
                //var temp = ((from a in DbContext.EMPLOYEE_ROLE
                //             join b in DbContext.ROLE on a.ROLEID equals b.ID
                //             join c in DbContext.ROLE_PERMISSION on b.ID equals c.ROLEID
                //             join d in DbContext.PERMISSION on c.PERMISSIONID equals d.ID
                //             where a.EMPLOYEENO == EmployeeNo && d.APPTYPE == "WORKFLOW"
                //             //&& d.APPTYPE=="ICPOS"
                //             select d).Union(
                //                        from a in DbContext.EMPLOYEE_PERMISSION
                //                        join b in DbContext.PERMISSION on a.PERMISSIONID equals b.ID
                //                        where a.EMPLOYEENO == EmployeeNo
                //                        && b.APPTYPE == "WORKFLOW"
                //                        //&& b.APPTYPE == "ICPOS"
                //                        select b
                //                    )).Distinct().ToList();

                //foreach (var item in temp)
                //{
                //    Permissions.Add(new PERMISSION()
                //    {
                //        NAME = item.NAME,
                //        CONTROLLER = item.CONTROLLER,
                //        ACTION = item.ACTION,
                //        PID = item.PID

                //    });
                //} 
                #endregion
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return Permissions;
        }


    }
}
