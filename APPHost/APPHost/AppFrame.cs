using Newtonsoft.Json;
using PLC.ModBus.Rtu.Driver;
using PLC.ModBus.Rtu.Driver.Descriptor;
using PLC.ModBus.Rtu.Driver.EventEntity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace APPHost
{
    public  class AppFrame:IDisposable
    {
        private static AppFrame app;

        private static object lockObj = new object();

        private List<PLCModBusRtuDriver> drivers;

        private object obj;

        private bool isExit = false;

        private Queue<ParkingLotStatusEventArgs> queue;

        public event EventHandler<ParkingLotStatusEventArgs> OnSelected;

        /// <summary>
        /// 查询事件触发的方法
        /// </summary>
        /// <param name="e"></param>
        protected void SelectedInfoEventMethod(ParkingLotStatusEventArgs e)
        {
            if (OnSelected != null)
            {
                OnSelected(this, e);
            }
        }

        private Thread selectThread;

        private AppFrame()
        {
            obj = new object();
            queue = new Queue<ParkingLotStatusEventArgs>();
            dynamic type = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            string filePath = $"{currentDirectory}\\Config\\Config.xml";
            drivers = new List<PLCModBusRtuDriver>();
            if (File.Exists(filePath))
            {
                //读取整个配置
                XmlDocument document = new XmlDocument();
                document.Load(filePath);
                XmlNodeList nodeList = document.SelectNodes("/PLCDrivers/PLCDriver");
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i].NodeType == XmlNodeType.Element)
                    {
                        //解析驱动
                        XmlElement element = nodeList[i] as XmlElement;
                        PLCModBusRtuDriver driver = GetDriverByXmlNode(element);
                        if (driver != null)
                        {
                            drivers.Add(driver);
                        }
                    }
                }
            }
            selectThread = new Thread(SelectEventMethod);
            selectThread.IsBackground = true;
            selectThread.Start();

            for (int i = 0; i < drivers.Count; i++)
            {
                drivers[i].OnSelected += AppFrame_OnSelected;
            }
        }

        private void AppFrame_OnSelected(object sender, PLC.ModBus.Rtu.Driver.EventEntity.ParkingLotStatusEventArgs e)
        {
            lock (obj)
            {
                queue.Enqueue(e);
                Monitor.Pulse(obj);
            }
        }

        void SelectEventMethod()
        {
            for(; ; )
            {
                if (isExit)
                {
                    break;
                }
                else
                {
                    if(queue.Count == 0)
                    {
                        lock (obj)
                        {
                            Monitor.Wait(obj);
                        }
                    }
                    else
                    {
                        ParkingLotStatusEventArgs e = null;
                        lock (obj)
                        {
                            e = queue.Dequeue();
                        }
                        SelectedInfoEventMethod(e);
                    }
                }
            }
        }

        private PLCModBusRtuDriver GetDriverByXmlNode(XmlElement element)
        {
            byte salve = byte.Parse(element.Attributes["salve"].Value, NumberStyles.HexNumber);
            string ipAddress = element.Attributes["ipAddress"].Value;
            int port = 502;
            int.TryParse(element.Attributes["port"]?.Value, out port);
            string sendInterval = element.Attributes["sendInterval"].Value;
            int interval = 10;
            int.TryParse(sendInterval, out interval);
            //关于寄存器 这里暂时不做配置
            PLCDescriptor descriptor = new PLCDescriptor(salve, ipAddress, port, interval);
            PLCModBusRtuDriver driver = new PLCModBusRtuDriver(descriptor);
            return driver;
        }

        public static AppFrame GetAppFrame()
        {
            lock (lockObj)
            {
                if (app == null)
                {
                    app = new AppFrame();
                }
            }
            return app;
        }

        public void Start()
        {
            for(int i = 0; i < drivers.Count; i++)
            {
                drivers[i].Start();
            }
        }


        #region


        /// <summary>
        /// 设置上升
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string SetUp(string userName, string ip,int port,int index)
        {
            PLCModBusRtuDriver driver = drivers.Find(p => p.Descriptor.IpAddress == ip && p.Descriptor.Port == port);
            if (driver != null)
            {
                if (driver.ConnStatus == PLC.ModBus.Rtu.Driver.TcpConn.PortStatus.Connected)
                {
                    driver.ReceCommandFromWeb(userName, index, true);
                }
                return driver.ConnStatus.ToString();
            }
            //return "not find";
            return "";
        }

        /// <summary>
        /// 设置下降
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string SetDown(string userName, string ip,int port, int index)
        {
            PLCModBusRtuDriver driver = drivers.Find(p => p.Descriptor.IpAddress == ip && p.Descriptor.Port == port);
            if (driver != null)
            {
                if (driver.ConnStatus == PLC.ModBus.Rtu.Driver.TcpConn.PortStatus.Connected)
                {
                    driver.ReceCommandFromWeb(userName, index, false);
                }
                return driver.ConnStatus.ToString();
            }
            //return "not find";
            return "";
        }

        public string SelectStatus(string userName, string ip,int port)
        {
            PLCModBusRtuDriver driver = drivers.Find(p => p.Descriptor.IpAddress == ip && p.Descriptor.Port == port);
            if (driver != null)
            {
                if (driver.ConnStatus == PLC.ModBus.Rtu.Driver.TcpConn.PortStatus.Connected)
                {
                    driver.ReceSelectFromWeb(userName);
                }
                return driver.ConnStatus.ToString();
            }
            //return "not find";
            return "";
        }

        public void Dispose()
        {
            isExit = true;
            if (selectThread.IsAlive)
            {
                selectThread.Abort();
            }
            for(int i = 0; i < drivers.Count; i++)
            {
                drivers[i].Stop();
            }
        }
        #endregion
    }
}
