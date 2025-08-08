using Newtonsoft.Json;

namespace EventRegistrator
{
    public class RepositoryLoader
    {
        private readonly string _path;
        private readonly object _lock = new();

        public RepositoryLoader(string path)
        {
            _path = path;
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
                    return JsonConvert.DeserializeObject<UserRepository>(jsonString) ?? new UserRepository();
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
}
