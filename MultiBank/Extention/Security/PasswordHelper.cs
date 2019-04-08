using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.Extention
{
    public static class PasswordHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Encrypt(string password, string sKey)
        {
            string ret = EncryptMD5Password(password.ToMD5(), sKey);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="md5Password">经过md5加密的密码</param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string EncryptMD5Password(string md5Password, string sKey)
        {
            string encryptedPassword = DESEncrypt.Encrypt(md5Password.ToLower(), sKey).ToLower();
            string ret = encryptedPassword.ToMD5().ToLower();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwdText">明文密码</param>
        public static void EnsurePasswordLegal(string pwdText)
        {
            if (pwdText == null || pwdText.Length < 6 || pwdText.Length > 15)
                throw new Exception("");
            //throw new Ace.Exceptions.InvalidDataException("密码必须是6-15位");
        }
    }
}