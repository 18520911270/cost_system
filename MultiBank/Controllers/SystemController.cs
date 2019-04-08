using MultiBank.BLL;
using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultiBank.Controllers
{
    [LoginAuthorizeAttribute]
    public class SystemController : WebController
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetSysUsers()
        {
            IGetSystemData _IGetSysData = new GetSystemData();
            var UserList = _IGetSysData.GetSysUser();
            var dt = ConverttoDataTable(UserList);
            return this.SuccessData(dt);
        }



        DataTable ConverttoDataTable(List<Sys_User> User)
        {
            DataTable dt = new DataTable();

            if (User != null && User.Count > 0)
            {
                dt.Columns.Add("全选");
                dt.Columns.Add("用户名");
                dt.Columns.Add("真实姓名");
                dt.Columns.Add("描述");
                dt.Columns.Add("创建时间");
                dt.Columns.Add("是否禁用");
                dt.Columns.Add("是否删除");

                foreach (var item in User)
                {
                    DataRow row = dt.NewRow();
                    row["用户名"] = item.UserName;
                    row["真实姓名"] = item.RealName;
                    row["描述"] = item.Description;
                    row["创建时间"] = item.CreationTime.ToString("yyyy-MM-dd HH:mm");
                    row["是否禁用"] = item.IsEnabled == 0 ? "是" : "否";
                    row["是否删除"] = item.IsDeleted == 1 ? "是" : "否";
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }


        public ActionResult AddPerson(string No)
        {
            IGetSystemData _IGetSysData = new GetSystemData();
            var user = _IGetSysData.GetEmployeeNoInfo(No);

            if (user == null)
            {
                return this.FailedMsg("工号不存在");
            }

            bool IsSuccess = _IGetSysData.InsertPersonInfo(user, this.CurrentSession.UserId);

            return this.SuccessData();
        }
    }
}
