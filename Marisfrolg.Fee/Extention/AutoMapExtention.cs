using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marisfrolg.Fee.Extention
{
    public static class AutoMapExtention
    {

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="F">From</typeparam>
        /// <typeparam name="T">To</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T MapTo<F, T>(this F source)
            where T : class
            where F : class
        {
            if (source == null) { return default(T); }
            Mapper.CreateMap<F, T>();
            return Mapper.Map<T>(source);
        }


        /// <summary>
        /// 集合映射
        /// </summary>
        /// <typeparam name="F">From</typeparam>
        /// <typeparam name="T">To</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> MapTo<F, T>(this IEnumerable<F> source)
            where T : class
            where F : class
        {
            if (source == null) { return new List<T> { }; }
            Type ty = source.GetType();
            Mapper.CreateMap<F, T>();
            return Mapper.Map<List<T>>(source);
        }
    }
}