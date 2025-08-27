using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Services
{
    public class EventService
    {
        private readonly IUserRepository _userRepository;
        private const string _defaultTitle = "SWS";
        private const char _hashtag = '#';
        public EventService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public RegistrationResult AddNewEvent(Event @event, DateTime eventTime)
        {
            var user = _userRepository.GetUserByTargetChat(@event.TargetChatId);
            user.AddEvent(@event);

            return new RegistrationResult { Event = @event, Success = true };
        }

        public static Event Create(MessageDTO message)
        {
            var hashtagName = ParseHashtagName(message.Text);
            return new Event(message.Created.ToString(), message.Id, message.ChatId, hashtagName);
        }

        public void EditTimeSlots(Event @event, string templateText)
        {

        }

        private static string ParseHashtagName(string text)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));
            var lastPart = text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            ).Last();
            if (lastPart == null || !lastPart.StartsWith(_hashtag))
            {
                throw new ArgumentException("Нету диеза");
            }
            return lastPart.Trim(_hashtag);
        }
    }
}
