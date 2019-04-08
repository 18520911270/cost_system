using Marisfrolg.Fee.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using WorkFlowEngine;


namespace Marisfrolg.Fee.BLL
{

    public class FeeBill : BillBase, IFeeBill, IMaxNo
    {

        public FeeBill()
            : base(BillType.费用报销单)
        {
        }

        public void CreateFeeBill(FeeBillModel pFeeBill)
        {
            //var document = pFeeBill.ToBsonDocument();
            //获取最大单号自动给号
            //pFeeBill.BillNo = GenerateMaxBillNo();
            MongoDBHelper.FeeBill.InsertOne(pFeeBill);


        }

        /// <summary>
        /// 获取费用报表数据
        /// </summary>
        /// <returns></returns>
        public List<FEE_Report> GetFeeReportData(DateTime startTime, DateTime endTime, List<string> SHopCodeList, List<string> companyCode)
        {
            if (SHopCodeList.Count > 0)
            {
                var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && SHopCodeList.Contains(c.PersonInfo.DepartmentCode) && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;
            }
            else
            {
                var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.ApprovalStatus != 3 && c.SpecialAttribute.Funds == 0 && companyCode.Contains(c.PersonInfo.CompanyCode) && c.CreateTime >= startTime && c.CreateTime <= endTime).Project(c => new FEE_Report
                {
                    Brand = c.ShopLogo,
                    Items = c.Items.Where(x => x.IsMarket == 1).ToList()
                }).ToList();
                return model;
            }
        }


        public void GetFeeBill(string pID)
        {

        }

        public string GenerateMaxDraftBox()
        {

            return "";
        }

