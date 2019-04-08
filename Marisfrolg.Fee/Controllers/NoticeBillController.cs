using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using Marisfrolg.Business;

namespace Marisfrolg.Fee.Controllers
{
    public class NoticeBillController : SecurityController
    {

        public ActionResult NoticeBill(string BillNo = null, string Mode = null, string IsCopy = null)
        {
            NoticeBill NB_Bl = new NoticeBill();
            List<NoticeBillModel> modellist = NB_Bl.FindSuitableData("良");
            FeeBillModelRef model = new FeeBillModelRef();
            if (!string.IsNullOrEmpty(BillNo))
            {
                //草稿箱功能(暂时还没有)
                if (BillNo.Contains("CK"))
                {
                    //暂时没处理
                }
                //编辑功能
                else if (BillNo.Contains("FT"))
                {
                    NoticeBillModel NoticeModel = NB_Bl.GetBillModel(BillNo);
                    model.ModelString = Public.JsonSerializeHelper.SerializeToJson(NoticeModel);
                    model.BillNo = NoticeModel.BillNo;
                    model.PersonInfo = NoticeModel.PersonInfo;
                    model.Owner = NoticeModel.Owner;
                    model.WorkNumber = NoticeModel.WorkNumber;
                    model.Remark = NoticeModel.Remark;
                    model.TransactionDate = NoticeModel.TransactionDate;
                    if (!string.IsNullOrEmpty(IsCopy))
                    {
                        model.IsCopy = 1;

                        //复制即累加次数
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("CopyCount", (model.CopyCount + 1).ToString());
                        var status = NB_Bl.PublicEditMethod(model.BillNo, dic);
                    }
                }
            }
            model.PageName = "Notice";
            model.CommitType = CommitType.付款通知书;
            return View(model);
        }

        [HttpPost]
        public string CreateNoticeBill(NoticeBillModel postbill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postbill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postbill.Items == null || postbill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postbill.TotalMoney = postbill.Items.Sum(all => all.money) + postbill.Items.Sum(all => all.taxmoney);
            }
            if (postbill.Photos == null || postbill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }
            if (postbill.ProviderInfo == null || string.IsNullOrEmpty(postbill.ProviderInfo.BankName) || string.IsNullOrEmpty(postbill.ProviderInfo.BankNo) || string.IsNullOrEmpty(postbill.ProviderInfo.ProviderName))
            {
                result = "供应商信息缺失";
                return result;
            }

            int Grade = GetSubjectCode(postbill.PersonInfo.CompanyCode, postbill.ProviderInfo.ProviderName);
            if (Grade != 3)
            {
                //验证支行的准确性
                var ResultInfo = GetSubbranchBankCode(postbill.ProviderInfo.BankName);
                if (string.IsNullOrEmpty(ResultInfo))
                {
                    result = "支行数据错误,输入关键地名选择提示支行勿手工录入";
                    return result;
                }
                postbill.ProviderInfo.SubbranchBankCode = ResultInfo;
            }

            foreach (var item in postbill.Items)
            {
                var value = DbContext.FEE_ACCOUNT.Where(c => c.NAME == item.name || c.OLDNAME == item.name).Select(c => c.IS_MARKET).FirstOrDefault();
                if (value == null)
                {
                    item.IsMarket = 0;
                }
                else
                {
                    item.IsMarket = Convert.ToInt32(item.IsMarket);
                }
            }

            var obj = GetBrandFromCosterCenterNew(postbill.PersonInfo.CostCenter);
            if (postbill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postbill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)  //分品牌
                    {
                        if (postbill.PersonInfo.Brand == null || postbill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postbill.PersonInfo.Brand)
                        {
                            if (DicModel.BRANDLIST.Contains(item))
                            {
                                brandcount++;
                            }
                        }
                        if (brandcount >= 2)
                        {
                            result = DicModel.BRANDLIST + "只能选择一项！";
                            return result;
                        }
                    }
                }

