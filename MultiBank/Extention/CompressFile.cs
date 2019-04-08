using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace MultiBank.Extention
{
    public class CompressFile
    {
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="sFile">原文件地址</param>
        /// <param name="outPath">新地址</param>
        /// <param name="outname">新名称</param>
        /// <param name="flag">压缩比例，默认50</param>
        /// <returns></returns>
        public static bool GetPicThumbnail(string sFile, string outPath, string outname, int flag = 50)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;

            //以下代码为保存图片时，设置压缩质量  
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100  
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    iSource.Save(outPath, jpegICIinfo, ep);//dFile是压缩后的新路径  
                }
                else
                {
                    iSource.Save(outPath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //释放句柄
                iSource.Dispose();

                //删掉原图
                System.IO.File.Delete(sFile);

                System.IO.File.Move(outPath, outname);
            }
        }
    }
}