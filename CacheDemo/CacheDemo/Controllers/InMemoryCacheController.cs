using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CacheDemo.Controllers
{
    public class InMemoryCacheController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetDataWithInMemoryCache()
        {
            JObject geoData;
            string content;                                                                                                                                                           
            ObjectCache cache = MemoryCache.Default;
            if (cache["VietNamMapJson"] == null)
            {
                ResponseModel response = VietNamMapCommon.LoadDataForVNMap(out geoData);
                if (ResponseType.Success != response.Type)
                {
                    content = JsonConvert.SerializeObject(new { success = false, data = string.Empty, message = response.Message });
                    return Content(content, "application/json");
                }
                CacheItemPolicy cachePolicy = new CacheItemPolicy();
                cache.Set("VietNamMapJson", geoData.ToString(Formatting.None), cachePolicy);
            }
            else
            {
                geoData = JObject.Parse(cache["VietNamMapJson"].ToString());
            }

            content = JsonConvert.SerializeObject(new { success = true, data = geoData });

            return Content(content, "application/json");
        }
    }
}