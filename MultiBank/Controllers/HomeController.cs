using MultiBank.BLL;
using MultiBank.Extention;
using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultiBank.Controllers
{
    [LoginAuthorizeAttribute]
    public class HomeController : WebController
    {
        public ActionResult Index()
        {
            return View(this.CurrentSession);
        }


        public ActionResult LogOut()
        {
            WebUtils.AbandonSession();
            return this.SuccessData();
        }



        public ActionResult EditPassword(string oldPassword, string newPassword)
        {
            IAccountAppService _IAccountAppService = new AccountAppService();

            string msg = string.Empty;

            bool IsSuccess = _IAccountAppService.ChangePassword(this.CurrentSession.UserName, oldPassword, newPassword, out msg);

            if (IsSuccess)
            {
                return this.SuccessMsg(msg);
            }
            else
            {
                return this.FailedMsg(msg);
            }
        }
    }
}
