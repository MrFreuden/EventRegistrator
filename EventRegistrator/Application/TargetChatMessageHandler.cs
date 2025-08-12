using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public class TargetChatMessageHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly EventService _eventService;
        private readonly RegistrationService _registrationService;

        public TargetChatMessageHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _eventService = new EventService(userRepository);
            _registrationService = new RegistrationService(userRepository);
        }

        public List<Response> HandleEdit(MessageDTO message)
        {
            if (IsReplyToPostMessage(message))
            {
                return _registrationService.HandleRegistrationEdit(message);
            }
            return [];
        }

        public List<Response> Handle(MessageDTO message)
        {
            if (IsFromChannel(message) && IsHasHashtag(message))
            {
                var @event = EventService.Create(message);
                var user = _userRepository.GetUserByTargetChat(message.ChatId);
                @event.TemplateText = user.GetTargetChat().GetHashtagByName(@event.HashtagName).TemplateText;
                var antwort = _eventService.AddNewEvent(@event, message.Created);
                antwort.MessageToReplyId = message.Id;
                return [antwort];
            }
            else if (IsReplyToPostMessage(message))
            {
                return _registrationService.Register(message);
            }
            return [];
        }

        private bool IsHasHashtag(MessageDTO message)
        {
            var lastPart = message.Text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            ).Last();
            if (_userRepository.GetUserByTargetChat(message.ChatId).ContainsHashtag(lastPart.Trim('#')))
            {
                return true;
            }
            return false;
        }

        private bool IsFromChannel(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (message.ForwardFromChat != null)
            {
                return user.ContainsChannel(message.ForwardFromChat.Id);
            }

            return false;
        }

        private bool IsReplyToPostMessage(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (message.ReplyToMessage != null && message.ReplyToMessage.ForwardFromChat != null)
            {
                return user.ContainsChannel(message.ReplyToMessage.ForwardFromChat.Id);
            }

            return false;
        }
    }
}
