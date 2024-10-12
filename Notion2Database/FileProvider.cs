namespace Notion2Database
{
    public class FileProvider
    {
        private string filepath;
        private JsonConverter converter = new JsonConverter();
        public string FileExtention;

        public FileProvider() { }

        public FileProvider(string filename)
        {
            FileExtention = converter.Format;
            filepath = $"{filename}.{FileExtention}";
        }

        public void Append<T>(T data)
        {
            var allData = new List<T>();

            if (File.Exists(filepath))
            {
                var oldData = Read<T>();
                allData.AddRange(oldData);
            }

            allData.Add(data);

            Add(allData);
        }

        public void Add<T>(T item)
        {
            File.WriteAllText(filepath, converter.Serialize(item));
        }

        public List<T> Read<T>()
        {
            if (File.Exists(filepath))
            {
                var data = File.ReadAllText(filepath);

                return converter.Deserialize<List<T>>(data);
            }

            return new List<T>();
        }

        public bool Exists()
        {
            return File.Exists(filepath);
        }
    }
}
