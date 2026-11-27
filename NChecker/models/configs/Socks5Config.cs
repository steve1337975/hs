namespace NChecker.models.configs
{
    internal class Socks5Config : ServerConfig
    {
        private string Username { get; set; }
        private string Password { get; set; }
        internal Socks5Config(string a1) : base(a1) { }
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
                }
            }
        }
    }
}
