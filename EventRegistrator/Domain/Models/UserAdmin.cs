using EventRegistrator.Application.Interfaces;
using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    [Serializable]
    public class UserAdmin
    {
        [JsonProperty]
        private readonly List<Event> _events;
        [JsonProperty]
        private readonly Dictionary<long, TargetChat> _targetChats;
        public long Id { get; set; }
        public long PrivateChatId { get; set; }
        public bool IsAsked { get; set; }
        public IState State { get; set; }

        public UserAdmin(long id)
        {
            Id = id;
            _events = new();
            _targetChats = new();
        }

        public void AddEvent(Event e)
        {
            _events.Add(e);
        }

        public void AddTargetChat(TargetChat chat)
        {
            ArgumentNullException.ThrowIfNull(chat);
            _targetChats[chat.Id] = chat;
        }

        public Event GetLastEvent()
        {
            return _events.Last();
        }

        public TargetChat GetTargetChat()
        {
            return _targetChats.First().Value;
        }

        public bool ContainsTargetChat(long id)
        {
            return _targetChats.ContainsKey(id);
        }
        
        public bool ContainsHashtag(string hashtag)
        {
            foreach (var chat in _targetChats.Values)
            {
                if (chat.ContainsHashtag(hashtag))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsChannel(long id)
        {
            foreach (var chat in _targetChats.Values)
            {
                if (chat.ChannelId == id)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class TargetChat
    {
        [JsonProperty]
        private readonly Dictionary<string, Hashtag> _hashtags;
        public long Id { get; }
        public long ChannelId { get; }
        public string ChannelName { get; }

        public TargetChat(long id, long channelId, string channelName)
        {
            Id = id;
            ChannelId = channelId;
            ChannelName = channelName;
            _hashtags = new();
        }

        public void AddHashtag(Hashtag hashtag)
        {
            if (_hashtags.TryAdd(hashtag.HashtagName, hashtag))
            {
                return;
            }
            throw new NotImplementedException();
        }

        public Hashtag GetHashtagByName(string name)
        { 
            if (_hashtags.TryGetValue(name, out var hashtag))
            {
                return hashtag;
            }
            throw new NotImplementedException();
        }

        public bool ContainsHashtag(string hashtag)
        {
            return _hashtags.ContainsKey(hashtag);
        }
    }

    public class Hashtag
    {
        public string HashtagName { get; }
        public string TemplateText { get; private set; }
        private const string defaultTemplate = "10:00 - 10 вільних місць\r\n10:15 - 10 вільних місць\r\n10:30 - 10 вільних місць";

        public Hashtag(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя хэштега не может быть пустым", nameof(name));

            HashtagName = name;
            TemplateText = defaultTemplate;
        }

        [JsonConstructor]
        private Hashtag()
        {
            HashtagName = "default";
            TemplateText = defaultTemplate;
        }

        public void EditTemplateText(string text)
        {
            if (IsTemplateValid(text))
            {
                TemplateText = text;
            }
        }

        private bool IsTemplateValid(string text)
        {
            return true;
        }
    }
}
