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
    public class DraftBox : BillBase, IDraftBox, IMaxNo
    {
        public DraftBox()
            : base(BillType.费用草稿单)
        {
        }


        public void CreateFeeBillDraftBox(FeeBillModel pFeeBill)
        {
            //获取最大单号自动给号
            pFeeBill.BillNo = GenerateMaxBillNo();
            MongoDBHelper.FeeBillDraftBox.InsertOne(pFeeBill);
        }

        public List<Models.FeeBillModel> GetDraftBoxByParameter(int type, string employeeNo)
        {
            DateTime startWeek = DateTime.Now.Date.AddDays(1 - Convert.ToInt32(DateTime.Now.Date.DayOfWeek.ToString("d"))); //本周周一
            DateTime endWeek = startWeek.AddDays(7); //下周一

            List<Models.FeeBillModel> result = new List<Models.FeeBillModel>();
            switch (type)
            {
                case 1: //今天
                    result = MongoDBHelper.FeeBillDraftBox.Find(c => c.CreateTime >= DateTime.Now.Date && c.Creator == employeeNo && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
                    break;
                case 2://本周
                    result = MongoDBHelper.FeeBillDraftBox.Find(c => c.CreateTime >= startWeek && c.CreateTime < endWeek && c.Creator == employeeNo && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
                    break;
                case 3://本月
                    result = MongoDBHelper.FeeBillDraftBox.Find(c => c.CreateTime >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) && c.CreateTime < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1) && c.Creator == employeeNo && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
                    break;
                case 4://上月

                    result = MongoDBHelper.FeeBillDraftBox.Find(c => c.CreateTime >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1) && c.CreateTime < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) && c.Creator == employeeNo && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
                    break;
                default:
                    break;
            }

            return result;

        }

        public void GetDraftBoxByID(string pID)
        {
            throw new NotImplementedException();
        }



        public override string GetMaxBillNo()
        {
            var maxBill = MongoDBHelper.FeeBillDraftBox.Find(c => c.CreateTime >= DateTime.Now.Date).SortByDescending(c => c.BillNo).Limit(1);
            if (maxBill == null || maxBill.Count() == 0)
            {
                return "";
            }

            return maxBill.First().BillNo;
        }

        public FeeBillModel GetBillModel(string billNo)
        {
            var model = MongoDBHelper.FeeBillDraftBox.Find(c => c.BillNo == billNo && c.Status == 0).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 删除草稿箱内容
        /// </summary>
        /// <param name="billNo"></param>
        /// <returns></returns>
        public bool DeleteDraftContent(string BillNo)
        {        
            long result = MongoDBHelper.FeeBillDraftBox.DeleteOne(c => c.BillNo == BillNo && c.Status == 0).DeletedCount;
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}