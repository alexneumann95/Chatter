using System.Net.Sockets;

namespace Chatter
{
    class Client
    {
        private Socket _handler;
        private byte[] _id; // Each client has a unique ID
        private byte[] _receiveBuffer = new byte[Server.BufferSize];
        private byte[] _sendBuffer = new byte[Server.BufferSize];

        public Socket Handler { get => _handler; }
        public byte[] ID { get => _id; }
        public byte[] ReceiveBuffer { get => _receiveBuffer; set => _receiveBuffer = value; }
        public byte[] SendBuffer { get => _sendBuffer; set => _sendBuffer = value; }

        public Client(Socket handler, byte[] id)
        {
            _handler = handler;
            _id = id;
        }

        // Converts the message stored in the receiveBuffer to a Message object
        public Message ConvertReceiveBufferToMsg()
        {
            int length = 0;
            for (int i = 0; i < _receiveBuffer.Length; i++)
            {
                if (_receiveBuffer[i] == 0)
                    length = i + 1;
            }

            byte[] msg = new byte[length];
            System.Buffer.BlockCopy(_receiveBuffer, 0, msg, 0, length);

            return new Message(msg, length);
        }
    }
}
