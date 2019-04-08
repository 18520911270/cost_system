using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Marisfrolg.Fee.BLL
{
    public class ErrorBill
    {
        public void CreateErrorRecord(ErrorContainer model)
        {
            var ErrorModel = MongoDBHelper.ErrorContainer.Find(c => c.BillType == model.BillType && c.WorkFlowID == model.WorkFlowID).FirstOrDefault();
            if (ErrorModel != null)
            {
                //存在这个数据就不再插入
                return;
            }
            model.NumberID = GetMaxBillNo() + 1;
            MongoDBHelper.ErrorContainer.InsertOne(model);
        }

        public int GetMaxBillNo()
        {
            var model = MongoDBHelper.ErrorContainer.Find(c => c.NumberID > 0).SortByDescending(c => c.NumberID).Limit(1);
            if (model == null || model.Count() == 0)
            {
                return 0;
            }
            else
            {
                return model.First().NumberID;
            }
        }

        public bool DeleteErrorData(string WorkId)
        {
            var result = MongoDBHelper.ErrorContainer.DeleteOne(c => c.WorkFlowID == WorkId);
            return result.DeletedCount > 0 ? true : false;
        }

        public List<ErrorContainer> GetAllData()
        {
            var model = MongoDBHelper.ErrorContainer.Find(c => true).ToList();
            return model;
        }
    }
}