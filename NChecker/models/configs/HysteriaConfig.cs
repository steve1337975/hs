using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class HysteriaConfig : ServerConfig
    {
        private HysteriaSecurity? Security { get; set; }
        private HysteriaObfuscation? Obfs { get; set; }
        private string ObfsPassword { get; set; }
        internal string Sni { get; set; }
        private int? Insecure { get; set; }
        private string Auth { get; set; }
        private int? UpMbps { get; set; }
        private int? DownMbps { get; set; }
        private string Alpn { get; set; }
        private HysteriaFingerprint? Fp { get; set; }
        internal HysteriaConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("security", StringComparison.OrdinalIgnoreCase))
                    Security = ParseSecurity(parameter.Value);
                else if (parameter.Key.Equals("obfs", StringComparison.OrdinalIgnoreCase))
                    Obfs = ParseObfuscation(parameter.Value);
                else if (parameter.Key.Equals("obfs-password", StringComparison.OrdinalIgnoreCase))
                    ObfsPassword = parameter.Value;
                else if (parameter.Key.Equals("sni", StringComparison.OrdinalIgnoreCase))
                    Sni = parameter.Value;
                else if (parameter.Key.Contains("insecure", StringComparison.OrdinalIgnoreCase))
                    Insecure = ParseInsecure(parameter.Value);
                else if (parameter.Key.Equals("auth", StringComparison.OrdinalIgnoreCase))
                    Auth = parameter.Value;
                else if (parameter.Key.Equals("upmbps", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parameter.Value, out var i)) UpMbps = i;
                }
                else if (parameter.Key.Equals("downmbps", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parameter.Value, out var i)) DownMbps = i;
                }
                else if (parameter.Key.Equals("alpn", StringComparison.OrdinalIgnoreCase))
                    Alpn = parameter.Value;
                else if (parameter.Key.Equals("fp", StringComparison.OrdinalIgnoreCase))
                    Fp = ParseFP(parameter.Value);
            }
        }
        internal override void NormalizeParams()
        {
            base.NormalizeParams();

            if (Protocol.Equals("hysteria", StringComparison.OrdinalIgnoreCase))
            {
                //Console.WriteLine(Url);
            }
            if (Security == null) Security = HysteriaSecurity.Tls;
            if (Obfs == HysteriaObfuscation.Salamander && string.IsNullOrEmpty(ObfsPassword)) throw new BrokenProtocolException();


            /*
            if (AllowInsecure == 0) AllowInsecure = null;
            if (AllowInsecure != null)
            {
                if (Sni?.Contains(".local", StringComparison.OrdinalIgnoreCase) == true)
                {

                }
            }
            */
            Fp = HysteriaFingerprint.Firefox;
        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (Security != null) queryString.Add("security", Security.ToString().ToLower());
            if (Obfs != null) queryString.Add("obfs", Obfs.ToString().ToLower());
            if (!string.IsNullOrEmpty(ObfsPassword)) queryString.Add("obfs-password", ObfsPassword);
            if (!string.IsNullOrEmpty(Sni)) queryString.Add("sni", Sni);
            if (Insecure != null) queryString.Add("allowInsecure", Insecure.ToString().ToLower());
            if (!string.IsNullOrEmpty(Auth)) queryString.Add("auth", Auth);
            if (UpMbps != null) queryString.Add("upmbps", UpMbps.ToString().ToString());
            if (DownMbps != null) queryString.Add("downmbps", DownMbps.ToString().ToString());
            if (Alpn != null) queryString.Add("alpn", Alpn.ToString().ToString());
            if (Fp != null) queryString.Add("fp", Fp.ToString().ToLower());
            return queryString.ToString();
        }
        private HysteriaSecurity? ParseSecurity(string a1)
        {
            if (!Enum.TryParse(a1, true, out HysteriaSecurity v1)) return null;
            return v1;
        }
        private enum HysteriaSecurity
        {
            Tls,
            Reality
        }
        private HysteriaObfuscation? ParseObfuscation(string a1)
        {
            if (!Enum.TryParse(a1, true, out HysteriaObfuscation v1)) return null;
            return v1;
        }
        private enum HysteriaObfuscation
        {
            Salamander
        }
        private int? ParseInsecure(string a1)
        {
            if (int.TryParse(a1, out int i)) return i;
            if (bool.TryParse(a1, out bool b)) return b ? 1 : 0;
            return 0;
        }
        private HysteriaFingerprint? ParseFP(string a1)
        {
            if (!Enum.TryParse(a1, true, out HysteriaFingerprint v1)) return HysteriaFingerprint.Firefox;
            return v1;
        }
        private enum HysteriaFingerprint
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
