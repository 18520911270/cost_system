using Marisfrolg.Business;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Marisfrolg.Fee.BLL;
using System.Data.Entity.Validation;
using System.IO;
using Marisfrolg.Fee.Extention;

namespace Marisfrolg.Fee.Controllers
{
    public class WorkFlowController : BaseController
    {

        public PackClass GetPack(string workFlowID, string pack)
        {
            PackClass NewModel = new PackClass();

            if (false && !string.IsNullOrEmpty(pack) && pack != "{}")
            {
                var model = Public.JsonSerializeHelper.DeserializeFromJson<Dictionary<string, string>>(pack);

                if (model.Keys.Contains("pack"))
                {
                    NewModel = Public.JsonSerializeHelper.DeserializeFromJson<PackClass>(model["pack"]);
                }
            }

            if (NewModel == null || string.IsNullOrEmpty(NewModel.Creator))
            {
                var model = GetBillModel(workFlowID);
                NewModel.Creator = model.Creator;
                NewModel.DepartmentCode = model.PersonInfo.DepartmentCode;
                NewModel.Department = model.PersonInfo.Department;
                NewModel.CostCenter = model.PersonInfo.CostCenter;
                NewModel.CompanyCode = model.PersonInfo.CompanyCode;
                NewModel.BillsType = model.BillsType;
                NewModel.Items = model.Items.Select(x => x.name).ToList();
                NewModel.IsHeadOffice = model.PersonInfo.IsHeadOffice;
                NewModel.IsUrgent = model.IsUrgent;
                NewModel.Funds = model.SpecialAttribute.Funds;
                NewModel.Brand = model.PersonInfo.Brand;
            }

            return NewModel;
        }


