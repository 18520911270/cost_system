using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Extention
{
    public class GetSapData
    {
        /// <summary>
        /// RFC 库
        /// </summary>
        private RfcRepository RFC = null;
        /// <summary>
        /// RFC 目的地
        /// </summary>
        private RfcDestination Dest = null;

        private string Client = string.Empty;
        public GetSapData()
        {
            #region 获取SAP连接
            try
            {
                Dest = SAPConnection.GetSAPConnection();
                RFC = Dest.Repository;
                Client = System.Configuration.ConfigurationManager.AppSettings["Client"];
            }
            catch (Exception ex)
            {
                if (ex.Message != "Destination configuration already initialized")
                {

                }
                else
                {
                    Dest = RfcDestinationManager.GetDestination("SAP");
                    RFC = Dest.Repository;
                }
            }
            #endregion
        }
    }
}