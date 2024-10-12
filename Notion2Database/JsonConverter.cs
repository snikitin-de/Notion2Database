using Newtonsoft.Json;

namespace Notion2Database
{
    public class JsonConverter
    {
        public string Format
        {
            get => "json";
        }

        public string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item, Formatting.Indented);
        }

        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
