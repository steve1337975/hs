using NChecker.models.errors;
using Newtonsoft.Json;
using System.Text;

namespace NChecker.models.configs
{
    internal class VmessConfig : ServerConfig
    {
        private string Version { get; set; }
        private VmessType? Type { get; set; }
        private VmessSecurity? Security { get; set; }
        private VmessNet? Net { get; set; }
        internal string Host { get; set; }
        internal string Sni { get; set; }
        private string Tls { get; set; }
        private string Path { get; set; }
        private VmessFingerprint? Fp { get; set; }
        internal VmessConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            try
            {
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(Url[8..]));
                VmessJsonModel vmessJsonModel = JsonConvert.DeserializeObject<VmessJsonModel>(json);
                if (vmessJsonModel != null)
                {
                    Version = vmessJsonModel.Version ?? "2";
                    Type = ParseType(vmessJsonModel.Type);
                    Security = ParseSecurity(vmessJsonModel.Security);
                    Net = ParseNet(vmessJsonModel.Net);
                    Host = vmessJsonModel.Host;
                    Sni = vmessJsonModel.Sni;
                    Tls = vmessJsonModel.Tls;
                    Path = vmessJsonModel.Path;
                    Fp = ParseFP(vmessJsonModel.Fingerprint);
                }

                if (Type == VmessType.None) Type = null;
                if (Path?.Equals("/", StringComparison.OrdinalIgnoreCase) == true) Path = null;
                if (Net == VmessNet.Tcp)
                {
                    if (Type != VmessType.Http)
                    {
                        Host = null;
                        Path = null;
                    }
                    Net = null;
                }
                else if (Net == VmessNet.Ws || Net == VmessNet.H2 || Net == VmessNet.Httpupgrade)
                {
                    Type = null;
                }
                else if (Net == VmessNet.Grpc)
                {
                    Host = null;
                    Type = null;
                }
                else if (Net == VmessNet.Kcp || Net == VmessNet.Quic)
                {
                    throw new BlockedTransportException();
                }
                if (Tls?.Equals("tls", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Fp = VmessFingerprint.Firefox;
                }
                else
                {
                    Tls = null;
                    Sni = null;
                    Fp = null;
                }
            }
            catch (FormatException exc)
            {
                throw new InvalidFormatProtocolException(exc.Message);
            }
        }
        internal override string GetUriQuery()
        {
            VmessJsonModel vmessJsonModel = new VmessJsonModel
            {
                Version = Version,
                Id = NormalizeId(UUID),
                Address = Domain ?? Address,
                Port = Port.ToString(),
                Remarks = Name
            };
            if (Type != null) vmessJsonModel.Type = Type.ToString().ToLower();
            if (Security != null) vmessJsonModel.Security = Security.ToString().ToLower();
            if (Net != null) vmessJsonModel.Net = Net.ToString().ToLower();
            if (Fp != null) vmessJsonModel.Fingerprint = Fp.ToString().ToLower();
            if (!string.IsNullOrEmpty(Sni)) vmessJsonModel.Sni = Sni;
            if (!string.IsNullOrEmpty(Host)) vmessJsonModel.Host = Host;
            if (!string.IsNullOrEmpty(Tls)) vmessJsonModel.Tls = Tls;
            if (!string.IsNullOrEmpty(Path)) vmessJsonModel.Path = Path;

            string json = JsonConvert.SerializeObject(vmessJsonModel);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
        private VmessSecurity? ParseSecurity(string a1)
        {
            if (!Enum.TryParse(a1, true, out VmessSecurity v1)) return null;
            return v1;
        }
        private enum VmessSecurity
        {
            None,
            Auto
        }
        private VmessNet? ParseNet(string a1)
        {
            if (!Enum.TryParse(a1, true, out VmessNet v1)) return null;
            return v1;
        }
        private enum VmessNet
        {
            Tcp,
            Ws,
            Grpc,
            H2,
            Httpupgrade,
            Kcp, Quic
        }
        private VmessType? ParseType(string a1)
        {
            if (!Enum.TryParse(a1, true, out VmessType v1)) return null;
            return v1;
            //return v1 == VmessType.WechatVideo ? "wechat-video" : v1.ToString().ToLower();
        }

        private enum VmessType
        {
            None,           // "none" — по умолчанию
            Http,           // "http" — маскировка TCP под HTTP
            Srtp,           // "srtp" — видеозвонки (kcp / quic)
            Utp,            // "utp" — торрент-трафик (kcp / quic)
            WechatVideo,    // "wechat-video" — маскировка под WeChat (kcp / quic)
            Dtls,           // "dtls" — DTLS 1.2 (kcp / quic)
            Wireguard       // "wireguard" — WireGuard (kcp / quic)
        }


        private VmessFingerprint? ParseFP(string a1)
        {
            if (!Enum.TryParse(a1, true, out VmessFingerprint v1)) return VmessFingerprint.Firefox;
            return v1;
        }
        private enum VmessFingerprint
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
