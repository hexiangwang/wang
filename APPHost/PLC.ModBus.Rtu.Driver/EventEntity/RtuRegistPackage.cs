using PLC.ModBus.Rtu.Driver.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.EventEntity
{
    /// <summary>
    /// 目前只考虑03
    /// </summary>
    public class RtuRegistPackage
    {
        /// <summary>
        /// 仪器地址
        /// </summary>
        public byte Address { get;private set; }

        /// <summary>
        /// 功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        /// 寄存器地址
        /// </summary>
        public ushort RegisterAddress { get; private set; }


        /// <summary>
        /// 寄存器长度或者要写入的值
        /// </summary>
        public ushort LengthOrValue { get; private set; }

        /// <summary>
        /// 发送的数据包
        /// </summary>
        public byte[] PackageBytes { get; private set; }

        private string userName;

        public string UserName
        {
            get
            {
                return userName;
            }
        }

        private string descript;

        public string Descript
        {
            get
            {
                return descript;
            }
        }

        private DateTime createTime;

        public DateTime CreateTime
        {
            get
            {
                return createTime;
            }
        }

        private DateTime sendTime;

        public DateTime SendTime
        {
            get
            {
                return sendTime;
            }
            set
            {
                sendTime = value;
            }
        }

        /// <summary>
        /// modbus数据包数据
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="funCode"></param>
        /// <param name="registAddress"></param>
        /// <param name="lengthOrValue"></param>
        public RtuRegistPackage(string userName,string descript, byte addr, byte funCode, UInt16 registAddress, UInt16 lengthOrValue)
        {
            this.userName = userName;
            this.descript = descript;
            this.createTime = DateTime.Now;
            this.Address = addr;
            this.FunctionCode = funCode;
            this.RegisterAddress = registAddress;
            this.LengthOrValue = lengthOrValue;
            this.PackageBytes = CreatePackage(addr, funCode, registAddress, lengthOrValue);
        }

        public RtuRegistPackage(byte[] bytes)
        {
            this.PackageBytes = bytes;
            this.Address = bytes[0];
            this.FunctionCode = bytes[1];
            this.RegisterAddress = (ushort)(bytes[2] * 256 + bytes[3]);
            this.LengthOrValue = (ushort)(bytes[4] * 256 + bytes[5]);
        }

        /// <summary>
        /// 创建数据包
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="funCode"></param>
        /// <param name="registAddress"></param>
        /// <param name="registLength"></param>
        /// <returns></returns>
        public static byte[] CreatePackage(byte addr, byte funCode, UInt16 registAddress, UInt16 registLength)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.Add(funCode);
            bytes.Add((byte)(registAddress >> 8));
            bytes.Add((byte)registAddress);
            bytes.Add((byte)(registLength >> 8));
            bytes.Add((byte)registLength);
            //必须为CRC校验
            ushort tempCrc = CRC16.CRC16Code(bytes.ToArray());
            bytes.Add((byte)(tempCrc >> 8));
            bytes.Add((byte)tempCrc);
            return bytes.ToArray();
        }

        public override string ToString()
        {
            string str = $"{userName} 构造时间:{createTime.ToString()} 发送时间:{sendTime.ToString()} {descript}\r\n发送指令:";
            return str += BytesToHex(PackageBytes);
        }

        private string BytesToHex(byte[] bytes)
        {
            string str = "";
            for(int i = 0; i < bytes.Length; i++)
            {
                str += string.Format(@"{0} ",bytes[i].ToString("X2"));
            }
            return str;
        }

    }
}
