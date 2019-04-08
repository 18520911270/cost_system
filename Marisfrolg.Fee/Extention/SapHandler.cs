using Marisfrolg.Fee.Models;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee
{
    public class SapHandler
    {
        //提交到810或者310
        private static string RFC
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["RFC"].ToString();
            }
        }

        //提交到800的预付原料
        private static string RFG
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["RFG"].ToString();
            }
        }

        /// <summary>
        /// 创建费用单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SapRetrun CreateAccounting(SapUpload model, string config)
        {
            string strConfig = String.Empty;
            if (config == "RFC")
            {
                strConfig = RFC;
            }
            else if (config == "RFG")
            {
                 strConfig = RFG;
            }
            RfcDestination conn = SapConnection.GetConnection(strConfig);
            RETAIL retail = new RETAIL(conn);//零售系统

            var obj = retail.CREATE_BAPI_ACC_DOCUMENT_POST(model);//执行创建凭证的原始方法

            return obj;

        }
    }
}