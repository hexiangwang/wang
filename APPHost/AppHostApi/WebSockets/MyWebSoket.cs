using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
    public class MyWebSoket : WebSocket, ISimpleWebSoket
    {
        private WebSocket _webSocket;

        public MyWebSoket(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }


        #region ISimpleWebSoket 实现
        public event EventHandler<MyWebSocketEventArgs> OnMessage;
        public event EventHandler<MyWebSocketEventArgs> OnClose;

        /// <summary>
        /// 接受消息
        /// </summary>
        /// <returns>接收到的消息</returns>
        public async Task<string> ReceiveAsync()
        {
            var bytes = new byte[1024];
            WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(bytes, 0, result.Count);
            }


            //接受数据量过大时
            List<byte> byteList = new List<byte>(bytes);
            do
            {
                bytes = new byte[1024 * 2];
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
                var validData = result.Count == bytes.Length ? bytes : bytes.Take(result.Count);
                byteList.AddRange(validData);
            }
            while (!result.EndOfMessage);

            string message = Encoding.UTF8.GetString(byteList.ToArray());
            return message;

        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">要发送的消息</param>
        /// <returns></returns>
        public async Task SendAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// 保持长连接
        /// </summary>
        public void KeepAlive()
        {
            while (!_webSocket.CloseStatus.HasValue)
            {
                string data = this.ReceiveAsync().Result;
                OnMessage?.Invoke(_webSocket, new MyWebSocketEventArgs(data));
            }
            OnClose?.Invoke(this, null);
        }

        #endregion

        #region WebSocket继承

        public override WebSocketCloseStatus? CloseStatus => _webSocket.CloseStatus;

        public override string CloseStatusDescription => _webSocket.CloseStatusDescription;

        public override WebSocketState State => _webSocket.State;

        public override string SubProtocol => _webSocket.SubProtocol;

        public override void Abort()
        {
            _webSocket.Abort();
        }

        public override async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            await _webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override void Dispose()
        {
            _webSocket.Dispose();
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return await _webSocket.ReceiveAsync(buffer, cancellationToken);
        }



        public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await _webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }
        #endregion



    }

    public class MyWebSocketEventArgs
    {
        public MyWebSocketEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get;  private set; }
    }
}