using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using NChecker.models;
using Sayaka.Common;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NChecker.classes
{
    internal class NetworkManager
    {
        private DatabaseReader reader;
        private MemoryStream dbMemoryStream;
        private int maxThreads = 10;
        private float fillingTimeout = 10, pingTimeout = 1, resolveIPTimeout = 0.75f;
        private readonly ConcurrentDictionary<string, IPAddress> _dnsCache = new();
        private readonly ConcurrentDictionary<string, string> _ptrCache = new();
        internal async Task Start()
        {
            await InitializeDatabaseAsync();
        }
        private async Task InitializeDatabaseAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(7)))
                // https://cdn.jsdelivr.net/npm/geolite2-city/GeoLite2-City.mmdb.gz
                // https://cdn.jsdelivr.net/npm/dbip-city-lite/dbip-city-lite.mmdb.gz
                using (Stream httpStream = await client.GetStreamAsync("https://cdn.jsdelivr.net/npm/geolite2-city/GeoLite2-City.mmdb.gz", cts.Token))
                using (GZipStream decompressionStream = new GZipStream(httpStream, CompressionMode.Decompress))
                {
                    dbMemoryStream = new MemoryStream();
                    await decompressionStream.CopyToAsync(dbMemoryStream);
                    dbMemoryStream.Position = 0;
                }
                reader = new DatabaseReader(dbMemoryStream);
                Console.WriteLine("База GeoLite2 успешно загружена в память и готова к работе!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка при обновлении базы GeoIP => {ex.Message}");
            }
        }
        internal async Task FillSubs(SubscriptionData[] a1)
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ProviderFakeUserAgent.Random);

            await Parallel.ForEachAsync(a1, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, async (subscriptionData, ct) =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(fillingTimeout));
                    string data = await httpClient.GetStringAsync(subscriptionData.Url, cts.Token);
                    try
                    {
                        data = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                    }
                    catch (FormatException) { }
                    if (string.IsNullOrEmpty(data))
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [FAIL] Пустой ответ | {subscriptionData.Url}");
                        return;
                    }
                    int addedCount = 0;
                    foreach (string rawRow in data.Split('\n'))
                    {
                        string row = rawRow.Trim();
                        if (!row.StartsWith("#") && row.Length > 16)
                        {
                            try
                            {
                                subscriptionData.AddServer(row);
                                addedCount++;
                            }
                            catch (Exception) { }
                        }
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ OK ] Загружено: {addedCount,5} серв. | {subscriptionData.Url}");
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [FAIL] Ошибка сети {subscriptionData.Url}");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [FAIL] Таймаут ({fillingTimeout}s) | {subscriptionData.Url}");
                }
            });
        }

        internal async Task TestResolveHost(ServerData a1)
        {
            if (IPAddress.TryParse(a1.ConfigData.Address, out IPAddress iPAddress))
            {
                // Если в ссылке был IP — вытаскиваем PTR-домен через отдельный кэш _ptrCache
                if (!_ptrCache.TryGetValue(a1.ConfigData.Address, out string ptrDomain))
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(resolveIPTimeout));
                        var host = await Dns.GetHostEntryAsync(iPAddress.ToString(), cts.Token);
                        ptrDomain = host?.HostName;
                        if (!string.IsNullOrEmpty(ptrDomain))
                        {
                            _ptrCache.TryAdd(a1.ConfigData.Address, ptrDomain);
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(ptrDomain))
                {
                    a1.ConfigData.Domain = ptrDomain;
                }
            }
            else
            {
                // Если в ссылке был домен — сохраняем его в Domain и переводим Address в IP
                string domain = a1.ConfigData.Address;
                a1.ConfigData.Domain = domain;

                if (!_dnsCache.TryGetValue(domain, out iPAddress))
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(resolveIPTimeout));
                        var addresses = await Dns.GetHostAddressesAsync(domain, cts.Token);
                        iPAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses.FirstOrDefault();
                        if (iPAddress != null)
                        {
                            _dnsCache.TryAdd(domain, iPAddress);
                        }
                    }
                    catch { }
                }

                a1.ConfigData.Address = iPAddress?.ToString();
            }
        }

        internal async Task TestResolveIP(ServerData a1)
        {
            if (!IPAddress.TryParse(a1.ConfigData.Address, out IPAddress ip) && !_dnsCache.TryGetValue(a1.ConfigData.Address, out ip))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(resolveIPTimeout));
                    var addresses = await Dns.GetHostAddressesAsync(a1.ConfigData.Address, cts.Token);
                    ip = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses.First();
                    if (ip != null) _dnsCache.TryAdd(a1.ConfigData.Address, ip);
                }
                catch { }
                a1.ConfigData.Address = ip?.ToString();
            }
        }
        internal async Task TestPing(ServerData a1)
        {
            if (!IPAddress.TryParse(a1.ConfigData.Address, out IPAddress iPAddress)) return;

            using var socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                LingerState = new LingerOption(true, 0)
            };

            try
            {
                var sw = Stopwatch.StartNew();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(pingTimeout));
                await socket.ConnectAsync(new IPEndPoint(iPAddress, a1.ConfigData.Port), cts.Token);
                sw.Stop();
                a1.IsAlive = socket.Connected;
            }
            catch { }
        }
        internal void TestCountry(ServerData a1)
        {
            try
            {
                if (!IPAddress.TryParse(a1.ConfigData.Address, out IPAddress iPAddress)) return;

                if (reader.TryCity(iPAddress, out CityResponse cityResponse))
                {
                    a1.Country = cityResponse.Country.Name;
                    a1.CountryCode = cityResponse.Country.IsoCode;
                    a1.City = cityResponse.City.Name;

                    if (string.IsNullOrWhiteSpace(a1.Country)) return;

                    string flag = Utils.CountryCodeToFlag(a1.CountryCode);
                    string location = string.IsNullOrWhiteSpace(a1.City) ? a1.Country : $"{a1.Country}, {a1.City}";
                    string prefix = string.IsNullOrEmpty(flag) ? location : $"{flag} {location}";

                    a1.ConfigData.Name = string.IsNullOrWhiteSpace(a1.ConfigData.Name) ? prefix : $"{prefix} | {a1.ConfigData.Name}";
                }
            }
            catch { }
        }
    }
}
