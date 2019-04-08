using MultiBank.Extention;
using MultiBank.Models;
using System;
using System.Web.Mvc;

namespace MultiBank.Controllers
{
    public class WebController : BaseController
    {

        AdminSession _session;
        public AdminSession CurrentSession
        {
            get
            {
                if (this._session != null)
                    return this._session;

                AdminSession session = WebUtils.GetCurrentSession();
                this._session = session;
                return session;
            }
            set
            {
                AdminSession session = value;
                WebUtils.SetSession(session);
                this._session = session;
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
    }
}
