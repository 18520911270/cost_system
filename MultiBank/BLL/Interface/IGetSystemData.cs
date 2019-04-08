using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public interface IGetSystemData : IDependency
    {
        List<Sys_User> GetSysUser();

        /// <summary>
        /// 根据工号返回一系列信息（非自己数据库）
        /// </summary>
        /// <param name="No"></param>
        /// <returns></returns>
        Sys_User GetEmployeeNoInfo(string No);


        bool InsertPersonInfo(Sys_User User,string CreatorId);
    }
}