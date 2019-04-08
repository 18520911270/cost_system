using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marisfrolg.Fee.Controllers
{
    public class BorrowBillController : SecurityController
    {

        public ActionResult BorrowBill(string BillNo = null, string Mode = null, string IsCopy = null)
        {
            FeeBillModelRef model = new FeeBillModelRef();
            if (!string.IsNullOrEmpty(BillNo))
            {
                //草稿箱功能(暂时还没有)
                if (BillNo.Contains("CJ"))
                {
                    //暂时没处理
                }
                //编辑功能
                else if (BillNo.Contains("JS"))
                {

                    BorrowBill BB_Bl = new BLL.BorrowBill();

                    BorrowBillModel BorrowModel = BB_Bl.GetBillModel(BillNo);
                    model.ModelString = Public.JsonSerializeHelper.SerializeToJson(BorrowModel);
                    model.BillNo = BorrowModel.BillNo;
                    model.PersonInfo = new PersonInfo() { Company = BorrowModel.PersonInfo.Company, CompanyCode = BorrowModel.PersonInfo.CompanyCode, CostCenter = BorrowModel.PersonInfo.CostCenter, Department = BorrowModel.PersonInfo.Department, DepartmentCode = BorrowModel.PersonInfo.DepartmentCode, IsHeadOffice = BorrowModel.PersonInfo.IsHeadOffice, Shop = BorrowModel.PersonInfo.Shop, ShopCode = BorrowModel.PersonInfo.ShopCode };
                    model.Owner = BorrowModel.Owner;
                    model.WorkNumber = BorrowModel.WorkNumber;
                    model.Remark = BorrowModel.Remark;
                    model.TransactionDate = BorrowModel.TransactionDate;
                    if (!string.IsNullOrEmpty(IsCopy))
                    {
                        model.IsCopy = 1;

                        //复制即累加次数
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("CopyCount", (model.CopyCount + 1).ToString());
                        var status = BB_Bl.PublicEditMethod(model.BillNo, dic);
                    }
                }
            }
            model.PageName = "Borrow";
            model.CommitType = CommitType.借款单;
            return View(model);
        }

        /// <summary>
        /// 提交单据
        /// </summary>
        /// <param name="postFeeBill"></param>
        /// <returns></returns>
        [HttpPost]
        public string CreateBorrowBill(BorrowBillModel postBorrowBill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postBorrowBill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postBorrowBill.Items == null || postBorrowBill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postBorrowBill.TotalMoney = postBorrowBill.Items.Sum(all => all.money) + postBorrowBill.Items.Sum(all => all.taxmoney);
            }
            if (postBorrowBill.Photos == null || postBorrowBill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }
            if (postBorrowBill.SpecialAttribute.MarketDebt != 1 && postBorrowBill.SpecialAttribute.BankDebt != 1 && postBorrowBill.SpecialAttribute.Cash != 1)
            {
                if (postBorrowBill.CollectionInfo == null || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.CardCode) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.Name) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.SubbranchBank) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.SubbranchBankCode))
                {
                    result = "收款信息缺失";
                    return result;
                }
            }

            foreach (var item in postBorrowBill.Items)
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

            var obj = GetBrandFromCosterCenterNew(postBorrowBill.PersonInfo.CostCenter);
            if (postBorrowBill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postBorrowBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)  //分品牌
                    {
                        if (postBorrowBill.PersonInfo.Brand == null || postBorrowBill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postBorrowBill.PersonInfo.Brand)
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
                if (!string.IsNullOrEmpty(postBorrowBill.PersonInfo.ShopCode))
                {
                    postBorrowBill.ShopLogo = DbContext.SHOP.Where(c => c.CODE == postBorrowBill.PersonInfo.ShopCode).Select(x => x.SHOPLOGO).FirstOrDefault();
                }
            }
            try
            {
                var lable = PublicDemand(postBorrowBill.PersonInfo.IsHeadOffice, postBorrowBill.BillsType, postBorrowBill.PersonInfo.DepartmentCode, obj, postBorrowBill.Items, postBorrowBill.SpecialAttribute, postBorrowBill.DepartmentName);

                //PackClass PackString = new PackClass() { Creator = postBorrowBill.Creator, BillsType = postBorrowBill.BillsType, Brand = postBorrowBill.PersonInfo.Brand, CompanyCode = postBorrowBill.PersonInfo.CompanyCode, CostCenter = postBorrowBill.PersonInfo.CostCenter, Department = postBorrowBill.PersonInfo.Department, DepartmentCode = postBorrowBill.PersonInfo.DepartmentCode, IsHeadOffice = postBorrowBill.PersonInfo.IsHeadOffice, Items = postBorrowBill.Items.Select(x => x.name).ToList(), IsUrgent = postBorrowBill.IsUrgent };

                //Dictionary<string, string> dic = new Dictionary<string, string>();

                //string pack = Public.JsonSerializeHelper.SerializeToJson(PackString);

                //dic.Add("pack", pack);

                //string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                string MaxNumber = new BorrowBill().GenerateMaxBillNo();



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
                    objectID = proxy.NewWorkFlowInstance(lable.CODE, postBorrowBill.Creator, MaxNumber, dicString);
                }
                catch
                {
                    DeleteInvalidBillNo(MaxNumber);
                    result = "服务内部出错，请联系数控中心";
                    return result;
                }

                postBorrowBill.CreateTime = DateTime.Now;
                postBorrowBill.Status = 0;
                postBorrowBill.WorkFlowID = objectID;
                postBorrowBill.BillsItems = postBorrowBill.Items;
                postBorrowBill.SurplusMoney = postBorrowBill.TotalMoney; //待还总额
                postBorrowBill.BillNo = MaxNumber;
                new BorrowBill().CreateBorrowBill(postBorrowBill);
                result = MaxNumber;

            }
            catch
            {

                result = "Fail";
            }
            return result;
        }

        [HttpPost]
        public string EditBorrowBillContent(BorrowBillModel postBorrowBill)
        {
            this.Request.Url.AbsoluteUri.ToString();

            string result = "Fail";
            //检查
            if (string.IsNullOrEmpty(postBorrowBill.COST_ACCOUNT))
            {
                result = "成本中心为空";
                return result;
            }

            if (postBorrowBill.Items == null || postBorrowBill.Items.Count < 1)
            {
                result = "缺少报销项";
                return result;
            }
            else
            {
                postBorrowBill.TotalMoney = postBorrowBill.Items.Sum(all => all.money) + postBorrowBill.Items.Sum(all => all.taxmoney);
            }
            if (postBorrowBill.Photos == null || postBorrowBill.Photos.Count < 1)
            {
                result = "缺少发票照片";
                return result;
            }
            if (postBorrowBill.SpecialAttribute.MarketDebt != 1 && postBorrowBill.SpecialAttribute.BankDebt != 1 && postBorrowBill.SpecialAttribute.Cash != 1)
            {
                if (postBorrowBill.CollectionInfo == null || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.CardCode) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.Name) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.SubbranchBank) || string.IsNullOrEmpty(postBorrowBill.CollectionInfo.SubbranchBankCode))
                {
                    result = "收款信息缺失";
                    return result;
                }
            }


            var obj = GetBrandFromCosterCenterNew(postBorrowBill.PersonInfo.CostCenter);
            if (postBorrowBill.PersonInfo.IsHeadOffice == 0)
            {
                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postBorrowBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (DicModel != null)
                {
                    if (DicModel.PARTBRAND == 1)  //分品牌
                    {
                        if (postBorrowBill.PersonInfo.Brand == null || postBorrowBill.PersonInfo.Brand.Count < 1)
                        {
                            result = "记账品牌至少选择一项！";
                            return result;
                        }
                        int brandcount = 0;
                        foreach (var item in postBorrowBill.PersonInfo.Brand)
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

                var lable = PublicDemand(postBorrowBill.PersonInfo.IsHeadOffice, postBorrowBill.BillsType, postBorrowBill.PersonInfo.DepartmentCode, obj, postBorrowBill.Items, postBorrowBill.SpecialAttribute, postBorrowBill.DepartmentName);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                var oldModel = new BorrowBill().GetBillModel(postBorrowBill.BillNo);
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                string objectID = proxy.NewWorkFlowInstance(lable.CODE, postBorrowBill.Creator, oldModel.BillNo, dicString);


                postBorrowBill.BillNo = oldModel.BillNo;
                postBorrowBill.Id = oldModel.Id;
                postBorrowBill.WorkFlowID = objectID;
                postBorrowBill.CreateTime = oldModel.CreateTime;
                postBorrowBill.Status = oldModel.Status;
                postBorrowBill.ApprovalPost = oldModel.ApprovalPost;
                postBorrowBill.ApprovalStatus = oldModel.ApprovalStatus;
                postBorrowBill.ApprovalTime = oldModel.ApprovalTime;
                postBorrowBill.SurplusMoney = postBorrowBill.TotalMoney;
                postBorrowBill.SpecialAttribute = postBorrowBill.SpecialAttribute;
                postBorrowBill.BillsItems = postBorrowBill.Items;

                string status = new BorrowBill().EditBorrowBill(postBorrowBill);
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
    }
}
