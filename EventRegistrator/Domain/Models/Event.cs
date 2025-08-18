using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    [Serializable]
    public class Event
    {
        [JsonProperty]
        private readonly List<TimeSlot> _slots;

        public Event(string title, int postId, long targetChatId, string hashtagName)
        {
            Id = new Guid();
            Title = title;
            PostId = postId;
            TargetChatId = targetChatId;
            HashtagName = hashtagName;
            _slots = new List<TimeSlot>();
        }

        public Guid Id { get; }
        public string Title { get; }
        public long TargetChatId { get; }
        public string HashtagName { get; }
        public int PostId { get; }
        public int CommentMessageId { get; set; }
        public int PrivateMessageId { get; set; }
        public string TemplateText { get; set; }
        public IReadOnlyCollection<TimeSlot> Slots => _slots.AsReadOnly();

        public void AddSlot(TimeSlot slot)
        {
            ArgumentNullException.ThrowIfNull(slot);

            if (_slots.Any(s => s.Time == slot.Time))
                throw new ArgumentException($"Слот на {slot.Time} уже существует.");

            _slots.Add(slot);
        }

        public void AddSlots(IEnumerable<TimeSlot> slots)
        {
            ArgumentNullException.ThrowIfNull(slots);

            foreach (var slot in slots)
            {
                if (_slots.Any(s => s.Time == slot.Time))
                    throw new ArgumentException($"Слот на {slot.Time} уже существует.");

                _slots.Add(slot);
            }
        }

        public void RemoveSlot(TimeSlot slot)
        {
            ArgumentNullException.ThrowIfNull(slot);
            _slots.Remove(slot);
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

        public bool AddRegistration(Registration registration)
        {
            var slot = TimeSlotParser.FindMatchingTimeSlot(_slots, registration);
            return slot.AddRegistration(registration);
        }
    }
}