        /// <summary>
        /// 提供给审批平台的人员查找接口(片区找人流程)
        /// </summary>
        /// <param name="workFlowID">流程id</param>
        /// <param name="type">查找方式</param>
        /// <param name="value">查找方式关键值</param>
        /// <param name="scope">人员管辖范围(1本部门,2店柜品牌,3子公司,4全集团)</param>
        /// <returns></returns>
        public string GetUsersByWorkFlowID(string workFlowID, int type, string value, string pack, int scope = 1)
        {
            Logger.Write("入口参数：" + Request.Url.AbsoluteUri);
            List<Proposer> pro = new List<Proposer>();
            List<FindPerson> PersonList = new List<FindPerson>();
            PackageData pkd = new PackageData();
            StateConde conde = new StateConde();
            try
            {
                #region 找人逻辑，加锁
                var model = GetPack(workFlowID, pack);
                EMPLOYEE employee = null;

                if (model == null || string.IsNullOrEmpty(model.Creator))
                {
                    conde.errorCode = 1;
                    conde.message = "获取人员失败:无法抓取到实例ID";
                    pkd.datas = new List<FindPerson>() { new FindPerson() { No = "01951", Name = "丁毅" } };
                    pkd.code = conde;
                    Logger.Write("记录返回审批人数据：" + string.Format("参数：workFlowID={0},datalist={1}", workFlowID, "获取人员失败:无法抓取到实例ID"), "WorkFlow" + DateTime.Now);
                    return Public.JsonSerializeHelper.SerializeToJson(pkd);
                }

                employee = DbContext.EMPLOYEE.Where(m => m.NO == model.Creator).FirstOrDefault();

                if (employee == null)
                {
                    conde.errorCode = 1;
                    conde.message = "获取人员失败:无法抓取人员对象";
                    pkd.datas = new List<FindPerson>();
                    pkd.code = conde;
                    Logger.Write("记录返回审批人数据：" + string.Format("参数：workFlowID={0},datalist={1}", workFlowID, "获取人员失败:无法抓取人员对象" + DateTime.Now), "WorkFlow");
                    return Public.JsonSerializeHelper.SerializeToJson(pkd);
                }
                switch (type)
                {
                    //使用工号查找本部门领导
                    case 1:
                        //EMPLOYEE employee = DbContext.EMPLOYEE.Where(m => m.NO == proposercode).FirstOrDefault();                  
                        break;
                    //使用角色查找
                    case 2:
                        break;
                    //使用标签查找
                    case 3:
                        break;
                    //使用权限查找
                    case 4:
                        //1.0得到具有该权限集合的所有人              
                        if (value == "中心负责人")
                        {
                            PersonList = GetPermission(value);
                            //得到根部门
                            foreach (var item in PersonList)
                            {
                                decimal i = GetDepartmenRoot(item.No, scope);
                                pro.Add(new Proposer() { Name = item.Name, No = item.No, DepartmentRoot = i });

                            }

                            //得到提交时候的根部门
                            decimal DepartmentRoot = Convert.ToDecimal(model.DepartmentCode);

                            string sql = "SELECT  VALUE,PARAMETERONE,PARAMETERTWO,SIDDEPARTMENTCODE FROM FEE_PERSON_EXTEND where TYPE='department' and DEPARTMENTCODE='" + DepartmentRoot + "' and DEPARTMENTNAME='" + value + "' ";
                            var Database = DbContext.Database.SqlQuery<SpecialArray>(sql).FirstOrDefault();


                            //正常情况下跨部门单还是由原部门老大负责审核
                            if (Database == null)
                            {

                            }
                            else
                            {
                                //严慧
                                if (!string.IsNullOrEmpty(Database.VALUE))
                                {
                                    var obj = GetBrandFromCosterCenter(model.CostCenter);
                                    var ValueData = Database.VALUE.Split(',').ToList();
                                    ValueData.Remove("");

                                    if (!string.IsNullOrEmpty(Database.SIDDEPARTMENTCODE))
                                    {
                                        decimal pid = Convert.ToDecimal(Database.SIDDEPARTMENTCODE);
                                        if (pid == employee.DEPID)
                                        {
                                            var tempdata = AjaxGetName(Database.PARAMETERTWO);
                                            PersonList = new List<FindPerson>(){new FindPerson(){
                                                No=Database.PARAMETERTWO,
                                                Name=tempdata
                                                }};
                                        }
                                        else
                                        {
                                            var tempdata = AjaxGetName(Database.PARAMETERONE);
                                            PersonList = new List<FindPerson>(){new FindPerson(){
                                                No=Database.PARAMETERONE,
                                                Name=tempdata
                                                }};
                                        }
                                    }
                                    else
                                    {
                                        if (ValueData.Count > 0 && ValueData.Contains(obj.NAME))
                                        {
                                            var tempdata = AjaxGetName(Database.PARAMETERONE);
                                            PersonList = new List<FindPerson>(){new FindPerson(){
                                             No=Database.PARAMETERONE,
                                             Name=tempdata
                                            }};
                                        }
                                        else
                                        {
                                            var tempdata = AjaxGetName(Database.PARAMETERTWO);
                                            PersonList = new List<FindPerson>(){new FindPerson(){
                                             No=Database.PARAMETERTWO,
                                             Name=tempdata
                                            }};
                                        }

                                    }
                                    break;
                                }
                                //else--杨燕
                            }


                            //目前只有明佳豪是这种显示为公司，找老大按部门的方式来查找的
                            var IsMatch = DbContext.DEPARTMENT.Where(c => c.ID == DepartmentRoot && (c.PID == 3573 || c.PID == 4786)).FirstOrDefault();
                            if (IsMatch != null)
                            {
                                DepartmentRoot = IsMatch.PID;
                            }
                            //惠州片区和惠州片区AUM
                            if (DepartmentRoot == 4206 || DepartmentRoot == 4307)
                            {
                                DepartmentRoot = 3573;
                            }
                            //还有变态的设计中心和采购中心，显示为部门，找人为按子部门找
                            if (DepartmentRoot == 4905)
                            {
                                DepartmentRoot = GetDepartmenRoot(model.Creator, 1);
                            }
                            //3.0筛选最后符合的人
                            foreach (var item in pro)
                            {
                                if (DepartmentRoot != item.DepartmentRoot)
                                {
                                    PersonList.RemoveAll(c => c.No == item.No);
                                }
                            }

                            //额外扩充数据（特殊部门给指定人员）
                            string sqlString = "SELECT  VALUE  FROM FEE_PERSON_EXTEND where TYPE='extradata' and DEPARTMENTCODE='" + DepartmentRoot + "' and DEPARTMENTNAME='" + value + "' ";
                            var SqlNo = DbContext.Database.SqlQuery<string>(sqlString).FirstOrDefault();

                            if (!string.IsNullOrEmpty(SqlNo))
                            {
                                var name = AjaxGetName(SqlNo);
                                PersonList.Add(new FindPerson() { Name = name, No = SqlNo });
                                break;
                            }

                            //根据人与费用项处理特殊情况
                            string sqlStringNew = "SELECT VALUE,PARAMETERONE,PARAMETERTWO FROM FEE_PERSON_EXTEND where TYPE='EmployeeNo' and DEPARTMENTCODE='" + DepartmentRoot + "' and DEPARTMENTNAME='" + value + "' ";
                            var SqlPerson = DbContext.Database.SqlQuery<SpecialArray>(sqlStringNew).FirstOrDefault();
                            if (SqlPerson != null)
                            {
                                if (SqlPerson.VALUE.Contains(employee.NO))
                                {
                                    bool IsTrue = true;

                                    foreach (var item in model.Items)
                                    {
                                        IsTrue = SqlPerson.PARAMETERTWO.Contains(item);
                                        if (IsTrue == false)
                                        {
                                            break;
                                        }
                                    }


                                    if (IsTrue)
                                    {
                                        var name = AjaxGetName(SqlPerson.PARAMETERONE);
                                        PersonList = new List<FindPerson>() { new FindPerson() { Name = name, No = SqlPerson.PARAMETERONE } };
                                        break;
                                    }
                                }
                            }
                            //中心负责人提单的处理

                            //判断是否是2000公司的人以及是否具有中心负责人权限
                            var p1 = PersonList.Where(c => c.No == employee.NO).FirstOrDefault();
                            if (p1 != null)
                            {
                                var query = (from a in DbContext.EMPLOYEE
                                             join b in DbContext.DEPARTMENT on a.DEPID equals b.ID
                                             where a.NO == employee.NO
                                             select b.COMPANYCODE
                                ).FirstOrDefault();
                                if (query == "2000")
                                {
                                    var name1 = AjaxGetName("00096");
                                    PersonList = new List<FindPerson>() { new FindPerson() { Name = name1, No = "00096" } };

                                    break;
                                }
                            }

                            //处理某个部门下面有一些特殊的人由另外的人进行审批
                            string sqlStringNew3 = "SELECT VALUE,PARAMETERONE FROM FEE_PERSON_EXTEND where TYPE='SuFOB' and DEPARTMENTCODE='" + DepartmentRoot + "' and DEPARTMENTNAME='" + value + "' ";
                            var SqlPerson3 = DbContext.Database.SqlQuery<SpecialArray>(sqlStringNew3).FirstOrDefault();
                            if (SqlPerson3 != null && SqlPerson3.VALUE.Contains(employee.NO))
                            {
                                var name1 = AjaxGetName(SqlPerson3.PARAMETERONE);
                                PersonList = new List<FindPerson>() { new FindPerson() { Name = name1, No = SqlPerson3.PARAMETERONE } };
                                break;
                            }

                        }
                        else if (value == "片区负责人")
                        {
                            PersonList = GetPermission(value);
                            //得到权限
                            foreach (var item in PersonList)
                            {
                                var powerlist = DbContext.UIVALUE.Where(c => c.VALUETYPE == 3 && c.EMPLOYEENO == item.No).Select(x => x.VALUE).FirstOrDefault();
                                pro.Add(new Proposer() { Name = item.Name, No = item.No, PowerList = powerlist });

                            }

                            //2.0筛选具有相同片区权限的人
                            foreach (var item in pro)
                            {
                                if (item.PowerList == null || !item.PowerList.Contains(model.DepartmentCode))
                                {
                                    PersonList.RemoveAll(c => c.No == item.No);
                                }
                            }
                        }
                        else if (value == "品牌审批岗")
                        {
                            string sql = string.Empty;

                            var obj = PublicGetCosterCenter(model.IsHeadOffice, model.CostCenter);

                            if (model.IsHeadOffice == 0)
                            {

                                sql = "select BrandPost from FEE_ACCOUNT_DICTIONARY where code='" + model.BillsType + "' and BRAND='" + obj.NAME + "'";
                            }
                            else
                            {
                                string _brand = "'%" + obj.NAME + "%'";
                                string _Department = "'%" + model.DepartmentCode + "%'";

                                sql = " select VALUE from FEE_PERSON_EXTEND where TYPE='BrandPost' and BRAND like " + _brand + " and DEPARTMENTCODE like " + _Department + "";
                            }
                            var DataBase = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                            if (!string.IsNullOrEmpty(DataBase))
                            {
                                var list = DataBase.Split(',').ToList();
                                list.Remove("");
                                foreach (var item in list)
                                {
                                    var name = AjaxGetName(item);
                                    PersonList.Add(new FindPerson() { No = item, Name = name });
                                }
                            }
                            else
                            {
                                if (obj.CODE == "2100")
                                {
                                    PersonList.Add(new FindPerson() { No = "02125", Name = "郑奕涛" });
                                }
                            }
                        }
                        //审批岗
                        else if (value.Contains("岗"))
                        {
                            if (!value.Contains("岗1") && !value.Contains("岗2"))
                            {
                                value = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == model.BillsType).Select(x => x.NAME).FirstOrDefault();
                            }

                            PersonList = GetPermission(value);

                            var obj = GetBrandFromCosterCenterNew(model.CostCenter);

                            if (value.Contains("岗1"))
                            {
                                //属于活动经费，强制扭转流程
                                if (model.Funds == 1)
                                {
                                    model.BillsType = "FY9";
                                }
                                PersonList = new List<FindPerson>();
                                var query = (from a in DbContext.FEE_ACCOUNT_DICTIONARY
                                             join b in DbContext.EMPLOYEE on a.EXTRAMODEONE equals b.NO
                                             where a.CODE == model.BillsType && a.BRAND == obj.NAME
                                             select new FindPerson
                                             {
                                                 No = b.NO,
                                                 Name = b.NAME
                                             }
                                ).FirstOrDefault();
                                if (query != null)
                                {
                                    PersonList.Add(query);
                                }
                                //为空的情况下
                                else
                                {
                                    query = (from a in DbContext.FEE_ACCOUNT_DICTIONARY
                                             join b in DbContext.EMPLOYEE on a.EXTRAMODEONE equals b.NO
                                             where a.CODE == model.BillsType && a.BRAND == "集团"
                                             select new FindPerson
                                             {
                                                 No = b.NO,
                                                 Name = b.NAME
                                             }
                                 ).FirstOrDefault();
                                    PersonList.Add(query);
                                }
                                break;
                            }

                            if (value.Contains("岗2"))
                            {
                                //属于活动经费，强制扭转流程
                                if (model.Funds == 1)
                                {
                                    model.BillsType = "FY9";
                                }
                                PersonList = new List<FindPerson>();
                                var query = (from a in DbContext.FEE_ACCOUNT_DICTIONARY
                                             join b in DbContext.EMPLOYEE on a.EXTRAMODETWO equals b.NO
                                             where a.CODE == model.BillsType && a.BRAND == obj.NAME
                                             select new FindPerson
                                             {
                                                 No = b.NO,
                                                 Name = b.NAME
                                             }
                                ).FirstOrDefault();
                                if (query != null)
                                {
                                    PersonList.Add(query);
                                }
                                else
                                {
                                    query = (from a in DbContext.FEE_ACCOUNT_DICTIONARY
                                             join b in DbContext.EMPLOYEE on a.EXTRAMODETWO equals b.NO
                                             where a.CODE == "FY10" && a.BRAND == "集团"
                                             select new FindPerson
                                             {
                                                 No = b.NO,
                                                 Name = b.NAME
                                             }
                                             ).FirstOrDefault();
                                    PersonList.Add(query);
                                }
                                break;
                            }


                            //片区且勾选了活动经费(活动经费>会所)
                            if (model.Funds == 1)
                            {
                                PersonList = GetPermission("活动经费");

                                //分公司
                                pro = new List<Proposer>();
                                foreach (var item in PersonList)
                                {
                                    decimal i = GetDepartmenRoot(item.No, 3);
                                    pro.Add(new Proposer() { Name = item.Name, No = item.No, DepartmentRoot = i });
                                }
                                //得到我选择的公司
                                decimal companyRoot = Convert.ToDecimal(model.CompanyCode);

                                foreach (var item in pro)
                                {
                                    if (companyRoot != item.DepartmentRoot)
                                    {
                                        PersonList.RemoveAll(c => c.No == item.No);
                                    }
                                }
                                //不是分公司的数据
                                if (PersonList.Count == 0)
                                {
                                    PersonList = GetPermission("活动经费");
                                }
                                break;
                            }
                            //会所费用归冯研文审批
                            if (model.Department.Contains("会所") && model.Items != null && !(model.Items.Where(c => c == "快递费" || c == "运输费").FirstOrDefault() != null) && !"FY8,FY11,FY13,FY16,FY17".Contains(model.BillsType))
                            {
                                PersonList = GetPermission("会所管理");

                                //分公司
                                pro = new List<Proposer>();
                                foreach (var item in PersonList)
                                {
                                    decimal i = GetDepartmenRoot(item.No, 3);
                                    pro.Add(new Proposer() { Name = item.Name, No = item.No, DepartmentRoot = i });
                                }
                                //得到我选择的公司
                                decimal companyRoot = Convert.ToDecimal(model.CompanyCode);

                                foreach (var item in pro)
                                {
                                    if (companyRoot != item.DepartmentRoot)
                                    {
                                        PersonList.RemoveAll(c => c.No == item.No);
                                    }
                                }
                                //不是分公司的数据
                                if (PersonList.Count == 0)
                                {
                                    PersonList = GetPermission("会所管理");
                                }
                                break;
                            }

                            //5.0借款单也要考虑分品牌
                            var DicModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == model.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                            var BrandName = "";  //品牌名称
                            if (DicModel != null && DicModel.PARTBRAND == 1)
                            {
                                if (DicModel.PARTBRAND == 1)  //分品牌的
                                {
                                    if (model.Brand != null && model.Brand.Count > 0)
                                    {
                                        foreach (var item in model.Brand)
                                        {
                                            if (DicModel.BRANDLIST.Contains(item))
                                            {
                                                BrandName = item;
                                                break;
                                            }
                                        }
                                    }
                                    BrandName = BrandName == "" ? "MA" : BrandName;

                                    //5.0筛选品牌
                                    pro = new List<Proposer>();
                                    foreach (var item in PersonList)
                                    {
                                        var i = DbContext.UIVALUE.Where(c => c.VALUETYPE == 2 && c.EMPLOYEENO == item.No).Select(x => x.VALUE).FirstOrDefault();
                                        pro.Add(new Proposer() { Name = item.Name, No = item.No, PowerList = i });
                                    }
                                    foreach (var item in pro)
                                    {
                                        if (string.IsNullOrEmpty(item.PowerList) || !item.PowerList.Contains(BrandName))
                                        {
                                            PersonList.RemoveAll(c => c.No == item.No);
                                        }
                                    }
                                }
                            }
                            //岗位要分公司，有点公司没有审批岗就给MA审批
                            else
                            {
                                //额外业务审批岗数据
                                string sqlString = "SELECT  VALUE,FYTYPE,FEEINFO  FROM FEE_PERSON_EXTEND where TYPE='extradata' and DEPARTMENTNAME='业务岗' and FYTYPE is not null and FEEINFO is not null ";
                                var SqlValue = DbContext.Database.SqlQuery<SpecialData>(sqlString).ToList();


                                if (SqlValue.Count > 0)
                                {
                                    foreach (var item in SqlValue)
                                    {
                                        if (model.BillsType == item.FYTYPE && model.Items.Where(c => c == item.FEEINFO).FirstOrDefault() != null)
                                        {
                                            var name = AjaxGetName(item.VALUE);
                                            PersonList = new List<FindPerson>() { new FindPerson() { Name = name, No = item.VALUE } };

                                            conde.errorCode = 0;
                                            conde.message = "成功";
                                            pkd.datas = PersonList;
                                            pkd.code = conde;
                                            return Public.JsonSerializeHelper.SerializeToJson(pkd);
                                        }
                                    }
                                }
                                //分公司
                                pro = new List<Proposer>();
                                foreach (var item in PersonList)
                                {
                                    decimal i = GetDepartmenRoot(item.No, 3);
                                    pro.Add(new Proposer() { Name = item.Name, No = item.No, DepartmentRoot = i });
                                }
                                //得到我选择的公司
                                decimal companyRoot = Convert.ToDecimal(model.CompanyCode);
                                //针对明佳豪公司
                                if (companyRoot == 4000)
                                {
                                    companyRoot = 1000;
                                }
                                else if (companyRoot == 1330)
                                {
                                    companyRoot = 1300;
                                }

                                foreach (var item in pro)
                                {
                                    if (companyRoot != item.DepartmentRoot)
                                    {
                                        PersonList.RemoveAll(c => c.No == item.No);
                                    }
                                }
                                //不是分公司的数据,归MA审批
                                if (PersonList.Count == 0)
                                {
                                    PersonList = GetPermission(value);
                                    companyRoot = 1000;
                                    foreach (var item in pro)
                                    {
                                        if (companyRoot != item.DepartmentRoot)
                                        {
                                            PersonList.RemoveAll(c => c.No == item.No);
                                        }
                                    }
                                }
                            }
                        }
                        //财务，出纳，总经办
                        else
                        {
                            PersonList = GetPermission(value);
                            ObjectList obj = null;
                            if (model.IsHeadOffice == 1)
                            {
                                obj = GetBrandFromCosterCenter(model.CostCenter);
                            }
                            else
                            {
                                obj = GetBrandFromCosterCenterNew(model.CostCenter);
                            }
                            List<string> temp = new List<string>();

                            string sql = string.Empty;


                            if (value == "财务会计" && obj.CODE == "1000" && model.IsHeadOffice == 0)
                            {
                                //固定资产类
                                if (model.BillsType == "FY14" || model.Items.Where(c => c == "经费-设备购置费").FirstOrDefault() != null)
                                {
                                    var name1 = AjaxGetName("01392");
                                    PersonList = new List<FindPerson>() { new FindPerson() { Name = name1, No = "01392" } };
                                    break;
                                }

                                sql = "select CODE from FEE_PERSON_CONFIG where value like '%" + model.DepartmentCode + "%' and  BRAND like '%" + obj.NAME + "%' and IDENTITY='" + value + "' and IsHeadOffice=0";
                            }
                            else
                            {
                                sql = "SELECT  VALUE FROM FEE_PERSON_EXTEND where TYPE='system' and Brand='" + obj.NAME + "' and DEPARTMENTNAME='" + value + "' ";

                            }
                            string Database = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                            temp = Database.Split(',').ToList();
                            //总部和片区流程

                            temp.Remove("");
                            PersonList = new List<FindPerson>();
                            foreach (var item in temp)
                            {
                                string PersonName = AjaxGetName(item);
                                FindPerson p = new FindPerson() { No = item, Name = PersonName };
                                PersonList.Add(p);
                            }

                            //额外增加的审批人
                            if (value == "财务会计" && model.Items.Where(c => c == "快递费" || c == "季末退货费" || c == "道具运费").FirstOrDefault() != null)
                            {
                                var name1 = AjaxGetName("01981");
                                var exctrl = new FindPerson() { Name = name1, No = "01981" };

                                if (PersonList.Count > 0)
                                {
                                    PersonList.Add(exctrl);
                                }
                            }

                        }
                        break;
                }
                PersonList = PersonList.Distinct().ToList();

                if (model.IsUrgent == 1)//加急单据，每个环节都需要提醒
                {
                    PersonList.ForEach(c => c.IsNotice = "1");
                }
                if (PersonList.Count == 0)
                {
                    var model1 = new ErrorContainer() { BillType = "LC", WorkFlowID = workFlowID, Createtime = DateTime.Now };
                    new ErrorBill().CreateErrorRecord(model1);
                }
                #endregion
            }
            catch (Exception e)
            {
                conde.errorCode = 1;
                conde.message = "获取人员失败:失败信息为" + e.StackTrace;
                pkd.datas = new List<FindPerson>();
                pkd.code = conde;

                var model = new ErrorContainer() { BillType = "LC", WorkFlowID = workFlowID, Createtime = DateTime.Now };
                new ErrorBill().CreateErrorRecord(model);

                return Public.JsonSerializeHelper.SerializeToJson(pkd);
            }
            Logger.Write("记录返回审批人数据：" + string.Format("参数：workFlowID={0},datalist={1}", workFlowID, Public.JsonSerializeHelper.SerializeToJson(PersonList)), "WorkFlow");
            conde.errorCode = 0;
            conde.message = "成功";
            pkd.datas = PersonList;
            pkd.code = conde;
            return Public.JsonSerializeHelper.SerializeToJson(pkd);
        }
        /// <summary>
        /// 给费用系统提供财务会计的调用方法
        /// </summary>
        /// <returns></returns>
        public string GetPermissionToFeeSystem()
        {
            var list = GetPermission("财务会计");
            return list.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(list);
        }

