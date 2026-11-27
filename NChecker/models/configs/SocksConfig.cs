using NChecker.models.errors;

namespace NChecker.models.configs
{
    internal class SocksConfig : ServerConfig
    {
        private string Username { get; set; }
        private string Password { get; set; }
        internal SocksConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            if (!string.IsNullOrEmpty(UUID))
            {
                string[] parts = UUID.Split(':');
                if (parts.Length > 0)
                {
                    Username = parts[0];
                    Password = parts[1];
                    if (Username.Equals("null", StringComparison.OrdinalIgnoreCase) || Password.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BrokenProtocolException();
                    }
                }
            }
        }
    }
}
