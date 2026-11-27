using NChecker.models.configs;
using NChecker.models.errors;

namespace NChecker.models
{
    internal class ServerData
    {
        internal string Url { get; private set; }
        internal string SubLastPath { get; set; }
        internal ServerConfig ConfigData { get; set; }
        internal string Country { get; set; }
        internal string CountryCode { get; set; }
        internal string City { get; set; }
        internal bool IsAlive { get; set; }
        internal ServerData(string a1, string a2)
        {
            Url = a1.Trim();
            SubLastPath = a2 ?? string.Empty;
            if (!Url.Contains("://"))
            {
                if (SubLastPath.Contains("socks4", StringComparison.OrdinalIgnoreCase))
                    Url = $"socks4://{Url}";
                else if (SubLastPath.Contains("socks5", StringComparison.OrdinalIgnoreCase))
                    Url = $"socks5://{Url}";
                else if (SubLastPath.Contains("https", StringComparison.OrdinalIgnoreCase))
                    Url = $"https://{Url}";
                else if (SubLastPath.Contains("http", StringComparison.OrdinalIgnoreCase))
                    Url = $"http://{Url}";
            }
            ConfigData = ServerConfigFactory.Create(Url);
            if (ConfigData == null) throw new UnsupportedProtocolException();
        }
        internal bool IsValid
        {
            get
            {
                if (ConfigData == null || ConfigData?.Address == null || ConfigData.Port == 0) return false;
                return true;
            }
        }
    }
}
