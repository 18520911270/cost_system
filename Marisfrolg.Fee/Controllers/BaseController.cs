using Marisfrolg.Business;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Aspose.Words;
using Aspose.Cells;
using System.Drawing.Imaging;

namespace Marisfrolg.Fee.Controllers
{
    public class BaseController : Controller
    {

        //微信接口配置参数
        protected static string Identify = "Marisfrolg";


        protected static string _WebApiBaseUri = System.Configuration.ConfigurationManager.AppSettings["WebApiBaseUri"];
        public BaseController()
        {

            this.ViewBag.WebApiBaseUri = System.Configuration.ConfigurationManager.AppSettings["WebApiBaseUri"];



        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="sFile"></param>
        /// <param name="outPath"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool GetPicThumbnail(string sFile, string outPath, string outname, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;

            //以下代码为保存图片时，设置压缩质量  
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100  
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    iSource.Save(outPath, jpegICIinfo, ep);//dFile是压缩后的新路径  
                }
                else
                {
                    iSource.Save(outPath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //释放句柄
                iSource.Dispose();

                //删掉原图
                System.IO.File.Delete(sFile);

                System.IO.File.Move(outPath, outname);
            }
        }


        [HttpPost]
        public string FileUpload(string time)
        {
            string filePathName = string.Empty;
            string NewfilePathName = string.Empty;
            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload");
            if (!System.IO.Directory.Exists(localPath))
            {
                System.IO.Directory.CreateDirectory(localPath);
            }
            string NewlocalPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", time);
            if (!System.IO.Directory.Exists(NewlocalPath))
            {
                System.IO.Directory.CreateDirectory(NewlocalPath);
            }
            if (HttpContext.Request.Files.Count == 0)
            {
                return "Fail";
            }
            else
            {

                for (int i = 0; i < HttpContext.Request.Files.Count; i++)
                {
                    var guid = Guid.NewGuid().ToString("N");

                    //保存源文件
                    var file = HttpContext.Request.Files[i];
                    string extension = Path.GetExtension(file.FileName);
                    filePathName = guid + extension;
                    string filename = Path.Combine(NewlocalPath, filePathName);
                    file.SaveAs(filename);

                    //保存WORD文件
                    if (extension == ".doc" || extension == ".docx")
                    {
                        extension = ".pdf";
                        NewfilePathName = guid + extension;
                        Document doc = new Document(file.InputStream);
                        doc.Save(Path.Combine(NewlocalPath, NewfilePathName), Aspose.Words.SaveFormat.Pdf);
                    }
                    //保存excel文件
                    else if (extension == ".xls" || extension == ".xlsx")
                    {
                        extension = ".pdf";
                        NewfilePathName = guid + extension;
                        Workbook excel = new Workbook(file.InputStream);
                        excel.Save(Path.Combine(NewlocalPath, NewfilePathName), Aspose.Cells.SaveFormat.Pdf);
                    }
                    else if (extension == ".jpg" || extension == ".jpeg" || extension == ".bmp" || extension == ".png" || extension == ".gif")
                    {
                        string outmame = Path.Combine(NewlocalPath, guid + ".jpg");

                        //压缩
                        GetPicThumbnail(filename, Path.Combine(NewlocalPath, Guid.NewGuid().ToString("N") + ".jpg"), outmame, 50);

                        filePathName = guid + ".jpg";
                    }

                    var applicationPath = VirtualPathUtility.ToAbsolute("~") == "/" ? "" : VirtualPathUtility.ToAbsolute("~");
                    string urlPath = applicationPath + "/Upload";
                }
            }

            if (string.IsNullOrEmpty(NewfilePathName))
            {
                return filePathName;
            }
            else
            {
                return NewfilePathName + "," + filePathName;
            }
        }


        public string Domain
        {
            get
            {
                return this.Request.Url.Host + Request.ApplicationPath;
            }
        }


