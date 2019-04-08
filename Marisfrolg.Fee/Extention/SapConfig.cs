using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee
{
    public class SapConfig : System.Configuration.IConfigurationSectionHandler
    {
        /// <summary>
        /// SAP主机IP
        /// </summary>
        public string AppServerHost
        {
            get;
            private set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string User
        {
            get;
            private set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Client
        /// </summary>
        public string Client
        {
            get;
            private set;
        }

        public string SystemID
        {
            get;
            private set;
        }

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            SapConfig sapConfig = new SapConfig();
            var appServerHost = section.SelectSingleNode("AppServerHost");
            if (appServerHost != null && appServerHost.Attributes != null)
            {
                var attribute = appServerHost.Attributes["Value"];
                if (attribute != null)
                    sapConfig.AppServerHost = attribute.Value;
            }

            var user = section.SelectSingleNode("User");
            if (user != null && user.Attributes != null)
            {
                var attribute = user.Attributes["Value"];
                if (attribute != null)
                    sapConfig.User = attribute.Value;
            }

            var password = section.SelectSingleNode("Password");
            if (password != null && password.Attributes != null)
            {
                var attribute = password.Attributes["Value"];
                if (attribute != null)
                    sapConfig.Password = attribute.Value;
            }

            var client = section.SelectSingleNode("Client");
            if (client != null && client.Attributes != null)
            {
                var attribute = client.Attributes["Value"];
                if (attribute != null)
                    sapConfig.Client = attribute.Value;
            }

            var systemID = section.SelectSingleNode("SystemID");
            if (systemID != null && systemID.Attributes != null)
            {
                var attribute = systemID.Attributes["Value"];
                if (attribute != null)
                    sapConfig.SystemID = attribute.Value;
            }

            return sapConfig;
        }
    }
}