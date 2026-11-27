namespace NChecker.models
{
    internal class SubscriptionData
    {
        private List<ServerData> servers = new List<ServerData>();
        internal string Url { get; private set; }
        internal string LastPath { get; set; }
        internal SubscriptionData(string a1)
        {
            Url = a1;
            LastPath = new Uri(Url).AbsolutePath.TrimEnd('/').Split('/').Last();
        }
        internal void AddServer(string a1) => servers.Add(new ServerData(a1, LastPath));
        internal ServerData[] Servers => servers.Where(x => x.IsValid && x.ConfigData != null).DistinctBy(x => x.ConfigData.GetCoreSignature()).ToArray();
    }
}
