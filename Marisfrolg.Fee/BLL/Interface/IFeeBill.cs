﻿using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.BLL
{
    /// <summary>
    /// 费用单据接口
    /// </summary>
    public interface IFeeBill
    {

        void CreateFeeBill(FeeBillModel pFeeBill);

        void GetFeeBill(string pID);


    }

}