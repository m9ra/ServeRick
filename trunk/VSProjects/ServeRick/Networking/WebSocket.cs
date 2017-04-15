using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

namespace ServeRick.Networking
{
    public class WebSocket
    {
        /// <summary>
        /// Magic string defined by RFC.
        /// </summary>
        static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// Client which is used for websocket communication.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// Controller of this websocket.
        /// </summary>
        private readonly WebSocketController _controller;

        /// <summary>
        /// Websocket's key.
        /// </summary>
        private readonly string _key;

        /// <summary>
        /// Lock for message sending.
        /// </summary>
        private readonly object _L_send = new object();

        /// <summary>
        /// Lock for sockets fields.
        /// </summary>
        private readonly object _L_fields = new object();

        /// <summary>
        /// Field values.
        /// </summary>
        private readonly Dictionary<object, object> _fields = new Dictionary<object, object>();

        internal WebSocket(WebSocketController controller, Client client, string key)
        {
            if (key == null)
                throw new NullReferenceException("_key");

            _controller = controller;
            _client = client;
            _key = key;

            _client.RemoveHandlers();
        }

        /// <summary>
        /// Gets value stored for given field.
        /// </summary>
        /// <typeparam name="T">Type of field</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>The value.</returns>
        public T Get<T>(WebSocketField<T> field)
        {
            if (!_fields.TryGetValue(field, out object fieldValue))
                return default(T);

            return (T)fieldValue;
        }


        /// <summary>
        /// Sets value for given field.
        /// </summary>
        /// <typeparam name="T">Type of field</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public T Set<T>(WebSocketField<T> field, T value)
        {
            _fields[field] = value;
            return value;
        }

        /// <summary>
        /// Initialize field with given initialization value value. If field already contains a value, initializationValue is not used.
        /// </summary>
        /// <typeparam name="T">Type of field</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="initializationValue">The value.</param>
        /// <returns><c>true</c> if initalization was used, <c>false</c> otherwise</returns>
        public bool Initialize<T>(WebSocketField<T> field, T initializationValue)
        {
            if (_fields.ContainsKey(field))
                return false;

            _fields[field] = initializationValue;
            return true;
        }

        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var messageLength = (UInt64)bytes.Length;

            var finFlag = 1;

            var opcode = 1;
            var mask = 0u;
            var header = new List<byte>(10);
            header.Add((byte)(finFlag << 7 | opcode));

            if (messageLength < 126)
            {
                header.Add((byte)(mask << 7 | messageLength));
            }
            else
            {
                var lenByteCount = messageLength < UInt16.MaxValue ? 2 : 8;
                var lenCode = messageLength < UInt16.MaxValue ? 126u : 127u;
                header.Add((byte)(mask << 7 | lenCode));

                var lengthBytes = BitConverter.GetBytes(messageLength).Reverse().ToArray();
                header.AddRange(lengthBytes.Skip(8 - lenByteCount).Take(lenByteCount));
            }

            var buffer = header.Concat(bytes).ToArray();
            lock (_L_send)
            {
                //TODO revise - client might be able to lock itself
                _client.Send(buffer, buffer.Length, null);
            }
        }

        internal void CompleteHandshake()
        {
            var acceptKey = calculateAcceptKey(_key);

            var handshakeCompletition = string.Format(
@"HTTP/1.1 101 Switching Protocols
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Accept: {0}" + "\r\n\r\n", acceptKey);

            var handshakeBytes = Encoding.ASCII.GetBytes(handshakeCompletition);
            _client.Send(handshakeBytes, handshakeBytes.Length, _onHandshakeSent);
        }

        private void _onHandshakeSent()
        {
            receiveFrame();
            _controller.OnOpen(this);
        }

        private void receiveFrame()
        {
            _client.Recieve(_onFrameReceive);
        }

        private void _onFrameReceive(Client client, byte[] data, int length)
        {
            if (length == 0)
            {
                closeClient();
                return;
            }
            var message = GetDecodedData(data, length);
            if (message == null)
            {
                closeClient();
                return;
            }

            _controller.MessageReceived(this, message);
            receiveFrame();
        }

        private void closeClient()
        {
            _client.Close();
            _controller.OnClose(this);
        }

        public static string GetDecodedData(byte[] buffer, int length)
        {
            var opcode = (buffer[0] & 0x0F);
            var finFlag = buffer[0] & 128;
            if (finFlag == 0)
                throw new NotImplementedException();

            switch (opcode)
            {
                case 1:
                    //text
                    break;
                case 8:
                    //close
                    return null;
                default:
                    throw new NotImplementedException("Unknown opcode: " + opcode);
            }

            var payloadLength = buffer[1] - 128;
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (payloadLength <= 125)
            {
                dataLength = payloadLength;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (payloadLength == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (payloadLength - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is smaller than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }

        private string calculateAcceptKey(string key)
        {
            var longKey = key + guid;
            var sha1 = SHA1CryptoServiceProvider.Create();
            var hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
