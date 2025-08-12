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

        public bool AddRegistration(Registration registration)
        {
            var slot = TimeSlotParser.FindMatchingTimeSlot(_slots, registration);
            return slot.AddRegistration(registration);
        }
    }
}
