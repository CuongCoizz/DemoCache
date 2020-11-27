using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CacheDemo.Controllers
{
    public class ResponeCachingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 1000)]
        public IActionResult GetData()
        {
            _ = VietNamMapCommon.LoadDataForVNMap(out JObject geoData);
            string content = JsonConvert.SerializeObject(new { success = true, data = geoData });
            return Content(content, "application/json");
        }
    }
}