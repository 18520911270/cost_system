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
    public class UserJobOperation
    {
        //public string AddUserJob(WorkFlowModel.UserJob Job)
        //{
        //    var model = MongoDBHelper.UserJobContainer.Find(c => c.WorkFlowId == Job.WorkFlowId).FirstOrDefault();
        //    if (model != null)
        //    {
        //        return "Fail";
        //    }
        //    MongoDBHelper.UserJobContainer.InsertOne(Job);
        //    return "Success";
        //}


        //public string EditUserJob(WorkFlowModel.UserJob Job)
        //{
        //    var model = MongoDBHelper.UserJobContainer.Find(c => c.WorkFlowId == Job.WorkFlowId).FirstOrDefault();
        //    if (model != null)
        //    {
        //        return "Fail";
        //    }
        //    var filter = Builders<WorkFlowModel.UserJob>.Filter.Eq("WorkFlowId", Job.WorkFlowId);
        //    var result = MongoDBHelper.UserJobContainer.FindOneAndReplace(filter, Job);
        //    return result != null ? "Success" : "Fail";
        //}
    }
}