using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notion2Database.Models;
using System.Net.Http.Json;

namespace Notion2Database.NotionAPI
{
    public class NotionAPIProvider
    {
        private readonly HttpClient httpClient;
        private IConfiguration config { get; }

        public NotionAPIProvider(HttpClient httpClient, IConfiguration config)
        {
            this.httpClient = httpClient;
            this.config = config;
        }

        public async Task<JObject> QueryDatabaseAsync(string databaseId, string? startCursor = null)
        {
            try
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api.notion.com/v1/databases/{databaseId}/query"))
                {
                    if (startCursor != null)
                    {
                        requestMessage.Content = JsonContent.Create(new { start_cursor = startCursor });
                    }

                    requestMessage.Headers.Add("Authorization", $"Bearer {config["NotionAPIConfiguration:InternalIntegrationSecret"]}");
                    requestMessage.Headers.Add("Notion-Version", config["NotionAPIConfiguration:NotionVersion"]);

                    var response = await httpClient.SendAsync(requestMessage);
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<JObject>(responseString) ?? [];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return [];
            }
        }

        public List<JToken> GetDatabaseResultsProperties(string databaseId)
        {
            string nextCursor = null;
            var hasMore = true;
            var properties = new List<JToken>();
            var rowNum = 1;

            while (hasMore)
            {
                var response = QueryDatabaseAsync(databaseId, nextCursor).Result;

                nextCursor = response["next_cursor"]!.ToString();
                hasMore = (bool)response["has_more"]!;

                foreach (var result in response["results"]!)
                {
                    if (result["properties"] != null)
                    {
                        properties.Add(result["properties"]!);
                    }

                    rowNum++;
                }
            }

            return properties;
        }

        public List<List<TypedDictionary>> GetProperties(string databaseId)
        {
            var resultsProperties = GetDatabaseResultsProperties(databaseId);
            var properties = new List<List<TypedDictionary>>();

            foreach (var property in resultsProperties)
            {
                var jTokenProperties = property.Children().OfType<JProperty>();
                var propertyRow = new List<TypedDictionary>();

                foreach (JProperty jTokenProperty in jTokenProperties)
                {
                    var key = jTokenProperty.Name;
                    var jTokenValue = jTokenProperty.Value;
                    var type = string.Empty;
                    var value = string.Empty;

                    foreach (JProperty jProp in jTokenValue.Children().OfType<JProperty>())
                    {
                        if (jProp.Name.ToString() == "type")
                        {
                            type = jProp.Value.ToString();
                        }
                    }

                    switch (type)
                    {
                        case "title":
                            value = jTokenValue["title"].HasValues ? jTokenValue["title"][0]["plain_text"].ToString() : string.Empty; break;
                        case "select":
                            value = jTokenValue["select"].HasValues ? jTokenValue["select"]["name"].ToString() : string.Empty; break;
                        case "multi_select":
                            value = jTokenValue["multi_select"].HasValues ? jTokenValue["multi_select"][0]["name"].ToString() : string.Empty; break;
                        case "rich_text":
                            value = jTokenValue["rich_text"].HasValues ? jTokenValue["rich_text"][0]["plain_text"].ToString() : string.Empty; break;
                        case "created_time":
                            value = jTokenValue["created_time"].ToString(); break;
                        case "date":
                            value = jTokenValue["date"].HasValues ? jTokenValue["date"]["start"].ToString() : string.Empty; break;
                        case "url":
                            value = jTokenValue["url"].ToString(); break;
                        case "number":
                            value = jTokenValue["number"].ToString(); break;
                    }

                    propertyRow.Add(new TypedDictionary(key, value, type));
                }

                properties.Add(propertyRow);
            }

            return properties;
        }
    }
}
