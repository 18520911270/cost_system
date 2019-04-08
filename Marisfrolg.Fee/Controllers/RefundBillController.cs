using Marisfrolg.Fee.BLL;
using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marisfrolg.Fee.Extention;

namespace Marisfrolg.Fee.Controllers
{
    public class RefundBillController : SecurityController
    {
        //
        // GET: /RefundBill/

        public ActionResult RefundBill()
        {
            return View();
        }

        /// <summary>
        /// 还款界面
        /// </summary>
        /// <param name="BorrowNo">借款单号</param>
        /// <param name="RefundType">还款类型</param>
        /// <returns></returns>
        public ActionResult RefundOperate(string BorrowNo = "", string RefundType = "", string BillNo = "", string Mode = "", string IsCopy = null)
        {
            RefundBill RB_Bl = new BLL.RefundBill();

            RefundBillModel oldModel = new RefundBillModel();
            if (string.IsNullOrEmpty(BorrowNo) && !string.IsNullOrEmpty(BillNo))
            {
                oldModel = RB_Bl.GetBillModel(BillNo);
                BorrowNo = oldModel.BorrowBillNo;
            }
            var BorrowModel = new BorrowBill().GetBillModel(BorrowNo);
            FeeBillModelRef model = new FeeBillModelRef();
            model.PageName = "Refund";
            model.CommitType = CommitType.还款单;
            model.RefundType = RefundType;
            model.SurplusMoney = BorrowModel.SurplusMoney;
            model.Owner = BorrowModel.Owner;
            model.WorkNumber = BorrowModel.WorkNumber;
            model.BorrowBillNo = BorrowModel.BillNo;
            model.Currency = BorrowModel.Currency;

            model.PersonInfo = new PersonInfo()
            {
                Company = BorrowModel.PersonInfo.Company,
                CompanyCode = BorrowModel.PersonInfo.CompanyCode,
                Department = BorrowModel.PersonInfo.Department,
                DepartmentCode = BorrowModel.PersonInfo.DepartmentCode,
                IsHeadOffice = BorrowModel.PersonInfo.IsHeadOffice,
                CostCenter = BorrowModel.PersonInfo.CostCenter,
                Shop = BorrowModel.PersonInfo.Shop,
                ShopCode = BorrowModel.PersonInfo.ShopCode
            };

            model.ModelString = Public.JsonSerializeHelper.SerializeToJson(model);
            if (!string.IsNullOrEmpty(BillNo))
            {
                model.BillNo = BillNo;
                model.BillsItems = oldModel.BillsItems;
                model.PersonInfo.Brand = oldModel.PersonInfo.Brand;
                model.SpecialAttribute = oldModel.SpecialAttribute;
                model.Remark = oldModel.Remark;
                model.CountTime = oldModel.CountTime;
                model.Photos = oldModel.Photos;
                model.RefundType = oldModel.RefundType;
                model.TransactionDate = oldModel.TransactionDate;
                if (!string.IsNullOrEmpty(IsCopy))
                {
                    model.IsCopy = 1;

                    //复制即累加次数
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("CopyCount", (model.CopyCount + 1).ToString());
                    var status = RB_Bl.PublicEditMethod(model.BillNo, dic);
                }
                model.ModelString = Public.JsonSerializeHelper.SerializeToJson(oldModel);
            }

            return View(model);
        }

