using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public interface IGetFeeData : IDependency
    {

        List<ReturnFeeData> GetFeeShowData(DateTime Time, string PayCompanyCode = "", string TradeType = "");

        int InserIntoBankSystem(string Sql);


        int EidtUserHabit(string UserName, string Value);


        UserHabit LookUserHabit(string UserName);

        /// <summary>
        /// 获取付方付款公司名称
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        string GetSubjectCode(string companyCode, string providerName);

        bool IsBelongInternalTransfer(string BillNo);


        bool BitchInserIntoBankSystem(List<PrePayData> Predata, string Creator);


        bool BitchUpdateStatus(List<PrePayData> Predata, string Creator);

        bool BitchUpdateStatus(List<string> Str);

        bool SynchData();
    }
}