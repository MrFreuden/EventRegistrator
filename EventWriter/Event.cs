using Telegram.Bot.Types;

namespace EventWriter
{
    public class Event
    {
        public List<TimeSlot> _slots;

        public Event(Guid id, string title, long channelId, int messageId, string hashtagName, string templateText)
        {
            Id = id;
            Title = title;
            ChannelId = channelId;
            MessageId = messageId;
            HashtagName = hashtagName;
            _slots = new List<TimeSlot>();
            TemplateText = templateText;
        }

        public Guid Id { get; }
        public string Title { get; private set; }
        public long ChannelId { get; }
        public int MessageId { get; }
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
    }
}
