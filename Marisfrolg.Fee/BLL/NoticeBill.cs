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

    public class NoticeBill : BillBase, INoticeBill, IMaxNo
    {

        public NoticeBill()
            : base(BillType.付款通知书)
        {

        }
        public override string GetMaxBillNo()
        {
            var maxBill = MongoDBHelper.NoticeBill.Find(c => c.CreateTime >= DateTime.Now.Date && c.CreateTime < DateTime.Now.Date.AddDays(1)).SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();

            return maxBill == null ? "" : maxBill.BillNo;
        }

        public void GetNoticeBill(string pID)
        {

        }

        public void CreateNoticeBill(NoticeBillModel pNoticeBill)
        {
            //pNoticeBill.BillNo = GenerateMaxBillNo();
            MongoDBHelper.NoticeBill.InsertOne(pNoticeBill);
        }


        public List<FEE_Report> GetFeeReportData(DateTime startTime, DateTime endTime, List<string> SHopCodeList,List<string> companyCode)
        {
            if (SHopCodeList.Count > 0)
            {
                var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && SHopCodeList.Contains(c.PersonInfo.DepartmentCode) && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;
            }
            else
            {
                var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;

            }

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
        public List<NoticeBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int PageIndex, int pageSize, out int totalNumber)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            //单据类型           

            totalNumber = Convert.ToInt32(MongoDBHelper.NoticeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0).Count());
            var list = MongoDBHelper.NoticeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).Skip((PageIndex - 1) * pageSize).Limit(pageSize).ToList();
            return list;
        }

        public List<string> GetAbnormalModel()
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && string.IsNullOrEmpty(c.ApprovalPost)).Project(c => c.WorkFlowID).ToList();
            return model;
        }

        public NoticeBillModel GetBillModel(string BillNo)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.BillNo == BillNo).FirstOrDefault();
            return model;
        }

        public NoticeBillModel GetBillModelPlus(string BillNo, int IsHeadOffice)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.PersonInfo.IsHeadOffice == IsHeadOffice).FirstOrDefault();
            return model;
        }


        public NoticeBillModel GetBillModel(string BillNo, string No)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.Creator == No).FirstOrDefault();
            return model;
        }

        public NoticeBillModel GetBillModel(string BillNo, List<string> str)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && str.Contains(c.PersonInfo.DepartmentCode)).FirstOrDefault();
            return model;
        }

        public List<NoticeBillModel> GetBillModelByNo(string No)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Creator == No).ToList();
            return model;
        }

        /// <summary>
        /// 根据时间获取所有数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<NoticeBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int Belong, List<string> DepartmentCode, string CheckStatus = "", string PrintStatus = "")
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            List<NoticeBillModel> list = new List<NoticeBillModel>();
            if (Belong == 1 || Belong == 3)
            {
                if (DepartmentCode.Count > 0)
                {
                    list = MongoDBHelper.NoticeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
                }
                else
                {
                    list = MongoDBHelper.NoticeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).ToList();
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
                list = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();

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

        /// <summary>
        /// 批量获取我审批的单据（对应具体的单据）
        /// </summary>
        public List<NoticeBillModel> GetMyProcess(string EmployeeNo, List<string> IdList)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => IdList.Contains(c.WorkFlowID) && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
            return model;
        }


        /// <summary>
        /// 获取发票缺失
        /// </summary>
        /// <param name="No"></param>
        /// <returns></returns>
        public List<NoticeBillModel> GetMissBill(string EmployeeNo)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.Creator == EmployeeNo && c.MissBill == 0 && c.ApprovalStatus == 2).ToList();
            return model;
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
                var filter = Builders<NoticeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<NoticeBillModel>.Update.Set("ApprovalStatus", ApprovalStatus).Set("ApprovalTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
            else
            {
                if (ApprovalStatus == 4)
                {
                    int status = MongoDBHelper.NoticeBill.Find(c => c.WorkFlowID == WorkFlowID).Project(x => x.ApprovalStatus).FirstOrDefault();
                    if (status == 4)
                    {
                        return "Success";
                    }
                }
                var filter = Builders<NoticeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<NoticeBillModel>.Update.Set("ApprovalStatus", ApprovalStatus);
                var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
        }


        /// <summary>
        /// 修改付款通知书审批岗位和办结时间
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus"></param>
        /// <param name="PostString"></param>
        /// <returns></returns>
        public string ChangeNoticeBillApprovalPost(string WorkFlowID, string PostString)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
            var update = Builders<NoticeBillModel>.Update.Set("ApprovalPost", PostString);
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result.ModifiedCount > 0 ? "Success" : "Fail";
        }

        /// <summary>
        /// 根据WorkFlowID获取对象
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <returns></returns>
        public NoticeBillModel GetModelFromWorkFlowID(string WorkFlowID)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.WorkFlowID == WorkFlowID).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 获取未办结的单
        /// </summary>
        /// <param name="Creator"></param>
        /// <returns></returns>
        public List<NoticeBillModel> GetNotFinishBill(string Creator)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.Creator == Creator && c.ApprovalStatus != 3 && c.ApprovalStatus != 2).ToList();
            return model;
        }

        /// <summary>
        /// 编辑付款通知书
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveNoticeBillChange(EditBillModel model)
        {
            //费用单有6处改动
            var newModel = MongoDBHelper.NoticeBill.Find(c => c.BillNo == model.BillNo).FirstOrDefault();
            if (newModel != null)
            {
                newModel.PersonInfo.Brand = model.Brand;
                newModel.PersonInfo.CostCenter = model.CostCenter;
                newModel.SpecialAttribute.Funds = model.SpecialAttribute.Funds;
                newModel.SpecialAttribute.Agent = model.SpecialAttribute.Agent;
                newModel.SpecialAttribute.Check = model.SpecialAttribute.Check;
                newModel.Currency = model.Currency;
                newModel.Items = model.Items;
                newModel.MissBill = model.MissBill;
                newModel.BillsItems = model.BillsItems;
                newModel.TotalMoney = model.Items.Sum(all => all.money) + model.Items.Sum(all => all.taxmoney);
                newModel.Photos = model.Photos;
                newModel.COST_ACCOUNT = model.CostCenter;
                newModel.PersonInfo.Department = model.DPName;
                newModel.PersonInfo.DepartmentCode = model.DPID;
                newModel.PersonInfo.Shop = model.ShopName;
                newModel.PersonInfo.ShopCode = model.ShopCode;
                newModel.Remark = model.Remark;
                newModel.PersonInfo.Company = model.E_Company;
                newModel.PersonInfo.CompanyCode = model.E_CompanyCode;

                var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", model.BillNo);
                var result = MongoDBHelper.NoticeBill.FindOneAndReplace(filter, newModel);
                return result != null ? "Success" : "Fail";
            }
            return "Fail";
        }

        /// <summary>
        /// 获取指定时间内的缺失发票
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<NoticeBillModel> GetMissBill(int Type, DateTime startTime, DateTime endTime)
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var model = MongoDBHelper.NoticeBill.Find(c => c.ApprovalStatus == 2 && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).ToList();
            if (model != null && model.Count > 0)
            {
                switch (Type)
                {
                    //全部
                    case 1:
                        break;
                    //未追回
                    case 2:
                        model = model.Where(c => c.MissBill == 0).ToList();
                        break;
                    //已追回
                    case 3:
                        model = model.Where(c => c.MissBill == 1).ToList();
                        break;
                    default:
                        break;
                }
            }
            return model;
        }



        public List<NoticeBillModel> GetMissBill(List<int> Type, string CompanyCode, List<int> IsBelong)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.ApprovalStatus == 2 && c.Status == 0 && Type.Contains(c.MissBill) && c.PersonInfo.CompanyCode == CompanyCode && IsBelong.Contains(c.PersonInfo.IsHeadOffice)).ToList();
            return model;
        }

        /// <summary>
        /// 回收发票
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string NoticeRecoverBill(string BillNo, string No)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<NoticeBillModel>.Update.Set("MissBill", 1).Set("RecycleNo", No);
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
        }


        /// <summary>
        /// 删除付款通知书
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string DelectNotcieBill(string BillNo)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<NoticeBillModel>.Update.Set("Status", 1);
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        /// <summary>
        /// 编辑单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string EditNoticeBill(NoticeBillModel model)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", model.BillNo);
            var result = MongoDBHelper.NoticeBill.FindOneAndReplace(filter, model);
            return result != null ? "Success" : "Fail";
        }


        public List<NoticeBillModel> GetReportData()
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.ApprovalStatus != 3 && c.PersonInfo.IsHeadOffice == 0).ToList();
            return model;
        }


        public List<NoticeBillModel> GetRelationBillList(string itemName, string DepartmentCode, string ShopCode, DateTime BeginTime, string ProviderName, int IsHeadOffice, int FindDadaType)
        {
            if (FindDadaType == 1)
            {
                if (IsHeadOffice == 0)
                {
                    var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.PersonInfo.DepartmentCode == DepartmentCode && c.CreateTime >= BeginTime && c.PersonInfo.ShopCode == ShopCode && c.ProviderInfo.ProviderName == ProviderName).ToList();
                    return model;
                }

                else
                {
                    var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.ProviderInfo.ProviderName == ProviderName).ToList();
                    return model;
                }
            }
            //点击的不是费用单的查看相关单据
            else
            {
                var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.PersonInfo.DepartmentCode == DepartmentCode && c.CreateTime >= BeginTime && c.PersonInfo.ShopCode == ShopCode).ToList();
                if (model.Count > 0)
                {
                    foreach (var item in model)
                    {
                        var temp = item.Items.Where(c => c.name == itemName).FirstOrDefault();
                        if (temp == null)
                        {
                            item.Status = 1;
                        }
                        else
                        {

                            item.TotalMoney = temp.money + temp.taxmoney;
                        }
                    }
                    model = model.Where(c => c.Status == 0).ToList();
                }
                return model;
            }
        }

        public string AddPrintedNum(string BillNo)
        {
            var OldNum = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.BillNo == BillNo).Project(x => x.PrintedCount).FirstOrDefault();
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<NoticeBillModel>.Update.Set("PrintedCount", OldNum + 1);
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public List<NoticeBillModel> CountSmallSortNum(int DepartmentSort, List<string> AreaList, List<string> AccountInfoList, List<int> BillTypesList, DateTime BeginTime, DateTime EndTime, string IsFund, string AreaCodeAndShopCode, List<string> MyCompanycode)
        {
            List<NoticeBillModel> Model = new List<NoticeBillModel>();
            List<NoticeBillModel> DeleteModel = new List<NoticeBillModel>();
            if (DepartmentSort == 3)
            {
                Model = MongoDBHelper.NoticeBill.Find(c => AreaList.Contains(c.PersonInfo.DepartmentCode) && c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else if (DepartmentSort == 2)
            {
                Model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else
            {
                Model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.PersonInfo.IsHeadOffice == DepartmentSort && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

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

            if (Model.Count > 0)
            {
                foreach (var item in Model)
                {
                    if (item.ProviderInfo.ProviderName.Contains("一次性"))
                    {
                        item.ProviderInfo.ProviderName = item.ProviderInfo.ProviderName + "-" + item.ProviderInfo.BankName;
                    }
                }
            }
            return Model;
        }


        public long TransferBill(string No1, string No2, string Name)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("Creator", No1) & Builders<NoticeBillModel>.Filter.Where(c => c.ApprovalStatus != 2 & c.ApprovalStatus != 3 && c.Status == 0);
            var update = Builders<NoticeBillModel>.Update.Set("Creator", No2).Set("WorkNumber", No2).Set("Owner", Name);
            var result = MongoDBHelper.NoticeBill.UpdateMany(filter, update);
            return result.MatchedCount;
        }


        public string PublicEditMethod(string BillNo, Dictionary<string, string> dic)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<NoticeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<NoticeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod<T>(string BillNo, Dictionary<string, T> dic) where T : class
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<NoticeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<NoticeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod(string BillNo, Dictionary<string, List<PostDescription>> dic)
        {
            var filter = Builders<NoticeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<NoticeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<NoticeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.NoticeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<NoticeBillModel> FindSuitableData(string BankName)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.ProviderInfo.BankName.Contains(BankName)).ToList();
            return model;
        }


        public List<NoticeBillModel> ReturnShelveNo(string EmployeeNo)
        {
            var model = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.ShelveNo == EmployeeNo).ToList();
            return model;
        }


        public List<MongoModel> GetListByYearAndMonth(int Year, int Month)
        {
            DateTime minTime = new DateTime(Year, Month, 1);
            if (Month == 12)
            {
                Year += 1;
                Month = 0;
            }
            DateTime maxTime = new DateTime(Year, Month + 1, 1);

            var list = MongoDBHelper.NoticeBill.Find(c => c.Status == 0 && c.ApprovalStatus != 3 && c.PersonInfo != null && c.PersonInfo.IsHeadOffice == 0 && c.Items != null && c.CreateTime >= minTime && c.CreateTime < maxTime).Project(x => new MongoModel
            {
                BillNo = x.BillNo,
                BusinessDate = x.TransactionDate,
                Items = x.Items,
                ShopCode = x.PersonInfo.ShopCode,
                DepartmentCode = x.PersonInfo.DepartmentCode,
                CreateTime = x.CreateTime
            }).ToList();
            return list;
        }


        /// <summary>
        /// 获取办结数据
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="endTime"></param>
        /// <param name="PostNotNull">流程是否为空</param>
        /// <returns></returns>
        public List<NoticeBillModel> GetCompleteBill(string createTime, string endTime, bool PostNotNull)
        {

            DateTime T1 = string.IsNullOrEmpty(createTime) == true ? new DateTime() : Convert.ToDateTime(createTime);
            DateTime T2 = string.IsNullOrEmpty(endTime) == true ? new DateTime(2099, 1, 1) : Convert.ToDateTime(endTime);

            if (PostNotNull)
            {
                return MongoDBHelper.NoticeBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString != null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
            else
            {
                return MongoDBHelper.NoticeBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString == null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
        }
    }
}