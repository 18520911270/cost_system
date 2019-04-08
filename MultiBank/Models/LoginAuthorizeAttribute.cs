using MultiBank.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultiBank.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SkipLoginAuthorizeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class LoginAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public LoginAuthorizeAttribute()
        {
        }
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.IsChildAction || this.SkipAuthorize(filterContext.ActionDescriptor))
            {
                return;
            }

            AdminSession session = null;

            WebController webController = filterContext.Controller as WebController;
            if (webController != null)
            {
                session = webController.CurrentSession;
            }
            else
            {
                session = MultiBank.Extention.WebUtils.GetCurrentSession();
            }

            if (session != null)
                return;

            HttpRequestBase httpRequest = filterContext.HttpContext.Request;
            if (httpRequest.IsAjaxRequest())
            {
                Result result = Result.CreateResult(ResultStatus.NotLogin, "未登录或登录超时，请重新登录");
                string json = JsonHelper.Serialize(result);
                ContentResult contentResult = new ContentResult() { Content = json };
                filterContext.Result = contentResult;
                return;
            }
            else
            {
                string url = System.Web.Mvc.UrlHelper.GenerateContentUrl("~/Account/Login", filterContext.HttpContext);
                //url = string.Concat(url, "?returnUrl=", httpRequest.RawUrl);

                RedirectResult redirectResult = new RedirectResult(url);
                filterContext.Result = redirectResult;
                return;
            }
        }

        protected virtual bool SkipAuthorize(ActionDescriptor actionDescriptor)
        {
            var skipAttributeCount = actionDescriptor.GetCustomAttributes(typeof(SkipLoginAuthorizeAttribute), false).Count();
            skipAttributeCount += actionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(SkipLoginAuthorizeAttribute), false).Count();

            if (skipAttributeCount > 0)
            {
                return true;
            }

            return false;
        }
    }

}