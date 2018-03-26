using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace Chatter
{
    public static class Server
    {
        public static readonly byte[] ServerID = Encoding.ASCII.GetBytes("Server");
        public const int ListenerPort = 5005;
        public const int BufferSize = 1024;
        private static Socket _listener;
        private static List<Client> _clients = new List<Client>();
        private static TextBox _serverLog;

        // Starts the server
        public static void Start(TextBox serverLog)
        {
            // Hook the server log output to the server
            _serverLog = serverLog;

            PrintToServerLog("Starting...");

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, ListenerPort));
            _listener.Listen(1);

            // Allow the server to recieve connections
            _listener.BeginAccept(new AsyncCallback(AcceptCallback), null);

            PrintToServerLog("Successfully started!");
        }

        // Stops the server
        public static void Stop()
        {
            if (_listener.Connected)
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Close();
            }
            PrintToServerLog("Server shutdown!");
        }

        // Prints a message to the server log
        public static void PrintToServerLog(string msg)
        {
            string output = "[" + DateTime.Now.ToLongTimeString() + "]" + " Server > " + msg + Environment.NewLine;
            _serverLog.Dispatcher.Invoke(new Action(() => _serverLog.Text += output));
        }

        // Handles new clients connecting
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket handler = _listener.EndAccept(ar); // Handler = Handles client's connection

            // Generate a new ID for the client
            string id = Guid.NewGuid().ToString();
            // Add a new client to the server list
            Client client = new Client(handler, Encoding.ASCII.GetBytes(id));
            _clients.Add(client);

            // Acknowledge a new client connected
            PrintToServerLog("A new client connected! ID: " + Encoding.ASCII.GetString(client.ID));

            // Send the ID to the client
            SendMsgToClient(client, new Message(ServerID, Rules.ID, client.ID));

            // Allow client to send data to the server
            client.Handler.BeginReceive(client.ReceiveBuffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            // Allow the server to accept a new connection
            _listener.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Sends a message to the client 
        private static void SendMsgToClient(Client client, Message message)
        {
            client.SendBuffer = message.Msg;
            client.Handler.BeginSend(client.SendBuffer, 0, client.SendBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }

        // Handles sending messages to the client
        private static void SendCallback(IAsyncResult ar)
        {
            Client client = ar.AsyncState as Client;
            client.Handler.EndSend(ar);
        }

        // Handles messages coming from the client
        private static void ReceiveCallback(IAsyncResult ar)
        {
            Client client = ar.AsyncState as Client;
            int bytesReceived = client.Handler.EndReceive(ar);

            if (bytesReceived > 0)
            {
                Message msgReceived = client.ConvertReceiveBufferToMsg();

                // Check client ID and sent ID matches
                if (!client.ID.SequenceEqual(msgReceived.ID))
                {
                    DisconnectClient(client);
                    return;
                }

                PrintToServerLog(msgReceived.ToString());

                // Allow the client to send another message
                client.Handler.BeginReceive(client.ReceiveBuffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            else
            {
                DisconnectClient(client);
            }
        }

        // Disconnects a client from the server
        private static void DisconnectClient(Client client)
        {
            string clientID = Encoding.ASCII.GetString(client.ID);

            client.Handler.Shutdown(SocketShutdown.Both);
            client.Handler.Disconnect(false);
            _clients.Remove(client);

            PrintToServerLog("Client " + clientID + " disconnected/kicked!");
        }
    }
}
