﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Threading;

namespace Chatter
{
    static class Client
    {
        private static readonly IPAddress _serverIPAdress = IPAddress.Loopback;
        private const int _serverPort = 5005;
        private static Socket _server;
        private static ClientStatus _status;
        private static Thread _connectionThread;
        private static Thread _listeningThread;
        private static byte[] _receiveBuffer = new byte[Server.BufferSize];
        private static byte[] _sendBuffer = new byte[Server.BufferSize];
        private static byte[] _id;
        private static TextBox _chatLog;

        public static ClientStatus Status { get => _status; }

        // Starts the client and connects to the server
        public static void Start(TextBox chatLog)
        {
            _status = ClientStatus.STARTING;
            _chatLog = chatLog;

            _connectionThread = new Thread(new ThreadStart(ConnectLoop));
            _connectionThread.Start();
        }

        // Stops the client and disconnects to the server
        public static void Stop()
        {
            _status = ClientStatus.DISCONNECTED;

            _server.Shutdown(SocketShutdown.Both);
            _server.Disconnect(false);

            if (_connectionThread.IsAlive)
                _connectionThread.Abort();
            if (_listeningThread.IsAlive)
                _listeningThread.Abort();
        }

        // Prints a message to the chat log
        public static void PrintToChatLog(string msg)
        {
            _chatLog.Dispatcher.Invoke(new Action(() => _chatLog.Text += "[" + DateTime.Now.ToLongTimeString() + "] " + msg + Environment.NewLine));
        }

        // Prints a message to the chat log (using "Client:" prefix)
        public static void PrintToChatLogAsClient(string msg)
        {
            _chatLog.Dispatcher.Invoke(new Action(() => _chatLog.Text += "[" + DateTime.Now.ToLongTimeString() + "] Client > " + msg + Environment.NewLine));
        }

        // Sends a message to the server
        public static void SendMessageToServer(byte rule, string data)
        {
            // Construct message
            Message msg = new Message(_id, rule, Encoding.ASCII.GetBytes(data));
            _sendBuffer = msg.Msg;
            _server.Send(_sendBuffer);
        }

        // Tries to connect to the server indefinitely
        private static void ConnectLoop()
        {
            PrintToChatLogAsClient("Starting...");
            _status = ClientStatus.CONNECTING;

            // Try to connect the client to the server
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int retryDelay = 0;
            while (!_server.Connected)
            {
                try
                {
                    // Try to connect
                    _server.Connect(_serverIPAdress, _serverPort);

                    // Get ID from server
                    Message msgReceived = ReceiveMessageFromServer();
                    _id = msgReceived.Data;
                    PrintToChatLogAsClient("ID recieved from server: " + System.Text.Encoding.ASCII.GetString(_id));

                    // Client is now connected to server
                    _status = ClientStatus.CONNECTED;
                    PrintToChatLogAsClient("Successfully connected to server!");

                    // Start listening thread
                    _listeningThread = new Thread(new ThreadStart(ListenLoop));
                    _listeningThread.Start();
                }
                catch (SocketException)
                {
                    retryDelay += 5000;
                    if (retryDelay > 30000) retryDelay = 30000;

                    PrintToChatLogAsClient("Failed to connect to server! Retrying in " + retryDelay / 1000 + " seconds...");
                    Thread.Sleep(retryDelay);
                }
            }
        }

        // Recieves & returns a message from the server
        private static Message ReceiveMessageFromServer()
        {
            try
            {
                int bytesReceived = _server.Receive(_receiveBuffer);
                if (bytesReceived == 0)
                    return null;
                else
                    return new Message(_receiveBuffer, bytesReceived);
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case 10054:
                        PrintToChatLogAsClient("You have been disconnected from the server! Please reboot the client");
                        Stop();
                        break;
                    case 10053:
                        PrintToChatLogAsClient("Server connection was aborted! Please reboot the client");
                        Stop();
                        break;
                    default:
                        throw e;
                }
            }

            return null;
        }

        // Recieves messages from the server and displays to the chat log
        private static void ListenLoop()
        {
            while (_status == ClientStatus.CONNECTED)
            {
                Message receivedMessage = ReceiveMessageFromServer();
                if (receivedMessage != null)
                    PrintToChatLog(receivedMessage.SID + " > " + receivedMessage.SData);
            }
        }
    }
}
