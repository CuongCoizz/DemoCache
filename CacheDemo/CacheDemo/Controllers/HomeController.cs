using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CacheDemo.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.Caching;

namespace CacheDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult NoCache()
        {
            ViewData["Message"] = "Your contact page." + DateTime.Now.ToString();
            return View();
        }
        
        public IActionResult GetData()
        {
            JObject geoData;
            string content = "";
            ResponseModel response = VietNamMapCommon.LoadDataForVNMap(out geoData);

            if (ResponseType.Success != response.Type)
            {
                content = JsonConvert.SerializeObject(new { success = false, data = string.Empty, message = response.Message });
                return Content(content, "application/json");
            }
            content = JsonConvert.SerializeObject(new { success = true, data = geoData });
            return Content(content, "application/json");
        }

        

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
