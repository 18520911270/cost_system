using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public interface IAccountAppService : IDependency
    {
        bool CheckLogin(string userName, string password, out Sys_User user, out string msg);

        bool ChangePassword(string userName, string oldPassword, string newPassword, out string msg);

        /// <summary>
        /// 根据工号或者工号和密码确定用户信息
        /// </summary>
        /// <param name="no"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Sys_User GetUserInfo(string no, string password = "");

    }
}