namespace EventRegistrator
{
    [Serializable]
    public class UserRepository
    {
        public Dictionary<long, UserAdmin> users { get; set; }

        public UserRepository()
        {
            users = new();
        }

        public void AddUser(UserAdmin user)
        {
            if (!users.ContainsKey(user.Id))
            {
                users[user.Id] = user;
            }
        }

        public void AddUser(long user)
        {
            if (!users.ContainsKey(user))
            {
                users[user] = new UserAdmin(user);
            }
        }

        public UserAdmin GetUser(long id)
        {
            if (users.TryGetValue(id, out UserAdmin? value))
            {
                return value;
            }
            throw new NotImplementedException();
        }

        public UserAdmin GetUserByTargetChat(long targetChatId)
        {
            var user = users.FirstOrDefault(u => u.Value.TargetChatId == targetChatId).Value;
            if (user != null)
            {
                return user;
            }
            throw new NotImplementedException();
        }
    }
}
