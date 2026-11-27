using NChecker.models.configs;
using NChecker.models.errors;

namespace NChecker.models
{
    internal static class ServerConfigFactory
    {
        internal static ServerConfig Create(string a1)
        {
            try
            {
                if (a1.StartsWith("vless", StringComparison.OrdinalIgnoreCase)) return new VlessConfig(a1);
                if (a1.StartsWith("trojan", StringComparison.OrdinalIgnoreCase)) return new TrojanConfig(a1);
                if (a1.StartsWith("hy", StringComparison.OrdinalIgnoreCase)) return new HysteriaConfig(a1);
                if (a1.StartsWith("ss", StringComparison.OrdinalIgnoreCase)) return new ShadowsocksConfig(a1);
                if (a1.StartsWith("vmess", StringComparison.OrdinalIgnoreCase)) return new VmessConfig(a1);
                if (a1.StartsWith("socks4", StringComparison.OrdinalIgnoreCase)) return new Socks4Config(a1);
                if (a1.StartsWith("socks5", StringComparison.OrdinalIgnoreCase)) return new Socks5Config(a1);
                if (a1.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                    //return new HttpsConfig(a1);
                }
                else if (a1.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return new HttpConfig(a1);
            }
            catch (InvalidFormatProtocolException) { }
            catch (BrokenProtocolException) { }
            catch (BlockedTransportException) { }
            catch (Exception exc)
            {
                //Console.WriteLine(exc.Message);
            }
            return null;
        }
    }
}
