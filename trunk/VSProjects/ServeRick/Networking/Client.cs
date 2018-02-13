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
    /// States which describes client life cycle.
    /// </summary>
    public enum ClientConnectionState { Connected, BeforeDisconnect, AfterDisconnect, AfterError, AfterClose, Disposed }

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
        /// When client arrived to system.
        /// </summary>
        readonly DateTime _creationTime = DateTime.Now;

        /// <summary>
        /// Chain of work items for processing clients response
        /// </summary>
        volatile WorkChain _workChain;

        /// <summary>
        /// Current state of client connection.
        /// </summary>
        volatile ClientConnectionState _currentConnectionState;

        /// <summary>
        /// Access to client state has to be protected by the lock.
        /// </summary>
        object _L_state = new object();

        /// <summary>
        /// If set stores network recording data
        /// </summary>
        List<byte> _networkRecording = null;

        #endregion

        #region Internal API exposed by client

        /// <summary>
        /// Determine that client is alredy closed
        /// </summary>
        internal bool IsClosed { get { return _currentConnectionState != ClientConnectionState.Connected; } }

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

        /// <summary>
        /// Event fired after client is completely disconnected and resources are freed.
        /// </summary>
        internal event Action OnDisconnected;

        #endregion

        #region Public API exposed by client

        public static long TotalRecievedData;

        public static long TotalSentData;

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

            _currentConnectionState = ClientConnectionState.Connected;

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

        #region Network API for communication with client

        /// <summary>
        /// Receive data with maximum size according to maxBytesReceive
        /// </summary>
        /// <param name="handler">Handler called when data are received</param>
        /// <param name="maxBytesReceive">Maximum bytes that can be received</param>
        internal void Receive(RecieveHandler handler, int maxBytesReceive = int.MaxValue)
        {
            Log.Trace("Client.Receive {0}", this);

            //limit maximum bytes that can be received according to buffer storage size
            if (maxBytesReceive > Buffer.Storage.Length)
                maxBytesReceive = Buffer.Storage.Length;

            socketOperationWrapper("Client.Receive", () =>
            {
                _socket.Client.BeginReceive(Buffer.Storage, 0, maxBytesReceive, SocketFlags.None, out SocketError error, _onReceived, handler);
                return error;
            });
        }

        /// <summary>
        /// Enables recording of cache writes.
        /// Causes all writes to have no network effect.
        /// </summary>
        internal void EnableNetworkRecording()
        {
            _networkRecording = new List<byte>();
        }

        /// <summary>
        /// Collects recorded cache data.
        /// </summary>
        internal byte[] CollectCache()
        {
            try
            {
                return _networkRecording.ToArray();
            }
            finally
            {
                _networkRecording = null;
            }
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

            socketOperationWrapper("Client.Send", () =>
            {
                _socket.Client.BeginSend(dataStorage, 0, sendedLength, SocketFlags.None, out var error, _onSent, Tuple.Create<SendHandler, Socket>(sendHandler, _socket.Client));
                return error;
            });
        }

        /// <summary>
        /// Closes client after some unexpected behaviour.
        /// </summary>
        internal void CloseOnError()
        {
            reportState(ClientConnectionState.AfterError);
        }

        /// <summary>
        /// Gracefuly disconnects client.
        /// </summary>
        internal void Disconnect()
        {
            reportState(ClientConnectionState.BeforeDisconnect);
        }

        #endregion

        #region Network operation callbacks

        private void _onReceived(IAsyncResult result)
        {
            Log.Trace("Client._onReceived {0}", this);

            int dataLength = 0;
            if (!socketOperationWrapper("Client._onReceived", () =>
            {
                dataLength = _socket.Client.EndReceive(result, out SocketError error);
                return error;
            }))
                //On error we stop processing                
                return;

            if (dataLength == 0)
            {
                //disconnection
                reportState(ClientConnectionState.BeforeDisconnect);
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

            if (!socketOperationWrapper("Client._onSent", () =>
            {
                dataLength = state.Item2.EndSend(result, out error);
                //TODO how could happen sending be parted ?
                return error;
            }))
                //on error stop processing
                return;

            Interlocked.Add(ref TotalSentData, dataLength);
            state.Item1?.Invoke();
        }

        private bool socketOperationWrapper(string actionName, Func<SocketError> socketAction, ClientConnectionState requiredMaximalState = ClientConnectionState.Connected)
        {
            lock (_L_state)
            {
                if (_currentConnectionState > requiredMaximalState)
                    return false;
            }

            try
            {
                var error = socketAction();
                if (error != SocketError.Success)
                {
                    reportState(ClientConnectionState.AfterError);
                    return false;
                }
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                var requestURI = this.Request?.RequestURI;
                Log.Error(actionName + " [Exception]: {0} | {1}", requestURI, ex);
                reportState(ClientConnectionState.AfterError);
                return false;
            }
            catch (SocketException ex)
            {
                var requestURI = this.Request?.RequestURI;
                Log.Error(actionName + " [Exception]: {0} | {1}", requestURI, ex);
                reportState(ClientConnectionState.AfterError);
                return false;
            }
        }

        #endregion

        #region Clients response work items handling

        /// <summary>
        /// Enqueue given work items to be processed to complete client response.
        /// </summary>
        /// <param name="workItems">Work items to be enqueued.</param>
        internal void EnqueueWork(params WorkItem[] workItems)
        {
            if (_networkRecording != null)
            {
                foreach (var work in workItems)
                {
                    var recordabe = work as INetworkRecordable;
                    if (recordabe == null)
                        throw new NotImplementedException();

                    _networkRecording.AddRange(recordabe.GetData());
                }

                //don't allow any real effects
                return;
            }


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

        private void reportState(ClientConnectionState state)
        {
            lock (_L_state)
            {
                if (_currentConnectionState >= state)
                    //there is nothing to do
                    return;

                _currentConnectionState = state;
            }

            // states are ordered in a decreasing order 
            // states can be increased only - therefore no state sequence won't be called twice
            // howeve, in some cirmustances there can be race condition - bigger state can be called before smaller one
            switch (state)
            {
                case ClientConnectionState.Connected:
                    //there is nothing to do
                    return;

                case ClientConnectionState.BeforeDisconnect:
                    _socket.Client.BeginDisconnect(false, _beginDisconnectCallback, this);
                    return;

                case ClientConnectionState.AfterDisconnect:
                    _socket.Client.Close();
                    reportState(ClientConnectionState.AfterClose);
                    return;

                case ClientConnectionState.AfterError:
                    _socket.Client.Close();
                    reportState(ClientConnectionState.AfterClose);
                    return;

                case ClientConnectionState.AfterClose:
                    _socket.Client.Dispose();
                    reportState(ClientConnectionState.Disposed);
                    return;

                case ClientConnectionState.Disposed:
                    // each client once has to reach this state
                    clientCleanup();
                    return;

                default:
                    throw new NotImplementedException("Unknown state reported: " + state);
            }
        }

        private void _beginDisconnectCallback(IAsyncResult ar)
        {
            //this is only paranoya - sometimes it looked 'this' was different to 'currentClient'
            var currentClient = ar.AsyncState as Client;
            currentClient._socket.Client.EndDisconnect(ar);
            reportState(ClientConnectionState.AfterDisconnect);
        }

        /// <summary>
        /// Handles correct disconnecting sequence (even in partially disconnected states)
        /// </summary>
        private void clientCleanup()
        {
            if (_workChain != null && !_workChain.IsComplete)
            {
                //we have to abort remaining work
                _workChain.Abort();
            }

            Buffer.Recycle();
            OnDisconnected?.Invoke();

            lock (_L_activeClients)
            {
                --ActiveClients;
            }

            Log.Trace("Client.disposed {0}", this);
        }

        /// <summary>
        /// Handler called after client is completed - its work, or has disconnected,...
        /// </summary>
        private void onChainCompleted()
        {
            lock (_L_state)
            {
                if (_currentConnectionState != ClientConnectionState.Connected)
                    //disconnection process is running
                    return;
            }

            //disconnect after response is completed
            Response.AfterSend += () => reportState(ClientConnectionState.BeforeDisconnect);
            Response.Close();
        }

        public override string ToString()
        {
            return "Client: " + GetHashCode();
        }

        #endregion
    }
}
