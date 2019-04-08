using MultiBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultiBank.Controllers
{
    public class BaseController : Controller
    {
        protected ContentResult JsonContent(object obj)
        {
            string json = JsonHelper.Serialize(obj);
            return base.Content(json);
        }

        protected ContentResult SuccessData(object data = null)
        {
            Result<object> result = Result.CreateResult<object>(ResultStatus.OK, data);
            return this.JsonContent(result);
        }
        protected ContentResult SuccessMsg(string msg = null)
        {
            Result result = new Result(ResultStatus.OK, msg);
            return this.JsonContent(result);
        }
        protected ContentResult AddSuccessData(object data, string msg = "添加成功")
        {
            Result<object> result = Result.CreateResult<object>(ResultStatus.OK, data);
            result.Msg = msg;
            return this.JsonContent(result);
        }
        protected ContentResult AddSuccessMsg(string msg = "添加成功")
        {
            return this.SuccessMsg(msg);
        }
        protected ContentResult UpdateSuccessMsg(string msg = "更新成功")
        {
            return this.SuccessMsg(msg);
        }
        protected ContentResult DeleteSuccessMsg(string msg = "删除成功")
        {
            return this.SuccessMsg(msg);
        }
        protected ContentResult FailedMsg(string msg = null)
        {
            Result retResult = new Result(ResultStatus.Failed, msg);
            return this.JsonContent(retResult);
        }

        protected void KeepCache(string name, object value)
        {
            MultiBank.MemoryCache.MemoryCachingClient M = new MultiBank.MemoryCache.MemoryCachingClient();
            M.Remove(name);
            M.Add(name, value);
        }

        protected T GetCache<T>(string name) where T : class
        {
            MultiBank.MemoryCache.MemoryCachingClient M = new MultiBank.MemoryCache.MemoryCachingClient();
            var model = M.GetData(name) as T;
            return model;
        }
    }
}
