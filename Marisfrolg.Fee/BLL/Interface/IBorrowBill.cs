using Marisfrolg.Fee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.BLL
{
    public interface IBorrowBill
    {
        void CreateBorrowBill(BorrowBillModel pBorrowBill);

        void GetBorrowBill(string pID);
    }
}