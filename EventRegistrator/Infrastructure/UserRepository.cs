using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Newtonsoft.Json;

namespace EventRegistrator.Infrastructure
{
    [Serializable]
    public class UserRepository : IUserRepository
    {
        private readonly object _lock = new();
        [JsonProperty]
        private readonly Dictionary<long, UserAdmin> _users;

        public UserRepository()
        {
            _users = new();
        }

        public void AddUser(UserAdmin user)
        {
            lock (_lock)
            {
                if (!_users.ContainsKey(user.Id))
                {
                    _users[user.Id] = user;
                }
            }
        }

        public void AddUser(long user)
        {
            lock (_lock)
            {
                if (!_users.ContainsKey(user))
                {
                    _users[user] = new UserAdmin(user);
                }
            }
        }

        public UserAdmin? GetUser(long id)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(id, out UserAdmin? value))
                {
                    return value;
                }
                return null;
            }
        }

        public UserAdmin? GetUserByTargetChat(long targetChatId)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Value.ContainsTargetChat(targetChatId)).Value;
                return user;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _users.Clear();
            }
        }

        public List<UserAdmin> GetAllUsers()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }
    }
}
