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
    public class BorrowBill : BillBase, IBorrowBill, IMaxNo
    {

        public BorrowBill()
            : base(BillType.借款单)
        {

        }
        public override string GetMaxBillNo()
        {
            var maxBill = MongoDBHelper.BorrowBill.Find(c => c.CreateTime >= DateTime.Now.Date).SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();

            return maxBill == null ? "" : maxBill.BillNo;
        }

        public void GetBorrowBill(string pID)
        {

        }

        public void CreateBorrowBill(BorrowBillModel pBorrowBill)
        {
            //pBorrowBill.BillNo = GenerateMaxBillNo();
            MongoDBHelper.BorrowBill.InsertOne(pBorrowBill);
        }

        /// <summary>
        /// 获取所有借款记录
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="PageIndex">页码数</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalNumber">总数量</param>
        /// <returns></returns>
        public List<BorrowBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int PageIndex, int pageSize, out int totalNumber)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            //单据类型           

            totalNumber = Convert.ToInt32(MongoDBHelper.BorrowBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0).Count());
            var list = MongoDBHelper.BorrowBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).Skip((PageIndex - 1) * pageSize).Limit(pageSize).ToList();
            return list;
        }

        /// <summary>
        /// 获取用户指定时间内待还借款记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<BorrowBillModel> GetBillForPrint(DateTime startTime, DateTime endTime)
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            var list = MongoDBHelper.BorrowBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && c.SurplusMoney > 0 && c.ApprovalStatus == 2).SortByDescending(x => x.CreateTime).ToList();
            return list;
        }


        /// <summary>
        /// 获取用户所有待还借款记录（未还清）首页上显示
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<BorrowBillModel> GetBillForPrint(string EmployeeNo)
        {
            var list = MongoDBHelper.BorrowBill.Find(c => c.Creator == EmployeeNo && c.Status == 0 && c.ApprovalStatus == 2).SortByDescending(x => x.CreateTime).ToList();
            list = list.Where(e => e.SurplusMoney > 0).ToList();
            return list;
        }

        public List<BorrowBillModel> GetBillModelByNo(string No)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Creator == No).ToList();
            return model;
        }

        public List<string> GetAbnormalModel()
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && string.IsNullOrEmpty(c.ApprovalPost)).Project(c => c.WorkFlowID).ToList();
            return model;
        }


        /// <summary>
        /// 获取用户所有待还借款记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<BorrowBillModel> GetBillForAll(string EmployeeNo)
        {
            var list = MongoDBHelper.BorrowBill.Find(c => c.Creator == EmployeeNo && c.Status == 0 && c.ApprovalStatus == 2).SortByDescending(x => x.CreateTime).ToList();
            return list;
        }


        ///// <summary>
        ///// 处理借款单剩余金额
        ///// </summary>
        ///// <param name="BillNo"></param>
        ///// <param name="Money"></param>
        ///// <returns></returns>
        //public string EditBorrowBill(string BillNo, decimal Money)
        //{
        //    var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
        //    var update = Builders<BorrowBillModel>.Update.Set("SurplusMoney", Money);
        //    var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
        //    return result != null && result.ModifiedCount > 0 ? "Success" : "";
        //}

        /// <summary>
        /// 获取用户所有待还借款记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<BorrowBillModel> GetMyBillList(DateTime startTime, DateTime endTime, int Belong, List<string> DepartmentCode, string CheckStatus = "", string PrintStatus = "")
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            List<BorrowBillModel> list = new List<BorrowBillModel>();
            if (Belong == 1 || Belong == 3)
            {
                if (DepartmentCode.Count > 0)
                {
                    list = MongoDBHelper.BorrowBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
                }
                else
                {
                    list = MongoDBHelper.BorrowBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).ToList();
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
                list = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
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

        public BorrowBillModel GetBillModel(string BillNo)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.BillNo == BillNo).FirstOrDefault();
            return model;
        }


        public BorrowBillModel GetBillModelPlus(string BillNo, int IsHeadOffice)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.PersonInfo.IsHeadOffice == IsHeadOffice).FirstOrDefault();
            return model;
        }

        public BorrowBillModel GetBillModel(string BillNo, string No)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.Creator == No).FirstOrDefault();
            return model;
        }

        public BorrowBillModel GetBillModel(string BillNo, List<string> str)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.BillNo == BillNo && str.Contains(c.PersonInfo.DepartmentCode)).FirstOrDefault();
            return model;
        }


        /// <summary>
        /// 批量获取我审批的单据（对应具体的单据）
        /// </summary>
        public List<BorrowBillModel> GetMyProcess(string EmployeeNo, List<string> IdList)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => IdList.Contains(c.WorkFlowID) && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
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
                var filter = Builders<BorrowBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<BorrowBillModel>.Update.Set("ApprovalStatus", ApprovalStatus).Set("ApprovalTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
            else
            {
                if (ApprovalStatus == 4)
                {
                    int status = MongoDBHelper.BorrowBill.Find(c => c.WorkFlowID == WorkFlowID).Project(x => x.ApprovalStatus).FirstOrDefault();
                    if (status == 4)
                    {
                        return "Success";
                    }
                }
                var filter = Builders<BorrowBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<BorrowBillModel>.Update.Set("ApprovalStatus", ApprovalStatus);
                var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }

        }


        /// <summary>
        /// 修改借款单审批岗位和办结时间
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus"></param>
        /// <param name="PostString"></param>
        /// <returns></returns>
        public string ChangeBorrowBillApprovalPost(string WorkFlowID, string PostString)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
            var update = Builders<BorrowBillModel>.Update.Set("ApprovalPost", PostString);
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result.ModifiedCount > 0 ? "Success" : "Fail";
        }

        /// <summary>
        /// 根据WorkFlowID获取对象
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <returns></returns>
        public BorrowBillModel GetModelFromWorkFlowID(string WorkFlowID)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.WorkFlowID == WorkFlowID).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 获取未办结的单
        /// </summary>
        /// <param name="Creator"></param>
        /// <returns></returns>
        public List<BorrowBillModel> GetNotFinishBill(string Creator)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.Creator == Creator && c.ApprovalStatus != 3 && c.ApprovalStatus != 2).ToList();
            return model;
        }

        /// <summary>
        /// 处理还款操作（费用报销单可以超出欠款额度）
        /// </summary>
        /// <param name="model"></param>
        public string DealUnualRefundBill(RefundBillModel model)
        {
            try
            {
                var BorrowModel = MongoDBHelper.BorrowBill.Find(c => c.BillNo == model.BorrowBillNo && c.Status == 0).FirstOrDefault();
                if (model.RefundType.ToUpper() == "FEEBILL" && model.RealRefundMoney > BorrowModel.SurplusMoney)//还钱大于欠钱
                {
                    ////1往还款单里面插入一个反向现金单
                    //model.Id = MongoDB.Bson.ObjectId.Empty;
                    //model.OffsetBillNo = model.BillNo;
                    //model.RefundType = "Cash";
                    //model.Remark = "总部现金冲账";
                    //model.Items = null;
                    //model.RealRefundMoney = BorrowModel.SurplusMoney - model.RealRefundMoney;
                    //model.WorkFlowID = ""; //将WorkFlowID置空
                    //model.Flag = 1;  //冲账
                    //model.ApprovalStatus = 2;//已经通过
                    //model.CreateTime = DateTime.Now;
                    //new RefundBill().CreateRefundBill(model);

                    var filter = Builders<RefundBillModel>.Filter.Eq("BillNo", model.BillNo);
                    var update = Builders<RefundBillModel>.Update.Set("OutDebt", 1).Set("DebtMoney", BorrowModel.SurplusMoney);
                    var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);

                    //2跟新原来的费用还款单还款额度（让其等于0）
                    var filter1 = Builders<BorrowBillModel>.Filter.Eq("BillNo", model.BorrowBillNo);
                    var update1 = Builders<BorrowBillModel>.Update.Set("SurplusMoney", 0);
                    var result1 = MongoDBHelper.BorrowBill.UpdateOne(filter1, update1);
                    return result.ModifiedCount > 0 ? "Success" : "Fail";
                }
                else
                {
                    var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", model.BorrowBillNo);
                    var update = Builders<BorrowBillModel>.Update.Set("SurplusMoney", BorrowModel.SurplusMoney - model.RealRefundMoney);
                    var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
                    return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
                }
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("拆分单据失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }


        /// <summary>
        /// 借款单编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveBorrowBillChange(EditBillModel model)
        {
            var newModel = MongoDBHelper.BorrowBill.Find(c => c.BillNo == model.BillNo).FirstOrDefault();
            if (newModel != null)
            {
                newModel.PersonInfo.CostCenter = model.CostCenter;
                newModel.Currency = model.Currency;
                newModel.PersonInfo.Brand = model.Brand;
                newModel.SpecialAttribute = model.SpecialAttribute;
                newModel.Items = model.Items;
                newModel.BillsItems = model.BillsItems;
                newModel.TotalMoney = model.Items.Sum(all => all.money) + model.Items.Sum(all => all.taxmoney);
                newModel.Photos = model.Photos;
                newModel.COST_ACCOUNT = model.CostCenter;
                newModel.PersonInfo.Department = model.DPName;
                newModel.PersonInfo.DepartmentCode = model.DPID;
                newModel.PersonInfo.Shop = model.ShopName;
                newModel.PersonInfo.ShopCode = model.ShopCode;
                newModel.Remark = model.Remark;
                newModel.SurplusMoney = newModel.TotalMoney;
                newModel.PersonInfo.Company = model.E_Company;
                newModel.PersonInfo.CompanyCode = model.E_CompanyCode;

                var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", model.BillNo);
                var result = MongoDBHelper.BorrowBill.FindOneAndReplace(filter, newModel);
                return result != null ? "Success" : "Fail";
            }
            return "Fail";
        }

        /// <summary>
        /// 删除借款单
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string DelectBorrowBill(string BillNo)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<BorrowBillModel>.Update.Set("Status", 1);
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        /// <summary>
        /// 编辑单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string EditBorrowBill(BorrowBillModel model)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", model.BillNo);
            var result = MongoDBHelper.BorrowBill.FindOneAndReplace(filter, model);
            return result != null ? "Success" : "Fail";
        }


        public List<BorrowBillModel> GetFeeReportData()
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.ApprovalStatus == 2 && c.Remark == "期初导入").ToList();
            return model;
        }


        public List<BorrowBillModel> GetRelationBillList(string itemName, string DepartmentCode, string ShopCode, DateTime BeginTime)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.PersonInfo.DepartmentCode == DepartmentCode && c.CreateTime >= BeginTime && c.PersonInfo.ShopCode == ShopCode).ToList();
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


        public string AddPrintedNum(string BillNo)
        {
            var OldNum = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.BillNo == BillNo).Project(x => x.PrintedCount).FirstOrDefault();
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<BorrowBillModel>.Update.Set("PrintedCount", OldNum + 1);
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<BorrowBillModel> CountSmallSortNum(int DepartmentSort, List<string> AreaList, List<string> AccountInfoList, List<int> BillTypesList, DateTime BeginTime, DateTime EndTime, string IsFund, string AreaCodeAndShopCode, List<string> MyCompanycode)
        {
            List<BorrowBillModel> Model = new List<BorrowBillModel>();
            List<BorrowBillModel> DeleteModel = new List<BorrowBillModel>();
            if (DepartmentSort == 3)
            {
                Model = MongoDBHelper.BorrowBill.Find(c => AreaList.Contains(c.PersonInfo.DepartmentCode) && c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else if (DepartmentSort == 2)
            {
                Model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else
            {
                Model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.PersonInfo.IsHeadOffice == DepartmentSort && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

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

            return Model;
        }



        public long TransferBill(string No1, string No2, string Name)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("Creator", No1) & Builders<BorrowBillModel>.Filter.Where(c=>c.ApprovalStatus != 3 && c.Status == 0);
            var update = Builders<BorrowBillModel>.Update.Set("Creator", No2).Set("WorkNumber", No2).Set("Owner", Name);
            var result = MongoDBHelper.BorrowBill.UpdateMany(filter, update);
            return result.MatchedCount;
        }

        public string PublicEditMethod(string BillNo, Dictionary<string, string> dic)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<BorrowBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<BorrowBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod<T>(string BillNo, Dictionary<string, T> dic) where T : class
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<BorrowBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<BorrowBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod(string BillNo, Dictionary<string, List<PostDescription>> dic)
        {
            var filter = Builders<BorrowBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<BorrowBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<BorrowBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.BorrowBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<BorrowBillModel> ReturnShelveNo(string EmployeeNo)
        {
            var model = MongoDBHelper.BorrowBill.Find(c => c.Status == 0 && c.ShelveNo == EmployeeNo).ToList();
            return model;
        }

        /// <summary>
        /// 获取办结数据
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="endTime"></param>
        /// <param name="PostNotNull">流程是否为空</param>
        /// <returns></returns>
        public List<BorrowBillModel> GetCompleteBill(string createTime, string endTime, bool PostNotNull)
        {

            DateTime T1 = string.IsNullOrEmpty(createTime) == true ? new DateTime() : Convert.ToDateTime(createTime);
            DateTime T2 = string.IsNullOrEmpty(endTime) == true ? new DateTime(2099, 1, 1) : Convert.ToDateTime(endTime);

            if (PostNotNull)
            {
                return MongoDBHelper.BorrowBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString != null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
            else
            {
                return MongoDBHelper.BorrowBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString == null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
        }
    }
}