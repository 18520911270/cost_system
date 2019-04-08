using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiBank.Extention
{
    public static class IdHelper
    {
        public static string CreateGuid()
        {
            Guid id = Guid.NewGuid();
            return id.ToString("N").ToLower();
        }

        public static long CreateSnowflakeId()
        {
            return Snowflake.Instance.GetId();
        }
    }
}