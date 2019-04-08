using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;


namespace Marisfrolg.Fee.BLL
{

    public class RefundFeeBill : BillBase, IRefundBill, IMaxNo
    {

        public RefundFeeBill()
            : base(BillType.费用报销还款单)
        {
        }

        public RefundBillModel GetBillModel(string BillNo)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.Status == 0 && c.BillNo == BillNo).FirstOrDefault();
            return model;
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
            var model = MongoDBHelper.RefundBill.Find(c => IdList.Contains(c.WorkFlowID) && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
            return model;
        }


        public override string GetMaxBillNo()
        {
            //这里要考虑费用单还款，要额外编号

            if (this.BillType == BillType.费用报销还款单)
            {
                var maxRefundFeeBill = MongoDBHelper.RefundBill.Find(c => c.CreateTime >= DateTime.Now.Date && c.CreateTime < DateTime.Now.Date.AddDays(1) && c.RefundType.ToUpper() == "FEEBILL").SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();
                var maxFeeBill = MongoDBHelper.FeeBill.Find(c => c.CreateTime >= DateTime.Now.Date && c.CreateTime < DateTime.Now.Date.AddDays(1)).SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();

                if (maxRefundFeeBill == null && maxFeeBill == null)
                {
                    return "";
                }
                string string1 = (maxRefundFeeBill == null ? "" : maxRefundFeeBill.BillNo);
                string string2 = (maxFeeBill == null ? "" : maxFeeBill.BillNo);
                ArrayList al = new ArrayList();
                al.Add(string1); al.Add(string2);
                al.Sort();
                return al[al.Count - 1].ToString();

            }
            else
            {
                var maxRefundFeeBill = MongoDBHelper.RefundBill.Find(c => c.CreateTime >= DateTime.Now.Date && c.Status == 0 && c.RefundType.ToUpper() == "CASH").SortByDescending(c => c.BillNo).Limit(1).FirstOrDefault();

                return maxRefundFeeBill == null ? "" : maxRefundFeeBill.BillNo;
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
        public List<RefundBillModel> GetBillForPrint(DateTime startTime, DateTime endTime)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            var list = MongoDBHelper.RefundBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).ToList();
            return list;
        }

        public void CreateRefundBill(RefundBillModel pFeeBill)
        {
            //var document = pFeeBill.ToBsonDocument();
            //获取最大单号自动给号
            pFeeBill.BillNo = GenerateMaxBillNo();
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
        public string ChangeBillStatus(string WorkFlowID, int ApprovalStatus, out object obj)
        {
            var model = MongoDBHelper.RefundBill.Find(c => c.WorkFlowID == WorkFlowID && c.Status == 0).FirstOrDefault();
            obj = model;
            //还款单
            if (MongoDBHelper.RefundBill.Find(c => c.WorkFlowID == WorkFlowID && c.Status == 0).FirstOrDefault() != null)
            {
                var filter = Builders<RefundBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<RefundBillModel>.Update.Set("ApprovalStatus", ApprovalStatus);
                var result = MongoDBHelper.RefundBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
            return "Fail";
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
    }
}