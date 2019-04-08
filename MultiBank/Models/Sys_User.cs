using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.Models
{
    public class Sys_User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string HeadIcon { get; set; }
        public string PASSWORD { get; set; }
        public decimal Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string WeChat { get; set; }
        public decimal? DepartmentId { get; set; }
        public decimal? RoleId { get; set; }
        public decimal? DutyId { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public decimal? CreateUserId { get; set; }
        public decimal IsEnabled { get; set; }
        public decimal IsDeleted { get; set; }
    }

    //public enum Gender
    //{
    //    Unknown = 0,
    //    Man = 1,
    //    Woman = 2,
    //}


    public class WeiXinUser
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
        public string userid { get; set; }
        public string name { get; set; }
        public List<decimal> department { get; set; }
        public string position { get; set; }
        public string mobile { get; set; }
        public decimal gender { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public decimal status { get; set; }
        public decimal isleader { get; set; }
    }

    public class extattr
    {
        public List<string> attrs { get; set; }
        public string english_name { get; set; }
        public string telephone { get; set; }
        public decimal enable { get; set; }
        public decimal hide_mobile { get; set; }
        public List<decimal> order { get; set; }
    }
}