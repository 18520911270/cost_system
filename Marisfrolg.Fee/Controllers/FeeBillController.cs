using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;
using System.IO;
using System.Data;

namespace Marisfrolg.Fee.Controllers
{
    public class FeeBillController : SecurityController
    {
        public ActionResult FeeBill(string BillNo = null, string Mode = null, string IsCopy = null)
        {
            FeeBillModelRef model = new FeeBillModelRef();
            if (!string.IsNullOrEmpty(BillNo))
            {
                //草稿箱功能
                if (BillNo.Contains("CG"))
                {
                    FeeBillModel FeeModel = new DraftBox().GetBillModel(BillNo);
                    model = FeeModel.MapTo<FeeBillModel, FeeBillModelRef>();
                    model.StringTime = (model.CreateTime.ToString("yyyy-MM-dd"));
                    model.DeleteDraftNo = BillNo;  //提交之后需要删除草稿箱
                    model.ModelString = Public.JsonSerializeHelper.SerializeToJson(model);
                }
                //编辑功能
                else if (BillNo.Contains("FB"))
                {

                    FeeBill FB_Bl = new BLL.FeeBill();
                    FeeBillModel FeeModel = FB_Bl.GetBillModel(BillNo);
                    model = FeeModel.MapTo<FeeBillModel, FeeBillModelRef>();
                    model.StringTime = (model.CreateTime.ToString("yyyy-MM-dd"));
                    if (!string.IsNullOrEmpty(IsCopy))
                    {
                        model.IsCopy = 1;
                        //复制即累加次数
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("CopyCount", (model.CopyCount + 1).ToString());
                        var status = FB_Bl.PublicEditMethod(model.BillNo, dic);
                    }
                    model.ModelString = Public.JsonSerializeHelper.SerializeToJson(model);
                }

            }
            model.PageName = "FeeBill";
            model.CommitType = CommitType.费用报销单;
            return View(model);
        }


        public ActionResult NoticeBill()
        {
            return View();
        }

        public string FlowCreate()
        {
            return "";
        }
        /// <summary>
        /// 提交单据
        /// </summary>
        /// <param name="postFeeBill"></param>
        /// <returns></returns>
        [HttpPost]
        public string CreateFeeBill(FeeBillModel postFeeBill)
        {

            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postFeeBill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postFeeBill.Items == null || postFeeBill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postFeeBill.TotalMoney = postFeeBill.Items.Sum(all => all.money) + postFeeBill.Items.Sum(all => all.taxmoney);
            }
            if (postFeeBill.Photos == null || postFeeBill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }


            foreach (var item in postFeeBill.Items)
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

