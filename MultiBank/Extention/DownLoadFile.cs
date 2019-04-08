using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace MultiBank.Extention
{
    public class DownLoadFile
    {
        public static string ExportExcel(DataTable dt, string TableName)
        {
            Workbook workbook = new Workbook(); //工作簿
            Worksheet sheet = workbook.Worksheets[0]; //工作表
            Cells cells = sheet.Cells;//单元格

            int Colnum = dt.Columns.Count;//表格列数
            int Rownum = dt.Rows.Count;//表格行数

            //生成行2 列名行
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(dt.Columns[i].ColumnName);
                cells.SetRowHeight(1, 25);
            }

            //生成数据行
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(dt.Rows[i][k].ToString());
                }
                cells.SetRowHeight(1 + i, 24);
            }

            string NewlocalPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload", "ExcelDownLoad");
            if (!System.IO.Directory.Exists(NewlocalPath))
            {
                System.IO.Directory.CreateDirectory(NewlocalPath);
            }
            string filePathName = TableName + '-' + DateTime.Now.ToString("yyMMddhhmmss") + ".xls";
            workbook.Save(Path.Combine(NewlocalPath, filePathName));
            return filePathName;
        }
    }
}