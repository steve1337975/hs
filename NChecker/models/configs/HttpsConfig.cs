using NChecker.models.errors;
using System.Collections.Specialized;
using System.Web;

namespace NChecker.models.configs
{
    internal class HttpsConfig : ServerConfig
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private string Security { get; set; }
        internal HttpsConfig(string a1) : base(a1) { }
        internal override void ParseServer()
        {
            base.ParseServer();
            if (!string.IsNullOrEmpty(UUID))
            {
                string[] parts = UUID.Split(':');
                if (parts.Length > 0)
                {
                    Username = parts[0];
                    Password = parts[1];
                    if (Username.Equals("null", StringComparison.OrdinalIgnoreCase) || Password.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BrokenProtocolException();
                    }
                }
            }
            foreach (var parameter in QueryParameters)
            {
                if (parameter.Key.Equals("Security", StringComparison.OrdinalIgnoreCase))
                {
                    Security = parameter.Value;
                }
            }
        }
        internal override string GetUriQuery()
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(Security)) queryString.Add("security", Security);
            return queryString.ToString();
        }
    }
}