        /// <summary>
        /// 提交还款单
        /// </summary>
        /// <param name="postRefundBill"></param>
        /// <returns></returns>
        [HttpPost]
        public string CreateRefundBill(RefundBillModel postRefundBill)
        {
            this.Request.Url.AbsoluteUri.ToString();
            string result = "Fail";
            //费用报销单还款

            var obj = GetBrandFromCosterCenterNew(postRefundBill.PersonInfo.CostCenter);
            if (postRefundBill.RefundType.ToUpper() == "FEEBILL")
            {
                //检查
                if (string.IsNullOrEmpty(postRefundBill.COST_ACCOUNT))
                {
                    result = "成本中心为空";
                    return result;
                }

                if (postRefundBill.Items == null || postRefundBill.Items.Count < 1)
                {
                    result = "缺少报销项";
                    return result;
                }
                else
                {
                    postRefundBill.RealRefundMoney = postRefundBill.Items.Sum(all => all.money) + postRefundBill.Items.Sum(all => all.taxmoney);
                }
                if (postRefundBill.Photos == null || postRefundBill.Photos.Count < 1)
                {
                    result = "缺少发票照片";
                    return result;
                }

                foreach (var item in postRefundBill.Items)
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

                if (postRefundBill.PersonInfo.IsHeadOffice == 0)
                {
                    var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postRefundBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                    if (DicModel != null)
                    {
                        if (DicModel.PARTBRAND == 1)
                        {
                            if (postRefundBill.PersonInfo.Brand == null || postRefundBill.PersonInfo.Brand.Count < 1)
                            {
                                result = "记账品牌至少选择一项！";
                                return result;
                            }
                            int brandcount = 0;
                            foreach (var item in postRefundBill.PersonInfo.Brand)
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
                    if (!string.IsNullOrEmpty(postRefundBill.PersonInfo.ShopCode))
                    {
                        postRefundBill.ShopLogo = DbContext.SHOP.Where(c => c.CODE == postRefundBill.PersonInfo.ShopCode).Select(x => x.SHOPLOGO).FirstOrDefault();
                    }
                }
            }
            //现金还款
            else
            {
                if (postRefundBill.RealRefundMoney < 0)
                {
                    result = "还款金额错误";
                    return result;
                }

            }
            try
            {
                var BorrowModel = new BorrowBill().GetBillModel(postRefundBill.BorrowBillNo);
                //费用还款可以还款超出欠款额度，只须拆成两单      
                if (postRefundBill.RealRefundMoney > BorrowModel.SurplusMoney && postRefundBill.RefundType.ToUpper() != "FEEBILL")
                {
                    result = "还款金额超出";
                    return result;
                }
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();

                string MaxNumber = string.Empty;

                if (postRefundBill.RefundType.ToUpper() == "CASH")
                {
                    postRefundBill.BillsType = "HK1";
                    MaxNumber = new RefundBill().GenerateMaxBillNo();
                }
                else
                {
                    MaxNumber = new FeeBill().GenerateMaxBillNo();
                }

                var lable = PublicDemand(postRefundBill.PersonInfo.IsHeadOffice, postRefundBill.BillsType, postRefundBill.PersonInfo.DepartmentCode, obj, postRefundBill.Items, postRefundBill.SpecialAttribute, postRefundBill.DepartmentName);

                //PackClass PackString = new PackClass() { Creator = postRefundBill.Creator, BillsType = postRefundBill.BillsType, Brand = postRefundBill.PersonInfo.Brand, CompanyCode = postRefundBill.PersonInfo.CompanyCode, CostCenter = postRefundBill.PersonInfo.CostCenter, Department = postRefundBill.PersonInfo.Department, DepartmentCode = postRefundBill.PersonInfo.DepartmentCode, IsHeadOffice = postRefundBill.PersonInfo.IsHeadOffice, Items = postRefundBill.Items == null ? null : postRefundBill.Items.Select(x => x.name).ToList() };

                //Dictionary<string, string> dic = new Dictionary<string, string>();

                //string pack = Public.JsonSerializeHelper.SerializeToJson(PackString);

                //dic.Add("pack", pack);

                //string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

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

                string objectID = string.Empty;

                try
                {
                    objectID = proxy.NewWorkFlowInstance(lable.CODE, postRefundBill.Creator, MaxNumber, dicString);
                }
                catch
                {
                    DeleteInvalidBillNo(MaxNumber);
                    result = "服务内部出错，请联系数控中心";
                    return result;
                }

                postRefundBill.CreateTime = DateTime.Now;
                postRefundBill.Status = 0;
                postRefundBill.WorkFlowID = objectID;
                postRefundBill.BillsItems = postRefundBill.Items;
                postRefundBill.BillNo = MaxNumber;

                new RefundBill().CreateRefundBill(postRefundBill);

                result = MaxNumber;
            }
            catch
            {

                result = "Fail";
            }


            return result;
        }

        public ActionResult CheckRefundRecode(string BorrowNo)
        {
            var BorrowModel = new BorrowBill().GetBillModel(BorrowNo);
            var RefundBillList = new RefundBill().GetRefundRecode(BorrowNo);
            List<RefundBillModelRef> ModelList = new List<RefundBillModelRef>();
            if (RefundBillList != null && RefundBillList.Count > 0)
            {
                foreach (var item in RefundBillList)
                {
                    var Temp = item.MapTo<RefundBillModel, RefundBillModelRef>();
                    Temp.SurplusMoney = BorrowModel.SurplusMoney;
                    Temp.TotalMoney = BorrowModel.TotalMoney;
                    Temp.StringTime = item.TransactionDate.ToString("yyyy-MM-dd");
                    ModelList.Add(Temp);
                }
            }
            ViewData["ModelString"] = Public.JsonSerializeHelper.SerializeToJson(ModelList);
            return View();
        }



        public int CheckRefundRecodeCount(string BorrowNo)
        {
            var BorrowModel = new BorrowBill().GetBillModel(BorrowNo);
            var RefundBillList = new RefundBill().GetRefundRecode(BorrowNo);
            return RefundBillList.Count;
        }


        [HttpPost]
        public string EditRefundBillContent(RefundBillModel postRefundBill)
        {
            this.Request.Url.AbsoluteUri.ToString();
            string result = "Fail";
            var obj = GetBrandFromCosterCenterNew(postRefundBill.PersonInfo.CostCenter);
            if (postRefundBill.RefundType.ToUpper() == "FEEBILL")
            {
                if (string.IsNullOrEmpty(postRefundBill.COST_ACCOUNT))
                {
                    result = "成本中心为空";
                    return result;
                }

                if (postRefundBill.Items == null || postRefundBill.Items.Count < 1)
                {
                    result = "缺少报销项";
                    return result;
                }
                else
                {
                    postRefundBill.RealRefundMoney = postRefundBill.Items.Sum(all => all.money) + postRefundBill.Items.Sum(all => all.taxmoney);
                }
                if (postRefundBill.Photos == null || postRefundBill.Photos.Count < 1)
                {
                    result = "缺少发票照片";
                    return result;
                }

                var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == postRefundBill.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                if (postRefundBill.PersonInfo.IsHeadOffice == 0)
                {
                    if (DicModel != null)
                    {
                        if (DicModel.PARTBRAND == 1)
                        {
                            if (postRefundBill.PersonInfo.Brand == null || postRefundBill.PersonInfo.Brand.Count < 1)
                            {
                                result = "记账品牌至少选择一项！";
                                return result;
                            }
                            int brandcount = 0;
                            foreach (var item in postRefundBill.PersonInfo.Brand)
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
            }
            //现金还款
            else
            {
                if (postRefundBill.RealRefundMoney < 0)
                {
                    result = "还款金额错误";
                    return result;
                }

            }
            try
            {
                var BorrowModel = new BorrowBill().GetBillModel(postRefundBill.BorrowBillNo);
                //费用还款可以还款超出欠款额度，只须拆成两单      
                if (postRefundBill.RealRefundMoney > BorrowModel.SurplusMoney && postRefundBill.RefundType.ToUpper() != "FEEBILL")
                {
                    result = "还款金额超出";
                    return result;
                }

                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                if (postRefundBill.RefundType.ToUpper() == "CASH")
                {
                    postRefundBill.BillsType = "HK1";
                }

                var lable = PublicDemand(postRefundBill.PersonInfo.IsHeadOffice, postRefundBill.BillsType, postRefundBill.PersonInfo.DepartmentCode, obj, postRefundBill.Items, postRefundBill.SpecialAttribute, postRefundBill.DepartmentName);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                string dicString = Public.JsonSerializeHelper.SerializeToJson(dic);

                var oldModel = new RefundBill().GetBillModel(postRefundBill.BillNo);
                string objectID = proxy.NewWorkFlowInstance(lable.CODE, postRefundBill.Creator, oldModel.BillNo, dicString);


                postRefundBill.BillNo = oldModel.BillNo;
                postRefundBill.Id = oldModel.Id;
                postRefundBill.WorkFlowID = objectID;
                postRefundBill.CreateTime = oldModel.CreateTime;
                postRefundBill.Status = oldModel.Status;
                postRefundBill.ApprovalPost = oldModel.ApprovalPost;
                postRefundBill.ApprovalStatus = oldModel.ApprovalStatus;
                postRefundBill.ApprovalTime = oldModel.ApprovalTime;
                postRefundBill.BillsItems = postRefundBill.Items;

                string status = new RefundBill().EditRefundBill(postRefundBill);
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