        //获取具有该权限的所有人
        private List<FindPerson> GetPermission(string value)
        {
            List<FindPerson> PersonList = new List<FindPerson>();
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(" (select x.NAME,x.NO from  EMPLOYEE x join  EMPLOYEE_ROLE a on x.NO=a.EMPLOYEENO join ROLE b on a.ROLEID=b.ID ");
                strSql.Append(" join ROLE_PERMISSION c on b.ID=c.ROLEID join PERMISSION d on c.PERMISSIONID=d.ID where d.APPTYPE ='WORKFLOW' AND  x.LEAVE=0 AND x.available=1 AND d.NAME ='" + value + "') ");
                strSql.Append(" union ");
                strSql.Append(" (select x.NAME,x.NO from EMPLOYEE x join EMPLOYEE_PERMISSION a on x.NO = a.EMPLOYEENO ");
                strSql.Append(" join PERMISSION b on a.PERMISSIONID = b.ID where b.APPTYPE = 'WORKFLOW' AND x.LEAVE=0 AND x.available=1 AND b.NAME = '" + value + "') ");
                PersonList = DbContext.Database.SqlQuery<FindPerson>(strSql.ToString()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Write("获取权限失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",权限为:" + value);

            }
            return PersonList;
        }

        /// <summary>
        /// 获取所在的公司
        /// </summary>
        /// <param name="DepartmenRoot"></param>
        /// <returns></returns>
        private string GetCompanyRoot(decimal DepartmenRoot)
        {
            var str = DbContext.DEPARTMENT.Where(c => c.ID == DepartmenRoot).Select(x => x.COMPANYCODE).FirstOrDefault();
            return str;
        }

        /// <summary>
        /// 区分根部门/所处公司
        /// </summary>
        /// <param name="EmployeeNo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public decimal GetDepartmenRoot(string EmployeeNo, int type)
        {
            decimal pid = new decimal();
            switch (type)
            {
                //本部门
                case 1:
                    EMPLOYEE employee = DbContext.EMPLOYEE.Where(m => m.NO == EmployeeNo).FirstOrDefault();
                    var boot = GetRootDepartment(employee.DEPARTMENT);
                    pid = boot.ID;
                    break;
                //品牌
                case 2:
                    break;
                //子公司 
                case 3:
                    var query = (from a in DbContext.EMPLOYEE
                                 join b in DbContext.DEPARTMENT on a.DEPID equals b.ID
                                 where a.NO == EmployeeNo
                                 select b.COMPANYCODE
                                 ).FirstOrDefault();

                    if (!string.IsNullOrEmpty(query))
                    {
                        pid = Convert.ToDecimal(query);
                    }
                    break;
                //集团(就是不加限制)
                case 4:
                    break;
                //得到顶级部门
                case 5:
                    break;
                default:
                    break;
            }
            return pid;
        }

        /// <summary>
        /// 提供给审批流修改对应单据状态
        /// </summary>
        /// <param name="BillType"></param>
        /// <param name="WorkFlowID"></param>
        /// <param name="ApprovalStatus">状态修改,0审批流程已启动1,驳回,2,流程成功结束，3流程已拒绝,4流程已审批</param>
        /// <returns></returns>

        public string ChangeBillStatus(string WorkFlowID, int ApprovalStatus)
        {

            WriteLog.WebGuiLogNewFile("开启修改单据状态", "ChangeBillStatus", string.Format("参数：WorkFlowID={0},ApprovalStatus={1}", WorkFlowID, ApprovalStatus), "WorkFlow");
            string status = "Fail";
            object data = null;
            string BillNo = string.Empty;
            string ModelName = GetBillModel(WorkFlowID, out data, out BillNo);
            if (data != null)
            {
                switch (ModelName)
                {
                    case "FeeBill":
                        status = new FeeBill().ChangeBillStatus(WorkFlowID, ApprovalStatus);
                        if (ApprovalStatus == 2)
                        {
                            status = CommitToOracleForFeeBill(data);
                        }
                        break;
                    case "NoticeBill":
                        status = new NoticeBill().ChangeBillStatus(WorkFlowID, ApprovalStatus);
                        if (ApprovalStatus == 2)
                        {
                            status = CommitToOracleForNotcieBill(data);
                        }
                        break;
                    case "BorrowBill":
                        status = new BorrowBill().ChangeBillStatus(WorkFlowID, ApprovalStatus);
                        if (ApprovalStatus == 2)
                        {
                            status = CommitToOracleForBorrowBill(data);
                        }
                        break;
                    case "RefundBill":
                        status = new RefundBill().ChangeBillStatus(WorkFlowID, ApprovalStatus);
                        if (ApprovalStatus == 2)
                        {
                            status = RefundOperation(data);  //还款操作
                            RefundBillModel model = data as RefundBillModel;  //得到还款对象
                            if (model.RefundType.ToUpper() != "FEEBILL")
                            {
                                status = CommitToOracleForRefundBill(model);
                            }
                            else
                            {
                                status = CommitToOracleForRefundBill(model);  //提交原费用单还款单
                                var offsetModel = new RefundBill().GetOffsetRecord(model.BillNo);  //得到冲销记录
                                if (offsetModel != null)
                                {
                                    status = CommitToOracleForRefundBill(offsetModel);   //得到冲销单
                                }
                            }

                        }
                        break;
                    default:
                        break;
                }
            }
            WriteLog.WebGuiLogNewFile("结束修改单据状态", "ChangeBillStatus", "status为" + status + "", "WorkFlow");
            if (status != "Success" && ApprovalStatus == 2)
            {
                var model = new ErrorContainer() { BillType = ModelName, WorkFlowID = WorkFlowID, Createtime = DateTime.Now };
                new ErrorBill().CreateErrorRecord(model);
            }
            return "True";
        }

        /// <summary>
        /// 根据特殊子节点查询该用户是否处于子节点下，并将子节点返回
        /// </summary>
        /// <param name="SidList"></param>
        /// <param name="No"></param>
        /// <returns></returns>
        public string ExitstRootNode(List<decimal> SidList, string No)
        {
            try
            {
                var uc_id = DbContext.EMPLOYEE_MUTI_DEPARTMENT.Where(c => c.EMPLOYEENO == No).Select(x => x.FROM_UC).FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw;
            }
            return "";
        }


        /// <summary>
        /// 提供给审批流修改对应单据审批岗位
        /// </summary>
        /// <param name="WorkFlowID">流程ID</param>
        /// <param name="ApprovalStatus">审批状态</param>
        /// <param name="PostString">岗位名称</param>
        /// <returns></returns>
        public string ChangeApprovalPost(string WorkFlowID, string PostString)
        {

            WriteLog.WebGuiLogNewFile("开启修改审批岗位", "ChangeApprovalPost", string.Format("参数：WorkFlowID={0},PostString={1}", WorkFlowID, PostString), "WorkFlow");
            string status = "Fail";
            object data = null;
            string BillNo = string.Empty;
            string ModelName = GetBillModel(WorkFlowID, out data, out BillNo);

            if (PostString.Contains("岗1") || PostString.Contains("岗2"))
            {
                PostString = "市场发展中心";
            }
            else if (PostString.Contains("岗") && PostString != "品牌审批岗")
            {
                switch (ModelName)
                {
                    case "FeeBill":
                        FeeBillModel FeeModel = data as FeeBillModel;
                        if (FeeModel != null)
                        {
                            var obj = GetBrandFromCosterCenterNew(FeeModel.COST_ACCOUNT);
                            var dicmModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == FeeModel.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                            if (dicmModel != null && (!string.IsNullOrEmpty(dicmModel.EXTRAMODEONE) || !string.IsNullOrEmpty(dicmModel.EXTRAMODETWO)))
                            {
                                PostString = "市场发展中心";
                            }
                        }
                        break;
                    case "NoticeBill":
                        NoticeBillModel NoticeModel = data as NoticeBillModel;
                        if (NoticeModel != null)
                        {
                            var obj = GetBrandFromCosterCenterNew(NoticeModel.COST_ACCOUNT);
                            var dicmModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == NoticeModel.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                            if (dicmModel != null && (!string.IsNullOrEmpty(dicmModel.EXTRAMODEONE) || !string.IsNullOrEmpty(dicmModel.EXTRAMODETWO)))
                            {
                                PostString = "市场发展中心";
                            }
                        }
                        break;
                    case "BorrowBill":
                        BorrowBillModel BorrowModel = data as BorrowBillModel;
                        if (BorrowModel != null)
                        {
                            var obj = GetBrandFromCosterCenterNew(BorrowModel.COST_ACCOUNT);
                            var dicmModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == BorrowModel.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                            if (dicmModel != null && (!string.IsNullOrEmpty(dicmModel.EXTRAMODEONE) || !string.IsNullOrEmpty(dicmModel.EXTRAMODETWO)))
                            {
                                PostString = "市场发展中心";
                            }
                        }
                        break;
                    case "RefundBill":
                        RefundBillModel RefundModel = data as RefundBillModel;
                        if (RefundModel != null)
                        {
                            var obj = GetBrandFromCosterCenterNew(RefundModel.COST_ACCOUNT);
                            var dicmModel = DbContext.FEE_ACCOUNT_DICTIONARY.Where(c => c.CODE == RefundModel.BillsType && c.BRAND == obj.NAME).FirstOrDefault();
                            if (dicmModel != null && (!string.IsNullOrEmpty(dicmModel.EXTRAMODEONE) || !string.IsNullOrEmpty(dicmModel.EXTRAMODETWO)))
                            {
                                PostString = "市场发展中心";
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            switch (ModelName)
            {
                case "FeeBill":
                    status = new FeeBill().ChangeFeeBillApprovalPost(WorkFlowID, PostString);
                    break;
                case "NoticeBill":
                    status = new NoticeBill().ChangeNoticeBillApprovalPost(WorkFlowID, PostString);
                    break;
                case "BorrowBill":
                    status = new BorrowBill().ChangeBorrowBillApprovalPost(WorkFlowID, PostString);
                    break;
                case "RefundBill":
                    status = new RefundBill().ChangeRefundBillApprovalPost(WorkFlowID, PostString);
                    break;
                default:
                    break;
            }
            try
            {
                GetWorkFlowData(BillNo, WorkFlowID);
            }
            catch (Exception)
            {
                //不处理
                WriteLog.WebGuiLogNewFile("添加审核节点时间错误", "ChangeApprovalPost", "WorkFlow为" + WorkFlowID + "", "WorkFlow");
            }

            WriteLog.WebGuiLogNewFile("结束修改修改审批岗位", "ChangeApprovalPost", "status为" + status + "", "WorkFlow");
            return "True";
        }

        private FeeBillModelRef GetBillModel(string WorkFlowID)
        {
            FeeBillModelRef model = new FeeBillModelRef();
            var FeeModel = new FeeBill().GetModelFromWorkFlowID(WorkFlowID);
            if (FeeModel != null)
            {
                model = FeeModel.MapTo<FeeBillModel, FeeBillModelRef>();
            }
            var NotcieModel = new NoticeBill().GetModelFromWorkFlowID(WorkFlowID);
            if (NotcieModel != null)
            {
                model.PersonInfo = NotcieModel.PersonInfo;
                model.CreateTime = NotcieModel.CreateTime;
                model.Creator = NotcieModel.Creator;
                model.SpecialAttribute = new SpecialAttribute();
                model.SpecialAttribute.Funds = NotcieModel.SpecialAttribute.Funds;
                model.SpecialAttribute.Agent = NotcieModel.SpecialAttribute.Agent;
                model.SpecialAttribute.Check = NotcieModel.SpecialAttribute.Check;
                model.Items = NotcieModel.Items;
                model.BillsType = NotcieModel.BillsType;
                model.IsUrgent = NotcieModel.IsUrgent;
            }
            var BorrowModel = new BorrowBill().GetModelFromWorkFlowID(WorkFlowID);
            if (BorrowModel != null)
            {
                model.PersonInfo = BorrowModel.PersonInfo;
                model.CreateTime = BorrowModel.CreateTime;
                model.Creator = BorrowModel.Creator;
                model.SpecialAttribute = BorrowModel.SpecialAttribute;
                model.Items = BorrowModel.Items;
                model.BillsType = BorrowModel.BillsType;
                model.IsUrgent = BorrowModel.IsUrgent;
            }
            var RefundModel = new RefundBill().GetModelFromWorkFlowID(WorkFlowID);
            if (RefundModel != null)
            {
                model = RefundModel.MapTo<RefundBillModel, FeeBillModelRef>();
            }
            return model;
        }

        private string GetBillModel(string WorkFlowID, out object data, out string BillNo)
        {
            string Result = "";
            var FeeModel = new FeeBill().GetModelFromWorkFlowID(WorkFlowID);
            if (FeeModel != null)
            {
                data = FeeModel;
                Result = "FeeBill";
                BillNo = FeeModel.BillNo;
                return Result;
            }
            var NotcieModel = new NoticeBill().GetModelFromWorkFlowID(WorkFlowID);
            if (NotcieModel != null)
            {
                data = NotcieModel;
                Result = "NoticeBill";
                BillNo = NotcieModel.BillNo;
                return Result;
            }
            var BorrowModel = new BorrowBill().GetModelFromWorkFlowID(WorkFlowID);
            if (BorrowModel != null)
            {
                data = BorrowModel;
                Result = "BorrowBill";
                BillNo = BorrowModel.BillNo;
                return Result;
            }
            var RefundModel = new RefundBill().GetModelFromWorkFlowID(WorkFlowID);
            if (RefundModel != null)
            {
                data = RefundModel;
                Result = "RefundBill";
                BillNo = RefundModel.BillNo;
                return Result;
            }
            data = null;
            BillNo = "";
            return "";
        }

        public new DEPARTMENT GetRootDepartment(DEPARTMENT son)
        {

            if (son.IS_UCSTAR != 2)
            {
                son.IsHeadOffice = false;
                return son;
            }
            else
            {
                son.IsHeadOffice = true;
            }

            //需要独立的部门维护到表
            string sql = "SELECT  VALUE  FROM FEE_PERSON_EXTEND where TYPE='company'  and  ( DEPARTMENTNAME='BaseController' or  DEPARTMENTNAME='WorkFlowController')";
            List<string> Database = DbContext.Database.SqlQuery<string>(sql).ToList();
            string NewDatabase = "";
            foreach (var item in Database)
            {
                NewDatabase += item + ",";
            }
            List<string> list = NewDatabase.Split(',').Distinct().ToList();
            list.Remove("");
            foreach (var item in list)
            {
                decimal i = Convert.ToDecimal(item);
                if (son.PID == i)
                {
                    son.IsHeadOffice = true;
                    return son;
                }
            }


            var parent = DbContext.DEPARTMENT.SingleOrDefault(s => s.ID == son.PID);
            if (parent != null)
            {
                return GetRootDepartment(parent);
            }
            else
            {
                return son;
            }

        }

        private string CommitToOracleForFeeBill(object data)
        {
            try
            {
                FeeBillModel model = data as FeeBillModel;

                var ErrorModel = DbContext.FEE_FEEBILL.Where(c => c.BILLNO == model.BillNo).FirstOrDefault();
                if (ErrorModel != null)
                {
                    return "Success";
                }

                FEE_FEEBILL FeeModel = new FEE_FEEBILL();
                FeeModel.MONGO_ID = model.Id.ToString();
                FeeModel.BILLNO = model.BillNo;
                FeeModel.CREATOR = model.Creator;
                FeeModel.WORKNUMBER = model.WorkNumber;
                FeeModel.CREATETIME = model.CreateTime;
                FeeModel.COST_ACCOUNT = model.COST_ACCOUNT;
                FeeModel.TRANSACTIONDATE = model.TransactionDate;
                FeeModel.REMARK = model.Remark;
                FeeModel.TOTALMONEY = model.TotalMoney;
                FeeModel.ISHEADOFFICE = (short)model.PersonInfo.IsHeadOffice;
                FeeModel.COMPANYCODE = model.PersonInfo.CompanyCode;
                FeeModel.BOOT_DP_NAME = model.PersonInfo.Department;
                FeeModel.BOOT_DP_ID = Convert.ToInt32(model.PersonInfo.DepartmentCode);
                FeeModel.SHOPCODE = model.PersonInfo.ShopCode;

                #region  多银行数据
                var obj = PublicGetCosterCenter(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter);
                //所处城市
                if (model.PersonInfo.IsHeadOffice == 1)
                {
                    FeeModel.CITY = "总部";
                }
                else
                {
                    if (model.PersonInfo.Department.Contains("会所"))
                    {
                        FeeModel.CITY = model.PersonInfo.Department;
                    }
                    else
                    {
                        string sql = string.Empty;
                        if (string.IsNullOrEmpty(model.PersonInfo.ShopCode))
                        {
                            sql = "select b.shortname from department a left join shop b on a.code=b.code where a.id='" + model.PersonInfo.DepartmentCode + "'";
                        }
                        else
                        {
                            sql = "select  shortname from shop where code='" + model.PersonInfo.ShopCode + "'";
                        }
                        FeeModel.CITY = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    }
                }
                //付款公司代码
                if (string.IsNullOrEmpty(model.PaymentCompanyCode))
                {
                    string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + model.PersonInfo.CostCenter + "%'";
                    var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    if (string.IsNullOrEmpty(pay))
                    {
                        FeeModel.PAYCOMPANYCODE = GetCompanyCode(obj.NAME);
                    }
                    else
                    {
                        FeeModel.PAYCOMPANYCODE = pay;
                    }
                }
                else
                {
                    FeeModel.PAYCOMPANYCODE = model.PaymentCompanyCode;
                }

                FeeModel.ACCOUNTNO = model.CollectionInfo.CardCode;
                FeeModel.ACCOUNTUSERNAME = model.CollectionInfo.Name;
                FeeModel.ACCOUNTSUBBRANCHBANK = model.CollectionInfo.SubbranchBankCode;

                #endregion

                if (model.PersonInfo.Brand != null && model.PersonInfo.Brand.Count > 0)
                {
                    string str = "";
                    foreach (var item in model.PersonInfo.Brand)
                    {
                        str += item + ",";
                    }
                    str = str.Remove(str.Length - 1);
                    FeeModel.BRANDCODE = str;
                }
                FeeModel.IS_TEAMMONEY = (short)model.SpecialAttribute.Funds;
                FeeModel.IS_AGENTMONEY = (short)model.SpecialAttribute.Agent;
                FeeModel.IS_MARKET_MINUS = (short)model.SpecialAttribute.MarketDebt;
                FeeModel.IS_BANK_MINUS = (short)model.SpecialAttribute.BankDebt;
                FeeModel.IS_CASH_MINUS = (short)model.SpecialAttribute.Cash;
                FeeModel.IS_KPI_CHECK = (short)model.SpecialAttribute.Check;
                FeeModel.CURRENCYCODE = model.Currency.Code;
                FeeModel.APPROVALTIME = DateTime.Now;

                //添加发生项
                if (model.Items != null && model.Items.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        //发生项
                        FEE_FEEBILL_ITEMS temp = new FEE_FEEBILL_ITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (FeeModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        FeeModel.FEE_FEEBILL_ITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_FEEBILL_ITEMS newTemp = new FEE_FEEBILL_ITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            FeeModel.FEE_FEEBILL_ITEMS.Add(newTemp);
                        }
                    }
                }
                //添加发票
                if (model.BillsItems != null && model.BillsItems.Count > 0)
                {
                    foreach (var item in model.BillsItems)
                    {
                        //发生项
                        FEE_FEEBILL_BILLITEMS temp = new FEE_FEEBILL_BILLITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (FeeModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        FeeModel.FEE_FEEBILL_BILLITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_FEEBILL_BILLITEMS newTemp = new FEE_FEEBILL_BILLITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            FeeModel.FEE_FEEBILL_BILLITEMS.Add(newTemp);
                        }
                    }
                }

                DbContext.FEE_FEEBILL.Add(FeeModel);
                int result = DbContext.SaveChanges();
                return result > 0 ? "Success" : "Fail";
            }
            catch (Exception ex)
            {
                Logger.Write("费用报销单提交到oracle的数据转换失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }

        private string CommitToOracleForNotcieBill(object data)
        {
            try
            {
                NoticeBillModel model = data as NoticeBillModel;

                var ErrorModel = DbContext.FEE_NOTICEBILL.Where(c => c.BILLNO == model.BillNo).FirstOrDefault();
                if (ErrorModel != null)
                {
                    return "Success";
                }

                FEE_NOTICEBILL NoticeModel = new FEE_NOTICEBILL();
                NoticeModel.MONGO_ID = model.Id.ToString();
                NoticeModel.BILLNO = model.BillNo;
                NoticeModel.CREATOR = model.Creator;
                NoticeModel.WORKNUMBER = model.WorkNumber;
                NoticeModel.CREATETIME = model.CreateTime;
                NoticeModel.COST_ACCOUNT = model.COST_ACCOUNT;
                NoticeModel.TRANSACTIONDATE = model.TransactionDate;
                NoticeModel.REMARK = model.Remark;
                NoticeModel.TOTALMONEY = model.TotalMoney;
                NoticeModel.ISHEADOFFICE = (short)model.PersonInfo.IsHeadOffice;
                NoticeModel.COMPANYCODE = model.PersonInfo.CompanyCode;
                NoticeModel.BOOT_DP_NAME = model.PersonInfo.Department;
                NoticeModel.BOOT_DP_ID = Convert.ToInt32(model.PersonInfo.DepartmentCode);
                NoticeModel.SHOPCODE = model.PersonInfo.ShopCode;

                #region  多银行数据
                var obj = PublicGetCosterCenter(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter);
                //所处城市
                if (model.PersonInfo.IsHeadOffice == 1)
                {
                    NoticeModel.CITY = "总部";
                }
                else
                {
                    if (model.PersonInfo.Department.Contains("会所"))
                    {
                        NoticeModel.CITY = model.PersonInfo.Department;
                    }
                    else
                    {
                        string sql = string.Empty;
                        if (string.IsNullOrEmpty(model.PersonInfo.ShopCode))
                        {
                            sql = "select b.shortname from department a left join shop b on a.code=b.code where a.id='" + model.PersonInfo.DepartmentCode + "'";
                        }
                        else
                        {
                            sql = "select  shortname from shop where code='" + model.PersonInfo.ShopCode + "'";
                        }
                        NoticeModel.CITY = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    }
                }
                //付款公司代码
                if (string.IsNullOrEmpty(model.PaymentCompanyCode))
                {
                    string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + model.PersonInfo.CostCenter + "%'";
                    var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    if (string.IsNullOrEmpty(pay))
                    {
                        NoticeModel.PAYCOMPANYCODE = GetCompanyCode(obj.NAME);
                    }
                    else
                    {
                        NoticeModel.PAYCOMPANYCODE = pay;
                    }
                }
                else
                {
                    NoticeModel.PAYCOMPANYCODE = model.PaymentCompanyCode;
                }

                NoticeModel.ACCOUNTNO = model.ProviderInfo.BankNo;
                NoticeModel.ACCOUNTUSERNAME = model.ProviderInfo.ProviderName;
                NoticeModel.ACCOUNTSUBBRANCHBANK = model.ProviderInfo.SubbranchBankCode;

                var pro = GetSubjectCode(model.PersonInfo.CompanyCode, model.ProviderInfo.ProviderName);
                NoticeModel.OPPPRIVATEFLAG = int.Parse(pro.CODE);
                NoticeModel.CURRENCYCODE = pro.NAME;

                #endregion


                if (model.PersonInfo.Brand != null && model.PersonInfo.Brand.Count > 0)
                {
                    string str = "";
                    foreach (var item in model.PersonInfo.Brand)
                    {
                        str += item + ",";
                    }
                    str = str.Remove(str.Length - 1);
                    NoticeModel.BRANDCODE = str;
                }
                NoticeModel.IS_TEAMMONEY = (short)model.SpecialAttribute.Funds;
                NoticeModel.IS_AGENTMONEY = (short)model.SpecialAttribute.Agent;
                NoticeModel.IS_KPI_CHECK = (short)model.SpecialAttribute.Check;
                //NoticeModel.CURRENCYCODE = model.Currency.Code;
                NoticeModel.PROVIDER_CODE = model.ProviderInfo.ProviderCode; //供应商代码
                NoticeModel.APPROVALTIME = DateTime.Now;

                //添加发生项
                if (model.Items != null && model.Items.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        //发生项
                        FEE_NOTICEBILL_ITEMS temp = new FEE_NOTICEBILL_ITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (NoticeModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        NoticeModel.FEE_NOTICEBILL_ITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_NOTICEBILL_ITEMS newTemp = new FEE_NOTICEBILL_ITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            NoticeModel.FEE_NOTICEBILL_ITEMS.Add(newTemp);
                        }
                    }
                }

                //添加发票信息
                if (model.BillsItems != null && model.BillsItems.Count > 0)
                {
                    foreach (var item in model.BillsItems)
                    {
                        //发生项
                        FEE_NOTICEBILL_BILLITEMS temp = new FEE_NOTICEBILL_BILLITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (NoticeModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        NoticeModel.FEE_NOTICEBILL_BILLITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_NOTICEBILL_BILLITEMS newTemp = new FEE_NOTICEBILL_BILLITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            NoticeModel.FEE_NOTICEBILL_BILLITEMS.Add(newTemp);
                        }
                    }
                }

                DbContext.FEE_NOTICEBILL.Add(NoticeModel);
                int result = DbContext.SaveChanges();
                return result > 0 ? "Success" : "Fail";
            }
            catch (DbEntityValidationException dbEx)
            {
                Logger.Write("付款通知书提交到oracle的数据转换失败：" + dbEx.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }

        private string CommitToOracleForBorrowBill(object data)
        {
            try
            {
                BorrowBillModel model = data as BorrowBillModel;

                var ErrorModel = DbContext.FEE_BORROWBILL.Where(c => c.BILLNO == model.BillNo).FirstOrDefault();
                if (ErrorModel != null)
                {
                    return "Success";
                }

                FEE_BORROWBILL BorrowModel = new FEE_BORROWBILL();
                BorrowModel.MONGO_ID = model.Id.ToString();
                BorrowModel.BILLNO = model.BillNo;
                BorrowModel.CREATOR = model.Creator;
                BorrowModel.WORKNUMBER = model.WorkNumber;
                BorrowModel.CREATETIME = model.CreateTime;
                BorrowModel.COST_ACCOUNT = model.COST_ACCOUNT;
                BorrowModel.TRANSACTIONDATE = model.TransactionDate;
                BorrowModel.REMARK = model.Remark;
                BorrowModel.TOTALMONEY = model.TotalMoney;
                BorrowModel.ISHEADOFFICE = (short)model.PersonInfo.IsHeadOffice;
                BorrowModel.COMPANYCODE = model.PersonInfo.CompanyCode;
                BorrowModel.BOOT_DP_NAME = model.PersonInfo.Department;
                BorrowModel.BOOT_DP_ID = Convert.ToInt32(model.PersonInfo.DepartmentCode);
                BorrowModel.SHOPCODE = model.PersonInfo.ShopCode;
                BorrowModel.IS_KPI_CHECK = 1;  //默认考核
                BorrowModel.CURRENCYCODE = model.Currency.Code;
                BorrowModel.APPROVALTIME = DateTime.Now;

                #region  多银行数据
                var obj = PublicGetCosterCenter(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter);
                //所处城市
                if (model.PersonInfo.IsHeadOffice == 1)
                {
                    BorrowModel.CITY = "总部";
                }
                else
                {
                    if (model.PersonInfo.Department.Contains("会所"))
                    {
                        BorrowModel.CITY = model.PersonInfo.Department;
                    }
                    else
                    {
                        string sql = string.Empty;
                        if (string.IsNullOrEmpty(model.PersonInfo.ShopCode))
                        {
                            sql = "select b.shortname from department a left join shop b on a.code=b.code where a.id='" + model.PersonInfo.DepartmentCode + "'";
                        }
                        else
                        {
                            sql = "select  shortname from shop where code='" + model.PersonInfo.ShopCode + "'";
                        }
                        BorrowModel.CITY = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    }
                }
                //付款公司代码
                if (string.IsNullOrEmpty(model.PaymentCompanyCode))
                {
                    string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + model.PersonInfo.CostCenter + "%'";
                    var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                    if (string.IsNullOrEmpty(pay))
                    {
                        BorrowModel.PAYCOMPANYCODE = GetCompanyCode(obj.NAME);
                    }
                    else
                    {
                        BorrowModel.PAYCOMPANYCODE = pay;
                    }
                }
                else
                {
                    BorrowModel.PAYCOMPANYCODE = model.PaymentCompanyCode;
                }

                BorrowModel.ACCOUNTNO = model.CollectionInfo.CardCode;
                BorrowModel.ACCOUNTUSERNAME = model.CollectionInfo.Name;
                BorrowModel.ACCOUNTSUBBRANCHBANK = model.CollectionInfo.SubbranchBankCode;

                #endregion

                if (model.PersonInfo.Brand != null && model.PersonInfo.Brand.Count > 0)
                {
                    string str = "";
                    foreach (var item in model.PersonInfo.Brand)
                    {
                        str += item + ",";
                    }
                    str = str.Remove(str.Length - 1);
                    BorrowModel.BRANDCODE = str;
                }
                //添加发生项
                if (model.Items != null && model.Items.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        //发生项
                        FEE_BORROWBILL_ITEMS temp = new FEE_BORROWBILL_ITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (BorrowModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        BorrowModel.FEE_BORROWBILL_ITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_BORROWBILL_ITEMS newTemp = new FEE_BORROWBILL_ITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            BorrowModel.FEE_BORROWBILL_ITEMS.Add(newTemp);
                        }
                    }
                }

                //添加发票信息
                if (model.BillsItems != null && model.BillsItems.Count > 0)
                {
                    foreach (var item in model.BillsItems)
                    {
                        //发生项
                        FEE_BORROWBILL_BILLITEMS temp = new FEE_BORROWBILL_BILLITEMS();
                        temp.ACCOUNT_CODE = item.code;
                        temp.ACCOUNT_NAME = item.name;
                        temp.MONEY = item.money;
                        temp.REASON_CODE = item.reason_code;
                        var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                        if (BorrowModel.IS_KPI_CHECK == 1 && ischeck == 1)
                        {
                            temp.IS_KPI_CHECK = 1;
                        }
                        else
                        {
                            temp.IS_KPI_CHECK = 0;
                        }
                        BorrowModel.FEE_BORROWBILL_BILLITEMS.Add(temp);
                        if (item.taxmoney > 0)
                        {
                            FEE_BORROWBILL_BILLITEMS newTemp = new FEE_BORROWBILL_BILLITEMS();
                            newTemp.ACCOUNT_CODE = item.taxcode;
                            string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                            if (value == "0")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                            }
                            else if (value == "1")
                            {
                                newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                            }
                            newTemp.MONEY = item.taxmoney;
                            newTemp.IS_KPI_CHECK = 0; //税额不考核
                            BorrowModel.FEE_BORROWBILL_BILLITEMS.Add(newTemp);
                        }
                    }
                }

                DbContext.FEE_BORROWBILL.Add(BorrowModel);
                int result = DbContext.SaveChanges();
                return result > 0 ? "Success" : "Fail";
            }
            catch (Exception ex)
            {
                Logger.Write("借款单提交到oracle的数据转换失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }

        private string CommitToOracleForRefundBill(RefundBillModel model)
        {
            try
            {
                //费用还款
                if (model.RefundType.ToUpper() == "FEEBILL")
                {

                    var ErrorModel = DbContext.FEE_FEEREFUNDBILL.Where(c => c.BILLNO == model.BillNo).FirstOrDefault();
                    if (ErrorModel != null)
                    {
                        return "Success";
                    }

                    FEE_FEEREFUNDBILL FeeRefundModel = new FEE_FEEREFUNDBILL();
                    FeeRefundModel.MONGO_ID = model.Id.ToString();
                    FeeRefundModel.BILLNO = model.BillNo;
                    FeeRefundModel.CREATOR = model.Creator;
                    FeeRefundModel.WORKNUMBER = model.WorkNumber;
                    FeeRefundModel.CREATETIME = model.CreateTime;
                    FeeRefundModel.COST_ACCOUNT = model.COST_ACCOUNT;
                    FeeRefundModel.TRANSACTIONDATE = model.TransactionDate;
                    FeeRefundModel.REMARK = model.Remark;
                    FeeRefundModel.TOTALMONEY = model.RealRefundMoney;
                    FeeRefundModel.ISHEADOFFICE = (short)model.PersonInfo.IsHeadOffice;
                    FeeRefundModel.COMPANYCODE = model.PersonInfo.CompanyCode;
                    FeeRefundModel.BOOT_DP_NAME = model.PersonInfo.Department;
                    FeeRefundModel.BOOT_DP_ID = Convert.ToInt32(model.PersonInfo.DepartmentCode);
                    FeeRefundModel.SHOPCODE = model.PersonInfo.ShopCode;
                    FeeRefundModel.APPROVALTIME = DateTime.Now;


                    #region  多银行数据
                    var obj = PublicGetCosterCenter(model.PersonInfo.IsHeadOffice, model.PersonInfo.CostCenter);
                    //所处城市
                    if (model.PersonInfo.IsHeadOffice == 1)
                    {
                        FeeRefundModel.CITY = "总部";
                    }
                    else
                    {
                        if (model.PersonInfo.Department.Contains("会所"))
                        {
                            FeeRefundModel.CITY = model.PersonInfo.Department;
                        }
                        else
                        {
                            string sql = string.Empty;
                            if (string.IsNullOrEmpty(model.PersonInfo.ShopCode))
                            {
                                sql = "select b.shortname from department a left join shop b on a.code=b.code where a.id='" + model.PersonInfo.DepartmentCode + "'";
                            }
                            else
                            {
                                sql = "select  shortname from shop where code='" + model.PersonInfo.ShopCode + "'";
                            }
                            FeeRefundModel.CITY = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                        }
                    }
                    //付款公司代码
                    if (string.IsNullOrEmpty(model.PaymentCompanyCode))
                    {
                        string sql = "select COMPANYCODE from FEE_PERSON_EXTEND where type='PaymentCode' and VALUE like '%" + model.PersonInfo.CostCenter + "%'";
                        var pay = DbContext.Database.SqlQuery<string>(sql).FirstOrDefault();
                        if (string.IsNullOrEmpty(pay))
                        {
                            FeeRefundModel.PAYCOMPANYCODE = GetCompanyCode(obj.NAME);
                        }
                        else
                        {
                            FeeRefundModel.PAYCOMPANYCODE = pay;
                        }
                    }
                    else
                    {
                        FeeRefundModel.PAYCOMPANYCODE = model.PaymentCompanyCode;
                    }

                    FeeRefundModel.ACCOUNTNO = model.CollectionInfo.CardCode;
                    FeeRefundModel.ACCOUNTUSERNAME = model.CollectionInfo.Name;
                    FeeRefundModel.ACCOUNTSUBBRANCHBANK = model.CollectionInfo.SubbranchBankCode;

                    if (model.DebtMoney == 0)
                    {
                        FeeRefundModel.LOANMONEY = model.RealRefundMoney;
                    }
                    else
                    {
                        FeeRefundModel.LOANMONEY = model.DebtMoney;
                    }

                    #endregion

                    if (model.PersonInfo.Brand != null && model.PersonInfo.Brand.Count > 0)
                    {
                        string str = "";
                        foreach (var item in model.PersonInfo.Brand)
                        {
                            str += item + ",";
                        }
                        str = str.Remove(str.Length - 1);
                        FeeRefundModel.BRANDCODE = str;
                    }
                    FeeRefundModel.IS_TEAMMONEY = (short)model.SpecialAttribute.Funds;
                    FeeRefundModel.IS_AGENTMONEY = (short)model.SpecialAttribute.Agent;
                    FeeRefundModel.IS_MARKET_MINUS = (short)model.SpecialAttribute.MarketDebt;
                    FeeRefundModel.IS_BANK_MINUS = (short)model.SpecialAttribute.BankDebt;
                    FeeRefundModel.IS_CASH_MINUS = (short)model.SpecialAttribute.Cash;
                    FeeRefundModel.IS_KPI_CHECK = (short)model.SpecialAttribute.Check;
                    FeeRefundModel.CURRENCYCODE = model.Currency.Code;

                    //添加发生项
                    if (model.Items != null && model.Items.Count > 0)
                    {
                        foreach (var item in model.Items)
                        {
                            //发生项
                            FEE_FEEREFUNDBILL_ITEMS temp = new FEE_FEEREFUNDBILL_ITEMS();
                            temp.ACCOUNT_CODE = item.code;
                            temp.ACCOUNT_NAME = item.name;
                            temp.MONEY = item.money;
                            temp.REASON_CODE = item.reason_code;
                            var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                            if (FeeRefundModel.IS_KPI_CHECK == 1 && ischeck == 1)
                            {
                                temp.IS_KPI_CHECK = 1;
                            }
                            else
                            {
                                temp.IS_KPI_CHECK = 0;
                            }
                            FeeRefundModel.FEE_FEEREFUNDBILL_ITEMS.Add(temp);
                            if (item.taxmoney > 0)
                            {
                                FEE_FEEREFUNDBILL_ITEMS newTemp = new FEE_FEEREFUNDBILL_ITEMS();
                                newTemp.ACCOUNT_CODE = item.taxcode;
                                string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                                if (value == "0")
                                {
                                    newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                                }
                                else if (value == "1")
                                {
                                    newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                                }
                                newTemp.MONEY = item.taxmoney;
                                newTemp.IS_KPI_CHECK = 0; //税额不考核
                                FeeRefundModel.FEE_FEEREFUNDBILL_ITEMS.Add(newTemp);
                            }
                        }
                    }

                    //添加发票信息
                    if (model.BillsItems != null && model.BillsItems.Count > 0)
                    {
                        foreach (var item in model.BillsItems)
                        {
                            //发生项
                            FEE_FEEREFUNDBILL_BLITEMS temp = new FEE_FEEREFUNDBILL_BLITEMS();
                            temp.ACCOUNT_CODE = item.code;
                            temp.ACCOUNT_NAME = item.name;
                            temp.MONEY = item.money;
                            temp.REASON_CODE = item.reason_code;
                            var ischeck = DbContext.FEE_ACCOUNT.Where(c => c.ACCOUNT == item.code).Select(x => x.IS_MARKET).FirstOrDefault();
                            if (FeeRefundModel.IS_KPI_CHECK == 1 && ischeck == 1)
                            {
                                temp.IS_KPI_CHECK = 1;
                            }
                            else
                            {
                                temp.IS_KPI_CHECK = 0;
                            }
                            FeeRefundModel.FEE_FEEREFUNDBILL_BLITEMS.Add(temp);
                            if (item.taxmoney > 0)
                            {
                                FEE_FEEREFUNDBILL_BLITEMS newTemp = new FEE_FEEREFUNDBILL_BLITEMS();
                                newTemp.ACCOUNT_CODE = item.taxcode;
                                string value = item.taxcode.Remove(0, item.taxcode.Length - 1);
                                if (value == "0")
                                {
                                    newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税";
                                }
                                else if (value == "1")
                                {
                                    newTemp.ACCOUNT_NAME = "应交税费-增值税-进项税(专卖店)";
                                }
                                newTemp.MONEY = item.taxmoney;
                                newTemp.IS_KPI_CHECK = 0; //税额不考核
                                FeeRefundModel.FEE_FEEREFUNDBILL_BLITEMS.Add(newTemp);
                            }
                        }
                    }

                    DbContext.FEE_FEEREFUNDBILL.Add(FeeRefundModel);
                }
                //现金还款
                else if (model.RefundType.ToUpper() == "CASH")
                {

                    var ErrorModel = DbContext.FEE_CASHREFUNDBILL.Where(c => c.BILLNO == model.BillNo).FirstOrDefault();
                    if (ErrorModel != null)
                    {
                        return "Success";
                    }

                    FEE_CASHREFUNDBILL CashRefundBill = new FEE_CASHREFUNDBILL();
                    CashRefundBill.MONGO_ID = model.Id.ToString();
                    CashRefundBill.BILLNO = model.BillNo;
                    CashRefundBill.CREATOR = model.Creator;
                    CashRefundBill.WORKNUMBER = model.WorkNumber;
                    CashRefundBill.CREATETIME = model.CreateTime;
                    CashRefundBill.COST_ACCOUNT = model.COST_ACCOUNT;
                    CashRefundBill.TRANSACTIONDATE = model.TransactionDate;
                    CashRefundBill.REMARK = model.Remark;
                    CashRefundBill.REALMONEY = model.RealRefundMoney;
                    CashRefundBill.ISHEADOFFICE = (short)model.PersonInfo.IsHeadOffice;
                    CashRefundBill.COMPANYCODE = model.PersonInfo.CompanyCode;
                    CashRefundBill.BOOT_DP_NAME = model.PersonInfo.Department;
                    CashRefundBill.BOOT_DP_ID = Convert.ToInt32(model.PersonInfo.DepartmentCode);
                    CashRefundBill.SHOPCODE = model.PersonInfo.ShopCode;
                    CashRefundBill.CURRENCYCODE = model.Currency.Code;
                    CashRefundBill.FLAG = model.Flag.ToString();
                    CashRefundBill.APPROVALTIME = DateTime.Now;
                    DbContext.FEE_CASHREFUNDBILL.Add(CashRefundBill);
                }
                int result = DbContext.SaveChanges();
                return result > 0 ? "Success" : "Fail";
            }
            catch (Exception ex)
            {
                Logger.Write("还款单提交到oracle的数据转换：" + ex.ToString() + ex.StackTrace + "," + System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "Fail";
            }
        }

        public string RefundOperation(object data)
        {
            RefundBillModel model = data as RefundBillModel;
            string result = new BorrowBill().DealUnualRefundBill(model);
            return result;
        }

        public string GetMyProcessCount(string EmployeeNo)
        {
            PackageData pkd = new PackageData();
            StateConde conde = new StateConde();
            List<RetrunBillCount> Count = new List<RetrunBillCount>();
            try
            {
                WorkFlowProxy.WorkFlowProxy proxy = new WorkFlowProxy.WorkFlowProxy();
                Dictionary<string, WorkFlowEngine.WorkFlowInstance> dic = new Dictionary<string, WorkFlowEngine.WorkFlowInstance>();
                string result = proxy.GetWorkFlowTaskList(EmployeeNo, "", ref dic);
                if (!string.IsNullOrEmpty(result) && dic.Count > 0)
                {
                    List<MongoDB.Bson.ObjectId> Mylist = dic.Select(c => c.Value).Select(c => c._id).ToList();
                    List<string> WorkFlowList = new List<string>();
                    foreach (var item in Mylist)
                    {
                        WorkFlowList.Add(item.ToString());
                    }

                    //未审批的费用报销单

                    var FeeModel = new Marisfrolg.Fee.BLL.FeeBill().GetMyProcess(EmployeeNo, WorkFlowList);

                    //未审批的付款通知书  

                    var NoticeModel = new Marisfrolg.Fee.BLL.NoticeBill().GetMyProcess(EmployeeNo, WorkFlowList);

                    //未审批的借款单  

                    var BorrowModel = new Marisfrolg.Fee.BLL.BorrowBill().GetMyProcess(EmployeeNo, WorkFlowList);

                    //未审批的还款单  

                    var RefundModel = new Marisfrolg.Fee.BLL.RefundBill().GetMyProcess(EmployeeNo, WorkFlowList);

                    Count.Add(new RetrunBillCount() { BillType = "FEE", BillCount = FeeModel.Count.ToString() });
                    Count.Add(new RetrunBillCount() { BillType = "NOTICE", BillCount = NoticeModel.Count.ToString() });
                    Count.Add(new RetrunBillCount() { BillType = "BORROW", BillCount = BorrowModel.Count.ToString() });
                    Count.Add(new RetrunBillCount() { BillType = "REFUND", BillCount = RefundModel.Count.ToString() });
                }
                Logger.Write("获取我审批的任务：" + string.Format("参数：工号为{0},返回数据为{1}", EmployeeNo, Public.JsonSerializeHelper.SerializeToJson(Count)), "WorkFlow");
                conde.errorCode = 0;
                conde.message = "成功";
                pkd.datas = Count;
                pkd.code = conde;
                return Public.JsonSerializeHelper.SerializeToJson(pkd);
            }
            catch (Exception ex)
            {
                WriteLog.WebGuiInLog("获取我审批的任务" + ex.ToString(), "首页控制器GetMyProcessCount", "");
            }
            return "";
        }

        public string ExternalCall(int Year, int Month)
        {
            string error = string.Empty;

            List<ExternalModel> Model = new List<ExternalModel>();

            var M1 = new FeeBill().GetListByYearAndMonth(Year, Month);
            var M2 = new NoticeBill().GetListByYearAndMonth(Year, Month);

            foreach (var item in M1)
            {
                foreach (var item1 in item.Items)
                {
                    Model.Add(new ExternalModel() { BillNo = item.BillNo, BusinessDate = item.BusinessDate.ToString("yyyy-MM-dd"), Money = item1.money + item1.taxmoney, SubjectName = item1.name, SubjectCode = item1.code, DepartmentID = item.DepartmentCode, ShopCode = item.ShopCode, CreateTime = item.CreateTime.ToString("yyyy-MM-dd") });
                }
            }

            foreach (var item in M2)
            {
                foreach (var item1 in item.Items)
                {
                    Model.Add(new ExternalModel() { BillNo = item.BillNo, BusinessDate = item.BusinessDate.ToString("yyyy-MM-dd"), Money = item1.money + item1.taxmoney, SubjectName = item1.name, SubjectCode = item1.code, DepartmentID = item.DepartmentCode, ShopCode = item.ShopCode, CreateTime = item.CreateTime.ToString("yyyy-MM-dd") });
                }
            }

            Model.Where(c => c.ShopCode == null).ToList().ForEach(c => c.ShopCode = "");

            error = Model.Count > 0 ? "0" : "1";

            return Public.JsonSerializeHelper.SerializeToJson(new { error = error, data = Model });
        }

        public ObjectList GetSubjectCode(string companyCode, string providerName)
        {
            //1为个人，2为公司，3为外汇
            int grade = 2;
            if (string.IsNullOrEmpty(providerName))
            {
                grade = 1;
            }
            else
            {
                if (providerName.Length <= 4)
                {
                    grade = 1;
                }
                else if (providerName.Length > 4)
                {
                    string str = providerName.Remove(1);
                    string pattern = @"^[a-zA-Z]*$";
                    bool Istrue = System.Text.RegularExpressions.Regex.IsMatch(str, pattern);
                    if (Istrue)
                    {
                        grade = 3;
                    }
                    else
                    {
                        grade = 2;
                    }
                }
            }

            string sql = "select GRADE as CODE,CURRENCY as NAME from FEE_SAPINFO where APPNAME='DefaultBank'  and COMPANYCODE='" + companyCode + "' and GRADE='" + grade + "'";
            var Database = DbContext.Database.SqlQuery<ObjectList>(sql).FirstOrDefault();

            return Database;
        }

        void GetWorkFlowData(string BillNo, string workFlowId)
        {
            //得到数据
            FeeBill FB = new FeeBill();
            NoticeBill FT = new NoticeBill();
            BorrowBill JS = new BorrowBill();
            RefundBill RB = new RefundBill();

            List<PostDescription> ListModel = new List<PostDescription>();
            string url = "http://192.168.2.14//WorkFlowServer/WorkFlowServer" + "/GetWorkFlowListByIDs";
            string postDataStr = String.Format("ids={0}", workFlowId);

            string Character = HttpGet(url, postDataStr);
            var ObjModel = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorkFlowEngine.WorkFlowInstance>>(Character);

            var obj1 = FB.Getxuqiu(ObjModel[0]);

            Dictionary<string, List<PostDescription>> dic = new Dictionary<string, List<PostDescription>>();
            dic.Add("PostString", obj1);

            if (BillNo.Contains("FB"))
            {
                var IsFee = FB.GetBillModel(BillNo) != null;
                if (IsFee)
                {
                    FB.PublicEditMethod(BillNo, dic);
                }
                else
                {
                    RB.PublicEditMethod(BillNo, dic);
                }
            }
            else if (BillNo.Contains("FT"))
            {
                FT.PublicEditMethod(BillNo, dic);
            }
            else if (BillNo.Contains("JS"))
            {
                JS.PublicEditMethod(BillNo, dic);
            }
            else if (BillNo.Contains("HK"))
            {
                RB.PublicEditMethod(BillNo, dic);
            }
        }
    }
}
