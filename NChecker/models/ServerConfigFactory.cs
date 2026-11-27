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
                if (a1.StartsWith("vmess", StringComparison.OrdinalIgnoreCase)) return new VmessConfig(a1);
                if (a1.StartsWith("trojan", StringComparison.OrdinalIgnoreCase)) return new TrojanConfig(a1);
                if (a1.StartsWith("hy", StringComparison.OrdinalIgnoreCase)) return new HysteriaConfig(a1);
                if (a1.StartsWith("ss", StringComparison.OrdinalIgnoreCase)) return new ShadowsocksConfig(a1);
                if (a1.StartsWith("socks", StringComparison.OrdinalIgnoreCase)) return new SocksConfig(a1);
                if (a1.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return new HttpsConfig(a1);
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
