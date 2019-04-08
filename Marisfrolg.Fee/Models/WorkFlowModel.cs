using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Models
{
    public class WorkFlowModel
    {

    }

    #region 数据封装类
    public class Proposer
    {
        //根部门
        public decimal DepartmentRoot { get; set; }
        //姓名
        public string Name { get; set; }
        //工号
        public string No { get; set; }
        /// <summary>
        /// 权限集合
        /// </summary>
        public string PowerList { get; set; }
    }

    /// <summary>
    /// 数据封装类
    /// </summary>
    public class PackageData
    {
        StateConde msg = new StateConde();

        public StateConde code
        {
            get { return msg; }
            set { this.msg = value; }

        }

        Object wodekey = new Object();

        /// <summary>
        /// 数据键
        /// </summary>
        public Object datas
        {
            get { return wodekey; }
            set { this.wodekey = value; }
        }
    }

    /// <summary>
    /// 状态描述类
    /// </summary>
    public class StateConde
    {
        /// <summary>
        /// 状态码 （0-成功 1-异常）
        /// </summary>
        public int errorCode { get; set; }

        /// <summary>
        /// 数据描述
        /// </summary>
        public string message { get; set; }
    }
    #endregion

    /// <summary>
    /// 流程实例对象
    /// </summary>
    public class FlowInstance
    {
        public string Description { get; set; }
        public List<string> PersonName { get; set; }
        public string Remark { get; set; }
        public List<int> KeyWord { get; set; }
        public List<string> StringTime { get; set; }
        public List<string> AuditList { get; set; }
        /// <summary>
        /// 活动ID
        /// </summary>
        public string ActiveID { get; set; }
        /// <summary>
        /// 节点状态
        /// </summary>
        public string NodeState { get; set; }
    }


    /// <summary>
    /// 打包类，用于传输到丁毅的接口容器
    /// </summary>
    public class PackClass
    {
        public string Creator { get; set; }
        public string DepartmentCode { get; set; }
        public string CostCenter { get; set; }

        public List<string> Items { get; set; }

        public int IsHeadOffice { get; set; }

        public string BillsType { get; set; }

        public string CompanyCode { get; set; }

        public int IsUrgent { get; set; }

        public string Department { get; set; }

        public int Funds { get; set; }

        public List<string> Brand { get; set; }
    }
}