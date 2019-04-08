using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.BLL
{
    public interface IRefundBill
    {
        void CreateRefundBill(RefundBillModel pFeeBill);

        void GetRefundBill(string pID);
    }
}   