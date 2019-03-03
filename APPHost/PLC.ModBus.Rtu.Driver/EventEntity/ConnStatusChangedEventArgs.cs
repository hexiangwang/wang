using PLC.ModBus.Rtu.Driver.TcpConn;
using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.EventEntity
{
    public class ConnStatusChangedEventArgs : EventArgs
    {
        private DateTime dt;

        public DateTime Dt
        {
            get
            {
                return dt;
            }
        }

        private PortStatus status;

        public PortStatus Status
        {
            get
            {
                return status;
            }
        }

        private bool isConn;

        public bool IsConn
        {
            get
            {
                return isConn;
            }
        }

        public ConnStatusChangedEventArgs(DateTime dt, bool isConn, PortStatus status)
        {
            this.dt = dt;
            this.isConn = isConn;
            this.status = status;
        }
    }
}
