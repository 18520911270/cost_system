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
    public class AccountController : WebController
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, bool IsRemember)
        {
            if (username.IsNullOrEmpty() || password.IsNullOrEmpty())
                return this.FailedMsg("用户名/密码不能为空");

            username = username.Trim();

            const string moduleName = "系统登录";
            string ip = WebHelper.GetUserIP();

            Sys_User user;
            string msg;


            IAccountAppService _IAccountAppService = new AccountAppService();
            //IGetFeeData _IGetFeeData = new GetFeeData();

            if (!_IAccountAppService.CheckLogin(username, password, out user, out msg))
            {
                Logger.Write("Login", moduleName, "用户名" + username + msg);

                return this.FailedMsg(msg);
            }

            AdminSession session = new AdminSession();
            session.UserId = user.Id;
            session.UserName = user.UserName;
            session.RealName = user.RealName;
            session.LoginIP = ip;
            session.LoginTime = DateTime.Now;
            session.headIcon = user.HeadIcon;
            session.Description = user.Description;
            session.WeChat = user.WeChat;
            session.IsAdmin = user.UserName.ToLower() == AppConsts.AdminUserName;
            session.IsRemember = IsRemember;
            //session.Habit = _IGetFeeData.LookUserHabit(this.CurrentSession.UserName);
            this.CurrentSession = session;


            IGetFeeData GetData = new GetFeeData();

            //int result = GetData.InserIntoBankSystem("");

            Logger.Write("Login", moduleName, "用户名" + username + msg);

            return this.SuccessMsg(msg);
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetPermissionJson()
        {
            return this.SuccessData(this.CurrentSession);
        }
    }
}
