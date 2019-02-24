using PLC.ModBus.Rtu.Driver.Common;
using PLC.ModBus.Rtu.Driver.Descriptor;
using PLC.ModBus.Rtu.Driver.EventEntity;
using PLC.ModBus.Rtu.Driver.TcpConn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PLC.ModBus.Rtu.Driver
{
    /// <summary>
    /// PLC的Modbus驱动
    /// </summary>
    public class PLCModBusRtuDriver
    {
        /// <summary>
        /// 最后发送指令的时间
        /// </summary>
        private DateTime sendCommandDateTime;

        /// <summary>
        /// 1~8地桩下降
        /// </summary>
        private ushort downRegist1 = 42;

        /// <summary>
        /// 9~10 地桩下降
        /// </summary>
        private ushort downRegist2 = 43;

        /// <summary>
        /// 读线圈
        /// </summary>
        private ushort readRegist = 45;

        /// <summary>
        /// 1~8 地桩上升
        /// </summary>
        private ushort upRegist1 = 47;

        /// <summary>
        /// 9~10地桩上升
        /// </summary>
        private ushort upRegist2 = 48;


        /// <summary>
        /// 连接端口
        /// </summary>
        private TcpClientPort tcpPort;

        /// <summary>
        /// 通讯信息
        /// </summary>
        private PLCDescriptor descriptor;

        public PLCDescriptor Descriptor
        {
            get
            {
                return descriptor;
            }
        }

        private PortStatus connStatus = PortStatus.Closed;

        public PortStatus ConnStatus
        {
            get
            {
                return connStatus;
            }
        }


        private System.Timers.Timer heartTimer;


        /// <summary>
        /// 接收缓存区
        /// </summary>
        private List<byte> receBytesCache;

        /// <summary>
        /// 接收的信号
        /// </summary>
        private ManualResetEvent response = new ManualResetEvent(false);

        /// <summary>
        /// 发送命令后，返回的数据
        /// </summary>
        protected byte[] cmdResponseBytes;

        /// <summary>
        /// 解析的互斥锁
        /// </summary>
        private object parseLock;

        /// <summary>
        /// 发送命令的互斥锁
        /// </summary>
        private object sendLock;

        /// <summary>
        /// 包的最大长度
        /// </summary>
        public static int MaxPackageLength = 1024;

        /// <summary>
        /// 允许的发送缓冲区数据包的个数
        /// </summary>
        public static int AllowSendCacheCount = 10;

        /// <summary>
        /// 解析线程
        /// </summary>
        Thread parseTd;

        /// <summary>
        /// 发送线程
        /// </summary>
        Thread sendTd;

        /// <summary>
        /// 退出标志位
        /// </summary>
        private bool isExit = false;

        /// <summary>
        /// 立即写的队列
        /// </summary>
        protected Queue<RtuRegistPackage> fastQueue;

        /// <summary>
        /// 可以适当延迟写的队列
        /// </summary>
        protected Queue<RtuRegistPackage> sendQueue;

        /// <summary>
        /// 当前发送的modbus指令
        /// </summary>
        private RtuRegistPackage modBusRtuMessage;

        /// <summary>
        /// 查询车位返回的结果
        /// </summary>
        public event EventHandler<ParkingLotStatusEventArgs> OnSelected;

        /// <summary>
        /// 查询事件触发的方法
        /// </summary>
        /// <param name="e"></param>
        protected void SelectedInfoEventMethod(ParkingLotStatusEventArgs e)
        {
            if(OnSelected != null)
            {
                OnSelected(this, e);
            }
        }

        public PLCModBusRtuDriver(PLCDescriptor descriptor)
        {
            sendCommandDateTime = DateTime.Now;

            this.descriptor = descriptor;
            parseLock = new object();
            sendLock = new object();

            receBytesCache = new List<byte>();

            fastQueue = new Queue<RtuRegistPackage>();
            sendQueue = new Queue<RtuRegistPackage>();

            parseTd = new Thread(new ThreadStart(ParseModBusPackage));
            parseTd.IsBackground = true;
            parseTd.Start();

            sendTd = new Thread(new ThreadStart(SendModBusPackage));
            sendTd.IsBackground = true;
            sendTd.Start();

            tcpPort = new TcpClientPort(descriptor.IpAddress, descriptor.Port, descriptor.AutoReConnInterval);
            tcpPort.OnConnNotify += TcpPort_OnConnNotify;
            tcpPort.OnExceptionHappened += TcpPort_OnExceptionHappened;
            tcpPort.OnPortDataReceived += TcpPort_OnPortDataReceived;

            heartTimer = new System.Timers.Timer();
            heartTimer.Interval = descriptor.SendInterval * 1000;
            heartTimer.Elapsed += SendTimer_Elapsed;
            heartTimer.Enabled = true;
        }

        /// <summary>
        /// 解析modbus数据包
        /// 全部验证
        /// </summary>
        void ParseModBusPackage()
        {
            while (!isExit)
            {
                lock (parseLock)
                {
                    if (receBytesCache.Count < 5)
                    {
                        Monitor.Wait(parseLock);
                    }
                    else
                    {
                        if (modBusRtuMessage == null)
                        {
                            receBytesCache.Clear();
                        }
                        else
                        {
                            if (receBytesCache[0] == modBusRtuMessage.Address)
                            {
                                //这里暂时不考虑程序报错
                                if (receBytesCache[1] == modBusRtuMessage.FunctionCode )
                                {
                                    //读 
                                    if (modBusRtuMessage.FunctionCode != 0x06)
                                    {
                                        //整个数据包的长度
                                        int length = receBytesCache[2] + 5;
                                        if (receBytesCache.Count >= length)
                                        {
                                            //验证CRC16 
                                            ushort crcValue = CRC16.CRC16Code(receBytesCache.ToArray(), 0, length - 2);
                                            if ((crcValue >> 8) == receBytesCache[length - 2] && (byte)crcValue == receBytesCache[length - 1])
                                            {
                                                cmdResponseBytes = receBytesCache.GetRange(0, length).ToArray();
                                                receBytesCache.RemoveRange(0, length);
                                                response.Set();
                                            }
                                            else
                                            {
                                                receBytesCache.RemoveAt(0);
                                            }
                                        }
                                        else
                                        {
                                            Monitor.Wait(parseLock);
                                        }
                                    }
                                    else
                                    {
                                        //写的话 数据是一模一样的
                                        if(modBusRtuMessage.PackageBytes.Length <= receBytesCache.Count)
                                        {
                                            bool isEquals = true;
                                            for(int i=0;i< modBusRtuMessage.PackageBytes.Length; i++)
                                            {
                                                if(modBusRtuMessage.PackageBytes[i] != receBytesCache[i])
                                                {
                                                    isEquals = false;
                                                    receBytesCache.RemoveAt(0);
                                                    break;
                                                }
                                            }
                                            if (isEquals)
                                            {
                                                cmdResponseBytes = receBytesCache.GetRange(0, modBusRtuMessage.PackageBytes.Length).ToArray();
                                                receBytesCache.RemoveRange(0, modBusRtuMessage.PackageBytes.Length);
                                                response.Set();
                                            }
                                        }
                                        else
                                        {
                                            Monitor.Wait(parseLock);
                                        }
                                    }
                                }
                                else
                                {
                                    if (receBytesCache[1] == modBusRtuMessage.FunctionCode + 0x80)
                                    {
                                        int length = 5;
                                        ushort crcValue = CRC16.CRC16Code(receBytesCache.ToArray(), 0, length - 2);
                                        if ((crcValue >> 8) == receBytesCache[length - 2] && (byte)crcValue == receBytesCache[length - 1])
                                        {
                                            Console.WriteLine("modbus查询返回异常");
                                            cmdResponseBytes = receBytesCache.GetRange(0, length).ToArray();
                                            receBytesCache.RemoveRange(0, length);
                                            response.Set();
                                        }
                                        else
                                        {
                                            receBytesCache.RemoveAt(0);
                                        }
                                    }
                                    else
                                    {
                                        receBytesCache.RemoveAt(0);
                                    }
                                }

                            }
                            else
                            {
                                receBytesCache.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        void SendModBusPackage()
        {
            while (!isExit)
            {
                if (fastQueue.Count + sendQueue.Count == 0)
                {
                    lock (sendLock)
                    {
                        Monitor.Wait(sendLock);
                    }
                }
                else
                {
                    RtuRegistPackage sendPackage = null;
                    int waitTime = 100;
                    lock (sendLock)
                    {
                        if (fastQueue.Count > 0)
                        {
                            sendPackage = fastQueue.Dequeue();
                        }
                        else
                        {
                            if (sendQueue.Count > 0)
                            {
                                sendPackage = sendQueue.Dequeue();
                            }
                        }
                    }



                    if (sendPackage != null)
                    {
                        sendPackage.SendTime = DateTime.Now;
                        Console.WriteLine(sendPackage.ToString());
                        //不是查询指令
                        if(sendPackage.FunctionCode != 0x03)
                        {
                            //不是还原指令
                            if(sendPackage.LengthOrValue != 0)
                            {
                                waitTime = 2000;
                            }
                        }
                        RtuRecePackage recePackage = Write(sendPackage);

                        if (recePackage != null)
                        {
                            if (sendPackage.FunctionCode == 0x03)
                            {
                                UpdataStatus(sendPackage.CreateTime, DateTime.Now, true, recePackage);
                            }
                        }
                        Console.WriteLine("发送指令成功");
                    }
                    else
                    {
                        if (sendPackage.FunctionCode == 0x03)
                        {
                            UpdataStatus(sendPackage.CreateTime, DateTime.Now, false, null);
                        }
                        Console.WriteLine("发送指令，返回超时！");
                    }
                    Thread.Sleep(waitTime);
                }
                
            }
        }
        

        /// <summary>
        /// 半双工 发送 同时返回数据包
        /// </summary>
        /// <param name="sendPackage"></param>
        /// <returns></returns>
        private RtuRecePackage Write(RtuRegistPackage sendPackage)
        {
            cmdResponseBytes = null;
            modBusRtuMessage = sendPackage; 
            if (tcpPort.ConnStatus == PortStatus.Connected)
            {
                response.Reset();
                lock (parseLock)
                {
                    receBytesCache.Clear();
                }
                tcpPort.Write(sendPackage.PackageBytes);
                if (response.WaitOne(1000))
                {
                    return new RtuRecePackage(sendPackage, cmdResponseBytes);
                }
            }
            return null;
        }

        private void SendTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (tcpPort != null && tcpPort.ConnStatus == PortStatus.Connected)
            {
                //发送了以后 如果没出队列，将不再发送
                if (sendQueue.Count==0)
                {
                    lock (sendLock)
                    {
                        RtuRegistPackage rtuPackage = new RtuRegistPackage("System", "查询车位状态",descriptor.Salve, 0x03, readRegist, 1);
                        sendQueue.Enqueue(rtuPackage);
                        Monitor.Pulse(sendLock);
                    }
                }
            }
        }

        /// <summary>
        /// 从web端 接受命令
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isUp"></param>
        public void ReceCommandFromWeb(string userName, int index,bool isUp)
        {
            RtuRegistPackage sendPackage = null;
            RtuRegistPackage tempPackage = null;
            if (isUp)
            {
                if(index <= 8)
                {
                    sendPackage = new RtuRegistPackage(userName,$"{index}#车位 上升", descriptor.Salve, 0x06, upRegist1, (ushort)(1 << index-1));
                    tempPackage = new RtuRegistPackage("System", $"{index}#车位 上升状态置0", descriptor.Salve, 0x06, upRegist1, 0);
                }
                else
                {
                    sendPackage = new RtuRegistPackage(userName, $"{index}#车位 上升", descriptor.Salve, 0x06, upRegist2, (ushort)(1 << index-1));
                    tempPackage = new RtuRegistPackage("System", $"{index}#车位 上升状态置0", descriptor.Salve, 0x06, upRegist2, 0);
                }
            }
            else
            {
                if (index <= 8)
                {
                    sendPackage = new RtuRegistPackage(userName, $"{index}#车位 下降", descriptor.Salve, 0x06, downRegist1, (ushort)(1 << index-1));
                    tempPackage = new RtuRegistPackage("System", $"{index}#车位 下降状态置0", descriptor.Salve, 0x06, downRegist1, 0);
                }
                else
                {
                    sendPackage = new RtuRegistPackage(userName, $"{index}#车位 下降", descriptor.Salve, 0x06, downRegist2, (ushort)(1 << index-1 ));
                    tempPackage = new RtuRegistPackage("System", $"{index}#车位 下降状态置0", descriptor.Salve, 0x06, downRegist2, 0);
                }
            }
            lock (sendLock)
            {
                fastQueue.Enqueue(sendPackage);
                fastQueue.Enqueue(tempPackage);
                Monitor.Pulse(sendLock);
            }
        }

        /// <summary>
        /// 实时查询
        /// </summary>
        public void ReceSelectFromWeb(string userName)
        {
            lock (sendLock)
            {
                RtuRegistPackage rtuPackage = new RtuRegistPackage(userName,"查询车位信息", descriptor.Salve, 0x03, readRegist, 1);
                sendQueue.Enqueue(rtuPackage);
                Monitor.Pulse(sendLock);
            }
        }

        private void TcpPort_OnPortDataReceived(object sender, EventEntity.ReceBytesEventArge e)
        {
            ShowHexBytes("接收字节:",e.ReceBytes);
            lock (parseLock)
            {
                receBytesCache.AddRange(e.ReceBytes);
                Monitor.Pulse(parseLock);
            }
        }

        public static void ShowHexBytes(string heard, byte[] bytes)
        {
            if(bytes != null && bytes.Length > 0)
            {
                string str = heard;
                for(int i = 0; i < bytes.Length; i++)
                {
                    str += string.Format(@"{0} ",bytes[i].ToString("X2"));
                }
                Console.WriteLine(str);
            }
        }

        private void TcpPort_OnExceptionHappened(object sender, EventEntity.ExceptionEventArgs e)
        {
            
        }

        private void TcpPort_OnConnNotify(object sender, EventEntity.ConnStatusChangedEventArgs e)
        {
            Console.WriteLine("连接状态改变:"+e.Status.ToString());
            connStatus = e.Status;
        }

        public void Start()
        {
            tcpPort.Run();
        }

        public void Stop()
        {
            heartTimer.Enabled = false;
            isExit = true;
            if (sendTd.IsAlive)
            {
                sendTd.Abort();
            }
            if (tcpPort != null)
            {
                tcpPort.Stop();
            }
            tcpPort = null;
        }

        public void UpdataStatus(DateTime sendTime,DateTime receTime,bool isSuccess, RtuRecePackage recePackage)
        {
            bool[] status = new bool[10];
            if (isSuccess)
            {
                int result = (recePackage.ReceBytes[3] << 8) + recePackage.ReceBytes[4];
                for (int i = 0; i < 10; i++)
                {
                    int temp = ((1 << i) & result);
                    if (temp != 0)
                    {
                        status[i] = true;
                    }
                    else
                    {
                        status[i] = false;
                    }
                }
            }
            ParkingLotStatusEventArgs info = new ParkingLotStatusEventArgs(isSuccess, descriptor.IpAddress, descriptor.Port, status, sendTime, receTime);
            SelectedInfoEventMethod(info);

        }
    }
}
