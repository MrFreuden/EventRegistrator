using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public class TargetChatMessageHandler
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

        public List<Response> HandleEdit(MessageDTO message)
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
                    messages.AddRange(Handle(message));
                    return messages;
                }
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
                var result = _eventService.AddNewEvent(@event, message.Created);
                if (result.Success)
                {
                    return [new Response
                    {
                        ChatId = result.Event.TargetChatId,
                        Text = result.Event.TemplateText,
                        ButtonData = (Constants.Cancel, Constants.Cancel),
                        SaveMessageIdCallback = id => { result.Event.CommentMessageId = id; },
                        MessageToReplyId = message.Id
                    }];
                }
            }
            else if (IsReplyToPostMessage(message))
            {
                var user = _userRepository.GetUserByTargetChat(message.ChatId);
                var lastEvent = user.GetLastEvent();

                var map = TimeSlotParser.GetMaper(lastEvent.TemplateText);
                var regs = TimeSlotParser.ParseRegistrationMessage(message, map);
                var result = _registrationService.ProcessRegistration(lastEvent, regs);
                if (result.Success)
                {
                    result.MessageId = message.Id;
                    return GetSuccessResponses(user, result);
                }
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
    }
}
