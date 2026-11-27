using NChecker.models;
using System.Text;

namespace NChecker.classes
{
    internal class ExportManager
    {
        private readonly string _dumpPath = Path.Combine(Directory.GetCurrentDirectory(), "dump");
        private int _maxLinesPerFile = 750;
        public async Task ExportAsync(ServerData[] servers)
        {
            if (servers == null || servers.Length == 0)
            {
                Console.WriteLine("[Export] Нет серверов для экспорта.");
                return;
            }
            Console.WriteLine($"[Export] Старт экспорта {servers.Length} серверов...");
            if (Directory.Exists(_dumpPath))
            {
                Directory.Delete(_dumpPath, true);
            }
            Directory.CreateDirectory(_dumpPath);

            // 1. Общий экспорт в корень dump/ (единый эмодзи чёрных списков 🛡️)
            await WriteFullAndPartsAsync(_dumpPath, "🛡️ VPN — BLACK LIST", "🛡️ VPN BLACK", servers);

            // 2. Распределение по протоколам: dump/Protocols/{PROTOCOL}/
            var byProtocol = servers
                .GroupBy(s => SanitizeFolderName(s.ConfigData?.Protocol, "Unknown"));

            foreach (var group in byProtocol)
            {
                string protocolName = group.Key.ToUpper();
                string protocolDir = Path.Combine(_dumpPath, "Protocols", protocolName);
                Directory.CreateDirectory(protocolDir);

                string header = $"⚡ {protocolName} — BLACK";
                await WriteFullAndPartsAsync(protocolDir, header, header, group.ToArray());
            }

            // 3. Распределение по странам: dump/Country/{COUNTRYCODE}/
            var byCountry = servers
                .GroupBy(s => SanitizeFolderName(s.CountryCode, "UNK"));

            foreach (var group in byCountry)
            {
                string countryCode = group.Key.ToUpper();
                string countryDir = Path.Combine(_dumpPath, "Countries", countryCode);
                Directory.CreateDirectory(countryDir);

                string header = $"🌍 GEO [{countryCode}] — BLACK";
                await WriteFullAndPartsAsync(countryDir, header, header, group.ToArray());
            }

            Console.WriteLine("[Export] Экспорт завершен!");
        }

        private async Task WriteFullAndPartsAsync(string targetPath, string fullHeaderName, string partBaseName, ServerData[] servers)
        {
            var urls = servers
                .Select(s => s.ConfigData?.GetNewUrl() ?? s.Url)
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();

            if (urls.Count == 0) return;

            // full.txt
            var fullHeader = new SubscriptionHeaderData(fullHeaderName, urls.Count);
            await WriteFileAsync(Path.Combine(targetPath, "full.txt"), fullHeader.GetHeaderString(), urls);

            // part_X.txt
            var chunks = urls.Chunk(_maxLinesPerFile).ToList();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var partHeader = new SubscriptionHeaderData($"{partBaseName} #{i + 1}", chunk.Length);
                await WriteFileAsync(Path.Combine(targetPath, $"part_{i + 1}.txt"), partHeader.GetHeaderString(), chunk);
            }
        }

        private async Task WriteFileAsync(string filePath, string header, IEnumerable<string> lines)
        {
            var allLines = new List<string> { header };
            allLines.AddRange(lines);

            await File.WriteAllLinesAsync(filePath, allLines, Encoding.UTF8);
        }

        private static string SanitizeFolderName(string raw, string fallback)
        {
            if (string.IsNullOrWhiteSpace(raw)) return fallback;

            string clean = raw.Trim().ToLower();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                clean = clean.Replace(c, '_');
            }

            return string.IsNullOrEmpty(clean) ? fallback : clean;
        }
    }
}
