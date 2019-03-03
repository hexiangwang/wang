using System;
using System.Threading;

namespace APPHost
{
    class Program
    {
        static void Main(string[] args)
        {
            AppFrame app = AppFrame.GetAppFrame();
            app.Start();
            app.OnSelected += App_OnSelected;
            Console.WriteLine("按Q退出");
            for (; ; )
            {
                string str = Console.ReadLine();
                if (str.Contains("u"))
                {
                    try
                    {
                        int index = int.Parse(str.TrimStart('u'));
                        app.SetUp("Web用户","127.0.0.1",502, index);
                    }
                    catch
                    {

                    }
                }
                if (str.Contains("d"))
                {
                    try
                    {
                        int index = int.Parse(str.TrimStart('d'));
                        app.SetDown("Web用户", "127.0.0.1",502, index);
                    }
                    catch
                    {

                    }
                }
                if (str.Contains("s"))
                {
                    app.SelectStatus("Web", "127.0.0.1", 502);
                }

                if (str.ToUpper() == "Q")
                {
                    app.Dispose();
                    break;
                }
            }
        }

        private static void App_OnSelected(object sender, PLC.ModBus.Rtu.Driver.EventEntity.ParkingLotStatusEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
