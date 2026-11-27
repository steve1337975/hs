using NChecker.models.errors;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NChecker.models.configs
{
    internal class ServerConfig
    {
        internal string Url { get; set; }
        internal string Protocol { get; set; }
        internal string Address { get; set; }
        internal int Port { get; set; }
        internal string UUID { get; set; }
        internal string Name { get; set; }
        internal string Domain { get; set; }
        protected List<KeyValuePair<string, string>> QueryParameters { get; private set; } = new List<KeyValuePair<string, string>>();
        internal ServerConfig(string a1)
        {
            Url = a1;
            ParseServer();
        }
        internal virtual void ParseServer()
        {
            if (Url.StartsWith("vmess", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string json = Encoding.UTF8.GetString(Convert.FromBase64String(Url[8..]));
                    VmessJsonModel vmessJsonModel = JsonConvert.DeserializeObject<VmessJsonModel>(json);
                    if (vmessJsonModel != null)
                    {
                        Protocol = "vmess";
                        Address = vmessJsonModel.Address;
                        Port = Convert.ToInt32(vmessJsonModel.Port);
                        UUID = vmessJsonModel.Id;
                        Name = vmessJsonModel.Remarks;
                    }
                }
                catch (FormatException exc)
                {
                    throw new InvalidFormatProtocolException(exc.Message);
                }
            }

            if (Url.StartsWith("hy"))
            {
                int commaIndex = Url.IndexOf(',');
                if (commaIndex != -1)
                {
                    int slashIndex = Url.IndexOf('/', commaIndex);
                    if (slashIndex != -1) Url = Url.Remove(commaIndex, slashIndex - commaIndex);
                }
            }
            if (Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            {
                Protocol = uri.Scheme;
                Address = uri.Host;
                Port = uri.Port;
                UUID = Uri.UnescapeDataString(uri.UserInfo);
                QueryParameters = ParseQuery(uri.Query);
                Name = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
            }

            try
            {
                if (Protocol.StartsWith("ss", StringComparison.OrdinalIgnoreCase))
                {
                    UUID = Encoding.UTF8.GetString(Convert.FromBase64String(UUID));
                }
            }
            catch (FormatException exc)
            {
                throw new InvalidFormatProtocolException(exc.Message);
            }

            if (UUID.Contains("telegram", StringComparison.OrdinalIgnoreCase) || UUID.Contains("://", StringComparison.OrdinalIgnoreCase) || UUID.Contains("www", StringComparison.OrdinalIgnoreCase))
            {
                throw new BrokenProtocolException();
            }
            Name = "Anti-RKN";
        }
        internal virtual string GetNewUrl()
        {
            if (Protocol.Equals("vmess", StringComparison.OrdinalIgnoreCase))
            {
                return "vmess://" + GetUriQuery();
            }
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = Protocol.ToLower();
            if (Protocol.Equals("ss", StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.UserName = Convert.ToBase64String(Encoding.UTF8.GetBytes(UUID));
            }
            else
            {
                uriBuilder.UserName = UUID;
            }
            uriBuilder.Host = Domain ?? Address;
            uriBuilder.Port = Port;
            uriBuilder.Fragment = Name;
            uriBuilder.Query = GetUriQuery();

            string url = uriBuilder.ToString();
            int queryIndex = url.LastIndexOf("/");
            if (queryIndex != -1) url = url.Remove(queryIndex, 1);
            return url;
        }
        internal virtual string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            return queryString.ToString();
        }
        internal string GetCoreSignature()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(GetNewUrl());
            byte[] hash = MD5.HashData(bytes);
            return Convert.ToHexString(hash);
        }
        protected string NormalizeId(string rawId)
        {
            if (Guid.TryParse(rawId.Trim(), out var guid))
            {
                return guid.ToString("D");
            }
            return rawId.Trim();
        }
        private List<KeyValuePair<string, string>> ParseQuery(string query)
        {
            var result = new List<KeyValuePair<string, string>>();
            foreach (var part in query.TrimStart('?')
                                      .Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2);

                string key = DecodeQueryComponent(pair[0]);
                string value = pair.Length == 2
                    ? DecodeQueryComponent(pair[1])
                    : "";

                result.Add(new KeyValuePair<string, string>(key, value));
            }
            return result;
        }
        private string DecodeQueryComponent(string value) => Uri.UnescapeDataString(value.Replace("+", " "));
    }
}
