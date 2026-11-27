using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class TrojanConfig : ServerConfig
    {
        private TrojanType? Type { get; set; }
        private TrojanSecurity? Security { get; set; }
        private TrojanFingerprint? Fp { get; set; }
        internal string Sni { get; set; }
        internal string Host { get; set; }
        private string Path { get; set; }
        private string ServiceName { get; set; }
        internal TrojanConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("Type", StringComparison.OrdinalIgnoreCase))
                {
                    Type = ParseType(parameter.Value);
                }
                else if (parameter.Key.Equals("Security", StringComparison.OrdinalIgnoreCase))
                {
                    Security = ParseSecurity(parameter.Value);
                }
                else if (parameter.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    Host = parameter.Value;
                }
                else if (parameter.Key.Equals("Sni", StringComparison.OrdinalIgnoreCase))
                {
                    Sni = parameter.Value;
                }
                else if (parameter.Key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                {
                    Path = parameter.Value;
                }
                else if (parameter.Key.Equals("serviceName", StringComparison.OrdinalIgnoreCase))
                {
                    ServiceName = parameter.Value;
                }
                else if (parameter.Key.Equals("Fp", StringComparison.OrdinalIgnoreCase))
                {
                    Fp = ParseFP(parameter.Value);
                }
            }


            if (Type == TrojanType.Raw || Type == TrojanType.Tcp) Type = null;
            if (Security == null) Security = TrojanSecurity.Tls;

            if (Type == TrojanType.Ws || Type == TrojanType.Httpupgrade || Type == TrojanType.Xhttp)
            {
                if (string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Sni)) Host = Sni;

                if (!string.IsNullOrEmpty(Path))
                {
                    if (Path.StartsWith("//")) Path = "/" + Path.TrimStart('/');
                    if (Path.Equals("/", StringComparison.OrdinalIgnoreCase)) Path = null;
                    else if (!Path.StartsWith('/')) Path = "/" + Path;
                }
            }
            else
            {
                Path = null;
                Host = null;
            }

            if (Type != TrojanType.Grpc) ServiceName = null;
            if (Path?.Contains("telegram", StringComparison.OrdinalIgnoreCase) == true) throw new BrokenProtocolException();

            Fp = TrojanFingerprint.Firefox;
        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (Type != null) queryString.Add("type", Type.ToString().ToLower());
            if (Security != null) queryString.Add("security", Security.ToString().ToLower());
            if (Fp != null) queryString.Add("fp", Fp.ToString().ToLower());
            if (!string.IsNullOrEmpty(Sni)) queryString.Add("sni", Sni);
            if (!string.IsNullOrEmpty(Host)) queryString.Add("host", Host);
            if (!string.IsNullOrEmpty(Path)) queryString.Add("path", Path);
            if (!string.IsNullOrEmpty(ServiceName)) queryString.Add("serviceName", ServiceName);
            return queryString.ToString();
        }
        private TrojanType? ParseType(string a1)
        {
            if (!Enum.TryParse(a1, true, out TrojanType v1)) return null;
            return v1;
        }
        private enum TrojanType
        {
            Raw, Tcp,
            Ws,
            Grpc,
            Xhttp,
            Httpupgrade
        }
        private TrojanSecurity? ParseSecurity(string a1)
        {
            if (!Enum.TryParse(a1, true, out TrojanSecurity v1)) return null;
            return v1;
        }
        private enum TrojanSecurity
        {
            Tls
        }
        private TrojanFingerprint? ParseFP(string a1)
        {
            if (!Enum.TryParse(a1, true, out TrojanFingerprint v1)) return TrojanFingerprint.Firefox;
            return v1;
        }
        private enum TrojanFingerprint
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
