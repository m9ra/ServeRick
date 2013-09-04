using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

using SharpServer.Memory;

namespace SharpServer.Networking
{

    delegate void RecieveHandler(Client client, byte[] bufferStorage, int dataLength);

    delegate void SendHandler();

    class Client
    {
        /// <summary>
        /// Socket for communication with client
        /// </summary>
        readonly TcpClient _socket;

        /// <summary>
        /// Clients buffer
        /// </summary>
        readonly DataBuffer _buffer;

        internal readonly HttpRequestParser Parser = new HttpRequestParser();

        internal Response Response { get; set; }

        internal Client(TcpClient clientSocket, DataBuffer clientBuffer)
        {
            _socket = clientSocket;
            _buffer = clientBuffer;
        }


        internal void Recieve(RecieveHandler handler, int maxBytesRecieve = int.MaxValue)
        {
          
            if (maxBytesRecieve > _buffer.Storage.Length)
            {
                maxBytesRecieve = _buffer.Storage.Length;
            }

            
            SocketError error;
            _socket.Client.BeginReceive(_buffer.Storage, 0, maxBytesRecieve, SocketFlags.None, out error, _onRecieved, handler);
            switch (error)
            {
                case SocketError.Success:
                    break;
                case SocketError.Shutdown:
                    onDisconnected();
                    break;
                default:
                    throw new NotImplementedException("Socket error");
            }
        }

        internal void Send(byte[] dataBytes, SendHandler sendHandler)
        {
            SocketError error;
            _socket.Client.BeginSend(dataBytes, 0, dataBytes.Length, SocketFlags.None, out error, _onSended, sendHandler);
            switch (error)
            {
                case SocketError.Success:
                    break;
                case SocketError.ConnectionAborted:
                case SocketError.Shutdown:
                    onDisconnected();
                    break;
                default:
                    throw new NotImplementedException("Socket error");
            }
        }

        private void _onRecieved(IAsyncResult result)
        {
            SocketError error;
            var dataLength = _socket.Client.EndReceive(result, out error);
           

            switch (error)
            {
                case SocketError.Success:
                    break;
                case SocketError.Shutdown:
                    onDisconnected();
                    break;
                default:
                    throw new NotImplementedException("Socket error");
            }

            var handler = result.AsyncState as RecieveHandler;
            handler(this, _buffer.Storage, dataLength);
        }

        private void _onSended(IAsyncResult result)
        {
            SocketError error;
            var dataLength = _socket.Client.EndSend(result, out error);
            
            switch (error)
            {
                case SocketError.Success:
                    break;
                case SocketError.Shutdown:
                    onDisconnected();
                    break;
                default:
                    throw new NotImplementedException("Socket error");
            }
            //TODO why could sending be parted ?

            var handler = result.AsyncState as SendHandler;
            handler();
        }

        private void _onDisconnected(IAsyncResult result)
        {
            _socket.Client.EndDisconnect(result);
            onDisconnected();
        }

        private void onDisconnected()
        {
            _buffer.Recycle();
        }

        internal void Close()
        {
            _socket.Client.BeginDisconnect(false, _onDisconnected, null);
        }
    }
}
