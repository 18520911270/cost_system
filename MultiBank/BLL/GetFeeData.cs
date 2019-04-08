using MultiBank.DAL;
using MultiBank.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MultiBank.BLL
{
    public class GetFeeData : IGetFeeData
    {
        public List<ReturnFeeData> GetFeeShowData(DateTime Time, string PayCompanyCode = "", string TradeType = "")
        {
            string sql = string.Format("SELECT ID,SYSTEMTYPE,BILLNO,BILLTYPE,DEPARTMENTNAME,SHOPNAME,SHOPCODE,CITY,PAYCOMPANYCODE,COMPANYCODE,BILLMONEY,LOANMONEY,AMOUNTMONEY,SETTINGTIME,OPPPRIVATEFLAG,ACCOUNTNO,ACCOUNTUSERNAME,ACCOUNTSUBBRANCHBANK,PAYSTATUS,ISDEL,CURRENCY,PREPAIDBANKNUMBER,REALPAYTIME,SUBMITPAYMENTTIME,CREATOR,DEALSTATE,SETTINGTIME FROM BANK_WARE WHERE SETTINGTIME>=to_date('{0}','YYYY-MM-DD HH24:MI:SS') AND  SETTINGTIME<to_date('{1}','YYYY-MM-DD HH24:MI:SS')", Time.ToString("yyyy-MM-dd HH:mm:ss"), Time.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));

            if (!string.IsNullOrEmpty(PayCompanyCode))
            {
                sql += " AND PAYCOMPANYCODE='" + PayCompanyCode + "'";
            }


            if (!string.IsNullOrEmpty(TradeType))
            {
                sql += " AND OPPPRIVATEFLAG='" + TradeType + "' ";
            }

            OracleHelper _oraDal = new OracleHelper();

            var dt = _oraDal.ExecuteQuery(sql);

            var list = _oraDal.DataTableToList<ReturnFeeData>(dt).ToList();

            return list;
        }

        /// <summary>
        /// 执行单个Sql
        /// </summary>
        /// <param name="Sql"></param>
        /// <returns></returns>
        public int InserIntoBankSystem(string Sql)
        {
            var model = GetFeeShowData(DateTime.Now.AddDays(-1));

            var first = model.Where(c => !string.IsNullOrEmpty(c.ACCOUNTSUBBRANCHBANK)).FirstOrDefault();

            OracleHelper _oraDal = new OracleHelper("TestMultiBankOracleDBConn");

            string lookSql = "select b.code,b.name,c.code from t_sy_banklocations_view a left join t_sy_banks_view b on a.BANKCODE=b.CODE left join t_sy_areas_view c on a.AREACODE=C.CODE  where a.code='" + first.ACCOUNTSUBBRANCHBANK + "'";

            DataTable dt = _oraDal.ExecuteQuery(lookSql);


            string sql = string.Format(" INSERT INTO T_MI_PAYMENTS(SRCOUTSYSTEMCODE,SRCBATCHNO,ORGCODE,APPLYORGCODE,PAYDATE,OURAMOUNT,LASTOURAMOUNT,CREATEDBY,LASTMODIFIEDBY,OPPPRIVATEFLAG,OPPBANKCODE,OPPBANKLOCATIONCODE,OPPBANKLOCATIONS,OPPBANKACCOUNTNUMBER,OPPBANKACCOUNTNAME,OPPBANKAREACODE,SRCSERIALNO,OURORGCODE,PAYTYPECODE)  VALUES ('{0}','{1}','{2}','{3}',{4},'{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}')", "FEE", first.BILLNO, first.COMPANYCODE, first.PAYCOMPANYCODE, "SYSDATE", first.AMOUNTMONEY, first.AMOUNTMONEY, "01987", "01987", first.OPPPRIVATEFLAG, dt.Rows[0][0].ToString(), first.ACCOUNTSUBBRANCHBANK, dt.Rows[0][1].ToString(), first.ACCOUNTNO, first.ACCOUNTUSERNAME, dt.Rows[0][2].ToString(), first.BILLNO, first.COMPANYCODE, "103");

            int count = _oraDal.ExecuteSQL(sql);

            return count;
        }


        public int EidtUserHabit(string UserName, string Value)
        {
            int result = 0;

            var habitValue = LookUserHabit(UserName);

            OracleHelper _oraDal = new OracleHelper();

            if (string.IsNullOrEmpty(habitValue.PayCompanyCode) && string.IsNullOrEmpty(habitValue.City) && string.IsNullOrEmpty(habitValue.BillType))
            {
                string NewSql = string.Format(" Insert into USER_HABIT (APPNAME,ACTIONNAME,EMPLOYEENO,OPERATIONVALUE) values('MultiBank','PayFee','{0}','{1}') ", UserName, Value);

                result = _oraDal.ExecuteSQL(NewSql);
            }
            else
            {
                string NewSql = string.Format("Update USER_HABIT set OPERATIONVALUE='{0}' where EMPLOYEENO='{1}' and APPNAME='MultiBank' and ACTIONNAME='PayFee'", Value, UserName);

                result = _oraDal.ExecuteSQL(NewSql);
            }

            return result;
        }


        public UserHabit LookUserHabit(string UserName)
        {
            UserHabit Habit = new UserHabit();

            string sql = string.Format("select OPERATIONVALUE from USER_HABIT where EMPLOYEENO='{0}' and APPNAME='MultiBank' and ACTIONNAME='PayFee'", UserName);

            OracleHelper _oraDal = new OracleHelper();

            var dt = _oraDal.ExecuteQuery(sql);

            if (dt.Rows.Count > 0)
            {
                try
                {
                    Habit = JsonHelper.Deserialize<UserHabit>(dt.Rows[0][0].ToString());
                }
                catch (Exception)
                {

                }
            }

            return Habit;
        }


        public bool IsBelongInternalTransfer(string BillNo)
        {
            string sql = string.Format("select b.ACCOUNT_NAME from FEE_NOTICEBILL a left join FEE_NOTICEBILL_ITEMS b on a.id=b.pid where a.billno='{0}' and b.ACCOUNT_NAME='内部转账'", BillNo);

            OracleHelper _oraDal = new OracleHelper();

            var dt = _oraDal.ExecuteQuery(sql);

            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //TestMultiBankOracleDBConn  MultiBankOracleDBConn
        public bool BitchInserIntoBankSystem(List<PrePayData> Predata, string Creator)
        {
            ArrayList DataList = new ArrayList();

            OracleHelper _oraDal = new OracleHelper("MultiBankOracleDBConn");

            foreach (var item in Predata)
            {
                string payInfo = GetTransferPurposes(item);  //待收付用途

                string lookSql = "select b.code,b.name,c.code from t_sy_banklocations_view a left join t_sy_banks_view b on a.BANKCODE=b.CODE left join t_sy_areas_view c on a.AREACODE=C.CODE  where a.code='" + item.DetailedData[0].ACCOUNTSUBBRANCHBANK + "'";
                DataTable dt = _oraDal.ExecuteQuery(lookSql);

                string sql = string.Format(" INSERT INTO T_MI_PAYMENTS(SRCOUTSYSTEMCODE,SRCBATCHNO,ORGCODE,APPLYORGCODE,PAYDATE,OURAMOUNT,LASTOURAMOUNT,CREATEDBY,LASTMODIFIEDBY,OPPPRIVATEFLAG,OPPBANKCODE,OPPBANKLOCATIONCODE,OPPBANKLOCATIONS,OPPBANKACCOUNTNUMBER,OPPBANKACCOUNTNAME,OPPBANKAREACODE,SRCSERIALNO,OURORGCODE,PAYTYPECODE,OPPOBJECTNAME,OPPOBJECTCODE,OPPCOUNTERPARTYCATEGORYCODE,OURBANKACCOUNTNUMBER,SETTLEMENTMODECODE,PURPOSE)  VALUES ('{0}','{1}','{2}','{3}',{4},'{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}')", "FEE", item.PREPAIDBANKNUMBER, item.PAYCOMPANYCODE, item.PAYCOMPANYCODE, "SYSDATE", item.TotalMoney, item.TotalMoney, Creator, Creator, item.OPPPRIVATEFLAG, dt.Rows[0][0].ToString(), item.DetailedData[0].ACCOUNTSUBBRANCHBANK, dt.Rows[0][1].ToString(), item.DetailedData[0].ACCOUNTNO.Replace("-", "").Replace(" ","").Trim(), item.ACCOUNTUSERNAME, dt.Rows[0][2].ToString(), item.PREPAIDBANKNUMBER, item.PAYCOMPANYCODE, item.TradeType, item.ACCOUNTUSERNAME, "1", "333", GetSubjectCode(item.PAYCOMPANYCODE, item.ACCOUNTUSERNAME), "101", payInfo);

                DataList.Add(sql);
            }

            return _oraDal.ExecuteSqlTran(DataList);
        }


        public bool BitchUpdateStatus(List<PrePayData> Predata, string Creator)
        {
            ArrayList DataList = new ArrayList();

            OracleHelper _oraDal = new OracleHelper();


            foreach (var item in Predata)
            {
                foreach (var temp in item.DetailedData)
                {
                    string sql = string.Format(" Update BANK_WARE set PAYSTATUS=1,CREATOR='{0}',SUBMITPAYMENTTIME={1},PREPAIDBANKNUMBER='{2}' where BILLNO='{3}' AND BILLTYPE='{4}'", Creator, "SYSDATE", item.PREPAIDBANKNUMBER, temp.BILLNO, temp.BILLTYPE);
                    DataList.Add(sql);
                }
            }

            return _oraDal.ExecuteSqlTran(DataList);
        }




        public string GetSubjectCode(string companyCode, string providerName)
        {
            //1为个人，2为公司，3为外汇
            int grade = 2;
            if (string.IsNullOrEmpty(providerName))
            {
                grade = 1;
            }
            else
            {
                if (providerName.Length <= 4)
                {
                    grade = 1;
                }
                else if (providerName.Length > 4)
                {
                    string str = providerName.Remove(1);
                    string pattern = @"^[a-zA-Z]*$";
                    bool Istrue = System.Text.RegularExpressions.Regex.IsMatch(str, pattern);
                    if (Istrue)
                    {
                        grade = 3;
                    }
                    else
                    {
                        grade = 2;
                    }
                }
            }

            string sql = "select a.ACCOUNTNUMBER from FEE_SAPINFO a left join fee_bank b on a.VALUE=b.ACCOUNTCODE where a.APPNAME='DefaultBank'  and a.COMPANYCODE='" + companyCode + "' and a.GRADE='" + grade + "'";

            OracleHelper _oraDal = new OracleHelper();

            var Database = _oraDal.ExecuteQuery(sql);

            if (Database.Rows.Count > 0)
            {
                return Database.Rows[0][0].ToString();
            }
            else
            {
                return "";
            }
        }



        public bool SynchData()
        {
            string sql = "call PROC_SYNC_BANK_ALLDATA()";
            OracleHelper _oraDal = new OracleHelper();
            int result = _oraDal.ExecuteSQL(sql);
            return true;
        }


        public bool BitchUpdateStatus(List<string> Str)
        {
            ArrayList DataList = new ArrayList();

            OracleHelper _oraDal = new OracleHelper();

            foreach (var item in Str)
            {

                string sql = string.Format(" Update BANK_WARE set PAYSTATUS=0,PREPAIDBANKNUMBER='',DEALSTATE='1' where PREPAIDBANKNUMBER='{0}' AND SYSTEMTYPE='FEE'", item);
                DataList.Add(sql);
            }

            return _oraDal.ExecuteSqlTran(DataList);
        }


        public string GetTransferPurposes(PrePayData data)
        {
            string value = string.Empty;
            try
            {
                if (data.OPPPRIVATEFLAG == 1)
                {
                    //结果集有两种单据以上就填写费用，如果只有单独一个
                    var list = data.DetailedData.Select(c => c.BILLTYPE).Distinct().ToList();
                    if (list.Count > 1)
                    {
                        value = "费用";
                    }
                    else
                    {
                        value = list[0];
                    }
                }
                else if (data.OPPPRIVATEFLAG == 2)
                {
                    //对公付款要查询相应单据
                    string sql = "select a.TRANSACTIONDATE,case when a.shopcode is null then a.boot_DP_name else c.name end name,b.account_name  from  FEE_NOTICEBILL a  left join  FEE_NOTICEBILL_ITEMS b on a.id=b.PID  left join shop c on a.shopcode=c.code where a.billno='" + data.DetailedData[0].BILLNO + "'";

                    OracleHelper _oraDal = new OracleHelper();

                    var dt = _oraDal.ExecuteQuery(sql);

                    if (dt.Rows.Count > 0)
                    {
                        value = Convert.ToDateTime(dt.Rows[0][0]).Month + dt.Rows[0][1].ToString() + dt.Rows[0][2].ToString().Replace('-', ' ').Trim();

                        int StrLength = GetLength(value);
                        if (StrLength > 32)
                        {
                            value = value.Remove(16);
                        }
                    }
                }
                else
                {
                    //外汇暂时不处理
                }
            }
            catch (Exception)
            {

                throw;
            }
            return value;
        }

        /// <summary>
        /// 判断字符长度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetLength(string str)
        {
            if (str.Length == 0)
                return 0;
            System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
            int tempLen = 0;
            byte[] s = ascii.GetBytes(str);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }
            return tempLen;
        }
    }
}