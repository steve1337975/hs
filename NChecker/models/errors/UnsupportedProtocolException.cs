namespace NChecker.models.errors
{
    internal class UnsupportedProtocolException : Exception
    {
        public UnsupportedProtocolException() : base() { }
    }
    internal class BlockedTransportException : Exception
    {
        public BlockedTransportException() : base() { }
    }
}
