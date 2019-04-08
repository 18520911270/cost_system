
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.Models
{
    public class AdminSession : AceSession<string>
    {
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string LoginIP { get; set; }
        public DateTime LoginTime { get; set; }
        public string headIcon { get; set; }
        public string Description { get; set; }
        public string WeChat { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsRemember { get; set; }
        public UserHabit Habit { get; set; }
    }

    public class AceSession<T>
    {
        public T UserId { get; set; }
    }


    public class UserHabit
    {
        public string PayCompanyCode { get; set; }
        public string City { get; set; }
        public string BillType { get; set; }
        public string TradeType { get; set; }
    }
}