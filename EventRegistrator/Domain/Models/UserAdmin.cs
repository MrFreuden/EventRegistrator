using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    [Serializable]
    public class UserAdmin
    {
        [JsonProperty]
        private readonly List<Event> _events;
        public long Id { get; set; }

        public UserAdmin(long id)
        {
            Id = id;
            TempleText = defaultTemplate;
            _events = new();
        }
        private const string defaultTemplate = "10:00 - 10 вільних місць\r\n10:15 - 10 вільних місць\r\n10:30 - 10 вільних місць";
        public long PrivateChatId {  get; set; }
        public long TargetChatId { get; set; }
        public long ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string HashtagName { get; set; }
        public string TempleText { get; set; }
        
        public bool IsAsked { get; set; }

        public void AddEvent(Event e)
        {
            _events.Add(e);
        }

        public Event GetLastEvent()
        {
            return _events.Last();
        }
    }
}
