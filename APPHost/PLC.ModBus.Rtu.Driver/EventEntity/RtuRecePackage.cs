using System;
using System.Collections.Generic;
using System.Text;

namespace PLC.ModBus.Rtu.Driver.EventEntity
{
    public class RtuRecePackage
    {
        private RtuRegistPackage send;
        
        public RtuRegistPackage SendPackage
        {
            get
            {
                return send;
            }
        }

        private byte[] receBytes;

        public byte[] ReceBytes
        {
            get
            {
                return receBytes;
            }
        }


        public RtuRecePackage(RtuRegistPackage send,byte[] receBytes) 
        {
            this.send = send;
            this.receBytes = receBytes;
        }
    }
}
