using Marisfrolg.Public;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee
{
    public class SapConnection
    {
        static Dictionary<string,RfcDestination> _RFCs = new Dictionary<string,RfcDestination>();
       
        public SapConnection()
        {

        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public static RfcDestination GetConnection(string DestinationName)
        {
            RfcDestination exists = null;
            //如果该连接已缓存，使用旧的

            if (_RFCs != null)
            {
                if (_RFCs.ContainsKey(DestinationName))
                {
                    exists = _RFCs[DestinationName];
                }
            }

            if (exists!=null)
            {
                return exists;
            }

            IDestinationConfiguration ID = new SaprouterConfig();
            try
            {
                RfcDestinationManager.RegisterDestinationConfiguration(ID);
            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("创建SAP连接失败：" + ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name, "**********************");
            }

            exists = RfcDestinationManager.GetDestination(DestinationName);
            try
            {
                exists.Repository.ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
                (ID as SaprouterConfig).RemoveDestination(DestinationName);
            }

            try
            {
                RfcDestinationManager.UnregisterDestinationConfiguration(ID);
            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("注销SAP连接池失败：" + ex.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name, "**********************");
            }

            //缓存本次新建连接
            _RFCs.Add(DestinationName, exists);
            return exists;

        }

        ~SapConnection()
        {
            /*
             注释说明：[该处请不要在析构函数里面这么写]
             1.  析构函数被执行就意味着GC已经在进行回收了
             2.  另外GC.Collect是对所有代的回收，该处析构可能位于 0 1 2三代之间
             3.  该对象创建调用比较频繁，如果在析构中经常GC，会导致GC不断被调用，GC执行期间，会导致其他所有工作线程的挂起,GC具备线程较高优先级
             4.  _RFC = null 这句也没啥意义，该处并不会导致什么行为，原本指向的地址对象依旧存在[并不会因为设置NULL就会对象回收等]，而且析构标志对象声明周期的结束。除非重写operator=，并附带相关逻辑，类似std::auto_ptr<int>该类实现
             */
            //_RFC = null;
            //GC.Collect();
        }

        //static SapConnection _SapConnection;
        //public static SapConnection GetConnection(string SYSID)
        //{
        //    if (_SapConnection == null)
        //    {
        //        _SapConnection = new SapConnection(SYSID);
        //    }
        //    return _SapConnection;
        //}


    }

    //登陆SAP前的准备工作
    public class SaprouterConfig : IDestinationConfiguration
    {
        private Dictionary<string, RfcConfigParameters> AvailableDestinations;


        public SaprouterConfig()
        {
            AvailableDestinations = new Dictionary<string, RfcConfigParameters>();
        }

        public RfcConfigParameters GetParameters(String destinationName)
        {
            SapConfig sapConfig = ConfigurationManager.GetSection(destinationName) as SapConfig;
            if (sapConfig == null)
                return null;

            RfcConfigParameters parms = new RfcConfigParameters();
            //if (!Properties.Settings.Default.INNER)
            //{
            //    parms.Add(RfcConfigParameters.SAPRouter, "/H/210.75.9.162/H/");
            //}
            parms.Add(RfcConfigParameters.AppServerHost, sapConfig.AppServerHost);   //SAP主机IP
            parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
            parms.Add(RfcConfigParameters.User, sapConfig.User);  //用户名
            parms.Add(RfcConfigParameters.Password, sapConfig.Password);  //密码
            parms.Add(RfcConfigParameters.Client, sapConfig.Client);  // Client
            parms.Add(RfcConfigParameters.Language, "ZH");  //登陆语言
            parms.Add(RfcConfigParameters.PoolSize, "10");
            parms.Add(RfcConfigParameters.MaxPoolSize, "100");
            parms.Add(RfcConfigParameters.SystemID, sapConfig.SystemID);
            parms.Add(RfcConfigParameters.IdleTimeout, "60");
            AvailableDestinations.Add(destinationName, parms);
            return parms;
        }

        public bool ChangeEventsSupported()
        {
            return true;
        }

        //public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        //removes the destination that is known under the given name
        public void RemoveDestination(string name)
        {
            if (name != null && AvailableDestinations.Remove(name))
            {
                changeHandler(name, new RfcConfigurationEventArgs(RfcConfigParameters.EventType.DELETED));
            }
        }

        //allows adding or modifying a destination for a specific application server
        public void AddOrEditDestination(string name, int poolSize, string user, string password, string language, string client, string applicationServer, string systemNumber)
        {
            //in productive code the given parameters should be checked for validity, e.g. that name is not null
            //as this is not relevant for the example, we omit it here
            RfcConfigParameters parameters = new RfcConfigParameters();
            parameters[RfcConfigParameters.Name] = name;
            parameters[RfcConfigParameters.MaxPoolSize] = Convert.ToString(poolSize);
            parameters[RfcConfigParameters.IdleTimeout] = Convert.ToString(1); // we keep connections for 10 minutes
            parameters[RfcConfigParameters.User] = user;
            parameters[RfcConfigParameters.Password] = password;
            parameters[RfcConfigParameters.Client] = client;
            parameters[RfcConfigParameters.Language] = language;
            parameters[RfcConfigParameters.AppServerHost] = applicationServer;
            parameters[RfcConfigParameters.SystemNumber] = systemNumber;
            RfcConfigParameters existingConfiguration;

            //if a destination of that name existed before, we need to fire a change event
            if (AvailableDestinations.TryGetValue(name, out existingConfiguration))
            {
                AvailableDestinations[name] = parameters;
                RfcConfigurationEventArgs eventArgs = new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, parameters);
                changeHandler(name, eventArgs);
            }
            else
            {
                AvailableDestinations[name] = parameters;
            }


        }

        private RfcDestinationManager.ConfigurationChangeHandler changeHandler;

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged
        {
            add
            {
                changeHandler = value;
            }
            remove
            {
                //do nothing
            }
        }
    }

}