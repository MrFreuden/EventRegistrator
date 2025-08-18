using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects;
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

        [JsonIgnore]
        public Stack<IState> StateHistory = new();
        [JsonIgnore]
        public IState State { get; set; }
        public MenuContext CurrentContext { get; set; }
        public int? LastMessageId { get; set; }

        public UserAdmin(long id)
        {
            Id = id;
            _events = new();
            _targetChats = new();
        }

        public void AddEvent(Event @event)
        {
            if (_events.Count > 6)
            {
                _events.RemoveRange(0, 5);
            }
            _events.Add(@event);
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

        public TargetChat GetTargetChat(long targetChatId)
        {
            return _targetChats[targetChatId];
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

        public IReadOnlyCollection<TargetChat> GetAllTargetChats()
        {
            return _targetChats.Values;
        }

        public IReadOnlyCollection<Hashtag> GetAllHashtags(long targetChatId)
        {
            return _targetChats[targetChatId].GetAllHashtags();
        }
    }
}
