using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.EventEntity
{
    public class ParkingLotStatusEventArgs
    {
        private string ip;
        /// <summary>
        /// PLC的ID
        /// </summary>
        public string Ip
        {
            get
            {
                return ip;
            }
        }

        private int port;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
        }

        private bool[] status;
        /// <summary>
        /// 车位状态
        /// </summary>
        public bool[] Status
        {
            get
            {
                return status;
            }
        }

        private DateTime sendTime;
        /// <summary>
        /// 指令打包时间
        /// </summary>
        public DateTime SendTime
        {
            get
            {
                return sendTime;
            }
        }

        private DateTime receTime;
        /// <summary>
        /// 指令接收时间
        /// </summary>
        public DateTime ReceTime
        {
            get
            {
                return receTime;
            }
        }

        private bool isSendSuccess;
        /// <summary>
        /// 发送是否成功
        /// </summary>
        private bool IsSendSuccess
        {
            get
            {
                return isSendSuccess;
            }
        }

        public ParkingLotStatusEventArgs(bool isSendSuccess, string ip,int port, bool[] status,DateTime sendtime,DateTime receTime)
        {
            this.isSendSuccess = isSendSuccess;
            this.ip = ip;
            this.port = port;
            this.status = status;
            this.sendTime = sendtime;
            this.receTime = receTime;
        }

        public override string ToString()
        {
            if (!isSendSuccess)
            {
                return $"{ip}:{port} {sendTime} 发送查询指令失败";
            }
            else
            {
                string str = $"{ip}:{port} {sendTime} 发送查询指令成功\r\n";
                for(int i = 0; i < status.Length; i++)
                {
                    str += string.Format("{0}#车位 {1}\r\n", i + 1, status[i] == true ? "使用" : "空闲");
                }
                return str;
            }
        }
    }
}
