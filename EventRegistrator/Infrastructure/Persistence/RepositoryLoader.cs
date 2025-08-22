using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventRegistrator.Infrastructure.Persistence
{
    public class RepositoryLoader
    {
        private readonly string _path;
        private readonly object _lock = new();
        private readonly JsonSerializerSettings _settings;

        public RepositoryLoader(string path)
        {
            _path = path;

            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new PrivateSetterContractResolver()
            };
        }

        public UserRepository LoadData()
        {
            lock (_lock)
            {
                if (!File.Exists(_path))
                {
                    return new UserRepository(this);
                }
                try
                {
                    var jsonString = File.ReadAllText(_path);
                    if (jsonString != null) Console.WriteLine("Загрузка успешна");
                    var repo = JsonConvert.DeserializeObject<UserRepository>(jsonString, _settings) ?? new UserRepository(this);
                    typeof(UserRepository).GetField("_loader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                                                                                        ?.SetValue(repo, this);
                    return repo;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    return new UserRepository(this);
                }
            }
        }

        public async Task SaveDataAsync(UserRepository service)
        {
            Console.WriteLine("Начало сохранения");
            await Task.Run(() => SaveData(service));
            Console.WriteLine("Конец сохранения");
        }

        private void SaveData(UserRepository service)
        {
            lock (_lock)
            {
                try
                {
                    var jsonString = JsonConvert.SerializeObject(service);
                    File.WriteAllText(_path, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                }
            }
        }
    }

    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as System.Reflection.PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
    public class TimeSpanOrDateTimeConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (TimeSpan.TryParse(value, out var ts))
                return ts;

            if (DateTime.TryParse(value, out var dt))
                return dt.TimeOfDay;

            throw new JsonSerializationException($"Не удалось преобразовать '{value}' в TimeSpan.");
        }

        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
