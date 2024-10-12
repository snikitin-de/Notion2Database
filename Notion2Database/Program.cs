using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notion2Database.NotionAPI;
using System.Text;

namespace Notion2Database
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            Console.Write("Введите идентификатор базы данных: ");
            var databaseId = Console.ReadLine();

            var client = new HttpClient();
            var notionAPIProvider = new NotionAPIProvider(client, config);
            var properties = notionAPIProvider.GetProperties(databaseId);

            var databaseColumns = string.Join(";", properties[0].Select(x => x.Key));
            var csvFile = databaseColumns + "\n";

            foreach (var property in properties)
            {
                csvFile += string.Join(";", property.Select(x => x.Value)) + "\n";
            }

            File.WriteAllText(databaseId + ".csv", csvFile, Encoding.UTF8);

            var json = JsonConvert.SerializeObject(properties);
            var fileProviderForJson = new FileProvider(databaseId);
            fileProviderForJson.Add(JsonConvert.DeserializeObject(json));
        }
    }
}
