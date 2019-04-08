using Marisfrolg.Business;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Marisfrolg.Fee.Controllers
{

    /// <summary>
    /// 登录检查
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LoginAuthorize : AuthorizeAttribute
    {
        public const string RecoverBill = "~/RecoverBill/InvoiceInfo";  //开票资料跳过验证
        public const string TaxInfo = "~/RecoverBill/TaxInfo";  //税收指南

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string CurrentURL = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            if (CurrentURL.Equals(RecoverBill) || CurrentURL.Equals(TaxInfo))
            {
                return true;
            }
            if (Marisfrolg.Public.Common.GetEmployeeInfo() == null)
            {
                return false;
            }
            SetCookie("EmployeeInfo"); //跟新cookie时间
            return true;
        }


        /// <summary>
        /// 设置Cookie过期时间（1小时过期）
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        private static void SetCookie(string cookiename)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            cookie.Expires = DateTime.Now.AddHours(3);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }

    [LoginAuthorize]
    public class SecurityController : BaseController
    {
        public SecurityController()
            : base()
        {




        }


    }
}
