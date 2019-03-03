using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.Descriptor
{
    /*
     * 对PLC的描述信息
     * 基本信息+命令信息
     * */
    public class PLCDescriptor
    {
        //private string id;
        ///// <summary>
        ///// ID标识 后期由具体配置去改
        ///// </summary>
        //public string Id
        //{
        //    get
        //    {
        //        return id;
        //    }
        //}

        private byte salve;
        /// <summary>
        /// 从机地址
        /// </summary>
        public byte Salve
        {
            get
            {
                return salve;
            }
        }

        private string ipAddress;
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress
        {
            get
            {
                return ipAddress;
            }
        }

        private int port;
        /// <summary>
        /// 通讯端口
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
        }

        private int sendInterval;
        /// <summary>
        /// 发送间隔
        /// </summary>
        public int SendInterval
        {
            get
            {
                return sendInterval;
            }
        }

        private int autoReConnInterval;
        /// <summary>
        /// 重连间隔
        /// </summary>
        public int AutoReConnInterval
        {
            get
            {
                return autoReConnInterval;
            }
        }

        public PLCDescriptor(byte salve, string ipAddress, int port = 502, int sendInterval = 30, int autoReConnInterval = 10)
        {
            this.salve = salve;
            this.ipAddress = ipAddress;
            this.port = port;
            this.sendInterval = sendInterval;
            this.autoReConnInterval = autoReConnInterval;
        }
    }
}
