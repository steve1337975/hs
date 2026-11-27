using NChecker.models;

namespace NChecker.classes
{
    internal class SubscriptionsManager
    {
        private List<SubscriptionData> subscriptions = new List<SubscriptionData>();
        internal SubscriptionsManager()
        {
            string raw = Properties.Resources.bl;
            string[] urls = raw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            foreach (string v1 in urls)
            {
                if (v1.StartsWith("#")) continue;
                if (v1.StartsWith("-")) break;
                subscriptions.Add(new SubscriptionData(v1));
            }
            subscriptions = subscriptions.DistinctBy(x => x.Url).ToList();
        }
        internal SubscriptionData[] Subscriptions => subscriptions.ToArray();
        internal ServerData[] AllServers => subscriptions.SelectMany(sub => sub.Servers).DistinctBy(x => x.ConfigData.GetCoreSignature()).ToArray();
    }
}
