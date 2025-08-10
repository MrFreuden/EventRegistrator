using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Newtonsoft.Json;

namespace EventRegistrator.Infrastructure
{
    [Serializable]
    public class UserRepository : IUserRepository
    {
        [JsonProperty]
        private readonly Dictionary<long, UserAdmin> _users;

        public UserRepository()
        {
            _users = new();
        }

        public void AddUser(UserAdmin user)
        {
            if (!_users.ContainsKey(user.Id))
            {
                _users[user.Id] = user;
            }
        }

        public void AddUser(long user)
        {
            if (!_users.ContainsKey(user))
            {
                _users[user] = new UserAdmin(user);
            }
        }

        public UserAdmin GetUser(long id)
        {
            if (_users.TryGetValue(id, out UserAdmin? value))
            {
                return value;
            }
            throw new NotImplementedException();
        }

        public UserAdmin GetUserByTargetChat(long targetChatId)
        {
            var user = _users.FirstOrDefault(u => u.Value.TargetChatId == targetChatId).Value;
            if (user != null)
            {
                return user;
            }
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _users.Clear();
        }

        public List<UserAdmin> GetAllUsers()
        {
            return _users.Values.ToList();
        }
    }
}
