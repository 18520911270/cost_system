using MultiBank.DAL;
using MultiBank.Extention;
using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public class GetSystemData : IGetSystemData
    {
        public List<Sys_User> GetSysUser()
        {
            string sql = string.Format("SELECT ID,USERNAME,PASSWORD, REALNAME,HEADICON,GENDER,BIRTHDAY,MOBILEPHONE,EMAIL,WECHAT,DEPARTMENTID, ROLEID,DUTYID,DESCRIPTION,CREATIONTIME,CREATEUSERID,ISENABLED,ISDELETED FROM BANK_USER");

            OracleHelper _oraDal = new OracleHelper();

            var dt = _oraDal.ExecuteQuery(sql);

            var list = _oraDal.DataTableToList<Sys_User>(dt).ToList();

            return list;
        }


        public Sys_User GetEmployeeNoInfo(string No)
        {
            Sys_User user = new Sys_User();

            string sql = string.Format("select name,depid from employee where no='{0}'  And available=1 and leave=0", No);

            OracleHelper _oraDal = new OracleHelper();

            var dt = _oraDal.ExecuteQuery(sql);

            //工号不存在或者离职
            if (dt.Rows.Count <= 0)
            {
                return null;
            }

            user.UserName = No;
            user.RealName = dt.Rows[0][0].ToString().Split('-').ToList()[0];
            user.DepartmentId = Convert.ToDecimal(dt.Rows[0][1]);

            try
            {
                //远程调用微信接口获取绑定企业微信号信息
                string url = "http://192.168.2.14/CompanyWXPlat/QyWeiXin/GetuseridInfo";

                //identify, string userid  Marisfrolg
                string postDataStr = string.Format("ValidationCode={0}&UserID={1}", "ZYAEGPXTNPI7RLPM", No);

                var result = MultiBank.Extention.WebHelper.HttpGet(url, postDataStr);

                var weixin = JsonHelper.Deserialize<WeiXinUser>(result);

                if (weixin.errcode == 0)
                {
                    user.MobilePhone = weixin.mobile;
                    user.Gender = weixin.gender;
                    user.Email = weixin.email;
                    user.HeadIcon = weixin.avatar;
                }
            }
            catch (Exception)
            {

            }

            return user;
        }


        public bool InsertPersonInfo(Sys_User User, string CreatorId)
        {
            OracleHelper _oraDal = new OracleHelper();

            string LookSql = string.Format("select  USERNAME from BANK_USER where USERNAME='{0}'", User.UserName);

            var LookDt = _oraDal.ExecuteQuery(LookSql);
            if (LookDt.Rows.Count > 0)
            {
                return false;
            }

            string sql = string.Format("Insert into BANK_USER (USERNAME,PASSWORD,REALNAME,HEADICON,MOBILEPHONE,WECHAT,DEPARTMENTID,DESCRIPTION,CREATIONTIME,CREATEUSERID,GENDER) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8},'{9}','{10}')", User.UserName, DESEncrypt.Encrypt("123456"), User.RealName, User.HeadIcon, User.MobilePhone, User.WeChat, User.DepartmentId, User.Description, "SYSDATE", CreatorId, User.Gender);

            int result = _oraDal.ExecuteSQL(sql);
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}