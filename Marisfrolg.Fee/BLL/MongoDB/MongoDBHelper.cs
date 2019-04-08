using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Marisfrolg.Fee.BLL
{

    public class MongoDBHelper
    {

        public static IMongoDatabase Database
        {
            get
            {
                //MongoConnection
                string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["MongoConnection"].ToString();
                string MongoDBName = System.Configuration.ConfigurationManager.ConnectionStrings["MongoDBName"].ToString();
                MongoClient client = new MongoClient(new MongoUrl(ConnStr));
                IMongoDatabase database = client.GetDatabase(MongoDBName);
                return database;

            }

            //get
            //{
            //    //MongoConnection
            //    string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["MongoConnection"].ToString();
            //    MongoServer server = MongoServer.Create(ConnStr);
            //    MongoDatabase mongoDatabase = server.GetDatabase("fee"); 
            //    //IMongoDatabase database = server.GetDatabase("fee");
            //    return mongoDatabase;

            //}

        }

        public static IMongoCollection<Models.NoticeBillModel> NoticeBill
        {
            get
            {

                var collection = Database.GetCollection<Models.NoticeBillModel>("noticeBills");

                return collection;

            }

        }


        public static IMongoCollection<Models.BorrowBillModel> BorrowBill
        {
            get
            {

                var collection = Database.GetCollection<Models.BorrowBillModel>("borrowBill");

                return collection;

            }

        }

        public static IMongoCollection<Models.RefundBillModel> RefundBill
        {
            get
            {

                var collection = Database.GetCollection<Models.RefundBillModel>("refundBill");

                return collection;

            }

        }

        public static IMongoCollection<Models.FeeBillModel> FeeBill
        {
            get
            {

                var collection = Database.GetCollection<Models.FeeBillModel>("feeBills");

                return collection;

            }

        }

        public static IMongoCollection<Models.FeeBillModel> FeeBillDraftBox
        {
            get
            {

                var collection = Database.GetCollection<Models.FeeBillModel>("feeBillDraftBoxs");

                return collection;

            }

        }

        public static IMongoCollection<Models.ErrorContainer> ErrorContainer
        {
            get
            {

                var collection = Database.GetCollection<Models.ErrorContainer>("errorContainer");

                return collection;

            }
        }

        public static IMongoCollection<Models.EditLog> EditLog
        {
            get
            {

                var collection = Database.GetCollection<Models.EditLog>("editLog");

                return collection;

            }
        }

        public static IMongoCollection<Models.CollectionInformation> CollectionInfoLog
        {
            get
            {

                var collection = Database.GetCollection<Models.CollectionInformation>("collectionInfoLog");

                return collection;

            }
        }


        //public static IMongoCollection<WorkFlowModel.UserJob> UserJobContainer
        //{
        //    get
        //    {

        //        var collection = Database.GetCollection<WorkFlowModel.UserJob>("userJob");

        //        return collection;

        //    }
        //}
    }
}