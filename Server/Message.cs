using System;
using System.Text;

namespace Chatter
{
    ///<summary>
    /// Represents the data that the server uses to transmit and receive data to & from clients
    /// A message being sent or recieved by the server must be in the following format:
    ///     |ID||RULE||DATA|
    ///     where ID is either the client ID or the ID of the server
    ///     Rule is given by the static Rule class
    ///</summary>
    public class Message
    {
        private byte[] _id;
        private byte _rule;
        private byte[] _data;

        public byte[] ID { get => _id; }
        public byte Rule { get => _rule; }
        public byte[] Data { get => _data; }
        public byte[] Msg { get => CreateMsg(); }
        public string SID { get => Encoding.ASCII.GetString(_id); }
        public string SRule { get => _rule.ToString(); }
        public string SData { get => Encoding.ASCII.GetString(_data); }
        public string SMsg { get => ToString(); }

        // Constructs a message through its components
        public Message(byte[] id, byte rule, byte[] data)
        {
            _id = id;
            _rule = rule;
            _data = data;
        }

        // Deconstructs a message into its components
        public Message(byte[] msg, int length)
        {
            byte[] temp = new byte[length];
            Buffer.BlockCopy(msg, 0, temp, 0, length);
            string message = Encoding.ASCII.GetString(temp);

            int idLastDelimiterIndex = message.IndexOf("|", 1);
            int ruleLastDelimiterIndex = message.IndexOf("|", idLastDelimiterIndex + 2);
            int dataLastDelimiterIndex = message.IndexOf("|", ruleLastDelimiterIndex + 2);

            string id = message.Substring(1, idLastDelimiterIndex - 1);
            string rule = message.Substring(idLastDelimiterIndex + 2, ruleLastDelimiterIndex - idLastDelimiterIndex - 2);
            string data = message.Substring(ruleLastDelimiterIndex + 2, dataLastDelimiterIndex - ruleLastDelimiterIndex - 2);

            _id = Encoding.ASCII.GetBytes(id);
            _rule = Byte.Parse(rule);
            _data = Encoding.ASCII.GetBytes(data);
        }

        // Creates the msg from the already set id, rule and data
        private byte[] CreateMsg()
        {
            string msg = "|" + Encoding.ASCII.GetString(_id) + "|"
                       + "|" + _rule + "|"
                       + "|" + Encoding.ASCII.GetString(_data) + "|";

            return Encoding.ASCII.GetBytes(msg);
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Msg);
        }
    }
}
