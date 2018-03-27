namespace Chatter
{
    public static class Rules
    {
        public const byte UNKNOWN = 0x01; // Unknown rule
        public const byte ID = 0x02; // Server is sending the client's ID to the client
        public const byte MESSAGE = 0x03; // Client/Server is sending a message to other
        public const byte TEST = 0x04; // Client/Server uses this rule to send a test message to see if the connection is still open
    }
}
