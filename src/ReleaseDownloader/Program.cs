namespace ReleaseDownloader
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;

    internal class Program
    {
        private static void Main(string[] args) => new Program().Run(args).Wait();

        private HttpClient client;

        private async Task Run(string[] args)
        {
            var (repo, token, name, _) = args;
            if (string.IsNullOrWhiteSpace(repo))
            {
                PrintUsage();
                return;
            }

            client = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    {"Authorization", $"Token {token}"},
                    {"User-Agent", " hbb/1.0"}
                }
            };
            var asset = await FindAssetAsync(repo, name);
            if (asset != null)
            {
                Console.WriteLine("File Name: {0}", asset.Name);
                Console.WriteLine("File Id: {0}", asset.Id);
                await DownloadFile(repo, asset);
            }
        }


        private void PrintUsage()
        {
            Console.WriteLine("Expected Token and Repository full name");
            Console.WriteLine("release-downloader username/token 1981c25df353b0a75e65948d4c4cf8fa90f568");
        }

        private async Task DownloadFile(string repo, Asset asset)
        {
            using var file = File.OpenWrite(asset.Name);
            var requestMessage = new HttpRequestMessage
            {
                Headers = {{"Accept", "application/octet-stream"}},
                RequestUri = new Uri($"https://api.github.com/repos/{repo}/releases/assets/{asset.Id}"),
                Method = HttpMethod.Get
            };
            var responseMessage = await client.SendAsync(requestMessage);
            //TODO: Progress
            var response = await responseMessage.Content.ReadAsStreamAsync();
            await response.CopyToAsync(file);
        }

        private async Task<Asset> FindAssetAsync(string repo, string name)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.github.com/repos/{repo}/releases")
            };
            var responseMessage = await client.SendAsync(message);
            var response = await responseMessage.Content.ReadAsStreamAsync();
            var serializer = new DataContractJsonSerializer(typeof(Release[]));
            var releases = (Release[]) serializer.ReadObject(response);
            var release = string.IsNullOrWhiteSpace(name)
                ? releases.First()
                : releases.FirstOrDefault(item =>
                    string.Equals(name, item.Name, StringComparison.InvariantCultureIgnoreCase));
            var asset = release?.Assets.First();
            return asset;
        }
    }
}