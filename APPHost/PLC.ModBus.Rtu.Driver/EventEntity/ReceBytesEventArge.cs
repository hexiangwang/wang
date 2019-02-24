using PLC.ModBus.Rtu.Driver.TcpConn;
using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.EventEntity
{
    public class ReceBytesEventArge : EventArgs
    {
        private byte[] receBytes;

        public byte[] ReceBytes
        {
            get
            {
                return receBytes;
            }
        }

        public ReceBytesEventArge(byte[] receBytes)
        {
            this.receBytes = receBytes;
        }


    }
}
