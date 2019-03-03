using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppHostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetController : ControllerBase
    {
        // POST: api/Set/up？ip=127.0.0.1&port=502&index=1
        [HttpPost("{operate}")]
        public bool Post(string operate,string ip,int port,int index)
        {
            try
            {
                if (index > 10 || index < 0) return false;

                AppFrame app = AppFrame.GetAppFrame();
                switch (operate.ToLower())
                {
                    case "up":
                        return !string.IsNullOrEmpty(app.SetUp("Web用户", ip: ip, port: port, index: index));
                    case "down":                 
                        return !string.IsNullOrEmpty(app.SetDown("Web用户", ip: ip, port: port, index: index)); 
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}