using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class VlessConfig : ServerConfig
    {
        private string Encryption { get; set; }
        private VlessPacketEncoding? PacketEncoding { get; set; }
        private VlessType? Type { get; set; }
        private VlessSecurity? Security { get; set; }
        private VlessFingerprint? Fp { get; set; }
        internal string Sni { get; set; }
        private string Pbk { get; set; }
        private string Sid { get; set; }
        private string Flow { get; set; }
        internal string Host { get; set; }
        private string Path { get; set; }
        private string ServiceName { get; set; }
        internal VlessConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("Encryption", StringComparison.OrdinalIgnoreCase))
                {
                    Encryption = parameter.Value;
                }
                else if (parameter.Key.Equals("PacketEncoding", StringComparison.OrdinalIgnoreCase))
                {
                    PacketEncoding = ParsePacketEncoding(parameter.Value);
                }
                else if (parameter.Key.Equals("Type", StringComparison.OrdinalIgnoreCase))
                {
                    Type = ParseType(parameter.Value);
                }
                else if (parameter.Key.Equals("Security", StringComparison.OrdinalIgnoreCase))
                {
                    Security = ParseSecurity(parameter.Value);
                }
                else if (parameter.Key.Equals("Fp", StringComparison.OrdinalIgnoreCase))
                {
                    Fp = ParseFP(parameter.Value);
                }
                else if (parameter.Key.Equals("Sni", StringComparison.OrdinalIgnoreCase))
                {
                    Sni = parameter.Value;
                }
                else if (parameter.Key.Equals("Pbk", StringComparison.OrdinalIgnoreCase))
                {
                    Pbk = parameter.Value;
                }
                else if (parameter.Key.Equals("Sid", StringComparison.OrdinalIgnoreCase))
                {
                    Sid = parameter.Value;
                }
                else if (parameter.Key.Equals("Flow", StringComparison.OrdinalIgnoreCase))
                {
                    Flow = parameter.Value;
                }
                else if (parameter.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    Host = parameter.Value;
                }
                else if (parameter.Key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                {
                    Path = parameter.Value;
                }
                else if (parameter.Key.Equals("serviceName", StringComparison.OrdinalIgnoreCase))
                {
                    ServiceName = parameter.Value;
                }
            }

            if (Encryption?.Equals("none", StringComparison.OrdinalIgnoreCase) == true) Encryption = null;
            if (Type == VlessType.Raw) Type = VlessType.Tcp;

            if (!string.IsNullOrEmpty(Path))
            {
                if (Path.StartsWith("//")) Path = "/" + Path.TrimStart('/');
                if (Path.Equals("/", StringComparison.OrdinalIgnoreCase)) Path = null;
                else if (!Path.StartsWith('/')) Path = "/" + Path;
            }

            if (Path?.Contains("telegram", StringComparison.OrdinalIgnoreCase) == true) throw new BrokenProtocolException();

            switch (Type)
            {
                case null:
                case VlessType.Tcp:
                    if (Security == null) Flow = null;
                    Path = null;
                    ServiceName = null;
                    Host = null;
                    break;
                case VlessType.Xhttp:
                    Flow = null;
                    ServiceName = null;
                    break;
                case VlessType.Ws:
                case VlessType.Httpupgrade:
                    Flow = null;
                    ServiceName = null;
                    break;
                case VlessType.Grpc:
                    Flow = null;
                    Path = null;
                    Host = null;
                    break;
            }

            if (Security == null)
            {
                Pbk = null;
                Sni = null;
                Sid = null;
                Fp = null;
                Flow = null;
            }
            else
            {
                Fp = VlessFingerprint.Firefox;

                if (Security == VlessSecurity.Tls)
                {
                    Pbk = null;
                    Sid = null;
                }
                else if (Security == VlessSecurity.Reality)
                {
                    Host = null;
                    if (string.IsNullOrEmpty(Pbk) || string.IsNullOrEmpty(Sni))
                        throw new BrokenProtocolException();
                }
            }

        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(Encryption)) queryString.Add("encryption", Encryption);
            if (PacketEncoding != null) queryString.Add("packetEncoding", PacketEncoding.ToString().ToLower());
            if (Type != null) queryString.Add("type", Type.ToString().ToLower());
            if (Security != null) queryString.Add("security", Security.ToString().ToLower());
            if (Fp != null) queryString.Add("fp", Fp.ToString().ToLower());
            if (!string.IsNullOrEmpty(Host)) queryString.Add("host", Host);
            if (!string.IsNullOrEmpty(Sni)) queryString.Add("sni", Sni);
            if (!string.IsNullOrEmpty(Pbk)) queryString.Add("pbk", Pbk);
            if (!string.IsNullOrEmpty(Sid)) queryString.Add("sid", Sid);
            if (!string.IsNullOrEmpty(Flow)) queryString.Add("flow", Flow);
            if (!string.IsNullOrEmpty(Path)) queryString.Add("path", Path);
            if (!string.IsNullOrEmpty(ServiceName)) queryString.Add("serviceName", ServiceName);
            return queryString.ToString();
        }
        private VlessPacketEncoding? ParsePacketEncoding(string a1)
        {
            if (!Enum.TryParse(a1, true, out VlessPacketEncoding v1)) return null;
            return v1;
        }
        internal enum VlessPacketEncoding
        {
            None,
            Xudp
        }
        private VlessType? ParseType(string a1)
        {
            if (!Enum.TryParse(a1, true, out VlessType v1)) return null;
            return v1;
        }
        private enum VlessType
        {
            Raw, Tcp,
            Ws,
            Grpc,
            Xhttp,
            Httpupgrade
        }
        private VlessSecurity? ParseSecurity(string a1)
        {
            if (!Enum.TryParse(a1, true, out VlessSecurity v1)) return null;
            return v1;
        }
        private enum VlessSecurity
        {
            Tls,
            Reality
        }
        private VlessFingerprint? ParseFP(string a1)
        {
            if (!Enum.TryParse(a1, true, out VlessFingerprint v1)) return VlessFingerprint.Firefox;
            return v1;
        }
        private enum VlessFingerprint
        {
            Chrome,
            Firefox,
            Edge,
            Safari,
            Ios,
            Android,
            Random
        }
    }
}
