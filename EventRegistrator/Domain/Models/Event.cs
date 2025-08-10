using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    [Serializable]
    public class Event
    {
        [JsonProperty]
        private readonly List<TimeSlot> _slots;

        public Event(Guid id, string title, long channelId, int postId, string hashtagName, string templateText)
        {
            Id = id;
            Title = title;
            ChannelId = channelId;
            PostId = postId;
            HashtagName = hashtagName;
            _slots = new List<TimeSlot>();
            TemplateText = templateText;
        }

        public Guid Id { get; }
        public string Title { get; private set; }
        public long ChannelId { get; set; }
        public int CommentMessageId { get; set; }
        public int PostId { get; set; }
        public int PrivateMessageId { get; set; }
        public string HashtagName { get; }
        public string TemplateText { get; set; }

        public void AddSlot(TimeSlot slot)
        {
            ArgumentNullException.ThrowIfNull(slot);
            if (IsSlotCanBeAdded(slot))
            {
                _slots.Add(slot);
            }
            else
            {
                throw new ArgumentException(slot.Time.ToString());
            }
        }

        public void AddSlots(List<TimeSlot> slots)
        {
            ArgumentNullException.ThrowIfNull(slots);
            foreach (var slot in slots)
            {
                if (IsSlotCanBeAdded(slot))
                {
                    _slots.Add(slot);
                }
                else
                {
                    throw new ArgumentException(slot.Time.ToString());
                }
            }
        }

        public bool IsSlotCanBeAdded(TimeSlot slot)
        {
            return _slots.FirstOrDefault(s => s.Time == slot.Time) == null;
        }

        public void RemoveSlot(TimeSlot slot)
        {
            _slots.Remove(slot);
        }

        public List<TimeSlot> GetSlots()
        {
            return _slots;
        }

        public void RemoveRegistrations(int messageId)
        {
            foreach (var slot in _slots)
            {
                var reg = slot.GetRegistration(messageId);
                if (reg == default) continue;
                slot.RemoveRegistration(reg);
            }
        }

        public List<int> RemoveRegistrations(long userId)
        {
            var messageIds = new List<int>();
            foreach (var slot in _slots)
            {
                var reg = slot.GetRegistration(userId);
                if (reg == default) continue;
                slot.RemoveRegistration(reg);
                messageIds.Add(reg.MessageId);
            }
            return messageIds;
        }
    }
}
