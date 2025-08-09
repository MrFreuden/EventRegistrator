using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    [Serializable]
    public class Registration
    {
        [JsonConstructor]
        public Registration(long userId, string name, DateTime registrationTime, int messageId)
        {
            UserId = userId;
            Name = name;
            RegistrationTime = registrationTime;
            MessageId = messageId;
        }

        public long UserId { get; }
        public string Name { get; }
        public DateTime RegistrationTime { get; }
        public int MessageId { get; }
    }
}