            if (postFeeBill.SpecialAttribute.MarketDebt != 1 && postFeeBill.SpecialAttribute.BankDebt != 1 && postFeeBill.SpecialAttribute.Cash != 1)
            {
                if (postFeeBill.CollectionInfo == null || string.IsNullOrEmpty(postFeeBill.CollectionInfo.CardCode) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.Name) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.SubbranchBank) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.SubbranchBankCode))
                {
                    result = "收款信息缺失";
                    return result;
                }
            }
            else
            {
                postFeeBill.CollectionInfo = new CollectionInfo();
            }

            var obj = PublicGetCosterCenter(postFeeBill.PersonInfo.IsHeadOffice, postFeeBill.PersonInfo.CostCenter);
            if (postFeeBill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postFeeBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)
                    {
                        if (postFeeBill.PersonInfo.Brand == null || postFeeBill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postFeeBill.PersonInfo.Brand)
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
                if (!string.IsNullOrEmpty(postFeeBill.PersonInfo.ShopCode))
                {
                    postFeeBill.ShopLogo = DbContext.SHOP.Where(c => c.CODE == postFeeBill.PersonInfo.ShopCode).Select(x => x.SHOPLOGO).FirstOrDefault();
                }
            }
            try
            {

                var lable = PublicDemand(postFeeBill.PersonInfo.IsHeadOffice, postFeeBill.BillsType, postFeeBill.PersonInfo.DepartmentCode, obj, postFeeBill.Items, postFeeBill.SpecialAttribute, postFeeBill.DepartmentName);


                //PackClass PackString = new PackClass() { Creator = postFeeBill.Creator, BillsType = postFeeBill.BillsType, Brand = postFeeBill.PersonInfo.Brand, CompanyCode = postFeeBill.PersonInfo.CompanyCode, CostCenter = postFeeBill.PersonInfo.CostCenter, Department = postFeeBill.PersonInfo.Department, DepartmentCode = postFeeBill.PersonInfo.DepartmentCode, IsHeadOffice = postFeeBill.PersonInfo.IsHeadOffice, Items = postFeeBill.Items.Select(x => x.name).ToList() };

                //Dictionary<string, string> dic = new Dictionary<string, string>();

                //string pack = Public.JsonSerializeHelper.SerializeToJson(PackString);

                //dic.Add("pack", pack);

                //string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                string MaxNumber = new FeeBill().GenerateMaxBillNo();



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
                    objectID = proxy.NewWorkFlowInstance(lable.CODE, postFeeBill.Creator, MaxNumber, dicString);
                }
                catch
                {
                    DeleteInvalidBillNo(MaxNumber);
                    result = "服务内部出错，请联系数控中心";
                    return result;
                }

                postFeeBill.CreateTime = DateTime.Now;
                postFeeBill.Status = 0;
                postFeeBill.WorkFlowID = objectID;
                postFeeBill.BillsItems = postFeeBill.Items;
                postFeeBill.BillNo = MaxNumber;
                new FeeBill().CreateFeeBill(postFeeBill);

                result = MaxNumber;
            }
            catch
            {
                result = "Fail";
            }


            return result;
        }

        /// <summary>
        /// 保存草稿箱
        /// </summary>
        /// <param name="postFeeBill"></param>
        /// <returns></returns>
        [HttpPost]
        public string CreateFeeBillDraftBox(FeeBillModel postFeeBill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postFeeBill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postFeeBill.Items == null || postFeeBill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postFeeBill.TotalMoney = postFeeBill.Items.Sum(all => all.money) + postFeeBill.Items.Sum(all => all.taxmoney);
            }
            if (postFeeBill.Photos == null || postFeeBill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }

            //最大支持6个月以内的业务补充
            if (DateTime.Now.Subtract(postFeeBill.TransactionDate).Days > 180)
            {
                result = "日期超出";
                return result;
            }

            ////选中了商场账扣，不需要银行开卡信息
            //if (postFeeBill.SpecialAttribute.MarketDebt != 1 && postFeeBill.SpecialAttribute.BankDebt != 1 && postFeeBill.SpecialAttribute.Cash != 1)
            //{
            //    if (postFeeBill.CollectionInfo == null || string.IsNullOrEmpty(postFeeBill.CollectionInfo.Bank) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.CardCode) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.City) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.Name) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.SubbranchBank))
            //    {
            //        result = "收款信息缺失";
            //        return result;
            //    }
            //}

            var obj = GetBrandFromCosterCenterNew(postFeeBill.PersonInfo.CostCenter);
            var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postFeeBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
            if (DicModel != null)
            {
                if (DicModel.PARTBRAND == 1)  //分品牌
                {
                    if (postFeeBill.PersonInfo.Brand == null || postFeeBill.PersonInfo.Brand.Count < 1)
                    {
                        result = "记账品牌至少选择一项！";
                        return result;
                    }
                    int brandcount = 0;
                    foreach (var item in postFeeBill.PersonInfo.Brand)
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
            try
            {

                postFeeBill.CreateTime = DateTime.Now;
                postFeeBill.Status = 0;
                postFeeBill.BillsItems = postFeeBill.Items;
                new DraftBox().CreateFeeBillDraftBox(postFeeBill);

                result = "Success";
            }
            catch
            {

                result = "Fail";
            }


            return result;
        }

        /// <summary>
        /// 获取报销项大类
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <returns></returns>
        public string GetCostManage(string IsHeadOffice, string EmployeeNo)
        {
            List<Person> Data = null;

            string sql = "select ID from FEE_PERSON_EXTEND where TYPE='AllAccount' and VALUE like '%" + EmployeeNo + "%'";
            var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            //给所有权限
            if (!string.IsNullOrEmpty(result))
            {
                var list = DbContext.FEE_ACCOUNT.Select(x => x.ACCOUNT_TYPE).Distinct().ToList();
                List<string> StrList = new List<string>();
                foreach (var item in list)
                {
                    StrList.Add(item.ToString());
                }
                Data = DbContext.FEE_ACCOUNT_TYPE.Where(c => StrList.Contains(c.CODE)).OrderBy(x => x.SORT).Select(x => new Person
                {
                    No = x.CODE,
                    Name = x.NAME,
                }).ToList();
                return Data == null ? "" : Public.JsonSerializeHelper.SerializeToJson(Data);
            }

            int num = 0;
            if (IsHeadOffice == "1")
            {
                num = 2;
            }
            else
            {
                num = 0;
            }

            try
            {
                var list = DbContext.FEE_ACCOUNT.Where(c => c.HIDE != num).Select(x => x.ACCOUNT_TYPE).Distinct().ToList();
                List<string> StrList = new List<string>();
                foreach (var item in list)
                {
                    StrList.Add(item.ToString());
                }
                Data = DbContext.FEE_ACCOUNT_TYPE.Where(c => StrList.Contains(c.CODE)).OrderBy(x => x.SORT).Select(x => new Person
                {
                    No = x.CODE,
                    Name = x.NAME,
                }).ToList();
                return Data == null ? "" : Public.JsonSerializeHelper.SerializeToJson(Data);
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("获取报销大类数据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }

        /// <summary>
        /// 获取具体报销项大类详情
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        public string GetAccountInfo(string IsHeadOffice, short Code, string EmployeeNo)
        {
            List<AccountInfo> Data = null;

            string sql = "select ID from FEE_PERSON_EXTEND where TYPE='AllAccount' and VALUE like '%" + EmployeeNo + "%'";
            var result = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
            //给所有权限
            if (!string.IsNullOrEmpty(result))
            {
                Data = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT_TYPE == Code).Select(x => new AccountInfo
                {
                    No = x.ACCOUNT,
                    Name = x.NAME,
                    ReasonCode = x.REASON_CODE,
                    SortId = x.SORT
                }).ToList();
            }
            else
            {
                if (IsHeadOffice == "1")
                {
                    Data = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT_TYPE == Code && c.HIDE != 2).Select(x => new AccountInfo
                    {
                        No = x.ACCOUNT,
                        Name = x.NAME,
                        ReasonCode = x.REASON_CODE,
                        SortId = x.SORT
                    }).ToList();
                }
                else
                {
                    Data = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT_TYPE == Code && c.HIDE != 0).OrderBy(x => x.SORT).Select(x => new AccountInfo
                    {
                        No = x.ACCOUNT,
                        Name = x.NAME,
                        ReasonCode = x.REASON_CODE,
                        ApprovalType = x.WORKFLOW_CODE,
                        Permission = x.PERMISSION,
                        Market = x.IS_MARKET,
                        SortId = x.SORT
                    }).OrderBy(c => c.ApprovalType).ToList();
                }

            }
            Data = Data.OrderBy(c => c.SortId).ToList();
            return Data == null ? "" : Public.JsonSerializeHelper.SerializeToJson(Data);
        }


        /// <summary>
        /// 删除草稿箱内容
        /// </summary>
        /// <param name="IsHeadOffice"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        public string DeleteDraftContent(string BillNo)
        {
            try
            {
                bool result = new Marisfrolg.Fee.BLL.DraftBox().DeleteDraftContent(BillNo);
                return result == true ? "Yes" : "";
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("删除草稿箱内容失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return "";
        }


        /// <summary>
        /// 获取收款信息
        /// </summary>
        /// <param name="No"></param>
        /// <returns></returns>
        public string GetCollectionInfo(string No)
        {
            var model = new CollectionInfoHelper().GetModel(No);
            List<string> str = new List<string>();
            if (model.Count > 0)
            {
                foreach (var item in model)
                {
                    str.Add(string.Format("{0},{1},{2},{3},{4}", item.Name, item.City, item.BankName, item.BankCode, item.SubbranchBank));
                }
            }
            return str.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(str);
        }

        /// <summary>
        /// 获取初始收款信息
        /// </summary>
        /// <param name="No"></param>
        /// <returns></returns>
        public string GetInitCollectionInfo(string No)
        {
            var model = DbContext.FEE_PERSONINFO.Where(c => c.NO == No).FirstOrDefault();

            if (model != null && string.IsNullOrEmpty(model.SUBBRANCHBANKCODE))
            {
                string code = GetSubbranchBankCode(model.SUBBRANCHBANK);

                if (!string.IsNullOrEmpty(code))
                {
                    model.SUBBRANCHBANKCODE = code;


                    string sql = string.Format("update FEE_PERSONINFO set SUBBRANCHBANKCODE='{0}'  where no='{1}'", code, No);

                    DbContext.Database.ExecuteSqlCommand(sql);
                }
            }

            return model == null ? "" : Public.JsonSerializeHelper.SerializeToJson(model);
        }


        /// <summary>
        /// 设为默认值（将收款银行设为默认值）
        /// </summary>
        /// <returns></returns>
        public string UpdateCollectionInfo(string No, string Name, string SubBranch, string Code, string Unionlinenumber)
        {
            string error = string.Empty;
            string msg = string.Empty;

            int result = 0;

            var P = DbContext.FEE_PERSONINFO.Where(c => c.NO == No).FirstOrDefault();
            if (P == null)
            {
                Marisfrolg.Business.FEE_PERSONINFO Per = new Marisfrolg.Business.FEE_PERSONINFO();
                Per.NO = No;
                Per.SUBBRANCHBANK = SubBranch;
                Per.BANKACCOUNT = Code;
                Per.NAME = Name;
                Per.SUBBRANCHBANKCODE = Unionlinenumber;
                DbContext.FEE_PERSONINFO.Add(Per);
                result = DbContext.SaveChanges();
            }
            else
            {
                string sql = string.Format("update FEE_PERSONINFO set SUBBRANCHBANK='{0}',BANKACCOUNT='{1}',NAME='{2}',SUBBRANCHBANKCODE='{3}'  where no='{4}'", SubBranch, Code, Name, Unionlinenumber, No);

                result = DbContext.Database.ExecuteSqlCommand(sql);

            }
            if (P == null && result > 0)
            {
                error = "0";
                msg = "新增成功";
            }
            else if (P != null && result > 0)
            {
                error = "0";
                msg = "编辑成功";
            }
            else
            {
                error = "1";
                msg = "系统繁忙";
            }
            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, msg = msg });
        }



        public string LoadSubBankData(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            value = value.Replace(" ", ",");
            var list = value.Split(',').ToList();
            if (list.Count <= 0)
            {
                return "";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("select NAME from t_sy_banklocations_view  where ");

            if (list.Count <= 1)
            {
                var temp = string.Format("'%{0}%'", list[0]);
                sb.Append("( name like " + temp + " or Code = '" + list[0] + "') and rownum<=100 ");
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                    {
                        var temp = string.Format("'%{0}%'", list[i]);
                        sb.Append("  name like " + temp + " ");
                    }
                    else
                    {
                        var temp = string.Format("'%{0}%'", list[i]);
                        sb.Append("  and  name like " + temp + " ");
                    }
                }

                sb.Append("and rownum<=100 ");
            }

            var dt = new ReportHelper().GetDataTable(sb.ToString(), 2);

            List<string> model = new List<string>();

            foreach (DataRow item in dt.Rows)
            {
                model.Add(item["NAME"].ToString());
            }

            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        /// <summary>
        /// 获取开户行
        /// </summary>
        /// <returns></returns>
        public string GetAllBankName()
        {
            string sql = "select NAME from t_sy_banks_view";
            var dt = new ReportHelper().GetDataTable(sql, 2);

            List<string> model = new List<string>();

            foreach (DataRow item in dt.Rows)
            {
                model.Add(item["NAME"].ToString());
            }

            return Public.JsonSerializeHelper.SerializeToJson(model);
        }

        /// <summary>
        /// 获取指定支行
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public string GetSubBranch(string Name)
        {
            string sql = "select a.Name from t_sy_banklocations_view a left join t_sy_banks_view  b on a.bankcode=b.code where b. name='" + Name + "'";
            var dt = new ReportHelper().GetDataTable(sql, 2);
            List<string> model = new List<string>();
            foreach (DataRow item in dt.Rows)
            {
                model.Add(item["NAME"].ToString());
            }
            return Public.JsonSerializeHelper.SerializeToJson(model);
        }


        /// <summary>
        /// 编辑单据
        /// </summary>
        /// <param name="BillNo"></param>
        /// <param name="PageName"></param>
        /// <returns></returns>
        [HttpPost]
        public string EditFeeBillContent(FeeBillModel postFeeBill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postFeeBill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postFeeBill.Items == null || postFeeBill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postFeeBill.TotalMoney = postFeeBill.Items.Sum(all => all.money) + postFeeBill.Items.Sum(all => all.taxmoney);
            }
            if (postFeeBill.Photos == null || postFeeBill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }
            if (postFeeBill.SpecialAttribute.MarketDebt != 1 && postFeeBill.SpecialAttribute.BankDebt != 1 && postFeeBill.SpecialAttribute.Cash != 1)
            {
                if (postFeeBill.CollectionInfo == null || string.IsNullOrEmpty(postFeeBill.CollectionInfo.CardCode) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.Name) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.SubbranchBank) || string.IsNullOrEmpty(postFeeBill.CollectionInfo.SubbranchBankCode))
                {
                    result = "收款信息缺失";
                    return result;
                }
            }
            else
            {
                postFeeBill.CollectionInfo = new CollectionInfo();
            }


            var obj = PublicGetCosterCenter(postFeeBill.PersonInfo.IsHeadOffice, postFeeBill.PersonInfo.CostCenter);
            if (postFeeBill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postFeeBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)  //分品牌
                    {
                        if (postFeeBill.PersonInfo.Brand == null || postFeeBill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postFeeBill.PersonInfo.Brand)
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

                var lable = PublicDemand(postFeeBill.PersonInfo.IsHeadOffice, postFeeBill.BillsType, postFeeBill.PersonInfo.DepartmentCode, obj, postFeeBill.Items, postFeeBill.SpecialAttribute, postFeeBill.DepartmentName);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                var oldModel = new FeeBill().GetBillModel(postFeeBill.BillNo);
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                string objectID = proxy.NewWorkFlowInstance(lable.CODE, postFeeBill.Creator, oldModel.BillNo, dicString);

                postFeeBill.BillNo = oldModel.BillNo;
                postFeeBill.Id = oldModel.Id;
                postFeeBill.WorkFlowID = objectID;
                postFeeBill.CreateTime = oldModel.CreateTime;
                postFeeBill.Status = oldModel.Status;
                postFeeBill.ApprovalPost = oldModel.ApprovalPost;
                postFeeBill.ApprovalStatus = oldModel.ApprovalStatus;
                postFeeBill.ApprovalTime = oldModel.ApprovalTime;
                postFeeBill.BillsItems = postFeeBill.Items;


                string status = new FeeBill().EditFeeBill(postFeeBill);
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


        public string GetCoscenterMean(int IsHeadOffice, string CosterCenter)
        {
            ObjectList Result = new ObjectList();
            if (IsHeadOffice == 1)
            {
                Result = GetBrandFromCosterCenter(CosterCenter);
            }
            else
            {
                Result = GetBrandFromCosterCenterNew(CosterCenter);
            }
            return Public.JsonSerializeHelper.SerializeToJson(Result);
        }
    }
}
