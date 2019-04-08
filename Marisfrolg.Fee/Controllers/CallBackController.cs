using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tencent;

namespace Marisfrolg.Fee.Controllers
{
    public class CallBackController : Controller
    {
        //
        // GET: /Home/
        WXBizMsgCrypt wxcpt;


        #region 微信企业号

        string sToken = "Marisfrolg";  //Token
        string sCorpID = "wxba047e6526d4545e";  //企业号ID
        string sEncodingAESKey = "h5f4q8lH72Qelx3SOmEo6pdilMjWzciqDucNfjbGNVV";   //  应用秘钥  8


        #endregion 


        public ActionResult Index()
        {
            Logger.Write("进入回调页面:CallBack/Index");
            if (Request.HttpMethod.ToLower() == "post")
            {
                wxcpt = new WXBizMsgCrypt(sToken, sEncodingAESKey, sCorpID);
                string sReqMsgSig = Request.QueryString["msg_signature"] == null ? "" : Request.QueryString["msg_signature"].ToString(); //微信加密签名
                string sReqTimeStamp = Request.QueryString["timestamp"] == null ? "" : Request.QueryString["timestamp"].ToString();  //时间戳
                string sReqNonce = Request.QueryString["nonce"] == null ? "" : Request.QueryString["nonce"].ToString();  //随机数
                string sReechostr = Request.QueryString["echostr"] == null ? "" : Request.QueryString["echostr"].ToString();  //随机字符串
                
                string copyRight = "{msg_signature:'" + sReqMsgSig
                                           + "',timestamp:'" + sReqTimeStamp
                                           + "',nonce:'" + sReqNonce
                                           + "',echostr:'" + sReechostr
                                           + "'}";
                Marisfrolg.Public.CookieHelper.RemoveCookie("copyRight");
                Marisfrolg.Public.CookieHelper.SetCookie("copyRight", copyRight);
               
                Logger.Write("msg_signature:" + sReqMsgSig + "|timestamp:" + sReqTimeStamp + "|nonce:" + sReqNonce + "|echostr:" + sReechostr);



            }
            else
            {
                string echostr = Valid();
                ViewBag.Title = echostr;
            }


            return View();


        }


        /// <summary>
        /// 验证URL
        /// </summary>
        /// <returns></returns>
        private string Valid()
        {
            if (wxcpt==null)
            {
                wxcpt = new WXBizMsgCrypt(sToken, sEncodingAESKey, sCorpID);
            }
            

            string sVerifyMsgSig = Request.QueryString["msg_signature"] == null ? "" : Request.QueryString["msg_signature"].ToString();
            string sVerifyTimeStamp = Request.QueryString["timestamp"] == null ? "" : Request.QueryString["timestamp"].ToString();
            string sVerifyNonce = Request.QueryString["nonce"] == null ? "" : Request.QueryString["nonce"].ToString();
            string sVerifyEchoStr = Request.QueryString["echostr"] == null ? "" : Request.QueryString["echostr"].ToString();
            int ret = 0;
            string sEchoStr = "";
            ret = wxcpt.VerifyURL(sVerifyMsgSig, sVerifyTimeStamp, sVerifyNonce, sVerifyEchoStr, ref sEchoStr);
            if (ret != 0)
            {
                //ret==0表示验证成功，sEchoStr参数表示明文，用户需要将sEchoStr作为get请求的返回参数，返回给企业号。
            }
            return sEchoStr;
        }




    }
}
