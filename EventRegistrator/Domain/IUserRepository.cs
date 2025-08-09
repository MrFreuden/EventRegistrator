using EventRegistrator.Domain.Models;

namespace EventRegistrator.Domain
{
    public interface IUserRepository
    {
        void AddUser(long user);
        void AddUser(UserAdmin user);
        void Clear();
        UserAdmin GetUser(long id);
        UserAdmin GetUserByTargetChat(long targetChatId);
    }
}