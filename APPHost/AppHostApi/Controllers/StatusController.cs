using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppHostApi.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public void Get()
        {
            var context = this.HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {

                ISimpleWebSoket webSoket = new MyWebSoket(context.WebSockets.AcceptWebSocketAsync().Result);

                webSoket.OnMessage += (o, e) =>
                {
                    Console.WriteLine(e.Data);
                };

                AppFrame app = AppFrame.GetAppFrame();
                EventHandler<PLC.ModBus.Rtu.Driver.EventEntity.ParkingLotStatusEventArgs> OnSelected = (o, e) => {

                    webSoket.SendAsync(e.ToString()).Wait();
                };

                app.OnSelected += OnSelected;

                webSoket.OnMessage += (o, e) => { webSoket.SendAsync(e.Data); };

                webSoket.KeepAlive();

                app.OnSelected -= OnSelected;
            }
        }

    }
}
