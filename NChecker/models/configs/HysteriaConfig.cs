using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class HysteriaConfig : ServerConfig
    {
        private HysteriaObfuscation? Obfs { get; set; }
        private string ObfsPassword { get; set; }
        internal string Sni { get; set; }
        private int? AllowInsecure { get; set; }
        internal HysteriaConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("Obfs", StringComparison.OrdinalIgnoreCase))
                {
                    Obfs = ParseObfuscation(parameter.Value);
                }
                else if (parameter.Key.Equals("obfs-password", StringComparison.OrdinalIgnoreCase))
                {
                    ObfsPassword = parameter.Value;
                }
                else if (parameter.Key.Equals("Sni", StringComparison.OrdinalIgnoreCase))
                {
                    Sni = parameter.Value;
                }
                else if (parameter.Key.Equals("insecure", StringComparison.OrdinalIgnoreCase))
                {
                    AllowInsecure = ParseAllowInsecure(parameter.Value);
                }
            }
            if (Obfs == HysteriaObfuscation.Salamander && string.IsNullOrEmpty(ObfsPassword)) throw new BrokenProtocolException();
            if (AllowInsecure != null)
            {
                if (Sni?.Contains(".local", StringComparison.OrdinalIgnoreCase) == true)
                {

                }
            }
        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (Obfs != null) queryString.Add("obfs", Obfs.ToString().ToLower());
            if (!string.IsNullOrEmpty(ObfsPassword)) queryString.Add("obfs-password", ObfsPassword);
            if (!string.IsNullOrEmpty(Sni)) queryString.Add("sni", Sni);
            if (AllowInsecure != null) queryString.Add("allowInsecure", AllowInsecure.ToString().ToLower());
            return queryString.ToString();
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
        private int? ParseAllowInsecure(string a1)
        {
            if (int.TryParse(a1, out int i)) return i;
            if (bool.TryParse(a1, out bool b)) return b ? 1 : 0;
            return 0;
        }
    }
}
