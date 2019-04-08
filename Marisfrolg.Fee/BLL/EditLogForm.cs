using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Marisfrolg.Fee.BLL
{
    /// <summary>
    /// 财务编辑单据操作
    /// </summary>
    public class EditLogForm
    {
        public void CreateEditRecord(EditLog model)
        {
            var ErrorModel = MongoDBHelper.EditLog.Find(c => c.BillNo == model.BillNo && c.CreateTime == model.CreateTime).FirstOrDefault();
            if (ErrorModel != null)
            {
                //存在这个数据就不再插入
                return;
            }
            model.NumberID = GetMaxBillNo() + 1;
            MongoDBHelper.EditLog.InsertOne(model);
        }

        public int GetMaxBillNo()
        {
            var model = MongoDBHelper.EditLog.Find(c => c.NumberID > 0).SortByDescending(c => c.NumberID).Limit(1);
            if (model == null || model.Count() == 0)
            {
                return 0;
            }
            else
            {
                return model.First().NumberID;
            }
        }
    }
}