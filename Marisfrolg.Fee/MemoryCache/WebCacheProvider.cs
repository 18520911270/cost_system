using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Marisfrolg.Fee
{
    /// <summary>
    /// 使用System.Web.Caching.Cache作为缓存提供程序。
    /// </summary>
    public class WebCacheProvider : IMemoryCachingClient
    {
        private System.Web.Caching.Cache cache = HttpRuntime.Cache;
        private object m_SyncObj = new object();
        private List<string> CacheKeys = new List<string>();

        public WebCacheProvider()
        {
        }

        public int Count
        {
            get { return cache.Count; }
        }

        public object this[string key]
        {
            get { return cache[key]; }
            set
            {
                if (CacheKeys.Contains(key))
                {
                    lock (m_SyncObj)
                    {
                        if (CacheKeys.Contains(key))
                        {
                            cache[key] = value;
                        }
                    }
                }
                else
                    this.Add(key, value);
            }
        }

        public object GetData(string key)
        {
            try
            {
                return cache.Get(key);
            }
            catch (Exception) { }
            return null;
        }

        public bool ContainsKey(string key)
        {
            return CacheKeys.Contains(key);
        }

        public void Flush()
        {
            lock (m_SyncObj)
            {
                try
                {
                    foreach (string key in CacheKeys)
                        cache.Remove(key);
                    CacheKeys.Clear();
                }
                catch (Exception) { }
            }
        }

        public void Add(string key, object value)
        {
            lock (m_SyncObj)
            {
                try
                {
                    cache.Add(key, value, null, DateTime.MaxValue, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    if (!CacheKeys.Contains(key))
                    {
                        CacheKeys.Add(key);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Add(string key, object value, int expiredMinutes)
        {
            lock (m_SyncObj)
            {
                try
                {
                    cache.Add(key, value, null, DateTime.Now.AddMinutes(expiredMinutes), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    if (!CacheKeys.Contains(key))
                    {
                        CacheKeys.Add(key);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Remove(string key)
        {
            lock (m_SyncObj)
            {
                try
                {
                    cache.Remove(key);
                    if (CacheKeys.Contains(key))
                        CacheKeys.Remove(key);
                }
                catch (Exception) { }
            }
        }

    }
}