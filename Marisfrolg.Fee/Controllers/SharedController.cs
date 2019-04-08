using Marisfrolg.Business;
using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Marisfrolg.Fee.Controllers
{
    public class SharedController : SecurityController
    {

        private delegate List<Person> MyDelegate1(string EmployeeNo, string CompanyCode, string DepartmentId);
        private delegate List<Person> MyDelegate2(string DepartmentId);
        private delegate List<Person> MyDelegate3(string EmployeeNo);

        /// <summary>
        /// 修改报销人
        /// </summary>
        /// <param name="IsHeadOffice">总部/片区</param>
        /// <param name="EmployeeNo">工号</param>
        /// <param name="DepartmentId">部门id</param>
        /// <returns></returns>
        [HttpGet]
        public string ChangePerson(string IsHeadOffice, string EmployeeNo, string CompanyCode, string DepartmentId = "", string ShopId = "")
        {
            List<Person> person = new List<Person>();
            try
            {
                //总部获取部门人员方法
                if (IsHeadOffice == "1")
                {
                    //EMPLOYEE_MUTI_DEPARTMENT找人
                    MyDelegate2 My2 = new MyDelegate2(NewMethod2);
                    IAsyncResult My2Result = My2.BeginInvoke(DepartmentId, null, null);
                    //人员表找人
                    MyDelegate3 My3 = new MyDelegate3(GetDepartmentAllPerson);
                    IAsyncResult My3Result = My3.BeginInvoke(EmployeeNo, null, null);

                    var list1 = My2.EndInvoke(My2Result);
                    var list2 = My3.EndInvoke(My3Result);

                    person.AddRange(list1);
                    person.AddRange(list2);

                    person = person.GroupBy(c => c.No).Select(p => new Person { No = p.Key, Name = p.FirstOrDefault().Name }).ToList();

                }
                //片区获取部门人员的方法  1.人员表  2.权限  3.EMPLOYEE_MUTI_DEPARTMENT表(使用线程委托的方式查找)
                else
                {
                    MyDelegate1 My1 = new MyDelegate1(NewMethod);
                    MyDelegate2 My2 = new MyDelegate2(NewMethod2);


                    IAsyncResult My1Result = My1.BeginInvoke(EmployeeNo, CompanyCode, DepartmentId, null, null);
                    IAsyncResult My2Result = My2.BeginInvoke(DepartmentId, null, null);

                    //权限找人
                    var list1 = My1.EndInvoke(My1Result);
                    //EMPLOYEE_MUTI_DEPARTMENT找人
                    var list2 = My2.EndInvoke(My2Result);
                    //人员表找人
                    var data = Convert.ToDecimal(DepartmentId);
                    var list3 = DbContext.EMPLOYEE.Where(c => c.DEPID == data && c.AVAILABLE == "1" && c.LEAVE == "0").Select(x => new Person()
                   {
                       No = x.NO,
                       Name = x.NAME
                   }).ToList();
                    List<string> DeleteList3 = new List<string>();

                    person.AddRange(list1);
                    person.AddRange(list2);
                    person.AddRange(list3);

                    //去重
                    var list4 = person.GroupBy(c => c.No).Select(p => new Person { No = p.Key, Name = p.FirstOrDefault().Name }).ToList();
                    person = list4;

                    if (!string.IsNullOrEmpty(ShopId))
                    {
                        foreach (var item in person)
                        {
                            //从uivalue获取片区权限
                            var value = DbContext.UIVALUE.Where(c => c.VALUETYPE == 4 && c.EMPLOYEENO == item.No).Select(x => x.VALUE).FirstOrDefault();
                            var ShopCode = DbContext.EMPLOYEE.Where(c => c.NO == item.No && c.AVAILABLE == "1" && c.LEAVE == "0").Select(x => x.SHOPCODE).FirstOrDefault();
                            //从人员信息表中获取门店
                            if (!string.IsNullOrEmpty(ShopCode))
                            {
                                value = value == null ? ShopCode : (value + "," + ShopCode);
                            }
                            if (string.IsNullOrEmpty(value) || !value.Contains(ShopId))
                            {
                                DeleteList3.Add(item.No);
                            }
                        }
                        foreach (var item in DeleteList3)
                        {
                            person.RemoveAll(c => c.No == item);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Write("修改报销人失败：" + ex.ToString() + "," + System.Reflection.MethodBase.GetCurrentMethod().Name + ",账号:");
            }

            return Public.JsonSerializeHelper.SerializeToJson(person);
        }

        private List<Person> NewMethod2(string DepartmentId)
        {
            List<Person> offices = new List<Person>();
            //3.0EMPLOYEE_MUTI_DEPARTMENT表找人
            var list3 = DbContext.EMPLOYEE_MUTI_DEPARTMENT.ToList();
            foreach (var item in list3)
            {
                var temp = item.UCSTAR_ID.Trim().Split(',').ToList();
                temp.Remove("");
                string DepidString = "";
                foreach (var item1 in temp)
                {
                    var depart = DbContext.DEPARTMENT.Where(c => c.UCSTAR_ID == item1).FirstOrDefault();
                    var rootdepart = GetRootDepartment(depart);
                    DepidString += rootdepart.ID + ",";
                }
                item.UCSTAR_ID = DepidString;
                if (item.UCSTAR_ID.Contains(DepartmentId))
                {
                    offices.Add(new Person() { No = item.EMPLOYEENO, Name = AjaxGetName(item.EMPLOYEENO) });
                }
            }
            return offices;
        }

        private List<Person> NewMethod(string EmployeeNo, string CompanyCode, string DepartmentId)
        {
            //1.0权限找人
            var list1 = DbContext.UIVALUE.Where(c => c.VALUETYPE == 3 && c.VALUE.Contains(DepartmentId)).Select(x => new Person()
            {
                No = x.EMPLOYEENO,
                Name = x.VALUE
            }).ToList();
            //2.1保存和登录用户处于不同公司的数据
            //List<string> DeleteList = new List<string>();
            //foreach (var item in list1)
            //{
            //    var tempCode = DbContext.EMPLOYEE.Where(c => c.NO == item.No).Select(x => x.DEPARTMENT.COMPANYCODE).FirstOrDefault();
            //    if (CompanyCode != tempCode)
            //    {
            //        DeleteList.Add(item.No);
            //    }
            //}
            ////3.0移除掉不同公司的数据
            //foreach (var item in DeleteList)
            //{
            //    list1.RemoveAll(c => c.No == item);
            //}
            //4.0获取根部门
            WorkFlowController wk = new WorkFlowController();
            List<string> DeleteList2 = new List<string>();
            var boot = wk.GetDepartmenRoot(EmployeeNo, 1);
            foreach (var item in list1)
            {
                var tempBoot = wk.GetDepartmenRoot(item.No, 1);
                if (boot != tempBoot)
                {
                    DeleteList2.Add(item.No);
                }
            }
            //4.1删除不是相同部门的人
            foreach (var item in DeleteList2)
            {
                list1.RemoveAll(c => c.No == item);
            }
            foreach (var item in list1)
            {
                item.Name = AjaxGetName(item.No);
            }
            return list1;
        }

        /// <summary>
        /// 获取所有部门人员
        /// </summary>
        /// <param name="EmployeeNo"></param>
        /// <returns></returns>
        private List<Person> GetDepartmentAllPerson(string EmployeeNo)
        {
            EMPLOYEE employeetemp = DbContext.EMPLOYEE.Where(m => m.NO == EmployeeNo).FirstOrDefault();
            var boot = new WorkFlowController().GetRootDepartment(employeetemp.DEPARTMENT);
            //找根部门的所有人员
            List<Person> list = new List<Person>();
            GetPersonList(list, boot.ID);
            return list;
        }

        private void GetPersonList(List<Person> personList, decimal ID)
        {
            //得到该部门下的人员
            List<EMPLOYEE> employee = DbContext.EMPLOYEE.Where(m => m.DEPID == ID && m.LEAVE == "0" && m.AVAILABLE == "1").ToList();
            if (employee != null && employee.Count > 0)
            {
                foreach (var item in employee)
                {
                    personList.Add(new Person() { Name = item.NAME, No = item.NO });
                }
            }


            List<DEPARTMENT> DepartmentList = DbContext.DEPARTMENT.Where(D => D.PID == ID).ToList();
            if (DepartmentList != null && DepartmentList.Count > 0)
            {
                foreach (var item in DepartmentList)
                {
                    GetPersonList(personList, item.ID);
                }
            }
        }
        /// <summary>
        /// 获取货币类型
        /// </summary>
        /// <returns></returns>
        public string GetMoneyType()
        {
            var model = DbContext.CURRENCY.Select(c => new Person
            {
                Name = c.NAME,
                No = c.CODE
            }).ToList();
            return model.Count == 0 ? "" : Public.JsonSerializeHelper.SerializeToJson(model);
        }


        /// <summary>
        /// 获取所有满足条件的人
        /// </summary>
        /// <returns></returns>
        public string GetPersonlist()
        {
            var PersonList = DbContext.EMPLOYEE.Where(c => c.AVAILABLE == "1" && c.LEAVE == "0").Select(x => new Marisfrolg.Fee.Models.TempData
             {
                 label = x.NAME,
                 value = x.NO

             }).ToList();
            return Public.JsonSerializeHelper.SerializeToJson(PersonList);
        }
    }
}
