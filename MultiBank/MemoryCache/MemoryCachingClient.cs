using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.MemoryCache
{
    public class MemoryCachingClient
    {
        public MemoryCachingClient()
        {
        }

        /// <summary>
        /// 获取缓存对象的提供程序
        /// </summary>
        internal IMemoryCachingClient CacheManager
        {
            get { return new WebCacheProvider(); }
        }

        /// <summary>
        /// 根据KEY获取对应的缓存对象
        /// </summary>
        /// <param name="pKey">要获取的缓存的KEY</param>
        /// <returns>对应的缓存对象</returns>
        public object this[string key]
        {
            get { return CacheManager[key]; }
            set { CacheManager[key] = value; }
        }

        /// <summary>
        /// 获取与指定key关联的缓存对象。
        /// </summary>
        /// <param name="key">从缓存返回对象的Key。</param>
        /// <returns>如果缓存中存在并能找到与key对应的对象，则返回该对象，否则返回null。</returns>
        public object GetData(string key)
        {
            return CacheManager.GetData(key);
        }

        /// <summary>
        /// 是否包含KEY
        /// </summary>
        /// <param name="pKey">要测试的KEY</param>
        /// <returns>TRUE包含，FALSE不包含</returns>
        public bool ContainsKey(string key)
        {
            return CacheManager.ContainsKey(key);
        }

        /// <summary>
        /// 添加缓存键及项
        /// </summary>
        /// <param name="pKey">缓存键</param>
        /// <param name="pObj">缓存对象</param>
        public void Add(string key, object value)
        {
            CacheManager.Add(key, value);
        }

        /// <summary>
        /// 添加缓存键及项
        /// </summary>
        /// <param name="pKey">缓存键</param>
        /// <param name="pObj">缓存对象</param>
        /// <param name="timesOut">过期时间分钟数</param>
        public void Add(string key, object value, int expiredMinutes)
        {
            CacheManager.Add(key, value, expiredMinutes);
        }

        /// <summary>
        /// 根据KEY移除相应的缓存
        /// </summary>
        /// <param name="pKey">要移除的KEY</param>
        public void Remove(string key)
        {
            CacheManager.Remove(key);
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Flush()
        {
            CacheManager.Flush();
        }
    }
}