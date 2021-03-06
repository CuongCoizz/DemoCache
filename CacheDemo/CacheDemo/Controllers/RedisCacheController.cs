﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CacheDemo.Controllers
{
    public class RedisCacheController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public RedisCacheController(IDistributedCache distributedCache)
        {
            this._distributedCache = distributedCache;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetDataWithRedisCache()
        {
            string cacheKey = "VietNamMapJson";
            JObject geoData;
            string content;
            try
            {
                var redisCacheVietNamMap = _distributedCache.Get(cacheKey);
                if (redisCacheVietNamMap == null)
                {
                    ResponseModel response = VietNamMapCommon.LoadDataForVNMap(out geoData);
                    if (ResponseType.Success != response.Type)
                    {
                        content = JsonConvert.SerializeObject(new { success = false, data = string.Empty, message = response.Message });
                        return Content(content, "application/json");
                    }
                    DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
                    {

                    };
                    _distributedCache.Set(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(geoData)));
                }
                else
                {
                    geoData = JObject.Parse(Encoding.UTF8.GetString(redisCacheVietNamMap));
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
            

            content = JsonConvert.SerializeObject(new { success = true, data = geoData });

            return Content(content, "application/json");
        }
    }
}