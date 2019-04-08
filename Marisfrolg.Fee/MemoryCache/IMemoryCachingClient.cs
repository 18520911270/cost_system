using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marisfrolg.Fee
{
    /// <summary>
    /// 定义一个缓存提供程序接口。
    /// </summary>
    public interface IMemoryCachingClient
    {
        /// <summary>
        /// 获取缓存对象的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 索引器访问对象值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// 获取key值保存的缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetData(string key);

        /// <summary>
        /// 是否包含该key对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 移除所有缓存
        /// </summary>
        void Flush();

        /// <summary>
        /// 添加缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(string key, object value);

        /// <summary>
        /// 添加缓存对象并设置过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiredMinutes"></param>
        void Add(string key, object value, int expiredMinutes);

        /// <summary>
        /// 移除key值缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

    }
}
