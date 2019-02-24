using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
    interface ISimpleWebSoket
    {
        event EventHandler<MyWebSocketEventArgs> OnMessage;
        event EventHandler<MyWebSocketEventArgs> OnClose;   
        Task SendAsync(string message);

        void KeepAlive();
    }
}
