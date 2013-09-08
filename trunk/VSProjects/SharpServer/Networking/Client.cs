﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

using SharpServer.Memory;

namespace SharpServer.Networking
{
    /// <summary>
    /// Handler for events when data are recieved from client
    /// </summary>
    /// <param name="client">Client which data has been recieved</param>
    /// <param name="bufferStorage">Buffer storage where recieved data are stored. Storage can be used ONLY WHEN HANDLER IS CALLED. Otherwise it can be unexpectedly recycled</param>
    /// <param name="dataLength">Length of data that has been recieved</param>
    delegate void RecieveHandler(Client client, byte[] bufferStorage, int dataLength);

    /// <summary>
    /// Handler used when data are sended
    /// </summary>
    delegate void SendHandler();

    /// <summary>
    /// Representation of http client
    /// </summary>
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

        /// <summary>
        /// Parser for http requests
        /// </summary>
        internal readonly HttpRequestParser Parser = new HttpRequestParser();

        /// <summary>
        /// Response object is used for creating response for client
        /// </summary>
        internal Response Response { get; set; }

        internal Client(TcpClient clientSocket, DataBuffer clientBuffer)
        {
            _socket = clientSocket;
            _buffer = clientBuffer;
        }

        #region Network API for communication with client

        /// <summary>
        /// Recieve data with maximum size according to maxBytesRecieve
        /// </summary>
        /// <param name="handler">Handler called when data are recieved</param>
        /// <param name="maxBytesRecieve">Maximum bytes that can be recieved</param>
        internal void Recieve(RecieveHandler handler, int maxBytesRecieve = int.MaxValue)
        {
            //limit maximum bytes that can be recieved according to buffer storage size
            if (maxBytesRecieve > _buffer.Storage.Length)
                maxBytesRecieve = _buffer.Storage.Length;

            SocketError error;
            _socket.Client.BeginReceive(_buffer.Storage, 0, maxBytesRecieve, SocketFlags.None, out error, _onRecieved, handler);

            checkError(error);
        }

        /// <summary>
        /// Send given data from given storage. After data are sended sendHandler is called
        /// </summary>
        /// <param name="dataStorage">Storage for data to send</param>
        /// <param name="sendHandler">Handler that is called after data are sended</param>
        internal void Send(byte[] dataStorage, SendHandler sendHandler)
        {
            SocketError error;
            _socket.Client.BeginSend(dataStorage, 0, dataStorage.Length, SocketFlags.None, out error, _onSended, sendHandler);

            checkError(error);
        }

        /// <summary>
        /// Close connection with client
        /// </summary>
        internal void Close()
        {
            _socket.Client.BeginDisconnect(false, _onDisconnected, null);
        }

        #endregion

        #region Network operation callbacks

        private void _onRecieved(IAsyncResult result)
        {
            SocketError error;
            var dataLength = _socket.Client.EndReceive(result, out error);

            checkError(error);

            var handler = result.AsyncState as RecieveHandler;
            handler(this, _buffer.Storage, dataLength);
        }

        private void _onSended(IAsyncResult result)
        {
            SocketError error;
            var dataLength = _socket.Client.EndSend(result, out error);

            //TODO why could sending be parted ?

            checkError(error);

            var handler = result.AsyncState as SendHandler;
            handler();
        }

        private void _onDisconnected(IAsyncResult result)
        {
            _socket.Client.EndDisconnect(result);
            onDisconnected();
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Handler called when client is disconnected (expectedly or unexpectedly)
        /// </summary>
        private void onDisconnected()
        {
            _buffer.Recycle();
        }

        /// <summary>
        /// Check error from socket operation
        /// </summary>
        /// <param name="error">Error object</param>
        private void checkError(SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                    //socket operation has been successfull
                    break;
                case SocketError.NotSocket:
                case SocketError.ConnectionReset:
                case SocketError.ConnectionAborted:
                case SocketError.Shutdown:
                    //client has unexpectedly disconnected
                    onDisconnected();
                    break;
                default:
                    throw new NotImplementedException("Socket error: "+error);
            }
        }

        #endregion
    }
}