                //增加店柜品牌属性
                if (!string.IsNullOrEmpty(postbill.PersonInfo.ShopCode))
                {
                    postbill.ShopLogo = DbContext.SHOP.Where(c => c.CODE == postbill.PersonInfo.ShopCode).Select(x => x.SHOPLOGO).FirstOrDefault();
                }
            }
            try
            {
                SpecialAttribute Spe = new SpecialAttribute() { Funds = postbill.SpecialAttribute.Funds, Agent = postbill.SpecialAttribute.Agent, Check = postbill.SpecialAttribute.Check };

                var lable = PublicDemand(postbill.PersonInfo.IsHeadOffice, postbill.BillsType, postbill.PersonInfo.DepartmentCode, obj, postbill.Items, Spe, postbill.DepartmentName);


                //PackClass PackString = new PackClass() { Creator = postbill.Creator, BillsType = postbill.BillsType, Brand = postbill.PersonInfo.Brand, CompanyCode = postbill.PersonInfo.CompanyCode, CostCenter = postbill.PersonInfo.CostCenter, Department = postbill.PersonInfo.Department, DepartmentCode = postbill.PersonInfo.DepartmentCode, IsHeadOffice = postbill.PersonInfo.IsHeadOffice, Items = postbill.Items.Select(x => x.name).ToList(), IsUrgent = postbill.IsUrgent };

                //Dictionary<string, string> dic = new Dictionary<string, string>();

                //string pack = Public.JsonSerializeHelper.SerializeToJson(PackString);

                //dic.Add("pack", pack);

                //string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                string MaxNumber = new NoticeBill().GenerateMaxBillNo();



                try
                {
                    string sql = " insert into FEE_BILLNO (BILLNO) values('" + MaxNumber + "')";
                    int num = DbContext.Database.ExecuteSqlCommand(sql);
                    if (num < 1)
                    {
                        DeleteInvalidBillNo(MaxNumber);
                        result = "服务器繁忙!请重新提交";
                        return result;
                    }
                }
                catch
                {
                    result = "服务器繁忙!请重新提交";
                    return result;
                }

                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                string objectID = string.Empty;
                try
                {
                    objectID = proxy.NewWorkFlowInstance(lable.CODE, postbill.Creator, MaxNumber, dicString);
                }
                catch
                {
                    DeleteInvalidBillNo(MaxNumber);
                    result = "服务内部出错，请联系数控中心";
                    return result;
                }

                CommitPorviderInfo(postbill.ProviderInfo, postbill.Creator);
                postbill.CreateTime = DateTime.Now;
                postbill.Status = 0;
                postbill.WorkFlowID = objectID;
                postbill.BillsItems = postbill.Items;
                postbill.BillNo = MaxNumber;
                new NoticeBill().CreateNoticeBill(postbill);
                result = MaxNumber;
            }
            catch
            {
                result = "Fail";
            }
            return result;
        }

        /// <summary>
        /// 维护供应商银行信息
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="No"></param>
        /// <returns></returns>
        public string CommitPorviderInfo(ProviderInfo Info, string No)
        {
            try
            {
                var ProviderModel = DbContext.FEE_PROVIDER.Where(c => c.CODE == Info.ProviderCode).FirstOrDefault();
                //一次性供应商的信息保存
                if (ProviderModel != null && ProviderModel.NAME.Contains("一次性"))
                {
                    var infoData = DbContext.FEE_PROVIDERBANK.Where(c => c.CODE == Info.ProviderCode && c.PROVIDERNAME == Info.ProviderName && c.BANKNAME == Info.BankName && c.BANKNO == Info.BankNo).FirstOrDefault();
                    if (infoData != null)
                    {
                        return "Success";
                    }
                    FEE_PROVIDERBANK providerBank = new FEE_PROVIDERBANK();
                    providerBank.CODE = Info.ProviderCode;
                    providerBank.BANKCODE = Info.BankNo;//银行编码（不知道神马玩意）
                    providerBank.BANKNAME = Info.BankName;
                    providerBank.BANKNO = Info.BankNo;
                    providerBank.BANKOWNER = No;
                    providerBank.COMPANYCODE = Info.CompanyCode;
                    providerBank.IBAN = Info.IBAN;
                    providerBank.SWIFT = Info.BankCode;
                    providerBank.ISUSERDATA = 1;  //用户自建数据
                    providerBank.PROVIDERNAME = Info.ProviderName;
                    providerBank.LAND = ProviderModel.LAND;
                    DbContext.FEE_PROVIDERBANK.Add(providerBank);
                    int Result = DbContext.SaveChanges();
                    if (Result < 0)
                    {
                        return "Fail";
                    }
                }
                //非一次性供应商的信息保存（不允许相同code不同companycode保存同样的银行信息）
                else
                {
                    var infoData = DbContext.FEE_PROVIDERBANK.Where(c => c.CODE == Info.ProviderCode && c.BANKNAME == Info.BankName.Trim() && c.BANKNO == Info.BankNo.Trim()).FirstOrDefault();
                    if (infoData != null)
                    {
                        return "Success";
                    }
                    FEE_PROVIDERBANK providerBank = new FEE_PROVIDERBANK();
                    providerBank.CODE = Info.ProviderCode;
                    providerBank.BANKCODE = Info.BankNo;//银行编码（不知道神马玩意）
                    providerBank.BANKNAME = Info.BankName;
                    providerBank.BANKNO = Info.BankNo;
                    providerBank.BANKOWNER = No;
                    providerBank.COMPANYCODE = Info.CompanyCode;
                    providerBank.IBAN = Info.IBAN;
                    providerBank.SWIFT = Info.BankCode;
                    providerBank.ISUSERDATA = 1;  //用户自建数据
                    providerBank.PROVIDERNAME = Info.ProviderName;
                    providerBank.LAND = ProviderModel.LAND;
                    DbContext.FEE_PROVIDERBANK.Add(providerBank);
                    int Result = DbContext.SaveChanges();
                    if (Result < 0)
                    {
                        return "Fail";
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                Logger.Write("跟新供应商银行信息错误：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }


        /// <summary>
        /// 专为一次性供应商使用
        /// </summary>
        /// <returns></returns>
        public string OnceProviderInfo()
        {
            //SAP数据
            var list = (from a in DbContext.FEE_PROVIDER
                        join b in DbContext.FEE_PROVIDERBANK on a.CODE equals b.CODE
                        where a.GROUP_TYPE != "Z003" & a.GROUP_TYPE != "Z004" & a.NAME.Contains("一次性")
                        select new
                        {
                            label = b.PROVIDERNAME + "--" + b.BANKNAME + "--" + b.BANKNO + "--" + (b.SWIFT == null ? "" : b.SWIFT + "|") + (b.IBAN == null ? "" : b.IBAN + "|") + (b.ISUSERDATA == 0 ? "SAP数据" : "自建数据"),
                            value = ""
                        }
                                ).ToList();
            return list.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(list);
        }

        /// <summary>
        /// 获取供应商信息
        /// </summary>
        /// <param name="InputWord"></param>
        /// <returns></returns>
        public string GetProviderInfo(string CompanyCode)
        {
            try
            {
                #region join和GroupBy的用法
                ////得到所有符合条件的数据
                //var model = DbContext.FEE_PROVIDER.Join(DbContext.FEE_PROVIDERBANK, c => c.CODE, p => p.CODE, (c, p) => new TempData
                //  {
                //      Name = c.NAME,
                //      Code = c.CODE,
                //      BankName = p.BANKNAME,
                //      BankNo = p.BANKNO
                //  }).ToList();
                ////var sb = model.Select(c => c.Name).ToList();
                ////对数据进行处理，相同code的数据组合在一个对象里面
                //var ls = model.GroupBy(a => a.Code).Select(g => (new ProviderInfo { Code = g.Key, Name = g.ElementAt(0).Name, BankName = g.Select(c => c.BankName).ToList(), BankNo = g.Select(c => c.BankNo).ToList() })).ToList(); 
                #endregion
                #region 原有逻辑已经不满足需求
                //var query = (from a in DbContext.FEE_PROVIDER
                //             join b in DbContext.FEE_PROVIDER_EXPAND on a.CODE equals b.PROVIDERCODE
                //             where a.GROUP_TYPE != "Z003" & a.GROUP_TYPE != "Z004"
                //             select new
                //             {
                //                 label = b.COMPANYCODE + "*" + a.NAME ,
                //                 value = a.CODE
                //             }
                //                ).ToList();

                //return query.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(query); 
                #endregion
                #region 旧代码
                //List<TempData> model = new List<TempData>();

                ////玛丝菲尔和奥姆公司
                //if (CompanyCode == "1000" || CompanyCode == "1300")
                //{
                //    //var list = DbContext.FEE_PROVIDER_EXPAND.Where(c => c.COMPANYCODE == CompanyCode).Select(x => x.PROVIDERCODE).ToList();
                //    var list = DbContext.FEE_PROVIDER_EXPAND.Select(x => x.PROVIDERCODE).ToList();

                //    model = DbContext.FEE_PROVIDER.Where(c => list.Contains(c.CODE) & c.GROUP_TYPE != "Z003" & c.GROUP_TYPE != "Z004").Select(c => new TempData
                //    {
                //        label = c.NAME,
                //        value = c.CODE
                //    }).ToList();
                //    foreach (var item in model)
                //    {
                //        item.label = CompanyCode + "-" + item.label;
                //    }
                //}
                ////明佳豪或者其他公司
                //else
                //{
                //    var list = (from a in DbContext.FEE_PROVIDER
                //                join b in DbContext.FEE_PROVIDER_EXPAND on a.CODE equals b.PROVIDERCODE
                //                select new
                //                {
                //                    label = b.COMPANYCODE + "-" + a.NAME,
                //                    value = a.CODE
                //                }
                //                   );
                //    return model == null ? "" : Public.JsonSerializeHelper.SerializeToJson(list);
                //}

                //return model == null ? "" : Public.JsonSerializeHelper.SerializeToJson(model); 
                #endregion

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //1.0正常数据，没有银行数据或者ISUSERDATA为0
                sb.Append(" select a.CODE as value,a.name||case when c.BANKNAME is null then '' else '--'||c.BANKNAME||'--' end ||case when c.BANKNO is null then '' else c.BANKNO end||'--'||'SAP数据'  as label from  FEE_PROVIDER a left join");
                sb.Append(" FEE_PROVIDER_EXPAND b on a.code=b.PROVIDERCODE");
                sb.Append(" left join FEE_PROVIDERBANK c on a.code=c.code");
                sb.Append(" where a.GROUP_TYPE!='Z003' and a.GROUP_TYPE!='Z004'");
                sb.Append(" and  b.PROVIDERCODE is not null and a.name  not like '%一次性%' and(c.ISUSERDATA is null or c.ISUSERDATA=0) and b.COMPANYCODE='" + CompanyCode + "'");
                sb.Append(" union ");
                //2.0用户自建数据，且不能为一次性功能商且ISUSERDATA=1
                sb.Append(" select a.CODE,a.name||'--'||c.BANKNAME||'--'||c.BANKNO||'--'||'自建数据'  from  FEE_PROVIDER a left join ");
                sb.Append("   FEE_PROVIDER_EXPAND b on a.code=b.PROVIDERCODE");
                sb.Append("  left join FEE_PROVIDERBANK c on a.code=c.code");
                sb.Append("   where a.GROUP_TYPE!='Z003' and a.GROUP_TYPE!='Z004'");
                sb.Append("  and  b.PROVIDERCODE is not null and a.name  not like '%一次性%' and c.ISUSERDATA=1 and c.COMPANYCODE=b.COMPANYCODE and  b.COMPANYCODE='" + CompanyCode + "' ");
                sb.Append("  union");
                //3.0一次性供应商不能导入银行信息
                sb.Append("   select a.CODE,a.name  from  FEE_PROVIDER a left join");
                sb.Append("   FEE_PROVIDER_EXPAND b on a.code=b.PROVIDERCODE");
                sb.Append("  where a.GROUP_TYPE!='Z003' and a.GROUP_TYPE!='Z004' and  b.PROVIDERCODE is not null and a.name  like '%一次性%' and  b.COMPANYCODE='" + CompanyCode + "'");
                var datalist = DbContext.Database.SqlQuery<TempData>(sb.ToString()).ToList();
                return datalist.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(datalist);
            }
            catch (Exception ex)
            {
                Logger.Write("获取供应商信息：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取供应商银行信息
        /// </summary>
        /// <returns></returns>
        public string GetProviderBankInfo(string COMPANYCODE, string CODE, string BANKNAME, string BANKNO)
        {
            try
            {
                var ProviderModel = DbContext.FEE_PROVIDER.Where(c => c.CODE == CODE).FirstOrDefault();
                if (ProviderModel == null || ProviderModel.NAME.Contains("一次性"))
                {
                    return "";
                }
                var model = DbContext.FEE_PROVIDERBANK.Where(c => (c.CODE == CODE && c.BANKNAME == BANKNAME && c.BANKNO == BANKNO && c.ISUSERDATA == 0) || (c.CODE == CODE && c.BANKNAME == BANKNAME && c.BANKNO == BANKNO && c.ISUSERDATA == 1 && c.COMPANYCODE == COMPANYCODE)).Select(x => new ProviderInfoShow
                {
                    BankName = x.BANKNAME,
                    BankNo = x.BANKNO,
                    IBAN = x.IBAN,
                    Swift = x.SWIFT
                }).FirstOrDefault();

                return model == null ? "" : Public.JsonSerializeHelper.SerializeToJson(model);
            }
            catch (Exception ex)
            {
                Logger.Write("获取供应商银行信息：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取国家名称
        /// </summary>
        /// <param name="CODE"></param>
        /// <returns></returns>
        public string GetCountryName(string CODE)
        {
            try
            {
                string LAND = DbContext.FEE_PROVIDER.Where(c => c.CODE == CODE).Select(c => c.LAND).FirstOrDefault();
                string Name = DbContext.FEE_COUNTRY.Where(c => c.LAND == LAND).Select(c => c.NAME).FirstOrDefault();
                return Name;
            }
            catch (Exception ex)
            {
                Logger.Write("获取国家信息错误：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        [HttpPost]
        public string EditNoticeBillContent(NoticeBillModel postbill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            if (string.IsNullOrEmpty(postbill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postbill.Items == null || postbill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postbill.TotalMoney = postbill.Items.Sum(all => all.money) + postbill.Items.Sum(all => all.taxmoney);
            }
            if (postbill.Photos == null || postbill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }
            if (postbill.ProviderInfo == null || string.IsNullOrEmpty(postbill.ProviderInfo.BankName) || string.IsNullOrEmpty(postbill.ProviderInfo.BankNo) || string.IsNullOrEmpty(postbill.ProviderInfo.ProviderName))
            {
                result = "供应商信息缺失";
                return result;
            }
            int Grade = GetSubjectCode(postbill.PersonInfo.CompanyCode, postbill.ProviderInfo.ProviderName);
            if (Grade != 3)
            {
                //验证支行的准确性
                var ResultInfo = GetSubbranchBankCode(postbill.ProviderInfo.BankName);
                if (string.IsNullOrEmpty(ResultInfo))
                {
                    result = "支行数据错误,输入关键地名选择提示支行勿手工录入";
                    return result;
                }
                postbill.ProviderInfo.SubbranchBankCode = ResultInfo;
            }

            var obj = GetBrandFromCosterCenterNew(postbill.PersonInfo.CostCenter);
            if (postbill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postbill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)  //分品牌
                    {
                        if (postbill.PersonInfo.Brand == null || postbill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postbill.PersonInfo.Brand)
                        {
                            if (DicModel.BRANDLIST.Contains(item))
                            {
                                brandcount++;
                            }
                        }
                        if (brandcount >= 2)
                        {
                            result = DicModel.BRANDLIST + "只能选择一项！";
                            return result;
                        }
                    }
                }
            }
            try
            {
                SpecialAttribute Spe = new SpecialAttribute() { Funds = postbill.SpecialAttribute.Funds, Agent = postbill.SpecialAttribute.Agent, Check = postbill.SpecialAttribute.Check };

                var lable = PublicDemand(postbill.PersonInfo.IsHeadOffice, postbill.BillsType, postbill.PersonInfo.DepartmentCode, obj, postbill.Items, Spe, postbill.DepartmentName);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                var oldModel = new NoticeBill().GetBillModel(postbill.BillNo);
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                string objectID = proxy.NewWorkFlowInstance(lable.CODE, postbill.Creator, oldModel.BillNo, dicString);

                CommitPorviderInfo(postbill.ProviderInfo, postbill.Creator);


                postbill.BillNo = oldModel.BillNo;
                postbill.Id = oldModel.Id;
                postbill.WorkFlowID = objectID;
                postbill.CreateTime = oldModel.CreateTime;
                postbill.Status = oldModel.Status;
                postbill.ApprovalPost = oldModel.ApprovalPost;
                postbill.ApprovalStatus = oldModel.ApprovalStatus;
                postbill.ApprovalTime = oldModel.ApprovalTime;
                postbill.BillsItems = postbill.Items;

                string status = new NoticeBill().EditNoticeBill(postbill);
                if (status != "Success")
                {
                    result = "编辑失败";
                    return result;
                }

                result = "Success";
            }
            catch
            {

                result = "Fail";
            }
            return result;
        }


        /// <summary>
        /// 判断是否为外汇
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public int GetSubjectCode(string companyCode, string providerName)
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
            return grade;
        }
    }
}
