using NChecker.models.errors;

namespace NChecker.models.configs
{
    internal class ShadowsocksConfig : ServerConfig
    {
        private string Method { get; set; }
        private string Password { get; set; }
        internal ShadowsocksConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            if (!string.IsNullOrEmpty(UUID))
            {
                string[] parts = UUID.Split(":");
                if (parts.Length > 0)
                {
                    Method = parts[0];
                    Password = parts[1];
                    if (Method.Equals("null", StringComparison.OrdinalIgnoreCase) || Password.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BrokenProtocolException();
                    }
                    if (Method.Equals("chacha20-poly1305", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BrokenProtocolException();
                    }
                }
            }
        }
    }
}
