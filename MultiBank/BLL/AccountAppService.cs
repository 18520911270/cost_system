using MultiBank.DAL;
using MultiBank.Extention;
using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public class AccountAppService : IAccountAppService
    {
        public bool CheckLogin(string userName, string password, out Sys_User user, out string msg)
        {
            user = null;
            msg = null;

            user = GetUserInfo(userName, password);

            if (user == null)
            {
                msg = "账户或者密码错误";
                return false;
            }

            if (user.IsEnabled == 0)
            {
                msg = "账户被系统锁定,请联系管理员";
                return false;
            }

            return true;
        }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public Sys_User GetUserInfo(string no, string password = "")
        {

            OracleHelper _oraDal = new OracleHelper();

            string sql = string.Format("SELECT ID,USERNAME,PASSWORD, REALNAME,HEADICON,GENDER,BIRTHDAY,MOBILEPHONE,EMAIL,WECHAT,DEPARTMENTID, ROLEID,DUTYID,DESCRIPTION,CREATIONTIME,CREATEUSERID,ISENABLED,ISDELETED FROM BANK_USER WHERE USERNAME='{0}' AND ISDELETED=0", no);

            if (!string.IsNullOrEmpty(password))
            {
                sql += string.Format("  And PASSWORD='{0}'", DESEncrypt.Encrypt(password));
            }

            var dt = _oraDal.ExecuteQuery(sql);

            var list = _oraDal.DataTableToList<Sys_User>(dt).ToList();

            var model = list.FirstOrDefault();

            return model;
        }


        public bool ChangePassword(string userName, string oldPassword, string newPassword, out string msg)
        {
            msg = null;

            var user = GetUserInfo(userName);

            if (user == null)
            {
                msg = "系统异常。。";
                return false;
            }

            if (!DESEncrypt.Encrypt(oldPassword).Equals(user.PASSWORD))
            {
                msg = "原始密码不正确。。";
                return false;
            }

            string sql = string.Format(" update BANK_USER set PASSWORD='{0}' where USERNAME='{1}'", DESEncrypt.Encrypt(newPassword), userName);

            OracleHelper _oraDal = new OracleHelper();

            int result = _oraDal.ExecuteSQL(sql);

            if (result > 0)
            {
                msg = "编辑成功";
                return true;
            }
            else
            {
                msg = "编辑失败";
                return false;
            }
        }
    }
}