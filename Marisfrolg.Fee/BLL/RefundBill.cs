using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;


namespace Marisfrolg.Fee.BLL
{

    public class RefundBill : BillBase, IRefundBill, IMaxNo
    {

        public RefundBill()
            : base(BillType.还款单)
        {
        }

        public RefundBillModel GetBillModel(string BillNo)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo).FirstOrDefault();
            return model;
        }


        public RefundBillModel GetBillModel(string BillNo, string No)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.Creator == No).FirstOrDefault();
            return model;
        }


        public List<string> GetAbnormalModel()
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && string.IsNullOrEmpty(c.ApprovalPost)).Project(c => c.WorkFlowID).ToList();
            return model;
        }

        public List<RefundBillModel> GetBillModelByNo(string No)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Creator == No).ToList();
            return model;
        }

        public RefundBillModel GetBillModelPlus(string BillNo, int IsHeadOffice)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.PersonInfo.IsHeadOffice == IsHeadOffice).FirstOrDefault();
            return model;
        }

        public RefundBillModel GetBillModel(string BillNo, List<string> str)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo && str.Contains(c.PersonInfo.DepartmentCode)).FirstOrDefault();
            return model;
        }

        public List<FEE_Report> GetFeeReportData(DateTime startTime, DateTime endTime, List<string> SHopCodeList, List<string> companyCode)
        {
            if (SHopCodeList.Count > 0)
            {
                var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && SHopCodeList.Contains(c.PersonInfo.DepartmentCode) && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && c.RefundType == "FeeBill" && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;
            }
            else
            {
                var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && c.RefundType == "FeeBill" && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;
            }
        }


        /// <summary>
        /// 查看还款记录
        /// </summary>
        /// <param name="BorrowBillNo"></param>
        /// <returns></returns>
        public List<RefundBillModel> GetRefundRecode(string BorrowBillNo)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BorrowBillNo == BorrowBillNo).ToList();
            return model;
        }

        /// <summary>
        /// 批量获取我审批的单据（对应具体的单据）
        /// </summary>
        public List<RefundBillModel> GetMyProcess(string EmployeeNo, List<string> IdList)
        {
            //不显示总部现金冲账
            var model = MongoDBHelper.RefundBill.Find(c => IdList.Contains(c.WorkFlowID) && c.Status == 0 && c.Flag == 0).SortByDescending(x => x.CreateTime).ToList();
            return model;
        }


        public override string GetMaxBillNo()
        {
            var maxBill = MongoDBHelper.RefundBill.Find(c => c.CreateTime >= DateTime.Now.Date && c.CreateTime < DateTime.Now.Date.AddDays(1) && c.RefundType.ToUpper() == "CASH").SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();

            return maxBill == null ? "" : maxBill.BillNo;
        }

        /// <summary>
        /// 获取单据
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="PageIndex">页码数</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalNumber">总数量</param>
        /// <returns></returns>
        public List<RefundBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int PageIndex, int pageSize, out int totalNumber)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            //单据类型           

            totalNumber = Convert.ToInt32(MongoDBHelper.RefundBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0).Count());
            var list = MongoDBHelper.RefundBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).Skip((PageIndex - 1) * pageSize).Limit(pageSize).ToList();
            return list;
        }


        /// <summary>
        /// 根据时间获取所有数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<RefundBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int Belong, List<string> DepartmentCode, string CheckStatus = "", string PrintStatus = "")
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            List<RefundBillModel> list = new List<RefundBillModel>();

            if (Belong == 1 || Belong == 3)
            {
                if (DepartmentCode.Count > 0)
                {
                    list = MongoDBHelper.RefundBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && c.Flag == 0 && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
                }
                else
                {
                    list = MongoDBHelper.RefundBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && c.Flag == 0).SortByDescending(x => x.CreateTime).ToList();
                }
                if (Belong == 3)
                {
                    list = list.Where(c => c.RecycleBin == 1).ToList();
                }
                else
                {
                    list = list.Where(c => c.RecycleBin == 0).ToList();
                }
            }
            else if (Belong == 2)
            {
                list = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && c.Flag == 0 && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
            }

            if (!string.IsNullOrEmpty(CheckStatus))
            {
                List<string> condition = CheckStatus.Split(',').ToList();
                int status = Convert.ToInt32(condition[0]);

                if (status == 0 || status == 1 || status == 4)
                {
                    list = list.Where(c => c.ApprovalStatus == status && !string.IsNullOrEmpty(c.ApprovalPost) && c.ApprovalPost.Contains(condition[1])).ToList();
                }
                else
                {
                    list = list.Where(c => c.ApprovalStatus == status).ToList();
                }
            }
            if (!string.IsNullOrEmpty(PrintStatus))
            {
                int count = Convert.ToInt32(PrintStatus);
                if (count > 0)
                {
                    list = list.Where(c => c.PrintedCount > 0).ToList();
                }
                else
                {
                    list = list.Where(c => c.PrintedCount == 0).ToList();
                }
            }
            return list;
        }

        public void CreateRefundBill(RefundBillModel pFeeBill)
        {
            //if (pFeeBill.RefundType.ToUpper() == "FEEBILL")
            //{
            //    pFeeBill.BillNo = new FeeBill().GenerateMaxBillNo();
            //}
            //else
            //{
            //    pFeeBill.BillNo = GenerateMaxBillNo();
            //}
            MongoDBHelper.RefundBill.InsertOne(pFeeBill);
        }

        public void GetRefundBill(string pID)
        {

        }
        /// <summary>
        /// 修改费用单审批状态
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus"></param>
        /// <returns></returns>
        public string ChangeBillStatus(string WorkFlowID, int ApprovalStatus)
        {
            if (ApprovalStatus == 2 || ApprovalStatus == 3)
            {
                var filter = Builders<RefundBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<RefundBillModel>.Update.Set("ApprovalStatus", ApprovalStatus).Set("ApprovalTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
            else
            {
                if (ApprovalStatus == 4)
                {
                    int status = MongoDBHelper.RefundBill.Find(c => c.WorkFlowID == WorkFlowID).Project(x => x.ApprovalStatus).FirstOrDefault();
                    if (status == 4)
                    {
                        return "Success";
                    }
                }
                var filter = Builders<RefundBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<RefundBillModel>.Update.Set("ApprovalStatus", ApprovalStatus);
                var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
        }


        /// <summary>
        /// 修改还款单审批岗位和办结时间
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus"></param>
        /// <param name="PostString"></param>
        /// <returns></returns>
        public string ChangeRefundBillApprovalPost(string WorkFlowID, string PostString)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
            var update = Builders<RefundBillModel>.Update.Set("ApprovalPost", PostString);
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result.ModifiedCount > 0 ? "Success" : "Fail";
        }

        /// <summary>
        /// 根据WorkFlowID获取对象
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <returns></returns>
        public RefundBillModel GetModelFromWorkFlowID(string WorkFlowID)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.WorkFlowID == WorkFlowID).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 获取未办结的单
        /// </summary>
        /// <param name="Creator"></param>
        /// <returns></returns>
        public List<RefundBillModel> GetNotFinishBill(string Creator)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.Creator == Creator && c.ApprovalStatus != 3 && c.ApprovalStatus != 2).ToList();
            return model;
        }

        ///// <summary>
        ///// 处理借款单剩余金额
        ///// </summary>
        ///// <param name="BillNo"></param>
        ///// <param name="Money"></param>
        ///// <returns></returns>
        //public string EditRefundBill(string BillNo)
        //{
        //    var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
        //    var update = Builders<RefundBillModel>.Update.Set("OutDebt", 0);
        //    var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
        //    return result != null && result.ModifiedCount > 0 ? "Success" : "";
        //}


        /// <summary>
        /// 费用还款单编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveRefundBillChange(EditBillModel model)
        {
            var newModel = MongoDBHelper.RefundBill.Find(c => c.BillNo == model.BillNo).FirstOrDefault();
            if (newModel != null && newModel.RefundType != "Cash")
            {
                newModel.PersonInfo.Brand = model.Brand;
                newModel.PersonInfo.CostCenter = model.CostCenter;
                newModel.SpecialAttribute = model.SpecialAttribute;
                newModel.Currency = model.Currency;
                newModel.Items = model.Items;
                newModel.BillsItems = model.BillsItems;
                newModel.RealRefundMoney = model.Items.Sum(all => all.money) + model.Items.Sum(all => all.taxmoney);
                newModel.Photos = model.Photos;
                newModel.COST_ACCOUNT = model.CostCenter;
                newModel.PersonInfo.Department = model.DPName;
                newModel.PersonInfo.DepartmentCode = model.DPID;
                newModel.PersonInfo.Shop = model.ShopName;
                newModel.PersonInfo.ShopCode = model.ShopCode;
                newModel.Remark = model.Remark;
                newModel.PersonInfo.Company = model.E_Company;
                newModel.PersonInfo.CompanyCode = model.E_CompanyCode;

                var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", model.BillNo);
                var result = MongoDBHelper.RefundBill.FindOneAndReplace(filter, newModel);
                return result != null ? "Success" : "Fail";
            }
            return "Fail";
        }

        /// <summary>
        /// 删除借款单
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string DelectRefundBill(string BillNo)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<RefundBillModel>.Update.Set("Status", 1);
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        /// <summary>
        /// 获取冲销记录
        /// </summary>
        /// <returns></returns>
        public RefundBillModel GetOffsetRecord(string BillNo)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.OffsetBillNo == BillNo && c.RefundType == "Cash" && c.Flag == 1 && c.ApprovalStatus == 2).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 编辑单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string EditRefundBill(RefundBillModel model)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", model.BillNo);
            var result = MongoDBHelper.RefundBill.FindOneAndReplace(filter, model);
            return result != null ? "Success" : "Fail";
        }


        public List<RefundBillModel> GetRelationBillList(string itemName, string DepartmentCode, string ShopCode, DateTime BeginTime)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.PersonInfo.DepartmentCode == DepartmentCode && c.CreateTime >= BeginTime && c.RefundType == "FeeBill" && c.PersonInfo.ShopCode == ShopCode).ToList();
            if (model.Count > 0)
            {
                foreach (var item in model)
                {
                    if (item.Items != null && item.Items.Count > 0)
                    {
                        var temp = item.Items.Where(c => c.name == itemName).FirstOrDefault();
                        if (temp == null)
                        {
                            item.Status = 1;
                        }
                        else
                        {
                            item.RealRefundMoney = temp.money + temp.taxmoney;
                        }
                    }
                    else
                    {
                        item.Status = 1;
                    }
                }
                model = model.Where(c => c.Status == 0).ToList();
            }
            return model;
        }

        public string AddPrintedNum(string BillNo)
        {
            var OldNum = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo).Project(x => x.PrintedCount).FirstOrDefault();
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<RefundBillModel>.Update.Set("PrintedCount", OldNum + 1);
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<RefundBillModel> CountSmallSortNum(int DepartmentSort, List<string> AreaList, List<string> AccountInfoList, List<int> BillTypesList, DateTime BeginTime, DateTime EndTime, string IsFund, string AreaCodeAndShopCode, List<string> MyCompanycode, string ReportType)
        {
            List<RefundBillModel> Model = new List<RefundBillModel>();
            List<RefundBillModel> DeleteModel = new List<RefundBillModel>();
            if (DepartmentSort == 3)
            {
                Model = MongoDBHelper.RefundBill.Find(c => AreaList.Contains(c.PersonInfo.DepartmentCode) && c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else if (DepartmentSort == 2)
            {
                Model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else
            {
                Model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.PersonInfo.IsHeadOffice == DepartmentSort && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            if (ReportType == "4" || ReportType == "7")
            {
                Model = Model.Where(c => c.RefundType == "FeeBill").ToList();
            }
            if (BillTypesList.Count > 0)
            {
                Model = Model.Where(c => BillTypesList.Contains(c.ApprovalStatus)).ToList();
            }
            if (AccountInfoList.Count > 0)
            {
                foreach (var item in Model)
                {
                    var IsDelete = true;
                    if (item.Items == null)
                    {
                        DeleteModel.Add(item);
                        continue;
                    }
                    foreach (var Titem in item.Items)
                    {
                        if (AccountInfoList.Contains(Titem.name))
                        {
                            IsDelete = false;
                            break;
                        }
                    }
                    if (IsDelete)
                    {
                        DeleteModel.Add(item);
                    }
                }
                if (DeleteModel.Count > 0)
                {
                    foreach (var item in DeleteModel)
                    {
                        Model.Remove(item);
                    }
                }
            }

            if (!string.IsNullOrEmpty(AreaCodeAndShopCode))
            {
                var list = AreaCodeAndShopCode.Split(',').ToList();
                list.Remove("");
                if (list.Count > 0 && list.Count < 3)
                {
                    Model = Model.Where(c => c.Items.Where(x => x.name == list[0]).FirstOrDefault() != null && c.PersonInfo.Department == list[1]).ToList();
                }
                else if (list.Count == 3)
                {
                    Model = Model.Where(c => c.Items.Where(x => x.name == list[0]).FirstOrDefault() != null && c.PersonInfo.Department == list[1] && c.PersonInfo.ShopCode == list[2]).ToList();
                }
            }
            if (MyCompanycode.Count > 0)
            {
                Model = Model.Where(c => MyCompanycode.Contains(c.PersonInfo.CompanyCode)).ToList();
            }

            return Model;
        }


        public long TransferBill(string No1, string No2, string Name)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("Creator", No1) & Builders<RefundBillModel>.Filter.Where(c => c.ApprovalStatus != 3 && c.Status == 0);
            var update = Builders<RefundBillModel>.Update.Set("Creator", No2).Set("WorkNumber", No2).Set("Owner", Name);
            var result = MongoDBHelper.RefundBill.UpdateMany(filter, update);
            return result.MatchedCount;
        }


        public string PublicEditMethod(string BillNo, Dictionary<string, string> dic)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<RefundBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<RefundBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod<T>(string BillNo, Dictionary<string, T> dic) where T : class
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<RefundBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<RefundBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod(string BillNo, Dictionary<string, List<PostDescription>> dic)
        {
            var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<RefundBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<RefundBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public void InsertOneData(RefundBillModel Model)
        {
            MongoDBHelper.RefundBill.InsertOne(Model);
        }


        public List<RefundBillModel> ReturnShelveNo(string EmployeeNo)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.ShelveNo == EmployeeNo).ToList();
            return model;
        }


        /// <summary>
        /// 获取办结数据
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="endTime"></param>
        /// <param name="PostNotNull">流程是否为空</param>
        /// <returns></returns>
        public List<RefundBillModel> GetCompleteBill(string createTime, string endTime, bool PostNotNull)
        {

            DateTime T1 = string.IsNullOrEmpty(createTime) == true ? new DateTime() : Convert.ToDateTime(createTime);
            DateTime T2 = string.IsNullOrEmpty(endTime) == true ? new DateTime(2099, 1, 1) : Convert.ToDateTime(endTime);

            if (PostNotNull)
            {
                return MongoDBHelper.RefundBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString != null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
            else
            {
                return MongoDBHelper.RefundBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString == null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
        }
    }
}