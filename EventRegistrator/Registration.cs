namespace EventRegistrator
{
    public class Registration
    {
        public Registration(long userId, string name, DateTime registrationTime)
        {
            UserId = userId;
            Name = name;
            RegistrationTime = registrationTime;
        }

        public long UserId { get; }
        public string Name { get; }
        public DateTime RegistrationTime { get; }
    }
}
