using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Marisfrolg.Fee.BLL
{
    public class CollectionInfoHelper
    {
        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="model"></param>
        public void CreateCollectionInfoRecord(CollectionInformation model)
        {
            var CollectionInfoModel = MongoDBHelper.CollectionInfoLog.Find(c => c.CreatorID == model.CreatorID && c.Name == model.Name && c.BankName == model.BankName && c.BankCode == model.BankCode && c.City == model.City && c.SubbranchBank == model.SubbranchBank).FirstOrDefault();
            if (CollectionInfoModel != null)
            {
                //存在这个数据就不再插入
                return;
            }
            MongoDBHelper.CollectionInfoLog.InsertOne(model);
        }

        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="CreatorID"></param>
        /// <returns></returns>
        public List<CollectionInformation> GetModel(string CreatorID)
        {
            var model = MongoDBHelper.CollectionInfoLog.Find(c => c.CreatorID == CreatorID).ToList();
            return model;
        }
    }
}