        public List<string> GetAbnormalModel()
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && string.IsNullOrEmpty(c.ApprovalPost)).Project(c => c.WorkFlowID).ToList();
            return model;
        }

        public FeeBillModel GetBillModel(string BillNo)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.BillNo == BillNo).FirstOrDefault();
            return model;
        }

        public FeeBillModel GetBillModelPlus(string BillNo, int IsHeadOffice)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.PersonInfo.IsHeadOffice == IsHeadOffice).FirstOrDefault();
            return model;
        }

        public FeeBillModel GetBillModel(string BillNo, string No)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && c.Creator == No).FirstOrDefault();
            return model;
        }

        public FeeBillModel GetBillModel(string BillNo, List<string> str)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.BillNo == BillNo && str.Contains(c.PersonInfo.DepartmentCode)).FirstOrDefault();
            return model;
        }

        public List<FeeBillModel> GetBillModelByNo(string No)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Creator == No).ToList();
            return model;
        }


        /// <summary>
        /// 批量获取我审批的单据（对应具体的单据）
        /// </summary>
        public List<FeeBillModel> GetMyProcess(string EmployeeNo, List<string> IdList)
        {
            var model = MongoDBHelper.FeeBill.Find(c => IdList.Contains(c.WorkFlowID) && c.Status == 0).SortByDescending(x => x.CreateTime).ToList();
            return model;
        }

        /// <summary>
        /// 获取未办理单据列表
        /// </summary>
        /// <returns></returns>
        public List<FeeBillModel> GetNoCheckBill(string EmployeeNo)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.Creator == EmployeeNo).SortByDescending(x => x.CreateTime).ToList();
            return model;
        }

        public override string GetMaxBillNo()
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

        /// <summary>
        /// 获取费用报销单单据
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="PageIndex">页码数</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalNumber">总数量</param>
        /// <returns></returns>
        public List<FeeBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int PageIndex, int pageSize, out int totalNumber)
        {
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            //单据类型           

            totalNumber = Convert.ToInt32(MongoDBHelper.FeeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0).Count());
            var list = MongoDBHelper.FeeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime > startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).Skip((PageIndex - 1) * pageSize).Limit(pageSize).ToList();
            return list;
        }

        /// <summary>
        /// 根据时间获取所有数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<FeeBillModel> GetBillForPrint(DateTime startTime, DateTime endTime, int Belong, List<string> DepartmentCode, string CheckStatus = "", string PrintStatus = "")
        {
            startTime = startTime.Date;
            endTime = endTime.Date.AddDays(1);
            var employee = Marisfrolg.Public.Common.GetEmployeeInfo();
            List<FeeBillModel> list = new List<FeeBillModel>();

            if (Belong == 1 || Belong == 3)
            {
                if (DepartmentCode.Count > 0)
                {
                    list = MongoDBHelper.FeeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
                }
                else
                {
                    list = MongoDBHelper.FeeBill.Find(c => c.Creator == employee.EmployeeNo && c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime).SortByDescending(x => x.CreateTime).ToList();
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
                list = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.CreateTime >= startTime && c.CreateTime < endTime && DepartmentCode.Contains(c.PersonInfo.DepartmentCode)).SortByDescending(x => x.CreateTime).ToList();
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
        /// 获取单个流程的图形化列表
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public List<FlowInstance> GetWorkFlowList(string BillNo, int Type)
        {
            string WorkFlowID = "";
            try
            {
                switch (Type)
                {
                    //费用报销单
                    case 1:
                        WorkFlowID = MongoDBHelper.FeeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //付款通知书
                    case 2:
                        WorkFlowID = MongoDBHelper.NoticeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //借款单
                    case 3:
                        WorkFlowID = MongoDBHelper.BorrowBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //还款单 
                    case 4:
                        WorkFlowID = MongoDBHelper.RefundBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    default:
                        break;
                }

                List<FlowInstance> Instance = new List<FlowInstance>();
                if (!string.IsNullOrEmpty(WorkFlowID))
                {
                    #region 旧方法
                    #endregion

                    //ActivityStatus节点状态： 0初始化，1开始，2结束
                    //LinkKeyword  任务分配的关键词  0：拒绝   1：同意    2：驳回    3：初始化    4：null   (按钮的显示)
                    //AssignmentKeyword   操作状态  0初始化 1结束 
                    //Status   0拒绝 1同意 2驳回
                    WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                    List<WorkFlowEngine.WorkFlowTable> model = proxy.GetWorkFlowTable(WorkFlowID);
                    List<string> OldPersonName = new List<string>();
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            FlowInstance SingeModel = new FlowInstance();
                            if (item.Name != "开始" && item.Name != "结束" && item.Name != "重新提交")
                            {
                                SingeModel.Description = item.Name;
                                SingeModel.PersonName = new List<string>();
                                SingeModel.KeyWord = new List<int>();
                                SingeModel.StringTime = new List<string>();
                                SingeModel.Remark = "";
                                if (item.Name.Contains("岗1") || item.Name.Contains("岗2"))
                                {
                                    SingeModel.Description = "市场发展中心";
                                    //旧数据，在下面可用
                                    var NodeOne = Instance.Where(c => c.Description.Contains("岗") && c.Description != "品牌审批岗").FirstOrDefault();
                                    if (NodeOne != null)
                                    {
                                        OldPersonName = NodeOne.PersonName;
                                    }
                                }
                                if (item.Name.Contains("岗1"))
                                {
                                    var NodeOne = Instance.Where(c => c.Description.Contains("岗") && c.Description != "品牌审批岗").FirstOrDefault();
                                    if (NodeOne != null)
                                    {
                                        //拒绝了
                                        if (NodeOne.KeyWord.Count > 0 && NodeOne.KeyWord[0] == 0)
                                        {
                                            SingeModel.PersonName = NodeOne.PersonName;
                                            SingeModel.KeyWord = NodeOne.KeyWord;
                                            SingeModel.StringTime = NodeOne.StringTime;
                                            SingeModel.Remark = NodeOne.Remark;
                                        }
                                        Instance.Remove(NodeOne);
                                    }
                                }
                                if (item.Name.Contains("岗2"))
                                {
                                    var NodeOne = Instance.Where(c => c.Description.Contains("市场发展中心")).FirstOrDefault();
                                    if (NodeOne != null)
                                    {
                                        //拒绝了
                                        if (NodeOne.KeyWord.Count > 0 && NodeOne.KeyWord[0] == 0)
                                        {
                                            SingeModel.PersonName = NodeOne.PersonName;
                                            SingeModel.KeyWord = NodeOne.KeyWord;
                                            SingeModel.StringTime = NodeOne.StringTime;
                                            SingeModel.Remark = NodeOne.Remark;
                                        }
                                        Instance.Remove(NodeOne);
                                    }
                                    else
                                    {
                                        var NodeTwo = Instance.Where(c => c.Description.Contains("岗") && c.Description != "品牌审批岗").FirstOrDefault();
                                        if (NodeTwo != null)
                                        {
                                            //拒绝了
                                            if (NodeTwo.KeyWord.Count > 0 && NodeTwo.KeyWord[0] == 0)
                                            {
                                                List<string> str = new List<string>();
                                                str.Add(OldPersonName[0]);
                                                SingeModel.PersonName = str;
                                                SingeModel.KeyWord = NodeTwo.KeyWord;
                                                SingeModel.StringTime = NodeTwo.StringTime;
                                                SingeModel.Remark = NodeTwo.Remark;
                                            }
                                            Instance.Remove(NodeTwo);
                                        }
                                    }
                                }
                                if (item.ActivityStatus == "2" || (item.ActivityStatus == "1" && item.TableLinkList.Where(c => c.UserList.Where(p => p.Status == "2" || p.Status == "1") != null) != null))
                                {
                                    if (item.TableLinkList != null && item.TableLinkList.Count > 0)
                                    {
                                        foreach (var item1 in item.TableLinkList)
                                        {
                                            if (!string.IsNullOrEmpty(item1.AssignmentKeyword) && item1.AssignmentKeyword == "1")
                                            {
                                                if (item1.UserList != null && item1.UserList.Count > 0)
                                                {
                                                    foreach (var item2 in item1.UserList)
                                                    {
                                                        if (item.Name.Contains("岗1") || item.Name.Contains("岗2"))
                                                        {
                                                            if (item2.Status == "1" || item2.Status == "0")
                                                            {
                                                                SingeModel.PersonName = OldPersonName;
                                                            }
                                                            SingeModel.KeyWord.Add(Convert.ToInt32(item2.Status));
                                                            SingeModel.StringTime.Add(item2.DateTime.ToString("yyyy-MM-dd"));
                                                            SingeModel.Remark = item2.Remark;
                                                        }
                                                        else
                                                        {
                                                            SingeModel.PersonName.Add(item2.UserCode);
                                                            SingeModel.KeyWord.Add(Convert.ToInt32(item2.Status));
                                                            SingeModel.StringTime.Add(item2.DateTime.ToString("yyyy-MM-dd"));
                                                            SingeModel.Remark = item2.Remark;
                                                        }

                                                    }
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(item1.AssignmentKeyword) && item1.AssignmentKeyword == "0")
                                            {
                                                if (item1.UserList != null && item1.UserList.Count > 0)
                                                {
                                                    foreach (var item2 in item1.UserList)
                                                    {
                                                        if (item2.Status == "1" || item2.Status == "2")
                                                        {
                                                            SingeModel.PersonName.Add(item2.UserCode);
                                                            SingeModel.KeyWord.Add(Convert.ToInt32(item2.Status));
                                                            SingeModel.StringTime.Add(item2.DateTime.ToString("yyyy-MM-dd"));
                                                            SingeModel.Remark = item2.Remark;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Instance.Add(SingeModel);
                            }
                        }
                    }
                }
                return Instance;
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("获取列表数据失败：" + ex.ToString() + "," + string.Format("WorkFlowID为:{0},BillNo为{1},type为:{2}", WorkFlowID, BillNo, Type));
            }
            return null;
        }


        public List<PostDescription> Getxuqiu(WorkFlowInstance Model)
        {
            List<PostDescription> ReturnModel = new List<PostDescription>();

            if (Model.Activities == null)
            {
                return null;
            }
            foreach (var item in Model.Activities)
            {
                if (Model.Assignments == null)
                {
                    Model.Assignments = new List<WorkFlowEngine.Assignment>();
                }

                PostDescription SingeModel = new PostDescription();

                var id = ObjectId.Parse(item.UnitID);
                var activeId = item._id.ToString();
                var name = Model.WorkFlow.Units.Where(c => c._id == id).Select(x => x.Name).FirstOrDefault();
                var list = Model.Assignments.Where(c => c.ActivityID == activeId && (c.Keyword == 12002 || c.Keyword == 12003 || c.Keyword == 12005)).Select(x => x.UserCode).ToList();
                var keyword = Model.Assignments.Where(c => c.ActivityID == activeId && (c.Keyword == 12002 || c.Keyword == 12003 || c.Keyword == 12005)).Select(x => x.Keyword).ToList();
                var StringTime = Model.Assignments.Where(c => c.ActivityID == activeId && (c.Keyword == 12002 || c.Keyword == 12003 || c.Keyword == 12005)).Select(x => x.updatetime).ToList();
                if (name != "开始" && name != "结束" && name != "重新提交")
                {
                    SingeModel.Post = name;
                    SingeModel.JobNumber = list.FirstOrDefault();
                    SingeModel.Time = StringTime.FirstOrDefault();

                    ReturnModel.Add(SingeModel);
                }
            }

            return ReturnModel;
        }


        /// <summary>
        /// 提供给修改流程人员的接口
        /// </summary>
        /// <param name="BillNo"></param>
        /// <param name="Type"></param>
        /// <param name="WorkFlowID"></param>
        /// <returns></returns>
        public string GetTransferData(string BillNo, out string WorkFlowID)
        {
            string Object_id = string.Empty;

            if (BillNo.Contains("FB"))
            {
                WorkFlowID = MongoDBHelper.FeeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).FirstOrDefault();
                if (string.IsNullOrEmpty(WorkFlowID))
                {
                    WorkFlowID = MongoDBHelper.RefundBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).FirstOrDefault();
                }
            }
            else if (BillNo.Contains("FT"))
            {
                WorkFlowID = MongoDBHelper.NoticeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).FirstOrDefault();
            }
            else if (BillNo.Contains("JS"))
            {
                WorkFlowID = MongoDBHelper.BorrowBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).FirstOrDefault();
            }
            else
            {

                WorkFlowID = MongoDBHelper.RefundBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).FirstOrDefault();
            }

            WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
            List<WorkFlowEngine.WorkFlowTable> model = proxy.GetWorkFlowTable(WorkFlowID);

            var St_1 = model.Where(c => c.ActivityStatus == "1").FirstOrDefault();
            if (St_1 != null)
            {
                var St_2 = St_1.TableLinkList.Where(c => c.UserList.Count > 0).FirstOrDefault();
                if (St_2 != null)
                {
                    Object_id = St_2.UserList[0]._id.ToString();
                }
            }
            return Object_id;
        }

        public List<FlowInstance> GetWorkFlowListPlus(string BillNo, int Type)
        {
            string WorkFlowID = "";
            try
            {
                switch (Type)
                {
                    //费用报销单
                    case 1:
                        WorkFlowID = MongoDBHelper.FeeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //付款通知书
                    case 2:
                        WorkFlowID = MongoDBHelper.NoticeBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //借款单
                    case 3:
                        WorkFlowID = MongoDBHelper.BorrowBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    //还款单 
                    case 4:
                        WorkFlowID = MongoDBHelper.RefundBill.Find(c => c.BillNo == BillNo).Project(x => x.WorkFlowID).Single();
                        break;
                    default:
                        break;
                }

                List<FlowInstance> Instance = new List<FlowInstance>();
                if (!string.IsNullOrEmpty(WorkFlowID))
                {
                    #region 旧方法
                    #endregion

                    //ActivityStatus节点状态： 0初始化，1开始，2结束
                    //LinkKeyword  任务分配的关键词  0：拒绝   1：同意    2：驳回    3：初始化    4：null   (按钮的显示)
                    //AssignmentKeyword   操作状态  0初始化 1结束 
                    //Status   0拒绝 1同意 2驳回
                    WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                    List<WorkFlowEngine.WorkFlowTable> model = proxy.GetWorkFlowTable(WorkFlowID);
                    if (model != null && model.Count > 0)
                    {
                        foreach (var item in model)
                        {
                            FlowInstance SingeModel = new FlowInstance();
                            if (item.Name != "开始" && item.Name != "结束" && item.Name != "重新提交")
                            {
                                SingeModel.Description = item.Name;
                                SingeModel.PersonName = new List<string>();
                                SingeModel.KeyWord = new List<int>();
                                SingeModel.StringTime = new List<string>();
                                SingeModel.Remark = "";
                                SingeModel.AuditList = new List<string>();
                                if (item.ActivityStatus == "2" || (item.ActivityStatus == "1" && item.TableLinkList.Where(c => c.UserList.Where(p => p.Status == "2" || p.Status == "1") != null) != null))
                                {
                                    if (item.TableLinkList != null && item.TableLinkList.Count > 0)
                                    {
                                        SingeModel.ActiveID = item.ActivityID;

                                        foreach (var item1 in item.TableLinkList)
                                        {
                                            if (!string.IsNullOrEmpty(item1.AssignmentKeyword) && item1.AssignmentKeyword == "1")
                                            {
                                                if (item1.UserList != null && item1.UserList.Count > 0)
                                                {
                                                    foreach (var item2 in item1.UserList)
                                                    {
                                                        SingeModel.PersonName.Add(item2.UserCode);
                                                        SingeModel.KeyWord.Add(Convert.ToInt32(item2.Status));
                                                        SingeModel.StringTime.Add(item2.DateTime.ToString("yyyy-MM-dd"));
                                                        SingeModel.Remark = item2.Remark;
                                                    }
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(item1.AssignmentKeyword) && item1.AssignmentKeyword == "0")
                                            {
                                                if (item1.UserList != null && item1.UserList.Count > 0)
                                                {
                                                    foreach (var item2 in item1.UserList)
                                                    {
                                                        if (item2.Status == "1" || item2.Status == "2")
                                                        {
                                                            SingeModel.PersonName.Add(item2.UserCode);
                                                            SingeModel.KeyWord.Add(Convert.ToInt32(item2.Status));
                                                            SingeModel.StringTime.Add(item2.DateTime.ToString("yyyy-MM-dd"));
                                                            SingeModel.Remark = item2.Remark;
                                                        }
                                                        if (item2.Status == "3")
                                                        {
                                                            SingeModel.AuditList.Add(item2.UserCode);
                                                        }
                                                        SingeModel.NodeState = item.ActivityStatus;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Instance.Add(SingeModel);
                            }
                        }
                    }
                }
                return Instance;
            }
            catch (Exception ex)
            {
                Marisfrolg.Public.Logger.Write("获取列表数据失败：" + ex.ToString() + "," + string.Format("WorkFlowID为:{0},BillNo为{1},type为:{2}", WorkFlowID, BillNo, Type));
            }
            return null;
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
                var filter = Builders<FeeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<FeeBillModel>.Update.Set("ApprovalStatus", ApprovalStatus).Set("ApprovalTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
            else
            {
                if (ApprovalStatus == 4)
                {
                    int status = MongoDBHelper.FeeBill.Find(c => c.WorkFlowID == WorkFlowID).Project(x => x.ApprovalStatus).FirstOrDefault();
                    if (status == 4)
                    {
                        return "Success";
                    }
                }
                var filter = Builders<FeeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
                var update = Builders<FeeBillModel>.Update.Set("ApprovalStatus", ApprovalStatus);
                var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
                return result != null && result.ModifiedCount > 0 ? "Success" : "Fail";
            }
        }

        /// <summary>
        /// 修改费用单审批岗位和办结时间
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus"></param>
        /// <param name="PostString"></param>
        /// <returns></returns>
        public string ChangeFeeBillApprovalPost(string WorkFlowID, string PostString)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("WorkFlowID", WorkFlowID);
            var update = Builders<FeeBillModel>.Update.Set("ApprovalPost", PostString);
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result.ModifiedCount > 0 ? "Success" : "Fail";
        }

        /// <summary>
        /// 根据WorkFlowID获取对象
        /// </summary>
        /// <param name="WorkFlowID"></param>
        /// <returns></returns>
        public FeeBillModel GetModelFromWorkFlowID(string WorkFlowID)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.WorkFlowID == WorkFlowID).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 获取未办结的单
        /// </summary>
        /// <param name="Creator"></param>
        /// <returns></returns>
        public List<FeeBillModel> GetNotFinishBill(string Creator)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.Creator == Creator && c.ApprovalStatus != 3 && c.ApprovalStatus != 2).ToList();
            return model;
        }

        /// <summary>
        /// 保存费用单的修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveFeeBillChange(EditBillModel model)
        {
            //费用单有6处改动
            var newModel = MongoDBHelper.FeeBill.Find(c => c.BillNo == model.BillNo).FirstOrDefault();
            if (newModel != null)
            {
                newModel.PersonInfo.Brand = model.Brand;
                newModel.PersonInfo.CostCenter = model.CostCenter;
                newModel.SpecialAttribute = model.SpecialAttribute;
                newModel.Currency = model.Currency;
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
                newModel.PersonInfo.Company = model.E_Company;
                newModel.PersonInfo.CompanyCode = model.E_CompanyCode;

                var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", model.BillNo);
                var result = MongoDBHelper.FeeBill.FindOneAndReplace(filter, newModel);
                return result != null ? "Success" : "Fail";
            }
            return "Fail";
        }

        /// <summary>
        /// 删除费用报销单
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string DelectFeeBill(string BillNo)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<FeeBillModel>.Update.Set("Status", 1);
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        /// <summary>
        /// 物理删除
        /// </summary>
        /// <param name="BillNo"></param>
        /// <returns></returns>
        public string DelePhysicsNo(string BillNo)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            var result = MongoDBHelper.FeeBill.DeleteOne(filter);
            return result != null && result.DeletedCount > 0 ? "Success" : "删除失败";
        }

        /// <summary>
        /// 编辑单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string EditFeeBill(FeeBillModel model)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", model.BillNo);
            var result = MongoDBHelper.FeeBill.FindOneAndReplace(filter, model);
            return result != null ? "Success" : "Fail";
        }


        public List<FeeBillModel> GetReportData()
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.ApprovalStatus != 3 && c.PersonInfo.IsHeadOffice == 0).ToList();
            return model;
        }



        public List<FeeBillModel> GetRelationBillList(string itemName, string DepartmentCode, string ShopCode, DateTime BeginTime)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.PersonInfo.DepartmentCode == DepartmentCode && c.CreateTime >= BeginTime && c.PersonInfo.ShopCode == ShopCode).ToList();
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
            var OldNum = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.BillNo == BillNo).Project(x => x.PrintedCount).FirstOrDefault();
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            var update = Builders<FeeBillModel>.Update.Set("PrintedCount", OldNum + 1);
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<FeeBillModel> CountSmallSortNum(int DepartmentSort, List<string> AreaList, List<string> AccountInfoList, List<int> BillTypesList, DateTime BeginTime, DateTime EndTime, string IsFund, string AreaCodeAndShopCode, List<string> MyCompanycode)
        {
            List<FeeBillModel> Model = new List<FeeBillModel>();
            List<FeeBillModel> DeleteModel = new List<FeeBillModel>();
            if (DepartmentSort == 3)
            {
                Model = MongoDBHelper.FeeBill.Find(c => AreaList.Contains(c.PersonInfo.DepartmentCode) && c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

            }
            else if (DepartmentSort == 2)
            {
                Model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();
            }
            else
            {
                Model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.PersonInfo.IsHeadOffice == DepartmentSort && c.CreateTime >= BeginTime && c.CreateTime <= EndTime).ToList();

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
            var filter = Builders<FeeBillModel>.Filter.Eq("Creator", No1) & Builders<FeeBillModel>.Filter.Where(c => c.ApprovalStatus != 2 & c.ApprovalStatus != 3 && c.Status == 0);
            var update = Builders<FeeBillModel>.Update.Set("Creator", No2).Set("WorkNumber", No2).Set("Owner", Name);
            var result = MongoDBHelper.FeeBill.UpdateMany(filter, update);
            return result.MatchedCount;
        }


        public string PublicEditMethod(string BillNo, Dictionary<string, string> dic)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<FeeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<FeeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public string PublicEditMethod<T>(string BillNo, Dictionary<string, T> dic) where T : class
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<FeeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<FeeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }

        public string PublicEditMethod(string BillNo, Dictionary<string, List<PostDescription>> dic)
        {
            var filter = Builders<FeeBillModel>.Filter.Eq("BillNo", BillNo);
            UpdateDefinition<FeeBillModel> update = null;
            int i = 0;
            foreach (var item in dic)
            {
                if (i == 0)
                {
                    update = Builders<FeeBillModel>.Update.Set("" + item.Key + "", item.Value);
                }
                else
                {
                    update = update.Set("" + item.Key + "", item.Value);
                }
                i++;
            }
            var result = MongoDBHelper.FeeBill.UpdateOne(filter, update);
            return result != null && result.ModifiedCount > 0 ? "Success" : "";
        }


        public List<FeeBillModel> ReturnShelveNo(string EmployeeNo)
        {
            var model = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.ShelveNo == EmployeeNo).ToList();
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

            var list = MongoDBHelper.FeeBill.Find(c => c.Status == 0 && c.ApprovalStatus != 3 && c.PersonInfo != null && c.PersonInfo.IsHeadOffice == 0 && c.Items != null && c.CreateTime >= minTime && c.CreateTime < maxTime).Project(x => new MongoModel
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
        public List<FeeBillModel> GetCompleteBill(string createTime, string endTime, bool PostNotNull)
        {

            DateTime T1 = string.IsNullOrEmpty(createTime) == true ? new DateTime() : Convert.ToDateTime(createTime);
            DateTime T2 = string.IsNullOrEmpty(endTime) == true ? new DateTime(2099, 1, 1) : Convert.ToDateTime(endTime);

            if (PostNotNull)
            {
                return MongoDBHelper.FeeBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString != null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
            else
            {
                return MongoDBHelper.FeeBill.Find(c => c.CreateTime >= T1 && c.CreateTime <= T2 && c.PostString == null && c.Status == 0 && !string.IsNullOrEmpty(c.WorkFlowID) && c.ApprovalStatus == 2).ToList();
            }
        }
    }
}