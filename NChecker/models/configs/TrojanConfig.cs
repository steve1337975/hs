using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class TrojanConfig : ServerConfig
    {
        private TrojanType? Type { get; set; }
        private TrojanSecurity? Security { get; set; }
        internal string Sni { get; set; }
        internal string Host { get; set; }
        private string Path { get; set; }
        private string ServiceName { get; set; }
        private int? AllowInsecure { get; set; }
        private string HeaderType { get; set; }
        private string Pbk { get; set; }
        private string Sid { get; set; }
        private TrojanFingerprint? Fp { get; set; }
        internal TrojanConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("type", StringComparison.OrdinalIgnoreCase))
                    Type = ParseType(parameter.Value);
                else if (parameter.Key.Equals("security", StringComparison.OrdinalIgnoreCase))
                    Security = ParseSecurity(parameter.Value);
                else if (parameter.Key.Equals("host", StringComparison.OrdinalIgnoreCase))
                    Host = parameter.Value;
                else if (parameter.Key.Equals("sni", StringComparison.OrdinalIgnoreCase))
                    Sni = parameter.Value;
                else if (parameter.Key.Equals("pbk", StringComparison.OrdinalIgnoreCase))
                    Pbk = parameter.Value;
                else if (parameter.Key.Equals("sid", StringComparison.OrdinalIgnoreCase))
                    Sid = parameter.Value;
                else if (parameter.Key.Equals("path", StringComparison.OrdinalIgnoreCase))
                    Path = parameter.Value;
                else if (parameter.Key.Equals("serviceName", StringComparison.OrdinalIgnoreCase))
                    ServiceName = parameter.Value;
                else if (parameter.Key.Equals("allowInsecure", StringComparison.OrdinalIgnoreCase) || parameter.Key.Equals("insecure", StringComparison.OrdinalIgnoreCase))
                    AllowInsecure = ParseAllowInsecure(parameter.Value);
                else if (parameter.Key.Equals("headerType", StringComparison.OrdinalIgnoreCase))
                    HeaderType = parameter.Value;
                else if (parameter.Key.Equals("fp", StringComparison.OrdinalIgnoreCase))
                    Fp = ParseFP(parameter.Value);
            }
        }
        internal override void NormalizeParams()
        {
            base.NormalizeParams();
            if (Type == TrojanType.Raw || Type == TrojanType.Tcp) Type = null;
            if (Type == TrojanType.Ws || Type == TrojanType.Httpupgrade || Type == TrojanType.Http) // XHttp
            {
                /*
                if (string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Sni)) Host = Sni;
                if (!string.IsNullOrEmpty(Path))
                {
                    if (Path.StartsWith("//")) Path = "/" + Path.TrimStart('/');
                    if (Path.Equals("/", StringComparison.OrdinalIgnoreCase)) Path = null;
                    else if (!Path.StartsWith('/')) Path = "/" + Path;
                }
                */
            }
            else
            {
                Path = null;
                Host = null;
            }
            if (Type != TrojanType.Grpc) ServiceName = null;
            if (Security == null) Security = TrojanSecurity.Tls;
            if (Host?.Contains("...", StringComparison.OrdinalIgnoreCase) == true
                || Sni?.Contains("...", StringComparison.OrdinalIgnoreCase) == true
                || UUID.Contains('@'))
            {
                throw new BrokenProtocolException();
            }
            Fp = TrojanFingerprint.Firefox;
        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (Type != null) queryString.Add("type", Type.ToString().ToLower());
            if (Security != null) queryString.Add("security", Security.ToString().ToLower());
            if (!string.IsNullOrEmpty(Sni)) queryString.Add("sni", Sni);
            if (!string.IsNullOrEmpty(Pbk)) queryString.Add("pbk", Pbk);
            if (!string.IsNullOrEmpty(Sid)) queryString.Add("sid", Sid);
            if (!string.IsNullOrEmpty(Host)) queryString.Add("host", Host);
            if (!string.IsNullOrEmpty(Path)) queryString.Add("path", Path);
            if (!string.IsNullOrEmpty(ServiceName)) queryString.Add("serviceName", ServiceName);
            if (AllowInsecure != null) queryString.Add("allowInsecure", AllowInsecure.ToString().ToLower());
            if (!string.IsNullOrEmpty(HeaderType)) queryString.Add("headerType", HeaderType);
            if (Fp != null) queryString.Add("fp", Fp.ToString().ToLower());
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
            Http,
            Httpupgrade
        }
        private TrojanSecurity? ParseSecurity(string a1)
        {
            if (!Enum.TryParse(a1, true, out TrojanSecurity v1)) return null;
            return v1;
        }
        private enum TrojanSecurity
        {
            Tls,
            Reality
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
        private int? ParseAllowInsecure(string a1)
        {
            if (int.TryParse(a1, out int i)) return i;
            if (bool.TryParse(a1, out bool b)) return b ? 1 : 0;
            return 0;
        }
    }
}