        private void WeiXin_SDK_Config_Prepare()
        {
            Marisfrolg.Fee.BLL.WeChat.WeiXin_SDK_CONFIG config = new Marisfrolg.Fee.BLL.WeChat.WeiXin_SDK_CONFIG();
            Marisfrolg.Fee.BLL.WeChat.WeiXin_SDK_CONFIG_MODEL config_model = config.getConfig(this.Request.Url.ToString());
            this.ViewData["Config"] = config_model;
        }


        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //微信JSSDK接口参数准备
            try
            {
                WeiXin_SDK_Config_Prepare();
            }
            catch { }

            base.OnActionExecuted(filterContext);
            string domain = regexdom(this.Request.Url.ToString());
            //string agentdomain = regexdom(_WebApiBaseUri);
            //this.ViewBag.WebApiBaseUri = _WebApiBaseUri.Replace(agentdomain,domain);
        }


        private string regexdom(string url)
        {
            string text = url;
            string pattern = @"(?<=http://)[\w\.]+[^/]";　//C#正则表达式提取匹配URL的模式，       
            string domain = "";
            MatchCollection mc = Regex.Matches(text, pattern);//满足pattern的匹配集合        
            foreach (Match match in mc)
            {
                domain = match.ToString();
            }
            return domain;
        }


