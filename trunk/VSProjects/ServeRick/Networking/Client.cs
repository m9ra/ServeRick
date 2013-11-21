using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using ServeRick.Memory;
using ServeRick.Sessions;
using ServeRick.Processing;

namespace ServeRick.Networking
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
    /// Filler used for accessing clients buffer for sending
    /// </summary>
    /// <param name="buffer">Buffer that will be filled via filler</param>
    /// <returns>Length of data filled in buffer from its begining</returns>
    delegate int BufferFiller(byte[] buffer);

    /// <summary>
    /// Representation of http client
    /// </summary>
    public class Client
    {
        #region Private client members

        /// <summary>
        /// Socket for communication with client
        /// </summary>
        readonly TcpClient _socket;

        /// <summary>
        /// Queue used for work items that are waiting for processing.
        /// Contained work items needs to be run to complete response.
        /// </summary>
        readonly Queue<ClientWorkItem> _responseWork = new Queue<ClientWorkItem>();

        /// <summary>
        /// Determine that work items in work queue are sended to processors
        /// </summary>
        bool _isResponseWorkStarted = false;

        #endregion

        #region Internal API exposed by client

        /// <summary>
        /// Clients buffer
        /// </summary>
        internal readonly DataBuffer Buffer;

        /// <summary>
        /// Parser for http requests
        /// </summary>
        internal readonly HttpRequestParser Parser = new HttpRequestParser();

        /// <summary>
        /// TODO avoid multiple assigning
        /// </summary>
        internal ProcessingUnit Unit { get; set; }

        #endregion

        #region Public API exposed by client

        /// <summary>
        /// ID resolved for client
        /// </summary>
        public string SessionID { get; internal set; }

        public IPAddress IP
        {
            get
            {
                var ep = _socket.Client.RemoteEndPoint as IPEndPoint;
                return ep == null ? null : ep.Address;
            }
        }

        /// <summary>
        /// Response object is used for creating response for client
        /// </summary>
        public Response Response { get; internal set; }

        /// <summary>
        /// TODO avoid multiple assigning
        /// </summary>
        public InputController Input { get; internal set; }

        /// <summary>
        /// Request collected for client
        /// </summary>
        public HttpRequest Request { get { return Parser.Request; } }

        #endregion

        internal Client(TcpClient clientSocket, DataBuffer clientBuffer)
        {
            _socket = clientSocket;
            Buffer = clientBuffer;
        }

        #region Network API for communication with client

        /// <summary>
        /// Recieve data with maximum size according to maxBytesRecieve
        /// </summary>
        /// <param name="handler">Handler called when data are recieved</param>
        /// <param name="maxBytesRecieve">Maximum bytes that can be recieved</param>
        internal void Recieve(RecieveHandler handler, int maxBytesRecieve = int.MaxValue)
        {
            Log.Trace("Client.Recieve {0}", this);

            //limit maximum bytes that can be recieved according to buffer storage size
            if (maxBytesRecieve > Buffer.Storage.Length)
                maxBytesRecieve = Buffer.Storage.Length;

            SocketError error;
            _socket.Client.BeginReceive(Buffer.Storage, 0, maxBytesRecieve, SocketFlags.None, out error, _onRecieved, handler);

            checkError("Client.Recieve", error);
        }

        /// <summary>
        /// Send given data from given storage. After data are sended sendHandler is called
        /// </summary>
        /// <param name="dataStorage">Storage for data to send</param>
        /// <param name="sendHandler">Handler that is called after data are sended</param>
        internal void Send(byte[] dataStorage, SendHandler sendHandler)
        {
            Log.Trace("Client.Send {0}", this);

            SocketError error;
            _socket.Client.BeginSend(dataStorage, 0, dataStorage.Length, SocketFlags.None, out error, _onSended, sendHandler);

            checkError("Client.Send", error);
        }

        /// <summary>
        /// Close connection with client
        /// </summary>
        internal void Close()
        {
            if (_socket.Connected)
                _socket.Client.BeginDisconnect(false, _onDisconnected, null);
        }

        #endregion

        #region Network operation callbacks

        private void _onRecieved(IAsyncResult result)
        {
            Log.Trace("Client._onRecieved {0}", this);

            SocketError error;
            var dataLength = _socket.Client.EndReceive(result, out error);

            if (checkError("Client._onRecieved", error))
            {
                //On error we stop processing                
                return;
            }

            var handler = result.AsyncState as RecieveHandler;
            handler(this, Buffer.Storage, dataLength);
        }

        private void _onSended(IAsyncResult result)
        {
            Log.Trace("Client._onRecieved {0}", this);

            SocketError error;
            var dataLength = _socket.Client.EndSend(result, out error);

            //TODO why could sending be parted ?

            if (checkError("Client._onSended", error))
            {
                //On error we stop processing
                return;
            }

            var handler = result.AsyncState as SendHandler;
            handler();
        }

        private void _onDisconnected(IAsyncResult result)
        {
            _socket.Client.EndDisconnect(result);
            onDisconnected();
        }

        #endregion

        #region Clients response work items handling

        /// <summary>
        /// Enqueue given work items to be processed to complete client response.
        /// </summary>
        /// <param name="workItems">Work items to be enqueued.</param>
        internal void EnqueueWork(params ClientWorkItem[] workItems)
        {
            foreach (var work in workItems)
            {
                work.SetClient(this);
                _responseWork.Enqueue(work);
            }
        }

        /// <summary>
        /// Start processing of response work items on client processing unit.
        /// </summary>
        internal void StartQueueProcessing()
        {
            if (_isResponseWorkStarted)
                throw new NotSupportedException("Work processing can be started only once");

            _isResponseWorkStarted = true;
            ProcessNextWorkItem();
        }

        /// <summary>
        /// Process next response work item in queue. If there the queue is
        /// empty, response is closed
        /// </summary>
        internal void ProcessNextWorkItem()
        {
            if (!_isResponseWorkStarted)
                throw new NotSupportedException("Next work item cannot be processed until work processing starts");

            if (_responseWork.Count == 0)
            {
                Response.Close();
                //work for currrent client has been done
                return;
            }

            var work = _responseWork.Dequeue();
            work.EnqueueToProcessor();
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Handler called when client is disconnected (expectedly or unexpectedly)
        /// </summary>
        private void onDisconnected()
        {
            Buffer.Recycle();
        }

        /// <summary>
        /// Check error from socket operation
        /// </summary>
        /// <param name="error">Error object</param>
        private bool checkError(string checkingMethod, SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                    //socket operation has been successfull
                    return false;
                case SocketError.NotSocket:
                case SocketError.ConnectionReset:
                case SocketError.ConnectionAborted:
                case SocketError.Shutdown:
                    //client has unexpectedly disconnected
                    Log.Error(checkingMethod + " {0} failed with {1}", this, error);
                    onDisconnected();
                    return true;
                default:
                    throw new NotImplementedException("Socket error: " + error);
            }
        }

        public override string ToString()
        {
            return "Client: " + GetHashCode();
        }

        #endregion



    }
}
