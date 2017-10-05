using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

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

        readonly DateTime _creationTime = DateTime.Now;

        /// <summary>
        /// Chain of work items for processing clients response
        /// </summary>
        WorkChain _workChain;

        /// <summary>
        /// Determine that clients socket has been already closed
        /// </summary>
        private volatile bool _isClosed;

        /// <summary>
        /// Determine that client has been completly disconnected
        /// </summary>
        private volatile bool _isDisconnected;

        /// <summary>
        /// Determine that disconnecting has been already started
        /// </summary>
        private volatile bool _disconnectingStarted;

        #endregion

        #region Internal API exposed by client

        /// <summary>
        /// Determine that client is alredy closed
        /// </summary>
        internal bool IsClosed { get { return _isClosed; } }


        internal int TimeFromStart { get { return (DateTime.Now - _creationTime).Milliseconds; } }

        /// <summary>
        /// Clients buffer
        /// </summary>
        internal readonly DataBuffer Buffer;

        /// <summary>
        /// Parser for http requests
        /// </summary>
        internal readonly HttpRequestParser Parser = new HttpRequestParser();

        /// <summary>
        /// Processing unit belonging to client;
        /// </summary>
        private ProcessingUnit _unit;

        /// <summary>
        /// TODO avoid multiple assigning
        /// </summary>
        internal ProcessingUnit Unit { get { return _unit; } }

        #endregion

        #region Public API exposed by client

        public static long TotalRecievedData;

        public static long TotalSendedData;

        public static long TotalClients;

        /// <summary>
        /// ID resolved for client
        /// </summary>
        public string SessionID { get; internal set; }

        private static object _L_activeClients = new object();

        public static int ActiveClients { get; private set; }

        public readonly IPAddress IP;

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

        internal Client(TcpClient clientSocket, IPAddress ip, DataBuffer clientBuffer)
        {
            _socket = clientSocket;
            IP = ip;
            Buffer = clientBuffer;

            lock (_L_activeClients)
            {
                ++TotalClients;
                ++ActiveClients;
            }
        }

        internal void SetUnit(ProcessingUnit unit)
        {
            if (_unit != null)
                throw new InvalidOperationException("Cannot set unit multiple times");

            _unit = unit;
            initializeWorkChain();
        }

        /// <summary>
        /// Removes all handlers attached 
        /// </summary>
        internal void RemoveHandlers()
        {
            //there are no handlers
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

            _socket.Client.BeginReceive(Buffer.Storage, 0, maxBytesRecieve, SocketFlags.None, out SocketError error, _onRecieved, handler);

            checkError("Client.Recieve", error);
        }

        /// <summary>
        /// Send given data from given storage. After data are sent sendHandler is called
        /// </summary>
        /// <param name="dataStorage">Storage for data to send</param>
        /// <param name="sendedLength">Length of data that will be sended</param>
        /// <param name="sendHandler">Handler that is called after data are sended</param>
        internal void Send(byte[] dataStorage, int sendedLength, SendHandler sendHandler)
        {
            Log.Trace("Client.Send {0}", this);

            _socket.Client.BeginSend(dataStorage, 0, sendedLength, SocketFlags.None, out var error, _onSent, Tuple.Create<SendHandler, Socket>(sendHandler, _socket.Client));

            checkError("Client.Send", error);
        }

        /// <summary>
        /// Close connection with client
        /// </summary>
        internal void Close()
        {
            if (_isClosed)
                return;

            _isClosed = true;
            _socket.Client.BeginDisconnect(false, _onClosed, null);
        }

        #endregion

        #region Network operation callbacks

        private void _onRecieved(IAsyncResult result)
        {
            Log.Trace("Client._onRecieved {0}", this);

            var dataLength = _socket.Client.EndReceive(result, out SocketError error);

            if (checkError("Client._onRecieved", error))
            {
                //On error we stop processing                
                return;
            }

            Interlocked.Add(ref TotalRecievedData, dataLength);
            var handler = result.AsyncState as RecieveHandler;
            handler(this, Buffer.Storage, dataLength);
        }

        private void _onSent(IAsyncResult result)
        {
            Log.Trace("Client._onSent {0}", this);

            var error = SocketError.Success;
            var state = result.AsyncState as Tuple<SendHandler, Socket>;
            int dataLength = 0;
            try
            {
                dataLength = state.Item2.EndSend(result, out error);

                //TODO how could happen sending be parted ?

                if (checkError("Client._onSent", error))
                {
                    //On error we stop processing
                    return;
                }
            }
            catch(ObjectDisposedException ex)
            {
                var requestURI = this.Request?.RequestURI;
                Log.Error("Client._onSent [Exception]: {0} | {1} | {2}", requestURI, error, ex);
                _onClosed();
                return;
            }
            catch (SocketException ex)
            {
                var requestURI = this.Request?.RequestURI;
                Log.Error("Client._onSent [Exception]: {0} | {1} | {2}", requestURI, error, ex);
                _onClosed();
                return;
            }

            Interlocked.Add(ref TotalSendedData, dataLength);
            state.Item1?.Invoke();
        }

        private void _onClosed(IAsyncResult result = null)
        {
            if (result != null && _socket.Connected)
            {
                try
                {
                    _socket.Client.EndDisconnect(result);
                }
                catch (SocketException ex)
                {
                    //why on Earth this is solved via exceptions??
                    var errorCode = ex.SocketErrorCode;
                    Log.Error("Client._oClosed Socket exception {0}", errorCode);
                    switch (errorCode)
                    {
                        case SocketError.ConnectionReset:
                            //peer has disconnect without proper notification
                            break;
                        default:
                            throw;
                    }

                }
            }

            _isClosed = true;
            disconnect();
        }

        #endregion

        #region Clients response work items handling

        /// <summary>
        /// Enqueue given work items to be processed to complete client response.
        /// </summary>
        /// <param name="workItems">Work items to be enqueued.</param>
        internal void EnqueueWork(params WorkItem[] workItems)
        {
            foreach (var work in workItems)
            {
                _workChain.InsertItem(work);
            }
        }

        /// <summary>
        /// Start processing of response work items on client processing unit.
        /// </summary>
        internal void StartChainProcessing()
        {
            _workChain.StartProcessing();
        }

        #endregion

        #region Private utilities

        private void initializeWorkChain()
        {
            _workChain = new WorkChain(_unit);
            _workChain.OnCompleted += onChainCompleted;
        }

        /// <summary>
        /// Handles correct disconnecting sequence (even in partially disconnected states)
        /// </summary>
        private void disconnect()
        {
            if (_isDisconnected)
            {
                Log.Error("Cannot disconnect client {0} twice", this);
                return;
            }

            _disconnectingStarted = true;

            if (_workChain != null && !_workChain.IsComplete)
            {
                //we have to abort remaining work
                _workChain.Abort();

                //aborting workchain causes completition routine to be called
                //so disconnecting will then continue
                return;
            }

            if (_socket.Connected)
            {
                //close socket
                Log.Trace("Client.disconnected/close {0}", this);

                _socket.Client.BeginDisconnect(false, _onClosed, null);

                //disconnecting continues after socket is closed
                return;
            }
            else
            {
                _isClosed = true;
            }

            if (_isClosed)
            {
                //socket is closed - we will lease resources
                _socket.Client.Dispose();
                Buffer.Recycle();

                _isDisconnected = true;

                lock (_L_activeClients)
                {
                    --ActiveClients;
                }

                Log.Trace("Client.disconnected/dispose {0}", this);
                //disconnecting is complete
            }
        }

        /// <summary>
        /// Handler called after client is completed - its work, or has disconnected,...
        /// </summary>
        private void onChainCompleted()
        {
            if (_disconnectingStarted || Response == null)
            {
                //client completed before response has been started
                //or client is forced to be disconnected
                disconnect();
            }
            else
            {
                //disconnect after response is completed
                Response.AfterSend += disconnect;
                Response.Close();
            }
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
                case SocketError.TimedOut:
                case SocketError.ConnectionAborted:
                case SocketError.Shutdown:
                case SocketError.HostUnreachable:
                case SocketError.ConnectionRefused:
                case SocketError.NetworkUnreachable:
                    //client has unexpectedly disconnected
                    Log.Warning(checkingMethod + " {0} failed with {1}", this, error);
                    _onClosed();
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
