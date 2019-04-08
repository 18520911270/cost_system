using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marisfrolg.Fee.Controllers
{
    /// <summary>
    /// 提供消息提醒的类
    /// </summary>
    public class MessageController : Controller
    {
        //
        // GET: /Message/

        public ActionResult Index()
        {
            return View();
        }
    }
}
