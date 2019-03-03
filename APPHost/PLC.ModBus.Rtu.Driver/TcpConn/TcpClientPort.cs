using PLC.ModBus.Rtu.Driver.EventEntity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace PLC.ModBus.Rtu.Driver.TcpConn
{
    public class TcpClientPort
    {
        private string ipAddress;

        private int port;

        private bool isExit = false;

        private TcpClient client;

        private NetworkStream ns;

        private PortStatus portStatus;

        public PortStatus ConnStatus
        {
            get
            {
                return portStatus;
            }
        }

        protected System.Timers.Timer autoReConnTime;

        private int autoReConnectionInterval;

        /// <summary>
        /// 接收数据事件
        /// </summary>
        public event EventHandler<ReceBytesEventArge> OnPortDataReceived;

        protected void PortDataReceived(ReceBytesEventArge e)
        {
            if(OnPortDataReceived != null)
            {
                OnPortDataReceived(this, e);
            }
        }

        /// <summary>
        /// 在网络中，如果突然中断，也是数据包的形式 只是接受的长度是0 主要是TCP
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnExceptionHappened;

        protected void ExceptionHappened(ExceptionEventArgs e)
        {
            if(OnExceptionHappened != null)
            {
                OnExceptionHappened(this, e);
            }
        }

        /// <summary>
        /// 连接状态发生了更改(主要是断开和连接上 发送通知)
        /// </summary>
        public event EventHandler<ConnStatusChangedEventArgs> OnConnNotify;

        protected void ConnNotify(ConnStatusChangedEventArgs e)
        {
            if(OnConnNotify != null)
            {
                OnConnNotify(this, e);
            }
        }

        public TcpClientPort(string ipAddress,int port,int autoReConnectionInterval = 5)
        {
            portStatus = PortStatus.Closed;
            this.ipAddress = ipAddress;
            this.port = port;
            this.autoReConnectionInterval = autoReConnectionInterval;
        }

        public  void Run()
        {
            TcpConn(ipAddress, port);
            if (autoReConnectionInterval != 0 && autoReConnectionInterval > 0)
            {
                if (autoReConnTime == null)
                {
                    autoReConnTime = new System.Timers.Timer();
                    autoReConnTime.Interval = autoReConnectionInterval * 1000;
                    autoReConnTime.Elapsed += AutoReConnTime_Elapsed;
                    autoReConnTime.Enabled = true;
                }
            }
            else
            {
                if (autoReConnTime != null)
                {
                    autoReConnTime.Enabled = false;
                }
            }
        }

        private void AutoReConnTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (portStatus == PortStatus.Connecting) return;
            autoReConnTime.Enabled = false;
            //根据状态 然后根据socket状态进行双重判断
            if (portStatus == PortStatus.Closed || portStatus == PortStatus.Error)
            {
                TcpConn(ipAddress, port);
            }
            else
            {
                if (!client.Client.Connected)
                {
                    TcpConn(ipAddress, port);
                }
            }
            autoReConnTime.Enabled = true;
        }

        private void TcpConn(string ipAddress, int port)
        {
            try
            {
                portStatus = PortStatus.Connecting;
                client = new TcpClient(AddressFamily.InterNetwork);
                AsyncCallback requestCallBack = new AsyncCallback(RequestCallBack);
                client.BeginConnect(IPAddress.Parse(ipAddress), port, requestCallBack, client);
            }
            catch (Exception ex)
            {
                portStatus = PortStatus.Error;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
            }
        }

        private void RequestCallBack(IAsyncResult iar)
        {
            try
            {
                portStatus = PortStatus.Connecting;
                client = (TcpClient)iar.AsyncState;
                client.EndConnect(iar);
                portStatus = PortStatus.Connected;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, true, portStatus));
                ns = client.GetStream();
                DataRead dataRead = new DataRead(ns, client.ReceiveBufferSize);
                ns.BeginRead(dataRead.msg, 0, dataRead.msg.Length, ReadCallBack, dataRead);
            }
            catch (Exception ex)
            {
                portStatus = PortStatus.Error;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
            }
        }

        private void ReadCallBack(IAsyncResult iar)
        {
            try
            {
                DataRead dataRead = (DataRead)iar.AsyncState;
                int recv = dataRead.ns.EndRead(iar);
                if (recv != 0)
                {
                    byte[] receBytes = new byte[recv];
                    Array.Copy(dataRead.msg, 0, receBytes, 0, recv);
                    if (isExit == false)
                    {
                        dataRead = new DataRead(ns, client.ReceiveBufferSize);
                        ns.BeginRead(dataRead.msg, 0, dataRead.msg.Length, ReadCallBack, dataRead);
                    }
                    //这里发送数据事件
                    PortDataReceived(new ReceBytesEventArge(receBytes));
                }
                else
                {
                    //这里表示掉线了
                    portStatus = PortStatus.Closed;
                    ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
                }
            }
            catch (Exception ex)
            {
                portStatus = PortStatus.Error;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
            }
        }

        public void Stop()
        {
            if (autoReConnTime != null)
            {
                autoReConnTime.Enabled = false;
                autoReConnTime.Elapsed -= AutoReConnTime_Elapsed;
            }
            isExit = true;
            if (client != null && client.Connected)
            {
                client.Close();
            }
            portStatus = PortStatus.Closed;
            ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
        }

        public void Dispose()
        {
            Stop();
            ns.Close();
            client.Client.Close();
            ns = null;
            client = null;
        }

        public void Write(byte[] bytes)
        {
            if (portStatus != PortStatus.Connected) return;
            //在重连的时候，禁止发送数据
            try
            {
                if (ns != null && ns.CanWrite)
                {
                    ns.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(SendCallBack), ns);
                    ns.Flush();
                }
                else
                {
                    portStatus = PortStatus.Closed;
                    ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
                }
            }
            catch (Exception ex)
            {
                portStatus = PortStatus.Error;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
            }

        }

        private void SendCallBack(IAsyncResult iar)
        {
            try
            {
                ns.EndWrite(iar);
            }
            catch (Exception ex)
            {
                portStatus = PortStatus.Error;
                ConnNotify(new ConnStatusChangedEventArgs(DateTime.Now, false, portStatus));
            }
            finally
            {

            }
        }
    }

    public enum PortStatus
    {
        Closed = 1,
        Connecting = 2,
        Connected = 4,
        Error = 8,
    }
}
