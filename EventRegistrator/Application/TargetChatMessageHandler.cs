using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public class TargetChatMessageHandler : IHandler
    {
        private const char _hashtag = '#';
        private readonly IUserRepository _userRepository;
        private readonly EventService _eventService;
        private readonly RegistrationService _registrationService;
        private readonly ResponseManager _responseManager;

        public TargetChatMessageHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _eventService = new EventService(userRepository);
            _registrationService = new RegistrationService();
            _responseManager = new ResponseManager();
        }

        public async Task<List<Response>> HandleEdit(MessageDTO message)
        {
            if (IsReplyToPostMessage(message))
            {
                var user = _userRepository.GetUserByTargetChat(message.ChatId);
                var lastEvent = user.GetLastEvent();

                var resultUndo = _registrationService.CancelRegistration(lastEvent, message.Id);
                if (resultUndo.Success)
                {
                    var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.GetSlots());
                    lastEvent.TemplateText = text;

                    var messages = GetSuccessResponsesForEdit(user, resultUndo);
                    messages.AddRange(await Handle(message));
                    return messages;
                }
            }
            return [];
        }

        public Task<List<Response>> HandleAsync(MessageDTO message)
        {
            throw new NotImplementedException();
        }

        public bool CanHandle(MessageDTO message)
        {
            return IsMessageFromTargetChat(message);
        }
        public async Task<List<Response>> Handle(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (IsFromChannel(message) && IsHasHashtag(message))
            {
                var createEventComand = new CreateEventCommand(_eventService);
                return await createEventComand.Execute(message, user);
            }
            else if (IsReplyToPostMessage(message))
            {
                var registerCommand = new RegisterCommand(_registrationService, _responseManager);
                return await registerCommand.Execute(message, user);
            }
            return [];
        }

        private List<Response> GetSuccessResponses(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            messages.Add(_responseManager.CreateLikeMessage(result.Event.TargetChatId, result.MessageId));
            return messages;
        }

        private List<Response> GetSuccessResponsesForEdit(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            messages.Add(_responseManager.CreateUnlikeMessage(result.Event.TargetChatId, result.MessageId));
            return messages;
        }

        private bool IsHasHashtag(MessageDTO message)
        {
            var lastPart = message.Text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            ).Last();
            if (_userRepository.GetUserByTargetChat(message.ChatId).ContainsHashtag(lastPart.Trim(_hashtag)))
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

        private bool IsMessageFromTargetChat(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            return user != null;
        }
    }
}
