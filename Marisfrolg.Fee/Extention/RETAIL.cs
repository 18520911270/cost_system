using Marisfrolg.Fee.Models;
using Marisfrolg.Public;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee
{
    public class RETAIL
    {
        RfcDestination Conn;

        public RETAIL(RfcDestination pConn)
        {
            Conn = pConn;
        }

        ///// <summary>
        ///// 获取物料状态
        ///// </summary>
        ///// <param name="MatCode">物料编号</param>
        ///// <param name="rList"></param>
        ///// <returns></returns>
        //public List<MaterialStatus> ZRFC_GETMATORDER(string MatCode)
        //{
        //    List<MaterialStatus> rList = new List<MaterialStatus>();
        //    try
        //    {
        //        RfcRepository repo = Conn.Repository;
        //        IRfcFunction rfcfunc = repo.CreateFunction("ZRFC_GETMATORDER");
        //        rfcfunc.SetValue("MATNR", MatCode);
        //        rfcfunc.Invoke(Conn);

        //        IRfcTable RfcTable = rfcfunc.GetTable("MATNRDATA");
        //        for (int i = 0; i < RfcTable.Count(); i++)
        //        {
        //            MaterialStatus model = new MaterialStatus();
        //            //model.MATNR = RfcTable[i].GetValue("MATNR").ToString();
        //            //model.ZCANORDER = RfcTable[i].GetValue("ZCANORDER").ToString();
        //            //model.ZREPROD = RfcTable[i].GetValue("ZREPROD").ToString();
        //            rList.Add(model);
        //        }
        //    }
        //    catch
        //    {
        //        rList = null;
        //    }
        //    return rList;
        //}


        public SapRetrun CREATE_BAPI_ACC_DOCUMENT_POST(SapUpload model)
        {

            try
            {
                RfcRepository repo = Conn.Repository;
                IRfcFunction Fun = repo.CreateFunction("ZBAPI_ACC_DOCUMENT_POST");

                //结构体doc
                IRfcStructure tb = Fun.GetStructure("DOCUMENTHEADER");
                DOCUMENTHEADER doc = model.结构体DOC;
                tb.SetValue("USERNAME", doc.USERNAME);
                tb.SetValue("HEADER_TXT", doc.HEADER_TXT);
                tb.SetValue("COMP_CODE", doc.COMP_CODE);
                tb.SetValue("DOC_DATE", doc.DOC_DATE);
                tb.SetValue("PSTNG_DATE", doc.PSTNG_DATE);
                tb.SetValue("TRANS_DATE", doc.TRANS_DATE);
                tb.SetValue("DOC_TYPE", doc.DOC_TYPE);
                tb.SetValue("BUS_ACT", doc.BUS_ACT);
                tb.SetValue("OBJ_TYPE", doc.OBJ_TYPE);
                tb.SetValue("REF_DOC_NO", doc.REF_DOC_NO);

                //一般性总账科目
                if (model.一般性总账 != null && model.一般性总账.Count > 0)
                {
                    IRfcTable tb1 = Fun.GetTable("ACCOUNTGL");
                    foreach (var item in model.一般性总账)
                    {
                        tb1.Insert();
                        tb1.CurrentRow.SetValue("ITEMNO_ACC", item.ITEMNO_ACC);
                        tb1.CurrentRow.SetValue("GL_ACCOUNT", item.GL_ACCOUNT);
                        tb1.CurrentRow.SetValue("ITEM_TEXT", item.ITEM_TEXT);
                        tb1.CurrentRow.SetValue("ALLOC_NMBR", item.ALLOC_NMBR);
                        tb1.CurrentRow.SetValue("COSTCENTER", item.COSTCENTER);
                        tb1.CurrentRow.SetValue("PROFIT_CTR", item.PROFIT_CTR);
                        tb1.CurrentRow.SetValue("ACCT_TYPE", item.ACCT_TYPE);
                        if (!string.IsNullOrEmpty(item.ASSET_NO))
                        {
                            tb1.CurrentRow.SetValue("ASSET_NO", item.ASSET_NO);
                            tb1.CurrentRow.SetValue("SUB_NUMBER", "0000");
                            tb1.CurrentRow.SetValue("ACCT_TYPE", "A");
                        }
                    }
                }

                //科目D
                if (model.科目D != null && model.科目D.Count > 0)
                {
                    IRfcTable tb1 = Fun.GetTable("ACCOUNTRECEIVABLE");
                    foreach (var item in model.科目D)
                    {
                        tb1.Insert();
                        tb1.CurrentRow.SetValue("ITEMNO_ACC", item.ITEMNO_ACC);
                        tb1.CurrentRow.SetValue("CUSTOMER", item.CUSTOMER);
                        tb1.CurrentRow.SetValue("ITEM_TEXT", item.ITEM_TEXT);
                        tb1.CurrentRow.SetValue("ALLOC_NMBR", item.ALLOC_NMBR);
                        tb1.CurrentRow.SetValue("PROFIT_CTR", item.PROFIT_CTR);
                    }
                }

                //科目K
                if (model.科目K != null && model.科目K.Count > 0)
                {
                    IRfcTable tb1 = Fun.GetTable("ACCOUNTPAYABLE");
                    foreach (var item in model.科目K)
                    {
                        tb1.Insert();
                        tb1.CurrentRow.SetValue("ITEMNO_ACC", item.ITEMNO_ACC);
                        tb1.CurrentRow.SetValue("VENDOR_NO", item.VENDOR_NO);
                        tb1.CurrentRow.SetValue("ITEM_TEXT", item.ITEM_TEXT);
                        tb1.CurrentRow.SetValue("ALLOC_NMBR", item.ALLOC_NMBR);
                        if (!string.IsNullOrEmpty(item.PO_NUMBER))
                        {
                            tb1.CurrentRow.SetValue("SP_GL_IND", item.UMSKZ);
                            tb1.CurrentRow.SetValue("GL_ACCOUNT", item.GL_ACCOUNT);
                            tb1.CurrentRow.SetValue("PROFIT_CTR", item.PROFIT_CTR);
                        }
                        else if (!string.IsNullOrEmpty(item.PROFIT_CTR))
                        {
                            tb1.CurrentRow.SetValue("GL_ACCOUNT", item.GL_ACCOUNT);
                            tb1.CurrentRow.SetValue("PROFIT_CTR", item.PROFIT_CTR);
                        }
                    }
                }

                if (model.币种 != null && model.币种.Count > 0)
                {
                    IRfcTable tb2 = Fun.GetTable("CURRENCYAMOUNT");
                    foreach (var item in model.币种)
                    {
                        tb2.Insert();
                        tb2.CurrentRow.SetValue("ITEMNO_ACC", item.ITEMNO_ACC);
                        tb2.CurrentRow.SetValue("CURRENCY", item.CURRENCY);
                        tb2.CurrentRow.SetValue("AMT_DOCCUR", item.AMT_DOCCUR);
                        tb2.CurrentRow.SetValue("EXCH_RATE", item.EXCH_RATE);
                    }
                }


                if (model.行项目 != null && model.行项目.Count > 0)
                {
                    IRfcTable tb3 = Fun.GetTable("EXTENSION2");
                    foreach (var item in model.行项目)
                    {
                        tb3.Insert();
                        tb3.CurrentRow.SetValue("STRUCTURE", item.STRUCTURE);
                        tb3.CurrentRow.SetValue("VALUEPART1", item.VALUEPART1);
                        tb3.CurrentRow.SetValue("VALUEPART2", item.VALUEPART2);
                        tb3.CurrentRow.SetValue("VALUEPART3", item.VALUEPART3);
                        tb3.CurrentRow.SetValue("VALUEPART4", item.VALUEPART4);
                    }
                }

                Fun.Invoke(Conn);

                SapRetrun result = new SapRetrun();

                var voucher = Fun.GetValue("OBJ_KEY").ToString();
                if (!string.IsNullOrEmpty(voucher) && voucher != "$")
                {
                    result.TYPE = "S";
                    result.MESSAGE = "凭证上传成功";
                    result.VOUCHER = voucher;

                }
                else
                {
                    var Return = Fun.GetTable("RETURN");
                    if (Return.RowCount >= 2)
                    {
                        result.TYPE = Return[1].GetValue("TYPE").ToString();
                        result.MESSAGE = Return[1].GetValue("MESSAGE").ToString();
                        result.VOUCHER = Return[1].GetValue("MESSAGE_V2").ToString();
                    }
                    else
                    {
                        result.TYPE = Return[0].GetValue("TYPE").ToString();
                        result.MESSAGE = Return[0].GetValue("MESSAGE").ToString();
                        result.VOUCHER = Return[0].GetValue("MESSAGE_V2").ToString();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}