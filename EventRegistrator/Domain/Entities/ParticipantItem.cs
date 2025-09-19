using EventRegistrator.Domain.Interfaces;

namespace EventRegistrator.Domain.Entities
{
    public class ParticipantItem : IPagiable
    {
        public ParticipantItem(string name, string timeString, string participantName, TimeSpan timeSlot)
        {
            Name = $"{name} ({timeString})";
            Callback = participantName;
            ParticipantName = participantName;
            TimeString = timeString;
            TimeSlot = timeSlot;
        }

        public string Name { get; }
        public string Callback { get; }
        public string ParticipantName { get; }
        public string TimeString { get; }
        public TimeSpan TimeSlot { get; }
    }
}
