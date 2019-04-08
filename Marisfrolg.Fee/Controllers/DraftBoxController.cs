using Marisfrolg.Business;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marisfrolg.Fee.Controllers
{
    public class DraftBoxController : BaseController
    {

        [LoginAuthorize]
        public ActionResult Index()
        {

            return View();


        }

        public string GetDraftBoxByParameter(int type)
        {
            Marisfrolg.Fee.BLL.DraftBox DraftBox = new BLL.DraftBox();
            List<Models.FeeBillModel> re = null;
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            switch (type)
            {
                case 1: //今天
                    re = DraftBox.GetDraftBoxByParameter(1, employee.EmployeeNo);
                    break;
                case 2://本周
                    re = DraftBox.GetDraftBoxByParameter(2, employee.EmployeeNo);
                    break;
                case 3://本月
                    re = DraftBox.GetDraftBoxByParameter(3, employee.EmployeeNo);
                    break;
                case 4://上月
                    re = DraftBox.GetDraftBoxByParameter(4, employee.EmployeeNo);
                    break;
                default:
                    break;
            }
            var query = new
            {
                Count = 0,
                TotalMoney = "0.00"
            };
            if (re == null)
            {
                return Public.JsonSerializeHelper.SerializeToJson(query);
            }

            var result = new
            {
                Count = re.Count,
                TotalMoney = re.Sum(c => c.TotalMoney).ToString("0.00"),
            };

            return Public.JsonSerializeHelper.SerializeToJson(result);
        }

        public string GetDraftBoxListByParameter(int type)
        {
            Marisfrolg.Fee.BLL.DraftBox DraftBox = new BLL.DraftBox();
            List<Models.FeeBillModel> re = null;
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            switch (type)
            {
                case 1: //今天
                    re = DraftBox.GetDraftBoxByParameter(1, employee.EmployeeNo);
                    break;
                case 2://本周
                    re = DraftBox.GetDraftBoxByParameter(2, employee.EmployeeNo);
                    break;
                case 3://本月
                    re = DraftBox.GetDraftBoxByParameter(3, employee.EmployeeNo);
                    break;
                case 4://上月
                    re = DraftBox.GetDraftBoxByParameter(4, employee.EmployeeNo);
                    break;
                default:
                    break;
            }

            return Public.JsonSerializeHelper.SerializeToJson(re);
        }


    }
}
