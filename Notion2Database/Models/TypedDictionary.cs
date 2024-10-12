namespace Notion2Database.Models
{
    public class TypedDictionary
    {
        public string Key;
        public string Value;
        public string Type;

        public TypedDictionary(string key, string value, string type)
        {
            Key = key;
            Value = value;
            Type = type;
        }
    }
}