        public ActionResult AutoLogin()
        {
            string errorMsg = "";

            //从微信获取传入参数

            #region 从微信平台引入的自动登陆
            string code = Request["code"];
            string state = Request["state"];
            Logger.Write("code:" + code + "|state:" + state);

            EnterpriceService.MfWeiXinEmpServiceSoapClient wsc = new EnterpriceService.MfWeiXinEmpServiceSoapClient();


            string userId = "";
            int result = 0;

            try
            {
                result = wsc.GetUserId(Identify, code, ref userId);
            }
            catch (Exception)
            {
                Logger.Write("企业微信调用失败");
            }

            Logger.Write("result:" + result + "|userid:" + userId);
            #endregion

            #region 从ODS平台引入的自动登陆
            if (string.IsNullOrEmpty(userId))
            {
                userId = Public.Common.Decryption(Request["uc"].ToString());
                string passWord = Request["up"].ToString();
                Logger.Write("userId:" + userId + "|passWord:" + passWord);
            }
            #endregion

            //自动登陆
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");


            try
            {


                EMPLOYEE employee = DbContext.EMPLOYEE.Where(m => m.NO == userId).FirstOrDefault();
                if (employee == null)
                {
                    errorMsg = "工号不存在!";
                }
                else
                {
                    if (employee.LEAVE == "1" || employee.AVAILABLE == "0")
                    {
                        errorMsg = "此账号不可用!";
                    }

                }


                if (string.IsNullOrEmpty(errorMsg))
                {
                    var query = (from a in DbContext.EMPLOYEE
                                 join b in DbContext.SHOP on a.SHOPCODE equals b.CODE
                                 into temp
                                 from b in temp.DefaultIfEmpty()
                                 join c in DbContext.DEPARTMENT on a.DEPID equals c.ID
                                 where a.NO == userId
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
                                     DepartmentName = (c == null ? "" : c.NAME)
                                 }).FirstOrDefault();


                    //获取当前员工可操作的店柜
                    UIVALUE uiValue = DbContext.UIVALUE.Where(m => m.EMPLOYEENO == query.EmployeeNo && m.VALUETYPE == 4).FirstOrDefault();
                    string uivalue = (uiValue == null || string.IsNullOrEmpty(uiValue.VALUE)) ? "" : uiValue.VALUE;
                    var Shops = (from m in DbContext.SHOP
                                 join d in DbContext.DEPARTMENT on m.DEPARTMENTID equals d.ID
                                 where uivalue.Contains(m.CODE)
                                 select new
                                 {
                                     shopCode = m.CODE,
                                     shopName = m.NAME,
                                     currency = m.CURRENCY.CODE,
                                     departmentName = d.NAME,
                                     departmentId = d.ID,
                                     property = m.SHOP_PROPERTY,
                                     oversea = m.OVERSEA
                                 }).Union(
                                     from x in DbContext.SHOP
                                     join y in DbContext.DEPARTMENT on x.DEPARTMENTID equals y.ID
                                     where (string.IsNullOrEmpty(query.ShopCode) ? false : x.CODE == query.ShopCode)
                                     select new
                                     {
                                         shopCode = x.CODE,
                                         shopName = x.NAME,
                                         currency = x.CURRENCY.CODE,
                                         departmentName = y.NAME,
                                         departmentId = y.ID,
                                         property = x.SHOP_PROPERTY,
                                         oversea = x.OVERSEA
                                     }
                                     ).OrderBy(s => s.shopCode).ToList();


                    DEPARTMENT root = GetRootDepartment(employee.DEPARTMENT);
                    var CompanyName = DbContext.COMPANY.Where(c => c.CODE == root.COMPANYCODE).Select(x => x.NAME).FirstOrDefault();
                    string employeeInfo = "{CompanyCode:'" + query.CompanyCode
                                           + "',CompanyName:'" + HttpUtility.UrlEncode(CompanyName)
                                           + "',DepartmentID:'" + query.DepartmentID
                                           + "',ShopCode:'" + (string.IsNullOrEmpty(query.ShopCode) ? "" : query.ShopCode)
                                           + "',ShopName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.ShopName) ? "" : query.ShopName), System.Text.Encoding.UTF8)
                                           + "',ShopType:'" + (string.IsNullOrEmpty(query.ShopType) ? "" : query.ShopType)
                                           + "',EmployeeNo:'" + query.EmployeeNo
                                           + "',Oversea:'" + query.Oversea
                                           + "',IsUcStar:'" + query.IsUcStar.ToString()
                                           + "',EmployeeName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.EmployeeName) ? "" : query.EmployeeName), System.Text.Encoding.UTF8)
                                           + "',DepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(query.DepartmentName) ? "" : query.DepartmentName), System.Text.Encoding.UTF8)
                                           + "',RootDepartmentName:'" + HttpUtility.UrlEncode((string.IsNullOrEmpty(root.NAME) ? "" : root.NAME), System.Text.Encoding.UTF8)
                                           + "',RootDepartmentID:'" + root.ID
                                           + "',COST_ACCOUNT:'" + (string.IsNullOrEmpty(root.COST_ACCOUNT) ? "" : root.COST_ACCOUNT)
                                           + "',IsHeadOffice:'" + (root.IsHeadOffice ? "1" : "0") //是否总部人员，1表示是
                                           + "',BankCardNo:'" + HttpUtility.UrlEncode("********", System.Text.Encoding.UTF8)
                                           + "',WebSocketConnection:'" + System.Web.Configuration.WebConfigurationManager.AppSettings["WebSocketConnection"]
                                           + "',UPCODE:'" + Common.Encryption(query.EmployeeNo)
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
                Logger.Write("登陆失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",登陆账号:" + userId);
                errorMsg = "数据库连接失败!";
            }
            catch (Exception ex)
            {
                Logger.Write("登陆失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",登陆账号:" + userId);
                errorMsg = "系统错误!";
            }


            return RedirectToAction("Index");
            //return RedirectToAction("Index", "Home");
        }



        public DEPARTMENT GetRootDepartment(DEPARTMENT son)
        {
            if (son.IS_UCSTAR != 2)
            {
                son.IsHeadOffice = false;
                return son;
            }
            else
            {
                son.IsHeadOffice = true;
            }

            //需要独立的部门维护到表
            string sql = "SELECT  VALUE  FROM FEE_PERSON_EXTEND where TYPE='company'  and DEPARTMENTNAME='BaseController' ";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            List<string> list = Database.Split(',').ToList();
            list.Remove("");
            foreach (var item in list)
            {
                decimal i = Convert.ToDecimal(item);
                if (son.PID == i)
                {
                    son.IsHeadOffice = true;
                    return son;
                }
            }

            var parent = DbContext.DEPARTMENT.SingleOrDefault(s => s.ID == son.PID);
            if (parent != null)
            {
                return GetRootDepartment(parent);
            }
            else
            {
                return son;
            }

        }



        //获取权限
        private List<PERMISSION> GetPermission(string EmployeeNo)
        {
            EmployeeInfo employeeInfo = Common.GetEmployeeInfo();
            List<PERMISSION> Permissions = new List<PERMISSION>();
            try
            {
                var temp = ((from a in DbContext.EMPLOYEE_ROLE
                             join b in DbContext.ROLE on a.ROLEID equals b.ID
                             join c in DbContext.ROLE_PERMISSION on b.ID equals c.ROLEID
                             join d in DbContext.PERMISSION on c.PERMISSIONID equals d.ID
                             where a.EMPLOYEENO == EmployeeNo
                             //&& d.APPTYPE=="ICPOS"
                             select d).Union(
                                        from a in DbContext.EMPLOYEE_PERMISSION
                                        join b in DbContext.PERMISSION on a.PERMISSIONID equals b.ID
                                        where a.EMPLOYEENO == EmployeeNo
                                        //&& b.APPTYPE == "ICPOS"
                                        select b
                                    )).Distinct().ToList();

                foreach (var item in temp)
                {
                    Permissions.Add(new PERMISSION()
                    {
                        NAME = item.NAME,
                        CONTROLLER = item.CONTROLLER,
                        ACTION = item.ACTION,

                    });
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return Permissions;
        }





        public OraConnection DbContext
        {
            get
            {
                //当第二次执行的时候直接取出线程嘈里面的对象
                //CallContext:是线程内部唯一的独用的数据槽(一块内存空间)
                //数据存储在线程栈中
                //线程内共享一个单例
                OraConnection dbcontext = CallContext.GetData("DbContext") as OraConnection;

                //判断线程里面是否有数据
                if (dbcontext == null)  //线程的数据槽里面没有次上下文
                {
                    dbcontext = new OraConnection();  //创建了一个EF上下文
                    //存储指定对象
                    CallContext.SetData("DbContext", dbcontext);
                }
                return dbcontext;
            }
        }

        //自定义错误处理
        protected override void OnException(ExceptionContext filterContext)
        {
            //记录错误日志
            Exception ex = filterContext.Exception;
            if (ex != null)
            {
                string controllerName = (string)filterContext.RouteData.Values["controller"];
                string actionName = (string)filterContext.RouteData.Values["action"];
                string errorInfo = "controller[" + controllerName + "],action[" + actionName + "]，error[" + ex.Message.ToString() + "]";
                Logger.Write(errorInfo);

                // 标记异常已处理
                filterContext.ExceptionHandled = true;

                ContentResult cResult = new ContentResult();
                cResult.Content = "系统异常,错误消息[" + ex.Message.ToString() + "]!";
                filterContext.Result = cResult;


                // 跳转到错误页
                //filterContext.Result = new RedirectResult(Url.Action("Error", "Shared"));

                //filterContext.RequestContext.HttpContext.Response.Write("123");
            }
        }




        public string GetCosterCenterName(string CosterCenter)
        {
            string str = CosterCenter.Remove(2);
            int i = 0;
            bool IsNumber = int.TryParse(CosterCenter.Remove(1), out i);
            //整合8位的成本中心
            if (str != "00" && CosterCenter.Length == 8 && IsNumber)
            {
                CosterCenter = "00" + CosterCenter;
            }
            string sql = "select names as NAME from FEE_SAPDATA where KOSTL='" + CosterCenter + "'";
            var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            return Database;
        }


        public string GetCompanyCode(string Brand)
        {
            string companyCode = string.Empty;
            switch (Brand.ToUpper())
            {
                case "MF":
                    companyCode = "1000";
                    break;
                case "SU":
                    companyCode = "2000";
                    break;
                case "AM":
                    companyCode = "1300";
                    break;
                case "KA":
                    companyCode = "2100";
                    break;
                case "ZCY":
                    companyCode = "2100";
                    break;
                case "MDC":
                    companyCode = "1000";
                    break;
                case "集团":
                    companyCode = "1000";
                    break;
                case "明佳豪":
                    companyCode = "4000";
                    break;
                case "DA":
                    companyCode = "1000";
                    break;
                default:
                    break;
            }
            return companyCode;
        }


        /// <summary>
        /// 总部获取成本中心归属方法
        /// </summary>
        /// <param name="CosterCenter"></param>
        /// <returns></returns>
        public ObjectList GetBrandFromCosterCenter(string CosterCenter)
        {
            ObjectList Result = new ObjectList();
            string str = CosterCenter.Remove(2);
            int i = 0;
            bool IsNumber = int.TryParse(CosterCenter.Remove(1), out i);
            //整合8位的成本中心
            if (str != "00" && CosterCenter.Length == 8 && IsNumber)
            {
                CosterCenter = "00" + CosterCenter;
            }
            string Brand = "MF";
            string sql = "select BUKRS as CODE,names as NAME from FEE_SAPDATA where KOSTL='" + CosterCenter + "'";
            var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();
            if (Database != null)
            {
                Result.CODE = Database.CODE;
                switch (Database.CODE)
                {
                    case "2000":
                        Brand = "SU";
                        break;
                    case "1300":
                        Brand = "AM";
                        break;
                    case "2100":
                        Brand = "KA";
                        break;
                    case "2300":
                        Brand = "DA";
                        break;
                        //2200走DA的流程，实际上显示为MT
                    case "2200":
                        Brand = "DA";
                        break;
                    //默认处理为1000公司
                    default:
                        CosterCenter = CosterCenter.Remove(0, 2);
                        CosterCenter = CosterCenter.Remove(4);
                        if (CosterCenter == "8000" || CosterCenter == "2001" || CosterCenter == "2002" || CosterCenter == "2000" || CosterCenter == "2009")
                        {
                            Brand = "MF";
                        }
                        else if (CosterCenter == "2004" || CosterCenter == "8002")
                        {
                            Brand = "ZCY";
                        }
                        else if (CosterCenter == "2003" || CosterCenter == "8001")
                        {
                            Brand = "MDC";
                        }
                        else if (CosterCenter == "2006" || CosterCenter == "8005")
                        {
                            Brand = "DA";
                        }
                        else if (CosterCenter == "2005" || CosterCenter == "8003")
                        {
                            Brand = "集团";
                        }
                        else if (CosterCenter.Remove(3) == "280")
                        {
                            Brand = "明佳豪";
                        }
                        else
                        {
                            Brand = "集团";
                        }
                        break;
                }
            }
            Result.NAME = Brand;
            return Result;
        }


        /// <summary>
        /// 片区获取成本中心归属方法
        /// </summary>
        /// <param name="CosterCenter"></param>
        /// <returns></returns>
        public ObjectList GetBrandFromCosterCenterNew(string CosterCenter)
        {
            ObjectList Result = new ObjectList();

            string NewSql = "SELECT BRAND FROM FEE_SHOPCOSCENTER_EXTEND where OLDCOSTERCENTER='" + CosterCenter + "'";
            string NewDatabase = DbContext.Database.SqlQuery<string>(NewSql).FirstOrDefault();

            //需要特殊处理的店柜
            if (!string.IsNullOrEmpty(NewDatabase))
            {
                Result.CODE = "1000";
                Result.NAME = NewDatabase;
            }
            else
            {
                string str = CosterCenter.Remove(2);
                int i = 0;
                bool IsNumber = int.TryParse(CosterCenter.Remove(1), out i);
                //整合8位的成本中心
                if (str != "00" && CosterCenter.Length == 8 && IsNumber)
                {
                    CosterCenter = "00" + CosterCenter;
                }
                string Brand = "MF";
                string sql = "select BUKRS as CODE,names as NAME from FEE_SAPDATA where KOSTL='" + CosterCenter + "'";
                var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();
                if (Database != null)
                {
                    Result.CODE = Database.CODE;
                    switch (Database.CODE)
                    {
                        case "2000":
                            Brand = "SU";
                            break;
                        case "1300":
                            Brand = "AM";
                            break;
                        case "1330":
                            Brand = "AM";
                            break;
                        case "1030":
                            if (CosterCenter.Contains("1011"))
                            {
                                Brand = "SU";
                            }
                            else if (CosterCenter.Contains("0011"))
                            {
                                Brand = "MF";
                            }
                            break;
                        case "1020":
                            Brand = "MF";
                            break;
                        case "2100":
                            Brand = "KA";
                            break;
                        //默认处理为1000公司
                        default:
                            //string laststring = CosterCenter.Remove(0, CosterCenter.Length - 1);
                            CosterCenter = CosterCenter.Remove(0, 2);
                            CosterCenter = CosterCenter.Remove(1);
                            if (CosterCenter == "8")
                            {
                                Brand = "MF";
                                //if (laststring == "8")
                                //{
                                //    Brand = "集团";
                                //}
                                //else
                                //{
                                //    Brand = "MF";
                                //}

                            }
                            else if (CosterCenter.ToUpper() == "C")
                            {
                                Brand = "MDC";
                            }
                            else
                            {
                                Brand = "集团";
                            }
                            break;
                    }
                }
                Result.NAME = Brand;
            }
            return Result;
        }


        public ObjectList PublicGetCosterCenter(int IsHeadOffice, string CosterCenter)
        {
            if (IsHeadOffice == 0)
            {
                return GetBrandFromCosterCenterNew(CosterCenter);
            }
            else
            {
                return GetBrandFromCosterCenter(CosterCenter);
            }
        }

        public string AjaxGetName(string NO)
        {
            var str = DbContext.EMPLOYEE.Where(c => c.NO == NO).Select(x => x.NAME).FirstOrDefault();
            return str;
        }


        /// <summary>
        /// 转化为SQl查询格式
        /// </summary>
        /// <param name="StrList"></param>
        /// <returns></returns>
        public string TransSqlData(string StrList)
        {
            string str = String.Empty;
            if (string.IsNullOrEmpty(StrList))
            {
                return str;
            }
            var list = StrList.Split(',').Distinct().ToList();
            list.Remove("");
            foreach (var item in list)
            {
                str += "'" + item + "',";
            }
            str = str.Remove(str.Length - 1);
            return str;
        }


        /// <summary>
        /// 李经理需求
        /// </summary>
        /// <param name="IsHeadOffice">总部/片区</param>
        /// <param name="BillsType">费用类型</param>
        /// <param name="DepartmentCode">部门ID</param>
        /// <param name="obj">成本中心对应的品牌</param>
        /// <param name="Items">费用项</param>
        /// <param name="Spe">特殊选项</param>
        /// <returns></returns>
        public ObjectList PublicDemand(int IsHeadOffice, string BillsType, string DepartmentCode, ObjectList obj, List<FeeBillItemModel> Items, SpecialAttribute Spe, string DepartmentName)
        {
            ObjectList ReturnObj = new ObjectList();
            string Label = ProcessMeaning(BillsType);
            if (BillsType == "HK1")
            {
                ReturnObj.CODE = Label;
                return ReturnObj;
            }

            //是否属于市场部需求
            var Istrue = IsBelongDemand(IsHeadOffice, Spe, obj, ref BillsType);
            if (Istrue)
            {
                Label = "Fee_S";
            }

            bool SkipOffice = false;
            if (IsHeadOffice == 0)
            {
                string sql = "select EXTRAMODEONE as c1,EXTRAMODETWO as c2,BrandPost as c3,name as c4 from FEE_ACCOUNT_DICTIONARY where code='" + BillsType + "' and BRAND='" + obj.NAME + "'";
                var Database = DbContext.Database.SqlQuery<PublicClass>(sql).FirstOrDefault();

                if (!string.IsNullOrEmpty(Database.c3))
                {
                    Label += "B";
                }

                if (Spe.Funds == 1)
                {
                    Label += "1";
                }
                else
                {
                    if (!string.IsNullOrEmpty(Database.c1))
                    {
                        Label += "1";
                    }
                    if (!string.IsNullOrEmpty(Database.c2) && Items.Where(c => c.name == "商场招待费").FirstOrDefault() != null)
                    {
                        Label += "2";
                    }
                }

                for (int i = 0; i < Items.Count; i++)
                {
                    string name = Items[i].name;
                    var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.NAME == name).Select(x => x.IS_MARKET).FirstOrDefault();
                    if (ischeck == 1 && i != Items.Count - 1)
                    {
                        continue;
                    }
                    else if (ischeck == 1 && i == Items.Count - 1)
                    {
                        SkipOffice = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Database.c4 != "无审批岗")
                {
                    ReturnObj.NAME = Database.c4.Replace("审批岗", "");
                }
            }
            else
            {
                string Department = "'%" + DepartmentCode + "%'";
                string brand = "'%" + obj.NAME + "%'";

                string sql = " select VALUE from FEE_PERSON_EXTEND where TYPE='BrandPost' and BRAND like " + brand + " and DEPARTMENTCODE like " + Department + "";
                var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (!string.IsNullOrEmpty(Database))
                {
                    Label += "B";
                }
            }

            if (Items.Count == 1 && Items[0].name == "内部转账")
            {
                SkipOffice = true;
            }

            if (SkipOffice == false)
            {
                Label += "G";
            }
            else
            {
                //进一步验证是否需要强制走总经办(2100)
                string brand = "'%" + obj.NAME + "%'";
                string sql = " select ID from FEE_PERSON_EXTEND where TYPE='KeepManager' and BRAND like " + brand + "";
                var Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (!string.IsNullOrEmpty(Database))
                {
                    Label += "G";
                }

                else
                {
                    if (!DepartmentName.Contains("会所"))
                    {
                        //市场部新需求，橱窗画、橱窗安装费、橱窗费用，营销活动及VIP活动费强制经过总经办
                        List<string> ListStr = new List<string>() { "橱窗画费", "橱窗安装费", "橱窗费用", "营销及VIP回馈费用" };
                        var IsTrue = Items.Where(c => ListStr.Contains(c.name)).ToList().Count > 0;
                        if (IsTrue)
                        {
                            Label += "G";
                        }
                    }
                }
            }

            if (Spe.Cash == 0 && Spe.MarketDebt == 0 && Spe.BankDebt == 0)
            {
                Label += "C";
            }

            ReturnObj.CODE = Label;

            return ReturnObj;
        }


        /// <summary>
        /// 流程系统初始标签定义
        /// </summary>
        /// <param name="BillsType"></param>
        /// <returns></returns>
        private string ProcessMeaning(string BillsType)
        {
            string str = string.Empty;
            switch (BillsType)
            {
                case "HK1":
                    str = "Fee_1";
                    break;
                case "FY1":
                    str = "Fee_H";
                    break;
                case "FY0":
                    str = "Fee_S";
                    break;
                default:
                    str = "Fee_A";
                    break;
            }
            return str;
        }


        private bool IsBelongDemand(int IsHeadOffice, SpecialAttribute Spe, ObjectList obj, ref string BillsType)
        {
            if (IsHeadOffice == 0 && (BillsType == "FY10" || BillsType == "FY15") && obj.CODE != "2100" && (Spe.Cash == 1 || Spe.MarketDebt == 1 || Spe.BankDebt == 1))
            {
                BillsType = "FY0";

                return true;
            }
            else
            {
                return false;
            }
        }


        public string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        public string GetSubbranchBankCode(string name)
        {
            //判断是否为外汇
            int length = GetForeignExchange(name);
            if (length == 3)
            {
                return "3";
            }
            string sql = "select code from t_sy_banklocations_view where name='" + name + "'";
            var dt = new Marisfrolg.Fee.BLL.ReportHelper().GetDataTable(sql, 2);
            if (dt.Rows.Count == 0)
            {
                return "";
            }
            else
            {
                return dt.Rows[0][0].ToString();
            }
        }

        /// <summary>
        /// 判断是否为外汇
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public int GetForeignExchange(string providerName)
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


        public void DeleteInvalidBillNo(string BillNo)
        {
            string sql = "Delete FEE_BILLNO where billno='" + BillNo + "'";
            DbContext.Database.ExecuteSqlCommand(sql);
        }
    }
}
