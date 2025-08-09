using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventRegistrator
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
                    return new UserRepository();
                }
                try
                {
                    var jsonString = File.ReadAllText(_path);
                    if (jsonString != null) Console.WriteLine("Загрузка успешна");
                    return JsonConvert.DeserializeObject<UserRepository>(jsonString, _settings) ?? new UserRepository();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    return new UserRepository();
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
}
