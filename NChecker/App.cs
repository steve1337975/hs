using NChecker.classes;
using System.Collections.Concurrent;

namespace NChecker
{
    internal class App
    {
        internal static App Instance { get; private set; } = new App();
        private SubscriptionsManager subscriptionsManager = new SubscriptionsManager();
        private NetworkManager networkManager = new NetworkManager();
        private ExportManager exportManager = new ExportManager();
        internal async Task Start()
        {
            bool isGitHub = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
            int maxThreads = isGitHub ? 250 : 500;
            ThreadPool.SetMinThreads(maxThreads + 100, maxThreads + 100);

            await networkManager.Start();
            await networkManager.FillSubs(subscriptionsManager.Subscriptions);

            var servers = subscriptionsManager.AllServers;
            Console.WriteLine($"Загружено и отфильтровано серверов для проверки: {servers.Length} (Потоков: {maxThreads})");
            await Parallel.ForEachAsync(servers, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, async (server, ct) =>
            {
                await networkManager.TestResolveIP(server);
            });

            servers = servers.Where(x => x.IsValid).DistinctBy(x => x.ConfigData.GetCoreSignature()).ToArray();

            /*
            servers = servers.OrderBy(x =>
            {
                if (x.ConfigData is VlessConfig vlessConfig)
                {
                    return vlessConfig.Sni ?? vlessConfig.Host ?? vlessConfig.Address;
                }
                else if (x.ConfigData is TrojanConfig trojanConfig)
                {
                    return trojanConfig.Sni ?? trojanConfig.Host ?? trojanConfig.Address;
                }
                else if (x.ConfigData is HysteriaConfig hysteriaConfig)
                {
                    return hysteriaConfig.Sni ?? hysteriaConfig.Address;
                }
                else if (x.ConfigData is VmessConfig vmessConfig)
                {
                    return vmessConfig.Sni ?? vmessConfig.Host ?? vmessConfig.Address;
                }
                return x.ConfigData.Address;
            }).ToArray();
            */

            Console.WriteLine($"Отфильтровано серверов по IP: {servers.Length}");
            var deadEndpoints = new ConcurrentDictionary<string, byte>();
            int checkedCount = 0;
            await Parallel.ForEachAsync(servers, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, async (server, ct) =>
            {
                try
                {
                    string key = $"{server.ConfigData.Address}:{server.ConfigData.Port}";
                    if (deadEndpoints.ContainsKey(key)) return;
                    await networkManager.TestPing(server);
                    if (!server.IsAlive) deadEndpoints.TryAdd(key, 0);
                }
                finally
                {
                    int current = Interlocked.Increment(ref checkedCount);
                    if (current % 10000 == 0)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Проверено: {current} / {servers.Length}");
                    }
                }
            });
            servers = servers.Where(x => x.IsAlive).ToArray();
            Console.WriteLine($"Рабочих серверов: {servers.Length}");
            await Parallel.ForEachAsync(servers, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, async (server, ct) =>
            {
                networkManager.TestCountry(server);
            });

            await exportManager.ExportAsync(servers);

        }
    }
}
