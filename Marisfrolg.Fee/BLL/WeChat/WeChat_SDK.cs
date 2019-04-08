using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Marisfrolg.Fee.BLL.WeChat
{
    public class WeiXin_SDK_CONFIG_MODEL
    {
        public string AppId { get; set; }
        public string Timestamp { get; set; }
        public string Noncestr { get; set; }
        public string Signature { get; set; }
    }

    public class WeiXin_SDK_CONFIG
    {

        public WeiXin_SDK_CONFIG_MODEL getConfig(string url)
        {
            WeiXin_SDK_CONFIG_MODEL result = new WeiXin_SDK_CONFIG_MODEL();
            result.AppId = System.Configuration.ConfigurationManager.AppSettings["Weixin_AppId"].ToString();;
            result.Timestamp = getTimestamp();
            result.Noncestr = getNoncestr();
            result.Signature = Get_Signature(result.Noncestr, result.Timestamp, url);

            return result;
        }

        /// <summary>
        /// 时间戳
        /// </summary>
        /// <returns></returns>
        public string getTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// 随机串
        /// </summary>
        /// <returns></returns>
        public string getNoncestr()
        {
            Random random = new Random();
            return GetMD5(random.Next(1000).ToString(), "GBK");
        }

        /** 获取大写的MD5签名结果 */
        public string GetMD5(string encypStr, string charset)
        {
            string retStr;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用GB2312编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch (Exception ex)
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        public string Getjsapi_ticket()
        {
            EnterpriceService.MfWeiXinEmpServiceSoapClient c = new EnterpriceService.MfWeiXinEmpServiceSoapClient();
            
            string strjson = c.Getticket("Marisfrolg");
            return strjson;
        }

        public string Get_Signature(string nonceStr, string timespanstr,string url)
        {
           
            string jsapi_ticket = Getjsapi_ticket();
            //string url = "http://wx.marisfrolg.com";

            string str = "jsapi_ticket=" + jsapi_ticket + "&noncestr=" + nonceStr +
                "&timestamp=" + timespanstr + "&url=" + url;
            string singature = SHA1Util.getSha1(str);
            return singature;
        }
    }


    class SHA1Util
    {
        public static String getSha1(String str)
        {
            //建立SHA1对象
            SHA1 sha = new SHA1CryptoServiceProvider();
            //将mystr转换成byte[] 
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] dataToHash = enc.GetBytes(str);
            //Hash运算
            byte[] dataHashed = sha.ComputeHash(dataToHash);
            //将运算结果转换成string
            string hash = BitConverter.ToString(dataHashed).Replace("-", "");
            return hash;
        }
    }

    public class CommonJsonModelAnalyzer
    {
        protected string _GetKey(string rawjson)
        {
            if (string.IsNullOrEmpty(rawjson))
                return rawjson;
            rawjson = rawjson.Trim();
            string[] jsons = rawjson.Split(new char[] { ':' });
            if (jsons.Length < 2)
                return rawjson;
            return jsons[0].Replace("\"", "").Trim();
        }
        protected string _GetValue(string rawjson)
        {
            if (string.IsNullOrEmpty(rawjson))
                return rawjson;
            rawjson = rawjson.Trim();
            string[] jsons = rawjson.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (jsons.Length < 2)
                return rawjson;
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < jsons.Length; i++)
            {
                builder.Append(jsons[i]);
                builder.Append(":");
            }
            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);
            string value = builder.ToString();
            if (value.StartsWith("\""))
                value = value.Substring(1);
            if (value.EndsWith("\""))
                value = value.Substring(0, value.Length - 1);
            return value;
        }
        protected List<string> _GetCollection(string rawjson)
        {
            //[{},{}]
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(rawjson))
                return list;
            rawjson = rawjson.Trim();
            StringBuilder builder = new StringBuilder();
            int nestlevel = -1;
            int mnestlevel = -1;
            for (int i = 0; i < rawjson.Length; i++)
            {
                if (i == 0)
                    continue;
                else if (i == rawjson.Length - 1)
                    continue;
                char jsonchar = rawjson[i];
                if (jsonchar == '{')
                {
                    nestlevel++;
                }
                if (jsonchar == '}')
                {
                    nestlevel--;
                }
                if (jsonchar == '[')
                {
                    mnestlevel++;
                }
                if (jsonchar == ']')
                {
                    mnestlevel--;
                }
                if (jsonchar == ',' && nestlevel == -1 && mnestlevel == -1)
                {
                    list.Add(builder.ToString());
                    builder = new StringBuilder();
                }
                else
                {
                    builder.Append(jsonchar);
                }
            }
            if (builder.Length > 0)
                list.Add(builder.ToString());
            return list;
        }
    }

    public class CommonJsonModel : CommonJsonModelAnalyzer
    {
        private string rawjson;
        private bool isValue = false;
        private bool isModel = false;
        private bool isCollection = false;
        public CommonJsonModel(string rawjson)
        {
            this.rawjson = rawjson;
            if (string.IsNullOrEmpty(rawjson))
                throw new Exception("missing rawjson");
            rawjson = rawjson.Trim();
            if (rawjson.StartsWith("{"))
            {
                isModel = true;
            }
            else if (rawjson.StartsWith("["))
            {
                isCollection = true;
            }
            else
            {
                isValue = true;
            }
        }
        public string Rawjson
        {
            get { return rawjson; }
        }
        public bool IsValue()
        {
            return isValue;
        }
        public bool IsValue(string key)
        {
            if (!isModel)
                return false;
            if (string.IsNullOrEmpty(key))
                return false;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                {
                    CommonJsonModel submodel = new CommonJsonModel(model.Value);
                    return submodel.IsValue();
                }
            }
            return false;
        }
        public bool IsModel()
        {
            return isModel;
        }
        public bool IsModel(string key)
        {
            if (!isModel)
                return false;
            if (string.IsNullOrEmpty(key))
                return false;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                {
                    CommonJsonModel submodel = new CommonJsonModel(model.Value);
                    return submodel.IsModel();
                }
            }
            return false;
        }
        public bool IsCollection()
        {
            return isCollection;
        }
        public bool IsCollection(string key)
        {
            if (!isModel)
                return false;
            if (string.IsNullOrEmpty(key))
                return false;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                {
                    CommonJsonModel submodel = new CommonJsonModel(model.Value);
                    return submodel.IsCollection();
                }
            }
            return false;
        }

        /// <summary>
        /// 当模型是对象，返回拥有的key
        /// </summary>
        /// <returns></returns>
        public List<string> GetKeys()
        {
            if (!isModel)
                return null;
            List<string> list = new List<string>();
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                string key = new CommonJsonModel(subjson).Key;
                if (!string.IsNullOrEmpty(key))
                    list.Add(key);
            }
            return list;
        }
        /// <summary>
        /// 当模型是对象，key对应是值，则返回key对应的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            if (!isModel)
                return null;
            if (string.IsNullOrEmpty(key))
                return null;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                    return model.Value;
            }
            return null;
        }
        /// <summary>
        /// 模型是对象，key对应是对象，返回key对应的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CommonJsonModel GetModel(string key)
        {
            if (!isModel)
                return null;
            if (string.IsNullOrEmpty(key))
                return null;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                {
                    CommonJsonModel submodel = new CommonJsonModel(model.Value);
                    if (!submodel.IsModel())
                        return null;
                    else
                        return submodel;
                }
            }
            return null;
        }
        /// <summary>
        /// 模型是对象，key对应是集合，返回集合
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CommonJsonModel GetCollection(string key)
        {
            if (!isModel)
                return null;
            if (string.IsNullOrEmpty(key))
                return null;
            foreach (string subjson in base._GetCollection(this.rawjson))
            {
                CommonJsonModel model = new CommonJsonModel(subjson);
                if (!model.IsValue())
                    continue;
                if (model.Key == key)
                {
                    CommonJsonModel submodel = new CommonJsonModel(model.Value);
                    if (!submodel.IsCollection())
                        return null;
                    else
                        return submodel;
                }
            }
            return null;
        }
        /// <summary>
        /// 模型是集合，返回自身
        /// </summary>
        /// <returns></returns>
        public List<CommonJsonModel> GetCollection()
        {
            List<CommonJsonModel> list = new List<CommonJsonModel>();
            if (IsValue())
                return list;
            foreach (string subjson in base._GetCollection(rawjson))
            {
                list.Add(new CommonJsonModel(subjson));
            }
            return list;
        }


        /// <summary>
        /// 当模型是值对象，返回key
        /// </summary>
        private string Key
        {
            get
            {
                if (IsValue())
                    return base._GetKey(rawjson);
                return null;
            }
        }
        /// <summary>
        /// 当模型是值对象，返回value
        /// </summary>
        private string Value
        {
            get
            {
                if (!IsValue())
                    return null;
                return base._GetValue(rawjson);
            }
        }
    }
}