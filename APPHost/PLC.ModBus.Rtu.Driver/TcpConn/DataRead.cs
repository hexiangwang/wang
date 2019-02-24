using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.TcpConn
{
    internal class DataRead
    {
        public NetworkStream ns;

        public byte[] msg;

        public DataRead(NetworkStream ns, int buffsersize)
        {
            this.ns = ns;
            msg = new byte[buffsersize];
        }
    }
}
