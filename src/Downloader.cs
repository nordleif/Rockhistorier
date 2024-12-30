using System.Globalization;
using System.Text;
using System.Xml;

namespace Rockhistorier
{
    internal class Downloader
    {
        public static async Task DownloadAsync(string url, string path)
        {
            var client = new HttpClient();
            var xmlText = await client.GetStringAsync(url);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");

            var items = xmlDoc.GetElementsByTagName("item");
            foreach (XmlNode item in items)
            {
                var title = item["title"]?.InnerText ?? string.Empty;
                var description = HtmlUtilities.ConvertToPlainText(item["description"]?.InnerText ?? string.Empty);
                var link = item["link"]?.InnerText ?? string.Empty;
                var pubDate = DateTime.ParseExact(item["pubDate"]?.InnerText ?? string.Empty, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                var enclosureUrl = item["enclosure"]?.Attributes["url"]?.Value ?? string.Empty;

                var imageUrl = string.Empty;
                var imageNode = xmlDoc.SelectSingleNode("//itunes:image", nsmgr);
                if (imageNode != null)
                    imageUrl = imageNode?.Attributes?["href"]?.Value ?? string.Empty;

                var fileNameWithoutExtension = Path.Combine(path, $"{pubDate.ToString("yyyyMMdd")} {GetLegalFileName(title)}");

                if (File.Exists($"{fileNameWithoutExtension}.mp3"))
                    continue;

                Console.WriteLine($"Downloading {title}...");

                File.WriteAllText($"{fileNameWithoutExtension}.txt", description);

                await DownloadFileAsync(enclosureUrl, $"{fileNameWithoutExtension}{Path.GetExtension(enclosureUrl)}");
            }
        }

        private static async Task DownloadFileAsync(string url, string filePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(filePath, content);
            }
        }

        private static string GetLegalFileName(string text)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (Array.IndexOf(invalidChars, c) == -1)
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
